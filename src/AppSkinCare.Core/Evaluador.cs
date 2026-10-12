using AppSkinCare.Core.Models;

namespace AppSkinCare.Core;

/// <summary>
/// Valora un producto a partir de su lista de ingredientes (INCI) y de lo que la persona cuenta de sí misma.
/// Reglas principales:
///  - Lo que no conoce lo dice claro y nunca lo da por bueno.
///  - Un ingrediente solo puede empeorar su valoración base según el contexto, nunca mejorar.
///  - Varios activos fuertes a la vez, o el mismo activo repetido en otro producto de la rutina, es lo que más irrita.
/// </summary>
public sealed class Evaluador(CatalogoIngredientes catalogo)
{
    private const int MaxIngredientes = 250;

    public const string TextoAvisoLegal =
        "Esto es información educativa: no es un diagnóstico y no sustituye a un médico/a ni a un dermatólogo/a. " +
        "Solo miro la lista de ingredientes: no sé cuánto lleva de cada uno (salvo que lo diga la etiqueta) " +
        "ni cómo reaccionará tu piel concreta.";

    private static readonly Dictionary<string, string> NombresGrupo = new(StringComparer.Ordinal)
    {
        ["retinoide"] = "un retinoide",
        ["aha"] = "un ácido AHA (glicólico, láctico…)",
        ["bha"] = "ácido salicílico",
        ["peroxido"] = "peróxido de benzoílo",
        ["vitamina-c"] = "vitamina C en forma de ácido ascórbico",
        ["exfoliante-fisico"] = "un exfoliante con partículas"
    };

    private static readonly HashSet<string> GruposQueSensibilizanAlSol = new(StringComparer.Ordinal)
    {
        "retinoide", "aha", "bha", "vitamina-c"
    };

    private static readonly Dictionary<string, string> AyudaTexto = new(StringComparer.Ordinal)
    {
        ["acne"] = "los granos",
        ["manchas"] = "las marcas y manchas",
        ["grasa"] = "el brillo y la grasa",
        ["seca"] = "la piel seca",
        ["atopia"] = "la piel con eccema"
    };

    public ResultadoEvaluacion Evaluar(PeticionEvaluacion peticion)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        var error = peticion.Validar();
        if (error is not null) throw new ArgumentException(error, nameof(peticion));

        var contexto = peticion.Contexto;
        var etiquetas = contexto.Etiquetas();
        var items = InciParser.Parse(peticion.Producto.Inci, MaxIngredientes);

        if (items.Count == 0)
            throw new ArgumentException("No he encontrado ingredientes en lo que has pegado. Prueba a copiar la lista completa, separada por comas.");

        var evaluados = new List<ItemEvaluado>();
        var desconocidos = new List<Desconocido>();
        var gruposIrritantes = new Dictionary<string, string>(StringComparer.Ordinal); // grupo -> nombre mostrado
        var hayMedicamento = false;
        var avisos = new List<string>();
        var ingredientesSinConcentracion = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            var ing = catalogo.Buscar(item);
            if (ing is null)
            {
                desconocidos.Add(new Desconocido
                {
                    Texto = item.Original,
                    Posicion = item.Posicion,
                    Sugerencia = catalogo.Sugerir(item)
                });
                continue;
            }

            evaluados.Add(EvaluarIngrediente(ing, item, contexto, etiquetas));

            if (ing.Irritante && ing.Grupo is not null)
                gruposIrritantes.TryAdd(ing.Grupo, ing.Nombre);
            if (ing.Medicamento) hayMedicamento = true;

            if (ing.NotaConcentracion is not null && item.Porcentaje is null &&
                ingredientesSinConcentracion.Add(ing.Id))
                avisos.Add($"No sé cuánto «{ing.Nombre}» lleva. {ing.NotaConcentracion}");
        }

        var alertas = new List<Alerta>();
        AnadirAlertasDeMezcla(gruposIrritantes, hayMedicamento, alertas);
        AnadirAlertasDeRutina(peticion, gruposIrritantes, alertas);

        if (gruposIrritantes.Keys.Any(GruposQueSensibilizanAlSol.Contains))
            avisos.Add("Con este tipo de activo, usa protector solar SPF 30 o más cada mañana (SPF 50 si tu piel se quema con facilidad).");

        if (gruposIrritantes.Count > 0)
            avisos.Add("Empieza poco a poco: 2-3 veces por semana, y pruébalo antes 7-10 días en una zona pequeña (detrás de la oreja o en el antebrazo). Si te escuece más de 15 minutos, se pone roja o se descama, para y aclárate con agua.");

        var reconocidos = evaluados.Count;
        var veredicto = CalcularVeredicto(evaluados, alertas, desconocidos.Count, reconocidos);
        var (titulo, resumen) = Redactar(veredicto, evaluados, alertas, desconocidos.Count);

        return new ResultadoEvaluacion
        {
            Veredicto = veredicto,
            Titulo = titulo,
            Resumen = resumen,
            Ingredientes = evaluados,
            Desconocidos = desconocidos,
            Alertas = alertas,
            Avisos = avisos,
            MensajeDesconocidos = desconocidos.Count == 0 ? null : MensajeDesconocidos(desconocidos.Count),
            Total = items.Count,
            Reconocidos = reconocidos,
            AvisoLegal = TextoAvisoLegal
        };
    }

    private static ItemEvaluado EvaluarIngrediente(Ingrediente ing, ItemInci item, Contexto contexto, HashSet<string> etiquetasBase)
    {
        var tramo = contexto.Tramo;
        var nivel = ing.NivelPara(tramo);
        var porQue = ing.PorQuePara(tramo);

        var etiquetas = new HashSet<string>(etiquetasBase, StringComparer.Ordinal);
        if (ing.PosAltaHasta is { } hasta && item.Posicion <= hasta) etiquetas.Add("posAlta");

        void Empeorar(Nivel nuevo, string motivo)
        {
            if (nuevo.Gravedad() <= nivel.Gravedad()) return;
            nivel = nuevo;
            porQue = motivo;
        }

        foreach (var regla in ing.Reglas)
            if (regla.Si.All(etiquetas.Contains))
                Empeorar(regla.Nivel, regla.PorQue);

        if (item.Porcentaje is { } pct)
        {
            var etiquetaTramo = tramo == Tramo.De13a14 ? "t1" : "t2";
            foreach (var limite in ing.Limites)
                if (pct > limite.MasDe && (limite.Tramo is null || limite.Tramo == etiquetaTramo))
                    Empeorar(limite.Nivel, limite.PorQue);
        }

        if (contexto.Isotretinoina && ing.Irritante)
            Empeorar(Nivel.Evitar,
                "Con isotretinoína (pastillas) la piel está muy sensible: nada de retinoides, ácidos, peróxido ni exfoliantes. Solo hidratante, bálsamo labial y protector solar. Habla con tu médico/a.");

        if (ing.Irritante && etiquetas.Contains("sensible"))
            Empeorar(Nivel.Precaucion,
                "Con la piel sensible o con eccema, los activos fuertes irritan con facilidad: si lo usas, empieza muy despacio.");

        var ayuda = nivel.Gravedad() == 0
            ? ing.Ayuda.Where(etiquetas.Contains).Select(a => AyudaTexto.GetValueOrDefault(a, a)).ToList()
            : [];

        return new ItemEvaluado
        {
            Texto = item.Original,
            Posicion = item.Posicion,
            IngredienteId = ing.Id,
            Nombre = ing.Nombre,
            Categoria = ing.Categoria,
            Nivel = nivel,
            PorQue = porQue,
            Resumen = ing.Resumen,
            Detalle = ing.Detalle,
            Porcentaje = item.Porcentaje,
            AyudaCon = ayuda,
            Fuentes = ing.Fuentes
        };
    }

    private static void AnadirAlertasDeMezcla(Dictionary<string, string> grupos, bool hayMedicamento, List<Alerta> alertas)
    {
        if (grupos.Count < 2) return;

        var lista = string.Join(" + ", grupos.Keys.Select(NombreGrupo));

        if (hayMedicamento)
        {
            alertas.Add(new Alerta
            {
                Nivel = Nivel.Precaucion,
                Texto = $"Lleva varios activos fuertes a la vez ({lista}). Esta combinación existe en tratamientos del acné, pero solo con indicación de un médico/a o dermatólogo/a."
            });
            return;
        }

        alertas.Add(new Alerta
        {
            Nivel = Nivel.Evitar,
            Texto = $"Mezcla varios activos fuertes ({lista}). Juntarlos multiplica el riesgo de irritación y de manchas; lo prudente es introducir uno solo, de uno en uno."
        });
    }

    private void AnadirAlertasDeRutina(PeticionEvaluacion peticion, Dictionary<string, string> gruposProducto, List<Alerta> alertas)
    {
        if (peticion.Rutina.Count == 0 || gruposProducto.Count == 0) return;

        var indice = 0;
        foreach (var otro in peticion.Rutina)
        {
            indice++;
            if (string.IsNullOrWhiteSpace(otro.Inci)) continue;

            var nombreOtro = string.IsNullOrWhiteSpace(otro.Nombre) ? $"tu producto {indice}" : $"«{otro.Nombre.Trim()}»";
            var gruposOtro = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in InciParser.Parse(otro.Inci, MaxIngredientes))
            {
                var ing = catalogo.Buscar(item);
                if (ing is { Irritante: true, Grupo: not null }) gruposOtro.Add(ing.Grupo);
            }

            foreach (var grupo in gruposProducto.Keys.Where(gruposOtro.Contains))
                alertas.Add(new Alerta
                {
                    Nivel = Nivel.Evitar,
                    Texto = $"Ya usas {NombreGrupo(grupo)} en {nombreOtro}. Repetir el mismo activo en varios productos es lo que más irrita (pasa mucho con las rutinas de redes sociales)."
                });

            foreach (var grupo in gruposOtro.Where(g => !gruposProducto.ContainsKey(g)))
                alertas.Add(new Alerta
                {
                    Nivel = Nivel.Precaucion,
                    Texto = $"En {nombreOtro} usas {NombreGrupo(grupo)}, que es otro activo fuerte. Mejor no usar dos a la vez: introduce uno solo cada vez."
                });
        }
    }

    private static string NombreGrupo(string grupo) => NombresGrupo.GetValueOrDefault(grupo, grupo);

    private static Veredicto CalcularVeredicto(List<ItemEvaluado> items, List<Alerta> alertas, int desconocidos, int reconocidos)
    {
        var peor = Nivel.Neutro;
        foreach (var i in items) peor = peor.Peor(i.Nivel);
        foreach (var a in alertas) peor = peor.Peor(a.Nivel);

        if (peor == Nivel.Evitar) return Veredicto.Evitar;
        if (peor == Nivel.Precaucion) return Veredicto.Precaucion;

        if (reconocidos == 0) return Veredicto.SinDatos;
        if (desconocidos == 0) return Veredicto.Buena;

        var total = reconocidos + desconocidos;
        return desconocidos * 2 > total ? Veredicto.SinDatos : Veredicto.Parcial;
    }

    private static (string Titulo, string Resumen) Redactar(Veredicto v, List<ItemEvaluado> items, List<Alerta> alertas, int desconocidos)
    {
        var ademas = desconocidos > 0
            ? $" Además, hay {desconocidos} {(desconocidos == 1 ? "ingrediente que no conozco" : "ingredientes que no conozco")}."
            : "";

        return v switch
        {
            Veredicto.Evitar => ("Mejor no",
                "Con lo que me cuentas de ti, este producto no es buena idea. Mira abajo qué lo causa." + ademas),
            Veredicto.Precaucion => ("Con ojo",
                "No es un «no» rotundo, pero hay cosas que conviene tener en cuenta antes de usarlo." + ademas),
            Veredicto.Buena => ("Pinta bien",
                "No veo ninguna alerta en sus ingredientes para tu edad y tu piel. Aun así, pruébalo primero en una zona pequeña."),
            Veredicto.Parcial => ("Parece bien, pero no lo conozco todo",
                $"En lo que conozco no hay alertas, pero hay {desconocidos} {(desconocidos == 1 ? "ingrediente" : "ingredientes")} que no sé valorar. Por eso no puedo decirte que sea buena idea."),
            _ => ("No puedo valorarlo",
                items.Count == 0
                    ? "No he reconocido ningún ingrediente de la lista, así que no puedo opinar."
                    : "No conozco suficientes ingredientes de la lista como para opinar con fiabilidad.")
        };
    }

    private static string MensajeDesconocidos(int n) =>
        $"No conozco {(n == 1 ? "este ingrediente" : $"estos {n} ingredientes")}, así que no puedo decirte nada sobre {(n == 1 ? "él" : "ellos")}. " +
        "Que no salgan como un problema no significa que sean seguros: significa que todavía no los he evaluado.";
}
