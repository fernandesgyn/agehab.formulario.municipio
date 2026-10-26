using System.Globalization;
using System.Text.Json;

namespace MunicipioPortal.Models;

public sealed record Municipio(int CodigoIbge, string Nome);

public sealed class MunicipiosCatalog
{
    public IReadOnlyList<Municipio> Todos { get; }
    private readonly HashSet<int> _codigos;

    public MunicipiosCatalog(IHostEnvironment env)
    {
        string path = Path.Combine(env.ContentRootPath, "Data", "municipios.json");
        var content = File.ReadAllText(path);
        var municipios = JsonSerializer.Deserialize<List<Municipio>>(content)
            ?? throw new InvalidOperationException("Catálogo de municípios não carregado.");
        if (municipios.Count != 246 || municipios.Select(m => m.CodigoIbge).Distinct().Count() != 246)
            throw new InvalidOperationException("Catálogo municipal incompleto ou duplicado.");
        Todos = municipios.OrderBy(m => m.Nome, StringComparer.Create(
            CultureInfo.GetCultureInfo("pt-BR"), false)).ToArray();
        _codigos = Todos.Select(m => m.CodigoIbge).ToHashSet();
    }

    public bool Existe(int codigoIbge) => _codigos.Contains(codigoIbge);
}
