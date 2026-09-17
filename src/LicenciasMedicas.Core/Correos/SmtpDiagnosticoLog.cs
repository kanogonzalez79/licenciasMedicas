using System.Globalization;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Correos;

/// <summary>
/// Deja un rastro en texto plano (logs/smtp.log) de cada intento de conexión SMTP -sea la prueba
/// de configuración o el envío real de un correo- para poder diagnosticar errores después de que
/// ocurrieron. Ver capability "diagnostico-smtp" y proposal.md del cambio
/// "agregar-log-diagnostico-smtp". Nunca registra la contraseña SMTP.
/// </summary>
public sealed class SmtpDiagnosticoLog
{
    private static readonly object Candado = new();

    private readonly string _rutaArchivo;

    public SmtpDiagnosticoLog(AppPaths appPaths)
    {
        _rutaArchivo = Path.Combine(appPaths.LogsDir, "smtp.log");
    }

    public void RegistrarExito(string operacion, string host, int puerto, string usuario, bool usaSsl) =>
        Escribir(operacion, host, puerto, usuario, usaSsl, exito: true, detalleError: null);

    public void RegistrarError(string operacion, string host, int puerto, string usuario, bool usaSsl, Exception excepcion) =>
        Escribir(operacion, host, puerto, usuario, usaSsl, exito: false, detalleError: DescribirCausaRaiz(excepcion));

    private void Escribir(string operacion, string host, int puerto, string usuario, bool usaSsl, bool exito, string? detalleError)
    {
        var marcaTiempo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var estado = exito ? "OK" : "ERROR";
        var linea = $"{marcaTiempo} [{estado}] {operacion} host={host}:{puerto} usuario={usuario} ssl={usaSsl}";
        if (detalleError is not null)
            linea += $" error={detalleError}";

        lock (Candado)
            File.AppendAllText(_rutaArchivo, linea + Environment.NewLine);
    }

    /// <summary>Baja hasta la excepción más interna, que suele traer la causa real del fallo SMTP.</summary>
    private static string DescribirCausaRaiz(Exception excepcion)
    {
        var causa = excepcion;
        while (causa.InnerException is not null)
            causa = causa.InnerException;

        var mensaje = causa.Message.Replace("\r", " ").Replace("\n", " ");
        return $"{causa.GetType().Name}: {mensaje}";
    }
}
