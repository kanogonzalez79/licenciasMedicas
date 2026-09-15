namespace LicenciasMedicas.Core.Parsing;

public sealed class LicenciaExtractionService
{
    public ExtractionResult ExtraerDesdeArchivo(string rutaPdf)
    {
        TextoPdf texto;
        try
        {
            texto = DocumentClassifier.LeerTexto(rutaPdf);
        }
        catch (Exception)
        {
            return ExtractionResult.Fail(MotivosFallo.ErrorLecturaPdf);
        }

        var tipo = DocumentClassifier.Clasificar(texto);
        return tipo switch
        {
            TipoFormulario.Tipo1 => Tipo1Extractor.Extraer(texto),
            TipoFormulario.Tipo2 => Tipo2Extractor.Extraer(texto),
            _ => ExtractionResult.Fail(MotivosFallo.TipoNoReconocido),
        };
    }
}
