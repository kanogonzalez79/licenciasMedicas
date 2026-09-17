using LicenciasMedicas.Core.Correos;

namespace LicenciasMedicas.Web.Endpoints;

public sealed record GuardarConfiguracionSmtpRequest(string Host, int Puerto, string Usuario, string Contrasena, string Remitente, bool UsaSsl);
public sealed record ProbarConexionSmtpRequest(string Host, int Puerto, string Usuario, string Contrasena, bool UsaSsl);

public static class ConfiguracionEndpoints
{
    public static void MapConfiguracionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/configuracion/smtp");

        group.MapGet("/", (ConfiguracionSmtpService servicio) => Results.Ok(servicio.Obtener()));

        group.MapPut("/", (GuardarConfiguracionSmtpRequest request, ConfiguracionSmtpService servicio) =>
        {
            try
            {
                servicio.Guardar(request.Host, request.Puerto, request.Usuario, request.Contrasena, request.Remitente, request.UsaSsl);
                return Results.Ok(servicio.Obtener());
            }
            catch (ConfiguracionSmtpInvalidaException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/probar", (ProbarConexionSmtpRequest request, ConfiguracionSmtpService servicio) =>
        {
            var resultado = servicio.ProbarConexion(request.Host, request.Puerto, request.Usuario, request.Contrasena, request.UsaSsl);
            return Results.Ok(new { exito = resultado.Exito, error = resultado.Error });
        });
    }
}
