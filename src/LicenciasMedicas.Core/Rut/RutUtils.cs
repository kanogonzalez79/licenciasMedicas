using System.Text.RegularExpressions;

namespace LicenciasMedicas.Core.Rut;

public readonly record struct RutValido(long Numero, char Dv);

public static class RutUtils
{
    private static readonly Regex RutPattern = new(@"^(\d{1,8})-?([\dkK])$", RegexOptions.Compiled);

    /// <summary>
    /// Intenta parsear y validar un RUT chileno en cualquier formato con o sin guion
    /// (ej. "17000494-9", "170004949"). Valida el dígito verificador con módulo 11.
    /// </summary>
    public static bool TryParse(string? rutCrudo, out RutValido rut)
    {
        rut = default;
        if (string.IsNullOrWhiteSpace(rutCrudo))
            return false;

        var limpio = rutCrudo.Trim().Replace(".", "").Replace(" ", "");
        var match = RutPattern.Match(limpio);
        if (!match.Success)
            return false;

        if (!long.TryParse(match.Groups[1].Value, out var numero))
            return false;

        var dv = char.ToUpperInvariant(match.Groups[2].Value[0]);
        if (!EsDvValido(numero, dv))
            return false;

        rut = new RutValido(numero, dv);
        return true;
    }

    public static char CalcularDv(long numero)
    {
        int suma = 0;
        int factor = 2;
        while (numero > 0)
        {
            suma += (int)(numero % 10) * factor;
            numero /= 10;
            factor = factor == 7 ? 2 : factor + 1;
        }

        var resto = 11 - (suma % 11);
        return resto switch
        {
            11 => '0',
            10 => 'K',
            _ => (char)('0' + resto)
        };
    }

    public static bool EsDvValido(long numero, char dv) => CalcularDv(numero) == char.ToUpperInvariant(dv);

    public static string Formatear(RutValido rut) => $"{rut.Numero}-{rut.Dv}";
}
