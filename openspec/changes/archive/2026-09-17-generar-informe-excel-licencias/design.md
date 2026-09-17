## Context

Ver `proposal.md` - Why. Hoy `LicenciasRepository.Buscar` (`src/LicenciasMedicas.Core/Licencias/LicenciasRepository.cs:75`) solo filtra el rango de fechas contra `FechaInicioReposo`, pagina resultados y agrega filtros de rut/nombre/unidad que este informe no necesita. No existe en el proyecto ninguna dependencia para generar archivos Excel. El frontend ya tiene un patrón de descarga de archivo vía GET simple: `licenciasApi.pdfUrl(id)` (`lib/api.ts`) construye una URL y `BuscarPage.tsx` hace `window.open(url, "_blank")`.

## Goals / Non-Goals

**Goals:**
- Generar el `.xlsx` en un solo request GET, sin paginar, reutilizando el patrón de descarga por URL ya usado para el PDF archivado.
- Que el modo "intersección" implemente correctamente la prueba de solapamiento de intervalos (ver spec, incluye el caso de una licencia que cubre todo el rango consultado).

**Non-Goals:**
- No se agregan filtros de rut, nombre o unidad al informe (decisión ya tomada: solo rango de fechas + modo).
- No se pagina ni se resume el resultado: para el volumen de datos de esta app (uso local, un solo usuario, SQLite) un `SELECT` sin límite y su construcción en memoria con ClosedXML es más que suficiente.
- No se reutiliza `BusquedaLicenciasFiltro`/`Buscar` para este informe: sus semántica de paginación y de fecha fija a `FechaInicioReposo` no calzan; se agrega un método de repositorio dedicado.

## Decisions

### Dependencia para generar el Excel: ClosedXML
Se agrega `ClosedXML` (MIT) al `.csproj` de `LicenciasMedicas.Core`. Alternativas consideradas:
- **EPPlus**: licencia Polyform Noncommercial desde v5 - no calza con claridad para una herramienta que igual podría usarse en contexto laboral.
- **NPOI**: API más verbosa (basada 1:1 en el formato binario/XML de Office), sin ventaja concreta aquí.
- **Interop de Excel**: requiere Excel instalado en la máquina - inaceptable para un `.exe` self-contained de escritorio.

ClosedXML no requiere Excel instalado, genera `.xlsx` en memoria (`XLWorkbook` -> `MemoryStream`) y es la opción más simple de las evaluadas.

### Nuevo método de repositorio en vez de reusar `Buscar`
`LicenciasRepository` gana un método `ObtenerParaInforme(connection, fechaDesde, fechaHasta, modo)` que:
- No pagina (trae todas las filas que califican).
- Arma la condición `WHERE` según el modo:
  - `FechaInicio`: `l.FechaInicioReposo BETWEEN @fechaDesde AND @fechaHasta`
  - `FechaTermino`: `l.FechaTerminoReposo BETWEEN @fechaDesde AND @fechaHasta`
  - `Interseccion`: `l.FechaInicioReposo <= @fechaHasta AND l.FechaTerminoReposo >= @fechaDesde` (prueba estándar de solapamiento de intervalos cerrados `[a,b]` y `[c,d]`: se solapan sii `a <= d AND c <= b`)
- Ordena por `l.FechaInicioReposo, l.NombreCompletoPaciente COLLATE NOCASE` para que el Excel salga en un orden estable y legible.

El modo se representa como un enum `ModoFechaInforme { FechaInicio, FechaTermino, Interseccion }` en el módulo `Licencias`.

### Generación del archivo: servicio dedicado
Nuevo `InformeLicenciasExcelService` (módulo nuevo `Core/Informes/`, análogo a como `Correos` aloja `CorreoRedaccionService`) con un método `Generar(IReadOnlyList<Licencia> licencias) -> byte[]` que arma el `XLWorkbook`: una hoja, fila de encabezado con los nombres de columna en español, y una fila por licencia con las columnas en el orden de la spec. Las columnas de fecha se escriben como texto (mismo formato `yyyy-MM-dd` que ya usa el dominio) en vez de como fecha nativa de Excel, para no depender de conversión de zona horaria ni de que Excel adivine el formato.

### Endpoint y transporte: GET con querystring, igual que el PDF
`GET /api/licencias/informe?fechaDesde=...&fechaHasta=...&modo=inicio|termino|interseccion` en `LicenciasEndpoints`, agregado junto al resto de endpoints de licencias. Valida que ambas fechas vengan presentes y que `fechaDesde <= fechaHasta` (`Results.BadRequest` con mensaje legible si no). Devuelve el archivo con `Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName: "...")`, mismo patrón que `GET /api/licencias/{id}/pdf`.

Se eligió GET simple (no POST) porque no hay body de archivo que subir, los parámetros caben en querystring, y así el frontend puede reusar el patrón `window.open(url, "_blank")` que ya usa `pdfUrl`, sin manejar blobs a mano.

Nombre de archivo descargado: `Informe_Licencias_{fechaDesde}_a_{fechaHasta}.xlsx` (fechas en `yyyy-MM-dd`).

### Frontend: página nueva "Informe"
`src/pages/InformePage.tsx`, agregada a `routes.tsx` (`path: "informe"`) y a `app-sidebar.tsx` (ícono `FileSpreadsheet` de lucide-react, entre "Redactar correo" y "Respaldo"). Formulario con los mismos inputs `type="date"` que `BuscarPage.tsx` para fecha desde/hasta, un `Select` para el modo (mismo componente shadcn que ya usa `BuscarPage` para Unidad), y un botón "Generar Excel" que valida en el cliente que ambas fechas estén presentes y que desde <= hasta antes de abrir la URL (evita un roundtrip inválido, aunque el backend igual valida). `lib/api.ts` gana `licenciasApi.informeUrl(fechaDesde, fechaHasta, modo)` que arma la URL con `URLSearchParams`, siguiendo el mismo patrón que `pdfUrl`.

## Risks / Trade-offs

- **[Riesgo] Un rango de fechas muy amplio (ej. "todo el historial") podría generar un Excel grande y una consulta sin límite.** → Mitigación: dado el volumen esperado de esta app (un consultorio/unidad, uso diario, SQLite local), no se considera un riesgo real hoy; si en el futuro se vuelve un problema, se puede agregar una cota razonable sin cambiar la spec.
- **[Riesgo] Confundir modo "intersección" con "inicio" u "otro" produce una fila incorrecta en el informe (falso negativo/positivo) sin que se note a simple vista.** → Mitigación: cubierto explícitamente con escenarios de test para los tres modos, incluyendo el caso de la licencia que cubre todo el rango consultado (ver `tasks.md` para el detalle de pruebas del repositorio).
