using System.Text.Json;
using AppSkinCare.Core;
using AppSkinCare.Core.Models;
using Xunit;

namespace AppSkinCare.Tests;

public class CatalogoTests
{
    private static readonly CatalogoIngredientes Catalogo = CatalogoIngredientes.Cargar();

    private static readonly HashSet<string> EtiquetasValidas = new()
    {
        "atopia", "sensible", "acne", "manchas", "grasa", "seca", "isotretinoina",
        "seAclara", "sinAclarado", "t1", "t2", "posAlta"
    };

    [Fact]
    public void Se_carga_la_base_completa()
    {
        Assert.True(Catalogo.Todos.Count >= 60);
    }

    [Fact]
    public void Todos_los_ingredientes_tienen_texto_para_la_persona()
    {
        foreach (var ing in Catalogo.Todos)
        {
            Assert.False(string.IsNullOrWhiteSpace(ing.Resumen), $"{ing.Id} sin resumen");
            Assert.False(string.IsNullOrWhiteSpace(ing.PorQue), $"{ing.Id} sin porQue");
        }
    }

    [Fact]
    public void Todo_ingrediente_se_puede_reconocer_por_algun_nombre_o_patron()
    {
        foreach (var ing in Catalogo.Todos)
            Assert.True(ing.Inci.Count > 0 || ing.Patrones.Count > 0, $"{ing.Id} no tiene nombres INCI ni patrones");
    }

    [Fact]
    public void Las_reglas_solo_usan_etiquetas_que_existen()
    {
        foreach (var ing in Catalogo.Todos)
            foreach (var regla in ing.Reglas)
            {
                Assert.NotEmpty(regla.Si);
                foreach (var etiqueta in regla.Si)
                    Assert.True(EtiquetasValidas.Contains(etiqueta), $"{ing.Id}: etiqueta desconocida «{etiqueta}»");
            }
    }

    [Fact]
    public void Los_irritantes_tienen_grupo()
    {
        foreach (var ing in Catalogo.Todos.Where(i => i.Irritante))
            Assert.False(string.IsNullOrEmpty(ing.Grupo), $"{ing.Id} es irritante pero no tiene grupo");
    }

    [Fact]
    public void Un_alias_repetido_entre_dos_ingredientes_es_un_error()
    {
        var a = Nuevo("a", "Niacinamide");
        var b = Nuevo("b", "niacinamide");

        Assert.Throws<InvalidOperationException>(() => new CatalogoIngredientes([a, b]));
    }

    [Fact]
    public void Los_patrones_reconocen_familias_enteras()
    {
        var item = InciParser.Parse("Lavandula Angustifolia (Lavender) Oil")[0];

        Assert.Equal("aceites-esenciales", Catalogo.Buscar(item)?.Id);
    }

    [Fact]
    public void Las_fuentes_citadas_existen_en_fuentes_json()
    {
        var ruta = BuscarArchivo("src/AppSkinCare.Web/wwwroot/data/fuentes.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
        var claves = doc.RootElement.EnumerateArray().Select(e => e.GetProperty("id").GetString()!).ToHashSet();

        foreach (var ing in Catalogo.Todos)
            foreach (var fuente in ing.Fuentes)
                Assert.True(claves.Contains(fuente), $"{ing.Id} cita una fuente que no existe: {fuente}");
    }

    [Fact]
    public void La_busqueda_encuentra_por_nombre_parcial_sin_tildes()
    {
        var resultado = Catalogo.Buscar("peroxido");

        Assert.Contains(resultado, i => i.Id == "peroxido-benzoilo");
    }

    private static Ingrediente Nuevo(string id, string inci) => new()
    {
        Id = id,
        Nombre = id,
        Inci = [inci],
        Resumen = "r",
        PorQue = "p",
        Nivel13a14 = Nivel.Ok,
        Nivel15a18 = Nivel.Ok
    };

    private static string BuscarArchivo(string relativa)
    {
        var raiz = Environment.GetEnvironmentVariable("APPSKINCARE_ROOT");
        var carpeta = raiz is not null ? new DirectoryInfo(raiz) : new DirectoryInfo(AppContext.BaseDirectory);

        while (carpeta is not null)
        {
            var candidata = Path.Combine(carpeta.FullName, relativa);
            if (File.Exists(candidata)) return candidata;
            carpeta = carpeta.Parent;
        }

        throw new FileNotFoundException($"No encuentro {relativa} subiendo desde {AppContext.BaseDirectory}");
    }
}
