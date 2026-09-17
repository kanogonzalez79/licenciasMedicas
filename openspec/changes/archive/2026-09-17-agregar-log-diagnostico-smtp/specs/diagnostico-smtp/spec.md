## Purpose

Deja un rastro persistente en un archivo de texto de cada intento de conexión al servidor SMTP (prueba de configuración o envío real de un correo), para poder diagnosticar errores intermitentes o recurrentes después de que ocurrieron.

## ADDED Requirements

### Requirement: Registro de cada intento de conexión SMTP
El sistema SHALL registrar en un archivo de texto cada intento de conexión al servidor SMTP, ya sea al probar una configuración o al enviar un correo real, incluyendo fecha y hora, tipo de operación, host, puerto, usuario y si la conexión usa SSL/TLS, junto con si el intento fue exitoso o falló.

#### Scenario: Prueba de conexión exitosa
- **WHEN** se prueba una configuración SMTP y la conexión y autenticación se completan correctamente
- **THEN** el sistema agrega una línea al registro indicando que la prueba fue exitosa, con host, puerto, usuario y uso de SSL

#### Scenario: Prueba de conexión fallida
- **WHEN** se prueba una configuración SMTP y la conexión o autenticación fallan
- **THEN** el sistema agrega una línea al registro indicando que la prueba falló, con host, puerto, usuario, uso de SSL y el motivo del error

#### Scenario: Envío de correo exitoso
- **WHEN** se envía un correo y la conexión, autenticación y envío al servidor SMTP se completan correctamente
- **THEN** el sistema agrega una línea al registro indicando que el envío fue exitoso, con host, puerto, usuario y uso de SSL

#### Scenario: Envío de correo fallido por error del servidor SMTP
- **WHEN** se intenta enviar un correo y falla la conexión, autenticación o envío al servidor SMTP
- **THEN** el sistema agrega una línea al registro indicando que el envío falló, con host, puerto, usuario, uso de SSL y el motivo del error

### Requirement: Detalle de la causa raíz sin exponer credenciales
El sistema SHALL incluir en el registro de un intento fallido el tipo y mensaje de la causa más profunda del error (siguiendo la cadena de excepciones internas cuando exista una), y SHALL no registrar en ningún caso la contraseña SMTP, ni cifrada ni en texto plano.

#### Scenario: Error con causa interna anidada
- **WHEN** un intento de conexión SMTP falla con una excepción que envuelve otra excepción interna con la causa real
- **THEN** el registro incluye el tipo y mensaje de la excepción interna más profunda, no solo el de la excepción externa

#### Scenario: La contraseña nunca queda registrada
- **WHEN** se registra cualquier intento de conexión SMTP, exitoso o fallido
- **THEN** el registro no contiene la contraseña SMTP en ninguna forma

### Requirement: Persistencia del registro entre reinicios
El sistema SHALL guardar el registro en un archivo dentro de la carpeta de datos de la aplicación (relativa al directorio del ejecutable) y SHALL conservar las líneas ya escritas al agregar nuevos intentos, sin sobrescribir el historial previo.

#### Scenario: Reinicio de la aplicación conserva el historial
- **WHEN** la aplicación se reinicia y luego se realiza un nuevo intento de conexión SMTP
- **THEN** el registro conserva las líneas escritas antes del reinicio y agrega la nueva línea a continuación
