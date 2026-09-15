## Why

El frontend actual es HTML/CSS/JS vanilla sin build step, con lógica duplicada en cada página (`mostrarMensaje`, `escaparHtml`, manejo de errores por try/catch repetido) y una estética utilitaria de intranet 2015. Se busca modernizar la apariencia a algo profesional y, de paso, eliminar esa duplicación adoptando React + Vite + shadcn/ui, sin perder la propiedad clave de la app actual: un único `.exe` autocontenido (herramienta local, loopback-only, sin deploy tradicional).

## What Changes

- Nuevo proyecto hermano `src/LicenciasMedicas.Web.Client` (Vite + React + TypeScript + shadcn/ui) que reemplaza las 5 páginas HTML estáticas y los `.js` de `wwwroot` por una SPA con React Router.
- Estructura plana: `pages/` con un archivo por pantalla (`InicioPage`, `UnidadesPage`, `BuscarPage`, `IngresarPage`, `RedactarCorreoPage`), sin capas `features/`, `hooks/` ni `types/` separadas — hooks de datos (TanStack Query) y tipos colocados junto a donde se usan, igual que el backend colocalize sus DTOs en los archivos de endpoint.
- `lib/api.ts` reemplaza `wwwroot/js/api.js` como capa de acceso a la API existente (mismas rutas `/api/*`, sin cambios de contrato).
- Manejo de errores centralizado vía `onError` global de TanStack Query + toast (shadcn `Sonner`), reemplazando el patrón manual de `mostrarMensaje`/`div#mensaje` repetido por página.
- Pipeline de build: `Web.csproj` gana un target de MSBuild que corre `npm ci && npm run build` sobre `Web.Client` y embebe su `dist/` como `EmbeddedResource` (vía `Link`, o como fallback `outDir` directo a `wwwroot`) — sin tocar `Program.cs` (sigue usando `ManifestEmbeddedFileProvider`). El target solo corre en `publish`, no en cada build de desarrollo.
- Flujo de desarrollo pasa a dos procesos: `dotnet run` (puerto fijo en dev) + `vite dev` con proxy de `/api/*` al backend; el puerto efímero actual se conserva para producción.
- Paleta de `site.css` portada 1:1 a tokens de shadcn (`--background`, `--primary`, `--destructive`, `--muted-foreground`, `--sidebar*`, etc.), agregando `--success`/`--success-foreground` (sin equivalente de fábrica en shadcn) y unificando el `--radius` (hoy inconsistente entre 4/6/8px).
- Tipografía: se reemplaza el system font stack por Inter auto-hospedada (`@fontsource/inter`, sin requests externos en runtime — coherente con que la app puede correr sin conectividad).
- Nomenclatura de páginas/rutas: se mantiene la convención ya implícita hoy — sustantivo para pantallas de catálogo/CRUD (`unidades`), verbo para pantallas de tarea/flujo (`buscar`, `ingresar`, `redactar-correo`) — y se adopta kebab-case en las rutas (`/redactar-correo` en vez de `/redactarCorreo`).
- **BREAKING**: se eliminan `wwwroot/*.html`, `wwwroot/css/site.css` y `wwwroot/js/*.js` del proyecto `LicenciasMedicas.Web`, reemplazados por el build embebido de `Web.Client`. No hay cambios en los endpoints de la API (`/api/unidades`, `/api/licencias`, `/api/procesamiento`, `/api/correos` se mantienen igual).

## Capabilities

### New Capabilities
- `theme-toggle`: permite al usuario alternar entre apariencia clara y oscura, con la preferencia persistida entre sesiones.

### Modified Capabilities
<!-- Sin cambios de requerimientos: el comportamiento funcional de unidades/buscar/ingresar/redactar-correo se preserva, solo cambia la implementación (vanilla JS -> React). -->

## Impact

- **Código afectado**: todo `src/LicenciasMedicas.Web/wwwroot/**` (eliminado); nuevo `src/LicenciasMedicas.Web.Client/**`; `src/LicenciasMedicas.Web/LicenciasMedicas.Web.csproj` (nuevo target de build); `LicenciasMedicas.sln` (nuevo proyecto).
- **Sin cambios**: `Program.cs` (sigue sirviendo `wwwroot` embebido igual que hoy), `LicenciasMedicas.Core` y todos los endpoints de `/api/*`.
- **Dependencias nuevas**: primera dependencia de Node.js/npm en el repo (solo en tiempo de build/desarrollo, no en runtime del `.exe` publicado) — requiere `package-lock.json` y Node instalado en la máquina que compile/publique.
- **Flujo de trabajo**: desarrollo local pasa de "un solo `dotnet run`" a dos procesos en paralelo (backend + `vite dev`) durante la iteración de UI.
