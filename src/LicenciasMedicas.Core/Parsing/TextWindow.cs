namespace LicenciasMedicas.Core.Parsing;

/// <summary>
/// Recorta el texto extraído de un PDF entre la PRIMERA ocurrencia de un ancla de inicio
/// y la PRIMERA ocurrencia (después de esa) de un ancla de fin. Es la base de la estrategia
/// "ventanas por ancla": si el contenido de un formulario aparece repetido más adelante en el
/// texto (artefacto de render visto en los PDF Tipo 1), esta técnica lo ignora por construcción,
/// porque siempre se queda con el primer bloque.
/// </summary>
public static class TextWindow
{
    public static string? Extraer(string texto, string anclaInicio, string? anclaFin)
    {
        var inicioIdx = texto.IndexOf(anclaInicio, StringComparison.OrdinalIgnoreCase);
        if (inicioIdx < 0)
            return null;

        var desde = inicioIdx + anclaInicio.Length;
        if (anclaFin is null)
            return texto[desde..];

        var finIdx = texto.IndexOf(anclaFin, desde, StringComparison.OrdinalIgnoreCase);
        return finIdx < 0 ? texto[desde..] : texto[desde..finIdx];
    }
}
