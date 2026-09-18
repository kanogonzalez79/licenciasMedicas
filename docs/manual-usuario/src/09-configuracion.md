---
grupo: Sistema
capabilities_openspec:
  - configuracion-smtp
  - correos-copia
  - diagnostico-smtp
---

# Configuración

Reúne dos configuraciones independientes que usa la pantalla [Redactar correo](#sec-04-redactar-correo) al enviar avisos: el servidor de correo saliente y la lista de correos en copia.

## Servidor SMTP

Los datos del servidor de correo que el sistema usa para enviar los avisos directamente desde "Redactar correo": Host, Puerto, Usuario, Contraseña, Correo remitente y si la conexión usa SSL/TLS.

![Tarjeta de configuración del servidor SMTP, con los botones Probar conexión y Guardar](01-smtp.png)

- **Probar conexión** — verifica que los datos ingresados permitan conectarse y autenticarse con el servidor, sin necesidad de guardarlos primero. Informa si la prueba fue exitosa o el motivo del error.
- **Guardar** — almacena la configuración de forma cifrada. Por seguridad, la contraseña guardada **nunca se muestra** al volver a abrir esta pantalla: si ya existe una configuración guardada y se quiere cambiar cualquier otro dato, hay que volver a escribir la contraseña para poder guardar.

::: importante
Mientras no haya una configuración SMTP guardada, el sistema no permite enviar ningún correo desde "Redactar correo" — lo indicará pidiendo configurar el servidor primero.
:::

## Correos en copia (CC)

Una lista de direcciones que reciben copia (CC) de **todo** aviso de licencias que se envíe, sin importar la unidad destinataria — por ejemplo, la jefatura de personal. Es independiente de la configuración del servidor SMTP.

![Tabla de correos en copia, con el campo para agregar uno nuevo](02-correos-copia.png)

Se agrega una dirección con el campo y el botón **Agregar** (debe tener formato válido y no estar ya registrada). Cada correo de la lista se puede **activar o desactivar** sin eliminarlo (un correo desactivado deja de recibir copias, pero queda guardado para reactivarlo después) o **eliminar** definitivamente.

::: nota
Para diagnosticar problemas de envío de correo (por ejemplo, fallos intermitentes de conexión), el sistema deja un registro de texto plano con cada intento de conexión al servidor SMTP — ver el [Anexo: soporte y solución de problemas](#sec-11-anexo-soporte).
:::
