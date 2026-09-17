## 1. Datos

- [x] 1.1 Crear `src/LicenciasMedicas.Core/Data/Migrations/003_correo_copia.sql` con la tabla `CorreoCopia` (`CorreoCopiaId`, `CorreoElectronico`, `Activo`) y el índice único `UX_CorreoCopia_CorreoElectronico` sobre `LOWER(CorreoElectronico)`, y verificar que `dotnet test` sigue en verde (la migración se aplica automáticamente en cada test que llama `Migrator.Migrar`).

## 2. Backend - Servicio de correos en copia

- [x] 2.1 Crear `src/LicenciasMedicas.Core/Correos/CorreoCopia.cs` con el modelo `CorreoCopia` (Id, CorreoElectronico, Activo) y las excepciones `CorreoCopiaInvalidoException` (formato inválido) y `CorreoCopiaDuplicadoException` (correo ya existente, activo o inactivo).
- [x] 2.2 Crear `src/LicenciasMedicas.Core/Correos/CorreoCopiaService.cs` con `Listar()`, `Agregar(string correo)`, `CambiarActivo(int id, bool activo)` y `Eliminar(int id)`, usando `EmailUtils.EsFormatoValido` para el formato y una búsqueda case-insensitive previa al INSERT para detectar duplicados (activos o inactivos) y lanzar `CorreoCopiaDuplicadoException` en ese caso.
- [x] 2.3 Agregar `internal ListarActivos()` (o reutilizar `Listar()` filtrando) para que `CorreoEnvioService` obtenga solo los correos activos.
- [x] 2.4 Escribir `tests/LicenciasMedicas.Core.Tests/CorreoCopiaServiceTests.cs` cubriendo: agregar correo válido, rechazo por formato inválido, rechazo por duplicado (tanto contra uno activo como contra uno inactivo, sin reactivarlo), desactivar/activar sin perder el registro, y eliminar (permitiendo volver a agregar la misma dirección después). Verificar con `dotnet test`.

## 3. Backend - Integración con el envío

- [x] 3.1 Modificar `src/LicenciasMedicas.Core/Correos/CorreoEnvioService.cs` para recibir `CorreoCopiaService` por constructor y, después de `mensaje.To.Add(...)`, agregar un `mensaje.Cc.Add(...)` por cada correo activo devuelto por `ListarActivos()`.
- [x] 3.2 Actualizar `tests/LicenciasMedicas.Core.Tests/CorreoEnvioServiceTests.cs` (ajustar el constructor del servicio en el `Setup`) y agregar un test que, con uno o más correos en copia activos configurados, verifique que `servidor.UltimoMensajeRecibido` contiene esas direcciones en el encabezado `Cc`, y otro que confirme que sin correos en copia el mensaje no incluye encabezado `Cc`. Verificar con `dotnet test`.
- [x] 3.3 Registrar `CorreoCopiaService` como singleton en `src/LicenciasMedicas.Web/Program.cs`, junto a `ConfiguracionSmtpService` y `CorreoEnvioService`, y verificar que `dotnet build` compila sin errores.

## 4. Backend - Endpoints

- [x] 4.1 Crear `src/LicenciasMedicas.Web/Endpoints/CorreosCopiaEndpoints.cs` con `MapGroup("/api/configuracion/correos-copia")`: `GET /` (listar), `POST /` (agregar, body `{ correoElectronico }`), `PATCH /{id}` (body `{ activo }`), `DELETE /{id}`; mapear las excepciones de `CorreoCopiaService` a `400 Bad Request` con `{ error }`.
- [x] 4.2 Registrar `app.MapCorreosCopiaEndpoints()` en `src/LicenciasMedicas.Web/Program.cs`, junto a `MapConfiguracionEndpoints()`.
- [x] 4.3 Verificar manualmente con `curl` contra el backend en dev (`dotnet run --project src/LicenciasMedicas.Web`): agregar un correo, listar, desactivar, reintentar agregar el mismo (debe rechazarse), eliminar.

## 5. Frontend

- [x] 5.1 Agregar en `src/LicenciasMedicas.Web.Client/src/lib/api.ts` el tipo `CorreoCopia` y `correosCopiaApi` (`listar`, `agregar`, `cambiarActivo`, `eliminar`) contra `/api/configuracion/correos-copia`.
- [x] 5.2 Agregar en `ConfiguracionPage.tsx` una nueva `Card` "Correos en copia (CC)", independiente de la card de SMTP existente: input + botón para agregar, y una tabla (correo, switch o checkbox de activo/inactivo, botón eliminar) siguiendo el patrón de lista de `UnidadesPage.tsx`; mostrar el error del backend (correo duplicado o formato inválido) con `toast.error`.
- [x] 5.3 Probar manualmente en el navegador (`npm run dev` + backend en dev): agregar un correo válido, agregar un formato inválido (debe rechazarse), agregar un duplicado activo y uno inactivo (ambos deben rechazarse), desactivar y reactivar, eliminar y volver a agregar la misma dirección.

## 6. Verificación end-to-end

- [x] 6.1 Con backend + frontend en dev, configurar SMTP (o usar el servidor falso de pruebas si aplica) y uno o más correos en copia activos, enviar un aviso desde "Redactar correo" para una unidad con licencias grabadas, y confirmar que los correos en copia reciben el mensaje además de la unidad. Limpiar después los datos de prueba (licencias, correos en copia) según las reglas del proyecto.
