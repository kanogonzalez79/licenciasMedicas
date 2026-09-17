using System.Runtime.Versioning;
using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Core.Tests;

[SupportedOSPlatform("windows")]
public sealed class ContrasenaSmtpProtectorTests
{
    [Fact]
    public void CifrarYDescifrar_DevuelveElValorOriginal()
    {
        const string contrasena = "S3cr3t@-de-prueba";

        var cifrada = ContrasenaSmtpProtector.Cifrar(contrasena);
        var descifrada = ContrasenaSmtpProtector.Descifrar(cifrada);

        Assert.Equal(contrasena, descifrada);
    }

    [Fact]
    public void Cifrar_NoDejaLaContrasenaEnTextoPlanoEnElResultado()
    {
        const string contrasena = "S3cr3t@-de-prueba";

        var cifrada = ContrasenaSmtpProtector.Cifrar(contrasena);

        Assert.DoesNotContain(contrasena, cifrada);
    }
}
