## Context

Ver proposal.md - Why. `fechas-reposo.ts` (`calcularDiasReposo`/`calcularFechaTermino`) es usado en vivo por `ingreso-manual-modal.tsx` en cada `onChange` de los tres campos de reposo (fecha inicio, fecha término, cantidad de días), no solo al grabar. No existe `ErrorBoundary` en el frontend, así que cualquier excepción sin capturar dentro de un `setState` desmonta toda la SPA.

## Goals / Non-Goals

**Goals:**
- Ningún valor ingresado en los campos de reposo del formulario manual puede producir un error no controlado.
- Un período de reposo mayor a 365 días queda bloqueado al grabar, con el mismo patrón de validación (`toast.error`) que ya usa `handleGuardar`.

**Non-Goals:**
- No se agrega un `ErrorBoundary` global en esta iteración; es una mejora de resiliencia más amplia, fuera del alcance de este bug puntual (se puede proponer como cambio aparte si se justifica).
- No se valida el rango de 365 días contra el catálogo de tipos de licencia (p. ej. límites distintos por tipo); es una sola cota global, aplicada igual a los 7 tipos.

## Decisions

- **Tope de 365 días, aplicado como validación de negocio en `handleGuardar`** (rechazo duro, igual patrón que folio/RUT/etc.), no solo como cota técnica silenciosa. Alternativa descartada: validar solo de forma defensiva sin bloquear el guardado — se descartó porque no daría ninguna señal al usuario ante un typo grande, dejando pasar datos claramente erróneos a la base.
- **`calcularFechaTermino`/`calcularDiasReposo` se vuelven defensivas de forma independiente del tope de 365**: antes de operar con `Date`, verifican que `cantidadDias` sea un entero finito positivo y que el resultado sea una fecha válida (`!isNaN(fecha.getTime())`); si no, devuelven `undefined` en lugar de lanzar. Esto cubre además otros casos degenerados (NaN, Infinity, strings no numéricos) más allá del límite de negocio de 365, que solo se aplica en el submit.
- **Los handlers `actualizarFechaInicio`/`actualizarFechaTermino`/`actualizarCantidadDias` en el modal tratan un resultado `undefined` del cálculo como "no calcular"**: guardan el valor tipeado tal cual en su propio campo, sin tocar el campo dependiente. No se muestra ningún error mientras se tipea — el único punto de rechazo visible es `handleGuardar`, igual que hoy con los demás campos obligatorios.
- **El límite de 365 se valida en `handleGuardar` comparando `fechaInicioReposo`/`fechaTerminoReposo` con `calcularDiasReposo`, y también sobre `cantidadDias` cuando las fechas no están ambas presentes** (mismo criterio que "Datos obligatorios del formulario": basta con fecha inicio + cantidad de días).

## Risks / Trade-offs

- [Un usuario tipea un rango >365 días pero nunca llega a completar `handleGuardar` (por ejemplo, cierra el modal)] → sin impacto: no se persiste nada, es solo estado de UI efímero.
- [El límite de 365 es una cota de negocio fija; algún tipo de licencia futuro podría necesitar un rango distinto] → si surge ese caso, se ajusta el valor en un cambio posterior; no se sobre-diseña una configuración por tipo hoy sin un caso real que lo pida.
