using System.Runtime.Versioning;
using Dapper;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Unidades;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LicenciasMedicas.Core.Correos;

public sealed class ConfiguracionSmtpNoConfiguradaException : Exception
{
    public ConfiguracionSmtpNoConfiguradaException()
        : base("No hay una configuración SMTP guardada. Configúrela antes de enviar un correo.")
    {
    }
}

public sealed class UnidadSinCorreoValidoException : Exception
{
    public UnidadSinCorreoValidoException()
        : base("La unidad no tiene un correo electrónico con formato válido registrado.")
    {
    }
}

public sealed class EnvioCorreoFallidoException : Exception
{
    public EnvioCorreoFallidoException(string mensaje, Exception inner) : base(mensaje, inner)
    {
    }
}

/// <summary>
/// Envía directamente por SMTP el aviso de licencias de una unidad en una fecha, y registra el
/// envío exitoso. Ver capability "enviar-correo" y design.md del cambio "enviar-correo-smtp".
/// Depende de ConfiguracionSmtpService (cifrado de contraseña con DPAPI), así que hereda su
/// restricción a Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CorreoEnvioService
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly UnidadesRepository _unidadesRepo;
    private readonly ConfiguracionSmtpService _configuracionSmtp;
    private readonly CorreoCopiaService _correoCopia;
    private readonly SmtpDiagnosticoLog _diagnosticoLog;

    public CorreoEnvioService(
        SqliteConnectionFactory connectionFactory, UnidadesRepository unidadesRepo,
        ConfiguracionSmtpService configuracionSmtp, CorreoCopiaService correoCopia, SmtpDiagnosticoLog diagnosticoLog)
    {
        _connectionFactory = connectionFactory;
        _unidadesRepo = unidadesRepo;
        _configuracionSmtp = configuracionSmtp;
        _correoCopia = correoCopia;
        _diagnosticoLog = diagnosticoLog;
    }

    public void Enviar(int unidadId, DateOnly fecha, string texto)
    {
        var credenciales = _configuracionSmtp.ObtenerCredenciales()
            ?? throw new ConfiguracionSmtpNoConfiguradaException();

        using var connection = _connectionFactory.Crear();
        var unidad = _unidadesRepo.ObtenerPorId(connection, unidadId)
            ?? throw new InvalidOperationException($"No existe la unidad {unidadId}.");

        if (!EmailUtils.EsFormatoValido(unidad.CorreoElectronico))
            throw new UnidadSinCorreoValidoException();

        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(MailboxAddress.Parse(credenciales.Remitente));
            mensaje.To.Add(MailboxAddress.Parse(unidad.CorreoElectronico!));
            foreach (var correoCopia in _correoCopia.ListarActivos())
                mensaje.Cc.Add(MailboxAddress.Parse(correoCopia));
            mensaje.Subject = $"Licencias médicas - {unidad.Descripcion} - {fecha:yyyy-MM-dd}";
            mensaje.Body = new TextPart("plain") { Text = texto };

            using var cliente = new SmtpClient();
            cliente.Connect(credenciales.Host, credenciales.Puerto,
                credenciales.UsaSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);
            cliente.Authenticate(credenciales.Usuario, credenciales.ContrasenaPlana);
            cliente.Send(mensaje);
            cliente.Disconnect(quit: true);
            _diagnosticoLog.RegistrarExito("envio", credenciales.Host, credenciales.Puerto, credenciales.Usuario, credenciales.UsaSsl);
        }
        catch (Exception ex)
        {
            _diagnosticoLog.RegistrarError("envio", credenciales.Host, credenciales.Puerto, credenciales.Usuario, credenciales.UsaSsl, ex);
            throw new EnvioCorreoFallidoException($"No se pudo enviar el correo: {ex.Message}", ex);
        }

        RegistrarEnvio(connection, unidadId, fecha);
    }

    private static void RegistrarEnvio(Microsoft.Data.Sqlite.SqliteConnection connection, int unidadId, DateOnly fecha)
    {
        const string sql = """
            INSERT INTO CorreosEnviados (UnidadId, Fecha, FechaHoraEnvio)
            VALUES (@unidadId, @fecha, @fechaHoraEnvio)
            ON CONFLICT (UnidadId, Fecha) DO UPDATE SET FechaHoraEnvio = @fechaHoraEnvio;
            """;
        connection.Execute(sql, new
        {
            unidadId,
            fecha = fecha.ToString("yyyy-MM-dd"),
            fechaHoraEnvio = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        });
    }
}
