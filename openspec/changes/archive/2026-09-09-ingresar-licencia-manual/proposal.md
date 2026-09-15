## Why

Hoy la única forma de ingresar una licencia médica es dejar el PDF en `incoming/` para que el parser automático (Tipo1/Tipo2) lo lea. Cuando el documento no puede ser leído automáticamente (formato distinto, mala calidad de escaneo, foto de un papel) no existe forma de dejar esa licencia registrada en el sistema: la persona usuaria queda sin poder ingresarla salvo que edite el PDF o insista con el parser. Se necesita una vía alternativa para ingresar los datos de la licencia a mano, siempre acompañada de su documento de respaldo (PDF o imagen).

## What Changes

- Nuevo botón "Ingresar manual" en la pantalla de Ingresar licencias, que abre un formulario con los datos de la licencia (folio, RUT y datos del paciente, tipo de licencia, fechas/días de reposo, datos del profesional, unidad, observaciones) más el adjunto (PDF o imagen) del documento de respaldo.
- El adjunto es obligatorio: no se puede ingresar una licencia manual sin su documento de respaldo.
- Cálculo automático y bidireccional de fecha término / cantidad de días: si se ingresan las dos fechas se calcula la cantidad de días (`(término - inicio) + 1`, conteo inclusivo); si se ingresan fecha inicio y cantidad de días se calcula la fecha término.
- El guardado ocurre en un solo paso (sin pasar por la grilla de revisión de "Procesar"/"Grabar"): valida los datos, valida que el folio y el hash del adjunto no estén ya usados (tanto en licencias grabadas como en el staging de licencias pendientes de revisión), archiva el documento y graba la licencia.
- Las licencias ingresadas de esta forma quedan marcadas internamente con un nuevo origen "Manual" (junto a los orígenes existentes Tipo1/Tipo2 del parser automático).
- El origen de la licencia (Manual o Automático) se muestra en la tabla de resultados de búsqueda y en la ficha de detalle de la licencia.

## Capabilities

### New Capabilities
- `ingresar-licencia-manual`: formulario y flujo de ingreso manual de una licencia con su documento de respaldo, en un solo paso, con validaciones de duplicado y marca de origen manual.

### Modified Capabilities
- `detalle-licencia`: la ficha de detalle debe mostrar el origen de la licencia (Manual o Automático).

## Impact

- **Backend**: nuevo endpoint de creación manual (recibe datos + archivo adjunto vía multipart), nuevo servicio que reutiliza `LicenciasRepository.Insertar` y las validaciones de duplicado de folio/hash contra `Licencias` y `LicenciaRevision`. Nuevo valor en el enum `TipoFormulario` (`Manual`). Sin cambios de esquema de base de datos.
- **Frontend**: nueva UI de formulario (Sheet/Dialog) en `IngresarPage`, con carga de archivo, validación de RUT y cálculo bidireccional de fechas/días. Cambios en `BuscarPage` (columna/indicador de origen) y en el modal de ficha de detalle.
- **Sin impacto** en el pipeline automático existente (`ProcesamientoService`, `LicenciaRevision`) ni en el envío de correos.
