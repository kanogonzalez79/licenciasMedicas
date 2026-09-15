using Dapper;
using Microsoft.Data.Sqlite;

namespace LicenciasMedicas.Core.Procesamiento;

public sealed class StagingRepository
{
    public string? ObtenerLoteActivo(SqliteConnection connection)
        => connection.QuerySingleOrDefault<string?>(
            "SELECT LoteId FROM LoteRevision ORDER BY FechaCreacion LIMIT 1;");

    public string CrearLote(SqliteConnection connection)
    {
        var loteId = Guid.NewGuid().ToString("N");
        connection.Execute("INSERT INTO LoteRevision (LoteId) VALUES (@loteId);", new { loteId });
        return loteId;
    }

    public bool ExisteHashEnStaging(SqliteConnection connection, string hash)
        => connection.QuerySingle<int>("SELECT COUNT(1) FROM LicenciaRevision WHERE HashArchivoSha256 = @hash;", new { hash }) > 0;

    public bool ExisteFolioEnStaging(SqliteConnection connection, string folio)
        => connection.QuerySingle<int>("SELECT COUNT(1) FROM LicenciaRevision WHERE Folio = @folio;", new { folio }) > 0;

    public int InsertarRevision(SqliteConnection connection, LicenciaRevision revision)
    {
        const string sql = """
            INSERT INTO LicenciaRevision (
                LoteId, NombreArchivoOriginal, RutaStaging, HashArchivoSha256, Folio, TipoFormulario,
                RutPacienteSinDv, DvPaciente, ApellidoPaternoPaciente, ApellidoMaternoPaciente, NombresPaciente,
                NombreCompletoPaciente, EdadPaciente, SexoPaciente, CodigoTipoLicencia, DescripcionTipoLicencia,
                FechaEmisionOtorgamiento, FechaInicioReposo, FechaTerminoReposo, CantidadDias,
                RutProfesionalSinDv, DvProfesional, NombreCompletoProfesional, CorreoProfesional, EspecialidadProfesional,
                UnidadId, Observaciones
            ) VALUES (
                @LoteId, @NombreArchivoOriginal, @RutaStaging, @HashArchivoSha256, @Folio, @TipoFormulario,
                @RutPacienteSinDv, @DvPaciente, @ApellidoPaternoPaciente, @ApellidoMaternoPaciente, @NombresPaciente,
                @NombreCompletoPaciente, @EdadPaciente, @SexoPaciente, @CodigoTipoLicencia, @DescripcionTipoLicencia,
                @FechaEmisionOtorgamiento, @FechaInicioReposo, @FechaTerminoReposo, @CantidadDias,
                @RutProfesionalSinDv, @DvProfesional, @NombreCompletoProfesional, @CorreoProfesional, @EspecialidadProfesional,
                @UnidadId, @Observaciones
            );
            SELECT last_insert_rowid();
            """;
        return connection.QuerySingle<int>(sql, revision);
    }

    public IReadOnlyList<LicenciaRevision> ObtenerPendientes(SqliteConnection connection)
        => connection.Query<LicenciaRevision>("SELECT * FROM LicenciaRevision ORDER BY FechaCreacion;").ToList();

    public LicenciaRevision? ObtenerPorId(SqliteConnection connection, int revisionId)
        => connection.QuerySingleOrDefault<LicenciaRevision>("SELECT * FROM LicenciaRevision WHERE RevisionId = @revisionId;", new { revisionId });

    public void ActualizarUnidad(SqliteConnection connection, int revisionId, int unidadId)
        => connection.Execute("UPDATE LicenciaRevision SET UnidadId = @unidadId WHERE RevisionId = @revisionId;", new { revisionId, unidadId });

    public void EliminarRevision(SqliteConnection connection, int revisionId, SqliteTransaction? transaction = null)
        => connection.Execute("DELETE FROM LicenciaRevision WHERE RevisionId = @revisionId;", new { revisionId }, transaction);

    public int ContarPorLote(SqliteConnection connection, string loteId)
        => connection.QuerySingle<int>("SELECT COUNT(1) FROM LicenciaRevision WHERE LoteId = @loteId;", new { loteId });

    public void EliminarLoteSiVacio(SqliteConnection connection, string loteId)
    {
        if (ContarPorLote(connection, loteId) == 0)
            connection.Execute("DELETE FROM LoteRevision WHERE LoteId = @loteId;", new { loteId });
    }

    public IReadOnlyList<string> ObtenerLotesId(SqliteConnection connection)
        => connection.Query<string>("SELECT LoteId FROM LoteRevision;").ToList();
}
