## 1. Cálculo defensivo en fechas-reposo.ts

- [x] 1.1 En `calcularFechaTermino`, validar que `cantidadDias` sea un entero finito positivo y que la fecha resultante sea válida (`!isNaN(fecha.getTime())`) antes de llamar `toISOString()`; devolver `undefined` en caso contrario, en vez de lanzar. Verificar con un test unitario que pasa un `cantidadDias` extremo (p. ej. `999999999999`) sin lanzar excepción.
- [x] 1.2 Exportar la constante del máximo de días de reposo (365) desde `fechas-reposo.ts` para que la use tanto el cálculo defensivo como la validación de negocio del modal.
- [x] 1.3 Agregar tests unitarios para `calcularDiasReposo`/`calcularFechaTermino` cubriendo: cálculo normal, `cantidadDias` fuera de rango representable, y valores no numéricos/NaN.

## 2. Handlers del formulario de ingreso manual

- [x] 2.1 En `ingreso-manual-modal.tsx`, actualizar `actualizarFechaInicio`, `actualizarFechaTermino` y `actualizarCantidadDias` para tratar un resultado `undefined` de `calcularFechaTermino`/`calcularDiasReposo` como "no calcular": el campo tipeado se guarda igual, pero el campo dependiente no se toca. Verificar manualmente que escribir una cantidad de días absurda antes de completar la fecha de inicio ya no rompe el formulario ni la SPA.

## 3. Validación de rango máximo al grabar

- [x] 3.1 En `handleGuardar`, agregar una validación que rechace (con `toast.error`) el guardado cuando el período de reposo (diferencia entre fecha inicio y fecha término, o la cantidad de días si solo esa está disponible) supere el máximo de 365 días, siguiendo el mismo patrón que las validaciones existentes de folio/RUT/etc.
- [x] 3.2 Verificar manualmente: completar el formulario con un rango de más de 365 días y confirmar que "Grabar" lo rechaza con un mensaje claro, y que un rango de 365 días o menos permite continuar.

## 4. Spec

- [x] 4.1 Ejecutar `openspec validate validar-rango-dias-reposo-manual --strict` y confirmar que no hay errores antes de archivar el cambio.
