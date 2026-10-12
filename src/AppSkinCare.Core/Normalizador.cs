using System.Globalization;
using System.Text;

namespace AppSkinCare.Core;

/// <summary>
/// Deja los nombres de ingredientes en una forma comparable:
/// minúsculas, sin tildes y con cualquier signo convertido en un único espacio.
/// "Alcohol Denat." y "alcohol denat" acaban siendo lo mismo; "PEG-100 Stearate" → "peg 100 stearate".
/// </summary>
/// <remarks>
/// Las tildes se quitan con una tabla propia y no con <c>string.Normalize</c>: con
/// <c>InvariantGlobalization</c> activado (lo habitual en Azure y en contenedores) Normalize no descompone
/// los caracteres y la búsqueda de «peróxido» dejaría de funcionar.
/// </remarks>
public static class Normalizador
{
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        var sb = new StringBuilder(texto.Length);
        var ultimoEspacio = true;

        foreach (var original in texto)
        {
            // Marcas combinadas (por si el texto llega ya descompuesto): se ignoran.
            if (CharUnicodeInfo.GetUnicodeCategory(original) == UnicodeCategory.NonSpacingMark) continue;

            var c = QuitarTilde(char.ToLowerInvariant(original));

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                ultimoEspacio = false;
            }
            else if (!ultimoEspacio)
            {
                sb.Append(' ');
                ultimoEspacio = true;
            }
        }

        return sb.ToString().Trim();
    }

    private static char QuitarTilde(char c) => c switch
    {
        'á' or 'à' or 'ä' or 'â' or 'ã' or 'å' or 'ā' => 'a',
        'é' or 'è' or 'ë' or 'ê' or 'ē' => 'e',
        'í' or 'ì' or 'ï' or 'î' or 'ī' => 'i',
        'ó' or 'ò' or 'ö' or 'ô' or 'õ' or 'ō' => 'o',
        'ú' or 'ù' or 'ü' or 'û' or 'ū' => 'u',
        'ý' or 'ÿ' => 'y',
        'ñ' => 'n',
        'ç' => 'c',
        _ => c
    };
}
