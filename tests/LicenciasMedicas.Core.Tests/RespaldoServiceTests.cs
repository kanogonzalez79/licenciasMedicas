using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Respaldo;
using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Tests;

public sealed class RespaldoServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _carpetaDestino;
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly RespaldoService _servicio;

    public RespaldoServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_respaldo_tests_" + Guid.NewGuid().ToString("N"));
        _paths = new AppPaths(Path.Combine(_tempDir, "app"));
        _carpetaDestino = Path.Combine(_tempDir, "destino");
        Directory.CreateDirectory(_carpetaDestino);

        _connectionFactory = new SqliteConnectionFactory(_paths.DbFilePath);
        using (var connection = _connectionFactory.Crear())
            Migrator.Migrar(connection);

        _servicio = new RespaldoService(_paths, _connectionFactory);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private void EscribirPdfOrigen(string rutaRelativa, string contenido = "contenido")
    {
        var ruta = Path.Combine(_paths.ArchivoDir, rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, contenido);
    }

    private void EscribirPdfDestino(string rutaRelativa, string contenido = "contenido")
    {
        var ruta = Path.Combine(_carpetaDestino, "archivo", rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, contenido);
    }

    [Fact]
    public void Respaldar_TomaSnapshotValidoYLegibleDeLaBaseDeDatos()
    {
        _servicio.Respaldar(_carpetaDestino);

        var rutaDb = Path.Combine(_carpetaDestino, "licencias.db");
        Assert.True(File.Exists(rutaDb));

        using var connection = new SqliteConnection($"Data Source={rutaDb}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Licencias;";
        cmd.ExecuteScalar(); // no debe lanzar si el archivo es una base SQLite válida y con el esquema migrado
    }

    [Fact]
    public void Respaldar_SegundaVezSobrescribeLaBaseDeDatosAnteriorSinDejarTemporales()
    {
        _servicio.Respaldar(_carpetaDestino);
        _servicio.Respaldar(_carpetaDestino);

        var archivosDb = Directory.GetFiles(_carpetaDestino, "licencias.db*");
        Assert.Equal(new[] { Path.Combine(_carpetaDestino, "licencias.db") }, archivosDb);
    }

    [Fact]
    public void Respaldar_ConLaAppUsandoLaBaseDeDatos_NoFallaNiCorrompeLaCopia()
    {
        using var conexionAbierta = _connectionFactory.Crear(); // simula la app en uso durante el respaldo

        _servicio.Respaldar(_carpetaDestino);

        var rutaDb = Path.Combine(_carpetaDestino, "licencias.db");
        using var connection = new SqliteConnection($"Data Source={rutaDb}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA integrity_check;";
        Assert.Equal("ok", (string)cmd.ExecuteScalar()!);
    }

    [Fact]
    public void DiferenciasPdfs_IdentificaFaltantesEnDestinoYSobrantesQueYaNoEstanEnOrigen()
    {
        EscribirPdfOrigen(Path.Combine("2026", "09", "nuevo.pdf"));
        EscribirPdfOrigen(Path.Combine("2026", "09", "existente.pdf"));
        EscribirPdfDestino(Path.Combine("2026", "09", "existente.pdf"));
        EscribirPdfDestino(Path.Combine("2026", "08", "viejo.pdf"));

        var (faltantes, sobrantes) = _servicio.DiferenciasPdfs(_carpetaDestino);

        Assert.Equal(new[] { Path.Combine("2026", "09", "nuevo.pdf") }, faltantes);
        Assert.Equal(new[] { Path.Combine("2026", "08", "viejo.pdf") }, sobrantes);
    }

    [Fact]
    public void Respaldar_SoloCopiaLosPdfsFaltantesSinRecopiarLosYaPresentes()
    {
        EscribirPdfOrigen(Path.Combine("2026", "09", "existente.pdf"), "version-origen");
        EscribirPdfOrigen(Path.Combine("2026", "09", "nuevo.pdf"), "nuevo");
        EscribirPdfDestino(Path.Combine("2026", "09", "existente.pdf"), "version-destino-no-debe-cambiar");

        var resultado = _servicio.Respaldar(_carpetaDestino);

        Assert.Equal(1, resultado.PdfsCopiados);
        Assert.Equal("version-destino-no-debe-cambiar", File.ReadAllText(Path.Combine(_carpetaDestino, "archivo", "2026", "09", "existente.pdf")));
        Assert.Equal("nuevo", File.ReadAllText(Path.Combine(_carpetaDestino, "archivo", "2026", "09", "nuevo.pdf")));
    }

    [Fact]
    public void Respaldar_EliminaEnDestinoLosPdfsQueYaNoExistenEnOrigen()
    {
        EscribirPdfDestino(Path.Combine("2026", "08", "eliminado.pdf"));

        var resultado = _servicio.Respaldar(_carpetaDestino);

        Assert.Equal(1, resultado.PdfsEliminados);
        Assert.False(File.Exists(Path.Combine(_carpetaDestino, "archivo", "2026", "08", "eliminado.pdf")));
    }

    [Fact]
    public void Respaldar_CarpetaDestinoInexistente_LanzaExcepcionSinCrearNiModificarNada()
    {
        var carpetaInexistente = Path.Combine(_tempDir, "no-existe");

        Assert.Throws<CarpetaDestinoNoDisponibleException>(() => _servicio.Respaldar(carpetaInexistente));
        Assert.False(Directory.Exists(carpetaInexistente));
    }

    [Fact]
    public void Respaldar_SiFallaAlEliminarSobrantes_YaDejaCopiadosLosPdfsNuevosYActualizadaLaBaseDeDatos()
    {
        EscribirPdfOrigen(Path.Combine("2026", "09", "nuevo.pdf"));
        EscribirPdfDestino(Path.Combine("2026", "08", "no-se-puede-borrar.pdf"));
        var rutaBloqueada = Path.Combine(_carpetaDestino, "archivo", "2026", "08", "no-se-puede-borrar.pdf");
        File.SetAttributes(rutaBloqueada, FileAttributes.ReadOnly);

        try
        {
            Assert.Throws<CarpetaDestinoNoDisponibleException>(() => _servicio.Respaldar(_carpetaDestino));

            // El orden copiar/sobrescribir-antes-de-borrar (ver design.md) implica que, si falla
            // justo en el paso de borrado, lo ya copiado y la base de datos quedan de todas formas.
            Assert.True(File.Exists(Path.Combine(_carpetaDestino, "archivo", "2026", "09", "nuevo.pdf")));
            Assert.True(File.Exists(Path.Combine(_carpetaDestino, "licencias.db")));
            Assert.True(File.Exists(rutaBloqueada));
        }
        finally
        {
            File.SetAttributes(rutaBloqueada, FileAttributes.Normal);
        }
    }
}
