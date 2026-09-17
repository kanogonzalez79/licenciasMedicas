using System.Runtime.Versioning;
using Dapper;
using LicenciasMedicas.Core.Data;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace LicenciasMedicas.Core.Correos;

/// <summary>
/// Guarda, obtiene y prueba la configuración SMTP usada para enviar directamente los correos de
/// aviso de licencias médicas. Ver capability "configuracion-smtp" y design.md del cambio
/// "enviar-correo-smtp" para las decisiones de cifrado y de fila única.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ConfiguracionSmtpService
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ConfiguracionSmtpService(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>Configuración vigente sin la contraseña, o null si nunca se guardó ninguna.</summary>
    public ConfiguracionSmtp? Obtener()
    {
        var registro = ObtenerRegistro();
        if (registro is null)
            return null;

        return new ConfiguracionSmtp
        {
            Host = registro.Host,
            Puerto = registro.Puerto,
            Usuario = registro.Usuario,
            Remitente = registro.Remitente,
            UsaSsl = registro.UsaSsl,
        };
    }

    /// <summary>Configuración vigente con la contraseña ya descifrada, para uso interno al enviar un correo.</summary>
    internal ConfiguracionSmtpCredenciales? ObtenerCredenciales()
    {
        var registro = ObtenerRegistro();
        if (registro is null)
            return null;

        return new ConfiguracionSmtpCredenciales
        {
            Host = registro.Host,
            Puerto = registro.Puerto,
            Usuario = registro.Usuario,
            ContrasenaPlana = ContrasenaSmtpProtector.Descifrar(registro.ContrasenaCifrada),
            Remitente = registro.Remitente,
            UsaSsl = registro.UsaSsl,
        };
    }

    public void Guardar(string host, int puerto, string usuario, string contrasenaPlana, string remitente, bool usaSsl)
    {
        if (string.IsNullOrWhiteSpace(host) || puerto <= 0 || string.IsNullOrWhiteSpace(usuario)
            || string.IsNullOrWhiteSpace(contrasenaPlana) || string.IsNullOrWhiteSpace(remitente))
            throw new ConfiguracionSmtpInvalidaException("Host, puerto, usuario, contraseña y remitente son obligatorios.");

        var contrasenaCifrada = ContrasenaSmtpProtector.Cifrar(contrasenaPlana);

        const string sql = """
            INSERT INTO ConfiguracionSmtp (Id, Host, Puerto, Usuario, ContrasenaCifrada, Remitente, UsaSsl)
            VALUES (1, @host, @puerto, @usuario, @contrasenaCifrada, @remitente, @usaSsl)
            ON CONFLICT (Id) DO UPDATE SET
                Host = @host, Puerto = @puerto, Usuario = @usuario,
                ContrasenaCifrada = @contrasenaCifrada, Remitente = @remitente, UsaSsl = @usaSsl;
            """;

        using var connection = _connectionFactory.Crear();
        connection.Execute(sql, new { host, puerto, usuario, contrasenaCifrada, remitente, usaSsl });
    }

    /// <summary>Prueba conexión y autenticación con los datos dados, sin requerir que ya estén guardados.</summary>
    public ResultadoPruebaSmtp ProbarConexion(string host, int puerto, string usuario, string contrasenaPlana, bool usaSsl)
    {
        try
        {
            using var cliente = new SmtpClient();
            cliente.Connect(host, puerto, usaSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);
            cliente.Authenticate(usuario, contrasenaPlana);
            cliente.Disconnect(quit: true);
            return new ResultadoPruebaSmtp(true, null);
        }
        catch (Exception ex)
        {
            return new ResultadoPruebaSmtp(false, ex.Message);
        }
    }

    private ConfiguracionSmtpRegistro? ObtenerRegistro()
    {
        using var connection = _connectionFactory.Crear();
        return connection.QuerySingleOrDefault<ConfiguracionSmtpRegistro>(
            "SELECT Host, Puerto, Usuario, ContrasenaCifrada, Remitente, UsaSsl FROM ConfiguracionSmtp WHERE Id = 1;");
    }

    private sealed class ConfiguracionSmtpRegistro
    {
        public string Host { get; set; } = string.Empty;
        public int Puerto { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string ContrasenaCifrada { get; set; } = string.Empty;
        public string Remitente { get; set; } = string.Empty;
        public bool UsaSsl { get; set; }
    }
}
