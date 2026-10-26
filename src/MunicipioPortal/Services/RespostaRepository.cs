using System.Data;
using Microsoft.Data.SqlClient;
using MunicipioPortal.Models;

namespace MunicipioPortal.Services;

/// <summary>
/// Persiste respostas no Microsoft SQL Server. Não cria nem modifica o esquema.
/// A conexão precisa ser configurada no IIS por ConnectionStrings__Municipios.
/// </summary>
public sealed class RespostaRepository(IConfiguration config)
{
    public async Task<long> SalvarAsync(
        FormularioInput dados,
        IReadOnlyDictionary<string, int> quantidades,
        CancellationToken cancellationToken)
    {
        var campos = QuestionarioDefinition.Campos;
        string colunas = string.Join(", ", campos.Select(c => "[" + c.Coluna + "]"));
        string parametros = string.Join(", ", campos.Select(c => "@" + c.Chave));
        string sql = $"""
            INSERT INTO dbo.RespostasMunicipais
                (CodigoIBGE, {colunas}, ProgramaHabitacionalTipo, ProgramaProprioDetalhes)
            OUTPUT INSERTED.Id
            VALUES (@CodigoIbge, {parametros}, @Programa, @Detalhes);
            """;

        await using var conexao = SqlServerConnectionFactory.Criar(config);
        await conexao.OpenAsync(cancellationToken);
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.Add("@CodigoIbge", SqlDbType.Int).Value = dados.CodigoIbge!.Value;
        foreach (var campo in campos)
            comando.Parameters.Add("@" + campo.Chave, SqlDbType.Int).Value = quantidades[campo.Chave];
        comando.Parameters.Add("@Programa", SqlDbType.VarChar, 16).Value = dados.Programa!;
        comando.Parameters.Add("@Detalhes", SqlDbType.NVarChar, -1).Value =
            dados.Programa == "PROPRIO" ? dados.Detalhes! : DBNull.Value;

        object? id = await comando.ExecuteScalarAsync(cancellationToken);
        if (id is null || id is DBNull)
            throw new InvalidOperationException("SQL Server não retornou o protocolo da resposta.");
        return Convert.ToInt64(id);
    }
}
