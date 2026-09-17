## ADDED Requirements

### Requirement: Ver y cambiar el estado de correo enviado desde la ficha de detalle
El sistema SHALL mostrar en la ficha de detalle si el correo de aviso de la licencia ya fue enviado o está pendiente, y SHALL permitir cambiar ese estado en cualquier momento, independientemente del origen de la licencia o de cuándo fue grabada.

#### Scenario: Marcar como correo enviado desde la ficha
- **WHEN** la persona usuaria cambia el estado de correo enviado a "enviado" desde la ficha de detalle de una licencia pendiente
- **THEN** el sistema guarda el cambio y la licencia deja de considerarse pendiente de aviso

#### Scenario: Revertir a pendiente desde la ficha
- **WHEN** la persona usuaria cambia el estado de correo enviado a "pendiente" desde la ficha de detalle de una licencia marcada como enviada
- **THEN** el sistema guarda el cambio y la licencia vuelve a considerarse pendiente de aviso
