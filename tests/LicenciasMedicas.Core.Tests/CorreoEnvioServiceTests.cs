using System.Runtime.Versioning;
using Dapper;
using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Core.Tests;

[SupportedOSPlatform("windows")]
public sealed class CorreoEnvioServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ConfiguracionSmtpService _configuracionSmtp;
    private readonly CorreoCopiaService _correoCopia;
    private readonly CorreoEnvioService _servicio;
    private readonly AppPaths _paths;
    private readonly int _unidadId;

    public CorreoEnvioServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_envio_tests_" + Guid.NewGuid().ToString("N"));
        _paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(_paths.DbFilePath);

        using (var connection = _connectionFactory.Crear())
        {
            Migrator.Migrar(connection);
            _unidadId = new UnidadesRepository().Crear(connection, "Unidad de Prueba", "unidad@ejemplo.cl");
        }

        _configuracionSmtp = new ConfiguracionSmtpService(_connectionFactory, new SmtpDiagnosticoLog(_paths));
        _correoCopia = new CorreoCopiaService(_connectionFactory);
        _servicio = new CorreoEnvioService(
            _connectionFactory, new UnidadesRepository(), _configuracionSmtp, _correoCopia, new SmtpDiagnosticoLog(_paths));
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    [Fact]
    public void Enviar_SinConfiguracionSmtpGuardada_LanzaConfiguracionSmtpNoConfiguradaException()
    {
        Assert.Throws<ConfiguracionSmtpNoConfiguradaException>(
            () => _servicio.Enviar(_unidadId, DateOnly.FromDateTime(DateTime.Now), "texto"));
    }

    [Fact]
    public void Enviar_UnidadSinCorreoValido_LanzaUnidadSinCorreoValidoException()
    {
        _configuracionSmtp.Guardar("smtp.ejemplo.cl", 587, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: true);

        int unidadSinCorreoId;
        using (var connection = _connectionFactory.Crear())
        {
            connection.Execute(
                "INSERT INTO Unidades (Descripcion, CorreoElectronico) VALUES ('Unidad Sin Correo', NULL);");
            unidadSinCorreoId = connection.QuerySingle<int>("SELECT last_insert_rowid();");
        }

        Assert.Throws<UnidadSinCorreoValidoException>(
            () => _servicio.Enviar(unidadSinCorreoId, DateOnly.FromDateTime(DateTime.Now), "texto"));
    }

    [Fact]
    public void Enviar_ConServidorSmtpFalsoYCredencialesValidas_RegistraElEnvioExitoso()
    {
        using var servidor = new ServidorSmtpFalso();
        _configuracionSmtp.Guardar("127.0.0.1", servidor.Puerto, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: false);
        var fecha = DateOnly.FromDateTime(DateTime.Now);

        _servicio.Enviar(_unidadId, fecha, "Texto de prueba del correo.");

        using var connection = _connectionFactory.Crear();
        var fechaHoraEnvio = connection.QuerySingleOrDefault<string>(
            "SELECT FechaHoraEnvio FROM CorreosEnviados WHERE UnidadId = @unidadId AND Fecha = @fecha;",
            new { unidadId = _unidadId, fecha = fecha.ToString("yyyy-MM-dd") });

        Assert.NotNull(fechaHoraEnvio);
        Assert.Contains("Texto de prueba del correo.", servidor.UltimoMensajeRecibido);
    }

    [Fact]
    public void Enviar_ConServidorSmtpFalsoYCredencialesValidas_RegistraElExitoEnElLogDeDiagnostico()
    {
        using var servidor = new ServidorSmtpFalso();
        _configuracionSmtp.Guardar("127.0.0.1", servidor.Puerto, "usuario", "clave-secreta", "avisos@ejemplo.cl", usaSsl: false);

        _servicio.Enviar(_unidadId, DateOnly.FromDateTime(DateTime.Now), "Texto de prueba del correo.");

        var contenidoLog = File.ReadAllText(Path.Combine(_paths.LogsDir, "smtp.log"));

        Assert.Contains($"[OK] envio host=127.0.0.1:{servidor.Puerto} usuario=usuario", contenidoLog);
        Assert.DoesNotContain("clave-secreta", contenidoLog);
    }

    [Fact]
    public void Enviar_ConServidorInalcanzable_RegistraElErrorEnElLogDeDiagnosticoYPropagaLaExcepcion()
    {
        // Puerto cerrado en loopback: falla rápido con "conexión rechazada", sin depender de red externa.
        _configuracionSmtp.Guardar("127.0.0.1", 65530, "usuario", "clave-secreta", "avisos@ejemplo.cl", usaSsl: false);

        Assert.Throws<EnvioCorreoFallidoException>(
            () => _servicio.Enviar(_unidadId, DateOnly.FromDateTime(DateTime.Now), "Texto de prueba del correo."));

        var contenidoLog = File.ReadAllText(Path.Combine(_paths.LogsDir, "smtp.log"));

        Assert.Contains("[ERROR] envio host=127.0.0.1:65530 usuario=usuario", contenidoLog);
        Assert.DoesNotContain("clave-secreta", contenidoLog);
    }

    [Fact]
    public void Enviar_ConCorreosEnCopiaActivos_LosAgregaComoCcEnElMensaje()
    {
        _correoCopia.Agregar("copia1@ejemplo.cl");
        _correoCopia.Agregar("copia2@ejemplo.cl");
        var copia3 = _correoCopia.Agregar("copia3-inactivo@ejemplo.cl");
        _correoCopia.CambiarActivo(copia3.CorreoCopiaId, activo: false);

        using var servidor = new ServidorSmtpFalso();
        _configuracionSmtp.Guardar("127.0.0.1", servidor.Puerto, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: false);

        _servicio.Enviar(_unidadId, DateOnly.FromDateTime(DateTime.Now), "Texto de prueba del correo.");

        Assert.Contains("Cc: copia1@ejemplo.cl, copia2@ejemplo.cl", servidor.UltimoMensajeRecibido);
        Assert.DoesNotContain("copia3-inactivo@ejemplo.cl", servidor.UltimoMensajeRecibido);
    }

    [Fact]
    public void Enviar_SinCorreosEnCopiaConfigurados_NoIncluyeEncabezadoCc()
    {
        using var servidor = new ServidorSmtpFalso();
        _configuracionSmtp.Guardar("127.0.0.1", servidor.Puerto, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: false);

        _servicio.Enviar(_unidadId, DateOnly.FromDateTime(DateTime.Now), "Texto de prueba del correo.");

        Assert.DoesNotContain("Cc:", servidor.UltimoMensajeRecibido);
    }

    [Fact]
    public void Enviar_ReintentoParaLaMismaUnidadYFecha_ActualizaLaFechaHoraDelEnvioSinDuplicarFila()
    {
        using var servidor = new ServidorSmtpFalso();
        _configuracionSmtp.Guardar("127.0.0.1", servidor.Puerto, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: false);
        var fecha = DateOnly.FromDateTime(DateTime.Now);

        _servicio.Enviar(_unidadId, fecha, "Primer envío.");

        using (var servidor2 = new ServidorSmtpFalso())
        {
            _configuracionSmtp.Guardar("127.0.0.1", servidor2.Puerto, "usuario", "clave", "avisos@ejemplo.cl", usaSsl: false);
            _servicio.Enviar(_unidadId, fecha, "Reenvío.");
        }

        using var connection = _connectionFactory.Crear();
        var cantidadFilas = connection.QuerySingle<int>(
            "SELECT COUNT(1) FROM CorreosEnviados WHERE UnidadId = @unidadId AND Fecha = @fecha;",
            new { unidadId = _unidadId, fecha = fecha.ToString("yyyy-MM-dd") });

        Assert.Equal(1, cantidadFilas);
    }
}
