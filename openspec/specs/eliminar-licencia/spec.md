# Eliminar Licencia Specification

## Purpose

Permite a la persona usuaria eliminar, desde los resultados de la búsqueda de licencias, una licencia ya grabada junto con su PDF archivado, evitando así que un registro grabado por error quede permanentemente en el sistema sin forma de corregirlo.

## Requirements

### Requirement: Control de eliminar en los resultados de búsqueda
El sistema SHALL mostrar, en cada fila de los resultados de búsqueda de licencias, un control para eliminar esa licencia.

#### Scenario: Control visible junto a las demás acciones de la fila
- **WHEN** la persona usuaria realiza una búsqueda que devuelve al menos un resultado
- **THEN** cada fila de resultados muestra un control "Eliminar" junto a "Ver PDF" y "Ver ficha"

### Requirement: Confirmación previa a la eliminación
El sistema SHALL solicitar confirmación antes de eliminar una licencia, y SHALL NOT eliminar nada si la persona usuaria cancela.

#### Scenario: Se pide confirmación al hacer clic en Eliminar
- **WHEN** la persona usuaria hace clic en el control "Eliminar" de una fila
- **THEN** se muestra un diálogo de confirmación antes de ejecutar cualquier borrado

#### Scenario: Cancelar la confirmación no elimina la licencia
- **WHEN** la persona usuaria cancela el diálogo de confirmación
- **THEN** la licencia permanece sin cambios y sigue apareciendo en los resultados de búsqueda

### Requirement: Eliminación física del registro y del PDF archivado
El sistema SHALL, al confirmarse la eliminación, borrar de forma permanente tanto el registro de la licencia en la base de datos como su PDF archivado en disco.

#### Scenario: Confirmar elimina el registro y el archivo
- **WHEN** la persona usuaria confirma la eliminación de una licencia
- **THEN** el registro de esa licencia deja de existir en la base de datos y su PDF archivado deja de existir en disco

#### Scenario: La licencia eliminada deja de aparecer en la búsqueda
- **WHEN** se repite la misma búsqueda después de eliminar una licencia
- **THEN** esa licencia ya no aparece entre los resultados

#### Scenario: El folio queda disponible para reingresar la licencia
- **WHEN** una licencia con un folio determinado fue eliminada
- **THEN** el sistema permite grabar una nueva licencia con ese mismo folio

### Requirement: Manejo de errores al eliminar
El sistema SHALL informar a la persona usuaria cuando la eliminación no se pudo completar, y SHALL completar la eliminación del registro aunque el PDF archivado ya no exista en disco.

#### Scenario: Intentar eliminar una licencia que ya no existe
- **WHEN** se solicita eliminar una licencia cuyo identificador ya no corresponde a ningún registro (por ejemplo, fue eliminada previamente)
- **THEN** el sistema responde que la licencia no fue encontrada y muestra un mensaje de error a la persona usuaria, sin afectar otros registros

#### Scenario: El PDF archivado ya no está en disco
- **WHEN** se confirma la eliminación de una licencia cuyo PDF archivado ya no existe en disco
- **THEN** el registro de la licencia se elimina igualmente de la base de datos, sin mostrar un error por el archivo faltante
