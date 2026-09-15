using LicenciasMedicas.Core.Respaldo;
using LicenciasMedicas.Web.Respaldo;

namespace LicenciasMedicas.Web.Endpoints;

public static class RespaldoEndpoints
{
    public static void MapRespaldoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/respaldo");

        group.MapPost("/", (RespaldoService servicio) =>
        {
            var carpetaDestino = SelectorCarpeta.Elegir();
            if (carpetaDestino is null)
                return Results.Ok(new { estado = "cancelado" });

            try
            {
                var resultado = servicio.Respaldar(carpetaDestino);
                return Results.Ok(new
                {
                    estado = "completado",
                    pdfsCopiados = resultado.PdfsCopiados,
                    pdfsEliminados = resultado.PdfsEliminados,
                });
            }
            catch (CarpetaDestinoNoDisponibleException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}
