# Informe Excel Licencias Specification

## Purpose

Permite a la persona usuaria generar y descargar un archivo Excel con el listado de licencias de un rango de fechas, eligiendo si ese rango aplica a la fecha de inicio, a la fecha de término, o si se deben incluir las licencias cuyo período se solapa con el rango consultado.

## Requirements

### Requirement: Parámetros del informe
El sistema SHALL pedir una fecha desde, una fecha hasta, y un modo de fecha (fecha de inicio, fecha de término, o intersección de rangos) para generar el informe. Las dos fechas SHALL ser obligatorias.

#### Scenario: Falta alguna de las fechas
- **WHEN** la persona usuaria intenta generar el informe sin haber ingresado la fecha desde o la fecha hasta
- **THEN** el sistema no genera el archivo y muestra un mensaje indicando que ambas fechas son obligatorias

#### Scenario: Fecha desde posterior a fecha hasta
- **WHEN** la persona usuaria ingresa una fecha desde posterior a la fecha hasta
- **THEN** el sistema no genera el archivo y muestra un mensaje indicando que el rango de fechas es inválido

### Requirement: Modo "fecha de inicio"
Cuando el modo elegido es "fecha de inicio", el sistema SHALL incluir en el informe las licencias cuya fecha de inicio de reposo esté dentro del rango consultado (ambos extremos inclusive).

#### Scenario: Licencia con inicio dentro del rango
- **WHEN** se genera el informe en modo "fecha de inicio" para un rango, y existe una licencia cuya fecha de inicio de reposo cae dentro de ese rango
- **THEN** esa licencia aparece en el informe

#### Scenario: Licencia con inicio fuera del rango
- **WHEN** se genera el informe en modo "fecha de inicio" para un rango, y existe una licencia cuya fecha de inicio de reposo cae fuera de ese rango (aunque su fecha de término sí esté dentro del rango)
- **THEN** esa licencia no aparece en el informe

### Requirement: Modo "fecha de término"
Cuando el modo elegido es "fecha de término", el sistema SHALL incluir en el informe las licencias cuya fecha de término de reposo esté dentro del rango consultado (ambos extremos inclusive).

#### Scenario: Licencia con término dentro del rango
- **WHEN** se genera el informe en modo "fecha de término" para un rango, y existe una licencia cuya fecha de término de reposo cae dentro de ese rango
- **THEN** esa licencia aparece en el informe

#### Scenario: Licencia con término fuera del rango
- **WHEN** se genera el informe en modo "fecha de término" para un rango, y existe una licencia cuya fecha de término de reposo cae fuera de ese rango (aunque su fecha de inicio sí esté dentro del rango)
- **THEN** esa licencia no aparece en el informe

### Requirement: Modo "intersección"
Cuando el modo elegido es "intersección", el sistema SHALL incluir en el informe toda licencia cuyo período de reposo (fecha de inicio a fecha de término) se solape en al menos un día con el rango consultado, usando la prueba de solapamiento de intervalos: la licencia califica si su fecha de inicio de reposo es menor o igual a la fecha hasta consultada, y su fecha de término de reposo es mayor o igual a la fecha desde consultada.

#### Scenario: Licencia que cubre todo el rango consultado
- **WHEN** se genera el informe en modo "intersección" para un rango, y existe una licencia cuyo período de reposo comienza antes del rango consultado y termina después de él (lo cubre por completo)
- **THEN** esa licencia aparece en el informe, aunque ni su fecha de inicio ni su fecha de término estén dentro del rango consultado

#### Scenario: Licencia que se solapa parcialmente al inicio o al final del rango
- **WHEN** se genera el informe en modo "intersección" para un rango, y existe una licencia cuyo período de reposo se solapa parcialmente con el rango consultado (comparte al menos un día)
- **THEN** esa licencia aparece en el informe

#### Scenario: Licencia sin ningún día en común con el rango
- **WHEN** se genera el informe en modo "intersección" para un rango, y existe una licencia cuyo período de reposo termina antes de que comience el rango consultado, o comienza después de que termina el rango consultado
- **THEN** esa licencia no aparece en el informe

### Requirement: Columnas del informe
El sistema SHALL generar el informe como un archivo Excel (`.xlsx`) con una fila de encabezado y, para cada licencia que califique, una fila con las columnas, en este orden: RUT del paciente sin dígito verificador, dígito verificador, nombre completo del paciente, número de folio, descripción del tipo de licencia, fecha de inicio de reposo, fecha de término de reposo, fecha de otorgamiento, y número de días.

#### Scenario: Licencia con fecha de otorgamiento no registrada
- **WHEN** una licencia que califica para el informe no tiene registrada su fecha de otorgamiento
- **THEN** la fila de esa licencia en el informe muestra esa columna vacía, sin inventar un valor

### Requirement: Informe sin resultados
El sistema SHALL generar igualmente el archivo Excel, con solo la fila de encabezado, cuando ninguna licencia califica para el rango y modo consultados.

#### Scenario: Rango sin licencias
- **WHEN** la persona usuaria genera el informe para un rango y modo en el que ninguna licencia califica
- **THEN** el sistema entrega un archivo Excel con la fila de encabezado y sin filas de datos, en vez de mostrar un error
