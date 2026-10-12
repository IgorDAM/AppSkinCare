namespace AppSkinCare.Core.Models;

public enum TipoPiel
{
    Normal,
    Seca,
    Grasa,
    Mixta
}

public enum Tramo
{
    /// <summary>13 y 14 años. Se usa la columna "10-14 años" del informe de referencia (la más prudente).</summary>
    De13a14,

    /// <summary>15 a 18 años. Columna "14-18 años" del informe.</summary>
    De15a18
}

/// <summary>Lo que la persona cuenta de sí misma. Nada de esto se guarda.</summary>
public sealed record Contexto
{
    public const int EdadMinima = 13;
    public const int EdadMaxima = 18;

    public int Edad { get; init; } = 15;
    public TipoPiel TipoPiel { get; init; } = TipoPiel.Normal;
    public bool Sensible { get; init; }

    /// <summary>Eccema / dermatitis atópica.</summary>
    public bool Atopia { get; init; }

    public bool Acne { get; init; }

    /// <summary>Le quedan marcas o manchas después de los granos o de las irritaciones.</summary>
    public bool Manchas { get; init; }

    public bool Isotretinoina { get; init; }

    /// <summary>El producto se aclara (limpiador, champú) en vez de dejarse puesto.</summary>
    public bool SeAclara { get; init; }

    public Tramo Tramo => Edad <= 14 ? Tramo.De13a14 : Tramo.De15a18;

    public string? Validar() =>
        Edad is < EdadMinima or > EdadMaxima
            ? $"Esta herramienta es para personas de {EdadMinima} a {EdadMaxima} años. Si tienes otra edad, pregunta a un pediatra o a un dermatólogo."
            : null;

    /// <summary>Etiquetas con las que se evalúan las reglas de los ingredientes.</summary>
    public HashSet<string> Etiquetas()
    {
        var e = new HashSet<string>(StringComparer.Ordinal);
        e.Add(Tramo == Tramo.De13a14 ? "t1" : "t2");
        if (Atopia) { e.Add("atopia"); e.Add("sensible"); }
        if (Sensible) e.Add("sensible");
        if (Acne) e.Add("acne");
        if (Manchas) e.Add("manchas");
        if (Isotretinoina) e.Add("isotretinoina");
        if (TipoPiel is TipoPiel.Grasa or TipoPiel.Mixta) e.Add("grasa");
        if (TipoPiel == TipoPiel.Seca) e.Add("seca");
        e.Add(SeAclara ? "seAclara" : "sinAclarado");
        return e;
    }
}
