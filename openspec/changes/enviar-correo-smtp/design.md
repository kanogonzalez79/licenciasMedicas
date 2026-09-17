## Context

Ver `proposal.md` - Why/What Changes para la motivación. Puntos de partida relevantes del código actual:

- `CorreoRedaccionService.Redactar` ya arma el texto del correo; `CorreosEndpoints` expone `GET /api/correos/unidades` y `GET /api/correos/redactar`. No hay dependencia de envío de correo en la solución hoy (ni MailKit ni `System.Net.Mail` en uso).
- `Unidad.CorreoElectronico` ya existe y `UnidadesEndpoints` ya exige formato válido al crear/editar (capability `unidades`), así que en la práctica toda unidad alcanzable desde la UI ya tiene un correo válido; el caso "unidad sin correo" es defensivo, no un flujo esperado.
- `AppPaths` resuelve todas las rutas de datos relativas al `.exe` (nunca `%AppData%`); la config SMTP nueva debe vivir en `data/licencias.db` igual que el resto de los datos, no en un archivo aparte.
- El proyecto Web ya es `net8.0-windows` con `UseWindowsForms` habilitado (para el diálogo nativo de "Buscar carpeta" de `respaldo-datos`), así que usar una API Windows-only para cifrar la contraseña SMTP no reduce la compatibilidad real de la app.
- `RespaldoService` copia `licencias.db` tal cual a la carpeta de respaldo elegida por la persona usuaria - la configuración SMTP cifrada viajará dentro de ese archivo sin cambios en `RespaldoService`.

## Goals / Non-Goals

**Goals:**
- Enviar el correo por SMTP directamente desde el backend, sin abrir un cliente de correo externo.
- Guardar la contraseña SMTP de forma que no quede legible como texto plano en `licencias.db` ni en respuestas de la API.
- Reusar el `CorreoElectronico` ya validado de `Unidad` como destinatario, sin pedir un correo nuevo en el flujo de envío.

**Non-Goals:**
- Adjuntar los PDF archivados al correo (decidido explícitamente fuera de alcance).
- Soportar múltiples configuraciones/cuentas SMTP o múltiples remitentes - una sola configuración vigente para toda la app.
- Historial completo de envíos (solo se guarda el último envío exitoso por unidad + fecha, ver spec `enviar-correo`).
- Soportar proveedores que exigen OAuth2 (Gmail/Office 365 modernos sin "contraseña de aplicación") - se asume SMTP + usuario/contraseña clásico, como un relay SMTP corporativo o una cuenta con contraseña de aplicación.

## Decisions

### Librería de envío: MailKit
Se agrega **MailKit** como dependencia de `LicenciasMedicas.Core`. `System.Net.Mail.SmtpClient` (el único cliente SMTP en el BCL) está marcado como no recomendado por Microsoft para código nuevo y tiene soporte débil de TLS moderno; MailKit es el reemplazo estándar en el ecosistema .NET y ya trae soporte de prueba de conexión (`ImapClient`/`SmtpClient.Connect` + `Authenticate`) que se reutiliza tanto para "Probar conexión" como para el envío real.

### Cifrado de la contraseña: DPAPI (`ProtectedData`, scope `CurrentUser`)
Se cifra la contraseña con `System.Security.Cryptography.ProtectedData.Protect`, scope `CurrentUser`, antes de guardarla en la tabla `ConfiguracionSmtp`; se descifra solo en memoria al armar el `SmtpClient`. Alternativas consideradas:
- **Texto plano**: más simple, pero deja una contraseña de correo real legible directamente en `licencias.db` (que además se respalda con `respaldo-datos`); no se justifica el ahorro dado que DPAPI es una API estándar de Windows, sin dependencias nuevas.
- **Gestor de secretos externo / vault**: sobredimensionado para una herramienta de escritorio de un solo usuario sin red.

DPAPI con scope `CurrentUser` es coherente con que la app ya es Windows-only (`UseWindowsForms`) y de un solo usuario.

### Modelo de datos: dos tablas nuevas, sin historial completo
- `ConfiguracionSmtp`: fila única (`Id` fijo, o `WHERE` sin filtro ya que solo existe una fila), con `Host`, `Puerto`, `Usuario`, `ContrasenaCifrada`, `Remitente`, `UsaSsl`. Guardar se implementa como upsert (borrar+insertar o `UPDATE`/`INSERT` condicional).
- `CorreosEnviados`: `UnidadId`, `Fecha`, `FechaHoraEnvio`, con índice único `(UnidadId, Fecha)`. Cada envío exitoso hace upsert sobre esa clave (`INSERT ... ON CONFLICT (UnidadId, Fecha) DO UPDATE SET FechaHoraEnvio = ...`), así siempre se lee el último envío sin agregaciones. Se descarta una tabla de historial completo porque la spec solo pide saber si ya se envió y cuándo fue la última vez, no un log de todos los intentos.
- Migración nueva `002_correo_smtp.sql` (embebida, igual que `001_init.sql`), aplicada automáticamente por `Migrator.Migrar` al iniciar.

### Exposición del "último envío" en el frontend
`GET /api/correos/unidades` (que ya devuelve `UnidadConLicenciasEnFecha` por fecha) se extiende con un campo `UltimoEnvio: string | null` (fecha/hora ISO del último envío exitoso para esa unidad + fecha, si existe), resuelto con un `LEFT JOIN` a `CorreosEnviados` en la misma consulta. Se prefiere esto a un endpoint separado de "estado de envío" porque el frontend ya carga esa lista antes de que la persona usuaria elija una unidad, y evita un round-trip adicional justo antes de abrir el diálogo de confirmación.

### Backend: servicio y endpoints nuevos
- `ConfiguracionSmtpService` (namespace `LicenciasMedicas.Core.Correos`): `Obtener()`, `Guardar(...)`, `ProbarConexion(...)`.
- `CorreoEnvioService`: arma el `MimeMessage` a partir del texto ya redactado/editado, obtiene el destinatario desde `Unidad.CorreoElectronico`, envía vía MailKit usando `ConfiguracionSmtpService.Obtener()`, y registra el envío en `CorreosEnviados` solo si el envío fue exitoso.
- `ConfiguracionEndpoints` nuevo: `GET /api/configuracion/smtp`, `PUT /api/configuracion/smtp`, `POST /api/configuracion/smtp/probar`.
- `CorreosEndpoints`: se agrega `POST /api/correos/enviar` (body: `unidadId`, `fecha`, `texto`); `GET /api/correos/redactar` se mantiene sin cambios para poblar el textarea inicial.

### Frontend: pantalla nueva + cambios en Redactar correo
- `ConfiguracionPage.tsx` nueva, con su ruta y entrada de menú (mismo patrón que las páginas existentes: `Card` + `Input`/`Label` + `Button`), formulario de host/puerto/usuario/contraseña/remitente/SSL, botón "Probar conexión" (muestra resultado con `toast`) y botón "Guardar".
- `RedactarCorreoPage.tsx`: el `Textarea` deja el prop `readOnly`; se agrega botón "Enviar correo" junto a "Copiar", que abre un `AlertDialog` (mismo componente que ya usa `BuscarPage` para confirmar eliminación) mostrando destinatario, cantidad de licencias y, si `UltimoEnvio` no es null, un aviso de reenvío. Confirmar dispara `POST /api/correos/enviar`.

## Risks / Trade-offs

- **[Riesgo] DPAPI `CurrentUser` es específico de la cuenta de Windows que corre la app.** Si se restaura un respaldo de `licencias.db` en otra cuenta de Windows u otro computador, la contraseña cifrada no se puede descifrar ahí. → Mitigación: no es un caso soportado hoy por `respaldo-datos` en general (ya asume mismo tipo de entorno); se documenta que tras restaurar un respaldo en otra cuenta/máquina hay que volver a ingresar la contraseña SMTP en Configuración. No se agrega manejo especial de error más allá de que "Probar conexión"/envío fallen con un mensaje claro si la contraseña no puede descifrarse.
- **[Riesgo] Proveedores de correo modernos (Gmail, Outlook.com/Office 365 con MFA) rechazan usuario+contraseña SMTP clásico.** → Mitigación: fuera de alcance resolver OAuth2; se documenta en la pantalla de Configuración que hay que usar una "contraseña de aplicación" si el proveedor la exige, y "Probar conexión" da el error concreto del servidor para diagnosticarlo.
- **[Trade-off] Sin historial completo de envíos**, solo el último por unidad+fecha. Si se necesita auditoría más fina a futuro (quién/cuándo se reenvió varias veces), habría que migrar `CorreosEnviados` a una tabla de eventos - se acepta el trade-off porque hoy no hay ese requisito.

## Migration Plan

1. Agregar dependencia MailKit al `.csproj` de `LicenciasMedicas.Core`.
2. Migración `002_correo_smtp.sql` con `ConfiguracionSmtp` y `CorreosEnviados` (aditiva, no toca tablas existentes) - se aplica sola al iniciar la app, igual que hoy.
3. Backend: servicios + endpoints nuevos, extensión de `UnidadConLicenciasEnFecha`/consulta de `LicenciasRepository` para incluir `UltimoEnvio`.
4. Frontend: `ConfiguracionPage` + ruta/menú, cambios en `RedactarCorreoPage`.
5. Sin flag de features ni rollback especial: es una capability aditiva (no reemplaza nada existente); si algo falla, revertir el commit/los endpoints nuevos no afecta el resto de la app porque `GET /api/correos/redactar` y el botón "Copiar" siguen intactos.
