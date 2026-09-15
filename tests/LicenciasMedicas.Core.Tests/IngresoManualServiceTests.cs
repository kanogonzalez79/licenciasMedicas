using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Procesamiento;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Core.Tests;

public sealed class IngresoManualServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo = new();
    private readonly StagingRepository _stagingRepo = new();
    private readonly UnidadesRepository _unidadesRepo = new();
    private readonly IngresoManualService _servicio;
    private readonly int _unidadId;

    public IngresoManualServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(_paths.DbFilePath);

        using (var connection = _connectionFactory.Crear())
        {
            Migrator.Migrar(connection);
            _unidadId = _unidadesRepo.Crear(connection, "Unidad de Prueba", "prueba@ejemplo.cl");
        }

        _servicio = new IngresoManualService(_paths, _connectionFactory, _licenciasRepo, _stagingRepo, _unidadesRepo);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private DatosLicenciaManual DatosValidos(string folio = "FOLIO-1", int unidadId = -1) => new()
    {
        Folio = folio,
        RutPaciente = "17000494-9",
        NombreCompletoPaciente = "Paciente de Prueba",
        CodigoTipoLicencia = 1,
        FechaInicioReposo = new DateOnly(2026, 1, 1),
        FechaTerminoReposo = new DateOnly(2026, 1, 5),
        CantidadDias = 5,
        UnidadId = unidadId == -1 ? _unidadId : unidadId,
    };

    private static byte[] ContenidoDePrueba(string texto = "contenido de prueba") => System.Text.Encoding.UTF8.GetBytes(texto);

    [Fact]
    public void Ingresar_DatosValidos_GrabaLicenciaManualYArchivaElAdjunto()
    {
        var resultado = _servicio.Ingresar(DatosValidos(), ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.True(resultado.Exito);
        Assert.NotNull(resultado.LicenciaId);

        using var connection = _connectionFactory.Crear();
        var licencia = _licenciasRepo.ObtenerPorId(connection, resultado.LicenciaId!.Value);
        Assert.NotNull(licencia);
        Assert.Equal((int)TipoFormulario.Manual, licencia!.TipoFormulario);
        Assert.True(licencia.EsIngresoManual);

        var archivosArchivados = Directory.GetFiles(_paths.ArchivoDir, "*.pdf", SearchOption.AllDirectories);
        Assert.Single(archivosArchivados);

        var busqueda = _licenciasRepo.Buscar(connection, new BusquedaLicenciasFiltro());
        Assert.Equal(1, busqueda.Total);
        Assert.True(busqueda.Items.Single().EsIngresoManual);
    }

    [Fact]
    public void Ingresar_ConImagenComoAdjunto_PreservaLaExtensionOriginal()
    {
        var resultado = _servicio.Ingresar(DatosValidos(), ContenidoDePrueba(), ".jpg", "foto.jpg");

        Assert.True(resultado.Exito);
        var archivosArchivados = Directory.GetFiles(_paths.ArchivoDir, "*.jpg", SearchOption.AllDirectories);
        Assert.Single(archivosArchivados);
    }

    [Fact]
    public void Ingresar_ArchivaConNombrePacienteEnTituloCaseYFolioEnElNombreDeArchivo()
    {
        var datos = DatosValidos("FOLIO-7");
        var conNombreEnMayusculas = new DatosLicenciaManual
        {
            Folio = datos.Folio,
            RutPaciente = datos.RutPaciente,
            NombreCompletoPaciente = "JOSÉ ANTONIO NÚÑEZ",
            CodigoTipoLicencia = datos.CodigoTipoLicencia,
            FechaInicioReposo = datos.FechaInicioReposo,
            FechaTerminoReposo = datos.FechaTerminoReposo,
            CantidadDias = datos.CantidadDias,
            UnidadId = datos.UnidadId,
        };

        var resultado = _servicio.Ingresar(conNombreEnMayusculas, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.True(resultado.Exito);
        var archivo = Directory.GetFiles(_paths.ArchivoDir, "*.pdf", SearchOption.AllDirectories).Single();
        Assert.Equal("José Antonio Núñez - FOLIO-7.pdf", Path.GetFileName(archivo));
    }

    [Fact]
    public void Ingresar_FolioVacio_RechazaPorFolioRequerido()
    {
        var resultado = _servicio.Ingresar(
            new DatosLicenciaManual
            {
                Folio = "   ",
                RutPaciente = "17000494-9",
                NombreCompletoPaciente = "Paciente de Prueba",
                CodigoTipoLicencia = 1,
                FechaInicioReposo = new DateOnly(2026, 1, 1),
                FechaTerminoReposo = new DateOnly(2026, 1, 5),
                CantidadDias = 5,
                UnidadId = _unidadId,
            },
            ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.FolioRequerido, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_RutPacienteInvalido_RechazaPorRutInvalido()
    {
        var datos = new DatosLicenciaManual
        {
            Folio = "FOLIO-2",
            RutPaciente = "11111111-2",
            NombreCompletoPaciente = "Paciente de Prueba",
            CodigoTipoLicencia = 1,
            FechaInicioReposo = new DateOnly(2026, 1, 1),
            FechaTerminoReposo = new DateOnly(2026, 1, 5),
            CantidadDias = 5,
            UnidadId = _unidadId,
        };

        var resultado = _servicio.Ingresar(datos, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosFallo.RutPacienteInvalido, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_CodigoTipoLicenciaInvalido_RechazaPorTipoLicenciaInvalido()
    {
        var datos = new DatosLicenciaManual
        {
            Folio = "FOLIO-3",
            RutPaciente = "17000494-9",
            NombreCompletoPaciente = "Paciente de Prueba",
            CodigoTipoLicencia = 99,
            FechaInicioReposo = new DateOnly(2026, 1, 1),
            FechaTerminoReposo = new DateOnly(2026, 1, 5),
            CantidadDias = 5,
            UnidadId = _unidadId,
        };

        var resultado = _servicio.Ingresar(datos, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosFallo.TipoLicenciaInvalido, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_FechaTerminoAntesQueInicio_RechazaPorFechasInvalidas()
    {
        var datos = new DatosLicenciaManual
        {
            Folio = "FOLIO-4",
            RutPaciente = "17000494-9",
            NombreCompletoPaciente = "Paciente de Prueba",
            CodigoTipoLicencia = 1,
            FechaInicioReposo = new DateOnly(2026, 1, 5),
            FechaTerminoReposo = new DateOnly(2026, 1, 1),
            CantidadDias = 5,
            UnidadId = _unidadId,
        };

        var resultado = _servicio.Ingresar(datos, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosFallo.FechasInvalidas, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_CantidadDiasNoCoincideConFechas_RechazaPorDiasInvalidos()
    {
        var datos = new DatosLicenciaManual
        {
            Folio = "FOLIO-5",
            RutPaciente = "17000494-9",
            NombreCompletoPaciente = "Paciente de Prueba",
            CodigoTipoLicencia = 1,
            FechaInicioReposo = new DateOnly(2026, 1, 1),
            FechaTerminoReposo = new DateOnly(2026, 1, 5),
            CantidadDias = 3,
            UnidadId = _unidadId,
        };

        var resultado = _servicio.Ingresar(datos, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosFallo.DiasInvalidos, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_RutProfesionalInvalido_RechazaPorRutProfesionalInvalido()
    {
        var datos = new DatosLicenciaManual
        {
            Folio = "FOLIO-6",
            RutPaciente = "17000494-9",
            NombreCompletoPaciente = "Paciente de Prueba",
            CodigoTipoLicencia = 1,
            FechaInicioReposo = new DateOnly(2026, 1, 1),
            FechaTerminoReposo = new DateOnly(2026, 1, 5),
            CantidadDias = 5,
            RutProfesional = "11111111-2",
            UnidadId = _unidadId,
        };

        var resultado = _servicio.Ingresar(datos, ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.RutProfesionalInvalido, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_AdjuntoVacio_RechazaPorAdjuntoRequerido()
    {
        var resultado = _servicio.Ingresar(DatosValidos(), Array.Empty<byte>(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.AdjuntoRequerido, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_UnidadInexistente_RechazaPorFaltaUnidad()
    {
        var resultado = _servicio.Ingresar(DatosValidos(unidadId: 999), ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.FaltaUnidad, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_FolioYaGrabado_RechazaPorDuplicadoFolio()
    {
        _servicio.Ingresar(DatosValidos("FOLIO-DUP"), ContenidoDePrueba("archivo A"), ".pdf", "a.pdf");

        var resultado = _servicio.Ingresar(DatosValidos("FOLIO-DUP"), ContenidoDePrueba("archivo B"), ".pdf", "b.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.DuplicadoFolioYaExistente, resultado.Motivo);
        // El segundo archivo no debe quedar huérfano en el directorio de archivo permanente.
        Assert.Single(Directory.GetFiles(_paths.ArchivoDir, "*.pdf", SearchOption.AllDirectories));
    }

    [Fact]
    public void Ingresar_MismoAdjuntoYaGrabado_RechazaPorDuplicadoYaProcesado()
    {
        var contenido = ContenidoDePrueba("mismo archivo");
        _servicio.Ingresar(DatosValidos("FOLIO-HASH-1"), contenido, ".pdf", "a.pdf");

        var resultado = _servicio.Ingresar(DatosValidos("FOLIO-HASH-2"), contenido, ".pdf", "b.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.DuplicadoYaProcesado, resultado.Motivo);
    }

    [Fact]
    public void Ingresar_FolioPendienteEnStaging_RechazaPorDuplicadoFolio()
    {
        using (var connection = _connectionFactory.Crear())
        {
            var loteId = _stagingRepo.CrearLote(connection);
            _stagingRepo.InsertarRevision(connection, new LicenciaRevision
            {
                LoteId = loteId,
                NombreArchivoOriginal = "pendiente.pdf",
                RutaStaging = Path.Combine(_tempDir, "pendiente.pdf"),
                HashArchivoSha256 = "hash-distinto",
                Folio = "FOLIO-STAGING",
                TipoFormulario = 1,
                RutPacienteSinDv = "11111111",
                DvPaciente = "1",
                NombreCompletoPaciente = "Otro Paciente",
                FechaInicioReposo = "2026-01-01",
                FechaTerminoReposo = "2026-01-05",
                CantidadDias = 5,
            });
        }

        var resultado = _servicio.Ingresar(DatosValidos("FOLIO-STAGING"), ContenidoDePrueba(), ".pdf", "licencia.pdf");

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosRechazoIngresoManual.DuplicadoFolioYaExistente, resultado.Motivo);
    }
}
