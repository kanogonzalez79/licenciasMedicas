using System.Text;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Correos;

/// <summary>
/// Genera el texto de correo (saludo desde plantilla.txt + tabla en texto plano) para las
/// licencias de una unidad grabadas en un día determinado.
/// </summary>
public sealed class CorreoRedaccionService
{
    private const string PlantillaPorDefecto =
        "Estimado(a), adjunto personas de la unidad {{UNIDAD}} que presentan licencias medicas:";

    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo;

    public CorreoRedaccionService(AppPaths paths, SqliteConnectionFactory connectionFactory, LicenciasRepository licenciasRepo)
    {
        _paths = paths;
        _connectionFactory = connectionFactory;
        _licenciasRepo = licenciasRepo;
    }

    public IReadOnlyList<UnidadConLicenciasEnFecha> ObtenerUnidadesConLicenciasEnFecha(DateOnly fecha)
    {
        using var connection = _connectionFactory.Crear();
        return _licenciasRepo.UnidadesConLicenciasGrabadasEnFecha(connection, fecha);
    }

    /// <summary>Retorna null si la unidad no tiene licencias grabadas ese día.</summary>
    public string? Redactar(int unidadId, DateOnly fecha)
    {
        using var connection = _connectionFactory.Crear();
        var licencias = _licenciasRepo.LicenciasDeUnidadGrabadasEnFecha(connection, unidadId, fecha);
        if (licencias.Count == 0)
            return null;

        var unidadDescripcion = licencias[0].UnidadDescripcion ?? string.Empty;
        var saludo = LeerOCrearPlantilla().Replace("{{UNIDAD}}", unidadDescripcion);

        var sb = new StringBuilder();
        sb.AppendLine(saludo);
        sb.AppendLine();
        sb.AppendLine(string.Join('\t', "RUT", "Nombre", "Fecha Inicio", "Fecha Término", "Cantidad de Días"));
        foreach (var licencia in licencias)
        {
            var rut = $"{licencia.RutPacienteSinDv}-{licencia.DvPaciente}";
            sb.AppendLine(string.Join('\t', rut, licencia.NombreCompletoPaciente, licencia.FechaInicioReposo, licencia.FechaTerminoReposo, licencia.CantidadDias));
        }

        return sb.ToString();
    }

    private string LeerOCrearPlantilla()
    {
        if (!File.Exists(_paths.PlantillaCorreoPath))
            File.WriteAllText(_paths.PlantillaCorreoPath, PlantillaPorDefecto);

        return File.ReadAllText(_paths.PlantillaCorreoPath);
    }
}
