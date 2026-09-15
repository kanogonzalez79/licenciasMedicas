## Context

Ver proposal.md - Why. Hoy `LicenciasEndpoints` compone `LicenciasRepository` y `AppPaths` directamente en el propio endpoint (así arma la ruta del PDF en `GET /api/licencias/{id}/pdf`), sin una capa de servicio intermedia; esa separación (repo = solo DB, endpoint = orquestación con el filesystem) es la que sigue este diseño. El frontend no tiene hoy ningún diálogo de confirmación (`AlertDialog`); solo existe `Dialog` (para mostrar contenido, no para confirmar acciones) y toasts de `sonner`.

## Goals / Non-Goals

**Goals:**
- Definir el orden de borrado (registro vs. archivo) para que un fallo parcial deje el sistema en un estado seguro.
- Definir dónde vive cada responsabilidad (repo vs. endpoint vs. componente UI) siguiendo los patrones ya existentes en el código.

**Non-Goals:**
- Soft-delete, papelera de reciclaje o registro de auditoría (decisión explícita: borrado físico, sin sistema de usuarios que permita atribuir el borrado a alguien).
- Eliminación múltiple/en lote.
- Restringir quién puede eliminar (no hay autenticación en la app hoy).

## Decisions

### Orden de borrado: primero la fila en la base de datos, luego el archivo
Se elimina primero el registro en `Licencias` y, solo si eso tiene éxito, se intenta borrar el PDF en disco.

- **Por qué**: la base de datos es la fuente de verdad para lo que aparece en la búsqueda. Si el archivo se borrara primero y luego fallara el borrado de la fila, quedaría un registro "vivo" cuyo PDF ya no existe (rompe "Ver PDF" de forma silenciosa). Borrando la fila primero, en el peor caso queda un archivo huérfano en disco sin ninguna fila que lo referencie - un problema de limpieza de disco, no de integridad de datos visible para la persona usuaria.
- **Alternativa descartada**: borrar el archivo primero y luego la fila (arriesga registros con PDF faltante si el segundo paso falla).
- El borrado del archivo es best-effort: si el archivo no existe (`File.Exists` es false) o falla al borrarlo, no se aborta la operación ni se devuelve error a la persona usuaria (ver spec `eliminar-licencia` - Manejo de errores).

### Orquestación en el endpoint, sin servicio nuevo
`LicenciasEndpoints` gana un `MapDelete("/api/licencias/{id:int}", ...)` que usa `repo.ObtenerPorId` (404 si no existe), borra el archivo vía `AppPaths.ArchivoDir` + `RutaPdfArchivado`, y llama a un nuevo `LicenciasRepository.Eliminar(connection, licenciaId)` que solo borra la fila.

- **Por qué**: sigue el patrón ya usado por el endpoint de PDF en el mismo archivo; no hay hoy una clase `LicenciasService`, y crear una solo para este método sería una capa nueva sin más beneficio que el que ya da el endpoint. `ProcesamientoService` sí es un servicio dedicado, pero maneja lógica de negocio bastante más compleja (staging, lotes, transacciones); no es el precedente a seguir aquí.
- **Alternativa descartada**: mover toda la orquestación (DB + archivo) a `LicenciasRepository.Eliminar`. Se descarta porque el repositorio no toca el filesystem en ningún otro método hoy (esa responsabilidad vive en `ProcesamientoService`/`AppPaths`), y mezclarla rompería esa separación.

### Confirmación con `AlertDialog` de shadcn
Se añade `components/ui/alert-dialog.tsx` (primitivo de shadcn, mismo estilo que `dialog.tsx` ya presente) para el diálogo de confirmación, en vez de `window.confirm()` nativo.

- **Por qué**: consistencia visual con el resto de la UI (que ya usa componentes shadcn para todo lo demás) y con la elección explícita de mantener el look & feel del proyecto.

### Refresco de resultados vía invalidación de query
Tras un borrado exitoso, se invalida el query `["licencias", "buscar"]` (prefijo, sin el filtro exacto) con `queryClient.invalidateQueries`, igual que hace `UnidadesPage` tras crear/editar una unidad.

## Risks / Trade-offs

- **[Sin auditoría]** Cualquier persona con acceso a la app puede eliminar una licencia sin dejar rastro de quién lo hizo → Mitigación: es una decisión explícita del alcance (ver proposal.md); el único resguardo es el diálogo de confirmación.
- **[Archivo huérfano]** Si el borrado del archivo falla tras borrar la fila (permisos, archivo bloqueado), el PDF queda en disco sin referencia → Mitigación: no bloquea ni falla la eliminación del registro; es un problema de limpieza de disco menor, no de datos, y puede resolverse manualmente si llegara a ocurrir.
- **[Folio reutilizable]** Al liberar el folio, una nueva carga con el mismo folio ya no se detecta como duplicado de la licencia eliminada → Mitigación: es el comportamiento buscado (permitir corregir y reingresar), no un defecto.
