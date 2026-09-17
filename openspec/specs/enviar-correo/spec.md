# enviar-correo Specification

## Purpose
Permite redactar y enviar directamente por correo electrónico, con un clic, el aviso de las licencias médicas grabadas de una unidad en una fecha determinada, sin depender de copiar y pegar el texto en un cliente de correo externo.

## Requirements

### Requirement: Texto editable antes de enviar
El sistema SHALL permitir editar libremente el texto del correo generado para una unidad y fecha antes de enviarlo o copiarlo.

#### Scenario: Editar el texto generado
- **WHEN** la persona usuaria modifica el contenido del texto generado para una unidad
- **THEN** el sistema conserva esos cambios y los usa como cuerpo del correo al enviarlo o copiarlo a continuación

### Requirement: Envío directo por correo
El sistema SHALL permitir enviar el texto (editado o no) como correo electrónico directamente a la unidad seleccionada, usando el correo electrónico registrado de la unidad como destinatario y agregando en copia (CC) todos los correos en copia activos configurados globalmente (independiente de la unidad), sin requerir un cliente de correo externo.

#### Scenario: Envío exitoso
- **WHEN** la persona usuaria confirma el envío del correo para una unidad con correo electrónico válido
- **THEN** el sistema envía el correo al destinatario de la unidad y muestra una confirmación de éxito

#### Scenario: Falla al enviar
- **WHEN** el envío no puede completarse por un error del servidor SMTP configurado (servidor inalcanzable, credenciales inválidas u otro error)
- **THEN** el sistema informa el error a la persona usuaria y no lo registra como enviado

#### Scenario: Envío con correos en copia configurados
- **WHEN** hay uno o más correos en copia activos configurados
- **THEN** el sistema los agrega como CC en el correo enviado a la unidad, además del destinatario principal

#### Scenario: Envío sin correos en copia configurados
- **WHEN** no hay ningún correo en copia activo configurado
- **THEN** el sistema envía el correo solo al destinatario de la unidad, sin copia

### Requirement: Confirmación previa al envío
El sistema SHALL requerir una confirmación explícita de la persona usuaria, mostrando el destinatario y la cantidad de licencias incluidas, antes de disparar el envío real del correo.

#### Scenario: Cancelar el envío
- **WHEN** la persona usuaria abre la confirmación de envío y la cancela
- **THEN** el sistema no envía el correo

### Requirement: Unidad sin correo electrónico válido
El sistema SHALL impedir el envío cuando la unidad seleccionada no tiene un correo electrónico con formato válido registrado, indicando que debe completarse en la gestión de unidades.

#### Scenario: Unidad sin correo configurado
- **WHEN** se intenta enviar el correo de una unidad que no tiene un correo electrónico con formato válido registrado
- **THEN** el sistema impide el envío y muestra un mensaje indicando que debe completarse el correo de la unidad

### Requirement: Registro de envíos y aviso de reenvío
El sistema SHALL registrar la fecha y hora del último envío exitoso para cada combinación de unidad y fecha de licencias, y SHALL advertir en la confirmación cuando ya exista un envío previo para esa misma combinación, permitiendo igualmente reenviar si la persona usuaria lo confirma.

#### Scenario: Reintento de envío ya realizado
- **WHEN** se inicia el envío para una unidad y fecha que ya fueron enviadas exitosamente antes
- **THEN** el sistema muestra en la confirmación cuándo fue el envío anterior y permite continuar con el reenvío si se confirma

#### Scenario: Primer envío de la combinación
- **WHEN** se inicia el envío para una unidad y fecha que nunca se enviaron antes
- **THEN** el sistema no muestra aviso de envío previo
