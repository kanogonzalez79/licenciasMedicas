namespace LicenciasMedicas.Core.Parsing;

public sealed class ExtractionResult
{
    public bool Exito { get; }
    public DatosLicenciaExtraidos? Datos { get; }
    public string? MotivoFallo { get; }

    private ExtractionResult(bool exito, DatosLicenciaExtraidos? datos, string? motivoFallo)
    {
        Exito = exito;
        Datos = datos;
        MotivoFallo = motivoFallo;
    }

    public static ExtractionResult Ok(DatosLicenciaExtraidos datos) => new(true, datos, null);

    public static ExtractionResult Fail(string motivo) => new(false, null, motivo);
}

public static class MotivosFallo
{
    public const string TipoNoReconocido = "TIPO_NO_RECONOCIDO";
    public const string RutPacienteInvalido = "RUT_PACIENTE_INVALIDO";
    public const string SeccionTrabajadorNoEncontrada = "SECCION_TRABAJADOR_NO_ENCONTRADA";
    public const string FechasInvalidas = "FECHAS_INVALIDAS";
    public const string DiasInvalidos = "DIAS_INVALIDOS";
    public const string TipoLicenciaInvalido = "TIPO_LICENCIA_INVALIDO";
    public const string FolioNoEncontrado = "FOLIO_NO_ENCONTRADO";
    public const string Tipo2SinComprobante = "TIPO2_SIN_COMPROBANTE";
    public const string ErrorLecturaPdf = "ERROR_LECTURA_PDF";
}
