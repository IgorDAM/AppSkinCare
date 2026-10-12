using AppSkinCare.Core.Models;

namespace AppSkinCare.Core;

/// <summary>
/// Genera una rutina mínima según el perfil. La base es siempre la misma que recomiendan la AAD y la AEDV:
/// limpiar suave, hidratar sin perfume y protegerse del sol. Lo demás solo se añade si hay un motivo.
/// </summary>
public sealed class GeneradorRutina
{
    public const string TextoAvisoLegal =
        "Esto es información educativa y no sustituye a un médico/a o dermatólogo/a. Si dudas, pregunta.";

    public Rutina Generar(PerfilRutina perfil)
    {
        ArgumentNullException.ThrowIfNull(perfil);
        var error = perfil.Validar();
        if (error is not null) throw new ArgumentException(error, nameof(perfil));

        var tratamientoPosible = !perfil.Isotretinoina && perfil.Acne == IntensidadAcne.Leve;
        var derivarPorAcne = perfil.Acne is IntensidadAcne.Moderado or IntensidadAcne.Intenso;

        var manana = new List<PasoRutina>
        {
            Limpiar(perfil),
            Hidratar(perfil),
            ProtectorSolar(perfil)
        };

        var noche = new List<PasoRutina> { Limpiar(perfil, esNoche: true) };

        if (tratamientoPosible)
            noche.Add(TratamientoLeve(perfil));
        else if (derivarPorAcne)
            noche.Add(PasoMedico());
        else if (perfil.Manchas && !perfil.Isotretinoina)
            noche.Add(PasoMarcas(perfil));

        noche.Add(Hidratar(perfil));

        return new Rutina
        {
            Titulo = "Tu rutina mínima",
            Resumen = Resumen(perfil, tratamientoPosible, derivarPorAcne),
            Manana = manana,
            Noche = noche,
            Evita = Evita(perfil),
            Consejos = Consejos(perfil),
            MedicoSi = MedicoSi(),
            AvisoLegal = TextoAvisoLegal
        };
    }

    private static string Resumen(PerfilRutina p, bool tratamiento, bool derivar)
    {
        if (p.Isotretinoina)
            return "Con isotretinoína (pastillas) tu piel está muy sensible. Lo único que necesitas es un hidratante, un bálsamo labial y protector solar. Ningún ácido, retinoide ni exfoliante, y cualquier duda con tu médico/a.";
        if (derivar)
            return "Con tus granos, los cosméticos solos no bastan: un dermatólogo/a puede recetarte tratamientos que sí funcionan. Mientras tanto, una rutina de tres pasos sin irritar.";
        if (tratamiento)
            return "Tres pasos de base y un solo tratamiento para los granos. Más productos no significan mejor piel: significan más riesgo de irritarla.";
        return "Tres pasos y ya está: limpiar suave, hidratar y proteger del sol. Es lo que recomiendan los dermatólogos y es suficiente para casi todas las pieles de tu edad.";
    }

    private static PasoRutina Limpiar(PerfilRutina p, bool esNoche = false)
    {
        var que = esNoche
            ? "Quita el protector solar, el sudor y el maquillaje. Con agua tibia y un limpiador suave, sin frotar."
            : "Con agua tibia y, si lo necesitas, un limpiador suave. Sin frotar fuerte.";

        var buscar = new List<string>
        {
            "Limpiador suave en gel o espuma (tensioactivos suaves, tipo glucósidos o isetionatos)",
            "Sin perfume"
        };

        var evitar = new List<string>
        {
            "Jabón de pastilla clásico: reseca",
            "Sulfatos fuertes (SLS) si tu piel es sensible o tiene eccema",
            "Cepillos o esponjas que raspen"
        };

        if (p.Atopia)
            evitar.Add("Duchas largas o muy calientes: mejor cortas (5-10 minutos) y con agua tibia");

        return new PasoRutina
        {
            Titulo = "Limpiar",
            Que = que,
            Buscar = buscar,
            Evitar = evitar
        };
    }

    private static PasoRutina Hidratar(PerfilRutina p)
    {
        var que = p.TipoPiel switch
        {
            TipoPiel.Seca => "Una crema más rica, para sellar la hidratación.",
            TipoPiel.Grasa => "Un gel-crema o fluido ligero, que no sea comedogénico (que no tape los poros).",
            TipoPiel.Mixta => "Una textura ligera, que no sea comedogénica (que no tape los poros).",
            _ => "Una crema sencilla, sin perfume."
        };

        if (p.Atopia)
            que = "Crema hidratante sin perfume cada día, nada más salir de la ducha, aunque no haya brote.";

        var buscar = new List<string> { "Sin perfume ni aceites esenciales" };

        if (p.TipoPiel == TipoPiel.Seca || p.Atopia)
            buscar.Add("Glicerina, ceramidas, vaselina, manteca de karité, avena coloidal");
        else
            buscar.Add("Glicerina, ceramidas, ácido hialurónico, pantenol");

        if (p.TipoPiel is TipoPiel.Grasa or TipoPiel.Mixta || p.Acne != IntensidadAcne.No)
            buscar.Add("Que ponga «no comedogénico» o textura en gel/fluido");

        var evitar = new List<string>
        {
            "Perfume y aceites esenciales",
            "Alcohol desnaturalizado entre los primeros ingredientes"
        };

        if (p.Atopia)
        {
            evitar.Add("Aceite de oliva como hidratante habitual (el ácido oleico puede alterar la barrera)");
            evitar.Add("Lanolina, propóleo, caléndula, árnica: sensibilizan con frecuencia");
        }

        return new PasoRutina
        {
            Titulo = "Hidratar",
            Que = que,
            Buscar = buscar,
            Evitar = evitar
        };
    }

    private static PasoRutina ProtectorSolar(PerfilRutina p)
    {
        var spf = p.PielClara || p.MuchoSol ? "SPF 50" : "SPF 30 o más";

        var buscar = new List<string>
        {
            $"{spf}, de amplio espectro (protege de UVA y UVB)",
            "Cara, cuello y orejas, cada mañana"
        };

        if (p.Sensible || p.Atopia)
            buscar.Add("Filtros minerales (óxido de zinc, dióxido de titanio): los mejor tolerados");

        if (p.TipoPiel is TipoPiel.Grasa or TipoPiel.Mixta || p.Acne != IntensidadAcne.No)
            buscar.Add("Textura en fluido o gel, que no sea pesada");

        return new PasoRutina
        {
            Titulo = "Protector solar",
            Que = "Es el paso que más cuida tu piel a largo plazo. Si estás al aire libre, vuelve a ponértelo cada 2 horas y después de bañarte.",
            Buscar = buscar,
            Evitar =
            [
                "Salir sin él «porque está nublado»",
                "Oxibenzona (benzofenona-3) si tienes otra opción",
                "Aerosol de filtros minerales con nanopartículas (no conviene respirarlos)"
            ]
        };
    }

    private static PasoRutina TratamientoLeve(PerfilRutina p) => new()
    {
        Titulo = "Tratamiento para los granos",
        Que = p.Manchas
            ? "Uno solo, no varios. Con marcas después de los granos, el ácido azelaico ayuda a los dos problemas a la vez. Como alternativa, el peróxido de benzoílo."
            : "Uno solo, no varios. El mejor respaldado por las guías de acné es el peróxido de benzoílo en las zonas con granos. El ácido azelaico es otra opción.",
        Buscar =
        [
            "Peróxido de benzoílo al 2,5 % o 5 % (empieza un día sí y otro no)",
            "o ácido azelaico (hasta el 10 % en cosmética)",
            "Si crees que necesitas un retinoide como el adapaleno, es un medicamento: pregúntalo a tu médico/a"
        ],
        Evitar =
        [
            "Mezclarlo con otros ácidos o retinoides",
            "Repetir el mismo activo en varios productos",
            "Subir de concentración «para ir más rápido»",
            "Apretar los granos (deja marcas y cicatrices)"
        ]
    };

    private static PasoRutina PasoMedico() => new()
    {
        Titulo = "Ve al médico/a",
        Que = "Con granos inflamados, grandes, dolorosos o que dejan cicatriz, lo mejor es pedir cita con tu médico/a o un dermatólogo/a. Cuanto antes se trata, menos marcas dejan.",
        Buscar = ["Una cita con tu médico/a de familia o con dermatología"],
        Evitar = ["Probar tratamientos de redes sociales mientras tanto", "Apretar los granos"]
    };

    private static PasoRutina PasoMarcas(PerfilRutina p) => new()
    {
        Titulo = "Para las marcas",
        Opcional = true,
        Que = "Las marcas tras los granos o las irritaciones se van con tiempo, protegiéndote del sol cada día y sin irritar la piel. Si quieres ayudar, usa un solo producto.",
        Buscar =
        [
            "Ácido azelaico (hasta el 10 % en cosmética)",
            "o niacinamida al 2-5 %"
        ],
        Evitar =
        [
            "Cremas «blanqueantes» sin receta o de importación dudosa: pueden llevar hidroquinona o mercurio",
            "Exfoliar fuerte para «borrar» las marcas: las empeora"
        ]
    };

    private static IReadOnlyList<string> Evita(PerfilRutina p)
    {
        var lista = new List<string>
        {
            "Rutinas de más de tres o cuatro pasos y productos «multiactivo» de moda",
            "Repetir el mismo activo en varios productos (mira bien las etiquetas)",
            "Mezclar varios ácidos o retinoides",
            "Exfoliantes con partículas, frotar fuerte y apretarte los granos",
            "Remedios caseros: limón, bicarbonato, pasta de dientes",
            "Microagujas y peelings hechos en casa",
            "Bronceado intencional y cabinas de rayos UV",
            "Cremas despigmentantes sin receta o de importación dudosa"
        };

        if (p.Isotretinoina)
            lista.Insert(0, "Retinoides, ácidos (AHA/BHA), peróxido de benzoílo y exfoliantes mientras tomes isotretinoína");

        return lista;
    }

    private static IReadOnlyList<string> Consejos(PerfilRutina p)
    {
        var lista = new List<string>
        {
            "Menos es más: limpiar, hidratar y protegerte del sol cubre lo que tu piel necesita.",
            "Lo que se ve en redes no es una pauta médica. En un estudio de 2025 sobre vídeos de TikTok de chicas de 7 a 18 años, las rutinas más vistas mezclaban de media 11 activos que pueden irritar y solo 1 de cada 4 incluía protector solar."
        };

        if (p.Sensible || p.Atopia)
            lista.Add("Introduce un producto nuevo cada vez y pruébalo 7-10 días detrás de la oreja o en el antebrazo antes de usarlo en la cara. Para si escuece más de 15 minutos, enrojece o descama.");

        if (p.Acne == IntensidadAcne.Leve)
            lista.Add("Dale tiempo al tratamiento: no cambies de producto cada semana. Y no te aprietes los granos.");

        if (p.Manchas)
            lista.Add("Tratar el acné pronto y no manipular los granos es lo que más ayuda a que no queden marcas. El protector solar cada día evita que se oscurezcan.");

        if (p.Atopia)
            lista.Add("Duchas cortas (5-10 minutos) con agua tibia y la crema justo al salir.");

        return lista;
    }

    private static IReadOnlyList<string> MedicoSi() =>
    [
        "Tienes granos grandes, dolorosos o que dejan cicatriz.",
        "Un eccema que no mejora con la crema hidratante, o una piel que empeora cada vez que usas un cosmético.",
        "Un lunar o una mancha que cambia de forma, color o tamaño.",
        "Notas que se te cae el pelo o te salen calvas.",
        "Un producto te ha quemado la piel.",
        "Tu piel te preocupa mucho o te hace sentir mal contigo. No es exagerar: cuéntaselo a tu médico/a o a alguien de confianza."
    ];
}
