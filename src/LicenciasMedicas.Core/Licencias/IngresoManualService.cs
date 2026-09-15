using System.Security.Cryptography;
using LicenciasMedicas.Core.Catalogos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Procesamiento;
using LicenciasMedicas.Core.Rut;
using LicenciasMedicas.Core.Unidades;

namespace LicenciasMedicas.Core.Licencias;

/// <summary>
/// Datos de una licencia tipeados a mano por la persona usuaria (ver capability
/// ingresar-licencia-manual), antes de validar. Los campos de reposo llegan ya resueltos
/// (el cálculo bidireccional fecha/días es responsabilidad del formulario), pero el servicio
/// igual revalida su coherencia porque el backend es la fuente de verdad.
/// </summary>
public sealed class DatosLicenciaManual
{
    public required string Folio { get; init; }
    public required string RutPaciente { get; init; }
    public string? ApellidoPaternoPaciente { get; init; }
    public string? ApellidoMaternoPaciente { get; init; }
    public string? NombresPaciente { get; init; }
    public required string NombreCompletoPaciente { get; init; }
    public int? EdadPaciente { get; init; }
    public string? SexoPaciente { get; init; }
    public required int CodigoTipoLicencia { get; init; }
    public DateOnly? FechaEmisionOtorgamiento { get; init; }
    public required DateOnly FechaInicioReposo { get; init; }
    public required DateOnly FechaTerminoReposo { get; init; }
    public required int CantidadDias { get; init; }
    public string? RutProfesional { get; init; }
    public string? NombreCompletoProfesional { get; init; }
    public string? CorreoProfesional { get; init; }
    public string? EspecialidadProfesional { get; init; }
    public required int UnidadId { get; init; }
    public string? Observaciones { get; init; }
}

public static class MotivosRechazoIngresoManual
{
    public const string FolioRequerido = "FOLIO_REQUERIDO";
    public const string NombrePacienteRequerido = "NOMBRE_PACIENTE_REQUERIDO";
    public const string RutProfesionalInvalido = "RUT_PROFESIONAL_INVALIDO";
    public const string FaltaUnidad = "FALTA_UNIDAD";
    public const string AdjuntoRequerido = "ADJUNTO_REQUERIDO";
    public const string AdjuntoFormatoInvalido = "ADJUNTO_FORMATO_INVALIDO";
    public const string AdjuntoDemasiadoGrande = "ADJUNTO_DEMASIADO_GRANDE";
    public const string DuplicadoFolioYaExistente = "DUPLICADO_FOLIO_YA_EXISTENTE";
    public const string DuplicadoYaProcesado = "DUPLICADO_YA_PROCESADO";
}

public sealed class IngresoManualResult
{
    public bool Exito { get; }
    public int? LicenciaId { get; }
    public string? Motivo { get; }

    private IngresoManualResult(bool exito, int? licenciaId, string? motivo)
    {
        Exito = exito;
        LicenciaId = licenciaId;
        Motivo = motivo;
    }

    public static IngresoManualResult Ok(int licenciaId) => new(true, licenciaId, null);

    public static IngresoManualResult Fail(string motivo) => new(false, null, motivo);
}

/// <summary>
/// Ingresa una licencia a mano, con su documento de respaldo, en un solo paso: valida los datos
/// y el documento, revisa duplicados contra las licencias grabadas y contra el staging del
/// ingreso automático, archiva el documento y graba la licencia con TipoFormulario = Manual.
/// </summary>
public sealed class IngresoManualService
{
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciasRepository _licenciasRepo;
    private readonly StagingRepository _stagingRepo;
    private readonly UnidadesRepository _unidadesRepo;

    public IngresoManualService(
        AppPaths paths,
        SqliteConnectionFactory connectionFactory,
        LicenciasRepository licenciasRepo,
        StagingRepository stagingRepo,
        UnidadesRepository unidadesRepo)
    {
        _paths = paths;
        _connectionFactory = connectionFactory;
        _licenciasRepo = licenciasRepo;
        _stagingRepo = stagingRepo;
        _unidadesRepo = unidadesRepo;
    }

    public IngresoManualResult Ingresar(DatosLicenciaManual datos, byte[] contenidoArchivo, string extensionArchivo, string nombreArchivoOriginal)
    {
        if (string.IsNullOrWhiteSpace(datos.Folio))
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.FolioRequerido);

        if (string.IsNullOrWhiteSpace(datos.NombreCompletoPaciente))
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.NombrePacienteRequerido);

        if (!RutUtils.TryParse(datos.RutPaciente, out var rutPaciente))
            return IngresoManualResult.Fail(MotivosFallo.RutPacienteInvalido);

        if (!CatalogoTipoLicencia.EsCodigoValido(datos.CodigoTipoLicencia))
            return IngresoManualResult.Fail(MotivosFallo.TipoLicenciaInvalido);

        if (datos.FechaTerminoReposo < datos.FechaInicioReposo)
            return IngresoManualResult.Fail(MotivosFallo.FechasInvalidas);

        var diasEsperados = datos.FechaTerminoReposo.DayNumber - datos.FechaInicioReposo.DayNumber + 1;
        if (datos.CantidadDias != diasEsperados || datos.CantidadDias <= 0)
            return IngresoManualResult.Fail(MotivosFallo.DiasInvalidos);

        RutValido? rutProfesional = null;
        if (!string.IsNullOrWhiteSpace(datos.RutProfesional))
        {
            if (!RutUtils.TryParse(datos.RutProfesional, out var rutProfesionalValido))
                return IngresoManualResult.Fail(MotivosRechazoIngresoManual.RutProfesionalInvalido);
            rutProfesional = rutProfesionalValido;
        }

        if (contenidoArchivo.Length == 0)
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.AdjuntoRequerido);

        using var connection = _connectionFactory.Crear();

        if (_unidadesRepo.ObtenerPorId(connection, datos.UnidadId) is null)
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.FaltaUnidad);

        var hash = Convert.ToHexString(SHA256.HashData(contenidoArchivo));

        // Mismo orden que ProcesamientoService.Procesar(): primero se descarta el archivo ya
        // procesado (mismo hash), luego el folio duplicado (que solo se conoce tras "leer" los datos).
        if (_licenciasRepo.ExisteHash(connection, hash) || _stagingRepo.ExisteHashEnStaging(connection, hash))
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.DuplicadoYaProcesado);

        if (_licenciasRepo.ExisteFolioOHash(connection, datos.Folio, hash) || _stagingRepo.ExisteFolioEnStaging(connection, datos.Folio))
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.DuplicadoFolioYaExistente);

        var (rutaRelativa, rutaAbsoluta) = _paths.ResolverRutaArchivoPermanente(datos.Folio, datos.NombreCompletoPaciente, extensionArchivo);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);
        File.WriteAllBytes(rutaAbsoluta, contenidoArchivo);

        try
        {
            var licencia = new Licencia
            {
                Folio = datos.Folio,
                TipoFormulario = (int)TipoFormulario.Manual,
                RutPacienteSinDv = rutPaciente.Numero.ToString(),
                DvPaciente = rutPaciente.Dv.ToString(),
                ApellidoPaternoPaciente = datos.ApellidoPaternoPaciente,
                ApellidoMaternoPaciente = datos.ApellidoMaternoPaciente,
                NombresPaciente = datos.NombresPaciente,
                NombreCompletoPaciente = datos.NombreCompletoPaciente,
                EdadPaciente = datos.EdadPaciente,
                SexoPaciente = datos.SexoPaciente,
                CodigoTipoLicencia = datos.CodigoTipoLicencia,
                DescripcionTipoLicencia = CatalogoTipoLicencia.Formatear(datos.CodigoTipoLicencia),
                FechaEmisionOtorgamiento = datos.FechaEmisionOtorgamiento?.ToString("yyyy-MM-dd"),
                FechaInicioReposo = datos.FechaInicioReposo.ToString("yyyy-MM-dd"),
                FechaTerminoReposo = datos.FechaTerminoReposo.ToString("yyyy-MM-dd"),
                CantidadDias = datos.CantidadDias,
                RutProfesionalSinDv = rutProfesional?.Numero.ToString(),
                DvProfesional = rutProfesional?.Dv.ToString(),
                NombreCompletoProfesional = datos.NombreCompletoProfesional,
                CorreoProfesional = datos.CorreoProfesional,
                EspecialidadProfesional = datos.EspecialidadProfesional,
                UnidadId = datos.UnidadId,
                RutaPdfArchivado = rutaRelativa,
                NombreArchivoOriginal = nombreArchivoOriginal,
                HashArchivoSha256 = hash,
                Observaciones = datos.Observaciones,
            };

            var licenciaId = _licenciasRepo.Insertar(connection, licencia);
            return IngresoManualResult.Ok(licenciaId);
        }
        catch (FolioDuplicadoException)
        {
            File.Delete(rutaAbsoluta);
            return IngresoManualResult.Fail(MotivosRechazoIngresoManual.DuplicadoFolioYaExistente);
        }
    }
}
