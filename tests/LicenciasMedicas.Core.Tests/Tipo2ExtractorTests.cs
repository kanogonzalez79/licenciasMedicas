using LicenciasMedicas.Core.Parsing;

namespace LicenciasMedicas.Core.Tests;

public class Tipo2ExtractorTests
{
    private static ExtractionResult ExtraerFixture()
    {
        var texto = DocumentClassifier.LeerTexto(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tipo2.pdf"));
        return Tipo2Extractor.Extraer(texto);
    }

    [Fact]
    public void Extraer_PdfReal_Exito()
    {
        var resultado = ExtraerFixture();
        Assert.True(resultado.Exito, resultado.MotivoFallo);
    }

    [Fact]
    public void Extraer_PdfReal_Folio()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal("130322980-5", datos.Folio);
    }

    [Fact]
    public void Extraer_PdfReal_DatosPaciente()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal(20124969, datos.RutPaciente.Numero);
        Assert.Equal('4', datos.RutPaciente.Dv);
        Assert.Equal("TIARE GEOMARA SALAS PARRA", datos.NombreCompletoPaciente);
        Assert.Equal(27, datos.EdadPaciente);
        Assert.Equal("F", datos.SexoPaciente);
    }

    [Fact]
    public void Extraer_PdfReal_Reposo()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal(new DateOnly(2026, 9, 2), datos.FechaInicioReposo);
        Assert.Equal(30, datos.CantidadDias);
        Assert.Equal(new DateOnly(2026, 10, 1), datos.FechaTerminoReposo);
        Assert.Equal(1, datos.CodigoTipoLicencia);
        Assert.Null(datos.Observaciones);
    }

    [Fact]
    public void Extraer_PdfReal_DatosProfesional()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal("YOEL LARREAL FERNANDEZ", datos.NombreCompletoProfesional);
        Assert.Equal(27446954, datos.RutProfesional!.Value.Numero);
        Assert.Equal('4', datos.RutProfesional!.Value.Dv);
        Assert.Equal("administracion@dalesalud.net", datos.CorreoProfesional);
        Assert.Equal("MEDICINA GENERAL", datos.EspecialidadProfesional);
    }

    [Fact]
    public void Extraer_SinPaginaDeComprobante_FallaDeFormaSegura()
    {
        var texto = new TextoPdf
        {
            TextoCompleto = "Este formulario es válido según lo establecido en la Resolución 608",
            TextoPorPagina = new[] { "Página 1 sin comprobante, Resolución 608" },
        };

        var resultado = Tipo2Extractor.Extraer(texto);

        Assert.False(resultado.Exito);
        Assert.Equal(MotivosFallo.Tipo2SinComprobante, resultado.MotivoFallo);
    }
}
