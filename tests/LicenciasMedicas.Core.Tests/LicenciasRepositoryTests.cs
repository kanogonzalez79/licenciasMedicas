using Dapper;
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
        return GrabarLicenciaDePrueba(folio, "2026-01-01", "2026-01-05");
    }

    private int GrabarLicenciaDePrueba(string folio, string fechaInicioReposo, string fechaTerminoReposo)
    {
        using var connection = _connectionFactory.Crear();
        var licencia = new Licencia
        {
            Folio = folio,
            TipoFormulario = 1,
            RutPacienteSinDv = "11111111",
            DvPaciente = "1",
            NombreCompletoPaciente = "Paciente de Prueba",
            FechaInicioReposo = fechaInicioReposo,
            FechaTerminoReposo = fechaTerminoReposo,
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

    [Fact]
    public void UnidadesConLicenciasGrabadasEnFecha_ConEnvioRegistrado_IncluyeUltimoEnvio()
    {
        GrabarLicenciaDePrueba("FOLIO-ENVIO");
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        using var connection = _connectionFactory.Crear();
        connection.Execute(
            "INSERT INTO CorreosEnviados (UnidadId, Fecha, FechaHoraEnvio) VALUES (@UnidadId, @Fecha, @FechaHoraEnvio);",
            new { UnidadId = _unidadId, Fecha = hoy.ToString("yyyy-MM-dd"), FechaHoraEnvio = "2026-01-01T10:00:00.000Z" });

        var resultado = _licenciasRepo.UnidadesConLicenciasGrabadasEnFecha(connection, hoy);

        var unidad = Assert.Single(resultado);
        Assert.Equal("2026-01-01T10:00:00.000Z", unidad.UltimoEnvio);
    }

    [Fact]
    public void UnidadesConLicenciasGrabadasEnFecha_SinEnvioRegistrado_UltimoEnvioEsNull()
    {
        GrabarLicenciaDePrueba("FOLIO-SIN-ENVIO");
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        using var connection = _connectionFactory.Crear();
        var resultado = _licenciasRepo.UnidadesConLicenciasGrabadasEnFecha(connection, hoy);

        var unidad = Assert.Single(resultado);
        Assert.Null(unidad.UltimoEnvio);
    }

    [Fact]
    public void ObtenerParaInforme_ModoFechaInicio_SoloIncluyeLicenciasConInicioEnElRango()
    {
        GrabarLicenciaDePrueba("INICIO-DENTRO", "2026-02-15", "2026-02-25");
        GrabarLicenciaDePrueba("INICIO-FUERA", "2026-02-05", "2026-02-15");

        using var connection = _connectionFactory.Crear();
        var resultado = _licenciasRepo.ObtenerParaInforme(
            connection, new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 20), ModoFechaInforme.FechaInicio);

        var folio = Assert.Single(resultado);
        Assert.Equal("INICIO-DENTRO", folio.Folio);
    }

    [Fact]
    public void ObtenerParaInforme_ModoFechaTermino_SoloIncluyeLicenciasConTerminoEnElRango()
    {
        GrabarLicenciaDePrueba("TERMINO-DENTRO", "2026-02-05", "2026-02-15");
        GrabarLicenciaDePrueba("TERMINO-FUERA", "2026-02-15", "2026-02-25");

        using var connection = _connectionFactory.Crear();
        var resultado = _licenciasRepo.ObtenerParaInforme(
            connection, new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 20), ModoFechaInforme.FechaTermino);

        var folio = Assert.Single(resultado);
        Assert.Equal("TERMINO-DENTRO", folio.Folio);
    }

    [Fact]
    public void ObtenerParaInforme_ModoInterseccion_IncluyeSoloLicenciasQueSeSolapanConElRango()
    {
        // Cubre por completo el rango consultado: ni el inicio ni el término caen dentro de él.
        GrabarLicenciaDePrueba("CUBRE-TODO", "2026-02-01", "2026-02-28");
        // Se solapa parcialmente al inicio del rango.
        GrabarLicenciaDePrueba("SOLAPA-INICIO", "2026-02-05", "2026-02-12");
        // Se solapa parcialmente al final del rango.
        GrabarLicenciaDePrueba("SOLAPA-FINAL", "2026-02-18", "2026-02-25");
        // Termina antes de que comience el rango consultado: sin ningún día en común.
        GrabarLicenciaDePrueba("SIN-SOLAPE-ANTES", "2026-01-01", "2026-02-05");
        // Comienza después de que termina el rango consultado: sin ningún día en común.
        GrabarLicenciaDePrueba("SIN-SOLAPE-DESPUES", "2026-02-25", "2026-03-01");

        using var connection = _connectionFactory.Crear();
        var resultado = _licenciasRepo.ObtenerParaInforme(
            connection, new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 20), ModoFechaInforme.Interseccion);

        var folios = resultado.Select(l => l.Folio).ToList();
        Assert.Equal(
            new[] { "CUBRE-TODO", "SOLAPA-INICIO", "SOLAPA-FINAL" }.OrderBy(f => f),
            folios.OrderBy(f => f));
    }

    [Fact]
    public void ObtenerParaInforme_SinLicenciasQueCalifiquen_DevuelveListaVacia()
    {
        GrabarLicenciaDePrueba("FUERA-DE-RANGO", "2026-01-01", "2026-01-05");

        using var connection = _connectionFactory.Crear();
        var resultado = _licenciasRepo.ObtenerParaInforme(
            connection, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), ModoFechaInforme.Interseccion);

        Assert.Empty(resultado);
    }
}
