namespace AppSkinCare.Core.Models;

/// <summary>
/// Cómo se valora un ingrediente para una persona concreta.
/// Neutro = ingrediente de formulación habitual, sin reglas especiales.
/// Ok = sin problema (o incluso útil). Precaucion = ojo, depende. Evitar = mejor no.
/// </summary>
public enum Nivel
{
    Neutro,
    Ok,
    Precaucion,
    Evitar
}

public static class NivelExtensions
{
    /// <summary>0 = sin alerta, 1 = precaución, 2 = evitar.</summary>
    public static int Gravedad(this Nivel nivel) => nivel switch
    {
        Nivel.Precaucion => 1,
        Nivel.Evitar => 2,
        _ => 0
    };

    public static Nivel Peor(this Nivel a, Nivel b) => b.Gravedad() > a.Gravedad() ? b : a;
}
