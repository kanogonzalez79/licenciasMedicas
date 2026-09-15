using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Web.Endpoints;

public static class LicenciasEndpoints
{
    private const long TamanoMaximoAdjuntoBytes = 20 * 1024 * 1024;

    private static readonly Dictionary<string, string> MensajesLegibles = new()
    {
        [MotivosFallo.RutPacienteInvalido] = "El RUT del paciente no es válido.",
        [MotivosFallo.FechasInvalidas] = "Las fechas de reposo no son válidas.",
        [MotivosFallo.DiasInvalidos] = "La cantidad de días no coincide con las fechas de reposo.",
        [MotivosFallo.TipoLicenciaInvalido] = "El código de tipo de licencia no es válido.",
        [MotivosRechazoIngresoManual.FolioRequerido] = "El folio es obligatorio.",
        [MotivosRechazoIngresoManual.NombrePacienteRequerido] = "El nombre del paciente es obligatorio.",
        [MotivosRechazoIngresoManual.RutProfesionalInvalido] = "El RUT del profesional no es válido.",
        [MotivosRechazoIngresoManual.FaltaUnidad] = "Falta asignar una unidad válida.",
        [MotivosRechazoIngresoManual.AdjuntoRequerido] = "Debe adjuntar el documento de respaldo (PDF o imagen).",
        [MotivosRechazoIngresoManual.AdjuntoFormatoInvalido] = "El documento debe ser un PDF o una imagen (JPG o PNG).",
        [MotivosRechazoIngresoManual.AdjuntoDemasiadoGrande] = "El documento adjunto supera el tamaño máximo permitido (20 MB).",
        [MotivosRechazoIngresoManual.DuplicadoFolioYaExistente] = "Ya existe una licencia grabada con este folio.",
        [MotivosRechazoIngresoManual.DuplicadoYaProcesado] = "Este documento ya fue grabado anteriormente.",
    };

    private static string MensajeLegible(string motivo) => MensajesLegibles.GetValueOrDefault(motivo, motivo);

    private static string ContentTypeSegunExtension(string rutaArchivo) => Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => "application/pdf",
    };

    private static string? ExtensionPermitida(string? contentType, string nombreArchivo)
    {
        var extensionOriginal = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return (contentType?.ToLowerInvariant(), extensionOriginal) switch
        {
            ("application/pdf", _) => ".pdf",
            ("image/jpeg", _) => ".jpg",
            ("image/png", _) => ".png",
            (_, ".pdf") => ".pdf",
            (_, ".jpg" or ".jpeg") => ".jpg",
            (_, ".png") => ".png",
            _ => null,
        };
    }

    public static void MapLicenciasEndpoints(this WebApplication app)
    {
        app.MapGet("/api/licencias", (
            DateOnly? fechaDesde, DateOnly? fechaHasta, string? rut, string? nombre, int? unidadId, int? page, int? pageSize,
            SqliteConnectionFactory factory, LicenciasRepository repo) =>
        {
            var filtro = new BusquedaLicenciasFiltro
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Rut = rut,
                Nombre = nombre,
                UnidadId = unidadId,
                Page = page ?? 1,
                PageSize = pageSize ?? 50,
            };

            using var connection = factory.Crear();
            var (items, total) = repo.Buscar(connection, filtro);
            return Results.Ok(new { items, total });
        });

        app.MapGet("/api/licencias/{id:int}", (int id, SqliteConnectionFactory factory, LicenciasRepository repo) =>
        {
            using var connection = factory.Crear();
            var licencia = repo.ObtenerPorId(connection, id);
            return licencia is null ? Results.NotFound() : Results.Ok(licencia);
        });

        app.MapPost("/api/licencias/manual", async (HttpRequest request, SqliteConnectionFactory factory, LicenciasRepository repo, IngresoManualService servicio) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "La solicitud debe enviarse como multipart/form-data." });

            var form = await request.ReadFormAsync();

            string? Obtener(string clave) => form.TryGetValue(clave, out var valores) ? valores.ToString() : null;
            int? ObtenerEntero(string clave) => int.TryParse(Obtener(clave), out var valor) ? valor : null;
            DateOnly? ObtenerFecha(string clave) => DateOnly.TryParse(Obtener(clave), out var valor) ? valor : null;

            var archivo = form.Files.GetFile("archivo");
            if (archivo is null || archivo.Length == 0)
                return Results.BadRequest(new { error = MensajeLegible(MotivosRechazoIngresoManual.AdjuntoRequerido) });

            if (archivo.Length > TamanoMaximoAdjuntoBytes)
                return Results.BadRequest(new { error = MensajeLegible(MotivosRechazoIngresoManual.AdjuntoDemasiadoGrande) });

            var extension = ExtensionPermitida(archivo.ContentType, archivo.FileName);
            if (extension is null)
                return Results.BadRequest(new { error = MensajeLegible(MotivosRechazoIngresoManual.AdjuntoFormatoInvalido) });

            var fechaInicio = ObtenerFecha("fechaInicioReposo");
            var fechaTermino = ObtenerFecha("fechaTerminoReposo");
            var cantidadDias = ObtenerEntero("cantidadDias");
            var codigoTipoLicencia = ObtenerEntero("codigoTipoLicencia");
            var unidadId = ObtenerEntero("unidadId");

            if (fechaInicio is null || fechaTermino is null)
                return Results.BadRequest(new { error = MensajeLegible(MotivosFallo.FechasInvalidas) });
            if (cantidadDias is null)
                return Results.BadRequest(new { error = MensajeLegible(MotivosFallo.DiasInvalidos) });
            if (codigoTipoLicencia is null)
                return Results.BadRequest(new { error = MensajeLegible(MotivosFallo.TipoLicenciaInvalido) });
            if (unidadId is null)
                return Results.BadRequest(new { error = MensajeLegible(MotivosRechazoIngresoManual.FaltaUnidad) });

            var datos = new DatosLicenciaManual
            {
                Folio = Obtener("folio") ?? string.Empty,
                RutPaciente = Obtener("rutPaciente") ?? string.Empty,
                ApellidoPaternoPaciente = Obtener("apellidoPaternoPaciente"),
                ApellidoMaternoPaciente = Obtener("apellidoMaternoPaciente"),
                NombresPaciente = Obtener("nombresPaciente"),
                NombreCompletoPaciente = Obtener("nombreCompletoPaciente") ?? string.Empty,
                EdadPaciente = ObtenerEntero("edadPaciente"),
                SexoPaciente = Obtener("sexoPaciente"),
                CodigoTipoLicencia = codigoTipoLicencia.Value,
                FechaEmisionOtorgamiento = ObtenerFecha("fechaEmisionOtorgamiento"),
                FechaInicioReposo = fechaInicio.Value,
                FechaTerminoReposo = fechaTermino.Value,
                CantidadDias = cantidadDias.Value,
                RutProfesional = Obtener("rutProfesional"),
                NombreCompletoProfesional = Obtener("nombreCompletoProfesional"),
                CorreoProfesional = Obtener("correoProfesional"),
                EspecialidadProfesional = Obtener("especialidadProfesional"),
                UnidadId = unidadId.Value,
                Observaciones = Obtener("observaciones"),
            };

            await using var memoria = new MemoryStream();
            await archivo.CopyToAsync(memoria);

            var resultado = servicio.Ingresar(datos, memoria.ToArray(), extension, archivo.FileName);
            if (!resultado.Exito)
                return Results.BadRequest(new { error = MensajeLegible(resultado.Motivo!) });

            using var connection = factory.Crear();
            return Results.Created($"/api/licencias/{resultado.LicenciaId}", repo.ObtenerPorId(connection, resultado.LicenciaId!.Value));
        });

        app.MapGet("/api/licencias/{id:int}/pdf", (int id, AppPaths paths, SqliteConnectionFactory factory, LicenciasRepository repo) =>
        {
            using var connection = factory.Crear();
            var licencia = repo.ObtenerPorId(connection, id);
            if (licencia is null)
                return Results.NotFound();

            var rutaAbsoluta = Path.Combine(paths.ArchivoDir, licencia.RutaPdfArchivado);
            return File.Exists(rutaAbsoluta) ? Results.File(rutaAbsoluta, ContentTypeSegunExtension(rutaAbsoluta)) : Results.NotFound();
        });

        app.MapDelete("/api/licencias/{id:int}", (int id, AppPaths paths, SqliteConnectionFactory factory, LicenciasRepository repo) =>
        {
            using var connection = factory.Crear();
            var licencia = repo.ObtenerPorId(connection, id);
            if (licencia is null)
                return Results.NotFound();

            var rutaAbsoluta = Path.Combine(paths.ArchivoDir, licencia.RutaPdfArchivado);
            try
            {
                if (File.Exists(rutaAbsoluta))
                    File.Delete(rutaAbsoluta);
            }
            catch (IOException)
            {
                // Borrado del archivo es best-effort: no bloquea la eliminación del registro.
            }

            repo.Eliminar(connection, id);
            return Results.Ok();
        });
    }
}
