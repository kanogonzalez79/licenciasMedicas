# configuracion-smtp Specification

## Purpose
Permite configurar y verificar el servidor SMTP que la aplicación usará para enviar directamente los correos de aviso de licencias médicas a las unidades.

## Requirements

### Requirement: Guardar configuración SMTP
El sistema SHALL permitir definir y guardar una configuración de servidor SMTP con host, puerto, usuario, contraseña, correo remitente e indicación de si la conexión usa SSL/TLS.

#### Scenario: Guardar configuración válida
- **WHEN** la persona usuaria completa host, puerto, usuario, contraseña y remitente, y guarda
- **THEN** el sistema almacena la configuración y la usa para los envíos de correo posteriores

#### Scenario: Campos obligatorios faltantes
- **WHEN** se intenta guardar la configuración sin host, puerto, usuario, contraseña o remitente
- **THEN** el sistema rechaza el guardado sin modificar la configuración existente

### Requirement: Contraseña no expuesta en texto plano
El sistema SHALL almacenar la contraseña SMTP cifrada en disco y SHALL no devolverla en texto plano en ninguna respuesta de la API.

#### Scenario: Consultar configuración guardada
- **WHEN** se solicita la configuración SMTP actual
- **THEN** el sistema retorna host, puerto, usuario, remitente y uso de TLS, pero no la contraseña en texto plano

### Requirement: Probar conexión
El sistema SHALL permitir probar la conexión y autenticación con los datos de servidor SMTP ingresados, sin exigir que ya estén guardados, e informar si la prueba fue exitosa o el motivo del error.

#### Scenario: Prueba exitosa
- **WHEN** se prueban credenciales válidas de un servidor SMTP alcanzable
- **THEN** el sistema informa que la conexión y autenticación fueron exitosas

#### Scenario: Prueba fallida
- **WHEN** se prueban credenciales inválidas o un servidor inalcanzable
- **THEN** el sistema informa que la prueba falló junto con un mensaje descriptivo del error, sin guardar esos datos como configuración vigente

### Requirement: Configuración como precondición de envío
El sistema SHALL exigir que exista una configuración SMTP guardada antes de permitir enviar cualquier correo; si no existe, SHALL rechazar el envío indicando que debe configurarse el servidor SMTP primero.

#### Scenario: Intento de envío sin configuración
- **WHEN** no hay ninguna configuración SMTP guardada y se intenta enviar un correo
- **THEN** el sistema rechaza el envío e indica que debe configurarse el servidor SMTP antes de poder enviar
