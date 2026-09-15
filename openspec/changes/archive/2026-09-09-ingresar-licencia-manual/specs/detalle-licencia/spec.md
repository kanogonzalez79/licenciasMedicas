## ADDED Requirements

### Requirement: Mostrar origen de la licencia en la ficha de detalle
El sistema SHALL mostrar, en la ficha de detalle de una licencia, si esta fue ingresada de forma manual o automática.

#### Scenario: Licencia ingresada manualmente
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia ingresada mediante el formulario manual
- **THEN** la ficha indica que su origen es "Manual"

#### Scenario: Licencia ingresada automáticamente
- **WHEN** la persona usuaria abre la ficha de detalle de una licencia ingresada mediante el parser automático (PDF)
- **THEN** la ficha indica que su origen es "Automático"
