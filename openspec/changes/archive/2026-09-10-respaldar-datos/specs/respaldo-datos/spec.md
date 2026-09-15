## Purpose

Permite a la persona usuaria respaldar manualmente, hacia una carpeta que ella misma elige (típicamente en un disco externo), la base de datos y los PDF archivados de las licencias ya grabadas, para proteger esos datos ante una falla del disco donde corre la aplicación.

## ADDED Requirements

### Requirement: Control de respaldo manual
El sistema SHALL ofrecer un control ("Respaldar") que permita a la persona usuaria iniciar manualmente un respaldo de los datos grabados. El sistema SHALL NOT ejecutar un respaldo de forma automática o programada sin que la persona usuaria lo solicite explícitamente.

#### Scenario: Iniciar el respaldo
- **WHEN** la persona usuaria hace clic en el control "Respaldar"
- **THEN** el sistema solicita elegir una carpeta destino antes de ejecutar el respaldo

### Requirement: Carpeta destino obligatoria
El sistema SHALL exigir que la persona usuaria elija una carpeta destino antes de ejecutar el respaldo, y SHALL NOT ejecutar ninguna operación de copiado o borrado si no se eligió una carpeta.

#### Scenario: Cancelar la selección de carpeta
- **WHEN** la persona usuaria cierra o cancela el selector de carpeta sin elegir una
- **THEN** el sistema no ejecuta ningún respaldo y no modifica nada en ninguna carpeta

#### Scenario: Carpeta elegida
- **WHEN** la persona usuaria elige una carpeta destino válida
- **THEN** el sistema ejecuta el respaldo hacia esa carpeta

### Requirement: Alcance del respaldo
El sistema SHALL respaldar únicamente los datos grabados de forma permanente: la base de datos de licencias y los PDF archivados de licencias ya grabadas. El sistema SHALL NOT incluir en el respaldo las licencias pendientes de revisión, la asignación de unidad pendiente, ni los archivos sin procesar todavía.

#### Scenario: Licencias pendientes no incluidas
- **WHEN** se ejecuta un respaldo mientras existen licencias pendientes de revisión o archivos sin procesar
- **THEN** esos archivos no se copian a la carpeta destino

### Requirement: Respaldo de la base de datos
El sistema SHALL respaldar la base de datos tomando una copia consistente de su contenido, y SHALL sobrescribir siempre en la carpeta destino la copia de base de datos de un respaldo anterior, sin conservar versiones previas.

#### Scenario: Segundo respaldo sobrescribe el anterior
- **WHEN** se ejecuta un respaldo hacia una carpeta destino que ya contiene una copia de la base de datos de un respaldo anterior
- **THEN** esa copia se reemplaza por la copia actual, sin dejar ambas versiones

#### Scenario: Copia consistente aunque la aplicación esté en uso
- **WHEN** se ejecuta un respaldo mientras la aplicación está en uso
- **THEN** la copia de la base de datos en la carpeta destino queda en un estado consistente, sin datos corruptos ni a medio escribir

### Requirement: Respaldo incremental de PDF archivados
El sistema SHALL copiar a la carpeta destino los PDF archivados que existan en el origen y no existan todavía en el destino, sin volver a copiar los que ya están presentes en el destino.

#### Scenario: Solo se copian los PDF nuevos
- **WHEN** se ejecuta un respaldo hacia una carpeta destino que ya contiene los PDF de un respaldo anterior
- **THEN** el sistema copia únicamente los PDF archivados que se agregaron al origen desde el respaldo anterior

### Requirement: Eliminación en el destino de PDF que ya no existen en el origen
El sistema SHALL eliminar de la carpeta destino los PDF archivados que ya no existan en el origen (por ejemplo, porque la licencia correspondiente fue eliminada), de forma que la carpeta destino refleje siempre el estado actual del origen.

#### Scenario: PDF eliminado en el origen se elimina también en el respaldo
- **WHEN** una licencia fue eliminada del sistema y su PDF ya no existe en el origen
- **THEN** el siguiente respaldo elimina ese PDF de la carpeta destino si estaba presente de un respaldo anterior

### Requirement: Carpeta destino no disponible
El sistema SHALL rechazar el respaldo con un mensaje de error claro cuando la carpeta destino elegida no está disponible o no se puede escribir en ella. Si la carpeta destino no está disponible desde antes de iniciar el respaldo, el sistema SHALL NOT modificar nada en ella.

#### Scenario: Carpeta destino no disponible desde el inicio
- **WHEN** la persona usuaria elige una carpeta destino que ya no está disponible o no se puede escribir en ella al momento de iniciar el respaldo
- **THEN** el sistema informa el error a la persona usuaria y no modifica nada en esa carpeta

#### Scenario: Disco externo desconectado a mitad del respaldo
- **WHEN** la carpeta destino deja de estar disponible después de haber empezado a copiar PDFs, pero antes de completarse el respaldo (por ejemplo, el disco externo fue desconectado)
- **THEN** el sistema informa el error a la persona usuaria y no continúa con el respaldo, aunque la carpeta destino puede quedar con PDFs que ya se habían copiado o que todavía no se eliminaron; el sistema SHALL NOT dejar la carpeta destino con menos datos de los que ya tenía antes del respaldo

### Requirement: Resumen del resultado del respaldo
El sistema SHALL mostrar, al finalizar un respaldo exitoso, un resumen con la cantidad de PDF copiados y la cantidad de PDF eliminados en el destino.

#### Scenario: Resumen tras un respaldo exitoso
- **WHEN** un respaldo termina exitosamente
- **THEN** el sistema muestra cuántos PDF se copiaron y cuántos se eliminaron en la carpeta destino
