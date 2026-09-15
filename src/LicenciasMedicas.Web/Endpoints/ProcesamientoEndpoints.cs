using LicenciasMedicas.Core.Procesamiento;

namespace LicenciasMedicas.Web.Endpoints;

public static class ProcesamientoEndpoints
{
    public static void MapProcesamientoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/procesamiento");

        group.MapPost("/procesar", (ProcesamientoService servicio) => Results.Ok(servicio.Procesar()));

        group.MapGet("/pendientes", (ProcesamientoService servicio) => Results.Ok(servicio.ObtenerPendientes()));

        group.MapPost("/grabar", (List<AsignacionUnidad> asignaciones, ProcesamientoService servicio)
            => Results.Ok(servicio.Grabar(asignaciones)));

        group.MapDelete("/revision/{id:int}", (int id, ProcesamientoService servicio)
            => servicio.DescartarPendiente(id) ? Results.Ok() : Results.NotFound());
    }
}
