using LicenciasMedicas.Core.Rut;

namespace LicenciasMedicas.Core.Tests;

public class RutUtilsTests
{
    [Theory]
    [InlineData("17000494-9", 17000494, '9')]
    [InlineData("20124969-4", 20124969, '4')]
    [InlineData("27446954-4", 27446954, '4')]
    public void TryParse_RutValido_RetornaNumeroYDv(string crudo, long numeroEsperado, char dvEsperado)
    {
        Assert.True(RutUtils.TryParse(crudo, out var rut));
        Assert.Equal(numeroEsperado, rut.Numero);
        Assert.Equal(dvEsperado, rut.Dv);
    }

    [Fact]
    public void TryParse_ConDvK_EsValido()
    {
        var dv = RutUtils.CalcularDv(17000497);
        Assert.True(RutUtils.TryParse($"17000497-{dv}", out var rut));
        Assert.Equal(dv, rut.Dv);
    }

    [Theory]
    [InlineData("17000494-0")]
    [InlineData("no-es-un-rut")]
    [InlineData("")]
    public void TryParse_RutInvalido_RetornaFalse(string crudo)
    {
        Assert.False(RutUtils.TryParse(crudo, out _));
    }

    [Fact]
    public void CalcularDv_EsConsistenteConValidacion()
    {
        for (long numero = 1; numero < 100_000; numero += 3541)
        {
            var dv = RutUtils.CalcularDv(numero);
            Assert.True(RutUtils.EsDvValido(numero, dv));
        }
    }
}
