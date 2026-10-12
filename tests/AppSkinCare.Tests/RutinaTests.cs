using AppSkinCare.Core;
using AppSkinCare.Core.Models;
using Xunit;

namespace AppSkinCare.Tests;

public class RutinaTests
{
    private static readonly GeneradorRutina Generador = new();

    [Fact]
    public void La_base_son_tres_pasos_por_la_manana()
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 15 });

        Assert.Equal(new[] { "Limpiar", "Hidratar", "Protector solar" }, r.Manana.Select(p => p.Titulo).ToArray());
        Assert.DoesNotContain(r.Noche, p => p.Titulo.StartsWith("Tratamiento"));
    }

    [Fact]
    public void Con_isotretinoina_no_hay_tratamiento_y_se_avisa_primero()
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 16, Acne = IntensidadAcne.Leve, Isotretinoina = true });

        Assert.DoesNotContain(r.Noche, p => p.Titulo.StartsWith("Tratamiento"));
        Assert.Contains("isotretinoína", r.Evita[0]);
        Assert.Contains("isotretinoína", r.Resumen);
    }

    [Fact]
    public void Un_acne_leve_recibe_un_solo_tratamiento()
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 15, Acne = IntensidadAcne.Leve });

        Assert.Single(r.Noche.Where(p => p.Titulo.StartsWith("Tratamiento")));
    }

    [Fact]
    public void Con_acne_y_manchas_el_azelaico_va_primero()
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 15, Acne = IntensidadAcne.Leve, Manchas = true });
        var tratamiento = r.Noche.First(p => p.Titulo.StartsWith("Tratamiento"));

        Assert.Contains("azelaico", tratamiento.Que);
    }

    [Theory]
    [InlineData(IntensidadAcne.Moderado)]
    [InlineData(IntensidadAcne.Intenso)]
    public void Con_acne_importante_se_deriva_al_medico(IntensidadAcne acne)
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 16, Acne = acne });

        Assert.Contains(r.Noche, p => p.Titulo.Contains("médico"));
    }

    [Fact]
    public void La_piel_clara_o_mucho_sol_piden_spf_50()
    {
        var clara = Generador.Generar(new PerfilRutina { Edad = 15, PielClara = true });
        var normal = Generador.Generar(new PerfilRutina { Edad = 15 });

        Assert.Contains("SPF 50", clara.Manana[2].Buscar[0]);
        Assert.Contains("SPF 30", normal.Manana[2].Buscar[0]);
    }

    [Fact]
    public void Con_eccema_o_piel_sensible_se_recomiendan_filtros_minerales()
    {
        var r = Generador.Generar(new PerfilRutina { Edad = 15, Atopia = true });

        Assert.Contains(r.Manana[2].Buscar, b => b.Contains("minerales"));
        Assert.Contains(r.Manana[1].Evitar, e => e.Contains("oliva"));
    }

    [Fact]
    public void Siempre_dice_cuando_ir_al_medico_y_lleva_aviso()
    {
        var r = Generador.Generar(new PerfilRutina());

        Assert.NotEmpty(r.MedicoSi);
        Assert.Contains("no sustituye", r.AvisoLegal);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(19)]
    public void Fuera_de_13_a_18_anos_no_genera_rutina(int edad)
    {
        Assert.Throws<ArgumentException>(() => Generador.Generar(new PerfilRutina { Edad = edad }));
    }
}
