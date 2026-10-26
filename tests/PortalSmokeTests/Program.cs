using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception("Failed: " + message);
}

Check(QuestionarioDefinition.Areas.Count == 4, "4 areas");
Check(QuestionarioDefinition.Campos.Count == 16, "16 fields");
Check(QuestionarioDefinition.Campos.DistinctBy(x => x.Chave).Count() == 16, "unique fields");
Check(QuestionarioDefinition.Campos.Any(x => x.Coluna == "Q2_EFETIVOS"), "SQL name");
Check(QuestionarioDefinition.Programas.Count == 4, "4 options");
Check(QuestionarioDefinition.ProgramaValido("PROPRIO"), "valid option");
Check(!QuestionarioDefinition.ProgramaValido("INVALIDO"), "reject invalid option");

string root = Path.GetFullPath("src/MunicipioPortal");
var municipalities = new MunicipiosCatalog(new StubEnvironment(root));
Check(municipalities.Todos.Count == 246, "246 cities");
Check(municipalities.Existe(5208707), "Goiania");
Check(municipalities.Existe(5200134), "Acreuna");
Check(!municipalities.Existe(9999999), "reject unknown city");

Check(SqlServerFailure.Classificar(18456).Codigo == "BD-ACESSO", "SQL authentication");
Check(SqlServerFailure.Classificar(208).Codigo == "BD-ESTRUTURA", "SQL schema");
Check(SqlServerFailure.Classificar(229).Codigo == "BD-PERMISSAO", "SQL permissions");
Check(SqlServerFailure.Classificar(53).Codigo == "BD-CONEXAO", "SQL network");
Check(SqlServerFailure.Classificar(2627).Codigo == "BD-DUPLICIDADE", "SQL duplicate");

var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
await using (var output = new StringWriter())
{
    int result = await SqlServerDiagnostic.VerificarAsync(config, output);
    Check(result == 2, "missing connection diagnostic exit code");
    Check(output.ToString().Contains("BD-CONFIG"), "missing connection diagnostic text");
}

var layout = File.ReadAllText(Path.Combine(root, "Pages", "Shared", "_Layout.cshtml"));
Check(layout.Contains("Agência Goiana de Habitação - AGEHAB"), "institutional brand");
Check(!layout.Contains("Formulário Municipal"), "old sidebar label removed");
Check(!layout.Contains("asp-page=\"/Admin/"), "no admin links");

Console.WriteLine("OK: 246 municipalities, 16 fields, validation and SQL Server diagnostics.");

sealed class StubEnvironment(string root) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "MunicipioPortal";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(root);
}
