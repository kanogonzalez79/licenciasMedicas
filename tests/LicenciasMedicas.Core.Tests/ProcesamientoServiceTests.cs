using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Procesamiento;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Core.Tests;

public sealed class ProcesamientoServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly UnidadesRepository _unidadesRepo = new();
    private readonly LicenciasRepository _licenciasRepo = new();
    private readonly StagingRepository _stagingRepo = new();
    private readonly ProcesamientoService _servicio;
    private readonly int _unidadId;

    public ProcesamientoServiceTests()
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

        _servicio = new ProcesamientoService(_paths, _connectionFactory, new LicenciaExtractionService(), _stagingRepo, _licenciasRepo);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private void CopiarFixturesAIncoming(params string[] nombres)
    {
        foreach (var nombre in nombres)
        {
            var origen = Path.Combine(AppContext.BaseDirectory, "Fixtures", nombre);
            File.Copy(origen, Path.Combine(_paths.IncomingDir, nombre));
        }
    }

    [Fact]
    public void Procesar_DosPdfValidos_QuedanPendientesYSalenDeIncoming()
    {
        CopiarFixturesAIncoming("tipo1.pdf", "tipo2.pdf");

        var resultado = _servicio.Procesar();

        Assert.Equal(2, resultado.Pendientes.Count);
        Assert.Empty(resultado.Fallidas);
        Assert.Empty(Directory.GetFiles(_paths.IncomingDir));
    }

    [Fact]
    public void Procesar_LlamadoDosVecesSinGrabar_NoDuplicaFilasPendientes()
    {
        CopiarFixturesAIncoming("tipo1.pdf", "tipo2.pdf");

        _servicio.Procesar();
        var segundaLlamada = _servicio.Procesar();

        Assert.Equal(2, segundaLlamada.Pendientes.Count);
    }

    [Fact]
    public void Grabar_ConUnidadValida_InsertaLicenciaYArchivaPdf()
    {
        CopiarFixturesAIncoming("tipo1.pdf", "tipo2.pdf");
        var pendientes = _servicio.Procesar().Pendientes;

        var asignaciones = pendientes.Select(p => new AsignacionUnidad(p.RevisionId, _unidadId)).ToList();
        var resultado = _servicio.Grabar(asignaciones);

        Assert.Equal(2, resultado.Grabadas.Count);
        Assert.Empty(resultado.NoGrabadas);
        Assert.Empty(_servicio.ObtenerPendientes());

        var archivosArchivados = Directory.GetFiles(_paths.ArchivoDir, "*.pdf", SearchOption.AllDirectories);
        Assert.Equal(2, archivosArchivados.Length);

        using var connection = _connectionFactory.Crear();
        var busqueda = _licenciasRepo.Buscar(connection, new BusquedaLicenciasFiltro());
        Assert.Equal(2, busqueda.Total);
    }

    [Fact]
    public void Grabar_ArchivaConNombrePacienteYFolioEnElNombreDeArchivo()
    {
        CopiarFixturesAIncoming("tipo1.pdf");
        var pendiente = _servicio.Procesar().Pendientes.Single();

        _servicio.Grabar(new[] { new AsignacionUnidad(pendiente.RevisionId, _unidadId) });

        var archivo = Directory.GetFiles(_paths.ArchivoDir, "*.pdf", SearchOption.AllDirectories).Single();
        Assert.Equal("Karina Scarlette Valdivia Oyarce - 24523866-5.pdf", Path.GetFileName(archivo));
    }

    [Fact]
    public void Grabar_ConAlgunasFilasMarcadasComoCorreoEnviado_GrabaCadaValorCorrectamente()
    {
        CopiarFixturesAIncoming("tipo1.pdf", "tipo2.pdf");
        var pendientes = _servicio.Procesar().Pendientes;

        var asignaciones = pendientes
            .Select((p, i) => new AsignacionUnidad(p.RevisionId, _unidadId, CorreoEnviado: i == 0))
            .ToList();
        var resultado = _servicio.Grabar(asignaciones);

        Assert.Equal(2, resultado.Grabadas.Count);

        using var connection = _connectionFactory.Crear();
        var busqueda = _licenciasRepo.Buscar(connection, new BusquedaLicenciasFiltro());
        Assert.Equal(1, busqueda.Items.Count(l => l.CorreoEnviado));
        Assert.Equal(1, busqueda.Items.Count(l => !l.CorreoEnviado));
    }

    [Fact]
    public void Grabar_SinUnidadAsignada_QuedaPendienteConMotivoFaltaUnidad()
    {
        CopiarFixturesAIncoming("tipo1.pdf");
        var pendiente = _servicio.Procesar().Pendientes.Single();

        var resultado = _servicio.Grabar(new[] { new AsignacionUnidad(pendiente.RevisionId, null) });

        Assert.Empty(resultado.Grabadas);
        Assert.Single(resultado.NoGrabadas);
        Assert.Equal("FALTA_UNIDAD", resultado.NoGrabadas[0].Motivo);
        Assert.Single(_servicio.ObtenerPendientes());
    }

    [Fact]
    public void RecuperarHuerfanosAlArranque_DevuelveArchivosPendientesAIncoming()
    {
        CopiarFixturesAIncoming("tipo1.pdf", "tipo2.pdf");
        _servicio.Procesar();
        Assert.Empty(Directory.GetFiles(_paths.IncomingDir));

        _servicio.RecuperarHuerfanosAlArranque();

        Assert.Equal(2, Directory.GetFiles(_paths.IncomingDir, "*.pdf").Length);
        Assert.Empty(_servicio.ObtenerPendientes());
    }

    [Fact]
    public void Procesar_ArchivoYaGrabado_SeReportaComoDuplicadoYPermaneceEnIncoming()
    {
        CopiarFixturesAIncoming("tipo1.pdf");
        var pendiente = _servicio.Procesar().Pendientes.Single();
        _servicio.Grabar(new[] { new AsignacionUnidad(pendiente.RevisionId, _unidadId) });

        // Se vuelve a dejar el mismo PDF en incoming (ya grabado antes).
        CopiarFixturesAIncoming("tipo1.pdf");
        var resultado = _servicio.Procesar();

        Assert.Empty(resultado.Pendientes);
        Assert.Single(resultado.Fallidas);
        Assert.Equal("DUPLICADO_YA_PROCESADO", resultado.Fallidas[0].Motivo);
        Assert.Single(Directory.GetFiles(_paths.IncomingDir));
    }
}
