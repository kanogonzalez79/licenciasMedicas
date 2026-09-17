## Purpose

Registra si el correo de aviso de una licencia médica individual ya fue enviado (por el sistema o por fuera de él, antes de ingresarla), independiente del registro de envíos SMTP por unidad y fecha, para poder ingresar licencias atrasadas sin que vuelvan a aparecer como pendientes de aviso.

## ADDED Requirements

### Requirement: Estado de correo enviado por defecto pendiente
El sistema SHALL registrar, para cada licencia grabada, si su correo de aviso ya fue enviado o está pendiente, comenzando en estado pendiente salvo que se marque explícitamente como enviado durante su ingreso.

#### Scenario: Licencia grabada sin marcar explícitamente
- **WHEN** se graba una licencia sin marcarla explícitamente como "correo enviado"
- **THEN** la licencia queda con su correo de aviso en estado pendiente

### Requirement: Marcar como correo enviado en la revisión del ingreso automático
El sistema SHALL permitir, en la grilla de revisión de licencias leídas por el parser automático, marcar cada fila individualmente como "correo ya enviado" antes de grabarla.

#### Scenario: Marcar una fila de la grilla
- **WHEN** la persona usuaria marca la opción "correo ya enviado" en una fila de la grilla de revisión antes de grabar
- **THEN** esa licencia queda grabada con su correo de aviso marcado como ya enviado

### Requirement: Marcar el lote completo como correo enviado
El sistema SHALL ofrecer, en la grilla de revisión de licencias leídas por el parser automático, una acción que marque como "correo ya enviado" a todas las filas pendientes de esa grilla de una sola vez.

#### Scenario: Marcar todo el lote
- **WHEN** la persona usuaria activa la acción de marcar todo el lote como "correo ya enviado" antes de grabar
- **THEN** todas las filas de la grilla de revisión quedan marcadas como "correo ya enviado", y las licencias correspondientes quedan grabadas con ese estado

#### Scenario: Ajustar filas individuales después de usar la acción de lote
- **WHEN** la persona usuaria desmarca manualmente una fila específica después de haber usado la acción de marcar todo el lote
- **THEN** esa fila en particular queda sin marcar, sin afectar el estado de las demás filas del lote

### Requirement: Exclusión de licencias marcadas de los avisos generados
El sistema SHALL excluir toda licencia marcada como "correo ya enviado" de cualquier conteo o texto de aviso generado para su unidad y fecha, sin importar cuándo fue grabada en el sistema.

#### Scenario: Unidad con licencias mixtas en la misma fecha
- **WHEN** una unidad tiene, para una misma fecha de ingreso al sistema, licencias marcadas como "correo ya enviado" junto con licencias pendientes
- **THEN** el aviso generado para esa unidad y fecha incluye solo las licencias pendientes, sin las marcadas como "correo ya enviado"
