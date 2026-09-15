using System.Globalization;

namespace LicenciasMedicas.Core.Paths;

/// <summary>
/// Resuelve todas las rutas de datos en relación al directorio donde vive el ejecutable
/// (nunca rutas fijas de usuario ni %AppData%), de modo que toda la carpeta del proyecto
/// se pueda mover a otro computador sin romper nada.
/// </summary>
public sealed class AppPaths
{
    public string BaseDir { get; }
    public string DataDir => Path.Combine(BaseDir, "data");
    public string IncomingDir => Path.Combine(DataDir, "incoming");
    public string StagingDir => Path.Combine(DataDir, "staging");
    public string ArchivoDir => Path.Combine(DataDir, "archivo");
    public string DbFilePath => Path.Combine(DataDir, "licencias.db");
    public string PlantillaCorreoPath => Path.Combine(DataDir, "plantilla.txt");
    public string LogsDir => Path.Combine(BaseDir, "logs");

    public AppPaths() : this(AppContext.BaseDirectory)
    {
    }

    public AppPaths(string baseDir)
    {
        BaseDir = baseDir;
        foreach (var dir in new[] { IncomingDir, StagingDir, ArchivoDir, LogsDir })
            Directory.CreateDirectory(dir);
    }

    public string RutaStagingLote(string loteId) => Path.Combine(StagingDir, loteId);

    /// <summary>
    /// Arma una ruta única para el archivo permanente de una licencia (nombre del paciente + folio,
    /// organizado por año/mes), preservando la extensión del archivo original (PDF o imagen). El
    /// folio ya es único en todo el sistema (se valida antes de archivar), así que no hace falta
    /// un correlativo adicional para distinguir varios documentos de la misma persona.
    /// </summary>
    public (string Relativa, string Absoluta) ResolverRutaArchivoPermanente(string folio, string nombreCompletoPaciente, string extension)
    {
        var ahora = DateTime.UtcNow;
        var folioSeguro = string.Join("_", folio.Split(Path.GetInvalidFileNameChars()));
        var nombreSeguro = string.Join("_", NormalizarNombreTituloCase(nombreCompletoPaciente).Split(Path.GetInvalidFileNameChars()));
        var nombreArchivo = $"{nombreSeguro} - {folioSeguro}{extension}";
        var relativa = Path.Combine(ahora.Year.ToString(), ahora.Month.ToString("D2"), nombreArchivo);
        var absoluta = Path.Combine(ArchivoDir, relativa);
        return (relativa, absoluta);
    }

    /// <summary>
    /// Normaliza un nombre a Título Case (primera letra de cada palabra en mayúscula, resto en
    /// minúscula), preservando tildes y "ñ". Los nombres extraídos por el parser suelen venir en
    /// MAYÚSCULAS; hay que bajarlos a minúsculas antes de aplicar ToTitleCase porque, si no,
    /// .NET trata cada palabra en mayúsculas como una sigla y la deja intacta.
    /// </summary>
    private static string NormalizarNombreTituloCase(string nombreCompleto)
    {
        var cultura = CultureInfo.GetCultureInfo("es-CL");
        return cultura.TextInfo.ToTitleCase(nombreCompleto.ToLower(cultura));
    }
}
