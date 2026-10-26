using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

namespace MunicipioPortal.Pages.Admin;

public sealed class IndexModel(RespostaRepository repositorio,
    ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<RespostaMunicipal> Respostas { get; private set; } = [];
    public string? Erro { get; private set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        try { Respostas = await repositorio.ListarAsync(token); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Consulta administrativa SQL Server falhou.");
            Erro = "Não foi possível consultar as respostas no SQL Server.";
        }
    }

    public async Task<IActionResult> OnPostSairAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Admin/Login");
    }

    public async Task<IActionResult> OnGetExportarAsync(CancellationToken token)
    {
        IReadOnlyList<RespostaMunicipal> registros;
        try { registros = await repositorio.ListarAsync(token); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exportação municipal falhou.");
            return StatusCode(503, "Não foi possível exportar os dados.");
        }

        static string Csv(object? input)
        {
            var value = Convert.ToString(input, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            if (value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+') ||
                value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@'))
                value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        var sb = new StringBuilder();
        var campos = QuestionarioDefinition.Campos;
        var cabecalho = new[] { "Id", "CodigoIBGE", "Municipio" }
            .Concat(campos.Select(c => c.Coluna))
            .Concat(["ProgramaHabitacionalTipo", "ProgramaProprioDetalhes", "CriadoEm"]);
        sb.AppendLine(string.Join(";", cabecalho.Select(Csv)));
        foreach (var r in registros)
        {
            var valores = new object?[] { r.Id, r.CodigoIbge, r.Municipio }
                .Concat(campos.Select(c => (object?)r.Quantidades[c.Chave]))
                .Concat([r.Programa, r.Detalhes, r.CriadoEm.ToString("yyyy-MM-dd HH:mm:ss")]);
            sb.AppendLine(string.Join(";", valores.Select(Csv)));
        }
        var conteudo = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(conteudo, "text/csv; charset=utf-8", "respostas-municipios-goianos.csv");
    }
}
