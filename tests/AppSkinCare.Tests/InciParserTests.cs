using AppSkinCare.Core;
using Xunit;

namespace AppSkinCare.Tests;

public class InciParserTests
{
    [Fact]
    public void Divide_por_comas_pero_no_dentro_de_parentesis()
    {
        var items = InciParser.Parse("Aqua (Water, Eau), Glycerin, Parfum");

        Assert.Equal(3, items.Count);
        Assert.Equal("Aqua (Water, Eau)", items[0].Original);
    }

    [Fact]
    public void Quita_el_prefijo_Ingredients()
    {
        var items = InciParser.Parse("Ingredients: Aqua, Glycerin");

        Assert.Equal(2, items.Count);
        Assert.Equal("Aqua", items[0].Original);
    }

    [Fact]
    public void Admite_saltos_de_linea_y_puntos_y_coma()
    {
        var items = InciParser.Parse("Aqua\nGlycerin; Niacinamide\r\nParfum");

        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void Las_posiciones_empiezan_en_uno_y_siguen_el_orden()
    {
        var items = InciParser.Parse("Aqua, Glycerin, Parfum");

        Assert.Equal(1, items[0].Posicion);
        Assert.Equal(3, items[2].Posicion);
    }

    [Theory]
    [InlineData("Salicylic Acid 2%", 2.0)]
    [InlineData("Niacinamide (5,5 %)", 5.5)]
    [InlineData("Retinol 0.3%", 0.3)]
    public void Extrae_el_porcentaje_si_lo_hay(string entrada, double esperado)
    {
        var item = InciParser.Parse(entrada)[0];

        Assert.Equal(esperado, item.Porcentaje);
    }

    [Fact]
    public void Sin_porcentaje_no_inventa_ninguno()
    {
        var item = InciParser.Parse("Niacinamide")[0];

        Assert.Null(item.Porcentaje);
    }

    [Fact]
    public void El_parentesis_aporta_candidatos_alternativos()
    {
        var item = InciParser.Parse("Parfum (Fragrance)")[0];

        Assert.Contains("parfum", item.Candidatos);
        Assert.Contains("fragrance", item.Candidatos);
    }

    [Fact]
    public void Las_barras_aportan_candidatos_pero_el_texto_completo_va_primero()
    {
        var item = InciParser.Parse("Aqua/Water/Eau")[0];

        Assert.Equal("aqua water eau", item.Candidatos[0]);
        Assert.Contains("water", item.Candidatos);
    }

    [Fact]
    public void Texto_vacio_no_da_ingredientes()
    {
        Assert.Empty(InciParser.Parse("  "));
        Assert.Empty(InciParser.Parse(null));
        Assert.Empty(InciParser.Parse(",,, ;"));
    }

    [Fact]
    public void Respeta_el_maximo_de_ingredientes()
    {
        var texto = string.Join(", ", Enumerable.Range(1, 400).Select(i => $"Ingrediente{i}"));

        Assert.Equal(250, InciParser.Parse(texto).Count);
    }
}
