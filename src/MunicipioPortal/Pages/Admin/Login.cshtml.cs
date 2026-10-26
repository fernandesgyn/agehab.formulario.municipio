using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using MunicipioPortal.Models;

namespace MunicipioPortal.Pages.Admin;

[EnableRateLimiting("acesso")]
public sealed class LoginModel(
    IConfiguration configuracao,
    IPasswordHasher<AdminIdentity> hasher) : PageModel
{
    [BindProperty]
    public string Senha { get; set; } = "";
    public string? Mensagem { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Admin/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var hash = configuracao["Admin:PasswordHash"];
        if (string.IsNullOrWhiteSpace(hash))
        {
            Mensagem = "Acesso administrativo ainda não configurado no servidor.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Senha) ||
            hasher.VerifyHashedPassword(new AdminIdentity(), hash, Senha) ==
                PasswordVerificationResult.Failed)
        {
            Mensagem = "Senha inválida.";
            return Page();
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Administração AGEHAB"),
             new Claim(ClaimTypes.Role, "Administrador")],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });
        return RedirectToPage("/Admin/Index");
    }
}
