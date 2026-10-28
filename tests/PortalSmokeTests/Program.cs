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
Check(sqlNovo.Contains("CREATE DATABASE AGEHAB_FORMULARIO_MUNICIPIOS;"), "database creation");
Check(sqlNovo.Contains("CREATE TABLE dbo.Municipios ("), "municipalities table");
Check(sqlNovo.Contains("CREATE TABLE dbo.RespostasMunicipais ("), "single definitive answers table");
Check(!sqlNovo.Contains("RespostasMunicipaisV2"), "no versioned table");
Check(!sqlNovo.Contains("IF OBJECT_ID"), "no conditional DDL or migrations");
Check(!sqlNovo.Contains("DROP TABLE"), "no destructive DDL");
Check(!sqlNovo.Contains("DELETE FROM"), "no data deletion");
Check(sqlNovo.Contains("CK_RM_Q2_CRESS_TOTAL"), "Q2 check constraint");
Check(sqlNovo.Contains("CK_RM_Q3_EXCLUSIVOS_TOTAL"), "Q3 check constraint");
foreach (var campo in QuestionarioDefinition.Campos)
    Check(sqlNovo.Contains(campo.Coluna + " INT NOT NULL"), "SQL numeric column " + campo.Coluna);
foreach (var item in QuestionarioDefinition.Infraestrutura)
    Check(sqlNovo.Contains(item.Coluna + " BIT NOT NULL"), "SQL Boolean column " + item.Coluna);
string repoText = File.ReadAllText(Path.Combine(root, "Services", "RespostaRepository.cs"));
Check(repoText.Contains("INSERT INTO dbo.RespostasMunicipais"), "final answers table in repository");
Check(!repoText.Contains("RespostasMunicipaisV2"), "repository has no V2");
string diagnosticText = File.ReadAllText(Path.Combine(root, "Services", "SqlServerDiagnostic.cs"));
Check(diagnosticText.Contains("OBJECT_ID(N'dbo.RespostasMunicipais'"), "diagnostic on final table");
Check(!diagnosticText.Contains("migracao-v2"), "diagnostic is migration-free");
Check(File.Exists(Path.Combine("database", "consultas.sql")), "queries for final schema");
Check(!File.Exists(Path.Combine("database", "migracao-v2.sql")), "no migration file");
Check(!File.Exists(Path.Combine("database", "consultas-v2.sql")), "no versioned query file");

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

Console.WriteLine("OK: 246 municípios, 17 quantidades, 9 estruturas, 4 programas, 13 itens de infraestrutura, SQL definitivo sem migrations.");

sealed class StubEnvironment(string root) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "MunicipioPortal";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } =
        new PhysicalFileProvider(root);
}
