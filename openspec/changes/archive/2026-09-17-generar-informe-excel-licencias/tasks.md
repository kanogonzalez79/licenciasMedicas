## 1. Dependencias y dominio

- [x] 1.1 Agregar el paquete ClosedXML a `src/LicenciasMedicas.Core/LicenciasMedicas.Core.csproj` y verificar que `dotnet restore` resuelve la dependencia sin conflictos.
- [x] 1.2 Agregar el enum `ModoFechaInforme` (`FechaInicio`, `FechaTermino`, `Interseccion`) en el módulo `Licencias` y verificar que compila.

## 2. Repositorio: consulta del informe

- [x] 2.1 Implementar `LicenciasRepository.ObtenerParaInforme(connection, fechaDesde, fechaHasta, modo)` sin paginación, con la condición `WHERE` específica por modo descrita en `design.md`, ordenado por `FechaInicioReposo, NombreCompletoPaciente COLLATE NOCASE`.
- [x] 2.2 Agregar tests en `LicenciasRepositoryTests.cs` para modo "fecha de inicio": licencia con inicio dentro del rango aparece, licencia con inicio fuera del rango (aunque el término esté dentro) no aparece. Verificar con `dotnet test`.
- [x] 2.3 Agregar tests para modo "fecha de término": misma lógica que 2.2 pero sobre `FechaTerminoReposo`. Verificar con `dotnet test`.
- [x] 2.4 Agregar tests para modo "intersección" cubriendo los tres casos de la spec: licencia que cubre todo el rango consultado (sin que inicio ni término caigan dentro), licencia solapada parcialmente al inicio o al final del rango, y licencia sin ningún día en común con el rango. Verificar con `dotnet test`.
- [x] 2.5 Agregar test de rango sin licencias que califiquen: `ObtenerParaInforme` devuelve una lista vacía (no lanza excepción). Verificar con `dotnet test`.

## 3. Generación del Excel

- [x] 3.1 Crear el módulo `Core/Informes` con `InformeLicenciasExcelService.Generar(IReadOnlyList<Licencia>) -> byte[]`, que arma el `XLWorkbook` con la fila de encabezado y las columnas en el orden de la spec (RUT sin DV, DV, nombre completo, folio, tipo de licencia, fecha inicio, fecha término, fecha de otorgamiento, número de días), fechas como texto `yyyy-MM-dd`.
- [x] 3.2 Verificar que una licencia sin `FechaEmisionOtorgamiento` produce una celda vacía en esa columna, sin inventar un valor (test unitario que abre el workbook generado con ClosedXML y lee la celda).
- [x] 3.3 Verificar que una lista vacía de licencias produce un workbook con solo la fila de encabezado (test unitario).

## 4. Endpoint

- [x] 4.1 Agregar `GET /api/licencias/informe` en `LicenciasEndpoints`, con parámetros `fechaDesde`, `fechaHasta`, `modo`; validar que ambas fechas estén presentes y que `fechaDesde <= fechaHasta`, devolviendo `400` con mensaje legible si no.
- [x] 4.2 Conectar el endpoint con `ObtenerParaInforme` + `InformeLicenciasExcelService.Generar` y devolver el archivo con `Results.File(...)`, `Content-Type` `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` y nombre `Informe_Licencias_{fechaDesde}_a_{fechaHasta}.xlsx`.
- [x] 4.3 Probar manualmente con `curl` contra el backend en dev (`GET /api/licencias/informe?fechaDesde=...&fechaHasta=...&modo=inicio`, y los otros dos modos) y confirmar que el archivo descargado abre correctamente en Excel/LibreOffice con las columnas esperadas.

## 5. Frontend: cliente API

- [x] 5.1 Agregar `licenciasApi.informeUrl(fechaDesde, fechaHasta, modo)` en `lib/api.ts`, siguiendo el mismo patrón que `pdfUrl`, y el tipo `ModoFechaInforme` correspondiente al enum del backend.

## 6. Frontend: página "Informe"

- [x] 6.1 Crear `src/pages/InformePage.tsx` con inputs de fecha desde/hasta (`type="date"`, mismo estilo que `BuscarPage.tsx`) y un `Select` para el modo de fecha (fecha de inicio / fecha de término / intersección).
- [x] 6.2 Validar en el cliente, antes de generar, que ambas fechas estén presentes y que desde <= hasta, mostrando el error con `toast` (patrón `sonner` ya usado en el resto de la app) en vez de abrir la URL.
- [x] 6.3 Al hacer clic en "Generar Excel", abrir `licenciasApi.informeUrl(...)` con `window.open(url, "_blank")`, igual que la descarga de PDF en `BuscarPage.tsx`.

## 7. Integración de navegación

- [x] 7.1 Agregar la ruta `informe` -> `InformePage` en `routes.tsx`.
- [x] 7.2 Agregar la entrada "Informe" (ícono `FileSpreadsheet` de `lucide-react`) al arreglo `paginas` de `app-sidebar.tsx`, entre "Redactar correo" y "Respaldo".

## 8. Verificación end-to-end

- [x] 8.1 Con backend (`dotnet run --project src/LicenciasMedicas.Web`) y frontend (`npm run dev`) levantados, generar el informe desde la UI para los tres modos contra datos de prueba y confirmar que el archivo descargado tiene las filas y columnas esperadas; limpiar cualquier licencia de prueba creada para esta verificación.
- [x] 8.2 Ejecutar `dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj` y `npm run lint` en `src/LicenciasMedicas.Web.Client`, confirmando que ambos pasan.
