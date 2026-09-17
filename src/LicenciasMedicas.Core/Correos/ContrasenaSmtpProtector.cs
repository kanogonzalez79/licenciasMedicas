using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace LicenciasMedicas.Core.Correos;

/// <summary>
/// Cifra/descifra la contraseña SMTP con DPAPI (scope CurrentUser), atada a la cuenta de Windows
/// que corre la app - coherente con que la app ya es Windows-only y de un solo usuario. Ver
/// design.md del cambio "enviar-correo-smtp": un respaldo restaurado en otra cuenta/máquina
/// no puede descifrar la contraseña guardada, hay que volver a ingresarla en Configuración.
/// </summary>
[SupportedOSPlatform("windows")]
public static class ContrasenaSmtpProtector
{
    private static readonly byte[] Entropia = Encoding.UTF8.GetBytes("LicenciasMedicas.ConfiguracionSmtp");

    public static string Cifrar(string contrasenaPlana)
    {
        var bytesPlanos = Encoding.UTF8.GetBytes(contrasenaPlana);
        var bytesCifrados = ProtectedData.Protect(bytesPlanos, Entropia, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(bytesCifrados);
    }

    public static string Descifrar(string contrasenaCifrada)
    {
        var bytesCifrados = Convert.FromBase64String(contrasenaCifrada);
        var bytesPlanos = ProtectedData.Unprotect(bytesCifrados, Entropia, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytesPlanos);
    }
}
