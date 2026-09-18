## Why

En el formulario de "Ingresar licencia manual", `calcularFechaTermino` (en `fechas-reposo.ts`) no valida el resultado antes de llamar `Date.toISOString()`. Si la persona usuaria escribe una "Cantidad de días" fuera de rango (por ejemplo un typo con un dígito de más) antes de completar "Fecha inicio reposo", el cálculo automático produce un `Date` inválido y lanza un `RangeError: Invalid time value` sin capturar, dentro del `setState` de React. Como no existe ningún `ErrorBoundary` en el árbol del frontend, esto no queda contenido en el modal: tumba toda la SPA (pantalla en blanco), obligando a recargar la aplicación y perder los datos ya tipeados del formulario.

## What Changes

- `calcularFechaTermino`/`calcularDiasReposo` (`fechas-reposo.ts`) dejan de poder lanzar un error sin capturar ante cualquier entrada: se vuelven defensivas frente a fechas o cantidades de días que produzcan un `Date` fuera de rango.
- Se agrega una regla de negocio: el reposo de una licencia (diferencia entre fecha de inicio y fecha de término, o la cantidad de días) no puede superar **365 días**. Si se supera, el sistema rechaza el guardado con un mensaje, igual que las demás validaciones existentes de `handleGuardar` (folio, RUT, etc.).
- El cálculo automático de fecha/días en el formulario deja de calcular (sin lanzar error) cuando el resultado excedería ese rango, dejando el campo dependiente sin actualizar en vez de romper el formulario.

## Capabilities

### New Capabilities

(ninguna)

### Modified Capabilities

- `ingresar-licencia-manual`: el requirement "Cálculo automático de fechas y días de reposo" gana scenarios para entradas inválidas o fuera del rango máximo de 365 días, y se agrega un nuevo requirement de validación del rango máximo de reposo.

## Impact

- `src/LicenciasMedicas.Web.Client/src/lib/fechas-reposo.ts`: lógica de cálculo, se vuelve defensiva y expone el límite de 365 días.
- `src/LicenciasMedicas.Web.Client/src/components/ingreso-manual-modal.tsx`: manejo de los handlers `actualizarFechaInicio`/`actualizarFechaTermino`/`actualizarCantidadDias` y la validación en `handleGuardar`.
- No afecta backend ni la base de datos: es una validación puramente de UI/cliente sobre datos que de todos modos ya se validan como obligatorios antes de grabar.
