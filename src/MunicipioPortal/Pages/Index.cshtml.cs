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

        ValidarSubtotais(quantidades);

        if (!QuestionarioDefinition.EstruturaValida(Dados.Estrutura))
            ModelState.AddModelError("Dados.Estrutura", "Selecione uma estrutura organizacional válida.");

        Dados.EstruturaOutra = Dados.EstruturaOutra?.Trim();
        if (Dados.Estrutura == "OUTRA")
        {
            if (string.IsNullOrWhiteSpace(Dados.EstruturaOutra) ||
                Dados.EstruturaOutra.Length is < 3 or > 300)
                ModelState.AddModelError("Dados.EstruturaOutra",
                    "Descreva a outra estrutura (entre 3 e 300 caracteres).");
        }
        else Dados.EstruturaOutra = null;

        if (!QuestionarioDefinition.ProgramaValido(Dados.Programa))
            ModelState.AddModelError("Dados.Programa", "Selecione uma alternativa da questão 7.");

        Dados.Detalhes = Dados.Detalhes?.Trim();
        if (Dados.Programa == "PROPRIO")
        {
            if (string.IsNullOrWhiteSpace(Dados.Detalhes) || Dados.Detalhes.Length is < 3 or > 2000)
                ModelState.AddModelError("Dados.Detalhes",
                    "Informe o nome do programa e o benefício concedido (de 3 a 2.000 caracteres).");
        }
        else Dados.Detalhes = null;

        var infraestrutura = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in QuestionarioDefinition.Infraestrutura)
        {
            Dados.Infraestrutura.TryGetValue(item.Chave, out var resposta);
            if (resposta == "SIM") infraestrutura[item.Chave] = true;
            else if (resposta == "NAO") infraestrutura[item.Chave] = false;
            else ModelState.AddModelError($"Dados.Infraestrutura[{item.Chave}]",
                $"Selecione Sim ou Não no item {item.Item}.");
        }

        if (!ModelState.IsValid) return Page();

        try
        {
            long protocolo = await repositorio.SalvarAsync(
                Dados, quantidades, infraestrutura, token);
            TempData["Protocolo"] = protocolo.ToString(CultureInfo.InvariantCulture);
            return RedirectToPage("/Enviado");
        }
        catch (SqlServerNaoConfiguradoException ex)
        {
            logger.LogError(ex, "BD-CONFIG: conexão do SQL Server não configurada.");
            ErroPersistencia = "A conexão com o SQL Server não foi configurada (BD-CONFIG). " +
                "A equipe de TI deve configurar ConnectionStrings__Municipios.";
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("Dados.CodigoIbge",
                "Este município já enviou uma resposta deste questionário. Para correções, entre em contato com a AGEHAB.");
        }
        catch (SqlException ex)
        {
            var falha = SqlServerFailure.Classificar(ex.Number);
            logger.LogError(ex, "Falha SQL Server {Codigo}, erro {Numero}.", falha.Codigo, ex.Number);
            ErroPersistencia = $"{falha.Mensagem} ({falha.Codigo}).";
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Falha de gravação no SQL Server.");
            ErroPersistencia = "O SQL Server não concluiu o registro. Contate a equipe de TI (BD-OPERACAO).";
        }
        return Page();
    }

    private void ValidarSubtotais(IReadOnlyDictionary<string, int> quantidades)
    {
        foreach (int numero in new[] { 2, 3 })
        {
            var area = QuestionarioDefinition.Areas.Single(a => a.Numero == numero);
            if (!area.Campos.All(c => quantidades.ContainsKey(c.Chave))) continue;
            int totalVinculos = area.Campos.Where(c => c.SomaTotal)
                .Sum(c => quantidades[c.Chave]);
            var especifico = area.Campos.Single(c => !c.SomaTotal);
            if (quantidades[especifico.Chave] > totalVinculos)
                ModelState.AddModelError($"Dados.Quantidades[{especifico.Chave}]",
                    $"A quantidade do item {especifico.Item} não pode superar o total dos vínculos da questão {numero}.");
        }
    }
}
