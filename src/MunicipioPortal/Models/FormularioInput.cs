namespace MunicipioPortal.Models;

public sealed class FormularioInput
{
    public int? CodigoIbge { get; set; }
    public Dictionary<string, string?> Quantidades { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public string? Estrutura { get; set; }
    public string? EstruturaOutra { get; set; }
    public string? Programa { get; set; }
    public string? Detalhes { get; set; }
    public Dictionary<string, string?> Infraestrutura { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
