## Context

Ver proposal.md - Why. Contexto técnico relevante:

- `BuscarPage.tsx` obtiene resultados con `licenciasApi.buscar(filtro)`, tipado como `Licencia[]` (`lib/api.ts:42-72`), que ya incluye **todos** los campos de la licencia (el backend hace `SELECT l.*` tanto en `Buscar` como en `ObtenerPorId`, `LicenciasRepository.cs:61` y `:115`). No hace falta ningún fetch adicional para mostrar el detalle.
- El proyecto usa componentes estilo shadcn/ui sobre `@base-ui/react`. Ya existe `components/ui/sheet.tsx`, que envuelve `@base-ui/react/dialog` como panel lateral, pero no hay un `Dialog` centrado reutilizable.
- No hay backend afectado por este cambio.

## Goals / Non-Goals

**Goals:**
- Mostrar el detalle completo (salvo campos técnicos de archivo) de una licencia en un modal, accesible desde la fila de resultados de búsqueda.
- Reutilizar los datos ya cargados en memoria; no agregar llamadas de red.
- Dejar un componente `Dialog` genérico y reutilizable para futuros usos (no acoplado a licencias).

**Non-Goals:**
- No se agrega edición de datos desde la ficha (es de solo lectura).
- No se integra visualización del PDF dentro del mismo modal.
- No se cambia el endpoint de búsqueda, el modelo de datos, ni columnas de la tabla existente.

## Decisions

### Componente Dialog centrado (no Sheet)
Se crea `components/ui/dialog.tsx` envolviendo `@base-ui/react/dialog` (misma dependencia que ya usa `sheet.tsx`), siguiendo la misma estructura de subcomponentes (`Dialog`, `DialogTrigger`, `DialogContent`, `DialogHeader`, `DialogFooter`, `DialogTitle`, `DialogDescription`) pero con overlay centrado en vez de panel lateral.

Alternativa considerada: reutilizar `Sheet` tal cual. Se descarta porque el uso previsto es una ficha de lectura puntual (se abre, se revisa, se cierra), no un panel de navegación persistente, y el usuario prefirió explícitamente un modal centrado.

### Datos en memoria, sin nuevo endpoint
El modal recibe el objeto `Licencia` completo de la fila seleccionada (ya presente en el array devuelto por `licenciasApi.buscar`). No se usa el endpoint `GET /api/licencias/{id}` ni se agrega método nuevo a `licenciasApi`.

Alternativa considerada: exponer `licenciasApi.obtenerPorId` y hacer fetch al abrir el modal. Se descarta por innecesaria: agregaría latencia y complejidad sin beneficio, dado que el dato ya es idéntico al que se muestra en el modal (mismo `SELECT l.*`).

### Agrupación de campos y exclusiones
La ficha agrupa los campos de `Licencia` en 5 secciones (Identificación, Paciente, Licencia médica, Profesional, Unidad), excluyendo explícitamente `rutaPdfArchivado`, `nombreArchivoOriginal` y `hashArchivoSha256` (campos técnicos de integridad de archivo, sin valor para el caso de uso de auditoría de datos clínicos/administrativos definido en proposal.md).

### Estado del modal en BuscarPage
Se agrega un estado local `licenciaSeleccionada: Licencia | null` en `BuscarPage.tsx`; el botón "Ver ficha" de cada fila lo setea, y el `Dialog` se abre controlado por `open={licenciaSeleccionada !== null}`. Cerrar el modal limpia el estado a `null`. No se toca el estado de filtros ni de paginación existente.

## Risks / Trade-offs

- [Un futuro cambio de columnas en `Licencia` requiere actualizar manualmente la agrupación de secciones del modal, ya que no hay generación automática de la ficha a partir del tipo] → Aceptable dado el tamaño del proyecto; se documenta en tasks.md como paso a revisar si se agregan campos a la tabla `Licencias`.
- [Dos componentes similares (`Sheet` y `Dialog`) conviven en `components/ui/`, con posible confusión sobre cuál usar a futuro] → Mitigado documentando en el propio archivo o en el PR que `Dialog` es para contenido modal centrado de una sola pantalla y `Sheet` para paneles laterales/navegación.
