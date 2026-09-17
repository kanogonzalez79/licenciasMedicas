using System.Diagnostics;
using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Procesamiento;
using LicenciasMedicas.Core.Respaldo;
using LicenciasMedicas.Core.Unidades;
using LicenciasMedicas.Web.Endpoints;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Puerto fijo solo en dev: así el proxy de "npm run dev" (Web.Client/vite.config.ts)
    // sabe adónde apuntar sin coordinarlo a mano en cada arranque.
    builder.WebHost.UseUrls("http://127.0.0.1:5244");
}
else
{
    // Solo loopback: es una herramienta local de un solo usuario, no debe exponerse en la red.
    // Puerto efímero (0): el sistema operativo asigna uno libre, sin colisiones que resolver a mano.
    builder.WebHost.UseUrls("http://127.0.0.1:0");
}

builder.Services.AddSingleton<AppPaths>();
builder.Services.AddSingleton(sp => new SqliteConnectionFactory(sp.GetRequiredService<AppPaths>().DbFilePath));
builder.Services.AddSingleton<LicenciaExtractionService>();
builder.Services.AddSingleton<UnidadesRepository>();
builder.Services.AddSingleton<LicenciasRepository>();
builder.Services.AddSingleton<StagingRepository>();
builder.Services.AddSingleton<ProcesamientoService>();
builder.Services.AddSingleton<IngresoManualService>();
builder.Services.AddSingleton<CorreoRedaccionService>();
builder.Services.AddSingleton<SmtpDiagnosticoLog>();
builder.Services.AddSingleton<ConfiguracionSmtpService>();
builder.Services.AddSingleton<CorreoCopiaService>();
builder.Services.AddSingleton<CorreoEnvioService>();
builder.Services.AddSingleton<RespaldoService>();

var app = builder.Build();

using (var connection = app.Services.GetRequiredService<SqliteConnectionFactory>().Crear())
{
    Migrator.Migrar(connection);
}

// Regla de arranque fija: cualquier fila de revisión pendiente de un lote anterior se descarta
// y su PDF vuelve a "incoming" (nunca se intenta "resumir" un lote entre reinicios del exe).
app.Services.GetRequiredService<ProcesamientoService>().RecuperarHuerfanosAlArranque();

// El cliente React solo se compila y embebe en wwwroot para builds Release (ver el target
// BuildClientAssets en el .csproj) - en Development no hay nada embebido (ni siquiera el
// manifiesto), así que ManifestEmbeddedFileProvider no debe construirse ahí. En dev, el
// frontend lo sirve "npm run dev" (Vite) por separado, con proxy de /api/* a este backend.
IFileProvider? wwwroot = null;
if (!app.Environment.IsDevelopment())
{
    wwwroot = new ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot");
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = wwwroot });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = wwwroot });
}

app.MapUnidadesEndpoints();
app.MapLicenciasEndpoints();
app.MapProcesamientoEndpoints();
app.MapCorreosEndpoints();
app.MapConfiguracionEndpoints();
app.MapCorreosCopiaEndpoints();
app.MapRespaldoEndpoints();

// SPA (React Router): cualquier ruta que no matchee un endpoint de arriba ni un archivo
// estático real (ej. /unidades, /redactar-correo) devuelve index.html para que el router
// del cliente la resuelva. Sin esto, entrar directo o refrescar en esas rutas da 404.
if (wwwroot is not null)
{
    app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = wwwroot });
}

app.Start();

var direccion = app.Services.GetRequiredService<IServer>()
    .Features.Get<IServerAddressesFeature>()!
    .Addresses.First();

Console.WriteLine($"LicenciasMedicas escuchando en {direccion}");

if (!app.Environment.IsDevelopment())
{
    try
    {
        Process.Start(new ProcessStartInfo(direccion) { UseShellExecute = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"No se pudo abrir el navegador automáticamente ({ex.Message}). Abra {direccion} manualmente.");
    }
}

await app.WaitForShutdownAsync();
