using System.Net;
using System.Net.Http.Json;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Unidades;
using LicenciasMedicas.Web.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LicenciasMedicas.Core.Tests;

public sealed class LicenciasEndpointsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo = new();
    private readonly WebApplication _app;
    private readonly int _unidadId;

    public LicenciasEndpointsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_endpoint_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        var paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(paths.DbFilePath);

        using (var connection = _connectionFactory.Crear())
        {
            Migrator.Migrar(connection);
            _unidadId = new UnidadesRepository().Crear(connection, "Unidad de Prueba", "prueba@ejemplo.cl");
        }

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(_connectionFactory);
        builder.Services.AddSingleton(_licenciasRepo);
        builder.Services.AddSingleton(paths);

        _app = builder.Build();
        _app.MapLicenciasEndpoints();
        _app.StartAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _app.StopAsync().GetAwaiter().GetResult();
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    private int GrabarLicenciaDePrueba(string folio)
    {
        using var connection = _connectionFactory.Crear();
        return _licenciasRepo.Insertar(connection, new Licencia
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
        });
    }

    [Fact]
    public async Task Patch_CorreoEnviado_LicenciaExistente_ActualizaYDevuelve204()
    {
        var licenciaId = GrabarLicenciaDePrueba("FOLIO-PATCH");
        var client = _app.GetTestClient();

        var respuesta = await client.PatchAsJsonAsync($"/api/licencias/{licenciaId}/correo-enviado", new CambiarCorreoEnviadoRequest(true));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);

        using var connection = _connectionFactory.Crear();
        Assert.True(_licenciasRepo.ObtenerPorId(connection, licenciaId)!.CorreoEnviado);
    }

    [Fact]
    public async Task Patch_CorreoEnviado_LicenciaInexistente_Devuelve404()
    {
        var client = _app.GetTestClient();

        var respuesta = await client.PatchAsJsonAsync("/api/licencias/999/correo-enviado", new CambiarCorreoEnviadoRequest(true));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_Dashboard_RangoValido_DevuelveLicenciasDelRango()
    {
        GrabarLicenciaDePrueba("FOLIO-DASH-1");
        var client = _app.GetTestClient();

        var respuesta = await client.GetAsync("/api/licencias/dashboard?fechaDesde=2026-01-01&fechaHasta=2026-01-31&modo=inicio");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var licencias = await respuesta.Content.ReadFromJsonAsync<List<Licencia>>();
        Assert.Single(licencias!);
        Assert.Equal("FOLIO-DASH-1", licencias![0].Folio);
    }

    [Theory]
    [InlineData("termino")]
    [InlineData("interseccion")]
    public async Task Get_Dashboard_OtrosModos_DevuelveLicenciasDelRango(string modo)
    {
        GrabarLicenciaDePrueba("FOLIO-DASH-MODO");
        var client = _app.GetTestClient();

        var respuesta = await client.GetAsync($"/api/licencias/dashboard?fechaDesde=2026-01-01&fechaHasta=2026-01-31&modo={modo}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var licencias = await respuesta.Content.ReadFromJsonAsync<List<Licencia>>();
        Assert.Single(licencias!);
    }

    [Fact]
    public async Task Get_Dashboard_FaltaFechaHasta_Devuelve400()
    {
        var client = _app.GetTestClient();

        var respuesta = await client.GetAsync("/api/licencias/dashboard?fechaDesde=2026-01-01&modo=inicio");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_Dashboard_FechaDesdePosteriorAHasta_Devuelve400()
    {
        var client = _app.GetTestClient();

        var respuesta = await client.GetAsync("/api/licencias/dashboard?fechaDesde=2026-02-01&fechaHasta=2026-01-01&modo=inicio");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_Dashboard_ModoInvalido_Devuelve400()
    {
        var client = _app.GetTestClient();

        var respuesta = await client.GetAsync("/api/licencias/dashboard?fechaDesde=2026-01-01&fechaHasta=2026-01-31&modo=invalido");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
