using Dapper;
using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Licencias;

public sealed class FolioDuplicadoException : Exception
{
    public FolioDuplicadoException(string folio) : base($"Ya existe una licencia grabada con el folio '{folio}'.")
    {
    }
}

public sealed class LicenciasRepository
{
    public bool ExisteFolioOHash(SqliteConnection connection, string folio, string hashSha256)
    {
        const string sql = "SELECT COUNT(1) FROM Licencias WHERE Folio = @folio OR HashArchivoSha256 = @hashSha256;";
        return connection.QuerySingle<int>(sql, new { folio, hashSha256 }) > 0;
    }

    public bool ExisteHash(SqliteConnection connection, string hashSha256)
    {
        const string sql = "SELECT COUNT(1) FROM Licencias WHERE HashArchivoSha256 = @hashSha256;";
        return connection.QuerySingle<int>(sql, new { hashSha256 }) > 0;
    }

    public int Insertar(SqliteConnection connection, Licencia licencia, SqliteTransaction? transaction = null)
    {
        const string sql = """
            INSERT INTO Licencias (
                Folio, TipoFormulario, RutPacienteSinDv, DvPaciente,
                ApellidoPaternoPaciente, ApellidoMaternoPaciente, NombresPaciente, NombreCompletoPaciente,
                EdadPaciente, SexoPaciente, CodigoTipoLicencia, DescripcionTipoLicencia,
                FechaEmisionOtorgamiento, FechaInicioReposo, FechaTerminoReposo, CantidadDias,
                RutProfesionalSinDv, DvProfesional, NombreCompletoProfesional, CorreoProfesional, EspecialidadProfesional,
                UnidadId, RutaPdfArchivado, NombreArchivoOriginal, HashArchivoSha256, Observaciones
            ) VALUES (
                @Folio, @TipoFormulario, @RutPacienteSinDv, @DvPaciente,
                @ApellidoPaternoPaciente, @ApellidoMaternoPaciente, @NombresPaciente, @NombreCompletoPaciente,
                @EdadPaciente, @SexoPaciente, @CodigoTipoLicencia, @DescripcionTipoLicencia,
                @FechaEmisionOtorgamiento, @FechaInicioReposo, @FechaTerminoReposo, @CantidadDias,
                @RutProfesionalSinDv, @DvProfesional, @NombreCompletoProfesional, @CorreoProfesional, @EspecialidadProfesional,
                @UnidadId, @RutaPdfArchivado, @NombreArchivoOriginal, @HashArchivoSha256, @Observaciones
            );
            SELECT last_insert_rowid();
            """;

        try
        {
            return connection.QuerySingle<int>(sql, licencia, transaction);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new FolioDuplicadoException(licencia.Folio);
        }
    }

    public Licencia? ObtenerPorId(SqliteConnection connection, int licenciaId)
    {
        const string sql = """
            SELECT l.*, u.Descripcion AS UnidadDescripcion
            FROM Licencias l
            JOIN Unidades u ON u.UnidadId = l.UnidadId
            WHERE l.LicenciaId = @licenciaId;
            """;
        return connection.QuerySingleOrDefault<Licencia>(sql, new { licenciaId });
    }

    public bool Eliminar(SqliteConnection connection, int licenciaId)
    {
        const string sql = "DELETE FROM Licencias WHERE LicenciaId = @licenciaId;";
        return connection.Execute(sql, new { licenciaId }) > 0;
    }

    public (IReadOnlyList<Licencia> Items, int Total) Buscar(SqliteConnection connection, BusquedaLicenciasFiltro filtro)
    {
        var where = new List<string>();
        var parametros = new DynamicParameters();

        if (filtro.FechaDesde is not null)
        {
            where.Add("l.FechaInicioReposo >= @fechaDesde");
            parametros.Add("fechaDesde", filtro.FechaDesde.Value.ToString("yyyy-MM-dd"));
        }

        if (filtro.FechaHasta is not null)
        {
            where.Add("l.FechaInicioReposo <= @fechaHasta");
            parametros.Add("fechaHasta", filtro.FechaHasta.Value.ToString("yyyy-MM-dd"));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Rut))
        {
            where.Add("l.RutPacienteSinDv LIKE @rut");
            parametros.Add("rut", $"%{filtro.Rut.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            where.Add("l.NombreCompletoPaciente LIKE @nombre COLLATE NOCASE");
            parametros.Add("nombre", $"%{filtro.Nombre.Trim()}%");
        }

        if (filtro.UnidadId is not null)
        {
            where.Add("l.UnidadId = @unidadId");
            parametros.Add("unidadId", filtro.UnidadId.Value);
        }

        var whereSql = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;

        var totalSql = $"SELECT COUNT(1) FROM Licencias l {whereSql};";
        var total = connection.QuerySingle<int>(totalSql, parametros);

        var pageSize = filtro.PageSize <= 0 ? 50 : filtro.PageSize;
        var page = filtro.Page <= 0 ? 1 : filtro.Page;
        parametros.Add("limit", pageSize);
        parametros.Add("offset", (page - 1) * pageSize);

        var itemsSql = $"""
            SELECT l.*, u.Descripcion AS UnidadDescripcion
            FROM Licencias l
            JOIN Unidades u ON u.UnidadId = l.UnidadId
            {whereSql}
            ORDER BY l.FechaInicioReposo DESC, l.LicenciaId DESC
            LIMIT @limit OFFSET @offset;
            """;
        var items = connection.Query<Licencia>(itemsSql, parametros).ToList();

        return (items, total);
    }

    public IReadOnlyList<Licencia> ObtenerParaInforme(SqliteConnection connection, DateOnly fechaDesde, DateOnly fechaHasta, ModoFechaInforme modo)
    {
        var condicion = modo switch
        {
            ModoFechaInforme.FechaInicio => "l.FechaInicioReposo BETWEEN @fechaDesde AND @fechaHasta",
            ModoFechaInforme.FechaTermino => "l.FechaTerminoReposo BETWEEN @fechaDesde AND @fechaHasta",
            ModoFechaInforme.Interseccion => "l.FechaInicioReposo <= @fechaHasta AND l.FechaTerminoReposo >= @fechaDesde",
            _ => throw new ArgumentOutOfRangeException(nameof(modo), modo, null),
        };

        var sql = $"""
            SELECT l.*, u.Descripcion AS UnidadDescripcion
            FROM Licencias l
            JOIN Unidades u ON u.UnidadId = l.UnidadId
            WHERE {condicion}
            ORDER BY l.FechaInicioReposo, l.NombreCompletoPaciente COLLATE NOCASE;
            """;

        var parametros = new
        {
            fechaDesde = fechaDesde.ToString("yyyy-MM-dd"),
            fechaHasta = fechaHasta.ToString("yyyy-MM-dd"),
        };

        return connection.Query<Licencia>(sql, parametros).ToList();
    }

    public IReadOnlyList<UnidadConLicenciasEnFecha> UnidadesConLicenciasGrabadasEnFecha(SqliteConnection connection, DateOnly fecha)
    {
        const string sql = """
            SELECT u.UnidadId, u.Descripcion, u.CorreoElectronico, COUNT(l.LicenciaId) AS CantidadLicencias, ce.FechaHoraEnvio AS UltimoEnvio
            FROM Unidades u
            JOIN Licencias l ON l.UnidadId = u.UnidadId
            LEFT JOIN CorreosEnviados ce ON ce.UnidadId = u.UnidadId AND ce.Fecha = @fecha
            WHERE DATE(l.FechaIngresoSistema, 'localtime') = @fecha
            GROUP BY u.UnidadId, u.Descripcion, u.CorreoElectronico, ce.FechaHoraEnvio
            ORDER BY u.Descripcion COLLATE NOCASE;
            """;
        return connection.Query<UnidadConLicenciasEnFecha>(sql, new { fecha = fecha.ToString("yyyy-MM-dd") }).ToList();
    }

    public IReadOnlyList<Licencia> LicenciasDeUnidadGrabadasEnFecha(SqliteConnection connection, int unidadId, DateOnly fecha)
    {
        const string sql = """
            SELECT l.*, u.Descripcion AS UnidadDescripcion
            FROM Licencias l
            JOIN Unidades u ON u.UnidadId = l.UnidadId
            WHERE l.UnidadId = @unidadId AND DATE(l.FechaIngresoSistema, 'localtime') = @fecha
            ORDER BY l.NombreCompletoPaciente COLLATE NOCASE;
            """;
        return connection.Query<Licencia>(sql, new { unidadId, fecha = fecha.ToString("yyyy-MM-dd") }).ToList();
    }
}
