namespace AppSkinCare.Core.Models;

public sealed record ProductoInci
{
    public string? Nombre { get; init; }
    public string Inci { get; init; } = "";
}

public sealed record PeticionEvaluacion
{
    public const int MaxCaracteres = 6000;
    public const int MaxProductosRutina = 5;

    public ProductoInci Producto { get; init; } = new();
    public IReadOnlyList<ProductoInci> Rutina { get; init; } = [];
    public Contexto Contexto { get; init; } = new();

    public string? Validar()
    {
        if (string.IsNullOrWhiteSpace(Producto?.Inci))
            return "Pega la lista de ingredientes del producto (suele estar en la parte de atrás del envase, después de la palabra «Ingredients»).";
        if (Producto.Inci.Length > MaxCaracteres)
            return $"La lista de ingredientes es demasiado larga (máximo {MaxCaracteres} caracteres).";
        if (Rutina is { Count: > MaxProductosRutina })
            return $"Puedes comparar con un máximo de {MaxProductosRutina} productos de tu rutina.";
        if (Rutina.Any(r => r.Inci is { Length: > MaxCaracteres }))
            return $"Alguna lista de tu rutina es demasiado larga (máximo {MaxCaracteres} caracteres).";
        return Contexto?.Validar();
    }
}

public enum Veredicto
{
    /// <summary>No veo ninguna alerta y conozco todos los ingredientes.</summary>
    Buena,

    /// <summary>No veo alertas en lo que conozco, pero hay ingredientes que no sé valorar.</summary>
    Parcial,

    Precaucion,
    Evitar,

    /// <summary>No conozco suficientes ingredientes para opinar.</summary>
    SinDatos
}

public sealed record ItemEvaluado
{
    public required string Texto { get; init; }
    public required int Posicion { get; init; }
    public required string IngredienteId { get; init; }
    public required string Nombre { get; init; }
    public required string Categoria { get; init; }
    public required Nivel Nivel { get; init; }
    public required string PorQue { get; init; }
    public required string Resumen { get; init; }
    public string? Detalle { get; init; }
    public double? Porcentaje { get; init; }
    public IReadOnlyList<string> AyudaCon { get; init; } = [];
    public IReadOnlyList<string> Fuentes { get; init; } = [];
}

public sealed record Desconocido
{
    public required string Texto { get; init; }
    public required int Posicion { get; init; }

    /// <summary>Si parece una errata de un ingrediente conocido. Es solo una pista, no se da por válido.</summary>
    public string? Sugerencia { get; init; }
}

/// <summary>Una alerta que no es de un ingrediente concreto: mezclas, duplicados con otros productos...</summary>
public sealed record Alerta
{
    public required Nivel Nivel { get; init; }
    public required string Texto { get; init; }
}

public sealed record ResultadoEvaluacion
{
    public required Veredicto Veredicto { get; init; }
    public required string Titulo { get; init; }
    public required string Resumen { get; init; }
    public required IReadOnlyList<ItemEvaluado> Ingredientes { get; init; }
    public required IReadOnlyList<Desconocido> Desconocidos { get; init; }
    public required IReadOnlyList<Alerta> Alertas { get; init; }
    public required IReadOnlyList<string> Avisos { get; init; }

    /// <summary>Mensaje claro sobre los ingredientes que no conocemos. Vacío si los conocemos todos.</summary>
    public string? MensajeDesconocidos { get; init; }

    public required int Total { get; init; }
    public required int Reconocidos { get; init; }
    public required string AvisoLegal { get; init; }
}
