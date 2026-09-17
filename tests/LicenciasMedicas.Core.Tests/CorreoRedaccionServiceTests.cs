using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Unidades;
using Xunit.Abstractions;

namespace LicenciasMedicas.Core.Tests;

public sealed class CorreoRedaccionServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo = new();
    private readonly CorreoRedaccionService _service;
    private readonly int _unidadId;
    private readonly ITestOutputHelper _output;

    public CorreoRedaccionServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_correo_redaccion_tests_" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(paths.DbFilePath);

        using var connection = _connectionFactory.Crear();
        Migrator.Migrar(connection);
        _unidadId = new UnidadesRepository().Crear(connection, "Unidad de Prueba", "prueba@ejemplo.cl");

        _service = new CorreoRedaccionService(paths, _connectionFactory, _licenciasRepo);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private void GrabarLicenciaDePrueba(string folio, string nombre, int cantidadDias)
    {
        using var connection = _connectionFactory.Crear();
        _licenciasRepo.Insertar(connection, new Licencia
        {
            Folio = folio,
            TipoFormulario = 1,
            RutPacienteSinDv = "11111111",
            DvPaciente = "1",
            NombreCompletoPaciente = nombre,
            FechaInicioReposo = "2026-01-01",
            FechaTerminoReposo = "2026-01-05",
            CantidadDias = cantidadDias,
            UnidadId = _unidadId,
            RutaPdfArchivado = "prueba.pdf",
            NombreArchivoOriginal = "prueba.pdf",
            HashArchivoSha256 = "hash-" + folio,
        });
    }

    [Fact]
    public void Redactar_ConNombresDeLargoVariable_GeneraTablaAlineada()
    {
        GrabarLicenciaDePrueba("FOLIO-1", "Juan Pérez", 5);
        GrabarLicenciaDePrueba("FOLIO-2", "María José González Ibáñez", 12);
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var texto = _service.Redactar(_unidadId, hoy);

        Assert.NotNull(texto);
        _output.WriteLine(texto);

        var lineas = texto!.TrimEnd('\n', '\r').Split('\n');
        // saludo, línea en blanco, encabezado, separador, fila 1, fila 2.
        Assert.Equal(6, lineas.Length);

        var encabezado = lineas[2];
        var separador = lineas[3];
        var fila1 = lineas[4];
        var fila2 = lineas[5];

        // Todas las líneas de la tabla tienen exactamente el mismo largo.
        Assert.Equal(encabezado.TrimEnd('\r').Length, separador.TrimEnd('\r').Length);
        Assert.Equal(encabezado.TrimEnd('\r').Length, fila1.TrimEnd('\r').Length);
        Assert.Equal(encabezado.TrimEnd('\r').Length, fila2.TrimEnd('\r').Length);

        // La línea separadora es solo guiones y espacios.
        Assert.Matches("^[- ]+$", separador.TrimEnd('\r'));

        // Cada columna empieza en la misma posición de carácter en encabezado y filas
        // (se ubica por el inicio de cada valor conocido).
        Assert.Equal(0, fila1.IndexOf("11111111-1", StringComparison.Ordinal));
        Assert.Equal(0, encabezado.IndexOf("RUT", StringComparison.Ordinal));

        var posicionColumnaNombreEnEncabezado = encabezado.IndexOf("Nombre", StringComparison.Ordinal);
        Assert.Equal(posicionColumnaNombreEnEncabezado, fila1.IndexOf("Juan Pérez", StringComparison.Ordinal));
        Assert.Equal(posicionColumnaNombreEnEncabezado, fila2.IndexOf("María José González Ibáñez", StringComparison.Ordinal));
    }

    [Fact]
    public void Redactar_ConUnaSolaLicencia_ColumnaUsaAnchoDelEncabezadoSiEsMayor()
    {
        GrabarLicenciaDePrueba("FOLIO-1", "Ana Ruiz", 3);
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var texto = _service.Redactar(_unidadId, hoy);

        Assert.NotNull(texto);
        var lineas = texto!.TrimEnd('\n', '\r').Split('\n');
        var encabezado = lineas[2].TrimEnd('\r');
        var separador = lineas[3].TrimEnd('\r');
        var fila = lineas[4].TrimEnd('\r');

        Assert.Equal(encabezado.Length, separador.Length);
        Assert.Equal(encabezado.Length, fila.Length);

        // La columna "RUT" (encabezado de 3 caracteres) es más ancha que el valor "12345678-1"
        // (10 caracteres), así que en este caso manda el valor, no el encabezado.
        var posicionNombre = encabezado.IndexOf("Nombre", StringComparison.Ordinal);
        Assert.Equal(posicionNombre, fila.IndexOf("Ana Ruiz", StringComparison.Ordinal));
    }

    [Fact]
    public void Redactar_UnidadSinLicenciasEnLaFecha_DevuelveNull()
    {
        var fechaSinLicencias = DateOnly.FromDateTime(DateTime.Now).AddDays(-30);

        var texto = _service.Redactar(_unidadId, fechaSinLicencias);

        Assert.Null(texto);
    }
}
