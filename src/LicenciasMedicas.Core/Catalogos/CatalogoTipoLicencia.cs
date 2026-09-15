namespace LicenciasMedicas.Core.Catalogos;

public static class CatalogoTipoLicencia
{
    public static readonly IReadOnlyDictionary<int, string> Descripciones = new Dictionary<int, string>
    {
        { 1, "Enfermedad o Accidente Común" },
        { 2, "Prórroga Medicina Preventiva" },
        { 3, "Licencia Maternal Pre y Post Natal" },
        { 4, "Enfermedad Grave Hijo Menor de 1 Año" },
        { 5, "Accidente del Trabajo o del Trayecto" },
        { 6, "Enfermedad Profesional" },
        { 7, "Patología del Embarazo" },
    };

    public static bool EsCodigoValido(int codigo) => Descripciones.ContainsKey(codigo);

    public static string Descripcion(int codigo) => Descripciones.GetValueOrDefault(codigo, "Desconocido");

    public static string Formatear(int codigo) => $"{codigo} - {Descripcion(codigo)}";
}
