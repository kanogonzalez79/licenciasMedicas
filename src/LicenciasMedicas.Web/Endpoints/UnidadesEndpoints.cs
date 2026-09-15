using LicenciasMedicas.Core.Correos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Web.Endpoints;

public sealed record CrearUnidadRequest(string Descripcion, string CorreoElectronico);
public sealed record EditarUnidadRequest(string Descripcion, string CorreoElectronico);

public static class UnidadesEndpoints
{
    public static void MapUnidadesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/unidades");

        group.MapGet("/", (SqliteConnectionFactory factory, UnidadesRepository repo) =>
        {
            using var connection = factory.Crear();
            return Results.Ok(repo.ObtenerTodas(connection));
        });

        group.MapPost("/", (CrearUnidadRequest request, SqliteConnectionFactory factory, UnidadesRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return Results.BadRequest(new { error = "La descripción es obligatoria." });

            if (!EmailUtils.EsFormatoValido(request.CorreoElectronico))
                return Results.BadRequest(new { error = "El correo electrónico es obligatorio y debe tener un formato válido." });

            using var connection = factory.Crear();
            try
            {
                var id = repo.Crear(connection, request.Descripcion.Trim(), request.CorreoElectronico.Trim());
                return Results.Created($"/api/unidades/{id}", repo.ObtenerPorId(connection, id));
            }
            catch (DescripcionDuplicadaException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPut("/{id:int}", (int id, EditarUnidadRequest request, SqliteConnectionFactory factory, UnidadesRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return Results.BadRequest(new { error = "La descripción es obligatoria." });

            if (!EmailUtils.EsFormatoValido(request.CorreoElectronico))
                return Results.BadRequest(new { error = "El correo electrónico es obligatorio y debe tener un formato válido." });

            using var connection = factory.Crear();
            if (repo.ObtenerPorId(connection, id) is null)
                return Results.NotFound();

            try
            {
                repo.Editar(connection, id, request.Descripcion.Trim(), request.CorreoElectronico.Trim());
                return Results.Ok(repo.ObtenerPorId(connection, id));
            }
            catch (DescripcionDuplicadaException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });
    }
}
