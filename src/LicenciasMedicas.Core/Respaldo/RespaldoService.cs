using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Respaldo;

public sealed record ResultadoRespaldo(int PdfsCopiados, int PdfsEliminados);

/// <summary>Carpeta destino no disponible o sin permisos de escritura, antes o durante el respaldo.</summary>
public sealed class CarpetaDestinoNoDisponibleException : Exception
{
    public CarpetaDestinoNoDisponibleException(string mensaje, Exception? inner = null) : base(mensaje, inner)
    {
    }
}

/// <summary>
/// Respalda hacia una carpeta destino (ej. un disco externo) únicamente lo ya grabado de
/// forma permanente: la base de datos y los PDF archivados. Ver design.md del cambio
/// "respaldar-datos" para el orden de operaciones y sus motivos.
/// </summary>
public sealed class RespaldoService
{
    private const string NombreArchivoBaseDeDatos = "licencias.db";
    private const string NombreCarpetaArchivo = "archivo";

    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;

    public RespaldoService(AppPaths paths, SqliteConnectionFactory connectionFactory)
    {
        _paths = paths;
        _connectionFactory = connectionFactory;
    }

    public ResultadoRespaldo Respaldar(string carpetaDestino)
    {
        VerificarCarpetaDestinoEscribible(carpetaDestino);

        var pdfsCopiados = CopiarPdfsFaltantes(carpetaDestino);
        RespaldarBaseDeDatos(carpetaDestino);
        var pdfsEliminados = EliminarPdfsSobrantes(carpetaDestino);

        return new ResultadoRespaldo(pdfsCopiados, pdfsEliminados);
    }

    private static void VerificarCarpetaDestinoEscribible(string carpetaDestino)
    {
        if (!Directory.Exists(carpetaDestino))
            throw new CarpetaDestinoNoDisponibleException($"La carpeta destino '{carpetaDestino}' no está disponible.");

        var rutaPrueba = Path.Combine(carpetaDestino, $".respaldo_prueba_{Guid.NewGuid():N}");
        try
        {
            File.WriteAllBytes(rutaPrueba, Array.Empty<byte>());
            File.Delete(rutaPrueba);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new CarpetaDestinoNoDisponibleException($"No se puede escribir en la carpeta destino '{carpetaDestino}'.", ex);
        }
    }

    /// <summary>Toma una copia consistente de la base de datos (VACUUM INTO) y sobrescribe la anterior en destino.</summary>
    private void RespaldarBaseDeDatos(string carpetaDestino)
    {
        var rutaFinal = Path.Combine(carpetaDestino, NombreArchivoBaseDeDatos);
        var rutaTemporal = rutaFinal + ".tmp";

        if (File.Exists(rutaTemporal))
            File.Delete(rutaTemporal);

        try
        {
            using var connection = _connectionFactory.Crear();
            using var comando = connection.CreateCommand();
            comando.CommandText = "VACUUM INTO $ruta;";
            comando.Parameters.AddWithValue("$ruta", rutaTemporal);
            comando.ExecuteNonQuery();

            File.Move(rutaTemporal, rutaFinal, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new CarpetaDestinoNoDisponibleException($"No se pudo escribir la base de datos en la carpeta destino '{carpetaDestino}'.", ex);
        }
    }

    /// <summary>Rutas relativas (bajo archivo/) presentes solo en origen, y presentes solo en destino.</summary>
    public (IReadOnlyList<string> Faltantes, IReadOnlyList<string> Sobrantes) DiferenciasPdfs(string carpetaDestino)
    {
        var origenDir = _paths.ArchivoDir;
        var destinoDir = Path.Combine(carpetaDestino, NombreCarpetaArchivo);

        var origen = ListarRutasRelativas(origenDir);
        var destino = ListarRutasRelativas(destinoDir);

        var faltantes = origen.Except(destino, StringComparer.OrdinalIgnoreCase).ToList();
        var sobrantes = destino.Except(origen, StringComparer.OrdinalIgnoreCase).ToList();

        return (faltantes, sobrantes);
    }

    private int CopiarPdfsFaltantes(string carpetaDestino)
    {
        var (faltantes, _) = DiferenciasPdfs(carpetaDestino);
        var origenDir = _paths.ArchivoDir;
        var destinoDir = Path.Combine(carpetaDestino, NombreCarpetaArchivo);

        foreach (var relativa in faltantes)
        {
            var rutaOrigen = Path.Combine(origenDir, relativa);
            var rutaDestino = Path.Combine(destinoDir, relativa);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(rutaDestino)!);
                File.Copy(rutaOrigen, rutaDestino, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new CarpetaDestinoNoDisponibleException($"No se pudo copiar '{relativa}' a la carpeta destino '{carpetaDestino}'.", ex);
            }
        }

        return faltantes.Count;
    }

    private int EliminarPdfsSobrantes(string carpetaDestino)
    {
        var (_, sobrantes) = DiferenciasPdfs(carpetaDestino);
        var destinoDir = Path.Combine(carpetaDestino, NombreCarpetaArchivo);

        foreach (var relativa in sobrantes)
        {
            try
            {
                File.Delete(Path.Combine(destinoDir, relativa));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new CarpetaDestinoNoDisponibleException($"No se pudo eliminar '{relativa}' de la carpeta destino '{carpetaDestino}'.", ex);
            }
        }

        return sobrantes.Count;
    }

    private static IReadOnlyList<string> ListarRutasRelativas(string carpeta)
    {
        if (!Directory.Exists(carpeta))
            return Array.Empty<string>();

        return Directory.EnumerateFiles(carpeta, "*", SearchOption.AllDirectories)
            .Select(ruta => Path.GetRelativePath(carpeta, ruta))
            .ToList();
    }
}
