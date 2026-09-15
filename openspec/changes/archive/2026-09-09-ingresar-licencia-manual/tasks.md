## 1. Backend - modelo y helpers compartidos

- [x] 1.1 Agregar `Manual = 3` a `TipoFormulario` (`src/LicenciasMedicas.Core/Parsing/TipoFormulario.cs`) y verificar que el proyecto Core compila
- [x] 1.2 Extraer el helper de ruta de archivo permanente (`ResolverRutaArchivoPermanente` de `ProcesamientoService`) a un método compartido (por ejemplo en `AppPaths` o una clase utilitaria en `Paths`), preservando la extensión original del archivo en vez de forzar `.pdf`, y verificar que `ProcesamientoService.Grabar()` sigue usándolo sin cambios de comportamiento
- [x] 1.3 Agregar `EsIngresoManual` (bool, calculado desde `TipoFormulario == Manual`) a la clase `Licencia` como propiedad calculada y verificar que aparece en la respuesta JSON de `/api/licencias` y `/api/licencias/{id}`

## 2. Backend - servicio de ingreso manual

- [x] 2.1 Crear `IngresoManualService` (o método equivalente) en `LicenciasMedicas.Core` que reciba los datos del formulario + stream/ruta temporal del adjunto, y valide: campos obligatorios presentes, RUT paciente válido (`RutUtils.TryParse`), código de tipo de licencia válido (`CatalogoTipoLicencia.EsCodigoValido`), coherencia entre fecha inicio/fecha término/cantidad de días — verificar con pruebas unitarias que cubran cada validación fallando y pasando
- [x] 2.2 En el mismo servicio, calcular el hash SHA-256 del adjunto y validar duplicados contra `LicenciasRepository.ExisteFolioOHash` y contra `StagingRepository` (folio y hash en `LicenciaRevision`), retornando un motivo de rechazo específico para cada caso — verificar con pruebas unitarias usando una licencia/staging existente en la base de datos de prueba
- [x] 2.3 Implementar el guardado: archivar el adjunto con el helper de la tarea 1.2, insertar en `Licencias` con `TipoFormulario = Manual` vía `LicenciasRepository.Insertar`, y revertir el archivo (borrar) si el insert falla por `FolioDuplicadoException` — verificar con una prueba de integración que graba una licencia manual y queda visible en `Buscar()`

## 3. Backend - endpoint HTTP

- [x] 3.1 Agregar `POST /api/licencias/manual` en `LicenciasEndpoints.cs` que acepte `multipart/form-data` (campos del formulario + `IFormFile` del adjunto), valide tipo de contenido (PDF, JPG, PNG) y tamaño máximo, y delegue en `IngresoManualService` — verificar con una llamada manual (curl/HTTP file) que crea una licencia y con una que la rechaza por tipo de archivo inválido
- [x] 3.2 Mapear los motivos de rechazo del servicio a respuestas 400 con un código de motivo legible (mismo estilo que `motivosLegibles` en `IngresarPage.tsx`) y verificar que cada motivo de la tarea 2.1/2.2 tiene un código distinto en la respuesta

## 4. Frontend - cliente API y formulario

- [x] 4.1 Agregar a `src/lib/api.ts` el tipo de datos del formulario manual y `licenciasApi.ingresarManual(...)` que arme un `FormData` (campos + archivo) y llame a `POST /api/licencias/manual` — verificar que compila y que el body enviado incluye el archivo
- [x] 4.2 Agregar un botón "Ingresar manual" en `IngresarPage.tsx` que abre un `Sheet` (o `Dialog`) con el formulario: folio, adjunto (input de archivo PDF/imagen), Unidad (mismo `Select` ya usado en la grilla), RUT + nombre del paciente, tipo de licencia (`Select` con las 7 opciones de `CatalogoTipoLicencia`), fecha inicio, fecha término, cantidad de días, datos del profesional (opcionales), observaciones — verificar abriendo la app y comprobando que todos los campos se muestran
- [x] 4.3 Implementar el cálculo bidireccional de fecha término / cantidad de días como función pura (fácil de probar) usada por el formulario: si cambian ambas fechas se recalculan los días; si cambian fecha inicio + días se recalcula la fecha término — verificar con casos de prueba (ej. 2026-01-01 a 2026-01-05 = 5 días)
- [x] 4.4 Validar en el formulario antes de enviar (folio no vacío, RUT con formato válido, tipo de licencia seleccionado, fechas/días completos, unidad seleccionada, archivo adjuntado) mostrando errores con `toast.error`, y mostrar los errores devueltos por el backend (tarea 3.2) si la validación de servidor rechaza el envío — verificar intentando grabar con campos faltantes y viendo el mensaje correspondiente
- [x] 4.5 Al grabar exitosamente, cerrar el formulario, limpiar sus campos e invalidar las queries de búsqueda de licencias para que la nueva licencia aparezca de inmediato si la pantalla de búsqueda está abierta

## 5. Frontend - mostrar origen en búsqueda y ficha de detalle

- [x] 5.1 Agregar una columna o indicador (`Badge`) de "Manual"/"Automático" en la tabla de resultados de `BuscarPage.tsx`, usando el campo `esIngresoManual` de la respuesta — verificar visualmente con al menos una licencia de cada origen
- [x] 5.2 Agregar el mismo indicador de origen a `src/components/licencia-detalle-modal.tsx` — verificar abriendo la ficha de una licencia manual y de una automática y comprobando que cada una muestra el origen correcto

## 6. Verificación de extremo a extremo

- [x] 6.1 Levantar la app, ingresar una licencia manual completa con un PDF adjunto, confirmar que aparece en la búsqueda marcada como "Manual", que su ficha de detalle es correcta, y que el PDF archivado se puede abrir con "Ver PDF"
- [x] 6.2 Repetir el flujo con una imagen (jpg/png) como adjunto y confirmar que también se archiva y se puede visualizar/descargar correctamente
- [x] 6.3 Confirmar que ingresar manualmente un folio ya existente (grabado o pendiente en la grilla de "Procesar") es rechazado con un mensaje claro, sin dejar archivos huérfanos en el directorio de archivo permanente
