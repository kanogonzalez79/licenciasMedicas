namespace LicenciasMedicas.Core.Unidades;

public sealed class Unidad
{
    public int UnidadId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? CorreoElectronico { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
    public string? FechaModificacion { get; set; }
}
