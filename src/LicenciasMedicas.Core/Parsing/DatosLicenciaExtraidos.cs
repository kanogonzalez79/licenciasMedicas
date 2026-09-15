using LicenciasMedicas.Core.Rut;

namespace LicenciasMedicas.Core.Parsing;

public sealed class DatosLicenciaExtraidos
{
    public required string Folio { get; init; }
    public required TipoFormulario TipoFormulario { get; init; }

    public required RutValido RutPaciente { get; init; }
    public string? ApellidoPaternoPaciente { get; init; }
    public string? ApellidoMaternoPaciente { get; init; }
    public string? NombresPaciente { get; init; }
    public required string NombreCompletoPaciente { get; init; }
    public int? EdadPaciente { get; init; }
    public string? SexoPaciente { get; init; }

    public required int CodigoTipoLicencia { get; init; }

    public DateOnly? FechaEmisionOtorgamiento { get; init; }
    public required DateOnly FechaInicioReposo { get; init; }
    public required DateOnly FechaTerminoReposo { get; init; }
    public required int CantidadDias { get; init; }

    public RutValido? RutProfesional { get; init; }
    public string? NombreCompletoProfesional { get; init; }
    public string? CorreoProfesional { get; init; }
    public string? EspecialidadProfesional { get; init; }

    public string? Observaciones { get; init; }
}
