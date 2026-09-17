namespace LicenciasMedicas.Core.Correos;

/// <summary>Configuración SMTP vigente, sin la contraseña - es lo único que puede exponerse en una respuesta HTTP.</summary>
public sealed class ConfiguracionSmtp
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Remitente { get; set; } = string.Empty;
    public bool UsaSsl { get; set; }
}

/// <summary>
/// Configuración SMTP completa, incluyendo la contraseña ya descifrada en memoria. Solo para uso
/// interno de LicenciasMedicas.Core.Correos (envío real de correos) - nunca se serializa en una
/// respuesta HTTP.
/// </summary>
public sealed class ConfiguracionSmtpCredenciales
{
    public required string Host { get; init; }
    public required int Puerto { get; init; }
    public required string Usuario { get; init; }
    public required string ContrasenaPlana { get; init; }
    public required string Remitente { get; init; }
    public required bool UsaSsl { get; init; }
}

public sealed class ConfiguracionSmtpInvalidaException : Exception
{
    public ConfiguracionSmtpInvalidaException(string mensaje) : base(mensaje)
    {
    }
}

public sealed record ResultadoPruebaSmtp(bool Exito, string? Error);
