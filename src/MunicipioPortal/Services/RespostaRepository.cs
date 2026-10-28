using System.Data;
using Microsoft.Data.SqlClient;
using MunicipioPortal.Models;

namespace MunicipioPortal.Services;

/// <summary>Persistência na tabela V2, preservando o histórico na tabela anterior.</summary>
public sealed class RespostaRepository(IConfiguration config)
{
    public async Task<long> SalvarAsync(
        FormularioInput dados,
        IReadOnlyDictionary<string, int> quantidades,
        IReadOnlyDictionary<string, bool> infraestrutura,
        CancellationToken cancellationToken)
    {
        var campos = QuestionarioDefinition.Campos;
        var itens = QuestionarioDefinition.Infraestrutura;
        var colunas = campos.Select(c => c.Coluna)
            .Concat(["Q6_ESTRUTURA", "Q6_OUTRA_DESCRICAO",
                "Q7_PROGRAMA", "Q7_PROGRAMA_DETALHES"])
            .Concat(itens.Select(i => i.Coluna)).ToArray();
        var parametros = campos.Select(c => "@" + c.Chave)
            .Concat(["@Estrutura", "@EstruturaOutra", "@Programa", "@Detalhes"])
            .Concat(itens.Select(i => "@" + i.Chave)).ToArray();

        string sql = $"""
            INSERT INTO dbo.RespostasMunicipaisV2
                (CodigoIBGE, {string.Join(", ", colunas.Select(c => "[" + c + "]"))})
            OUTPUT INSERTED.Id
            VALUES (@CodigoIbge, {string.Join(", ", parametros)});
            """;

        await using var conexao = SqlServerConnectionFactory.Criar(config);
        await conexao.OpenAsync(cancellationToken);
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.Add("@CodigoIbge", SqlDbType.Int).Value = dados.CodigoIbge!.Value;
        foreach (var campo in campos)
            comando.Parameters.Add("@" + campo.Chave, SqlDbType.Int).Value =
                quantidades[campo.Chave];

        comando.Parameters.Add("@Estrutura", SqlDbType.VarChar, 40).Value = dados.Estrutura!;
        comando.Parameters.Add("@EstruturaOutra", SqlDbType.NVarChar, 300).Value =
            dados.Estrutura == "OUTRA" ? dados.EstruturaOutra! : DBNull.Value;
        comando.Parameters.Add("@Programa", SqlDbType.VarChar, 16).Value = dados.Programa!;
        comando.Parameters.Add("@Detalhes", SqlDbType.NVarChar, 2000).Value =
            dados.Programa == "PROPRIO" ? dados.Detalhes! : DBNull.Value;

        foreach (var item in itens)
            comando.Parameters.Add("@" + item.Chave, SqlDbType.Bit).Value =
                infraestrutura[item.Chave];

        object? id = await comando.ExecuteScalarAsync(cancellationToken);
        if (id is null or DBNull)
            throw new InvalidOperationException("O SQL Server não retornou o protocolo.");
        return Convert.ToInt64(id);
    }
}
