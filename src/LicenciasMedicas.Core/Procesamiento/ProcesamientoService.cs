using System.Security.Cryptography;
using LicenciasMedicas.Core.Catalogos;
using LicenciasMedicas.Core.Data;
using LicenciasMedicas.Core.Licencias;
using LicenciasMedicas.Core.Parsing;
using LicenciasMedicas.Core.Paths;
using LicenciasMedicas.Core.Rut;

namespace LicenciasMedicas.Core.Procesamiento;

/// <summary>
/// Orquesta el flujo Procesar -&gt; staging -&gt; Grabar -&gt; archivado permanente descrito en el plan:
/// los PDF leídos con éxito se mueven a una carpeta de staging al presionar "Procesar" (para no
/// reprocesarlos), y solo salen de forma definitiva de "incoming" cuando se confirma "Grabar".
/// </summary>
public sealed class ProcesamientoService
{
    private readonly AppPaths _paths;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly LicenciaExtractionService _extraction;
    private readonly StagingRepository _stagingRepo;
    private readonly LicenciasRepository _licenciasRepo;

    public ProcesamientoService(
        AppPaths paths,
        SqliteConnectionFactory connectionFactory,
        LicenciaExtractionService extraction,
        StagingRepository stagingRepo,
        LicenciasRepository licenciasRepo)
    {
        _paths = paths;
        _connectionFactory = connectionFactory;
        _extraction = extraction;
        _stagingRepo = stagingRepo;
        _licenciasRepo = licenciasRepo;
    }

    /// <summary>
    /// Regla de arranque fija: cualquier fila pendiente en LicenciaRevision hace que su PDF
    /// vuelva a "incoming" y se borre la fila. Nunca se intenta "resumir" un lote entre
    /// reinicios del ejecutable.
    /// </summary>
    public void RecuperarHuerfanosAlArranque()
    {
        using var connection = _connectionFactory.Crear();
        var pendientes = _stagingRepo.ObtenerPendientes(connection);

        foreach (var pendiente in pendientes)
        {
            DevolverArchivoAIncoming(pendiente.RutaStaging, pendiente.NombreArchivoOriginal);
            _stagingRepo.EliminarRevision(connection, pendiente.RevisionId);
        }

        foreach (var loteId in _stagingRepo.ObtenerLotesId(connection))
        {
            _stagingRepo.EliminarLoteSiVacio(connection, loteId);
            var carpetaLote = _paths.RutaStagingLote(loteId);
            if (Directory.Exists(carpetaLote) && !Directory.EnumerateFileSystemEntries(carpetaLote).Any())
                Directory.Delete(carpetaLote);
        }
    }

    public ResultadoProcesar Procesar()
    {
        using var connection = _connectionFactory.Crear();

        var loteId = _stagingRepo.ObtenerLoteActivo(connection) ?? _stagingRepo.CrearLote(connection);
        var carpetaLote = _paths.RutaStagingLote(loteId);
        Directory.CreateDirectory(carpetaLote);

        var fallidas = new List<FilaFallida>();

        var archivos = Directory.Exists(_paths.IncomingDir)
            ? Directory.EnumerateFiles(_paths.IncomingDir, "*.pdf", SearchOption.TopDirectoryOnly).ToList()
            : new List<string>();

        foreach (var rutaArchivo in archivos)
        {
            var nombreArchivo = Path.GetFileName(rutaArchivo);

            string hash;
            try
            {
                hash = CalcularHashSha256(rutaArchivo);
            }
            catch (IOException)
            {
                // Archivo bloqueado/en uso (ej. aún copiándose): se omite, se reintenta en el próximo Procesar.
                continue;
            }

            if (_licenciasRepo.ExisteHash(connection, hash) || _stagingRepo.ExisteHashEnStaging(connection, hash))
            {
                fallidas.Add(new FilaFallida(nombreArchivo, "DUPLICADO_YA_PROCESADO"));
                continue;
            }

            var resultado = _extraction.ExtraerDesdeArchivo(rutaArchivo);
            if (!resultado.Exito)
            {
                fallidas.Add(new FilaFallida(nombreArchivo, resultado.MotivoFallo ?? "ERROR_DESCONOCIDO"));
                continue;
            }

            var datos = resultado.Datos!;

            if (_licenciasRepo.ExisteFolioOHash(connection, datos.Folio, hash) || _stagingRepo.ExisteFolioEnStaging(connection, datos.Folio))
            {
                fallidas.Add(new FilaFallida(nombreArchivo, "DUPLICADO_FOLIO_YA_EXISTENTE"));
                continue;
            }

            var rutaStagingDestino = ResolverRutaSinColision(carpetaLote, nombreArchivo);
            File.Move(rutaArchivo, rutaStagingDestino);

            var revision = new LicenciaRevision
            {
                LoteId = loteId,
                NombreArchivoOriginal = nombreArchivo,
                RutaStaging = rutaStagingDestino,
                HashArchivoSha256 = hash,
                Folio = datos.Folio,
                TipoFormulario = (int)datos.TipoFormulario,
                RutPacienteSinDv = datos.RutPaciente.Numero.ToString(),
                DvPaciente = datos.RutPaciente.Dv.ToString(),
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
                RutProfesionalSinDv = datos.RutProfesional?.Numero.ToString(),
                DvProfesional = datos.RutProfesional?.Dv.ToString(),
                NombreCompletoProfesional = datos.NombreCompletoProfesional,
                CorreoProfesional = datos.CorreoProfesional,
                EspecialidadProfesional = datos.EspecialidadProfesional,
                Observaciones = datos.Observaciones,
            };
            _stagingRepo.InsertarRevision(connection, revision);
        }

        var pendientes = _stagingRepo.ObtenerPendientes(connection);
        return new ResultadoProcesar { Pendientes = pendientes, Fallidas = fallidas };
    }

    public IReadOnlyList<LicenciaRevision> ObtenerPendientes()
    {
        using var connection = _connectionFactory.Crear();
        return _stagingRepo.ObtenerPendientes(connection);
    }

    public ResultadoGrabar Grabar(IReadOnlyList<AsignacionUnidad> asignaciones)
    {
        using var connection = _connectionFactory.Crear();
        var grabadas = new List<int>();
        var noGrabadas = new List<FilaNoGrabada>();

        foreach (var asignacion in asignaciones)
        {
            var revision = _stagingRepo.ObtenerPorId(connection, asignacion.RevisionId);
            if (revision is null)
            {
                noGrabadas.Add(new FilaNoGrabada(asignacion.RevisionId, "NO_ENCONTRADA"));
                continue;
            }

            var unidadId = asignacion.UnidadId ?? revision.UnidadId;
            if (unidadId is null)
            {
                noGrabadas.Add(new FilaNoGrabada(asignacion.RevisionId, "FALTA_UNIDAD"));
                continue;
            }

            if (asignacion.UnidadId is not null && asignacion.UnidadId != revision.UnidadId)
                _stagingRepo.ActualizarUnidad(connection, revision.RevisionId, asignacion.UnidadId.Value);

            var (rutaRelativa, rutaAbsoluta) = _paths.ResolverRutaArchivoPermanente(revision.Folio, revision.NombreCompletoPaciente, ".pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);
            File.Move(revision.RutaStaging, rutaAbsoluta);

            try
            {
                var licencia = new Licencia
                {
                    Folio = revision.Folio,
                    TipoFormulario = revision.TipoFormulario,
                    RutPacienteSinDv = revision.RutPacienteSinDv,
                    DvPaciente = revision.DvPaciente,
                    ApellidoPaternoPaciente = revision.ApellidoPaternoPaciente,
                    ApellidoMaternoPaciente = revision.ApellidoMaternoPaciente,
                    NombresPaciente = revision.NombresPaciente,
                    NombreCompletoPaciente = revision.NombreCompletoPaciente,
                    EdadPaciente = revision.EdadPaciente,
                    SexoPaciente = revision.SexoPaciente,
                    CodigoTipoLicencia = revision.CodigoTipoLicencia,
                    DescripcionTipoLicencia = revision.DescripcionTipoLicencia,
                    FechaEmisionOtorgamiento = revision.FechaEmisionOtorgamiento,
                    FechaInicioReposo = revision.FechaInicioReposo,
                    FechaTerminoReposo = revision.FechaTerminoReposo,
                    CantidadDias = revision.CantidadDias,
                    RutProfesionalSinDv = revision.RutProfesionalSinDv,
                    DvProfesional = revision.DvProfesional,
                    NombreCompletoProfesional = revision.NombreCompletoProfesional,
                    CorreoProfesional = revision.CorreoProfesional,
                    EspecialidadProfesional = revision.EspecialidadProfesional,
                    UnidadId = unidadId.Value,
                    RutaPdfArchivado = rutaRelativa,
                    NombreArchivoOriginal = revision.NombreArchivoOriginal,
                    HashArchivoSha256 = revision.HashArchivoSha256,
                    Observaciones = revision.Observaciones,
                    CorreoEnviado = asignacion.CorreoEnviado,
                };

                using var transaction = connection.BeginTransaction();
                _licenciasRepo.Insertar(connection, licencia, transaction);
                _stagingRepo.EliminarRevision(connection, revision.RevisionId, transaction);
                transaction.Commit();

                grabadas.Add(asignacion.RevisionId);
                _stagingRepo.EliminarLoteSiVacio(connection, revision.LoteId);
            }
            catch (FolioDuplicadoException)
            {
                File.Move(rutaAbsoluta, revision.RutaStaging);
                noGrabadas.Add(new FilaNoGrabada(asignacion.RevisionId, "FOLIO_DUPLICADO"));
            }
        }

        return new ResultadoGrabar { Grabadas = grabadas, NoGrabadas = noGrabadas };
    }

    public bool DescartarPendiente(int revisionId)
    {
        using var connection = _connectionFactory.Crear();
        var revision = _stagingRepo.ObtenerPorId(connection, revisionId);
        if (revision is null)
            return false;

        DevolverArchivoAIncoming(revision.RutaStaging, revision.NombreArchivoOriginal);
        _stagingRepo.EliminarRevision(connection, revisionId);
        _stagingRepo.EliminarLoteSiVacio(connection, revision.LoteId);
        return true;
    }

    private void DevolverArchivoAIncoming(string rutaStaging, string nombreArchivoOriginal)
    {
        if (!File.Exists(rutaStaging))
            return;

        var destino = ResolverRutaSinColision(_paths.IncomingDir, nombreArchivoOriginal);
        File.Move(rutaStaging, destino);
    }

    private static string ResolverRutaSinColision(string carpeta, string nombreArchivo)
    {
        var destino = Path.Combine(carpeta, nombreArchivo);
        if (!File.Exists(destino))
            return destino;

        var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);
        var extension = Path.GetExtension(nombreArchivo);
        var contador = 1;
        string candidato;
        do
        {
            candidato = Path.Combine(carpeta, $"{nombreBase}_{contador}{extension}");
            contador++;
        } while (File.Exists(candidato));

        return candidato;
    }

    private static string CalcularHashSha256(string rutaArchivo)
    {
        using var stream = File.OpenRead(rutaArchivo);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}
