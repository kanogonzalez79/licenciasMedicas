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
        sb.Append(FormatearTabla(licencias));

        return sb.ToString();
    }

    private const string SeparadorColumnas = "  ";

    private static string FormatearTabla(IReadOnlyList<Licencia> licencias)
    {
        string[] encabezados = ["RUT", "Nombre", "Fecha Inicio", "Fecha Término", "Cantidad de Días"];
        var filas = licencias
            .Select(licencia => new[]
            {
                $"{licencia.RutPacienteSinDv}-{licencia.DvPaciente}",
                licencia.NombreCompletoPaciente,
                licencia.FechaInicioReposo,
                licencia.FechaTerminoReposo,
                licencia.CantidadDias.ToString(),
            })
            .ToList();

        var anchos = new int[encabezados.Length];
        for (var i = 0; i < encabezados.Length; i++)
        {
            anchos[i] = encabezados[i].Length;
            foreach (var fila in filas)
                anchos[i] = Math.Max(anchos[i], fila[i].Length);
        }

        string FormatearFila(IReadOnlyList<string> valores) =>
            string.Join(SeparadorColumnas, valores.Select((valor, i) => valor.PadRight(anchos[i])));

        var sb = new StringBuilder();
        sb.AppendLine(FormatearFila(encabezados));
        sb.AppendLine(FormatearFila(anchos.Select(ancho => new string('-', ancho)).ToArray()));
        foreach (var fila in filas)
            sb.AppendLine(FormatearFila(fila));

        return sb.ToString();
    }

    private string LeerOCrearPlantilla()
    {
        if (!File.Exists(_paths.PlantillaCorreoPath))
            File.WriteAllText(_paths.PlantillaCorreoPath, PlantillaPorDefecto);

        return File.ReadAllText(_paths.PlantillaCorreoPath);
    }
}
