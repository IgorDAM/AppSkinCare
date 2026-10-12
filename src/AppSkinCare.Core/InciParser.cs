using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AppSkinCare.Core;

/// <summary>Un ingrediente tal y como aparece escrito en la etiqueta.</summary>
/// <param name="Original">Texto original (recortado).</param>
/// <param name="Candidatos">Nombres normalizados con los que intentar reconocerlo, del más completo al más parcial.</param>
/// <param name="Porcentaje">Porcentaje si la etiqueta lo indica (p. ej. "Salicylic Acid 2%").</param>
/// <param name="Posicion">Posición en la lista (1 = primero). En una etiqueta, el orden va de más a menos cantidad.</param>
public sealed record ItemInci(string Original, IReadOnlyList<string> Candidatos, double? Porcentaje, int Posicion);

public static partial class InciParser
{
    private const int MaxLongitudItem = 150;

    [GeneratedRegex(@"^\s*(ingredients?|ingredientes|inci|composition|composici[oó]n)\s*[:：]\s*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PrefijoRegex();

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*%", RegexOptions.CultureInvariant)]
    private static partial Regex PorcentajeRegex();

    [GeneratedRegex(@"[\(\[]([^()\[\]]*)[\)\]]", RegexOptions.CultureInvariant)]
    private static partial Regex ParentesisRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex EspaciosRegex();

    public static IReadOnlyList<ItemInci> Parse(string? texto, int max = 250)
    {
        if (string.IsNullOrWhiteSpace(texto)) return [];

        texto = PrefijoRegex().Replace(texto, "");
        var resultado = new List<ItemInci>();

        foreach (var trozo in Dividir(texto))
        {
            if (resultado.Count >= max) break;
            var item = CrearItem(trozo, resultado.Count + 1);
            if (item is not null) resultado.Add(item);
        }

        return resultado;
    }

    /// <summary>Divide por comas, puntos y comas, saltos de línea o viñetas, sin partir lo que está entre paréntesis.</summary>
    private static IEnumerable<string> Dividir(string texto)
    {
        var sb = new StringBuilder();
        var profundidad = 0;

        foreach (var c in texto)
        {
            if (c is '(' or '[') profundidad++;
            else if (c is ')' or ']' && profundidad > 0) profundidad--;

            var esSeparador = c is '\n' or '\r' || (profundidad == 0 && c is ',' or ';' or '•' or '·' or '|' or '●');

            if (esSeparador)
            {
                if (c is '\n' or '\r') profundidad = 0;
                if (sb.Length > 0) yield return sb.ToString();
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0) yield return sb.ToString();
    }

    private static ItemInci? CrearItem(string trozo, int posicion)
    {
        var original = trozo.Trim();
        var t = original.Trim('.', ' ', '*', '†', '°', ' ', '\t');
        if (t.Length == 0) return null;

        double? porcentaje = null;
        var m = PorcentajeRegex().Match(t);
        if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var valor))
        {
            porcentaje = valor;
            t = PorcentajeRegex().Replace(t, " ");
        }

        t = Regex.Replace(t, @"\(\s*\)|\[\s*\]", " ");
        t = EspaciosRegex().Replace(t, " ").Trim();
        if (t.Length == 0) return null;
        if (t.Length > MaxLongitudItem) t = t[..MaxLongitudItem];

        var candidatos = new List<string>();

        void Anadir(string s)
        {
            var n = Normalizador.Normalizar(s);
            if (n.Length > 0 && !candidatos.Contains(n)) candidatos.Add(n);
        }

        Anadir(t);

        var fuera = ParentesisRegex().Replace(t, " ");
        Anadir(fuera);

        foreach (Match g in ParentesisRegex().Matches(t))
            foreach (var parte in g.Groups[1].Value.Split(',', '/'))
                Anadir(parte);

        if (fuera.Contains('/'))
            foreach (var parte in fuera.Split('/'))
                Anadir(parte);

        return candidatos.Count == 0 ? null : new ItemInci(original, candidatos, porcentaje, posicion);
    }
}
