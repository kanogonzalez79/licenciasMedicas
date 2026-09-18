# Dashboard Licencias Specification

## Purpose

Ofrecer una vista de resumen que muestre el volumen, la distribución y la tendencia en el tiempo de las licencias médicas dentro de un rango de fechas elegido por la persona usuaria, sin tener que abrir el Excel de informe para armar esos totales a mano.

## Requirements

### Requirement: Rango de fechas obligatorio antes de mostrar contenido
El sistema SHALL requerir una fecha de inicio, una fecha de término y un modo de aplicación del rango (fecha de inicio de reposo, fecha de término de reposo, o intersección de rangos) antes de calcular y mostrar cualquier contenido del dashboard. El sistema SHALL usar las mismas tres opciones de modo que el informe Excel de licencias.

#### Scenario: Ingresar al dashboard sin haber elegido un rango
- **WHEN** la persona usuaria abre la página de Dashboard por primera vez
- **THEN** el sistema muestra únicamente los controles de fecha desde, fecha hasta y modo de aplicación, sin totales ni gráficos

#### Scenario: Falta una de las fechas
- **WHEN** la persona usuaria intenta aplicar el rango habiendo completado solo la fecha desde o solo la fecha hasta
- **THEN** el sistema rechaza la operación y no calcula el dashboard

#### Scenario: Fecha desde posterior a fecha hasta
- **WHEN** la persona usuaria aplica un rango donde la fecha desde es posterior a la fecha hasta
- **THEN** el sistema rechaza la operación y no calcula el dashboard

#### Scenario: Aplicar un rango válido
- **WHEN** la persona usuaria completa fecha desde, fecha hasta (con desde &lt;= hasta) y un modo, y confirma
- **THEN** el sistema calcula y muestra el contenido del dashboard para las licencias que cumplen ese rango según el modo elegido

### Requirement: Reajuste del rango sin salir de la página
El sistema SHALL permitir modificar la fecha desde, la fecha hasta o el modo, y volver a aplicar el rango, sin necesidad de navegar a otra página ni recargar la aplicación.

#### Scenario: Cambiar el rango después de ver el dashboard
- **WHEN** la persona usuaria ya visualizó el dashboard para un rango y modifica la fecha hasta o el modo, y vuelve a aplicar
- **THEN** el sistema recalcula y reemplaza todo el contenido mostrado con los datos del nuevo rango

### Requirement: Resumen de volumen
Una vez aplicado un rango válido, el sistema SHALL mostrar el total de licencias del rango, el total de días de reposo sumando los días completos de cada licencia (`CantidadDias`), y el promedio de días de reposo por licencia.

#### Scenario: Rango con licencias
- **WHEN** el rango aplicado incluye licencias
- **THEN** el sistema muestra el total de licencias, el total de días de reposo y el promedio de días, calculados sobre esas licencias

#### Scenario: Licencia que se solapa parcialmente con el rango en modo intersección
- **WHEN** el modo aplicado es intersección de rangos y una licencia incluida empieza antes de la fecha desde o termina después de la fecha hasta
- **THEN** el total de días de reposo suma la cantidad de días completa de esa licencia, sin recortarla a los días que caen dentro del rango consultado

#### Scenario: Rango sin licencias
- **WHEN** el rango aplicado no tiene ninguna licencia asociada
- **THEN** el sistema muestra el total de licencias en cero y no muestra un promedio de días indefinido (por ejemplo, división por cero)

### Requirement: Distribución por tipo de licencia y por unidad
El sistema SHALL mostrar, para el rango aplicado, la cantidad de licencias agrupada por tipo de licencia y la cantidad de licencias agrupada por unidad, cada una ordenada de mayor a menor cantidad.

#### Scenario: Varios tipos y unidades en el rango
- **WHEN** el rango aplicado incluye licencias de más de un tipo y de más de una unidad
- **THEN** el sistema muestra ambos rankings ordenados de mayor a menor cantidad de licencias

### Requirement: Distribución entre ingreso manual e ingreso por parser automático
El sistema SHALL mostrar, para el rango aplicado, qué proporción de las licencias fue ingresada manualmente frente a las leídas por el parser automático de PDF.

#### Scenario: Rango con ambos tipos de ingreso
- **WHEN** el rango aplicado incluye licencias ingresadas manualmente y licencias leídas por el parser automático
- **THEN** el sistema muestra la proporción o cantidad de cada una de las dos formas de ingreso

### Requirement: Tendencia temporal con granularidad automática
El sistema SHALL mostrar la cantidad de licencias del rango agrupadas en el tiempo según la fecha que corresponda al modo aplicado, eligiendo automáticamente la granularidad según el largo del rango: por día si el rango es de 60 días o menos, por semana si es de más de 60 días y hasta 365 días, y por mes si es de más de 365 días.

#### Scenario: Rango corto agrupa por día
- **WHEN** el rango aplicado abarca 60 días o menos
- **THEN** la tendencia temporal agrupa la cantidad de licencias por día

#### Scenario: Rango medio agrupa por semana
- **WHEN** el rango aplicado abarca más de 60 días y hasta 365 días
- **THEN** la tendencia temporal agrupa la cantidad de licencias por semana

#### Scenario: Rango largo agrupa por mes
- **WHEN** el rango aplicado abarca más de 365 días
- **THEN** la tendencia temporal agrupa la cantidad de licencias por mes
