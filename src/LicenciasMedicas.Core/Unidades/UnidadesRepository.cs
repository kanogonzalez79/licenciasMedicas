using Dapper;
using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Unidades;

public sealed class DescripcionDuplicadaException : Exception
{
    public DescripcionDuplicadaException(string descripcion)
        : base($"Ya existe una unidad con la descripción '{descripcion}'.")
    {
    }
}

public sealed class UnidadesRepository
{
    public IReadOnlyList<Unidad> ObtenerTodas(SqliteConnection connection)
        => connection.Query<Unidad>("SELECT * FROM Unidades ORDER BY Descripcion COLLATE NOCASE;").ToList();

    public Unidad? ObtenerPorId(SqliteConnection connection, int unidadId)
        => connection.QuerySingleOrDefault<Unidad>("SELECT * FROM Unidades WHERE UnidadId = @unidadId;", new { unidadId });

    public int Crear(SqliteConnection connection, string descripcion, string? correoElectronico)
    {
        try
        {
            const string sql = """
                INSERT INTO Unidades (Descripcion, CorreoElectronico)
                VALUES (@descripcion, @correoElectronico);
                SELECT last_insert_rowid();
                """;
            return connection.QuerySingle<int>(sql, new { descripcion, correoElectronico });
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new DescripcionDuplicadaException(descripcion);
        }
    }

    public void Editar(SqliteConnection connection, int unidadId, string descripcion, string? correoElectronico)
    {
        try
        {
            const string sql = """
                UPDATE Unidades
                SET Descripcion = @descripcion,
                    CorreoElectronico = @correoElectronico,
                    FechaModificacion = strftime('%Y-%m-%dT%H:%M:%fZ','now')
                WHERE UnidadId = @unidadId;
                """;
            connection.Execute(sql, new { unidadId, descripcion, correoElectronico });
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new DescripcionDuplicadaException(descripcion);
        }
    }
}
