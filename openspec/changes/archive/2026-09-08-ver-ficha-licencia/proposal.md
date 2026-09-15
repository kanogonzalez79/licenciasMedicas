## Why

En la pantalla de búsqueda de licencias, la única forma de revisar el contenido de una licencia es abrir el PDF archivado en una pestaña nueva. Eso no permite verificar rápidamente qué quedó realmente cargado en la base de datos (por ejemplo, para auditar que los datos coincidan con el PDF original). Los datos ya están disponibles en el resultado de la búsqueda, pero no hay ninguna forma de visualizarlos completos desde la interfaz.

## What Changes

- Agregar un botón "Ver ficha" en cada fila de resultados de `BuscarPage`, junto al botón "Ver PDF" existente.
- Al hacer clic, se abre un modal (Dialog centrado) con los datos completos de la licencia, agrupados en secciones: Identificación, Paciente, Licencia médica, Profesional, Unidad.
- El modal usa los datos que la fila ya tiene en memoria (del resultado de `licenciasApi.buscar`); no se agrega ninguna llamada nueva al backend.
- Se excluyen del modal los campos técnicos de archivo (ruta del PDF archivado, nombre de archivo original, hash SHA-256).
- El modal es una acción independiente de "Ver PDF": no incluye acceso directo al PDF ni depende de tenerlo abierto.
- Se crea un componente `Dialog` reutilizable en `components/ui/`, envolviendo `@base-ui/react/dialog`, siguiendo el mismo patrón que el componente `Sheet` existente (no hay ningún `Dialog` centrado en el proyecto hoy).

## Capabilities

### New Capabilities
- `detalle-licencia`: ver el detalle completo de una licencia (datos almacenados en la base de datos) desde la pantalla de búsqueda, sin depender del PDF archivado.

### Modified Capabilities
(ninguna — no cambia el comportamiento de búsqueda ni de visualización de PDF existentes)

## Impact

- **Frontend únicamente**, dentro de `src/LicenciasMedicas.Web.Client`:
  - `src/pages/BuscarPage.tsx`: nuevo botón y estado de licencia seleccionada para el modal.
  - `src/components/ui/`: nuevo componente `Dialog` (no existe hoy; se crea siguiendo el patrón de `sheet.tsx`).
- Sin cambios de backend, de API ni de esquema de base de datos: el endpoint `GET /api/licencias` ya devuelve todos los campos de `Licencia` en cada resultado de búsqueda.
