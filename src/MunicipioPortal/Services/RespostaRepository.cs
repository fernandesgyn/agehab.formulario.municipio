using System.Data;
using Microsoft.Data.SqlClient;
using MunicipioPortal.Models;

namespace MunicipioPortal.Services;

/// <summary>Operações parametrizadas em SQL Server; não cria nem altera tabelas.</summary>
public sealed class RespostaRepository(IConfiguration config)
{
    private SqlConnection CriarConexao()
    {
        var connectionString = config.GetConnectionString("Municipios");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Municipios não configurada.");
        return new SqlConnection(connectionString);
    }

    public async Task<long> SalvarAsync(FormularioInput dados,
        IReadOnlyDictionary<string, int> quantidades, CancellationToken token)
    {
        var campos = QuestionarioDefinition.Campos;
        string colunas = string.Join(", ", campos.Select(c => c.Coluna));
        string parametros = string.Join(", ", campos.Select(c => "@" + c.Chave));
        string query = $"""
            INSERT INTO dbo.RespostasMunicipais
                (CodigoIBGE, {colunas}, ProgramaHabitacionalTipo, ProgramaProprioDetalhes)
            OUTPUT INSERTED.Id
            VALUES (@CodigoIbge, {parametros}, @Programa, @Detalhes);
            """;

        await using var conexao = CriarConexao();
        await conexao.OpenAsync(token);
        await using var cmd = new SqlCommand(query, conexao);
        cmd.Parameters.Add("@CodigoIbge", SqlDbType.Int).Value = dados.CodigoIbge!.Value;
        foreach (var campo in campos)
            cmd.Parameters.Add("@" + campo.Chave, SqlDbType.Int).Value = quantidades[campo.Chave];
        cmd.Parameters.Add("@Programa", SqlDbType.VarChar, 16).Value = dados.Programa!;
        cmd.Parameters.Add("@Detalhes", SqlDbType.NVarChar, -1).Value =
            dados.Programa == "PROPRIO" ? dados.Detalhes! : DBNull.Value;
        object? id = await cmd.ExecuteScalarAsync(token);
        return Convert.ToInt64(id);
    }

    private const string QueryDados = """
        SELECT R.*, M.Nome AS Municipio
          FROM dbo.RespostasMunicipais R
          INNER JOIN dbo.Municipios M ON M.CodigoIBGE = R.CodigoIBGE
        """;

    public async Task<IReadOnlyList<RespostaMunicipal>> ListarAsync(CancellationToken token)
    {
        var resultado = new List<RespostaMunicipal>();
        await using var conexao = CriarConexao();
        await conexao.OpenAsync(token);
        await using var cmd = new SqlCommand(QueryDados + " ORDER BY M.Nome;", conexao);
        await using var reader = await cmd.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
            resultado.Add(Converter(reader));
        return resultado;
    }

    public async Task<RespostaMunicipal?> ObterAsync(long id, CancellationToken token)
    {
        await using var conexao = CriarConexao();
        await conexao.OpenAsync(token);
        await using var cmd = new SqlCommand(QueryDados + " WHERE R.Id = @Id;", conexao);
        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
        await using var reader = await cmd.ExecuteReaderAsync(token);
        return await reader.ReadAsync(token) ? Converter(reader) : null;
    }

    private static RespostaMunicipal Converter(SqlDataReader r)
    {
        var nums = QuestionarioDefinition.Campos.ToDictionary(
            c => c.Chave, c => r.GetInt32(r.GetOrdinal(c.Coluna)));
        int idx = r.GetOrdinal("ProgramaProprioDetalhes");
        return new RespostaMunicipal(
            r.GetInt64(r.GetOrdinal("Id")),
            r.GetInt32(r.GetOrdinal("CodigoIBGE")),
            r.GetString(r.GetOrdinal("Municipio")),
            r.GetDateTime(r.GetOrdinal("CriadoEm")),
            r.GetString(r.GetOrdinal("ProgramaHabitacionalTipo")),
            r.IsDBNull(idx) ? null : r.GetString(idx),
            nums);
    }
}
