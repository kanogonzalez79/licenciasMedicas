## Why

Hoy, cuando falla una prueba de conexión SMTP o el envío real de un correo, el único rastro del error es un toast en el navegador (`ex.Message`) que desaparece al cerrarse — no incluye la excepción interna (que suele traer la causa real: DNS, timeout, credenciales, TLS) y no queda ningún historial para detectar si el problema es puntual o recurrente. Se necesita un registro en un archivo de texto que persista cada intento de conexión SMTP para poder diagnosticar errores después del hecho.

## What Changes

- Nuevo componente `SmtpDiagnosticoLog` (en `LicenciasMedicas.Core.Correos`) que escribe una línea de texto por intento en `logs/smtp.log` (dentro de `AppPaths.LogsDir`, ya existente pero sin uso hoy).
- Cada línea incluye fecha/hora, operación (`prueba` o `envio`), host, puerto, usuario y uso de SSL — nunca la contraseña.
- En éxito, registra que la conexión/autenticación (y el envío, si aplica) se completó.
- En error, registra el tipo de excepción y su mensaje, bajando hasta la excepción interna más profunda (donde MailKit suele exponer la causa real), sin stack trace completo.
- Se conecta en los dos puntos que abren una conexión SMTP real: `ConfiguracionSmtpService.ProbarConexion` y la parte SMTP de `CorreoEnvioService.Enviar` (no en validaciones previas como "unidad sin correo válido", que no son fallas de conexión).
- Archivo de texto plano, sin rotación ni límite de tamaño (volumen esperado: bajo, uso manual de un solo usuario) y sin vista en la UI — se abre directamente desde `logs/smtp.log` cuando haga falta diagnosticar.

## Capabilities

### New Capabilities
- `diagnostico-smtp`: registro en archivo de texto de cada intento de conexión SMTP (prueba de configuración o envío real de correo), con su resultado y el detalle del error cuando falla.

### Modified Capabilities
(ninguna — no cambia el comportamiento observable de `configuracion-smtp` ni `enviar-correo` hacia la persona usuaria, solo agrega un registro interno adicional)

## Impact

- `src/LicenciasMedicas.Core/Correos/`: nuevo archivo `SmtpDiagnosticoLog.cs`.
- `src/LicenciasMedicas.Core/Correos/ConfiguracionSmtpService.cs`: `ProbarConexion` invoca el log de diagnóstico.
- `src/LicenciasMedicas.Core/Correos/CorreoEnvioService.cs`: `Enviar` invoca el log de diagnóstico alrededor de la conexión/envío SMTP.
- `src/LicenciasMedicas.Web/Program.cs`: registro del nuevo servicio en el contenedor de DI.
- Sin cambios de API pública, sin nuevas dependencias externas, sin cambios de esquema de base de datos.
