## Context

La aplicación es un ejecutable .NET self-contained para Windows, de un solo usuario, que expone su UI como una SPA React servida por un backend ASP.NET Core Minimal API escuchando solo en loopback (ver `Program.cs`). Backend y pantalla corren siempre en el mismo equipo físico. Los datos viven bajo `AppPaths.DataDir`: `licencias.db` (SQLite) y `archivo/` (PDFs organizados por año/mes, con nombres únicos `{folio}__{sufijo}.ext` que nunca se sobrescriben in situ). Ver `proposal.md` para la motivación y `specs/respaldo-datos/spec.md` para el comportamiento exigido.

Restricción clave: un `<input>` HTML estándar no entrega al backend una ruta de sistema de archivos usable para hacer copiado/borrado de archivos — solo entrega los bytes de los archivos seleccionados, sin la ruta de carpeta real. Como este es un backend Windows corriendo en el mismo equipo que la pantalla, la solución más simple es que el propio proceso backend muestre el diálogo nativo "Buscar carpeta" de Windows.

## Goals / Non-Goals

**Goals:**
- Selector de carpeta nativo de Windows, disparado por el backend al presionar "Respaldar".
- Copia consistente de `licencias.db` que sobrescribe siempre la anterior en destino.
- Sincronización tipo espejo de `archivo/` hacia destino (copiar faltantes, borrar sobrantes), sin re-copiar archivos ya presentes.
- Errores claros cuando el destino no está disponible o se desconecta a mitad de camino.

**Non-Goals:**
- Programar o automatizar el respaldo (por diseño, es 100% manual, ver proposal).
- Mantener historial/versionado de respaldos anteriores.
- Proteger contra un borrado accidental en el origen (decisión explícita: el espejo replica los borrados).
- Soportar otro sistema operativo distinto de Windows, o destinos que no sean una carpeta local/de red accesible como ruta de archivos (no hay integración con nube).
- Cifrado del respaldo o de la carpeta destino (no se pidió; si el disco externo contiene datos de salud sin cifrar, es una decisión operativa de quien lo use, no de esta funcionalidad).

## Decisions

**Selector de carpeta: diálogo nativo de Windows desde el backend, no desde el navegador.**
El endpoint que atiende el clic en "Respaldar" muestra un `FolderBrowserDialog` (o el picker nativo equivalente) en un hilo STA dedicado del propio proceso backend, y bloquea esa solicitud HTTP hasta que la persona usuaria eligió una carpeta o canceló. Alternativa descartada: la File System Access API del navegador (`showDirectoryPicker()`) — solo funciona en navegadores basados en Chromium, y hubiera obligado a que el respaldo lo ejecute el navegador (bajando cada PDF por HTTP y escribiéndolo mediante el handle del navegador) en vez del backend, mucho más complejo para el mismo resultado visible.

**Copia consistente de la base de datos: `VACUUM INTO` sobre la conexión existente.**
Se ejecuta `VACUUM INTO '<destino>/licencias.db.tmp'` y luego se reemplaza el archivo final en destino solo si el `VACUUM INTO` terminó sin error (escribir a un nombre temporal y renombrar al final evita dejar una copia a medio escribir si algo falla). Alternativa descartada: copiar el archivo `.db` con `File.Copy` — no garantiza consistencia si hay una escritura en curso al mismo tiempo.

**Sincronización de PDFs: comparación por ruta relativa, sin hash de contenido.**
Se listan las rutas relativas (año/mes/archivo) presentes en `archivo/` y en destino, y se copian las que faltan en destino y se borran en destino las que ya no están en origen. No se compara contenido/hash porque los PDF archivados nunca se modifican después de escritos (nombre único por licencia) — comparar solo presencia/ausencia es suficiente y más simple.

**Orden de operaciones: copiar y sobrescribir primero, borrar al final.**
Dentro de una misma ejecución de respaldo: 1) copiar PDFs nuevos, 2) sobrescribir `licencias.db` en destino, 3) recién al final, borrar en destino los PDFs que ya no existen en origen. Así, si el destino se desconecta a mitad de camino, el peor caso es que quede con datos de más (PDFs que debieron borrarse pero no se llegó a esa etapa), nunca con datos de menos.

## Risks / Trade-offs

- [Riesgo] Mostrar un diálogo de Windows Forms desde un proceso ASP.NET Core sin ciclo de mensajes de UI propio podría no comportarse como un diálogo normal (foco, bloqueo, aparición detrás de otras ventanas) → Mitigación: validar esto con un prototipo mínimo antes de construir el resto del endpoint; es el mayor riesgo técnico de este cambio.
- [Riesgo] El disco externo se desconecta a mitad de la sincronización → Mitigación: orden de operaciones descrito arriba (copiar/sobrescribir antes de borrar) más detección temprana de que el destino ya no responde, para abortar con un mensaje claro en vez de lanzar una excepción genérica.
- [Riesgo] Con muchos años de PDFs acumulados, listar y comparar todos los archivos en cada respaldo podría volverse lento → Mitigación: aceptable para el volumen esperado (una persona, un disco, uso ocasional); no se optimiza de entrada, se revisa si llega a ser un problema real.

## Open Questions

- Si la sincronización de PDFs falla a mitad de camino (por ejemplo, se desconecta el disco después de copiar algunos archivos nuevos), ¿el mensaje de error debe indicar cuántos PDFs sí se llegaron a copiar antes de fallar, o basta con un error genérico? No cambia el comportamiento exigido por la spec (que ya exige informar el error), solo el nivel de detalle del mensaje — se puede decidir durante la implementación.
