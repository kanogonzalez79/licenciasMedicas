using System.Globalization;
using System.Text.RegularExpressions;
using LicenciasMedicas.Core.Catalogos;
using LicenciasMedicas.Core.Rut;

namespace LicenciasMedicas.Core.Parsing;

/// <summary>
/// Extrae datos del formulario "Tipo 2" a partir de su página de "Comprobante de Licencia
/// Médica Electrónica" (formato limpio "Etiqueta : Valor"), que es mucho más confiable que
/// reconstruir palabras desde los casilleros letra-por-letra de las páginas 1-3. Si el PDF no
/// trae esa página, se falla de forma segura (decisión de v1, sin fallback de casilleros).
/// </summary>
public static class Tipo2Extractor
{
    private const string AnclaComprobante = "Comprobante de Licencia Médica Electrónica";
    private const string AnclaDatosProfesional = "1. Datos Profesional";
    private const string AnclaDatosTrabajador = "2. Datos Trabajador";
    private const string AnclaDatosReposo = "3. Datos Reposo";
    private const string AnclaEstadoLicencia = "4. Estado de la licencia";

    private static readonly Regex FolioRegex = new(@"N°\s*\d+\s+(\d{1,10}-[\dkK])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RutRegex = new(@"Rut\s*:\s*(\d{1,8}-[\dkK])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NombreRegex = new(@"Nombre\s*:\s*(.+?)(?:\r?\n|$)", RegexOptions.Compiled);
    private static readonly Regex ProfesionalNombreRegex = new(@"Profesional\s*:\s*(.+?)(?:\r?\n|$)", RegexOptions.Compiled);
    private static readonly Regex EspecialidadRegex = new(@"Especialidad\s*:\s*(.+?)(?:\r?\n|$)", RegexOptions.Compiled);
    private static readonly Regex EdadRegex = new(@"Edad\s*:\s*(\d+)", RegexOptions.Compiled);
    private static readonly Regex SexoRegex = new(@"Sexo\s*:\s*(\w+)", RegexOptions.Compiled);
    private static readonly Regex TipoLicenciaRegex = new(@"Tipo Licencia\s*:\s*(\d+)\.", RegexOptions.Compiled);
    private static readonly Regex FechaInicioRegex = new(@"Fecha Inicio\s*:\s*(\d{2}-\d{2}-\d{4})", RegexOptions.Compiled);
    private static readonly Regex FechaTerminoRegex = new(@"Fecha\s*\r?\n?\s*t[eé]rmino\s*:\s*(\d{2}-\d{2}-\d{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DiasRegex = new(@"N°\s*Días\s*:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EmailRegex = new(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ExtractionResult Extraer(TextoPdf texto)
    {
        var paginaComprobante = texto.TextoPorPagina.FirstOrDefault(
            p => p.Contains(AnclaComprobante, StringComparison.OrdinalIgnoreCase));

        if (paginaComprobante is null)
            return ExtractionResult.Fail(MotivosFallo.Tipo2SinComprobante);

        var folioMatch = FolioRegex.Match(paginaComprobante);
        if (!folioMatch.Success)
            return ExtractionResult.Fail(MotivosFallo.FolioNoEncontrado);
        var folio = folioMatch.Groups[1].Value;

        var ventanaProfesional = TextWindow.Extraer(paginaComprobante, AnclaDatosProfesional, AnclaDatosTrabajador);
        var ventanaTrabajador = TextWindow.Extraer(paginaComprobante, AnclaDatosTrabajador, AnclaDatosReposo);
        var ventanaReposo = TextWindow.Extraer(paginaComprobante, AnclaDatosReposo, AnclaEstadoLicencia);

        if (ventanaTrabajador is null)
            return ExtractionResult.Fail(MotivosFallo.SeccionTrabajadorNoEncontrada);

        var nombreMatch = NombreRegex.Match(ventanaTrabajador);
        if (!nombreMatch.Success || !TrySplitApellidosNombres(nombreMatch.Groups[1].Value, out var apellidoPaterno, out var apellidoMaterno, out var nombres))
            return ExtractionResult.Fail(MotivosFallo.SeccionTrabajadorNoEncontrada);

        var rutMatch = RutRegex.Match(ventanaTrabajador);
        if (!rutMatch.Success || !RutUtils.TryParse(rutMatch.Groups[1].Value, out var rutPaciente))
            return ExtractionResult.Fail(MotivosFallo.RutPacienteInvalido);

        var edadMatch = EdadRegex.Match(ventanaTrabajador);
        int? edad = edadMatch.Success ? int.Parse(edadMatch.Groups[1].Value) : null;

        var sexoMatch = SexoRegex.Match(ventanaTrabajador);
        var sexo = sexoMatch.Success ? NormalizarSexo(sexoMatch.Groups[1].Value) : null;

        var codigoMatch = TipoLicenciaRegex.Match(ventanaTrabajador);
        if (!codigoMatch.Success || !int.TryParse(codigoMatch.Groups[1].Value, out var codigoTipoLicencia) || !CatalogoTipoLicencia.EsCodigoValido(codigoTipoLicencia))
            return ExtractionResult.Fail(MotivosFallo.TipoLicenciaInvalido);

        if (ventanaReposo is null)
            return ExtractionResult.Fail(MotivosFallo.FechasInvalidas);

        var fechaInicioMatch = FechaInicioRegex.Match(ventanaReposo);
        if (!fechaInicioMatch.Success || !TryParseFecha(fechaInicioMatch.Groups[1].Value, out var fechaInicio))
            return ExtractionResult.Fail(MotivosFallo.FechasInvalidas);

        var diasMatch = DiasRegex.Match(ventanaReposo);
        if (!diasMatch.Success || !int.TryParse(diasMatch.Groups[1].Value, out var dias) || dias <= 0)
            return ExtractionResult.Fail(MotivosFallo.DiasInvalidos);

        var fechaTerminoCalculada = fechaInicio.AddDays(dias - 1);
        string? observaciones = null;

        var fechaTerminoMatch = FechaTerminoRegex.Match(ventanaReposo);
        var fechaTermino = fechaTerminoCalculada;
        if (fechaTerminoMatch.Success && TryParseFecha(fechaTerminoMatch.Groups[1].Value, out var fechaTerminoExplicita))
        {
            fechaTermino = fechaTerminoExplicita;
            if (fechaTerminoExplicita != fechaTerminoCalculada)
                observaciones = "Fecha término no coincide con el cálculo (inicio + días - 1)";
        }

        string? nombreProfesional = null;
        string? correoProfesional = null;
        string? especialidad = null;
        RutValido? rutProfesional = null;
        if (ventanaProfesional is not null)
        {
            var profesionalNombreMatch = ProfesionalNombreRegex.Match(ventanaProfesional);
            if (profesionalNombreMatch.Success && TrySplitApellidosNombres(profesionalNombreMatch.Groups[1].Value, out var pPaterno, out var pMaterno, out var pNombres))
                nombreProfesional = $"{pNombres} {pPaterno} {pMaterno}".Trim();

            var profesionalRutMatch = RutRegex.Match(ventanaProfesional);
            if (profesionalRutMatch.Success && RutUtils.TryParse(profesionalRutMatch.Groups[1].Value, out var rutProf))
                rutProfesional = rutProf;

            var especialidadMatch = EspecialidadRegex.Match(ventanaProfesional);
            if (especialidadMatch.Success)
                especialidad = especialidadMatch.Groups[1].Value.Trim();

            var emailMatch = EmailRegex.Match(ventanaProfesional);
            if (emailMatch.Success)
                correoProfesional = emailMatch.Value;
        }

        // La página de "Comprobante" no siempre trae el correo del profesional (viene en las
        // páginas 1-3 con casillero); se busca ahí como best-effort, sin invalidar la fila si no aparece.
        if (correoProfesional is null)
        {
            var emailEnDocumento = EmailRegex.Match(texto.TextoCompleto);
            if (emailEnDocumento.Success)
                correoProfesional = emailEnDocumento.Value;
        }

        var nombreCompletoPaciente = $"{nombres} {apellidoPaterno} {apellidoMaterno}".Trim();

        var datos = new DatosLicenciaExtraidos
        {
            Folio = folio,
            TipoFormulario = TipoFormulario.Tipo2,
            RutPaciente = rutPaciente,
            ApellidoPaternoPaciente = apellidoPaterno,
            ApellidoMaternoPaciente = apellidoMaterno,
            NombresPaciente = nombres,
            NombreCompletoPaciente = nombreCompletoPaciente,
            EdadPaciente = edad,
            SexoPaciente = sexo,
            CodigoTipoLicencia = codigoTipoLicencia,
            FechaEmisionOtorgamiento = null,
            FechaInicioReposo = fechaInicio,
            FechaTerminoReposo = fechaTermino,
            CantidadDias = dias,
            RutProfesional = rutProfesional,
            NombreCompletoProfesional = nombreProfesional,
            CorreoProfesional = correoProfesional,
            EspecialidadProfesional = especialidad,
            Observaciones = observaciones,
        };

        return ExtractionResult.Ok(datos);
    }

    private static bool TrySplitApellidosNombres(string valor, out string apellidoPaterno, out string apellidoMaterno, out string nombres)
    {
        apellidoPaterno = apellidoMaterno = nombres = string.Empty;

        var partes = valor.Split(',', 2, StringSplitOptions.TrimEntries);
        if (partes.Length != 2)
            return false;

        var apellidos = Regex.Replace(partes[0].Trim(), @"\s+", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (apellidos.Length < 1)
            return false;

        apellidoPaterno = apellidos[0];
        apellidoMaterno = apellidos.Length > 1 ? string.Join(' ', apellidos.Skip(1)) : string.Empty;
        nombres = Regex.Replace(partes[1].Trim(), @"\s+", " ");
        return !string.IsNullOrEmpty(nombres);
    }

    private static bool TryParseFecha(string ddMmYyyy, out DateOnly fecha)
        => DateOnly.TryParseExact(ddMmYyyy, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);

    private static string NormalizarSexo(string valor) => valor.Trim().ToUpperInvariant() switch
    {
        var v when v.StartsWith('F') => "F",
        var v when v.StartsWith('M') => "M",
        _ => valor.Trim().ToUpperInvariant(),
    };
}
