using Dapper;
using LicenciasMedicas.Core.Data;

namespace LicenciasMedicas.Core.Correos;

/// <summary>
/// Guarda y gestiona la lista de correos en copia (CC) que se agregan a todo aviso de licencias
/// enviado, sin importar la unidad. Ver capability "correos-copia" y design.md del cambio
/// "correos-copia" para las decisiones de duplicados y de activar/desactivar vs. eliminar.
/// </summary>
public sealed class CorreoCopiaService
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public CorreoCopiaService(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<CorreoCopia> Listar()
    {
        using var connection = _connectionFactory.Crear();
        return connection.Query<CorreoCopia>(
            "SELECT CorreoCopiaId, CorreoElectronico, Activo FROM CorreoCopia ORDER BY CorreoElectronico COLLATE NOCASE;").AsList();
    }

    /// <summary>Correos en copia activos, para uso interno al enviar un correo.</summary>
    internal IReadOnlyList<string> ListarActivos()
    {
        using var connection = _connectionFactory.Crear();
        return connection.Query<string>(
            "SELECT CorreoElectronico FROM CorreoCopia WHERE Activo = 1 ORDER BY CorreoElectronico COLLATE NOCASE;").AsList();
    }

    public CorreoCopia Agregar(string correoElectronico)
    {
        var correo = correoElectronico.Trim();
        if (!EmailUtils.EsFormatoValido(correo))
            throw new CorreoCopiaInvalidoException();

        using var connection = _connectionFactory.Crear();

        var yaExiste = connection.QuerySingle<int>(
            "SELECT COUNT(1) FROM CorreoCopia WHERE CorreoElectronico = @correo COLLATE NOCASE;", new { correo }) > 0;
        if (yaExiste)
            throw new CorreoCopiaDuplicadoException();

        var id = connection.QuerySingle<int>(
            """
            INSERT INTO CorreoCopia (CorreoElectronico, Activo) VALUES (@correo, 1);
            SELECT last_insert_rowid();
            """, new { correo });

        return new CorreoCopia { CorreoCopiaId = id, CorreoElectronico = correo, Activo = true };
    }

    public void CambiarActivo(int id, bool activo)
    {
        using var connection = _connectionFactory.Crear();
        connection.Execute("UPDATE CorreoCopia SET Activo = @activo WHERE CorreoCopiaId = @id;", new { id, activo });
    }

    public void Eliminar(int id)
    {
        using var connection = _connectionFactory.Crear();
        connection.Execute("DELETE FROM CorreoCopia WHERE CorreoCopiaId = @id;", new { id });
    }
}
