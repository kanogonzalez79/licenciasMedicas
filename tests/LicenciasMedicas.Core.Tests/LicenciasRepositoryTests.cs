using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Procesamiento;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Core.Tests;

public sealed class LicenciasRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo = new();
    private readonly int _unidadId;

    public LicenciasRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(_paths.DbFilePath);

        using var connection = _connectionFactory.Crear();
        Migrator.Migrar(connection);
        _unidadId = new UnidadesRepository().Crear(connection, "Unidad de Prueba", "prueba@ejemplo.cl");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private int GrabarLicenciaDePrueba(string folio)
    {
        using var connection = _connectionFactory.Crear();
        var licencia = new Licencia
        {
            Folio = folio,
            TipoFormulario = 1,
            RutPacienteSinDv = "11111111",
            DvPaciente = "1",
            NombreCompletoPaciente = "Paciente de Prueba",
            FechaInicioReposo = "2026-01-01",
            FechaTerminoReposo = "2026-01-05",
            CantidadDias = 5,
            UnidadId = _unidadId,
            RutaPdfArchivado = "prueba.pdf",
            NombreArchivoOriginal = "prueba.pdf",
            HashArchivoSha256 = "hash-" + folio,
        };
        return _licenciasRepo.Insertar(connection, licencia);
    }

    [Fact]
    public void Eliminar_LicenciaExistente_LaBorraYLibraElFolio()
    {
        var licenciaId = GrabarLicenciaDePrueba("FOLIO-1");

        using var connection = _connectionFactory.Crear();
        var eliminada = _licenciasRepo.Eliminar(connection, licenciaId);

        Assert.True(eliminada);
        Assert.Null(_licenciasRepo.ObtenerPorId(connection, licenciaId));

        // El folio queda libre: se puede volver a grabar una licencia con el mismo folio.
        var nuevaLicenciaId = GrabarLicenciaDePrueba("FOLIO-1");
        Assert.NotEqual(licenciaId, nuevaLicenciaId);
    }

    [Fact]
    public void Eliminar_LicenciaInexistente_DevuelveFalse()
    {
        using var connection = _connectionFactory.Crear();
        var eliminada = _licenciasRepo.Eliminar(connection, 999);

        Assert.False(eliminada);
    }
}
