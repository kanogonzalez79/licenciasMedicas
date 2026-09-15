namespace LicenciasMedicas.Core.Procesamiento;

public sealed class LicenciaRevision
{
    public int RevisionId { get; set; }
    public string LoteId { get; set; } = string.Empty;
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public string RutaStaging { get; set; } = string.Empty;
    public string HashArchivoSha256 { get; set; } = string.Empty;
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
    public int? UnidadId { get; set; }
    public string? Observaciones { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
}

public sealed record FilaFallida(string NombreArchivoOriginal, string Motivo);

public sealed record FilaNoGrabada(int RevisionId, string Motivo);

public sealed record AsignacionUnidad(int RevisionId, int? UnidadId);

public sealed class ResultadoProcesar
{
    public required IReadOnlyList<LicenciaRevision> Pendientes { get; init; }
    public required IReadOnlyList<FilaFallida> Fallidas { get; init; }
}

public sealed class ResultadoGrabar
{
    public required IReadOnlyList<int> Grabadas { get; init; }
    public required IReadOnlyList<FilaNoGrabada> NoGrabadas { get; init; }
}
