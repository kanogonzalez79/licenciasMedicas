using LicenciasMedicas.Core.Parsing;

namespace LicenciasMedicas.Core.Tests;

public class Tipo1ExtractorTests
{
    private static ExtractionResult ExtraerFixture()
    {
        var texto = DocumentClassifier.LeerTexto(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tipo1.pdf"));
        return Tipo1Extractor.Extraer(texto);
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
        Assert.Equal("24523866-5", datos.Folio);
    }

    [Fact]
    public void Extraer_PdfReal_DatosPaciente()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal(17000494, datos.RutPaciente.Numero);
        Assert.Equal('9', datos.RutPaciente.Dv);
        Assert.Equal("KARINA SCARLETTE VALDIVIA OYARCE", datos.NombreCompletoPaciente);
        Assert.Equal(37, datos.EdadPaciente);
        Assert.Equal("F", datos.SexoPaciente);
    }

    [Fact]
    public void Extraer_PdfReal_Reposo()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal(new DateOnly(2026, 9, 4), datos.FechaInicioReposo);
        Assert.Equal(7, datos.CantidadDias);
        Assert.Equal(new DateOnly(2026, 9, 10), datos.FechaTerminoReposo);
        Assert.Equal(1, datos.CodigoTipoLicencia);
    }

    [Fact]
    public void Extraer_PdfReal_DatosProfesional()
    {
        var datos = ExtraerFixture().Datos!;
        Assert.Equal("SEBASTIÁN GUSTAVO MIRANDA RODRÍGUEZ", datos.NombreCompletoProfesional);
        Assert.Equal(18361392, datos.RutProfesional!.Value.Numero);
        Assert.Equal('8', datos.RutProfesional!.Value.Dv);
        Assert.Equal("SMIRANDAR@DOCENTE.USS.CL", datos.CorreoProfesional);
    }
}
