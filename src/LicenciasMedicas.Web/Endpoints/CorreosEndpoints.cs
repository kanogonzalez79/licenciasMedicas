using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Web.Endpoints;

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
    }
}
