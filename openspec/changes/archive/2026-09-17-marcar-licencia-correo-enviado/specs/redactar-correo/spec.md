## MODIFIED Requirements

### Requirement: Texto de aviso por unidad y fecha
El sistema SHALL generar, para una unidad y fecha dadas, un texto compuesto por un saludo (desde una plantilla configurable, con el nombre de la unidad interpolado) seguido de una tabla en texto plano con las licencias médicas de esa unidad grabadas en esa fecha que no estén marcadas como "correo ya enviado", con las columnas RUT, Nombre, Fecha Inicio, Fecha Término y Cantidad de Días.

#### Scenario: Unidad sin licencias grabadas en la fecha
- **WHEN** se solicita el texto de aviso para una unidad y fecha sin licencias grabadas
- **THEN** el sistema no genera texto para esa combinación

#### Scenario: Unidad con licencias grabadas en la fecha
- **WHEN** se solicita el texto de aviso para una unidad y fecha con licencias grabadas que no están marcadas como "correo ya enviado"
- **THEN** el sistema genera el saludo seguido de una fila de encabezado y una fila por cada licencia no marcada, con los valores de RUT, Nombre, Fecha Inicio, Fecha Término y Cantidad de Días de esa licencia

#### Scenario: Unidad con todas sus licencias de la fecha marcadas como correo ya enviado
- **WHEN** se solicita el texto de aviso para una unidad y fecha cuyas licencias grabadas están todas marcadas como "correo ya enviado"
- **THEN** el sistema no genera texto para esa combinación, igual que si no tuviera licencias grabadas
