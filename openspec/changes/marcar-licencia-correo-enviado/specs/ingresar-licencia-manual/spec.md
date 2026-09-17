## ADDED Requirements

### Requirement: Marcar licencia como correo ya enviado al ingresar manualmente
El sistema SHALL permitir marcar, en el formulario de ingreso manual, que el correo de aviso de la licencia ya fue enviado (por ejemplo por tratarse de una licencia atrasada notificada antes de ingresarla al sistema), sin que esa marca sea obligatoria para grabar.

#### Scenario: Marcar la licencia como correo ya enviado
- **WHEN** la persona usuaria marca la opción "correo ya enviado" al completar el formulario de ingreso manual y confirma el guardado
- **THEN** la licencia queda grabada con su correo de aviso marcado como ya enviado

#### Scenario: No marcar la opción
- **WHEN** la persona usuaria completa el formulario de ingreso manual sin marcar la opción "correo ya enviado"
- **THEN** la licencia queda grabada con su correo de aviso pendiente, igual que el comportamiento actual
