using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _dbFilePath;

    public SqliteConnectionFactory(string dbFilePath)
    {
        _dbFilePath = dbFilePath;
    }

    public SqliteConnection Crear()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
        };

        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();
        return connection;
    }
}
