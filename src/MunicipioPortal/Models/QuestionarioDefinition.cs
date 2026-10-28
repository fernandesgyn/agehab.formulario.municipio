namespace MunicipioPortal.Models;

public sealed record Campo(string Chave, string Coluna, string Rotulo, string Item, bool SomaTotal = true);
public sealed record Area(int Numero, string Titulo, string? Orientacao, IReadOnlyList<Campo> Campos);
public sealed record OpcaoPrograma(string Codigo, string Titulo);
public sealed record OpcaoEstrutura(string Codigo, string Titulo);
public sealed record ItemInfraestrutura(string Chave, string Coluna, string Item, string Texto);

public static class QuestionarioDefinition
{
    private const string OrientacaoVinculos =
        "Coloque a quantidade em cada vínculo, sendo \"0\" para os casos em que não tiver profissionais no respectivo vínculo.";

    private static Campo Vinculo(int numero, int item, string chave, string rotulo) =>
        new($"q{numero}_{chave}", $"Q{numero}_{chave.ToUpperInvariant()}", rotulo, $"{numero}.{item}");

    public static readonly IReadOnlyList<Area> Areas =
    [
        new(2, "Quantidade de Profissionais que atuam no cadastro de seleção das famílias",
            OrientacaoVinculos,
            [
                Vinculo(2, 1, "efetivos", "Efetivos/concursados"),
                Vinculo(2, 2, "terceirizados", "Terceirizados"),
                Vinculo(2, 3, "temporarios", "Contrato Temporário"),
                Vinculo(2, 4, "comissionados", "Cargo em Comissão"),
                new("q2_cress", "Q2_CRESS", "Do total de profissionais informados, quantos são assistentes sociais com registro ativo no CRESS?", "2.5", false)
            ]),
        new(3, "Quantidade de Assistentes Sociais que atuaram na realização das Visitas Sociais /Elaboração de Relatório Social para seleção",
            OrientacaoVinculos,
            [
                Vinculo(3, 1, "efetivos", "Efetivos/concursados"),
                Vinculo(3, 2, "terceirizados", "Terceirizados"),
                Vinculo(3, 3, "temporarios", "Contrato Temporário"),
                Vinculo(3, 4, "comissionados", "Cargo em Comissão"),
                new("q3_exclusivos", "Q3_EXCLUSIVOS", "Do total de profissionais informados, quantos assistentes sociais atuaram exclusivamente na seleção das famílias?", "3.5", false)
            ]),
        new(4, "Quantidade de Profissionais que atuam no Trabalho Social (TS) - Fase Pós-Ocupacional",
            OrientacaoVinculos,
            [
                Vinculo(4, 1, "efetivos", "Efetivos/concursados"),
                Vinculo(4, 2, "terceirizados", "Terceirizados"),
                Vinculo(4, 3, "temporarios", "Contrato Temporário"),
                Vinculo(4, 4, "comissionados", "Cargo em Comissão")
            ]),
        new(5, "Informe a quantidade de profissionais que compõem a equipe responsável pelo Trabalho Social na fase pós-ocupacional, conforme as categorias abaixo.",
            null,
            [
                new("q5_cress_coordenacao", "Q5_CRESS_COORDENACAO", "Assistentes sociais com registro ativo no CRESS, responsáveis pela coordenação do Trabalho Social", "5.1"),
                new("q5_areas_afins", "Q5_AREAS_AFINS", "Profissionais de áreas afins", "5.2"),
                new("q5_administrativos", "Q5_ADMINISTRATIVOS", "Profissionais administrativos de nível médio", "5.3")
            ])
    ];

    public static readonly IReadOnlyList<Campo> Campos = Areas.SelectMany(a => a.Campos).ToArray();

    public static readonly IReadOnlyList<OpcaoEstrutura> Estruturas =
    [
        new("SECRETARIA_EXCLUSIVA", "Secretaria Municipal exclusiva de Habitação"),
        new("SECRETARIA_MISTA", "Secretaria Municipal que acumula Habitação e outras políticas públicas"),
        new("SUPERINTENDENCIA", "Superintendência ou órgão equivalente"),
        new("DIRETORIA", "Diretoria"),
        new("GERENCIA_COORDENACAO", "Gerência ou Coordenação"),
        new("DEPARTAMENTO_SETOR", "Departamento ou Setor"),
        new("VINCULADA_OUTRA", "A política habitacional está vinculada a outra Secretaria (ex.: Assistência Social, Planejamento ou Infraestrutura)"),
        new("SEM_ESTRUTURA", "Não existe estrutura específica para a área habitacional"),
        new("OUTRA", "Outra estrutura")
    ];

    public static readonly IReadOnlyList<OpcaoPrograma> Programas =
    [
        new("PROPRIO", "7.1 - Sim"),
        new("ESTADUAL", "7.2 - Não, só atuamos junto ao Governo de Goiás com Programa Pra Ter Onde Morar"),
        new("FEDERAL", "7.3 - Não, só atuamos junto ao Governo Federal com empreendimentos do Minha Casa, Minha Vida"),
        new("AMBOS", "7.4 - Não, atuamos com Pra Ter Onde Morar e Minha Casa Minha Vida")
    ];

    public static readonly IReadOnlyList<ItemInfraestrutura> Infraestrutura =
    [
        new("q8_01", "Q8_01", "8.1", "Possui Educação Infantil (0-5 anos) CMEI em até no máximo 1,0 Km do empreendimento?"),
        new("q8_02", "Q8_02", "8.2", "Possui Escola Municipal/Estadual Fundamental I até 1,5 Km do empreendimento?"),
        new("q8_03", "Q8_03", "8.3", "Possui Escola Municipal/Estadual Fundamental II até 1,5 Km do empreendimento?"),
        new("q8_04", "Q8_04", "8.4", "Possui acesso a esporte e lazer (praças, quadras esportivas, outros) em até 1,5 Km do empreendimento?"),
        new("q8_05", "Q8_05", "8.5", "Possui Assistência Social CRAS/CREAS em até 2Km do empreendimento?"),
        new("q8_06", "Q8_06", "8.6", "Possui Posto de Saúde em até no máximo 2,5 Km do empreendimento?"),
        new("q8_07", "Q8_07", "8.7", "Possui acesso de equipamentos de uso cotidiano (padaria, merceria, mercados ou farmácias) em até no máximo 1 Km do empreendimento?"),
        new("q8_08", "Q8_08", "8.8", "Possui acesso de equipamentos de uso eventual ( supermercado, agência bancária, lotéricas e correios) em até no máximo 1,5 Km do empreendimento?"),
        new("q8_09", "Q8_09", "8.9", "Possui Coleta de Materiais Recicláveis?"),
        new("q8_10", "Q8_10", "8.10", "Possui Serviço de Limpeza Urbana?"),
        new("q8_11", "Q8_11", "8.11", "Possui Asfaltamento?"),
        new("q8_12", "Q8_12", "8.12", "Possui Iluminação Pública?"),
        new("q8_13", "Q8_13", "8.13", "Possui Posto policial?")
    ];

    public static bool EstruturaValida(string? codigo) => Estruturas.Any(e => e.Codigo == codigo);
    public static bool ProgramaValido(string? codigo) => Programas.Any(p => p.Codigo == codigo);
}
