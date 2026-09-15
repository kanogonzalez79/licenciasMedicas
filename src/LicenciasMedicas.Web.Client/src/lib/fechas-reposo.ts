// Cálculo bidireccional fecha término / cantidad de días para el formulario de ingreso manual.
// Conteo inclusivo: 2026-01-01 a 2026-01-05 son 5 días (igual que en el papel de la licencia).

function parseFechaISO(fecha: string): Date {
  return new Date(`${fecha}T00:00:00Z`);
}

export function calcularDiasReposo(fechaInicio: string, fechaTermino: string): number {
  const diffMs = parseFechaISO(fechaTermino).getTime() - parseFechaISO(fechaInicio).getTime();
  return Math.round(diffMs / 86400000) + 1;
}

export function calcularFechaTermino(fechaInicio: string, cantidadDias: number): string {
  const fecha = parseFechaISO(fechaInicio);
  fecha.setUTCDate(fecha.getUTCDate() + (cantidadDias - 1));
  return fecha.toISOString().slice(0, 10);
}
