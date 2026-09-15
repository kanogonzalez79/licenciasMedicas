# Contexto del Proyecto

## Descripción General
- Tipo: Aplicación web (backend ASP.NET Core + frontend SPA en React), empaquetada y usada como herramienta de escritorio de un solo usuario en Windows (se publica como un único .exe self-contained, escucha solo en loopback 127.0.0.1, sin exposición en red).
- Stack: Backend en C# / .NET 8 (ASP.NET Core Minimal APIs) + Dapper. Frontend en React 19 + TypeScript + Vite + Tailwind CSS 4 + shadcn/ui + TanStack Query + React Router + next-themes (dark mode) + sonner (toasts).
- Base de datos: SQLite (Microsoft.Data.Sqlite), archivo local `data/licencias.db`. Migraciones SQL embebidas en `src/LicenciasMedicas.Core/Data/Migrations/*.sql`, se aplican automáticamente al iniciar la app (`Migrator.Migrar`).

## Comandos del Sistema
- Servidor de desarrollo (backend): `ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/LicenciasMedicas.Web` — escucha en `http://127.0.0.1:5244` (puerto fijo solo en dev, para que el proxy de Vite lo encuentre).
- Servidor de desarrollo (frontend): `cd src/LicenciasMedicas.Web.Client && npm run dev` (Vite; puerto 5173 por defecto, hace proxy de `/api/*` al backend).
- Construcción (Build): `dotnet build` (solución completa). En Release, el target `BuildClientAssets` compila el cliente React y lo embebe en `wwwroot`.
- Publicación: `dotnet publish -c Release --project src/LicenciasMedicas.Web` — genera un único `.exe` self-contained win-x64 (PublishSingleFile), sin DLLs sueltas.
- Pruebas (Test): `dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj`
- Lint frontend: `cd src/LicenciasMedicas.Web.Client && npm run lint` (oxlint)

## Reglas de Desarrollo
- Es una herramienta local de un solo usuario: en producción escucha solo en loopback con puerto efímero; nunca exponerla en la red ni agregar autenticación multiusuario sin que se pida explícitamente.
- Todas las rutas de datos se resuelven en relación al directorio del ejecutable (`AppPaths`), nunca rutas fijas de usuario ni `%AppData%`, para que la carpeta del proyecto se pueda mover a otro computador sin romper nada.
- El flujo central es Procesar -> staging -> Grabar -> archivado permanente: los PDF leídos se mueven a staging al "Procesar" (para no reprocesarlos) y solo salen definitivamente de "incoming" al confirmar "Grabar". Los documentos archivados quedan en `data/archivo/{año}/{mes}/` nombrados como `{Nombre Paciente en Título Case} - {Folio}{extensión}`.
- El dominio y el código (clases, métodos, variables, comentarios) están en español; mantener esa convención en el backend.
- No sugerir librerías de escritorio nativo (Electron, Tauri, WPF/WinForms para UI) — la UI es siempre la SPA de React servida por el backend; la única dependencia de WinForms existente es puntual para el diálogo nativo "Buscar carpeta" del respaldo.
- Cambios de comportamiento (specs) se gestionan con OpenSpec (`openspec/`) — usar `/opsx:explore`, `/opsx:propose`, `/opsx:apply`, `/opsx:archive` para trabajo no trivial en vez de improvisar directamente en el código.
- [Agrega aquí cualquier otra regla de código que uses siempre]

## Formas de Testear
- Tests unitarios/integración: `dotnet test` sobre `LicenciasMedicas.Core.Tests` — cada test usa un `AppPaths` apuntando a un directorio temporal propio, no toca datos reales.
- Prueba manual end-to-end: levantar backend (`dotnet run --project src/LicenciasMedicas.Web`) + frontend (`npm run dev` en `Web.Client`), y usar la UI en el navegador, o pegarle directo a los endpoints `/api/...` con `curl`.
- No se necesita túnel (Cloudflare, ngrok, etc.): la app corre 100% local en loopback, no hay nada que exponer a internet para probarla.
- Fixtures de PDFs reales para pruebas: `tests/LicenciasMedicas.Core.Tests/Fixtures/*.pdf` y `_ejemplos/*.pdf`.
- Al probar manualmente contra el entorno de desarrollo, limpiar los datos de prueba después (eliminar licencias desde la UI o `DELETE /api/licencias/{id}`) para no ensuciar `data/licencias.db` del dev.

## Mapa de Módulos del Backend (`src/LicenciasMedicas.Core`)
- `Catalogos`: catálogos de referencia, ej. `CatalogoTipoLicencia` (descripciones de los códigos de tipo de licencia).
- `Correos`: `CorreoRedaccionService` — redacta el texto de aviso para las unidades con licencias grabadas en un día.
- `Data`: `SqliteConnectionFactory`, `Migrator` y las migraciones SQL embebidas (`Data/Migrations/*.sql`).
- `Licencias`: entidad `Licencia`, `LicenciasRepository`, `IngresoManualService` (ingreso manual con adjunto).
- `Parsing`: `LicenciaExtractionService` y los extractores por tipo de PDF (tipo1/tipo2), basados en PdfPig.
- `Paths`: `AppPaths` — resuelve todas las rutas de datos relativas al ejecutable (incoming, staging, archivo, logs, db).
- `Procesamiento`: `ProcesamientoService` — orquesta el flujo Procesar -> staging -> Grabar -> archivado permanente, y `StagingRepository`/`LicenciaRevision`.
- `Respaldo`: `RespaldoService` — copia el respaldo de datos a una carpeta elegida por la persona usuaria.
- `Rut`: `RutUtils`/`RutValido` — parseo y validación de RUT chileno (dígito verificador).
- `Unidades`: `UnidadesRepository` — CRUD de las unidades organizacionales a las que se asignan las licencias.

En `src/LicenciasMedicas.Web`: `Endpoints/` contiene los grupos de minimal API (`/api/licencias`, `/api/procesamiento`, `/api/unidades`, `/api/correos`, `/api/respaldo`); `wwwroot/` recibe el cliente React embebido solo en builds Release. El cliente vive en `src/LicenciasMedicas.Web.Client` (pantallas: Unidades, Buscar licencia, Ingresar licencias, Redactar correo, Respaldo).

## Glosario de Dominio
- **Licencia médica**: documento que certifica el reposo médico de un paciente; se ingresa por PDF (parser automático) o de forma manual con un formulario + adjunto.
- **Folio**: identificador único de la licencia en todo el sistema — nunca se permite un folio duplicado, ni siquiera entre lo ya grabado y lo pendiente en staging.
- **Unidad**: unidad organizacional (con correo de contacto) a la que se asigna una licencia para su gestión y aviso.
- **Tipo de formulario**: 1 o 2 según la plantilla de PDF que reconoce el parser automático, 3 = ingreso manual.
- **Incoming**: carpeta donde se depositan los PDF nuevos a procesar (`data/incoming`).
- **Staging**: carpeta temporal donde quedan los PDF ya leídos pero pendientes de confirmar con "Grabar" (`data/staging`), agrupados por lote.
- **Archivo**: destino final de los documentos ya grabados, organizado por año/mes (`data/archivo/{año}/{mes}/`), nombrados como `{Paciente en Título Case} - {Folio}{extensión}`.
- **Revisión (`LicenciaRevision`)**: fila pendiente de confirmación tras "Procesar", antes de "Grabar" (o descartar).
- **Lote**: agrupación de los PDF leídos juntos en una misma pasada de "Procesar".

## Especificación del Comportamiento (OpenSpec)
- `openspec/specs/` es la fuente de verdad de las capabilities ya implementadas (hoy: `archivo-documento-licencia`, `detalle-licencia`, `eliminar-licencia`, `ingresar-licencia-manual`, `respaldo-datos`, `theme-toggle`, `unidades`). Antes de modificar una funcionalidad existente, revisar su spec ahí.
- Para trabajo no trivial, usar el flujo `/opsx:explore` -> `/opsx:propose` (o `explore` + captura manual) -> `/opsx:apply` -> `/opsx:archive`, en vez de improvisar directo en el código.

## Datos Sensibles
- El sistema maneja datos médicos y personales de pacientes (RUT, nombre completo, tipo de licencia, fechas y motivo de reposo) — son datos sensibles.
- No registrar estos datos en logs compartidos ni subir PDFs, dumps de `licencias.db` o capturas de pantalla con datos reales a servicios externos (pastebins, gists, chats).
- Al armar ejemplos, pruebas o capturas para compartir, preferir datos ficticios en vez de los de un paciente real.

## Control de Versiones
- Esta carpeta **no es un repositorio git** actualmente (`git status` falla con "not a git repository"). No asumir que se pueden hacer commits, branches o PRs hasta que se inicialice uno explícitamente.

## Gotchas Conocidos
- **Tildes/ñ por línea de comandos en Git Bash (Windows)**: se corrompen (mojibake) al pasarlas como argumento literal a comandos como `curl`. Si hay que probar un flujo con texto acentuado por `curl`, escribir el valor en un archivo UTF-8 y pasarlo con `-F "campo=<archivo.txt"` en vez de como argumento directo.
- **`.NET` `TextInfo.ToTitleCase` con palabras en MAYÚSCULAS**: no las modifica (las trata como sigla); hay que aplicar `.ToLower()` antes. Ver `AppPaths.NormalizarNombreTituloCase` como referencia ya resuelta en este proyecto.
- **Puerto de Vite ocupado**: si un `npm run dev` anterior quedó colgado (el proceso hijo de `vite` a veces sobrevive a `TaskStop` del wrapper `npm`), Vite arranca en 5174 o el siguiente puerto libre en vez de 5173 — revisar el log de arranque para confirmar el puerto real antes de navegar.
