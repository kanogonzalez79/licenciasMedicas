## Why

Hoy el envío directo de correos de aviso (capability `enviar-correo`) solo admite el correo de la unidad como destinatario. Hay direcciones (por ejemplo jefaturas o buzones de control) que deben quedar en copia en **todos** los avisos que se envíen, sin importar la unidad, y hoy no hay forma de configurar eso.

## What Changes

- Se agrega una configuración independiente de "Correos en copia (CC)": una lista de direcciones de correo, cada una con estado activo/inactivo, gestionable desde la pantalla de Configuración (separada de la configuración del servidor SMTP).
- Al agregar un correo se valida su formato y que no exista ya entre los registrados (activo o inactivo); si ya existe, se rechaza el alta y se indica que debe activarse el existente en vez de duplicarlo.
- El envío directo de correo (`enviar-correo`) agrega automáticamente todos los correos de copia activos como CC en cada correo enviado, además del destinatario de la unidad.
- No afecta la redacción del texto del aviso (`redactar-correo`) ni ningún otro flujo de envío: solo aplica al envío SMTP de la capability `enviar-correo`.

## Capabilities

### New Capabilities
- `correos-copia`: gestión de la lista de direcciones de correo en copia (CC) global — agregar, validar duplicados, activar/desactivar.

### Modified Capabilities
- `enviar-correo`: el envío directo de correo agrega en copia (CC) todos los correos de copia activos configurados, además del destinatario de la unidad.

## Impact

- Backend: nueva tabla de datos para los correos de copia (migración SQL nueva), nuevo servicio de gestión y nuevos endpoints en `Endpoints/`; `CorreoEnvioService.Enviar` se modifica para agregar los CC al armar el `MimeMessage`.
- Frontend: nueva sección en `ConfiguracionPage.tsx` (independiente de la card de SMTP) para listar, agregar, activar/desactivar y eliminar correos de copia.
- No hay cambios de esquema en `ConfiguracionSmtp` ni en `Unidades`.
