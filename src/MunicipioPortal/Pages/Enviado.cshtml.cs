using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MunicipioPortal.Pages;

public sealed class EnviadoModel : PageModel
{
    public string Protocolo { get; private set; } = "";

    public IActionResult OnGet()
    {
        Protocolo = TempData["Protocolo"]?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(Protocolo))
            return RedirectToPage("/Index");
        return Page();
    }
}
