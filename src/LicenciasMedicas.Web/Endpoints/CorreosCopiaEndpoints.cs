using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Web.Endpoints;

public sealed record AgregarCorreoCopiaRequest(string CorreoElectronico);
public sealed record CambiarActivoCorreoCopiaRequest(bool Activo);

public static class CorreosCopiaEndpoints
{
    public static void MapCorreosCopiaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/configuracion/correos-copia");

        group.MapGet("/", (CorreoCopiaService servicio) => Results.Ok(servicio.Listar()));

        group.MapPost("/", (AgregarCorreoCopiaRequest request, CorreoCopiaService servicio) =>
        {
            try
            {
                return Results.Ok(servicio.Agregar(request.CorreoElectronico));
            }
            catch (CorreoCopiaInvalidoException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (CorreoCopiaDuplicadoException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPatch("/{id:int}", (int id, CambiarActivoCorreoCopiaRequest request, CorreoCopiaService servicio) =>
        {
            servicio.CambiarActivo(id, request.Activo);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", (int id, CorreoCopiaService servicio) =>
        {
            servicio.Eliminar(id);
            return Results.NoContent();
        });
    }
}
