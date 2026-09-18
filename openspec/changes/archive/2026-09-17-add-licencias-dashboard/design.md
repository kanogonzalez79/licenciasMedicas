## Context

El flujo de filtrado por rango de fechas ya existe para el informe Excel: `InformePage.tsx` pide fecha desde, fecha hasta y modo (`ModoFechaInforme` = inicio/termino/interseccion), y `LicenciasRepository.ObtenerParaInforme` arma el `WHERE` SQL correspondiente y devuelve la lista completa de `Licencia` para ese rango. Ver `proposal.md` para la motivación. El frontend no tiene hoy ninguna librería de gráficos.

## Goals / Non-Goals

**Goals:**
- Reutilizar el modelo de filtro de fecha/modo existente en vez de crear uno paralelo.
- Mantener la agregación (totales, rankings, series de tiempo) del lado del cliente, ya que el volumen de datos de una app de escritorio de un solo usuario es chico y esto evita duplicar lógica de agrupación en SQL y en TypeScript.
- Dejar la página en un estado consistente cuando no hay rango aplicado o cuando el rango no tiene licencias.

**Non-Goals:**
- No se agrega el bloque de "correo enviado / pendiente" en esta iteración (quedó fuera de alcance en la propuesta).
- No se persiste el último rango consultado entre sesiones; cada visita a la página parte sin rango aplicado.
- No se pagina ni se virtualiza la lista subyacente: se asume que el rango de un dashboard de uso normal (semanas/meses/un año) trae un volumen de licencias manejable en memoria del navegador.

## Decisions

### Endpoint JSON dedicado, reutilizando la query de informe
Se agrega `GET /api/licencias/dashboard` en `LicenciasEndpoints.cs`, con los mismos parámetros y la misma validación que `GET /api/licencias/informe` (`fechaDesde`, `fechaHasta`, `modo` obligatorios; `fechaDesde <= fechaHasta`). Internamente llama a `LicenciasRepository.ObtenerParaInforme` (sin cambios) y devuelve `Results.Ok(licencias)` en vez de generar un `.xlsx`.

Alternativa considerada: exponer endpoints de agregación en SQL (`GROUP BY` por tipo, por unidad, por período). Se descarta para esta iteración porque implica mantener varias queries nuevas por un beneficio de performance que no aplica a los volúmenes esperados (uso personal, un backend SQLite local); si el volumen de datos creciera mucho se puede revisar.

### Agregación en el cliente
Un módulo nuevo en el frontend (p. ej. `lib/dashboard-agregaciones.ts`) recibe el arreglo de `Licencia` del endpoint y calcula:
- Totales de volumen (cantidad, suma de `CantidadDias`, promedio).
- Rankings por `DescripcionTipoLicencia` y por `UnidadDescripcion` (agrupar y contar, orden descendente).
- Proporción manual (`EsIngresoManual`) vs. parser automático.
- Serie de tiempo: bucketing por día/semana/mes de la fecha relevante según el modo aplicado (fecha de inicio de reposo para modo "inicio", fecha de término para modo "termino"; para "interseccion" se usa la fecha de inicio de reposo de cada licencia como eje, ya que es el criterio más intuitivo para ubicar un evento en el tiempo).
- Elección de granularidad según el largo del rango: ≤ 60 días → día; ≤ 365 días → semana; > 365 días → mes (umbrales definidos en la spec de `dashboard-licencias`).

### Recharts para los gráficos
Se agrega `recharts` como dependencia de producción de `LicenciasMedicas.Web.Client`. Es la opción estándar para gráficos declarativos en React, compatible con React 19, y cubre barras (rankings), líneas/barras de serie de tiempo y el gráfico manual-vs-parser sin código de bajo nivel en SVG/canvas. Los tiles de totales (volumen) no necesitan la librería: son `Card` de shadcn/ui con el número.

### Gate de fecha inline, sin bloquear la página
Igual que `InformePage`, los controles de fecha/modo quedan siempre visibles arriba de la página (no un `Dialog`/modal). El contenido del dashboard (tiles + gráficos) solo se renderiza cuando hay un rango aplicado en el estado local del componente; al reaplicar con un rango distinto, se reemplaza todo el contenido. Esto reutiliza el mismo patrón de validación por `toast.error` que ya usa `InformePage` (fechas requeridas, desde &lt;= hasta).

### Ubicación en el menú
Se agrega "Dashboard" al arreglo `paginas` del grupo `licencias` en `app-sidebar.tsx`, entre "Redactar correo" e "Informe" o después de "Informe" (orden exacto a definir al implementar, sin impacto de diseño). Requiere actualizar el spec `navegacion-menu` (ya cubierto en la delta de specs de este change) y la ruta correspondiente en `routes.tsx`.

## Risks / Trade-offs

- **[Riesgo] Rango muy amplio (varios años) trae muchas filas al cliente** → Mitigación: para el perfil de uso de esta app (una persona, un set de licencias de una organización) el volumen esperado es de cientos a pocos miles de filas por año; si en el futuro se vuelve un problema real, se puede migrar el cálculo de agregados a SQL sin cambiar la spec (las requirements describen el resultado, no cómo se calcula).
- **[Riesgo] Nueva dependencia de frontend (Recharts) aumenta el bundle** → Mitigación: es una librería madura, con tree-shaking razonable, y el bundle final igual se sirve embebido y local (no hay presupuesto de red que cuidar al ser una app de escritorio de un solo usuario).
- **[Trade-off] Elegir el eje de tiempo por fecha de inicio de reposo en modo intersección** → Es una decisión arbitraria pero explícita (documentada en la spec) para no dejar dos fechas candidatas sin criterio; se puede ajustar más adelante si en el uso real resulta confuso.

## Migration Plan

No aplica migración de datos (no hay cambios de esquema). Es una funcionalidad nueva aditiva: no modifica flujos existentes de Procesar/Grabar/archivado, y el único cambio a comportamiento existente es la lista de páginas visibles en el grupo "Licencias" del menú.
