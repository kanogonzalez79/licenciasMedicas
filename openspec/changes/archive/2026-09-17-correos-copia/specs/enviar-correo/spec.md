## MODIFIED Requirements

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
