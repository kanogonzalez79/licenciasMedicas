## 1. Datos

- [x] 1.1 Agregar migración embebida `002_correo_smtp.sql` con las tablas `ConfiguracionSmtp` (fila única: Host, Puerto, Usuario, ContrasenaCifrada, Remitente, UsaSsl) y `CorreosEnviados` (UnidadId, Fecha, FechaHoraEnvio, índice único `(UnidadId, Fecha)`), y verificar que `dotnet test tests/LicenciasMedicas.Core.Tests` sigue en verde con la migración aplicándose desde cero en un `AppPaths` temporal.
- [x] 1.2 Agregar el campo `UltimoEnvio` a `UnidadConLicenciasEnFecha` y extender la consulta de `LicenciasRepository.UnidadesConLicenciasGrabadasEnFecha` con un `LEFT JOIN` a `CorreosEnviados`, verificado con un test que graba un envío y comprueba que aparece en el resultado.

## 2. Backend - Configuración SMTP

- [x] 2.1 Agregar la dependencia MailKit a `LicenciasMedicas.Core.csproj` y verificar que `dotnet build` compila sin warnings nuevos.
- [x] 2.2 Implementar cifrado/descifrado de la contraseña SMTP con `ProtectedData` (scope `CurrentUser`) en un helper dedicado, con test unitario que cifra y descifra un valor de ejemplo.
- [x] 2.3 Implementar `ConfiguracionSmtpService` (`Obtener`, `Guardar`, `ProbarConexion`) en `LicenciasMedicas.Core.Correos`, con tests que cubran: guardar y volver a obtener (sin exponer la contraseña en texto plano), rechazo por campos obligatorios faltantes, y prueba de conexión fallida con mensaje de error.
- [x] 2.4 Exponer `ConfiguracionEndpoints` (`GET /api/configuracion/smtp`, `PUT /api/configuracion/smtp`, `POST /api/configuracion/smtp/probar`) y registrar el grupo en `Program.cs`, verificado con `curl` contra el backend en Development.

## 3. Backend - Envío de correo

- [x] 3.1 Implementar `CorreoEnvioService.Enviar(unidadId, fecha, texto)`: valida configuración SMTP existente, valida correo de la unidad, arma y envía el `MimeMessage` vía MailKit, y registra el envío (upsert en `CorreosEnviados`) solo si fue exitoso. Cubrir con tests: envío sin configuración SMTP guardada, unidad sin correo válido, y registro del envío tras éxito (usando un servidor SMTP falso/fake en el test, no uno real).
- [x] 3.2 Agregar `POST /api/correos/enviar` en `CorreosEndpoints` (body: unidadId, fecha, texto) que devuelve error claro en caso de falla de envío, verificado con `curl` contra el backend en Development.

## 4. Frontend - Configuración

- [x] 4.1 Agregar `correosApi`/`configuracionApi` en `lib/api.ts` con los métodos para obtener/guardar la configuración SMTP, probar conexión y enviar el correo.
- [x] 4.2 Crear `ConfiguracionPage.tsx` (formulario host/puerto/usuario/contraseña/remitente/SSL, botón "Probar conexión" con `toast` de resultado, botón "Guardar") y agregar su ruta + entrada de menú, verificado navegando la pantalla en el navegador con el backend + `npm run dev` levantados.

## 5. Frontend - Redactar correo

- [x] 5.1 Quitar `readOnly` del `Textarea` en `RedactarCorreoPage.tsx` y verificar manualmente que el texto se puede editar antes de copiar/enviar.
- [x] 5.2 Agregar botón "Enviar correo" junto a "Copiar", con `AlertDialog` de confirmación (destinatario, cantidad de licencias, y aviso de reenvío usando `UltimoEnvio` si no es null) y deshabilitado cuando la unidad no tiene correo válido, verificado manualmente: envío exitoso, envío con error del servidor SMTP, e intento de reenvío mostrando el aviso.

## 6. Verificación end-to-end

- [x] 6.1 `dotnet test tests/LicenciasMedicas.Core.Tests` en verde y `npm run lint` en `Web.Client` sin errores.
- [x] 6.2 Probar manualmente el flujo completo (Configuración → Redactar correo → Enviar) contra un servidor SMTP real o de pruebas (ej. Mailtrap/Papercut local), confirmando que el correo llega con el texto editado, y limpiar cualquier configuración/dato de prueba usado (según "Formas de Testear" del proyecto).
