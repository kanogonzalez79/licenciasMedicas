using ClosedXML.Excel;
using LicenciasMedicas.Core.Licencias;

namespace LicenciasMedicas.Core.Informes;

public static class InformeLicenciasExcelService
{
    private static readonly string[] Encabezados =
    [
        "RUT", "DV", "Nombre completo", "Folio", "Tipo de licencia",
        "Fecha inicio", "Fecha término", "Fecha de otorgamiento", "N° de días",
    ];

    public static byte[] Generar(IReadOnlyList<Licencia> licencias)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Licencias");

        for (var columna = 0; columna < Encabezados.Length; columna++)
            hoja.Cell(1, columna + 1).Value = Encabezados[columna];

        for (var indice = 0; indice < licencias.Count; indice++)
        {
            var licencia = licencias[indice];
            var fila = indice + 2;

            EscribirTexto(hoja.Cell(fila, 1), licencia.RutPacienteSinDv);
            EscribirTexto(hoja.Cell(fila, 2), licencia.DvPaciente);
            hoja.Cell(fila, 3).Value = licencia.NombreCompletoPaciente;
            EscribirTexto(hoja.Cell(fila, 4), licencia.Folio);
            hoja.Cell(fila, 5).Value = licencia.DescripcionTipoLicencia ?? string.Empty;
            EscribirTexto(hoja.Cell(fila, 6), licencia.FechaInicioReposo);
            EscribirTexto(hoja.Cell(fila, 7), licencia.FechaTerminoReposo);
            EscribirTexto(hoja.Cell(fila, 8), licencia.FechaEmisionOtorgamiento ?? string.Empty);
            hoja.Cell(fila, 9).Value = licencia.CantidadDias;
        }

        hoja.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    /// <summary>Fuerza formato de texto para no depender de que Excel adivine el tipo (RUT/folio numéricos, fechas ISO).</summary>
    private static void EscribirTexto(IXLCell celda, string valor)
    {
        celda.SetValue(valor);
        celda.Style.NumberFormat.Format = "@";
    }
}
