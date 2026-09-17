using System.Runtime.Versioning;
using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Tests;

[SupportedOSPlatform("windows")]
public sealed class ConfiguracionSmtpServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ConfiguracionSmtpService _servicio;

    public ConfiguracionSmtpServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_smtp_tests_" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(_tempDir);
        var connectionFactory = new SqliteConnectionFactory(paths.DbFilePath);
        using (var connection = connectionFactory.Crear())
            Migrator.Migrar(connection);

        _servicio = new ConfiguracionSmtpService(connectionFactory);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    [Fact]
    public void Obtener_SinConfiguracionGuardada_DevuelveNull()
    {
        Assert.Null(_servicio.Obtener());
    }

    [Fact]
    public void GuardarYObtener_DevuelveLosDatosGuardadosSinLaContrasena()
    {
        _servicio.Guardar("smtp.ejemplo.cl", 587, "usuario@ejemplo.cl", "contrasena-secreta", "avisos@ejemplo.cl", usaSsl: true);

        var configuracion = _servicio.Obtener();

        Assert.NotNull(configuracion);
        Assert.Equal("smtp.ejemplo.cl", configuracion!.Host);
        Assert.Equal(587, configuracion.Puerto);
        Assert.Equal("usuario@ejemplo.cl", configuracion.Usuario);
        Assert.Equal("avisos@ejemplo.cl", configuracion.Remitente);
        Assert.True(configuracion.UsaSsl);
        // ConfiguracionSmtp no tiene una propiedad de contraseña: no hay forma de que la API la exponga.
        Assert.DoesNotContain("Contrasena", typeof(ConfiguracionSmtp).GetProperties().Select(p => p.Name));
    }

    [Fact]
    public void Guardar_DosVeces_SobrescribeLaConfiguracionAnteriorEnVezDeAgregarOtraFila()
    {
        _servicio.Guardar("smtp1.ejemplo.cl", 587, "u1@ejemplo.cl", "clave1", "avisos1@ejemplo.cl", usaSsl: true);
        _servicio.Guardar("smtp2.ejemplo.cl", 25, "u2@ejemplo.cl", "clave2", "avisos2@ejemplo.cl", usaSsl: false);

        var configuracion = _servicio.Obtener();

        Assert.Equal("smtp2.ejemplo.cl", configuracion!.Host);
        Assert.Equal(25, configuracion.Puerto);
        Assert.False(configuracion.UsaSsl);
    }

    [Theory]
    [InlineData("", 587, "usuario", "clave", "remitente@ejemplo.cl")]
    [InlineData("smtp.ejemplo.cl", 0, "usuario", "clave", "remitente@ejemplo.cl")]
    [InlineData("smtp.ejemplo.cl", 587, "", "clave", "remitente@ejemplo.cl")]
    [InlineData("smtp.ejemplo.cl", 587, "usuario", "", "remitente@ejemplo.cl")]
    [InlineData("smtp.ejemplo.cl", 587, "usuario", "clave", "")]
    public void Guardar_ConCampoObligatorioFaltante_RechazaSinModificarLaConfiguracionExistente(
        string host, int puerto, string usuario, string contrasena, string remitente)
    {
        _servicio.Guardar("smtp-original.ejemplo.cl", 587, "original@ejemplo.cl", "clave-original", "avisos@ejemplo.cl", usaSsl: true);

        Assert.Throws<ConfiguracionSmtpInvalidaException>(
            () => _servicio.Guardar(host, puerto, usuario, contrasena, remitente, usaSsl: true));

        Assert.Equal("smtp-original.ejemplo.cl", _servicio.Obtener()!.Host);
    }

    [Fact]
    public void ProbarConexion_ServidorInalcanzable_DevuelveFallaConMensaje()
    {
        // Puerto cerrado en loopback: falla rápido con "conexión rechazada", sin depender de red externa.
        var resultado = _servicio.ProbarConexion("127.0.0.1", 65530, "usuario", "clave", usaSsl: false);

        Assert.False(resultado.Exito);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Error));
    }
}
