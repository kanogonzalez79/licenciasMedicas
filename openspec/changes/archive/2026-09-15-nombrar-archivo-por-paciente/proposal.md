## Why

Hoy, al archivar definitivamente un documento (PDF o imagen) de una licencia, el archivo queda nombrado como `{folio}__{sufijo aleatorio}` dentro de `data/archivo/{año}/{mes}/`. Ese nombre es opaco: quien navega la carpeta a mano no puede identificar de quién es el documento sin abrir el sistema. Se necesita que el nombre del archivo incluya el nombre completo del paciente, para que la carpeta por año/mes sea navegable por humanos.

## What Changes

- Al archivar un documento (automático o manual), el nombre de archivo pasa a ser `{Nombre Completo Paciente en Título Case} - {Folio}{extensión}`, en vez de `{folio}__{sufijo aleatorio}{extensión}`.
- El nombre del paciente se normaliza a Título Case (primera letra de cada palabra en mayúscula, resto en minúscula), preservando tildes y `ñ`. Como el nombre suele venir en MAYÚSCULAS desde el parser automático, la normalización debe pasar primero a minúsculas antes de aplicar Título Case.
- Se reutiliza la sanitización de caracteres inválidos para nombre de archivo que ya existe para el folio, aplicada también al nombre del paciente.
- No se introduce un correlativo: el folio ya es único en todo el sistema (se valida antes de archivar), por lo que `Nombre - Folio` no puede chocar entre dos documentos de la misma persona ni de personas distintas.
- El cambio aplica por igual al flujo automático (`ProcesamientoService.Grabar`) y al ingreso manual (`IngresoManualService.Ingresar`), porque ambos comparten `AppPaths.ResolverRutaArchivoPermanente`.
- La ruta relativa (`Licencia.RutaPdfArchivado`) sigue siendo la fuente de verdad que usa el sistema para ubicar el archivo; el nuevo nombre es solo para navegación humana, no se parsea de vuelta en ninguna parte.

## Capabilities

### New Capabilities
- `archivo-documento-licencia`: define cómo se nombra el documento (PDF o imagen) de una licencia al archivarlo definitivamente en `data/archivo/{año}/{mes}/`.

### Modified Capabilities
(ninguna — ni `ingresar-licencia-manual` ni ninguna otra spec existente describe hoy el esquema de nombre de archivo archivado)

## Impact

- `src/LicenciasMedicas.Core/Paths/AppPaths.cs`: `ResolverRutaArchivoPermanente` pasa a recibir también el nombre completo del paciente y a construir el nombre de archivo con él.
- `src/LicenciasMedicas.Core/Procesamiento/ProcesamientoService.cs`: actualiza el llamado a `ResolverRutaArchivoPermanente` en `Grabar`.
- `src/LicenciasMedicas.Core/Licencias/IngresoManualService.cs`: actualiza el llamado a `ResolverRutaArchivoPermanente` en `Ingresar`.
- No hay cambios de esquema de base de datos ni de API pública: `RutaPdfArchivado` sigue siendo una ruta relativa opaca.
