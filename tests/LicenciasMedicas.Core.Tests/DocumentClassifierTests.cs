using LicenciasMedicas.Core.Parsing;

namespace LicenciasMedicas.Core.Tests;

public class DocumentClassifierTests
{
    [Fact]
    public void Clasificar_PdfTipo1_RetornaTipo1()
    {
        var texto = DocumentClassifier.LeerTexto(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tipo1.pdf"));
        Assert.Equal(TipoFormulario.Tipo1, DocumentClassifier.Clasificar(texto));
    }

    [Fact]
    public void Clasificar_PdfTipo2_RetornaTipo2()
    {
        var texto = DocumentClassifier.LeerTexto(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tipo2.pdf"));
        Assert.Equal(TipoFormulario.Tipo2, DocumentClassifier.Clasificar(texto));
    }

    [Fact]
    public void Clasificar_TextoSinAnclas_RetornaDesconocido()
    {
        var texto = new TextoPdf { TextoCompleto = "un documento cualquiera", TextoPorPagina = new[] { "un documento cualquiera" } };
        Assert.Equal(TipoFormulario.Desconocido, DocumentClassifier.Clasificar(texto));
    }
}
