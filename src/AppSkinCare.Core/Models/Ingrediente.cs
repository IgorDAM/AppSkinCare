namespace AppSkinCare.Core.Models;

/// <summary>
/// Una regla que solo puede EMPEORAR la valoración base del ingrediente
/// cuando se cumplen todas las condiciones de <see cref="Si"/>.
/// Condiciones posibles: atopia, sensible, acne, manchas, grasa, seca, isotretinoina,
/// seAclara, sinAclarado, t1 (13-14 años), t2 (15-18 años), posAlta (ingrediente entre los primeros).
/// </summary>
public sealed class Regla
{
    public required IReadOnlyList<string> Si { get; init; }
    public required Nivel Nivel { get; init; }
    public required string PorQue { get; init; }
}

/// <summary>Si el producto indica un porcentaje mayor que <see cref="MasDe"/>, la valoración empeora.</summary>
public sealed class Limite
{
    public required double MasDe { get; init; }
    public required Nivel Nivel { get; init; }
    public required string PorQue { get; init; }

    /// <summary>"t1" (13-14), "t2" (15-18) o null para ambos.</summary>
    public string? Tramo { get; init; }
}

public sealed class Ingrediente
{
    public required string Id { get; init; }
    public required string Nombre { get; init; }

    /// <summary>Nombres INCI (y alias habituales) con los que aparece en las etiquetas.</summary>
    public IReadOnlyList<string> Inci { get; init; } = [];

    /// <summary>Expresiones regulares (sobre el texto normalizado) para familias grandes, como los aceites esenciales.</summary>
    public IReadOnlyList<string> Patrones { get; init; } = [];

    public string Categoria { get; init; } = "otro";

    /// <summary>Qué es, en dos frases y sin tecnicismos.</summary>
    public required string Resumen { get; init; }

    /// <summary>Por qué recibe esa valoración a la edad de la persona.</summary>
    public required string PorQue { get; init; }

    public string? PorQue13a14 { get; init; }
    public string? PorQue15a18 { get; init; }

    /// <summary>Información técnica para quien quiera saber más.</summary>
    public string? Detalle { get; init; }

    public required Nivel Nivel13a14 { get; init; }
    public required Nivel Nivel15a18 { get; init; }

    /// <summary>Activo fuerte que puede irritar: cuenta para detectar mezclas y duplicados.</summary>
    public bool Irritante { get; init; }

    /// <summary>Familia del activo (retinoide, aha, bha, peroxido...). Dos irritantes de grupos distintos = mezcla.</summary>
    public string? Grupo { get; init; }

    public bool Medicamento { get; init; }
    public bool ProhibidoUe { get; init; }

    /// <summary>Si aparece en una posición igual o menor, se activa la condición "posAlta".</summary>
    public int? PosAltaHasta { get; init; }

    /// <summary>Condiciones (acne, manchas, grasa, seca, atopia) en las que este ingrediente puede ayudar.</summary>
    public IReadOnlyList<string> Ayuda { get; init; } = [];

    public IReadOnlyList<Regla> Reglas { get; init; } = [];
    public IReadOnlyList<Limite> Limites { get; init; } = [];

    /// <summary>Qué decirle a la persona cuando el producto no indica cuánto lleva.</summary>
    public string? NotaConcentracion { get; init; }

    public IReadOnlyList<string> Fuentes { get; init; } = [];

    public Nivel NivelPara(Tramo tramo) => tramo == Tramo.De13a14 ? Nivel13a14 : Nivel15a18;

    public string PorQuePara(Tramo tramo) =>
        (tramo == Tramo.De13a14 ? PorQue13a14 : PorQue15a18) ?? PorQue;
}
