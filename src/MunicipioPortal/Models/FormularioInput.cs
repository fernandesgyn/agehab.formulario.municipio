namespace MunicipioPortal.Models;

public sealed class FormularioInput
{
    public int? CodigoIbge { get; set; }
    public Dictionary<string, string?> Quantidades { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public string? Programa { get; set; }
    public string? Detalhes { get; set; }
}

public sealed record RespostaMunicipal(
    long Id,
    int CodigoIbge,
    string Municipio,
    DateTime CriadoEm,
    string Programa,
    string? Detalhes,
    IReadOnlyDictionary<string, int> Quantidades)
{
    public int Total(int numero) => QuestionarioDefinition.Areas
        .Single(a => a.Numero == numero).Campos
        .Sum(c => Quantidades.GetValueOrDefault(c.Chave));
}

public sealed class AdminIdentity;
