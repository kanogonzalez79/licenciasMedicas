## Context

El ingreso de licencias hoy tiene un único camino: `ProcesamientoService.Procesar()` lee PDFs de `incoming/`, los pasa por `LicenciaExtractionService` (Tipo1/Tipo2), y deja filas en `LicenciaRevision` (staging) para que la persona usuaria asigne Unidad y confirme con `Grabar()`, que mueve el PDF a archivo permanente e inserta en `Licencias` vía `LicenciasRepository.Insertar`. Ese `Insertar` ya maneja folio duplicado a nivel de base de datos (índice único `UX_Licencias_Folio`, capturado como `FolioDuplicadoException`).

El ingreso manual (ver proposal.md - Why) no pasa por el parser ni por el staging: es un formulario que va directo a `Licencias`, en un solo paso, siempre con un documento adjunto. No hay cambios de esquema porque `Licencias` ya exige `Folio`, `RutaPdfArchivado`, `NombreArchivoOriginal` y `HashArchivoSha256` (todo eso existe también en el ingreso manual, solo que el documento no pasó por OCR).

## Goals / Non-Goals

**Goals:**
- Reutilizar la lógica de validación de duplicados y el archivado de documentos que ya existen para el flujo automático, en vez de reimplementarlas.
- Dejar registrado de forma inequívoca qué licencias fueron tipeadas a mano vs. leídas por el parser.
- No introducir una segunda tabla ni un segundo modelo de datos para las licencias manuales: son licencias como cualquier otra, con un origen distinto.

**Non-Goals:**
- No se modifica el pipeline `Procesar`/`Grabar`/`LicenciaRevision` existente.
- No se agrega edición posterior de licencias ya grabadas (ni manuales ni automáticas) — eso es una capability aparte si se llega a pedir.
- No se soporta ingresar una licencia sin documento de respaldo (decidido explícitamente: siempre hay adjunto).

## Decisions

**Origen como valor de `TipoFormulario`, no como columna nueva.** `TipoFormulario` (`Tipo1=1`, `Tipo2=2`) ya es, en la práctica, "cómo se originó este registro". Se agrega `Manual=3` al enum existente en vez de crear una columna `Origen`/`EsManual` separada. Evita migración de esquema (la columna ya es `INTEGER NOT NULL`) y evita tener dos campos que puedan quedar inconsistentes entre sí. La API expone además un campo derivado (`esIngresoManual: bool`, calculado a partir de `TipoFormulario == Manual`) en las respuestas de búsqueda/detalle, para que el frontend no tenga que conocer el mapeo de códigos.

**Guardado directo reutilizando `LicenciasRepository.Insertar`, sin pasar por `LicenciaRevision`.** Alternativa considerada: insertar igual una fila de staging y "auto-confirmarla". Se descarta porque `LicenciaRevision.HashArchivoSha256` tiene índice único pensado para deduplicar archivos que llegan por lote, y el flujo manual es de a una licencia por vez con confirmación inmediata — pasar por staging solo agregaría un paso sin beneficio (no hay nada que "revisar" después, la persona ya revisó lo que tipeó).

**Duplicados: mismo criterio que el pipeline automático, contra ambas tablas.** El nuevo servicio de ingreso manual valida folio y hash del adjunto contra `Licencias` (`LicenciasRepository.ExisteFolioOHash`) **y** contra `LicenciaRevision` (`StagingRepository.ExisteFolioEnStaging` / `ExisteHashEnStaging`), igual que hace `ProcesamientoService.Procesar()` hoy. Motivo: un PDF podría estar pendiente de revisión en la grilla automática con el mismo folio que alguien está por tipear a mano. El chequeo previo es una validación de UX (mensaje claro antes de intentar grabar); el índice único de `Licencias.Folio` sigue siendo la barrera real contra condiciones de carrera, igual que hoy con `FolioDuplicadoException`.

**Archivado del documento: mismo esquema de rutas que el flujo automático.** El helper que arma la ruta permanente (`{año}/{mes}/{folio}__{guid}.pdf` bajo `ArchivoDir`) hoy es privado de `ProcesamientoService`. Se extrae a un lugar compartido (por ejemplo un método estático o un pequeño helper en `Paths`) para que el nuevo servicio de ingreso manual lo use tal cual, incluyendo para imágenes (la extensión del archivo archivado sigue la del adjunto original, no se fuerza `.pdf`).

**Endpoint: multipart/form-data en un solo POST.** `POST /api/licencias/manual` recibe los campos del formulario más el archivo adjunto en una sola request multipart (primer uso de `IFormFile`/multipart en este backend — hasta ahora todo el manejo de archivos es por filesystem local). Alternativa considerada (subir el archivo primero a un endpoint temporal y luego crear la licencia referenciándolo) se descarta por agregar estado intermedio (archivos huérfanos si el segundo paso falla) sin necesidad, dado que todo ocurre en una sola interacción de la persona usuaria.

**Cálculo de fecha/días: lógica nueva, sin precedente en el parser.** Ningún extractor calcula días desde fechas (siempre leen el valor impreso). Se implementa `dias = (fechaTermino.DayNumber - fechaInicio.DayNumber) + 1` como única fórmula, y su inversa `fechaTermino = fechaInicio.AddDays(dias - 1)`, en el frontend (recalcula en cada cambio de los campos relevantes) y se revalida en el backend contra los tres valores enviados (si los tres llegan inconsistentes, se rechaza).

**Validaciones de negocio en el backend, no solo en el formulario.** RUT (módulo 11, vía `RutUtils.TryParse`), código de tipo de licencia (`CatalogoTipoLicencia.EsCodigoValido`), coherencia de fechas/días y presencia de campos obligatorios se validan en el servicio backend, no solo en el cliente — el formulario del frontend da feedback inmediato, pero el backend es la fuente de verdad (igual que el resto del sistema).

## Risks / Trade-offs

- **[Riesgo] Carga de archivos vía HTTP es nueva en este backend (nada de límites de tamaño, tipo mime, etc. configurado hoy)** → Mitigación: restringir por tipo de contenido a PDF/jpg/png y aplicar un límite de tamaño razonable (ej. 20 MB) a nivel del endpoint, igual que cualquier upload.
- **[Riesgo] El origen "Manual" no tiene forma de revertirse si alguien se equivoca al tipear el formulario** → Mitigación: no distinta a la de hoy — la licencia se puede eliminar por completo con la funcionalidad existente de `eliminar-licencia` y volver a ingresarse; no se agrega edición en este cambio (Non-Goal).
- **[Trade-off] `TipoFormulario` deja de significar solo "formato del PDF reconocido" y pasa a significar también "origen del registro"** → Aceptado conscientemente: es la opción sin migración de esquema, y el significado ampliado ("cómo se originó este registro") es coherente con los tres valores.

## Migration Plan

Cambio aditivo puro: nuevo endpoint, nuevo valor de enum, nuevos elementos de UI. No requiere migración de base de datos ni afecta datos existentes. Se despliega en un solo paso; si hay que revertir, basta con ocultar el botón "Ingresar manual" y dejar de exponer el endpoint (las licencias ya grabadas con `TipoFormulario=Manual` seguirían siendo válidas y visibles).
