using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AppSkinCare.Core.Models;

namespace AppSkinCare.Core;

/// <summary>
/// La base de ingredientes: carga los JSON incrustados en el ensamblado y permite
/// reconocer un ingrediente a partir de lo que pone en la etiqueta.
/// </summary>
public sealed class CatalogoIngredientes
{
    private const string PrefijoRecurso = "AppSkinCare.Core.Data.ingredientes.";

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly Dictionary<string, Ingrediente> _porAlias = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Ingrediente> _porId = new(StringComparer.Ordinal);
    private readonly List<(Regex Patron, Ingrediente Ingrediente)> _patrones = [];

    public IReadOnlyList<Ingrediente> Todos { get; }

    public CatalogoIngredientes(IEnumerable<Ingrediente> ingredientes)
    {
        Todos = ingredientes.ToList();

        foreach (var ing in Todos)
        {
            if (!_porId.TryAdd(ing.Id, ing))
                throw new InvalidOperationException($"Id de ingrediente repetido: {ing.Id}");

            foreach (var nombre in ing.Inci.Append(ing.Nombre))
            {
                var clave = Normalizador.Normalizar(nombre);
                if (clave.Length == 0) continue;

                if (_porAlias.TryGetValue(clave, out var otro) && otro.Id != ing.Id)
                    throw new InvalidOperationException(
                        $"El nombre «{nombre}» está en dos ingredientes: {otro.Id} y {ing.Id}");

                _porAlias[clave] = ing;
            }

            foreach (var patron in ing.Patrones)
                _patrones.Add((new Regex(patron, RegexOptions.CultureInvariant | RegexOptions.Compiled,
                    TimeSpan.FromMilliseconds(100)), ing));
        }
    }

    public static CatalogoIngredientes Cargar()
    {
        var ensamblado = typeof(CatalogoIngredientes).Assembly;
        var todos = new List<Ingrediente>();

        var recursos = ensamblado.GetManifestResourceNames()
            .Where(n => n.StartsWith(PrefijoRecurso, StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal);

        foreach (var recurso in recursos)
        {
            using var flujo = ensamblado.GetManifestResourceStream(recurso)
                              ?? throw new InvalidOperationException($"No se pudo abrir {recurso}");
            var lista = JsonSerializer.Deserialize<List<Ingrediente>>(flujo, OpcionesJson)
                        ?? throw new InvalidOperationException($"{recurso} está vacío");
            todos.AddRange(lista);
        }

        if (todos.Count == 0)
            throw new InvalidOperationException("No se encontró ningún archivo de ingredientes.");

        return new CatalogoIngredientes(todos);
    }

    public Ingrediente? PorId(string id) => _porId.GetValueOrDefault(id);

    /// <summary>Reconoce un ingrediente de la etiqueta. Solo coincidencias exactas (normalizadas) o por patrón.</summary>
    public Ingrediente? Buscar(ItemInci item)
    {
        foreach (var candidato in item.Candidatos)
            if (_porAlias.TryGetValue(candidato, out var ing))
                return ing;

        foreach (var candidato in item.Candidatos)
            foreach (var (patron, ing) in _patrones)
                if (patron.IsMatch(candidato))
                    return ing;

        return null;
    }

    /// <summary>Búsqueda para el buscador de la web: por trozo de nombre o de INCI.</summary>
    public IReadOnlyList<Ingrediente> Buscar(string? consulta, string? categoria = null, int max = 60)
    {
        var q = Normalizador.Normalizar(consulta);

        return Todos
            .Where(i => categoria is null || i.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
            .Where(i => q.Length == 0
                        || Normalizador.Normalizar(i.Nombre).Contains(q, StringComparison.Ordinal)
                        || i.Inci.Any(n => Normalizador.Normalizar(n).Contains(q, StringComparison.Ordinal)))
            .OrderBy(i => i.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Take(max)
            .ToList();
    }

    public IReadOnlyList<string> Categorias() =>
        Todos.Select(i => i.Categoria).Distinct().Order(StringComparer.Ordinal).ToList();

    /// <summary>
    /// Si el texto parece una errata de un ingrediente conocido, devuelve su nombre.
    /// Es solo una pista para la persona: el ingrediente sigue contando como desconocido.
    /// </summary>
    public string? Sugerir(ItemInci item)
    {
        var texto = item.Candidatos[0];
        if (texto.Length < 5) return null;

        var maxDistancia = texto.Length < 8 ? 1 : 2;
        var mejorDistancia = int.MaxValue;
        Ingrediente? mejor = null;

        foreach (var (clave, ing) in _porAlias)
        {
            if (Math.Abs(clave.Length - texto.Length) > maxDistancia) continue;
            var d = Distancia(texto, clave, maxDistancia);
            if (d <= maxDistancia && d < mejorDistancia)
            {
                mejorDistancia = d;
                mejor = ing;
            }
        }

        return mejor?.Nombre;
    }

    /// <summary>Distancia de Levenshtein con corte temprano.</summary>
    internal static int Distancia(string a, string b, int corte)
    {
        var previa = new int[b.Length + 1];
        var actual = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) previa[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            actual[0] = i;
            var minimoFila = actual[0];

            for (var j = 1; j <= b.Length; j++)
            {
                var coste = a[i - 1] == b[j - 1] ? 0 : 1;
                actual[j] = Math.Min(Math.Min(actual[j - 1] + 1, previa[j] + 1), previa[j - 1] + coste);
                if (actual[j] < minimoFila) minimoFila = actual[j];
            }

            if (minimoFila > corte) return corte + 1;
            (previa, actual) = (actual, previa);
        }

        return previa[b.Length];
    }
}
