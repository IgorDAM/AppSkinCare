using AppSkinCare.Core;
using AppSkinCare.Core.Models;
using Xunit;

namespace AppSkinCare.Tests;

public class EvaluadorTests
{
    private static readonly Evaluador Evaluador = new(CatalogoIngredientes.Cargar());

    private static ResultadoEvaluacion Evaluar(string inci, Contexto? contexto = null, params ProductoInci[] rutina) =>
        Evaluador.Evaluar(new PeticionEvaluacion
        {
            Producto = new ProductoInci { Inci = inci },
            Contexto = contexto ?? new Contexto { Edad = 16 },
            Rutina = rutina
        });

    // ---------- lo que no conoce, lo dice ----------

    [Fact]
    public void Una_crema_sencilla_y_conocida_pinta_bien()
    {
        var r = Evaluar("Aqua, Glycerin, Cetearyl Alcohol, Ceramide NP, Dimethicone, Panthenol");

        Assert.Equal(Veredicto.Buena, r.Veredicto);
        Assert.Empty(r.Desconocidos);
        Assert.Null(r.MensajeDesconocidos);
    }

    [Fact]
    public void Un_ingrediente_desconocido_se_dice_claro_y_nunca_se_da_por_bueno()
    {
        var r = Evaluar("Aqua, Glycerin, Cetearyl Alcohol, Zzxqwv Extract");

        Assert.Equal(Veredicto.Parcial, r.Veredicto);
        Assert.Equal(1, r.Desconocidos.Count);
        Assert.Equal("Zzxqwv Extract", r.Desconocidos[0].Texto);
        Assert.DoesNotContain(r.Ingredientes, i => i.Texto == "Zzxqwv Extract");
        Assert.Contains("No conozco", r.MensajeDesconocidos!);
        Assert.Contains("no significa que sean seguros", r.MensajeDesconocidos!);
    }

    [Fact]
    public void Si_no_conoce_casi_nada_no_opina()
    {
        var r = Evaluar("Foo Bar, Baz Qux, Quux Corge");

        Assert.Equal(Veredicto.SinDatos, r.Veredicto);
        Assert.Equal(0, r.Reconocidos);
        Assert.Equal(3, r.Desconocidos.Count);
    }

    [Fact]
    public void Si_la_mayoria_es_desconocida_tampoco_opina()
    {
        var r = Evaluar("Aqua, Foo Bar, Baz Qux, Quux Corge");

        Assert.Equal(Veredicto.SinDatos, r.Veredicto);
    }

    [Fact]
    public void Un_desconocido_no_tapa_una_alerta_real()
    {
        var r = Evaluar("Aqua, Hydroquinone, Foo Bar");

        Assert.Equal(Veredicto.Evitar, r.Veredicto);
        Assert.Contains("Además", r.Resumen);
    }

    [Fact]
    public void Una_errata_se_marca_desconocida_pero_con_pista()
    {
        var r = Evaluar("Aqua, Niacinamid");

        Assert.Equal(1, r.Desconocidos.Count);
        Assert.Contains("Niacinamida", r.Desconocidos[0].Sugerencia!);
    }

    // ---------- por edad ----------

    [Fact]
    public void El_retinol_se_evita_a_los_13_y_exige_precaucion_a_los_16()
    {
        Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Glycerin, Retinol", new Contexto { Edad = 13 }).Veredicto);
        Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Glycerin, Retinol", new Contexto { Edad = 14 }).Veredicto);
        Assert.Equal(Veredicto.Precaucion, Evaluar("Aqua, Glycerin, Retinol", new Contexto { Edad = 16 }).Veredicto);
    }

    [Fact]
    public void Los_peptidos_antiedad_se_reconocen_por_patron()
    {
        var r = Evaluar("Aqua, Palmitoyl Tripeptide-5", new Contexto { Edad = 13 });

        Assert.Equal(Veredicto.Evitar, r.Veredicto);
        Assert.Equal("peptidos", r.Ingredientes[1].IngredienteId);
    }

    // ---------- concentraciones ----------

    [Fact]
    public void Mas_de_un_2_por_ciento_de_salicilico_se_evita()
    {
        Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Salicylic Acid 3%", new Contexto { Edad = 17 }).Veredicto);
    }

    [Fact]
    public void Un_2_por_ciento_de_salicilico_a_los_17_esta_bien_pero_avisa_del_sol()
    {
        var r = Evaluar("Aqua, Salicylic Acid 2%", new Contexto { Edad = 17 });

        Assert.Equal(Veredicto.Buena, r.Veredicto);
        Assert.Contains(r.Avisos, a => a.Contains("protector solar"));
    }

    [Fact]
    public void Sin_porcentaje_avisa_de_que_no_sabe_cuanto_lleva()
    {
        var r = Evaluar("Aqua, Salicylic Acid", new Contexto { Edad = 17 });

        Assert.Contains(r.Avisos, a => a.Contains("No sé cuánto") && a.Contains("2 %"));
    }

    [Fact]
    public void Peroxido_de_benzoilo_al_10_por_ciento_a_los_13_pide_precaucion()
    {
        Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Benzoyl Peroxide 10.5%", new Contexto { Edad = 13 }).Veredicto);
        Assert.Equal(Veredicto.Precaucion, Evaluar("Aqua, Benzoyl Peroxide 7%", new Contexto { Edad = 13 }).Veredicto);
        Assert.Equal(Veredicto.Buena, Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 13 }).Veredicto);
    }

    // ---------- mezclas y rutina ----------

    [Fact]
    public void Dos_activos_fuertes_en_el_mismo_producto_se_evitan()
    {
        var r = Evaluar("Aqua, Retinol, Glycolic Acid", new Contexto { Edad = 17 });

        Assert.Equal(Veredicto.Evitar, r.Veredicto);
        Assert.Contains(r.Alertas, a => a.Nivel == Nivel.Evitar && a.Texto.Contains("Mezcla"));
    }

    [Fact]
    public void La_combinacion_de_medicamentos_del_acne_es_precaucion_no_prohibicion()
    {
        var r = Evaluar("Aqua, Adapalene, Benzoyl Peroxide", new Contexto { Edad = 16, Acne = true });

        Assert.Equal(Veredicto.Precaucion, r.Veredicto);
        Assert.Contains(r.Alertas, a => a.Texto.Contains("indicación de un médico"));
    }

    [Fact]
    public void Repetir_el_mismo_activo_en_otro_producto_de_la_rutina_se_evita()
    {
        var r = Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 16 },
            new ProductoInci { Nombre = "Mi limpiador", Inci = "Aqua, Benzoyl Peroxide 5%" });

        Assert.Equal(Veredicto.Evitar, r.Veredicto);
        Assert.Contains(r.Alertas, a => a.Texto.Contains("Ya usas") && a.Texto.Contains("Mi limpiador"));
    }

    [Fact]
    public void Otro_activo_distinto_en_la_rutina_pide_introducirlos_de_uno_en_uno()
    {
        var r = Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 16 },
            new ProductoInci { Inci = "Aqua, Retinol" });

        Assert.Equal(Veredicto.Precaucion, r.Veredicto);
        Assert.Contains(r.Alertas, a => a.Texto.Contains("uno solo cada vez"));
    }

    // ---------- según la persona ----------

    [Fact]
    public void Con_isotretinoina_cualquier_activo_fuerte_se_evita()
    {
        var r = Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 16, Isotretinoina = true });

        Assert.Equal(Veredicto.Evitar, r.Veredicto);
        Assert.Contains("isotretinoína", r.Ingredientes[1].PorQue);
    }

    [Fact]
    public void El_perfume_es_precaucion_y_con_eccema_se_evita()
    {
        Assert.Equal(Veredicto.Precaucion, Evaluar("Aqua, Glycerin, Parfum").Veredicto);
        Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Glycerin, Parfum", new Contexto { Edad = 16, Atopia = true }).Veredicto);
    }

    [Fact]
    public void Un_activo_fuerte_con_piel_sensible_nunca_sale_limpio()
    {
        var r = Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 16, Sensible = true });

        Assert.Equal(Veredicto.Precaucion, r.Veredicto);
    }

    [Fact]
    public void El_alcohol_solo_preocupa_si_esta_entre_los_primeros_ingredientes()
    {
        var primero = Evaluar("Alcohol Denat., Aqua, Glycerin");
        var ultimo = Evaluar("Aqua, Glycerin, Cetearyl Alcohol, Dimethicone, Panthenol, Allantoin, Tocopherol, Alcohol Denat.");

        Assert.Equal(Veredicto.Precaucion, primero.Veredicto);
        Assert.Equal(Veredicto.Buena, ultimo.Veredicto);
    }

    [Fact]
    public void La_vaselina_en_cara_con_piel_grasa_pide_precaucion_pero_si_se_aclara_no()
    {
        var grasa = new Contexto { Edad = 16, TipoPiel = TipoPiel.Grasa };

        Assert.Equal(Veredicto.Precaucion, Evaluar("Aqua, Petrolatum", grasa).Veredicto);
        Assert.Equal(Veredicto.Buena, Evaluar("Aqua, Petrolatum", grasa with { SeAclara = true }).Veredicto);
    }

    [Fact]
    public void Lo_que_ayuda_solo_se_dice_si_no_hay_alerta()
    {
        var r = Evaluar("Aqua, Benzoyl Peroxide 2.5%", new Contexto { Edad = 16, Acne = true });

        Assert.Contains("los granos", r.Ingredientes[1].AyudaCon);
    }

    // ---------- reconocimiento ----------

    [Fact]
    public void Reconoce_nombres_con_formatos_distintos()
    {
        var r = Evaluar("AQUA/WATER/EAU, Glycerin, Parfum (Fragrance), Alcohol Denat., Aloe Vera");

        Assert.Contains(r.Ingredientes, i => i.IngredienteId == "agua");
        Assert.Contains(r.Ingredientes, i => i.IngredienteId == "perfume");
        Assert.Contains(r.Ingredientes, i => i.IngredienteId == "alcohol-denat");
    }

    [Fact]
    public void Los_aceites_esenciales_se_reconocen_por_patron()
    {
        var r = Evaluar("Aqua, Melaleuca Alternifolia (Tea Tree) Leaf Oil");

        Assert.Equal("aceites-esenciales", r.Ingredientes[1].IngredienteId);
        Assert.Equal(Nivel.Precaucion, r.Ingredientes[1].Nivel);
    }

    [Fact]
    public void La_hidroquinona_y_el_mercurio_se_evitan_a_cualquier_edad()
    {
        foreach (var edad in new[] { 13, 15, 18 })
        {
            Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Hydroquinone", new Contexto { Edad = edad }).Veredicto);
            Assert.Equal(Veredicto.Evitar, Evaluar("Aqua, Mercurous Chloride", new Contexto { Edad = edad }).Veredicto);
        }
    }

    [Fact]
    public void Los_ingredientes_salen_en_el_orden_de_la_etiqueta()
    {
        var r = Evaluar("Aqua, Glycerin, Niacinamide");

        Assert.Equal(1, r.Ingredientes[0].Posicion);
        Assert.Equal("glicerina", r.Ingredientes[1].IngredienteId);
        Assert.Equal(3, r.Ingredientes[2].Posicion);
    }

    // ---------- entradas incorrectas ----------

    [Theory]
    [InlineData(12)]
    [InlineData(19)]
    [InlineData(0)]
    public void Fuera_de_13_a_18_anos_no_evalua(int edad)
    {
        Assert.Throws<ArgumentException>(() => Evaluar("Aqua", new Contexto { Edad = edad }));
    }

    [Fact]
    public void Sin_ingredientes_no_evalua()
    {
        Assert.Throws<ArgumentException>(() => Evaluar("   "));
        Assert.Throws<ArgumentException>(() => Evaluar(",,, ;;"));
    }

    [Fact]
    public void El_resultado_siempre_lleva_el_aviso_legal()
    {
        Assert.Contains("no sustituye", Evaluar("Aqua, Glycerin").AvisoLegal);
    }
}
