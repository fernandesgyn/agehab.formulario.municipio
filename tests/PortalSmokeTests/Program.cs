using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MunicipioPortal.Models;

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
Console.WriteLine("OK: 246 municipalities, 16 fields, 4 options.");

sealed class StubEnvironment(string root) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "MunicipioPortal";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(root);
}
