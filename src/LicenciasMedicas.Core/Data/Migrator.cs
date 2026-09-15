using System.Reflection;
using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Data;

/// <summary>
/// Micro-migrador basado en PRAGMA user_version: ejecuta los scripts embebidos en
/// Data/Migrations (NNN_descripcion.sql) cuya versión sea mayor a la actual, en orden.
/// </summary>
public static class Migrator
{
    public static void Migrar(SqliteConnection connection)
    {
        var versionActual = ObtenerVersion(connection);
        var assembly = typeof(Migrator).Assembly;

        var migraciones = assembly.GetManifestResourceNames()
            .Where(n => n.Contains(".Data.Migrations.") && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(n => (Nombre: n, Version: ExtraerVersion(n)))
            .Where(m => m.Version > versionActual)
            .OrderBy(m => m.Version)
            .ToList();

        foreach (var (nombre, version) in migraciones)
        {
            var sql = LeerRecurso(assembly, nombre);

            using var transaction = connection.BeginTransaction();
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();

            EstablecerVersion(connection, version);
        }
    }

    private static string LeerRecurso(Assembly assembly, string nombreRecurso)
    {
        using var stream = assembly.GetManifestResourceStream(nombreRecurso)
            ?? throw new InvalidOperationException($"No se encontró el recurso embebido '{nombreRecurso}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static int ObtenerVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void EstablecerVersion(SqliteConnection connection, int version)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA user_version = {version};";
        cmd.ExecuteNonQuery();
    }

    private static int ExtraerVersion(string nombreRecurso)
    {
        var partes = nombreRecurso.Split('.');
        var nombreArchivoSinExtension = partes[^2];
        var prefijo = nombreArchivoSinExtension.Split('_')[0];
        return int.Parse(prefijo);
    }
}
