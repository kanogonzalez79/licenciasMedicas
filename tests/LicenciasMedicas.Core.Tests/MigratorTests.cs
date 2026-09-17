using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Paths;

namespace LicenciasMedicas.Core.Tests;

public sealed class MigratorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SqliteConnectionFactory _connectionFactory;

    public MigratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "lm_migrator_tests_" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(_tempDir);
        _connectionFactory = new SqliteConnectionFactory(paths.DbFilePath);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort en limpieza de test */ }
    }

    [Fact]
    public void Migrar_DesdeCero_CreaTablasDeConfiguracionSmtpYCorreosEnviados()
    {
        using var connection = _connectionFactory.Crear();
        Migrator.Migrar(connection);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('ConfiguracionSmtp','CorreosEnviados');";
        using var reader = cmd.ExecuteReader();
        var nombres = new List<string>();
        while (reader.Read())
            nombres.Add(reader.GetString(0));

        Assert.Contains("ConfiguracionSmtp", nombres);
        Assert.Contains("CorreosEnviados", nombres);
    }
}
