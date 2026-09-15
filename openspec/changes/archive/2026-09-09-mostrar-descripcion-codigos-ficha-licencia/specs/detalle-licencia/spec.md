## ADDED Requirements

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
