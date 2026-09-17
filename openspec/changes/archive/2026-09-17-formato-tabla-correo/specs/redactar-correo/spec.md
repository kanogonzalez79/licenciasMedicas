## Purpose

Genera el texto de correo de aviso (saludo + tabla de licencias médicas) para una unidad y fecha determinadas, dejándolo listo para editar, copiar o enviar.

## ADDED Requirements

### Requirement: Texto de aviso por unidad y fecha
El sistema SHALL generar, para una unidad y fecha dadas, un texto compuesto por un saludo (desde una plantilla configurable, con el nombre de la unidad interpolado) seguido de una tabla en texto plano con las licencias médicas de esa unidad grabadas en esa fecha, con las columnas RUT, Nombre, Fecha Inicio, Fecha Término y Cantidad de Días.

#### Scenario: Unidad sin licencias grabadas en la fecha
- **WHEN** se solicita el texto de aviso para una unidad y fecha sin licencias grabadas
- **THEN** el sistema no genera texto para esa combinación

#### Scenario: Unidad con licencias grabadas en la fecha
- **WHEN** se solicita el texto de aviso para una unidad y fecha con licencias grabadas
- **THEN** el sistema genera el saludo seguido de una fila de encabezado y una fila por cada licencia, con los valores de RUT, Nombre, Fecha Inicio, Fecha Término y Cantidad de Días de esa licencia

### Requirement: Tabla de licencias alineada en columnas de ancho fijo
El sistema SHALL alinear las columnas de la tabla de licencias usando un ancho fijo por columna — calculado a partir del valor más largo de esa columna entre el encabezado y todas las filas incluidas — separando cada columna de la siguiente con un espacio de separación fijo, de modo que la tabla se lea alineada en un visor de texto con fuente monoespaciada, sin depender de tabulaciones.

#### Scenario: Valores de largo variable en una misma columna
- **WHEN** el texto de aviso incluye licencias cuyos nombres de paciente tienen largos distintos
- **THEN** todas las columnas de todas las filas, incluido el encabezado, quedan alineadas entre sí usando ese ancho fijo por columna

### Requirement: Línea separadora bajo el encabezado
El sistema SHALL incluir, inmediatamente bajo la fila de encabezado de la tabla, una línea compuesta por guiones del mismo ancho que cada columna (con el mismo separador entre columnas), antes de las filas de licencias.

#### Scenario: Tabla con licencias
- **WHEN** se genera el texto de aviso para una unidad y fecha con licencias grabadas
- **THEN** la línea que sigue inmediatamente al encabezado está compuesta solo por guiones y espacios separadores, con el mismo ancho de columna que el resto de la tabla
