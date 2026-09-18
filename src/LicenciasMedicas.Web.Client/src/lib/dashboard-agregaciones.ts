import type { Licencia, ModoFechaInforme } from "./api";

export type ItemRanking = { etiqueta: string; cantidad: number };

export type TotalesVolumen = {
  totalLicencias: number;
  totalDiasReposo: number;
  promedioDias: number;
};

export type ProporcionIngreso = {
  manual: number;
  automatico: number;
};

export type Granularidad = "dia" | "semana" | "mes";

export type PuntoSerieTemporal = { etiqueta: string; cantidad: number };

export type SerieTemporal = {
  granularidad: Granularidad;
  puntos: PuntoSerieTemporal[];
};

function parseFechaISO(fecha: string): Date {
  return new Date(`${fecha}T00:00:00Z`);
}

function formatearFechaISO(fecha: Date): string {
  return fecha.toISOString().slice(0, 10);
}

export function calcularTotalesVolumen(licencias: Licencia[]): TotalesVolumen {
  const totalLicencias = licencias.length;
  const totalDiasReposo = licencias.reduce((suma, licencia) => suma + licencia.cantidadDias, 0);
  const promedioDias = totalLicencias === 0 ? 0 : totalDiasReposo / totalLicencias;
  return { totalLicencias, totalDiasReposo, promedioDias };
}

function calcularRanking(licencias: Licencia[], obtenerEtiqueta: (licencia: Licencia) => string): ItemRanking[] {
  const conteos = new Map<string, number>();
  for (const licencia of licencias) {
    const etiqueta = obtenerEtiqueta(licencia);
    conteos.set(etiqueta, (conteos.get(etiqueta) ?? 0) + 1);
  }
  return [...conteos.entries()]
    .map(([etiqueta, cantidad]) => ({ etiqueta, cantidad }))
    .sort((a, b) => b.cantidad - a.cantidad);
}

export function calcularRankingPorTipo(licencias: Licencia[]): ItemRanking[] {
  return calcularRanking(licencias, (licencia) => licencia.descripcionTipoLicencia ?? "Sin tipo");
}

export function calcularRankingPorUnidad(licencias: Licencia[]): ItemRanking[] {
  return calcularRanking(licencias, (licencia) => licencia.unidadDescripcion ?? "Sin unidad");
}

/** Recorta un ranking a los `max` primeros y agrupa el resto en un ítem "Otras", para que un gráfico no intente mostrar más categorías de las que se pueden distinguir. */
export function limitarRankingConOtras(ranking: ItemRanking[], max: number): ItemRanking[] {
  if (ranking.length <= max) return ranking;
  const top = ranking.slice(0, max - 1);
  const restoCantidad = ranking.slice(max - 1).reduce((suma, item) => suma + item.cantidad, 0);
  return [...top, { etiqueta: "Otras", cantidad: restoCantidad }];
}

export function calcularProporcionIngreso(licencias: Licencia[]): ProporcionIngreso {
  let manual = 0;
  let automatico = 0;
  for (const licencia of licencias) {
    if (licencia.esIngresoManual) manual += 1;
    else automatico += 1;
  }
  return { manual, automatico };
}

/** Umbrales de la spec dashboard-licencias: <=60 días día, <=365 días semana, >365 días mes. */
export function elegirGranularidad(fechaDesde: string, fechaHasta: string): Granularidad {
  const dias = Math.round((parseFechaISO(fechaHasta).getTime() - parseFechaISO(fechaDesde).getTime()) / 86400000) + 1;
  if (dias <= 60) return "dia";
  if (dias <= 365) return "semana";
  return "mes";
}

function fechaRelevante(licencia: Licencia, modo: ModoFechaInforme): string {
  return modo === "termino" ? licencia.fechaTerminoReposo : licencia.fechaInicioReposo;
}

function inicioDeSemana(fecha: Date): Date {
  const resultado = new Date(fecha);
  const diaSemana = resultado.getUTCDay();
  const offsetHastaLunes = diaSemana === 0 ? 6 : diaSemana - 1;
  resultado.setUTCDate(resultado.getUTCDate() - offsetHastaLunes);
  return resultado;
}

function inicioDeMes(fecha: Date): Date {
  return new Date(Date.UTC(fecha.getUTCFullYear(), fecha.getUTCMonth(), 1));
}

function claveBucket(fecha: Date, granularidad: Granularidad): string {
  if (granularidad === "mes") return fecha.toISOString().slice(0, 7);
  if (granularidad === "semana") return formatearFechaISO(inicioDeSemana(fecha));
  return formatearFechaISO(fecha);
}

function siguienteBucket(fecha: Date, granularidad: Granularidad): Date {
  const resultado = new Date(fecha);
  if (granularidad === "mes") resultado.setUTCMonth(resultado.getUTCMonth() + 1);
  else if (granularidad === "semana") resultado.setUTCDate(resultado.getUTCDate() + 7);
  else resultado.setUTCDate(resultado.getUTCDate() + 1);
  return resultado;
}

/**
 * Agrupa las licencias del rango en buckets de tiempo (día/semana/mes según el largo del
 * rango) y rellena con cero los buckets sin licencias, para que el gráfico de tendencia
 * quede continuo en vez de mostrar solo los puntos con datos.
 */
export function calcularSerieTemporal(
  licencias: Licencia[],
  fechaDesde: string,
  fechaHasta: string,
  modo: ModoFechaInforme,
): SerieTemporal {
  const granularidad = elegirGranularidad(fechaDesde, fechaHasta);

  const conteos = new Map<string, number>();
  for (const licencia of licencias) {
    const clave = claveBucket(parseFechaISO(fechaRelevante(licencia, modo)), granularidad);
    conteos.set(clave, (conteos.get(clave) ?? 0) + 1);
  }

  let cursor: Date;
  if (granularidad === "semana") cursor = inicioDeSemana(parseFechaISO(fechaDesde));
  else if (granularidad === "mes") cursor = inicioDeMes(parseFechaISO(fechaDesde));
  else cursor = parseFechaISO(fechaDesde);

  const claveLimite = claveBucket(parseFechaISO(fechaHasta), granularidad);

  const puntos: PuntoSerieTemporal[] = [];
  let clave = claveBucket(cursor, granularidad);
  let iteraciones = 0;
  // Salvaguarda ante un rango con fechaDesde posterior a fechaHasta: evita un loop infinito.
  while (iteraciones < 10000) {
    puntos.push({ etiqueta: clave, cantidad: conteos.get(clave) ?? 0 });
    if (clave === claveLimite) break;
    cursor = siguienteBucket(cursor, granularidad);
    clave = claveBucket(cursor, granularidad);
    iteraciones += 1;
  }

  return { granularidad, puntos };
}

const formatoDiaMes = new Intl.DateTimeFormat("es-CL", { day: "numeric", month: "short", timeZone: "UTC" });
const formatoMesAnio = new Intl.DateTimeFormat("es-CL", { month: "short", year: "numeric", timeZone: "UTC" });

/** Convierte la etiqueta interna de un punto de la serie (fecha ISO o "yyyy-MM") en un texto legible para el eje del gráfico. */
export function formatearEtiquetaSerie(etiqueta: string, granularidad: Granularidad): string {
  if (granularidad === "mes") return formatoMesAnio.format(new Date(`${etiqueta}-01T00:00:00Z`));
  if (granularidad === "semana") return `Sem. ${formatoDiaMes.format(parseFechaISO(etiqueta))}`;
  return formatoDiaMes.format(parseFechaISO(etiqueta));
}
