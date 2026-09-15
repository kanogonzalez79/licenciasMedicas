using System.Text.RegularExpressions;

namespace LicenciasMedicas.Core.Correos;

public static class EmailUtils
{
    private static readonly Regex EmailPattern = new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled);

    /// <summary>
    /// Valida el formato general de un correo electrónico (usuario@dominio.tld, sin espacios).
    /// Recorta espacios en blanco al inicio y al final antes de evaluar.
    /// </summary>
    public static bool EsFormatoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return false;

        return EmailPattern.IsMatch(correo.Trim());
    }
}
