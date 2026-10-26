namespace MunicipioPortal.Models;

public sealed record Campo(string Chave, string Coluna, string Rotulo);
public sealed record Area(int Numero, string Titulo, IReadOnlyList<Campo> Campos);
public sealed record OpcaoPrograma(string Codigo, string Titulo);

public static class QuestionarioDefinition
{
    private static readonly (string Chave, string Titulo)[] Vinculos =
    [
        ("efetivos", "Efetivos / concursados"),
        ("terceirizados", "Terceirizados"),
        ("temporarios", "Contrato temporário"),
        ("comissionados", "Cargo em comissão")
    ];

    private static readonly (int Numero, string Titulo)[] AreasFonte =
    [
        (2, "Cadastro e seleção das famílias"),
        (3, "Visitas sociais e elaboração de relatório social"),
        (4, "Trabalho Social (TS) — fase pré-ocupacional"),
        (5, "Trabalho Social (TS) — fase pós-ocupacional")
    ];

    public static readonly IReadOnlyList<Area> Areas = AreasFonte
        .Select(a => new Area(a.Numero, a.Titulo,
            Vinculos.Select(v => new Campo(
                $"q{a.Numero}_{v.Chave}",
                $"Q{a.Numero}_{v.Chave.ToUpperInvariant()}",
                v.Titulo)).ToArray())).ToArray();

    public static readonly IReadOnlyList<Campo> Campos =
        Areas.SelectMany(a => a.Campos).ToArray();

    public static readonly IReadOnlyList<OpcaoPrograma> Programas =
    [
        new("PROPRIO", "Sim, possui programa habitacional próprio com recursos exclusivos do município"),
        new("ESTADUAL", "Não, atua somente com o Governo de Goiás no Programa Pra Ter Onde Morar"),
        new("FEDERAL", "Não, atua somente com o Governo Federal em empreendimentos do Minha Casa, Minha Vida"),
        new("AMBOS", "Não, atua com os programas Pra Ter Onde Morar e Minha Casa, Minha Vida")
    ];

    public static bool ProgramaValido(string? codigo) =>
        Programas.Any(p => p.Codigo == codigo);

    public static string DescricaoPrograma(string codigo) =>
        Programas.FirstOrDefault(p => p.Codigo == codigo)?.Titulo ?? codigo;
}
