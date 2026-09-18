import { describe, expect, it } from "vitest";
import { calcularDiasReposo, calcularFechaTermino } from "./fechas-reposo";

describe("calcularDiasReposo", () => {
  it("cuenta los días de forma inclusiva", () => {
    expect(calcularDiasReposo("2026-01-01", "2026-01-05")).toBe(5);
  });

  it("cuenta un solo día cuando inicio y término coinciden", () => {
    expect(calcularDiasReposo("2026-01-01", "2026-01-01")).toBe(1);
  });
});

describe("calcularFechaTermino", () => {
  it("calcula la fecha de término para un cálculo normal", () => {
    expect(calcularFechaTermino("2026-01-01", 5)).toBe("2026-01-05");
  });

  it("calcula un solo día de reposo", () => {
    expect(calcularFechaTermino("2026-01-01", 1)).toBe("2026-01-01");
  });

  it("no lanza y devuelve undefined con una cantidad de días fuera del rango representable", () => {
    expect(() => calcularFechaTermino("2026-01-01", 999999999999)).not.toThrow();
    expect(calcularFechaTermino("2026-01-01", 999999999999)).toBeUndefined();
  });

  it("devuelve undefined con cantidadDias no numérica (NaN)", () => {
    expect(calcularFechaTermino("2026-01-01", Number("abc"))).toBeUndefined();
  });

  it("devuelve undefined con cantidadDias cero o negativa", () => {
    expect(calcularFechaTermino("2026-01-01", 0)).toBeUndefined();
    expect(calcularFechaTermino("2026-01-01", -5)).toBeUndefined();
  });

  it("devuelve undefined con cantidadDias no entera", () => {
    expect(calcularFechaTermino("2026-01-01", 2.5)).toBeUndefined();
  });
});
