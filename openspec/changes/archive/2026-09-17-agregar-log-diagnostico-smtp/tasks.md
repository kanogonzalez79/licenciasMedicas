## 1. Registro de diagnóstico SMTP

- [x] 1.1 Crear `SmtpDiagnosticoLog` en `LicenciasMedicas.Core/Correos/`, recibiendo `AppPaths` e inyectable como singleton, que agregue una línea de texto a `logs/smtp.log` por cada intento (operación `prueba`/`envio`, fecha/hora, host, puerto, usuario, uso de SSL, y éxito o error). En error, debe bajar hasta la `InnerException` más profunda para reportar tipo y mensaje de la causa real, y nunca debe incluir la contraseña SMTP. Verificar con un test unitario (usando un `AppPaths` de directorio temporal) que las líneas de éxito y error tienen el formato esperado y que ninguna contiene la contraseña usada en la prueba.
- [x] 1.2 Registrar `SmtpDiagnosticoLog` como singleton en `src/LicenciasMedicas.Web/Program.cs` y verificar que la app sigue arrancando (`dotnet build`).

## 2. Integración con prueba de conexión

- [x] 2.1 Modificar `ConfiguracionSmtpService.ProbarConexion` para invocar `SmtpDiagnosticoLog` tanto en el camino exitoso como en el `catch`. Verificar con un test de integración que, tras una prueba fallida (host inválido o puerto cerrado), `logs/smtp.log` gana una línea de error con host/puerto/usuario y motivo, y que tras una prueba exitosa gana una línea de éxito.

## 3. Integración con envío real de correo

- [x] 3.1 Modificar `CorreoEnvioService.Enviar` para invocar `SmtpDiagnosticoLog` alrededor del bloque que hace `Connect`/`Authenticate`/`Send` (no alrededor de las validaciones previas de unidad o configuración faltante, que no son fallas de conexión). Verificar con un test que un envío fallido por error del servidor SMTP deja una línea de error en el log, y uno exitoso deja una línea de éxito, sin afectar el comportamiento ya cubierto por `EnvioCorreoFallidoException`.

## 4. Verificación manual

- [x] 4.1 Con el backend corriendo en desarrollo, configurar credenciales SMTP inválidas y usar "Probar conexión" en la UI; confirmar que `logs/smtp.log` registra el intento fallido con el detalle del error. Corregir las credenciales, volver a probar, y confirmar que se agrega una línea de éxito sin perder la línea anterior (el archivo se conserva entre intentos).
