using MunicipioPortal.Models;
using MunicipioPortal.Services;

bool verificarBanco = args.Length == 1 && args[0] == "--verificar-sqlserver";
var builder = WebApplication.CreateBuilder(verificarBanco ? [] : args);

if (verificarBanco)
{
    Environment.ExitCode = await SqlServerDiagnostic.VerificarAsync(builder.Configuration, Console.Out);
    return;
}

builder.Services.AddRazorPages();
builder.Services.AddSingleton<MunicipiosCatalog>();
builder.Services.AddScoped<RespostaRepository>();
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
app.MapRazorPages();
app.Run();

public partial class Program;
