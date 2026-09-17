namespace LicenciasMedicas.Core.Licencias;

public sealed class Licencia
{
    public int LicenciaId { get; set; }
    public string Folio { get; set; } = string.Empty;
    public int TipoFormulario { get; set; }
    public string RutPacienteSinDv { get; set; } = string.Empty;
    public string DvPaciente { get; set; } = string.Empty;
    public string? ApellidoPaternoPaciente { get; set; }
    public string? ApellidoMaternoPaciente { get; set; }
    public string? NombresPaciente { get; set; }
    public string NombreCompletoPaciente { get; set; } = string.Empty;
    public int? EdadPaciente { get; set; }
    public string? SexoPaciente { get; set; }
    public int? CodigoTipoLicencia { get; set; }
    public string? DescripcionTipoLicencia { get; set; }
    public string? FechaEmisionOtorgamiento { get; set; }
    public string FechaInicioReposo { get; set; } = string.Empty;
    public string FechaTerminoReposo { get; set; } = string.Empty;
    public int CantidadDias { get; set; }
    public string? RutProfesionalSinDv { get; set; }
    public string? DvProfesional { get; set; }
    public string? NombreCompletoProfesional { get; set; }
    public string? CorreoProfesional { get; set; }
    public string? EspecialidadProfesional { get; set; }
    public int UnidadId { get; set; }
    public string RutaPdfArchivado { get; set; } = string.Empty;
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public string HashArchivoSha256 { get; set; } = string.Empty;
    public string FechaIngresoSistema { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // Solo presente en resultados de búsqueda (JOIN con Unidades).
    public string? UnidadDescripcion { get; set; }

    /// <summary>Calculado, no se persiste: distingue las licencias tipeadas a mano de las leídas por el parser.</summary>
    public bool EsIngresoManual => TipoFormulario == (int)Parsing.TipoFormulario.Manual;
}

public sealed class BusquedaLicenciasFiltro
{
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
    public string? Rut { get; init; }
    public string? Nombre { get; init; }
    public int? UnidadId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>A qué fecha de la licencia aplica el rango consultado en el informe Excel.</summary>
public enum ModoFechaInforme
{
    FechaInicio,
    FechaTermino,
    Interseccion,
}

public sealed class UnidadConLicenciasEnFecha
{
    public int UnidadId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? CorreoElectronico { get; set; }
    public int CantidadLicencias { get; set; }

    /// <summary>Fecha/hora ISO del último envío exitoso del correo para esta unidad y fecha, o null si nunca se envió.</summary>
    public string? UltimoEnvio { get; set; }
}
