async function apiFetch<T>(url: string, options?: RequestInit): Promise<T> {
  const respuesta = await fetch(url, options);
  const contentType = respuesta.headers.get("content-type") ?? "";
  const cuerpo = contentType.includes("application/json") ? await respuesta.json() : null;

  if (!respuesta.ok) {
    const mensaje = cuerpo && typeof cuerpo === "object" && "error" in cuerpo ? String(cuerpo.error) : `Error ${respuesta.status}`;
    throw new Error(mensaje);
  }

  return cuerpo as T;
}

// -- Unidades --------------------------------------------------------------

export type Unidad = {
  unidadId: number;
  descripcion: string;
  correoElectronico: string | null;
  fechaCreacion: string;
  fechaModificacion: string | null;
};

export const unidadesApi = {
  listar: () => apiFetch<Unidad[]>("/api/unidades"),
  crear: (descripcion: string, correoElectronico: string) =>
    apiFetch<Unidad>("/api/unidades", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ descripcion, correoElectronico }),
    }),
  editar: (id: number, descripcion: string, correoElectronico: string) =>
    apiFetch<Unidad>(`/api/unidades/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ descripcion, correoElectronico }),
    }),
};

// -- Licencias ---------------------------------------------------------------

export type Licencia = {
  licenciaId: number;
  folio: string;
  tipoFormulario: number;
  rutPacienteSinDv: string;
  dvPaciente: string;
  apellidoPaternoPaciente: string | null;
  apellidoMaternoPaciente: string | null;
  nombresPaciente: string | null;
  nombreCompletoPaciente: string;
  edadPaciente: number | null;
  sexoPaciente: string | null;
  codigoTipoLicencia: number | null;
  descripcionTipoLicencia: string | null;
  fechaEmisionOtorgamiento: string | null;
  fechaInicioReposo: string;
  fechaTerminoReposo: string;
  cantidadDias: number;
  rutProfesionalSinDv: string | null;
  dvProfesional: string | null;
  nombreCompletoProfesional: string | null;
  correoProfesional: string | null;
  especialidadProfesional: string | null;
  unidadId: number;
  rutaPdfArchivado: string;
  nombreArchivoOriginal: string;
  hashArchivoSha256: string;
  fechaIngresoSistema: string;
  observaciones: string | null;
  unidadDescripcion: string | null;
  esIngresoManual: boolean;
  correoEnviado: boolean;
};

export type FiltroLicencias = {
  fechaDesde?: string;
  fechaHasta?: string;
  rut?: string;
  nombre?: string;
  unidadId?: string;
};

export type ResultadoBusquedaLicencias = {
  items: Licencia[];
  total: number;
};

export type ModoFechaInforme = "inicio" | "termino" | "interseccion";

export type NuevaLicenciaManual = {
  folio: string;
  rutPaciente: string;
  apellidoPaternoPaciente?: string;
  apellidoMaternoPaciente?: string;
  nombresPaciente?: string;
  nombreCompletoPaciente: string;
  codigoTipoLicencia: number;
  fechaInicioReposo: string;
  fechaTerminoReposo: string;
  cantidadDias: number;
  rutProfesional?: string;
  nombreCompletoProfesional?: string;
  correoProfesional?: string;
  especialidadProfesional?: string;
  unidadId: number;
  observaciones?: string;
  correoEnviado?: boolean;
  archivo: File;
};

export const licenciasApi = {
  buscar: (filtro: FiltroLicencias) => {
    const params = new URLSearchParams();
    Object.entries(filtro).forEach(([clave, valor]) => {
      if (valor !== null && valor !== undefined && valor !== "") params.set(clave, valor);
    });
    return apiFetch<ResultadoBusquedaLicencias>(`/api/licencias?${params.toString()}`);
  },
  pdfUrl: (id: number) => `/api/licencias/${id}/pdf`,
  informeUrl: (fechaDesde: string, fechaHasta: string, modo: ModoFechaInforme) => {
    const params = new URLSearchParams({ fechaDesde, fechaHasta, modo });
    return `/api/licencias/informe?${params.toString()}`;
  },
  eliminar: (id: number) => apiFetch<void>(`/api/licencias/${id}`, { method: "DELETE" }),
  ingresarManual: (datos: NuevaLicenciaManual) => {
    const formData = new FormData();
    formData.set("folio", datos.folio);
    formData.set("rutPaciente", datos.rutPaciente);
    if (datos.apellidoPaternoPaciente) formData.set("apellidoPaternoPaciente", datos.apellidoPaternoPaciente);
    if (datos.apellidoMaternoPaciente) formData.set("apellidoMaternoPaciente", datos.apellidoMaternoPaciente);
    if (datos.nombresPaciente) formData.set("nombresPaciente", datos.nombresPaciente);
    formData.set("nombreCompletoPaciente", datos.nombreCompletoPaciente);
    formData.set("codigoTipoLicencia", String(datos.codigoTipoLicencia));
    formData.set("fechaInicioReposo", datos.fechaInicioReposo);
    formData.set("fechaTerminoReposo", datos.fechaTerminoReposo);
    formData.set("cantidadDias", String(datos.cantidadDias));
    if (datos.rutProfesional) formData.set("rutProfesional", datos.rutProfesional);
    if (datos.nombreCompletoProfesional) formData.set("nombreCompletoProfesional", datos.nombreCompletoProfesional);
    if (datos.correoProfesional) formData.set("correoProfesional", datos.correoProfesional);
    if (datos.especialidadProfesional) formData.set("especialidadProfesional", datos.especialidadProfesional);
    formData.set("unidadId", String(datos.unidadId));
    if (datos.observaciones) formData.set("observaciones", datos.observaciones);
    if (datos.correoEnviado) formData.set("correoEnviado", "true");
    formData.set("archivo", datos.archivo);
    return apiFetch<Licencia>("/api/licencias/manual", { method: "POST", body: formData });
  },
  cambiarCorreoEnviado: (id: number, correoEnviado: boolean) =>
    apiFetch<void>(`/api/licencias/${id}/correo-enviado`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ correoEnviado }),
    }),
};

// -- Procesamiento -----------------------------------------------------------

export type LicenciaRevision = {
  revisionId: number;
  loteId: string;
  nombreArchivoOriginal: string;
  hashArchivoSha256: string;
  folio: string;
  tipoFormulario: number;
  rutPacienteSinDv: string;
  dvPaciente: string;
  apellidoPaternoPaciente: string | null;
  apellidoMaternoPaciente: string | null;
  nombresPaciente: string | null;
  nombreCompletoPaciente: string;
  edadPaciente: number | null;
  sexoPaciente: string | null;
  codigoTipoLicencia: number | null;
  descripcionTipoLicencia: string | null;
  fechaEmisionOtorgamiento: string | null;
  fechaInicioReposo: string;
  fechaTerminoReposo: string;
  cantidadDias: number;
  rutProfesionalSinDv: string | null;
  dvProfesional: string | null;
  nombreCompletoProfesional: string | null;
  correoProfesional: string | null;
  especialidadProfesional: string | null;
  unidadId: number | null;
  observaciones: string | null;
  fechaCreacion: string;
};

export type FilaFallida = {
  nombreArchivoOriginal: string;
  motivo: string;
};

export type FilaNoGrabada = {
  revisionId: number;
  motivo: string;
};

export type AsignacionUnidad = {
  revisionId: number;
  unidadId: number | null;
  correoEnviado: boolean;
};

export type ResultadoProcesar = {
  pendientes: LicenciaRevision[];
  fallidas: FilaFallida[];
};

export type ResultadoGrabar = {
  grabadas: number[];
  noGrabadas: FilaNoGrabada[];
};

export const procesamientoApi = {
  procesar: () => apiFetch<ResultadoProcesar>("/api/procesamiento/procesar", { method: "POST" }),
  pendientes: () => apiFetch<LicenciaRevision[]>("/api/procesamiento/pendientes"),
  grabar: (asignaciones: AsignacionUnidad[]) =>
    apiFetch<ResultadoGrabar>("/api/procesamiento/grabar", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(asignaciones),
    }),
  descartar: (revisionId: number) => apiFetch<void>(`/api/procesamiento/revision/${revisionId}`, { method: "DELETE" }),
};

// -- Correos -------------------------------------------------------------------

export type UnidadConLicenciasEnFecha = {
  unidadId: number;
  descripcion: string;
  correoElectronico: string | null;
  cantidadLicencias: number;
  ultimoEnvio: string | null;
};

export const correosApi = {
  unidadesEnFecha: (fecha: string) => apiFetch<UnidadConLicenciasEnFecha[]>(`/api/correos/unidades?fecha=${encodeURIComponent(fecha)}`),
  redactar: (unidadId: number, fecha: string) =>
    apiFetch<{ texto: string }>(`/api/correos/redactar?unidadId=${unidadId}&fecha=${encodeURIComponent(fecha)}`),
  enviar: (unidadId: number, fecha: string, texto: string) =>
    apiFetch<{ estado: string }>("/api/correos/enviar", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ unidadId, fecha, texto }),
    }),
};

// -- Configuración SMTP ----------------------------------------------------------

export type ConfiguracionSmtp = {
  host: string;
  puerto: number;
  usuario: string;
  remitente: string;
  usaSsl: boolean;
};

export type ResultadoPruebaSmtp = { exito: boolean; error: string | null };

export const configuracionApi = {
  obtenerSmtp: () => apiFetch<ConfiguracionSmtp | null>("/api/configuracion/smtp"),
  guardarSmtp: (datos: ConfiguracionSmtp & { contrasena: string }) =>
    apiFetch<ConfiguracionSmtp>("/api/configuracion/smtp", {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(datos),
    }),
  probarSmtp: (datos: { host: string; puerto: number; usuario: string; contrasena: string; usaSsl: boolean }) =>
    apiFetch<ResultadoPruebaSmtp>("/api/configuracion/smtp/probar", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(datos),
    }),
};

// -- Correos en copia (CC) -------------------------------------------------------

export type CorreoCopia = {
  correoCopiaId: number;
  correoElectronico: string;
  activo: boolean;
};

export const correosCopiaApi = {
  listar: () => apiFetch<CorreoCopia[]>("/api/configuracion/correos-copia"),
  agregar: (correoElectronico: string) =>
    apiFetch<CorreoCopia>("/api/configuracion/correos-copia", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ correoElectronico }),
    }),
  cambiarActivo: (id: number, activo: boolean) =>
    apiFetch<void>(`/api/configuracion/correos-copia/${id}`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ activo }),
    }),
  eliminar: (id: number) => apiFetch<void>(`/api/configuracion/correos-copia/${id}`, { method: "DELETE" }),
};

// -- Respaldo ------------------------------------------------------------------

export type ResultadoRespaldo =
  | { estado: "cancelado" }
  | { estado: "completado"; pdfsCopiados: number; pdfsEliminados: number };

export const respaldoApi = {
  ejecutar: () => apiFetch<ResultadoRespaldo>("/api/respaldo/", { method: "POST" }),
};
