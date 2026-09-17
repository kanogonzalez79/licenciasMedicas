## Context

Ver proposal.md - Why. Puntos técnicos relevantes ya confirmados en la exploración previa (`/opsx:explore`):

- `Licencias.FechaIngresoSistema` es un timestamp de auditoría con default `strftime('now')` en el `INSERT` (`001_init.sql`), nunca fijado por la app. No se toca ni se reutiliza como mecanismo de este cambio.
- `LicenciasRepository.UnidadesConLicenciasGrabadasEnFecha` y `LicenciasDeUnidadGrabadasEnFecha` agrupan por `DATE(FechaIngresoSistema, 'localtime')`; ambas alimentan la capability `redactar-correo` (texto de aviso) y la pantalla que lista unidades con licencias de una fecha.
- El registro de envíos reales por SMTP (`CorreosEnviados`, capability `enviar-correo`) es una tabla aparte, a nivel de `(UnidadId, Fecha)`, y no se modifica.
- La grilla de revisión del ingreso automático (`IngresarPage.tsx`) hoy solo administra `AsignacionUnidad(RevisionId, UnidadId?)` por fila antes de "Grabar"; no tiene un concepto de capability propio en `openspec/specs/` (no hay spec formal para "procesar/grabar" hoy), por eso su comportamiento nuevo se documenta en la capability nueva `licencia-correo-enviado`.

## Goals / Non-Goals

**Goals:**
- Un flag `CorreoEnviado` por licencia, decidido y nombrado igual que el texto de UI (`CorreoEnviado`), aceptando que coexiste con la tabla `CorreosEnviados` sin relacionarse con ella.
- Poder fijar el flag en los dos puntos de ingreso (manual y grilla automática) y cambiarlo después desde la ficha de detalle.
- Que el filtro de exclusión sea un único punto en el backend (las consultas de `LicenciasRepository` usadas por `redactar-correo`), no lógica duplicada en el frontend.

**Non-Goals:**
- No se agrega un historial de cambios del flag (quién y cuándo lo cambió) — solo el estado actual.
- No se toca `CorreoEnvioService`, `ConfiguracionSmtpService` ni la tabla `CorreosEnviados`.
- No se agrega la capability formal para "procesar/grabar" en general; solo se documentan los requisitos nuevos que introduce este cambio en `licencia-correo-enviado`.

## Decisions

### Columna `CorreoEnviado` en `Licencias`, no una tabla aparte
`INTEGER NOT NULL DEFAULT 0` (convención SQLite booleana ya usada en el proyecto, ver `Activo` en `CorreosCopia`). Migración nueva `004_licencia_correo_enviado.sql`. Alternativa descartada: una tabla `LicenciasSinAviso` aparte — innecesaria, es un atributo 1:1 de la licencia, no una relación.

### Filtro centralizado en `LicenciasRepository`
`UnidadesConLicenciasGrabadasEnFecha` y `LicenciasDeUnidadGrabadasEnFecha` agregan `AND l.CorreoEnviado = 0` al `WHERE`. Como `CorreoRedaccionService` (capability `redactar-correo`) consume ambos métodos, el conteo y el texto quedan consistentes por construcción sin duplicar la regla en la capa de servicio.

### Extender `AsignacionUnidad` con el flag, no un endpoint separado
`AsignacionUnidad(RevisionId, UnidadId?, CorreoEnviado)` viaja junto con la asignación de unidad en el mismo `POST` de "Grabar" (`ProcesamientoService.Grabar`), igual que hoy viaja `UnidadId`. Evita una segunda llamada de red por fila y mantiene "Grabar" como una operación atómica por lote.

### Acción de "marcar todo el lote" es solo de UI (frontend)
No requiere endpoint nuevo: el botón "Marcar lote como correo enviado" en `IngresarPage.tsx` simplemente pone `CorreoEnviado = true` en el estado local de todas las filas pendientes antes de armar el payload de `Grabar`; la persona usuaria puede seguir desmarcando filas individuales después. Mismo patrón que ya usa el estado `asignaciones` por fila.

### `IngresoManualModal`/`DatosLicenciaManual`
Se agrega un checkbox "Correo ya enviado" al formulario; `DatosLicenciaManual.CorreoEnviado` (bool, default `false`) viaja hasta `IngresoManualService.Ingresar`, que lo asigna directo al `Licencia.CorreoEnviado` antes de insertar.

### Edición posterior: `PATCH /api/licencias/{id}/correo-enviado`
Sigue el mismo patrón ya existente en `CorreosCopiaEndpoints.MapPatch("/{id:int}", ...)`: un endpoint puntual con body `{ correoEnviado: bool }`, no un `PUT` genérico de toda la licencia (evita reabrir edición de campos que hoy son inmutables tras grabar, como RUT o fechas). El modal de detalle (`licencia-detalle-modal.tsx`) agrega un control (switch/checkbox) que llama a este endpoint y refresca su propio estado en éxito.

### Indicador en Búsqueda
Un badge adicional en `BuscarPage.tsx`, mismo patrón visual que el badge de "Origen" (`esIngresoManual`), mostrando "Correo enviado" solo cuando el flag está en `true` (sin badge para el caso pendiente, para no agregar ruido visual a la mayoría de las filas que siguen el flujo normal).

## Risks / Trade-offs

- **Confusión de nombres** (`Licencias.CorreoEnviado` vs. tabla `CorreosEnviados`) → Mitigado documentando explícitamente la distinción en el comentario XML de la entidad `Licencia` y en el `Purpose` de la capability `licencia-correo-enviado`; decisión consciente del usuario por simpleza.
- **Migración de datos históricos**: las licencias ya grabadas antes de este cambio quedarán con `CorreoEnviado = 0` (pendiente) por el `DEFAULT` de la columna, incluyendo licencias antiguas cuyo aviso ya se envió por el flujo normal actual (no hay forma de inferir eso retroactivamente) → Aceptado: no rompe nada porque esas licencias ya fueron cubiertas por `CorreosEnviados` en su momento; el nuevo filtro solo golpea licencias nuevas que se ingresen desde ahora.
- **Doble lugar donde se puede fijar el flag al ingreso automático** (fila individual y acción de lote) podría dejar estado inconsistente si la persona usuaria alterna entre ambas antes de grabar → Mitigado: la acción de lote solo setea el estado local de todas las filas pendientes en ese momento; una edición individual posterior a esa acción simplemente sobreescribe esa fila, comportamiento explícito y ya cubierto por el escenario "Ajustar filas individuales después de usar la acción de lote" en la spec.

## Migration Plan

1. Agregar `004_licencia_correo_enviado.sql` (`ALTER TABLE Licencias ADD COLUMN CorreoEnviado INTEGER NOT NULL DEFAULT 0`). Se aplica sola al iniciar la app vía `Migrator.Migrar`, sin pasos manuales.
2. No requiere backfill: el default `0` es el comportamiento correcto para todo lo ya grabado (ver Risks).
3. Sin plan de rollback especial más allá de no aplicar la migración — es aditiva y no rompe lecturas existentes de `Licencia`.
