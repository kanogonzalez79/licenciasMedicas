using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace LicenciasMedicas.Core.Parsing;

public sealed class TextoPdf
{
    public required string TextoCompleto { get; init; }
    public required IReadOnlyList<string> TextoPorPagina { get; init; }
}

public static class DocumentClassifier
{
    private const string AnclaTipo1 = "SECCION 0:";
    private const string AnclaTipo2Comprobante = "Comprobante de Licencia Médica Electrónica";
    private const string AnclaTipo2Resolucion = "Resolución 608";

    public static TextoPdf LeerTexto(string rutaPdf)
    {
        using var documento = PdfDocument.Open(rutaPdf);
        var paginas = new List<string>(documento.NumberOfPages);
        foreach (var pagina in documento.GetPages())
            paginas.Add(ContentOrderTextExtractor.GetText(pagina));

        return new TextoPdf
        {
            TextoCompleto = string.Join("\n", paginas),
            TextoPorPagina = paginas,
        };
    }

    public static TipoFormulario Clasificar(TextoPdf texto)
    {
        if (texto.TextoCompleto.Contains(AnclaTipo1, StringComparison.OrdinalIgnoreCase))
            return TipoFormulario.Tipo1;

        if (texto.TextoCompleto.Contains(AnclaTipo2Comprobante, StringComparison.OrdinalIgnoreCase)
            || texto.TextoCompleto.Contains(AnclaTipo2Resolucion, StringComparison.OrdinalIgnoreCase))
            return TipoFormulario.Tipo2;

        return TipoFormulario.Desconocido;
    }
}
