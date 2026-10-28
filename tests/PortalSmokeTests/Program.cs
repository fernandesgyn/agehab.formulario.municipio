using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception("Failed: " + message);
}

var areas = QuestionarioDefinition.Areas;
Check(areas.Count == 4, "numeric sections 2-5");
Check(areas.Select(a => a.Numero).SequenceEqual([2, 3, 4, 5]), "numeric section numbering");
Check(QuestionarioDefinition.Campos.Count == 17, "17 numeric fields");
Check(QuestionarioDefinition.Campos.Select(c => c.Chave).Distinct().Count() == 17, "unique input keys");
Check(QuestionarioDefinition.Campos.Select(c => c.Coluna).Distinct().Count() == 17, "unique database columns");
Check(QuestionarioDefinition.Campos.Count(c => c.Item.StartsWith("2.")) == 5, "five Q2 fields");
Check(QuestionarioDefinition.Campos.Count(c => c.Item.StartsWith("3.")) == 5, "five Q3 fields");
Check(QuestionarioDefinition.Campos.Count(c => c.Item.StartsWith("4.")) == 4, "four Q4 fields");
Check(QuestionarioDefinition.Campos.Count(c => c.Item.StartsWith("5.")) == 3, "three Q5 fields");
Check(!areas[0].Campos.Last().SomaTotal, "Q2 CRESS does not add to link totals");
Check(!areas[1].Campos.Last().SomaTotal, "Q3 exclusive does not add to link totals");
Check(QuestionarioDefinition.Campos.Any(c => c.Coluna == "Q2_CRESS"), "CRESS count");
Check(QuestionarioDefinition.Campos.Any(c => c.Coluna == "Q3_EXCLUSIVOS"), "exclusively selecting families");
Check(QuestionarioDefinition.Campos.Any(c => c.Coluna == "Q5_CRESS_COORDENACAO"), "social workers coordination");
Check(QuestionarioDefinition.Estruturas.Count == 9, "nine organization options");
Check(QuestionarioDefinition.EstruturaValida("OUTRA"), "valid other organization option");
Check(!QuestionarioDefinition.EstruturaValida("INVALIDA"), "reject invalid organization");
Check(QuestionarioDefinition.Programas.Count == 4, "four Q7 options");
Check(QuestionarioDefinition.ProgramaValido("PROPRIO"), "own municipal program");
Check(!QuestionarioDefinition.ProgramaValido("INVALIDO"), "reject invalid program");
Check(QuestionarioDefinition.Infraestrutura.Count == 13, "13 Q8 items");
Check(QuestionarioDefinition.Infraestrutura[0].Item == "8.1", "Q8 beginning");
Check(QuestionarioDefinition.Infraestrutura[^1].Item == "8.13", "Q8 final");
Check(QuestionarioDefinition.Infraestrutura.All(i => i.Coluna.StartsWith("Q8_")), "Q8 SQL columns");
Check(QuestionarioDefinition.Infraestrutura.Select(i => i.Coluna).Distinct().Count() == 13, "Q8 unique columns");

string root = Path.GetFullPath("src/MunicipioPortal");
var municipalities = new MunicipiosCatalog(new StubEnvironment(root));
Check(municipalities.Todos.Count == 246, "246 cities");
Check(municipalities.Existe(5208707), "Goiania");
Check(municipalities.Existe(5200134), "Acreuna");
Check(!municipalities.Existe(9999999), "reject unknown city");

string formulario = File.ReadAllText(Path.Combine(root, "Pages", "Index.cshtml"));
Check(formulario.Contains("7. O Município possui algum Programa Habitacional próprio"), "Q7 label");
Check(formulario.Contains("8.0 Acesso aos Serviços Públicos"), "Q8 title");
Check(formulario.Contains("Dados.Infraestrutura["), "Q8 binding");
Check(formulario.Contains("Dados.Estrutura"), "Q6 select");

string sqlNovo = File.ReadAllText(Path.Combine("database", "schema.sql"));
string sqlMigracao = File.ReadAllText(Path.Combine("database", "migracao-v2.sql"));
Check(sqlNovo.Contains("CREATE TABLE dbo.RespostasMunicipaisV2"), "new SQL schema");
Check(!sqlNovo.Contains("CREATE TABLE dbo.RespostasMunicipais ("), "no old schema in initial install");
Check(sqlMigracao.Contains("IF OBJECT_ID(N'dbo.RespostasMunicipaisV2', N'U') IS NULL"), "idempotent migration");
Check(!sqlMigracao.Contains("DROP TABLE"), "no table deletion");
Check(!sqlMigracao.Contains("DELETE FROM"), "no deletion of older answers");
Check(sqlMigracao.Contains("CK_RM2_Q2_CRESS_TOTAL"), "Q2 check constraint");
Check(sqlMigracao.Contains("CK_RM2_Q3_EXCLUSIVOS_TOTAL"), "Q3 check constraint");
foreach (var campo in QuestionarioDefinition.Campos)
    Check(sqlMigracao.Contains(campo.Coluna + " INT NOT NULL"), "SQL numeric column " + campo.Coluna);
foreach (var item in QuestionarioDefinition.Infraestrutura)
    Check(sqlMigracao.Contains(item.Coluna + " BIT NOT NULL"), "SQL Boolean column " + item.Coluna);

Check(SqlServerFailure.Classificar(18456).Codigo == "BD-ACESSO", "SQL auth");
Check(SqlServerFailure.Classificar(208).Codigo == "BD-ESTRUTURA", "SQL schema");
Check(SqlServerFailure.Classificar(229).Codigo == "BD-PERMISSAO", "SQL permissions");
Check(SqlServerFailure.Classificar(53).Codigo == "BD-CONEXAO", "SQL network");
Check(SqlServerFailure.Classificar(2627).Codigo == "BD-DUPLICIDADE", "SQL unique");

var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
await using (var output = new StringWriter())
{
    int result = await SqlServerDiagnostic.VerificarAsync(config, output);
    Check(result == 2, "missing connection diagnostic exit code");
    Check(output.ToString().Contains("BD-CONFIG"), "missing connection diagnostic");
}

string layout = File.ReadAllText(Path.Combine(root, "Pages", "Shared", "_Layout.cshtml"));
Check(layout.Contains("Agência Goiana de Habitação - AGEHAB"), "institutional brand");
Check(!layout.Contains("asp-page=\"/Admin/"), "no admin links");

Console.WriteLine("OK: 246 municípios, 17 quantidades, 9 estruturas, 4 programas, 13 itens de infraestrutura, SQL V2.");

sealed class StubEnvironment(string root) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "MunicipioPortal";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } =
        new PhysicalFileProvider(root);
}
