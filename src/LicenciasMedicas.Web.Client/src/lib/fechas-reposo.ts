// Cálculo bidireccional fecha término / cantidad de días para el formulario de ingreso manual.
// Conteo inclusivo: 2026-01-01 a 2026-01-05 son 5 días (igual que en el papel de la licencia).

export const MAXIMO_DIAS_REPOSO = 365;

function parseFechaISO(fecha: string): Date {
  return new Date(`${fecha}T00:00:00Z`);
}

export function calcularDiasReposo(fechaInicio: string, fechaTermino: string): number {
  const diffMs = parseFechaISO(fechaTermino).getTime() - parseFechaISO(fechaInicio).getTime();
  return Math.round(diffMs / 86400000) + 1;
}

// Devuelve undefined (en vez de lanzar) cuando cantidadDias no es un entero positivo
// finito o cuando la fecha resultante queda fuera del rango representable por Date:
// un typo en "Cantidad de días" no debe poder tumbar el formulario.
export function calcularFechaTermino(fechaInicio: string, cantidadDias: number): string | undefined {
  if (!Number.isInteger(cantidadDias) || cantidadDias < 1) return undefined;

  const fecha = parseFechaISO(fechaInicio);
  fecha.setUTCDate(fecha.getUTCDate() + (cantidadDias - 1));
  if (Number.isNaN(fecha.getTime())) return undefined;

  return fecha.toISOString().slice(0, 10);
}
