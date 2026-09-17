## Why

Hoy "Redactar correo" solo genera el texto del aviso; la persona usuaria tiene que copiarlo y pegarlo a mano en su cliente de correo para enviarlo a la unidad. Se quiere eliminar ese paso manual: enviar el correo directamente desde la app con un clic, usando el `CorreoElectronico` que ya tiene cada unidad.

## What Changes

- Se agrega una pantalla nueva **Configuración** para definir el servidor SMTP de salida: host, puerto, usuario, contraseña (cifrada en disco con DPAPI, atada a la cuenta de Windows), remitente y uso de SSL/TLS, con una acción "Probar conexión" antes de guardar.
- En **Redactar correo**, el texto generado deja de ser de solo lectura: se puede editar antes de enviarlo.
- Se agrega el botón **Enviar correo** junto al "Copiar" existente. Al presionarlo se pide confirmación (destinatario y cantidad de licencias) y, al confirmar, el correo se envía por SMTP directamente desde la app, sin adjuntos.
- Se registra la fecha y hora del último envío exitoso por unidad + fecha. Si se vuelve a intentar enviar el mismo unidad + fecha, la confirmación avisa que ya se había enviado (mostrando cuándo) pero permite reenviar igual.
- Si la unidad no tiene un correo electrónico válido (caso hoy inalcanzable desde la UI, ya que `unidades` lo exige, pero posible por datos preexistentes), el botón "Enviar correo" se deshabilita con un aviso que redirige a cargarlo en Unidades.

## Capabilities

### New Capabilities
- `configuracion-smtp`: configuración del servidor SMTP usado para enviar correos (host, puerto, credenciales cifradas, remitente, TLS) y verificación de esa configuración mediante una prueba de conexión.
- `enviar-correo`: redacción editable y envío directo por SMTP del aviso de licencias de una unidad en una fecha dada, con confirmación previa y registro de los envíos realizados.

### Modified Capabilities
(ninguna — `unidades` ya exige correo con formato válido al crear/editar y no cambia su comportamiento; este cambio solo consume esa garantía)

## Impact

- **Backend**: nueva dependencia MailKit para el envío SMTP; nueva migración SQL embebida (tabla de configuración SMTP de fila única + tabla de registro de envíos por unidad/fecha); cifrado de la contraseña SMTP con `System.Security.Cryptography.ProtectedData` (DPAPI); nuevo servicio de envío en `LicenciasMedicas.Core.Correos`; nuevos endpoints `POST /api/correos/enviar` y de configuración (`GET/PUT /api/configuracion/smtp`, `POST /api/configuracion/smtp/probar`).
- **Frontend**: nueva página `ConfiguracionPage.tsx` con su ruta y entrada de menú; cambios en `RedactarCorreoPage.tsx` (textarea editable, botón "Enviar correo", diálogo de confirmación, aviso de reenvío y de unidad sin correo); nuevos métodos en `lib/api.ts`.
- **Datos**: nuevas tablas vía migración; no afecta el esquema de `Licencias` ni `Unidades`.
