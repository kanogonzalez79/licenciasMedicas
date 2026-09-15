## Why

Hoy, una vez que una licencia médica se graba en la base de datos, no existe forma de eliminarla desde la aplicación. Si se grabó por error (unidad equivocada, datos mal leídos por el parser) no hay manera de deshacerlo ni de liberar el folio para reingresar la licencia corregida.

## What Changes

- Se agrega un control "Eliminar" en cada fila de los resultados de la pantalla de búsqueda de licencias (`BuscarPage`), junto a "Ver PDF" y "Ver ficha".
- Al hacer clic, se muestra un diálogo de confirmación (estilo shadcn `AlertDialog`) antes de ejecutar el borrado.
- Al confirmar, se elimina físicamente el registro de la licencia en la base de datos y su PDF archivado en disco.
- Se agrega el endpoint `DELETE /api/licencias/{id}`, orquestado directamente en `LicenciasEndpoints` (sin una capa de servicio nueva), que reutiliza `LicenciasRepository.ObtenerPorId` para ubicar el registro y el archivo, borra el archivo si existe, y borra la fila mediante un nuevo método `LicenciasRepository.Eliminar`.
- Tras eliminar exitosamente, la pantalla de búsqueda refresca sus resultados.
- Es un borrado físico e individual: sin soft-delete, sin registro de auditoría (la app no tiene sistema de usuarios) y sin selección múltiple/eliminación en lote.
- El folio de la licencia eliminada queda libre y puede volver a grabarse si se reprocesa el mismo PDF.

## Capabilities

### New Capabilities
- `eliminar-licencia`: permite eliminar, desde los resultados de la búsqueda de licencias, una licencia ya grabada junto con su PDF archivado, previa confirmación.

### Modified Capabilities
(ninguna)

## Impact

- **Frontend** (`LicenciasMedicas.Web.Client`): `pages/BuscarPage.tsx` (nuevo control y manejo del diálogo de confirmación y de la mutación), `lib/api.ts` (nuevo método `licenciasApi.eliminar`), posible nuevo componente `components/ui/alert-dialog.tsx` (shadcn) si no existe.
- **Backend** (`LicenciasMedicas.Web`): `Endpoints/LicenciasEndpoints.cs` (nuevo `MapDelete`).
- **Core** (`LicenciasMedicas.Core`): `Licencias/LicenciasRepository.cs` (nuevo método `Eliminar`), uso de `Paths/AppPaths.ArchivoDir` para borrar el PDF.
- **Base de datos**: sin cambios de esquema; el borrado libera el valor único de `Folio`.
