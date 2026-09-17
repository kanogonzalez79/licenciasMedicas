# Ingresar Licencia Manual Specification

## Purpose

Permite ingresar los datos de una licencia médica directamente a través de un formulario, junto con su documento de respaldo (PDF o imagen), para los casos en que el documento no puede procesarse automáticamente por el parser.

## Requirements

### Requirement: Acceso al ingreso manual de licencias
El sistema SHALL ofrecer, en la pantalla de Ingresar licencias, un control para abrir un formulario de ingreso manual de una licencia.

#### Scenario: Abrir el formulario de ingreso manual
- **WHEN** la persona usuaria hace clic en "Ingresar manual"
- **THEN** se muestra un formulario para completar los datos de la licencia y adjuntar su documento de respaldo

### Requirement: Documento de respaldo obligatorio
El sistema SHALL exigir un documento adjunto (PDF o imagen) para poder grabar una licencia ingresada manualmente.

#### Scenario: Intento de grabar sin adjunto
- **WHEN** la persona usuaria intenta grabar el formulario sin haber adjuntado un documento
- **THEN** el sistema rechaza el guardado e indica que el documento es obligatorio

#### Scenario: Formatos de documento aceptados
- **WHEN** la persona usuaria adjunta un archivo en formato PDF o imagen (jpg/png)
- **THEN** el sistema acepta el archivo como documento de respaldo

### Requirement: Datos obligatorios del formulario
El sistema SHALL exigir folio, RUT del paciente válido, nombre del paciente, tipo de licencia, fecha de inicio de reposo, fecha de término de reposo o cantidad de días, y unidad, para poder grabar una licencia ingresada manualmente.

#### Scenario: Falta un dato obligatorio
- **WHEN** la persona usuaria intenta grabar el formulario sin completar alguno de los datos obligatorios
- **THEN** el sistema rechaza el guardado e indica qué dato falta

#### Scenario: RUT del paciente inválido
- **WHEN** el RUT ingresado para el paciente no corresponde a un RUT chileno válido (dígito verificador incorrecto)
- **THEN** el sistema rechaza el guardado e indica que el RUT no es válido

### Requirement: Cálculo automático de fechas y días de reposo
El sistema SHALL calcular automáticamente el dato de reposo faltante entre fecha de término y cantidad de días, en base a los otros dos datos ingresados, usando un conteo inclusivo de días.

#### Scenario: Se ingresan las dos fechas
- **WHEN** la persona usuaria ingresa fecha de inicio y fecha de término de reposo
- **THEN** el sistema calcula y muestra la cantidad de días como la diferencia entre ambas fechas más un día

#### Scenario: Se ingresan fecha de inicio y cantidad de días
- **WHEN** la persona usuaria ingresa la fecha de inicio de reposo y la cantidad de días
- **THEN** el sistema calcula y muestra la fecha de término de reposo correspondiente

### Requirement: Prevención de folios y documentos duplicados
El sistema SHALL rechazar el ingreso manual de una licencia cuyo folio o cuyo documento adjunto (por hash) ya corresponda a una licencia grabada o a una licencia pendiente de revisión del ingreso automático.

#### Scenario: Folio ya grabado
- **WHEN** el folio ingresado ya corresponde a una licencia grabada en el sistema
- **THEN** el sistema rechaza el guardado e indica que el folio ya existe

#### Scenario: Documento ya usado en una licencia pendiente de revisión
- **WHEN** el documento adjunto corresponde (por hash) a un archivo que está pendiente de revisión en el ingreso automático
- **THEN** el sistema rechaza el guardado e indica que el documento ya está en uso

### Requirement: Guardado directo de la licencia manual
El sistema SHALL grabar la licencia ingresada manualmente en un solo paso, sin requerir una asignación de unidad posterior ni pasar por la grilla de revisión del ingreso automático.

#### Scenario: Guardado exitoso
- **WHEN** la persona usuaria completa el formulario con todos los datos obligatorios y un documento válido, y confirma el guardado
- **THEN** el sistema archiva el documento, graba la licencia y la deja disponible de inmediato en las búsquedas

### Requirement: Marca de origen manual
El sistema SHALL registrar que una licencia fue ingresada manualmente, de forma que se pueda distinguir de las licencias ingresadas mediante el parser automático.

#### Scenario: Origen registrado al grabar
- **WHEN** se graba una licencia a través del formulario de ingreso manual
- **THEN** la licencia queda registrada con origen "Manual"

### Requirement: Mostrar origen de la licencia en la búsqueda
El sistema SHALL mostrar, en los resultados de búsqueda de licencias, si cada licencia fue ingresada de forma manual o automática.

#### Scenario: Licencia ingresada manualmente
- **WHEN** los resultados de búsqueda incluyen una licencia ingresada mediante el formulario manual
- **THEN** la fila correspondiente indica que su origen es "Manual"

#### Scenario: Licencia ingresada automáticamente
- **WHEN** los resultados de búsqueda incluyen una licencia ingresada mediante el parser automático (PDF)
- **THEN** la fila correspondiente indica que su origen es "Automático"

### Requirement: Marcar licencia como correo ya enviado al ingresar manualmente
El sistema SHALL permitir marcar, en el formulario de ingreso manual, que el correo de aviso de la licencia ya fue enviado (por ejemplo por tratarse de una licencia atrasada notificada antes de ingresarla al sistema), sin que esa marca sea obligatoria para grabar.

#### Scenario: Marcar la licencia como correo ya enviado
- **WHEN** la persona usuaria marca la opción "correo ya enviado" al completar el formulario de ingreso manual y confirma el guardado
- **THEN** la licencia queda grabada con su correo de aviso marcado como ya enviado

#### Scenario: No marcar la opción
- **WHEN** la persona usuaria completa el formulario de ingreso manual sin marcar la opción "correo ya enviado"
- **THEN** la licencia queda grabada con su correo de aviso pendiente, igual que el comportamiento actual
