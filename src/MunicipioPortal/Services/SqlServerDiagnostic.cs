using Microsoft.Data.SqlClient;
using MunicipioPortal.Models;

namespace MunicipioPortal.Services;

/// <summary>Verifica somente leitura, sem expor endpoint na internet.</summary>
public static class SqlServerDiagnostic
{
    public static async Task<int> VerificarAsync(
        IConfiguration config,
        TextWriter saida,
        CancellationToken token = default)
    {
        try
        {
            await using var conexao = SqlServerConnectionFactory.Criar(config);
            await conexao.OpenAsync(token);
            await saida.WriteLineAsync("SQL Server: conexão estabelecida.");

            await using var tabelas = new SqlCommand("""
                SELECT
                    CASE WHEN OBJECT_ID(N'dbo.Municipios', N'U') IS NOT NULL THEN 1 ELSE 0 END,
                    CASE WHEN OBJECT_ID(N'dbo.RespostasMunicipaisV2', N'U') IS NOT NULL THEN 1 ELSE 0 END;
                """, conexao);
            await using var leitor = await tabelas.ExecuteReaderAsync(token);
            await leitor.ReadAsync(token);
            bool municipios = leitor.GetInt32(0) == 1;
            bool respostas = leitor.GetInt32(1) == 1;
            await leitor.CloseAsync();
            if (!municipios || !respostas)
            {
                await saida.WriteLineAsync(
                    "BD-ESTRUTURA: faltam dbo.Municipios ou dbo.RespostasMunicipaisV2.");
                await saida.WriteLineAsync(
                    "Para um banco existente, execute database/migracao-v2.sql no SSMS.");
                return 3;
            }

            var tabela = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var colunas = new SqlCommand("""
                SELECT [name] FROM sys.columns
                WHERE [object_id] = OBJECT_ID(N'dbo.RespostasMunicipaisV2');
                """, conexao))
            await using (var rd = await colunas.ExecuteReaderAsync(token))
            {
                while (await rd.ReadAsync(token)) tabela.Add(rd.GetString(0));
            }

            string[] obrigatorias = ["Id", "CodigoIBGE", "CriadoEm",
                "Q6_ESTRUTURA", "Q6_OUTRA_DESCRICAO",
                "Q7_PROGRAMA", "Q7_PROGRAMA_DETALHES",
                .. QuestionarioDefinition.Campos.Select(c => c.Coluna),
                .. QuestionarioDefinition.Infraestrutura.Select(i => i.Coluna)];
            string[] ausentes = obrigatorias.Where(c => !tabela.Contains(c)).ToArray();
            if (ausentes.Length > 0)
            {
                await saida.WriteLineAsync(
                    "BD-ESTRUTURA: campos ausentes: " + string.Join(", ", ausentes));
                return 3;
            }

            await using var consulta = new SqlCommand("""
                SELECT COUNT_BIG(*) FROM dbo.Municipios;
                SELECT ISNULL(HAS_PERMS_BY_NAME('dbo.RespostasMunicipaisV2', 'OBJECT', 'INSERT'), 0);
                """, conexao);
            await using var resultado = await consulta.ExecuteReaderAsync(token);
            await resultado.ReadAsync(token);
            long totalMunicipios = resultado.GetInt64(0);
            await resultado.NextResultAsync(token);
            await resultado.ReadAsync(token);
            int podeInserir = resultado.GetInt32(0);

            await saida.WriteLineAsync($"Municípios: {totalMunicipios} (esperado: 246).");
            await saida.WriteLineAsync(
                "Permissão INSERT em RespostasMunicipaisV2: " +
                (podeInserir == 1 ? "OK" : "NEGADA"));
            if (totalMunicipios != 246 || podeInserir != 1) return 3;

            await saida.WriteLineAsync("OK: SQL Server preparado para as novas respostas (V2).");
            return 0;
        }
        catch (SqlServerNaoConfiguradoException)
        {
            await saida.WriteLineAsync(
                "BD-CONFIG: configure ConnectionStrings__Municipios no ambiente.");
            return 2;
        }
        catch (SqlException ex)
        {
            var erro = SqlServerFailure.Classificar(ex.Number);
            await saida.WriteLineAsync($"{erro.Codigo}: {erro.Mensagem}");
            return 3;
        }
        catch (Exception)
        {
            await saida.WriteLineAsync(
                "BD-OPERACAO: falha ao validar SQL Server. Confira logs do servidor.");
            return 3;
        }
    }
}
