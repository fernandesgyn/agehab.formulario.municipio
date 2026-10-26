using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MunicipioPortal.Models;
using MunicipioPortal.Services;

if (args.Length == 1 && args[0] == "--gerar-hash-admin")
{
    Console.Write("Digite a senha administrativa (mínimo de 12 caracteres): ");
    var senha = new StringBuilder();
    if (!Console.IsInputRedirected)
    {
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter) break;
            if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0) senha.Length--;
            else if (!char.IsControl(tecla.KeyChar)) senha.Append(tecla.KeyChar);
        }
        Console.WriteLine();
    }
    else senha.Append(Console.ReadLine());
    if (senha.Length < 12) { Console.Error.WriteLine("Senha curta."); return; }
    Console.WriteLine(new PasswordHasher<AdminIdentity>().HashPassword(new AdminIdentity(), senha.ToString()));
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});
builder.Services.AddSingleton<MunicipiosCatalog>();
builder.Services.AddScoped<RespostaRepository>();
builder.Services.AddSingleton<IPasswordHasher<AdminIdentity>, PasswordHasher<AdminIdentity>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acesso";
        options.AccessDeniedPath = "/acesso";
        options.Cookie.Name = "Agehab.Municipios.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("acesso", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(180));
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

public partial class Program;
