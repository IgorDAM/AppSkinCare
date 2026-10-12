namespace AppSkinCare.Core.Models;

public enum IntensidadAcne
{
    No,

    /// <summary>Algunos granitos o puntos negros.</summary>
    Leve,

    /// <summary>Muchos granos, algunos inflamados.</summary>
    Moderado,

    /// <summary>Granos grandes, dolorosos, o que dejan cicatriz.</summary>
    Intenso
}

public sealed record PerfilRutina
{
    public int Edad { get; init; } = 15;
    public TipoPiel TipoPiel { get; init; } = TipoPiel.Normal;
    public bool Sensible { get; init; }
    public bool Atopia { get; init; }
    public IntensidadAcne Acne { get; init; } = IntensidadAcne.No;
    public bool Manchas { get; init; }
    public bool Isotretinoina { get; init; }

    /// <summary>Se quema con facilidad con el sol.</summary>
    public bool PielClara { get; init; }

    /// <summary>Pasa mucho tiempo al aire libre (deporte, playa, piscina...).</summary>
    public bool MuchoSol { get; init; }

    public string? Validar() =>
        Edad is < Contexto.EdadMinima or > Contexto.EdadMaxima
            ? $"Esta herramienta es para personas de {Contexto.EdadMinima} a {Contexto.EdadMaxima} años. Si tienes otra edad, pregunta a un pediatra o a un dermatólogo."
            : null;
}

public sealed record PasoRutina
{
    public required string Titulo { get; init; }
    public required string Que { get; init; }
    public IReadOnlyList<string> Buscar { get; init; } = [];
    public IReadOnlyList<string> Evitar { get; init; } = [];
    public bool Opcional { get; init; }
}

public sealed record Rutina
{
    public required string Titulo { get; init; }
    public required string Resumen { get; init; }
    public required IReadOnlyList<PasoRutina> Manana { get; init; }
    public required IReadOnlyList<PasoRutina> Noche { get; init; }
    public required IReadOnlyList<string> Evita { get; init; }
    public required IReadOnlyList<string> Consejos { get; init; }
    public required IReadOnlyList<string> MedicoSi { get; init; }
    public required string AvisoLegal { get; init; }
}
