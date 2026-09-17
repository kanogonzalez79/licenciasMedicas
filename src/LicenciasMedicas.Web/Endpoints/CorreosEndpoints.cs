using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Web.Endpoints;

public sealed record EnviarCorreoRequest(int UnidadId, DateOnly Fecha, string Texto);

public static class CorreosEndpoints
{
    public static void MapCorreosEndpoints(this WebApplication app)
    {
        app.MapGet("/api/correos/unidades", (DateOnly fecha, CorreoRedaccionService servicio)
            => Results.Ok(servicio.ObtenerUnidadesConLicenciasEnFecha(fecha)));

        app.MapGet("/api/correos/redactar", (DateOnly fecha, int unidadId, CorreoRedaccionService servicio) =>
        {
            var texto = servicio.Redactar(unidadId, fecha);
            return texto is null ? Results.NotFound() : Results.Ok(new { texto });
        });

        app.MapPost("/api/correos/enviar", (EnviarCorreoRequest request, CorreoEnvioService servicio) =>
        {
            try
            {
                servicio.Enviar(request.UnidadId, request.Fecha, request.Texto);
                return Results.Ok(new { estado = "enviado" });
            }
            catch (ConfiguracionSmtpNoConfiguradaException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (UnidadSinCorreoValidoException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (EnvioCorreoFallidoException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });
    }
}
