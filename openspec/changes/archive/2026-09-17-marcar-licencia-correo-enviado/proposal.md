## Why

Hoy no existe forma de ingresar licencias médicas atrasadas (papel que ya fue avisado por correo fuera del sistema, antes de existir este registro) sin que el sistema las trate como pendientes de aviso: como el agrupador de "Redactar/Enviar Correo" es la fecha real en que la licencia se graba en la base de datos, una licencia atrasada ingresada hoy cae en el mismo lote de "hoy" que las licencias realmente nuevas de esa misma unidad, mezclándose con ellas.

## What Changes

- Nueva columna `Licencias.CorreoEnviado` (booleano, por defecto pendiente) que indica si el correo de aviso de esa licencia ya fue enviado (por el sistema o por fuera de él, antes de ingresarla).
- El formulario de ingreso manual permite marcar una licencia como "correo ya enviado" al grabarla.
- La grilla de revisión del ingreso automático (Procesar → Grabar) permite marcar cada fila individualmente como "correo ya enviado", además de una acción para marcar todo el lote pendiente de una sola vez.
- La ficha de detalle de una licencia ya grabada permite cambiar este estado en cualquier momento, sin depender de cómo se ingresó originalmente.
- Redactar/Enviar Correo excluye del conteo por unidad y del texto de aviso generado a toda licencia marcada como "correo ya enviado", sin importar su fecha de ingreso al sistema.
- La búsqueda de licencias muestra un indicador visual cuando una licencia está marcada como "correo ya enviado".
- No se modifica el registro existente de envíos reales por SMTP (tabla `CorreosEnviados`, a nivel de unidad y fecha) ni su lógica de advertencia de reenvío.

## Capabilities

### New Capabilities
- `licencia-correo-enviado`: define el estado "correo enviado" de una licencia individual (independiente del registro de envíos SMTP por unidad+fecha), su valor por defecto, cómo se marca durante el ingreso automático (fila individual y lote completo), y que puede cambiarse en cualquier momento después de grabada la licencia.

### Modified Capabilities
- `ingresar-licencia-manual`: el formulario de ingreso manual agrega la opción de marcar la licencia como "correo ya enviado" al grabarla.
- `redactar-correo`: el texto de aviso y el conteo de licencias por unidad y fecha excluyen las licencias marcadas como "correo ya enviado".
- `detalle-licencia`: la ficha de detalle permite ver y cambiar el estado de "correo enviado" de la licencia, además de mostrarlo.

## Impact

- Backend: nueva migración SQL (columna `CorreoEnviado` en `Licencias`); `LicenciasRepository` (insert, `UnidadesConLicenciasGrabadasEnFecha`, `LicenciasDeUnidadGrabadasEnFecha`, nuevo método de actualización puntual del flag); `Licencia` (entidad); `IngresoManualService`/`DatosLicenciaManual`; `Procesamiento/ProcesamientoService.Grabar` y `AsignacionUnidad`; nuevo endpoint `PATCH` en `Endpoints` para licencias ya grabadas.
- Frontend: `IngresoManualModal`, `IngresarPage.tsx` (grilla de revisión + acción de lote), `licencia-detalle-modal.tsx`, `BuscarPage.tsx` (badge), `lib/api.ts` (tipos y llamada al nuevo endpoint).
- No afecta `CorreoEnvioService`, `ConfiguracionSmtpService` ni la tabla `CorreosEnviados`.
