using AppSkinCare.Core.Models;

namespace AppSkinCare.Web;

/// <summary>Lo que la web necesita saber de un ingrediente (sin las reglas internas del motor).</summary>
public sealed record IngredienteDto(
    string Id,
    string Nombre,
    string Categoria,
    string Resumen,
    IReadOnlyList<string> Inci,
    Nivel Nivel13a14,
    Nivel Nivel15a18,
    string PorQue13a14,
    string PorQue15a18,
    string? Detalle,
    bool Medicamento,
    bool ProhibidoUe,
    IReadOnlyList<string> Fuentes)
{
    public static IngredienteDto De(Ingrediente i) => new(
        i.Id, i.Nombre, i.Categoria, i.Resumen, i.Inci,
        i.Nivel13a14, i.Nivel15a18,
        i.PorQuePara(Tramo.De13a14), i.PorQuePara(Tramo.De15a18),
        i.Detalle, i.Medicamento, i.ProhibidoUe, i.Fuentes);
}
