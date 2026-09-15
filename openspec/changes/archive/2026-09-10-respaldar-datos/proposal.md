## Why

Hoy no existe ningún mecanismo de respaldo: todos los datos (base de datos SQLite y los PDF archivados de las licencias grabadas) viven únicamente en el disco del equipo donde corre la aplicación. Si ese disco falla, todo el archivo de licencias médicas grabadas se pierde de forma permanente. Se necesita una forma de respaldar manualmente lo ya grabado hacia un disco externo, antes de que existan datos reales en producción.

## What Changes

- Se agrega un botón "Respaldar" que, al presionarlo, exige elegir una carpeta destino (típicamente en un disco externo) antes de ejecutar nada.
- El respaldo cubre únicamente lo ya grabado de forma permanente: la base de datos (`licencias.db`) y los PDF archivados (`archivo/`). No incluye lo pendiente de revisión o asignación de unidad (`staging/`, `incoming/`), que se puede volver a obtener reprocesando o reingresando el documento original.
- La base de datos se respalda tomando una copia consistente (no una copia de archivo cruda mientras la aplicación la tiene abierta) y **sobrescribe siempre** el `licencias.db` anterior en la carpeta destino: no se mantiene historial de versiones de la base de datos.
- Los PDF archivados se respaldan como un **espejo incremental**: se copian a la carpeta destino los PDF que están en el origen y faltan en el destino, y se eliminan del destino los PDF que ya no existen en el origen (por ejemplo, porque la licencia fue eliminada). Esto es intencional: el respaldo protege contra la falla del disco principal, no contra un borrado accidental en el origen — las licencias eliminadas se pueden volver a conseguir desde otro repositorio si fuera necesario.
- Al finalizar, se muestra un resumen del resultado (base de datos actualizada, cantidad de PDF copiados y cantidad de PDF eliminados en el destino).
- Si la carpeta destino no está disponible o no se puede escribir en ella (por ejemplo, el disco externo no está conectado), el respaldo se rechaza con un mensaje de error claro y no se modifica nada en el destino.

## Capabilities

### New Capabilities
- `respaldo-datos`: Permite respaldar manualmente, hacia una carpeta elegida por la persona usuaria, la base de datos y los PDF archivados de las licencias ya grabadas.

### Modified Capabilities

(ninguna — este cambio es puramente aditivo, no modifica el comportamiento de capacidades existentes)

## Impact

- Backend: nuevo endpoint y servicio de respaldo (snapshot consistente de SQLite + sincronización de archivos de `archivo/` hacia la carpeta destino). Requiere un mecanismo para abrir un selector nativo de carpeta de Windows desde el proceso backend, dado que un `<input>` HTML estándar no entrega una ruta de sistema de archivos utilizable por el servidor.
- Frontend: nuevo punto de entrada en la interfaz (botón/página "Respaldar") que dispara la selección de carpeta y muestra el resultado del respaldo.
- No afecta la base de datos de la aplicación en sí (no se agregan tablas ni columnas), ni el comportamiento de ingreso, búsqueda, eliminación o correos existente.
