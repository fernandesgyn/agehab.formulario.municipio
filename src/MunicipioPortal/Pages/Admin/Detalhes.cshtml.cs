using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

namespace MunicipioPortal.Pages.Admin;

public sealed class DetalhesModel(RespostaRepository repositorio,
    ILogger<DetalhesModel> logger) : PageModel
{
    public RespostaMunicipal? Resposta { get; private set; }

    public async Task<IActionResult> OnGetAsync(long id, CancellationToken token)
    {
        if (id <= 0) return BadRequest();
        try
        {
            Resposta = await repositorio.ObterAsync(id, token);
            return Resposta is null ? NotFound() : Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha na consulta da resposta {Protocolo}.", id);
            return StatusCode(503, "Não foi possível consultar a resposta.");
        }
    }
}
