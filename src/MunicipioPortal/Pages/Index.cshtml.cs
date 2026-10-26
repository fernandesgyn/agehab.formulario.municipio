using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

namespace MunicipioPortal.Pages;

public sealed class IndexModel(
    MunicipiosCatalog municipios,
    RespostaRepository repositorio,
    ILogger<IndexModel> logger) : PageModel
{
    [BindProperty]
    public FormularioInput Dados { get; set; } = new();

    public IReadOnlyList<Municipio> Municipios => municipios.Todos;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        if (Dados.CodigoIbge is null || !municipios.Existe(Dados.CodigoIbge.Value))
            ModelState.AddModelError("Dados.CodigoIbge", "Selecione um município válido de Goiás.");

        var quantidades = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var campo in QuestionarioDefinition.Campos)
        {
            Dados.Quantidades.TryGetValue(campo.Chave, out var valor);
            if (!int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out int numero) ||
                numero < 0 || numero > 100_000)
            {
                ModelState.AddModelError($"Dados.Quantidades[{campo.Chave}]",
                    "Informe um número inteiro entre 0 e 100.000.");
                continue;
            }
            quantidades[campo.Chave] = numero;
        }

        if (!QuestionarioDefinition.ProgramaValido(Dados.Programa))
            ModelState.AddModelError("Dados.Programa", "Selecione uma alternativa da questão 6.");

        Dados.Detalhes = Dados.Detalhes?.Trim();
        if (Dados.Programa == "PROPRIO")
        {
            if (string.IsNullOrWhiteSpace(Dados.Detalhes) ||
                Dados.Detalhes.Length is < 3 or > 2000)
                ModelState.AddModelError("Dados.Detalhes",
                    "Informe o programa e o benefício concedido (de 3 a 2.000 caracteres).");
        }
        else Dados.Detalhes = null;

        if (!ModelState.IsValid) return Page();

        try
        {
            long id = await repositorio.SalvarAsync(Dados, quantidades, token);
            TempData["Protocolo"] = id.ToString(CultureInfo.InvariantCulture);
            return RedirectToPage("/Enviado");
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("Dados.CodigoIbge",
                "Este município já enviou uma resposta. Para correções, entre em contato com a AGEHAB.");
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            logger.LogError(ex, "Falha ao salvar resposta municipal.");
            ModelState.AddModelError(string.Empty,
                "Não foi possível concluir o envio. Tente novamente ou contate a equipe responsável.");
        }
        return Page();
    }
}
