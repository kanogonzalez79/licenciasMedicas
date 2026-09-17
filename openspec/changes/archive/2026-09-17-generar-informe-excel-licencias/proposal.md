## Why

Hoy no existe forma de sacar del sistema un listado tabular de licencias para un rango de fechas (por ejemplo, para reportar a RRHH o mutualidad). La pantalla "Buscar licencia" está pensada para búsqueda puntual y paginada en pantalla, no para generar un informe exportable con todas las licencias de un período.

## What Changes

- Se agrega una página nueva "Informe" en el sidebar, con un formulario que pide fecha desde, fecha hasta, y un selector de modo de fecha (fecha inicio / fecha término / intersección de rangos).
- Se agrega un endpoint que genera un archivo Excel (`.xlsx`) con todas las licencias que califican según el rango y modo elegidos, sin paginar, y lo entrega para descarga directa desde el navegador.
- El Excel contiene las columnas: RUT (sin DV), DV, nombre completo, folio, tipo de licencia, fecha inicio, fecha término, fecha de otorgamiento, número de días.
- El modo "intersección" usa la prueba estándar de solapamiento de intervalos (`FechaInicioReposo <= fechaHasta AND FechaTerminoReposo >= fechaDesde`), de forma que también captura licencias que cubren todo el rango consultado sin que su inicio ni su término caigan dentro de él.
- Se agrega la dependencia ClosedXML (MIT) al backend para generar el archivo `.xlsx`.

## Capabilities

### New Capabilities
- `informe-excel-licencias`: generación y descarga de un informe Excel de licencias para un rango de fechas, con selección de a qué fecha de la licencia aplica el rango (inicio, término o intersección).

### Modified Capabilities
(ninguna — la búsqueda en pantalla actual no está documentada como spec propia y no cambia su comportamiento)

## Impact

- **Backend**: nuevo endpoint en `LicenciasEndpoints` (o un endpoint dedicado de informes), nuevo método de consulta en `LicenciasRepository` (no reutiliza `Buscar`, que pagina y solo filtra por fecha de inicio), nueva dependencia ClosedXML en `LicenciasMedicas.Core.csproj`.
- **Frontend**: nueva página `InformePage.tsx`, nueva entrada en `app-sidebar.tsx`, nueva ruta en el router, nueva función en `lib/api.ts` para descargar el archivo.
- **Datos**: solo lectura de la tabla `Licencias` ya existente; no requiere migración.
