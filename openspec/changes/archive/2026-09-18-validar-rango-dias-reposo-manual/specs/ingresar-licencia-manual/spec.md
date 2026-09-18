## MODIFIED Requirements

### Requirement: Cálculo automático de fechas y días de reposo
El sistema SHALL calcular automáticamente el dato de reposo faltante entre fecha de término y cantidad de días, en base a los otros dos datos ingresados, usando un conteo inclusivo de días. El sistema SHALL evitar cualquier error no controlado al calcular, incluso cuando los valores ingresados producirían una fecha fuera del rango representable.

#### Scenario: Se ingresan las dos fechas
- **WHEN** la persona usuaria ingresa fecha de inicio y fecha de término de reposo
- **THEN** el sistema calcula y muestra la cantidad de días como la diferencia entre ambas fechas más un día

#### Scenario: Se ingresan fecha de inicio y cantidad de días
- **WHEN** la persona usuaria ingresa la fecha de inicio de reposo y la cantidad de días
- **THEN** el sistema calcula y muestra la fecha de término de reposo correspondiente

#### Scenario: Cantidad de días produce una fecha fuera de rango
- **WHEN** la persona usuaria ingresa una cantidad de días (u otro valor) que haría que la fecha de término calculada quedara fuera del rango de fechas representable
- **THEN** el sistema no realiza el cálculo automático ni lanza un error, y el formulario permanece usable con el campo dependiente sin actualizar

## ADDED Requirements

### Requirement: Rango máximo de días de reposo
El sistema SHALL rechazar el guardado de una licencia (manual) cuyo período de reposo, calculado entre la fecha de inicio y la fecha de término, o indicado como cantidad de días, supere los 365 días.

#### Scenario: Rango de reposo excede el máximo permitido
- **WHEN** la persona usuaria intenta grabar el formulario y la diferencia entre fecha de inicio y fecha de término (o la cantidad de días ingresada) supera 365 días
- **THEN** el sistema rechaza el guardado e indica que el rango de reposo supera el máximo permitido

#### Scenario: Rango de reposo dentro del máximo permitido
- **WHEN** la persona usuaria completa el formulario con un período de reposo de 365 días o menos
- **THEN** el sistema no aplica esta restricción y permite continuar con el guardado
