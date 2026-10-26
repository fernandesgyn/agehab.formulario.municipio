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

    /// <summary>Mensagem pública sem detalhes de credenciais ou SQL.</summary>
    public string? ErroPersistencia { get; private set; }

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
        catch (SqlServerNaoConfiguradoException ex)
        {
            logger.LogError(ex, "BD-CONFIG: conexão do SQL Server não configurada no IIS.");
            ErroPersistencia = "A conexão com o SQL Server não foi configurada neste servidor (BD-CONFIG). " +
                "A equipe de TI deve configurar ConnectionStrings__Municipios para habilitar os envios.";
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("Dados.CodigoIbge",
                "Este município já enviou uma resposta. Para correções, entre em contato com a AGEHAB.");
        }
        catch (SqlException ex)
        {
            var falha = SqlServerFailure.Classificar(ex.Number);
            logger.LogError(ex, "Falha de gravação no SQL Server: {Codigo}, erro {Numero}.",
                falha.Codigo, ex.Number);
            ErroPersistencia = $"{falha.Mensagem} ({falha.Codigo}).";
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "BD-OPERACAO: falha ao executar a gravação.");
            ErroPersistencia = "O SQL Server não concluiu o registro. " +
                "A equipe de TI deve verificar os logs da aplicação (BD-OPERACAO).";
        }
        return Page();
    }
}
