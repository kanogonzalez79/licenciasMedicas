namespace LicenciasMedicas.Web.Respaldo;

/// <summary>
/// Muestra el diálogo nativo "Buscar carpeta" de Windows. Se ejecuta en un hilo STA
/// dedicado porque los diálogos de Windows Forms lo requieren y el hilo que atiende
/// la solicitud HTTP en Kestrel no lo es.
/// </summary>
public static class SelectorCarpeta
{
    /// <summary>Retorna la ruta elegida, o null si la persona usuaria canceló.</summary>
    public static string? Elegir()
    {
        string? rutaElegida = null;

        var hilo = new Thread(() =>
        {
            using var dialogo = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Elegir carpeta destino para el respaldo",
                UseDescriptionForTitle = true,
            };

            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                rutaElegida = dialogo.SelectedPath;
        });

        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        hilo.Join();

        return rutaElegida;
    }
}
