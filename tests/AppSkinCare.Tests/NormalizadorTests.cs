using AppSkinCare.Core;
using Xunit;

namespace AppSkinCare.Tests;

public class NormalizadorTests
{
    [Theory]
    [InlineData("Alcohol Denat.", "alcohol denat")]
    [InlineData("PEG-100 Stearate", "peg 100 stearate")]
    [InlineData("Ácido Hialurónico", "acido hialuronico")]
    [InlineData("  Aqua   (Water) ", "aqua water")]
    [InlineData("Caprylic/Capric Triglyceride", "caprylic capric triglyceride")]
    public void Normaliza_a_minusculas_sin_tildes_ni_signos(string entrada, string esperado)
    {
        Assert.Equal(esperado, Normalizador.Normalizar(entrada));
    }

    [Fact]
    public void Texto_vacio_o_nulo_da_cadena_vacia()
    {
        Assert.Equal("", Normalizador.Normalizar(null));
        Assert.Equal("", Normalizador.Normalizar("   "));
    }
}
