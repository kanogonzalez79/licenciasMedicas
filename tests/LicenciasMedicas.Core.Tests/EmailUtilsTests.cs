using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Core.Tests;

public class EmailUtilsTests
{
    [Theory]
    [InlineData("persona@dominio.cl")]
    [InlineData("persona.apellido@sub.dominio.com")]
    [InlineData("  persona@dominio.cl  ")]
    public void EsFormatoValido_CorreoValido_RetornaTrue(string correo)
    {
        Assert.True(EmailUtils.EsFormatoValido(correo));
    }

    [Theory]
    [InlineData("dominio.cl")]
    [InlineData("persona@dominio")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EsFormatoValido_CorreoInvalido_RetornaFalse(string? correo)
    {
        Assert.False(EmailUtils.EsFormatoValido(correo));
    }
}
