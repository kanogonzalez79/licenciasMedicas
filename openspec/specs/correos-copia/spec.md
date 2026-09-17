# correos-copia Specification

## Purpose
Permite configurar, de forma independiente al servidor SMTP, una lista de direcciones de correo que se agregan automáticamente en copia (CC) a todo aviso de licencias enviado por correo, sin importar la unidad destinataria.

## Requirements

### Requirement: Guardar correos en copia
El sistema SHALL permitir agregar una dirección de correo electrónico a la lista de correos en copia, validando que tenga un formato válido y que no exista ya registrada en la lista, esté activa o inactiva.

#### Scenario: Agregar correo válido y nuevo
- **WHEN** se agrega una dirección con formato válido que no existe en la lista
- **THEN** el sistema la guarda como correo en copia activo

#### Scenario: Formato inválido
- **WHEN** se intenta agregar una dirección con formato inválido
- **THEN** el sistema rechaza el alta sin modificar la lista

#### Scenario: Correo ya existente
- **WHEN** se intenta agregar una dirección que ya existe en la lista, esté activa o inactiva
- **THEN** el sistema rechaza el alta indicando que ya existe, sin crear un registro duplicado ni reactivar el existente automáticamente

### Requirement: Activar o desactivar un correo en copia
El sistema SHALL permitir activar o desactivar individualmente cada correo en copia registrado, sin eliminarlo de la lista.

#### Scenario: Desactivar un correo
- **WHEN** se desactiva un correo en copia activo
- **THEN** el sistema deja de incluirlo en los envíos posteriores, pero lo conserva en la lista

#### Scenario: Reactivar un correo
- **WHEN** se activa un correo en copia que estaba desactivado
- **THEN** el sistema vuelve a incluirlo en los envíos posteriores

### Requirement: Eliminar un correo en copia
El sistema SHALL permitir eliminar definitivamente un correo en copia de la lista.

#### Scenario: Eliminar un correo
- **WHEN** se elimina un correo en copia de la lista
- **THEN** el sistema deja de incluirlo en los envíos posteriores y permite volver a agregar esa misma dirección como un registro nuevo

### Requirement: Consultar la lista de correos en copia
El sistema SHALL permitir consultar todos los correos en copia registrados junto con su estado activo o inactivo.

#### Scenario: Consultar la lista
- **WHEN** se solicita la lista de correos en copia
- **THEN** el sistema retorna cada correo registrado junto con su estado activo o inactivo
