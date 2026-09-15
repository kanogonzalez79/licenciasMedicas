using System.Text.RegularExpressions;
using LicenciasMedicas.Core.Catalogos;
using LicenciasMedicas.Core.Rut;

namespace LicenciasMedicas.Core.Parsing;

/// <summary>
/// Extrae datos del formulario "Tipo 1" (una sola página, texto plano, ancla "SECCION 0:").
/// El contenido del formulario puede aparecer repetido 2-3 veces dentro del texto extraído de
/// la misma página (artefacto de render observado en los ejemplos reales); por eso todo el
/// parseo se hace con TextWindow, que siempre toma la primera ocurrencia de cada ancla.
/// </summary>
public static class Tipo1Extractor
{
    private const string AnclaA1 = "A.1 IDENTIFICACION DEL TRABAJADOR";
    private const string AnclaA3 = "A.3 TIPO DE LICENCIA";
    private const string AnclaA4 = "A.4 CARACTERISTICAS DEL REPOSO";
    private const string AnclaA5 = "A.5 IDENTIFICACION DEL PROFESIONAL";
    private const string AnclaA6 = "A.6 DIAGNOSTICO";

    private static readonly Regex FolioRegex = new(@"Folio:\s*(\d{1,10}-[\dkK])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PacienteRegex = new(
        @"([A-ZÁÉÍÓÚÑ]+)\s+([A-ZÁÉÍÓÚÑ]+)\s+([A-ZÁÉÍÓÚÑ\s]+?)\s+(\d{1,8}-[\dkK])\s+(\d{1,3})\s+([MF])\b",
        RegexOptions.Compiled);

    private static readonly Regex ProfesionalRegex = new(
        @"([A-ZÁÉÍÓÚÑ]+)\s+([A-ZÁÉÍÓÚÑ]+)\s+([A-ZÁÉÍÓÚÑ\s]+?)\s+(\d{1,8}-[\dkK])",
        RegexOptions.Compiled);

    private static readonly Regex FechaEmisionRegex = new(
        @"FECHA EMISION LICENCIA\s*(\d{2})\s*Dia\s*(\d{2})\s*Mes\s*(\d{2})\s*Ano",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FechaInicioRegex = new(
        @"FECHA INICIO DE REPOSO\s*(\d{2})\s*Dia\s*(\d{2})\s*Mes\s*(\d{2})\s*Ano",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DiasRegex = new(
        @"N DE DIAS\s*(\d+)\s*N DE DIAS EN PALABRAS",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EmailRegex = new(
        @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ExtractionResult Extraer(TextoPdf texto)
    {
        var t = texto.TextoCompleto;

        var folioMatch = FolioRegex.Match(t);
        if (!folioMatch.Success)
            return ExtractionResult.Fail(MotivosFallo.FolioNoEncontrado);
        var folio = folioMatch.Groups[1].Value;

        var ventanaA1 = TextWindow.Extraer(t, AnclaA1, AnclaA3);
        if (ventanaA1 is null)
            return ExtractionResult.Fail(MotivosFallo.SeccionTrabajadorNoEncontrada);

        var pacienteMatch = PacienteRegex.Match(ventanaA1);
        if (!pacienteMatch.Success)
            return ExtractionResult.Fail(MotivosFallo.SeccionTrabajadorNoEncontrada);

        var apellidoPaterno = pacienteMatch.Groups[1].Value;
        var apellidoMaterno = pacienteMatch.Groups[2].Value;
        var nombres = CollapseSpaces(pacienteMatch.Groups[3].Value);
        var rutPacienteCrudo = pacienteMatch.Groups[4].Value;
        var edad = int.Parse(pacienteMatch.Groups[5].Value);
        var sexo = pacienteMatch.Groups[6].Value;

        if (!RutUtils.TryParse(rutPacienteCrudo, out var rutPaciente))
            return ExtractionResult.Fail(MotivosFallo.RutPacienteInvalido);

        var fechaEmisionMatch = FechaEmisionRegex.Match(ventanaA1);
        DateOnly? fechaEmision = fechaEmisionMatch.Success
            ? ConstruirFecha(fechaEmisionMatch.Groups[1].Value, fechaEmisionMatch.Groups[2].Value, fechaEmisionMatch.Groups[3].Value)
            : null;

        var fechaInicioMatch = FechaInicioRegex.Match(ventanaA1);
        if (!fechaInicioMatch.Success)
            return ExtractionResult.Fail(MotivosFallo.FechasInvalidas);
        var fechaInicio = ConstruirFecha(fechaInicioMatch.Groups[1].Value, fechaInicioMatch.Groups[2].Value, fechaInicioMatch.Groups[3].Value);
        if (fechaInicio is null)
            return ExtractionResult.Fail(MotivosFallo.FechasInvalidas);

        var diasMatch = DiasRegex.Match(ventanaA1);
        if (!diasMatch.Success || !int.TryParse(diasMatch.Groups[1].Value, out var dias) || dias <= 0)
            return ExtractionResult.Fail(MotivosFallo.DiasInvalidos);

        var fechaTermino = fechaInicio.Value.AddDays(dias - 1);

        var ventanaA3 = TextWindow.Extraer(t, AnclaA3, AnclaA4);
        var codigoMatch = ventanaA3 is null ? null : Regex.Match(ventanaA3.TrimStart(), @"^(\d)");
        if (codigoMatch is null || !codigoMatch.Success)
            return ExtractionResult.Fail(MotivosFallo.TipoLicenciaInvalido);
        var codigoTipoLicencia = int.Parse(codigoMatch.Groups[1].Value);
        if (!CatalogoTipoLicencia.EsCodigoValido(codigoTipoLicencia))
            return ExtractionResult.Fail(MotivosFallo.TipoLicenciaInvalido);

        string? nombreProfesional = null;
        string? correoProfesional = null;
        RutValido? rutProfesional = null;
        var ventanaA5 = TextWindow.Extraer(t, AnclaA5, AnclaA6);
        if (ventanaA5 is not null)
        {
            var profesionalMatch = ProfesionalRegex.Match(ventanaA5);
            if (profesionalMatch.Success)
            {
                var pApellidoPaterno = profesionalMatch.Groups[1].Value;
                var pApellidoMaterno = profesionalMatch.Groups[2].Value;
                var pNombres = CollapseSpaces(profesionalMatch.Groups[3].Value);
                nombreProfesional = $"{pNombres} {pApellidoPaterno} {pApellidoMaterno}".Trim();

                if (RutUtils.TryParse(profesionalMatch.Groups[4].Value, out var rutProf))
                    rutProfesional = rutProf;
            }

            var emailMatch = EmailRegex.Match(ventanaA5);
            if (emailMatch.Success)
                correoProfesional = emailMatch.Value;
        }

        var nombreCompletoPaciente = $"{nombres} {apellidoPaterno} {apellidoMaterno}".Trim();

        var datos = new DatosLicenciaExtraidos
        {
            Folio = folio,
            TipoFormulario = TipoFormulario.Tipo1,
            RutPaciente = rutPaciente,
            ApellidoPaternoPaciente = apellidoPaterno,
            ApellidoMaternoPaciente = apellidoMaterno,
            NombresPaciente = nombres,
            NombreCompletoPaciente = nombreCompletoPaciente,
            EdadPaciente = edad,
            SexoPaciente = sexo,
            CodigoTipoLicencia = codigoTipoLicencia,
            FechaEmisionOtorgamiento = fechaEmision,
            FechaInicioReposo = fechaInicio.Value,
            FechaTerminoReposo = fechaTermino,
            CantidadDias = dias,
            RutProfesional = rutProfesional,
            NombreCompletoProfesional = nombreProfesional,
            CorreoProfesional = correoProfesional,
            EspecialidadProfesional = null,
        };

        return ExtractionResult.Ok(datos);
    }

    private static DateOnly? ConstruirFecha(string dd, string mm, string yy)
    {
        if (!int.TryParse(dd, out var dia) || !int.TryParse(mm, out var mes) || !int.TryParse(yy, out var anioCorto))
            return null;

        var anio = 2000 + anioCorto;
        try
        {
            return new DateOnly(anio, mes, dia);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string CollapseSpaces(string s) => Regex.Replace(s.Trim(), @"\s+", " ");
}
