## Purpose

Gestiona las unidades a las que pertenecen los pacientes de las licencias médicas, asegurando que cada unidad tenga una descripción única y un correo electrónico válido para su futuro uso en el módulo de correos.

## ADDED Requirements

### Requirement: Correo electrónico obligatorio al crear una unidad
El sistema SHALL exigir un correo electrónico con formato válido al crear una unidad. Una solicitud de creación sin correo electrónico, o con un correo electrónico que no tiene formato válido, SHALL ser rechazada sin crear la unidad.

#### Scenario: Creación con correo válido
- **WHEN** se crea una unidad con una descripción y un correo electrónico con formato válido
- **THEN** la unidad se crea y persiste con ese correo electrónico

#### Scenario: Creación sin correo electrónico
- **WHEN** se intenta crear una unidad sin ingresar un correo electrónico
- **THEN** la solicitud se rechaza con un error de validación y no se crea la unidad

#### Scenario: Creación con correo con formato inválido
- **WHEN** se intenta crear una unidad con un correo electrónico que no tiene formato de dirección de correo válida
- **THEN** la solicitud se rechaza con un error de validación y no se crea la unidad

### Requirement: Correo electrónico obligatorio al editar una unidad
El sistema SHALL exigir un correo electrónico con formato válido al editar una unidad, incluyendo unidades existentes que aún no tengan un correo electrónico guardado. Una solicitud de edición sin correo electrónico, o con un correo electrónico que no tiene formato válido, SHALL ser rechazada sin modificar la unidad.

#### Scenario: Edición con correo válido
- **WHEN** se edita una unidad existente ingresando un correo electrónico con formato válido
- **THEN** la unidad se actualiza y persiste con ese correo electrónico

#### Scenario: Edición sin correo electrónico
- **WHEN** se intenta guardar la edición de una unidad sin ingresar un correo electrónico
- **THEN** la solicitud se rechaza con un error de validación y la unidad no se modifica

#### Scenario: Edición de una unidad legacy sin correo previo
- **WHEN** se guarda la edición de una unidad que no tenía correo electrónico guardado, sin completar uno con formato válido
- **THEN** la solicitud se rechaza con un error de validación, igual que si fuera una unidad con correo previo

### Requirement: Formato válido de correo electrónico
El sistema SHALL considerar válido un correo electrónico cuando tiene la forma general `usuario@dominio.tld` (sin espacios en blanco, con un `@` y al menos un punto en la parte del dominio). El sistema SHALL recortar espacios en blanco al inicio y al final antes de evaluar el formato.

#### Scenario: Texto sin arroba
- **WHEN** el texto ingresado como correo electrónico no contiene el carácter `@`
- **THEN** se considera de formato inválido

#### Scenario: Dominio sin punto
- **WHEN** el texto ingresado como correo electrónico tiene un `@` pero la parte posterior no contiene ningún punto
- **THEN** se considera de formato inválido

#### Scenario: Correo con espacios alrededor
- **WHEN** el texto ingresado como correo electrónico tiene espacios en blanco al inicio o al final pero un correo válido en el medio
- **THEN** se considera de formato válido una vez recortados los espacios

### Requirement: Descripción de unidad obligatoria y única
El sistema SHALL exigir una descripción no vacía al crear o editar una unidad, y SHALL rechazar una descripción que ya esté en uso por otra unidad (comparación insensible a mayúsculas/minúsculas).

#### Scenario: Descripción vacía
- **WHEN** se intenta crear o editar una unidad sin ingresar una descripción
- **THEN** la solicitud se rechaza con un error de validación

#### Scenario: Descripción duplicada
- **WHEN** se intenta crear o editar una unidad con una descripción que ya usa otra unidad existente
- **THEN** la solicitud se rechaza informando que la descripción ya está en uso

### Requirement: Listado de unidades
El sistema SHALL permitir listar todas las unidades registradas, ordenadas por descripción, incluyendo su correo electrónico actual.

#### Scenario: Listado incluye unidades legacy sin correo
- **WHEN** se listan las unidades y existe una unidad creada antes de que el correo fuera obligatorio y aún no editada
- **THEN** esa unidad aparece en el listado con su correo electrónico vacío
