using ClosedXML.Excel;
using LicenciasMedicas.Core.Informes;
using LicenciasMedicas.Core.Licencias;

namespace LicenciasMedicas.Core.Tests;

public sealed class InformeLicenciasExcelServiceTests
{
    private static Licencia LicenciaDePrueba() => new()
    {
        Folio = "FOLIO-1",
        TipoFormulario = 1,
        RutPacienteSinDv = "11111111",
        DvPaciente = "1",
        NombreCompletoPaciente = "Paciente De Prueba",
        DescripcionTipoLicencia = "Enfermedad o Accidente Común",
        FechaInicioReposo = "2026-02-10",
        FechaTerminoReposo = "2026-02-15",
        CantidadDias = 5,
        UnidadId = 1,
        RutaPdfArchivado = "prueba.pdf",
        NombreArchivoOriginal = "prueba.pdf",
        HashArchivoSha256 = "hash",
    };

    private static IXLWorksheet AbrirPrimeraHoja(byte[] bytes)
    {
        using var memoria = new MemoryStream(bytes);
        var libro = new XLWorkbook(memoria);
        return libro.Worksheet(1);
    }

    [Fact]
    public void Generar_LicenciaSinFechaDeOtorgamiento_DejaEsaColumnaVacia()
    {
        var licencia = LicenciaDePrueba();
        licencia.FechaEmisionOtorgamiento = null;

        var bytes = InformeLicenciasExcelService.Generar([licencia]);
        var hoja = AbrirPrimeraHoja(bytes);

        Assert.Equal("Fecha de otorgamiento", hoja.Cell(1, 8).GetString());
        Assert.Equal(string.Empty, hoja.Cell(2, 8).GetString());
    }

    [Fact]
    public void Generar_SinLicencias_DejaSoloLaFilaDeEncabezado()
    {
        var bytes = InformeLicenciasExcelService.Generar([]);
        var hoja = AbrirPrimeraHoja(bytes);

        Assert.Equal("RUT", hoja.Cell(1, 1).GetString());
        Assert.True(hoja.Cell(2, 1).IsEmpty());
    }
}
