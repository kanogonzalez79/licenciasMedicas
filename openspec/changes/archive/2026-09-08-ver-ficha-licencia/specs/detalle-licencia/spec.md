## Purpose

Permite a la persona usuaria ver, desde la pantalla de búsqueda de licencias, el detalle completo de una licencia tal como está almacenado en la base de datos, para poder verificar o auditar esos datos sin depender del PDF archivado.

## ADDED Requirements

### Requirement: Ver ficha de una licencia desde los resultados de búsqueda
El sistema SHALL mostrar, en cada fila de los resultados de búsqueda de licencias, un control para abrir el detalle completo de esa licencia.

#### Scenario: Abrir la ficha de una licencia
- **WHEN** la persona usuaria hace clic en el control "Ver ficha" de una fila de resultados
- **THEN** se muestra un modal con el detalle completo de esa licencia

#### Scenario: Cerrar la ficha
- **WHEN** la persona usuaria cierra el modal de detalle
- **THEN** el modal se oculta y la pantalla de búsqueda queda visible e intacta, sin perder los filtros ni los resultados actuales

### Requirement: Contenido de la ficha de detalle
El sistema SHALL mostrar en la ficha de detalle los datos de la licencia almacenados en la base de datos, agrupados en las secciones: Identificación, Paciente, Licencia médica, Profesional y Unidad.

#### Scenario: Datos mostrados coinciden con los del resultado de búsqueda
- **WHEN** la persona usuaria abre la ficha de una licencia desde una fila de resultados
- **THEN** los valores mostrados en la ficha (folio, datos del paciente, tipo y fechas de la licencia, datos del profesional, unidad) corresponden exactamente a los datos de esa licencia devueltos por la búsqueda

#### Scenario: Campo opcional sin valor
- **WHEN** un campo de la licencia (por ejemplo especialidad del profesional u observaciones) no tiene valor almacenado
- **THEN** la ficha lo indica de forma clara en vez de mostrar un valor vacío o inconsistente

### Requirement: Independencia respecto de la visualización del PDF
El sistema SHALL permitir ver la ficha de detalle de una licencia independientemente de si el PDF archivado está disponible o de si fue abierto previamente.

#### Scenario: Ver la ficha sin haber abierto el PDF
- **WHEN** la persona usuaria hace clic en "Ver ficha" sin haber usado el botón "Ver PDF" de esa misma fila
- **THEN** el modal de detalle se abre igualmente con los datos de la licencia

### Requirement: Exclusión de campos técnicos de archivo
El sistema SHALL excluir de la ficha de detalle los campos técnicos de archivo de la licencia: ruta del PDF archivado, nombre de archivo original y hash SHA-256 del archivo.

#### Scenario: Ficha sin datos técnicos de archivo
- **WHEN** la persona usuaria visualiza la ficha de detalle de una licencia
- **THEN** la ficha no muestra la ruta del PDF archivado, el nombre de archivo original ni el hash SHA-256
