## Context

`AppPaths.ResolverRutaArchivoPermanente(string folio, string extension)` construye hoy la ruta del documento archivado como `{año}/{mes}/{folioSeguro}__{guid8}{extension}`. Es el único punto que arma ese nombre, y lo llaman dos consumidores: `ProcesamientoService.Grabar` (flujo automático) e `IngresoManualService.Ingresar` (flujo manual). Ver proposal.md - Why.

El nombre de archivo es opaco para el resto del sistema: `Licencia.RutaPdfArchivado` guarda la ruta relativa completa en la BD, y es lo único que usan los endpoints que sirven o borran el documento (`GET/DELETE /api/licencias/{id}/pdf`). Ningún código vuelve a parsear el nombre de archivo para extraer folio o nombre.

## Goals / Non-Goals

**Goals:**
- Cambiar el nombre de archivo a `{Nombre Título Case} - {Folio}{extensión}`, sin tocar el esquema de carpetas por año/mes ni la forma en que se guarda `RutaPdfArchivado`.
- Un único punto de normalización de nombre (Título Case + sanitización) reutilizado por ambos flujos.

**Non-Goals:**
- No se agrega un mecanismo de correlativo: el folio ya es único en todo el sistema (validado antes de archivar), así que `Nombre - Folio` no puede chocar.
- No se migran ni renombran los archivos ya archivados con el esquema anterior (`folio__guid`); el cambio aplica solo hacia adelante.
- No se cambia la validación de duplicados por folio/hash, que ya existe.

## Decisions

**Firma del método**: `ResolverRutaArchivoPermanente` pasa a recibir también `nombreCompletoPaciente`, junto a `folio` y `extension`. Alternativa descartada: resolver el nombre en cada llamador y pasar el nombre de archivo ya armado — se descarta porque la normalización (Título Case + sanitización) es lógica de dominio que debe vivir en un solo lugar, igual que hoy vive el sufijo `folioSeguro`.

**Título Case**: pasar el nombre a minúsculas antes de aplicar `TextInfo.ToTitleCase` (con `CultureInfo("es-CL")` o invariante). Motivo: `ToTitleCase` de .NET trata una palabra que ya está toda en mayúsculas como una sigla y la deja intacta — como el nombre extraído por el parser suele venir en MAYÚSCULAS, aplicar `ToTitleCase` directo no tendría efecto. Bajarlo a minúsculas primero evita ese comportamiento.

**Formato del nombre**: `{Nombre} - {Folio}{extensión}`, con guion como separador (decisión explícita del usuario). Se reutiliza la misma sanitización de caracteres inválidos (`Path.GetInvalidFileNameChars()`) que ya se aplica al folio, ahora también sobre el nombre del paciente.

**Sin correlativo**: se descarta explícitamente el mecanismo de "resolver ruta sin colisión" (como el que ya existe en `ProcesamientoService.ResolverRutaSinColision` para incoming/staging) para el archivo permanente, porque el folio por sí solo ya garantiza unicidad. Agregarlo sería complejidad sin beneficio.

## Risks / Trade-offs

- [Nombre del paciente vacío o solo espacios tras sanitizar] → No debería ocurrir: `NombreCompletoPaciente` es un campo requerido en ambos flujos (extracción automática y formulario manual). Si igual ocurriera, el nombre de archivo quedaría como ` - {Folio}{extensión}` (folio sigue garantizando unicidad y el archivo se crea igual).
- [Nombres de archivo largos] → Nombres de pacientes chilenos compuestos (dos apellidos + varios nombres) más folio se mantienen muy por debajo del límite de 255 caracteres de Windows/NTFS; no se trunca.
- [Archivos ya archivados con el esquema anterior] → Quedan con su nombre actual (`folio__guid`); no se renombran retroactivamente. Es un cambio solo hacia adelante, documentado en Non-Goals.

## Migration Plan

Sin migración de datos: el cambio solo afecta a documentos archivados después del deploy. Los registros y archivos existentes no se tocan. No requiere rollback especial: si se revierte el código, los nuevos archivos vuelven a nombrarse con el esquema anterior.
