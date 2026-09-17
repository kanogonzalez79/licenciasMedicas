using System.Runtime.Versioning;
using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Tests;

[SupportedOSPlatform("windows")]
public sealed class CorreoCopiaServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly CorreoCopiaService _servicio;

    public CorreoCopiaServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_correocopia_tests_" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(_tempDir);
        var connectionFactory = new SqliteConnectionFactory(paths.DbFilePath);
        using (var connection = connectionFactory.Crear())
            Migrator.Migrar(connection);

        _servicio = new CorreoCopiaService(connectionFactory);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    [Fact]
    public void Agregar_CorreoValidoYNuevo_QuedaListadoComoActivo()
    {
        _servicio.Agregar("copia@ejemplo.cl");

        var lista = _servicio.Listar();

        Assert.Single(lista);
        Assert.Equal("copia@ejemplo.cl", lista[0].CorreoElectronico);
        Assert.True(lista[0].Activo);
    }

    [Theory]
    [InlineData("no-es-un-correo")]
    [InlineData("")]
    [InlineData("   ")]
    public void Agregar_ConFormatoInvalido_LanzaCorreoCopiaInvalidoExceptionSinModificarLaLista(string correo)
    {
        Assert.Throws<CorreoCopiaInvalidoException>(() => _servicio.Agregar(correo));

        Assert.Empty(_servicio.Listar());
    }

    [Fact]
    public void Agregar_CorreoQueYaExisteActivo_LanzaCorreoCopiaDuplicadoException()
    {
        _servicio.Agregar("copia@ejemplo.cl");

        Assert.Throws<CorreoCopiaDuplicadoException>(() => _servicio.Agregar("copia@ejemplo.cl"));
        Assert.Single(_servicio.Listar());
    }

    [Fact]
    public void Agregar_CorreoQueYaExisteInactivo_LanzaCorreoCopiaDuplicadoExceptionSinReactivarlo()
    {
        var agregado = _servicio.Agregar("copia@ejemplo.cl");
        _servicio.CambiarActivo(agregado.CorreoCopiaId, activo: false);

        Assert.Throws<CorreoCopiaDuplicadoException>(() => _servicio.Agregar("copia@ejemplo.cl"));

        var lista = _servicio.Listar();
        Assert.Single(lista);
        Assert.False(lista[0].Activo);
    }

    [Fact]
    public void Agregar_MismoCorreoConMayusculasDistintas_TambienSeConsideraDuplicado()
    {
        _servicio.Agregar("copia@ejemplo.cl");

        Assert.Throws<CorreoCopiaDuplicadoException>(() => _servicio.Agregar("COPIA@ejemplo.cl"));
    }

    [Fact]
    public void CambiarActivo_DesactivarYReactivar_ConservaElRegistroEnLaLista()
    {
        var agregado = _servicio.Agregar("copia@ejemplo.cl");

        _servicio.CambiarActivo(agregado.CorreoCopiaId, activo: false);
        Assert.False(_servicio.Listar()[0].Activo);

        _servicio.CambiarActivo(agregado.CorreoCopiaId, activo: true);
        Assert.True(_servicio.Listar()[0].Activo);
    }

    [Fact]
    public void Eliminar_QuitaElRegistroYPermiteAgregarLaMismaDireccionDeNuevo()
    {
        var agregado = _servicio.Agregar("copia@ejemplo.cl");

        _servicio.Eliminar(agregado.CorreoCopiaId);
        Assert.Empty(_servicio.Listar());

        _servicio.Agregar("copia@ejemplo.cl");
        Assert.Single(_servicio.Listar());
    }
}
