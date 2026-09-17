# Detalle Licencia Specification

## Purpose

Permite a la persona usuaria ver, desde la pantalla de búsqueda de licencias, el detalle completo de una licencia tal como está almacenado en la base de datos, para poder verificar o auditar esos datos sin depender del PDF archivado.

## Requirements

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

### Requirement: Mostrar origen de la licencia en la ficha de detalle
El sistema SHALL mostrar, en la ficha de detalle de una licencia, si esta fue ingresada de forma manual o automática.

#### Scenario: Licencia ingresada manualmente
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia ingresada mediante el formulario manual
- **THEN** la ficha indica que su origen es "Manual"

#### Scenario: Licencia ingresada automáticamente
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia ingresada mediante el parser automático (PDF)
- **THEN** la ficha indica que su origen es "Automático"

### Requirement: Mostrar descripción de campos codificados en la ficha de detalle
El sistema SHALL mostrar, para los campos "Tipo de formulario" y "Sexo" de la ficha de detalle, la descripción textual del código almacenado junto al código mismo. Cuando el valor almacenado no corresponda a ningún código conocido, la ficha SHALL mostrar el valor tal como está almacenado, sin inventar una descripción.

#### Scenario: Descripción de tipo de formulario
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia
- **THEN** el campo "Tipo de formulario" muestra la descripción correspondiente a su código: "Formulario tipo 1" para el código 1, "Formulario tipo 2" para el código 2, y "Manual" para el código 3

#### Scenario: Descripción de sexo del paciente
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia con sexo del paciente informado
- **THEN** el campo "Sexo" muestra la descripción correspondiente a su código: "Masculino" para el código "M" y "Femenino" para el código "F"

#### Scenario: Código sin descripción conocida
- **WHEN** el valor almacenado de "Tipo de formulario" o "Sexo" no corresponde a ninguno de los códigos conocidos
- **THEN** la ficha muestra ese valor tal como está almacenado, sin descripción inventada

### Requirement: Ver y cambiar el estado de correo enviado desde la ficha de detalle
El sistema SHALL mostrar en la ficha de detalle si el correo de aviso de la licencia ya fue enviado o está pendiente, y SHALL permitir cambiar ese estado en cualquier momento, independientemente del origen de la licencia o de cuándo fue grabada.

#### Scenario: Marcar como correo enviado desde la ficha
- **WHEN** la persona usuaria cambia el estado de correo enviado a "enviado" desde la ficha de detalle de una licencia pendiente
- **THEN** el sistema guarda el cambio y la licencia deja de considerarse pendiente de aviso

#### Scenario: Revertir a pendiente desde la ficha
- **WHEN** la persona usuaria cambia el estado de correo enviado a "pendiente" desde la ficha de detalle de una licencia marcada como enviada
- **THEN** el sistema guarda el cambio y la licencia vuelve a considerarse pendiente de aviso
