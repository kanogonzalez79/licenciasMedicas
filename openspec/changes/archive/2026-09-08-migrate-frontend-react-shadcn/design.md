## Context

La app es una herramienta local de un solo usuario: `Program.cs` liga a `http://127.0.0.1:0` (puerto efímero, solo loopback) y abre el navegador automáticamente al arrancar. No hay deploy tradicional ni CI (`.github` no existe); `dotnet publish` se corre a mano y produce un build framework-dependent (no self-contained, no single-file). Todo `wwwroot/**` se empaqueta hoy como `EmbeddedResource` y se sirve vía `ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot")`. Ver `proposal.md` - Why para la motivación del cambio.

El frontend actual son 5 páginas HTML (`index`, `unidades`, `buscar`, `ingresar`, `redactarCorreo`) con JS vanilla por página que repite un patrón idéntico: `escaparHtml()`, `mostrarMensaje()`, y try/catch alrededor de cada llamada a `Api.*` en `wwwroot/js/api.js`.

## Goals / Non-Goals

**Goals:**
- Reemplazar el frontend por React + Vite + shadcn/ui preservando el modelo de distribución de un solo `.exe` (sin Node en runtime).
- Eliminar la duplicación de manejo de mensajes/errores existente en cada página.
- Portar la paleta de colores actual al sistema de theming de shadcn en vez de adoptar su paleta por defecto.

**Non-Goals:**
- No se cambian los endpoints de la API (`/api/unidades`, `/api/licencias`, `/api/procesamiento`, `/api/correos`) ni `LicenciasMedicas.Core`.
- No se agrega autenticación, multi-usuario, ni se cambia el modelo loopback-only.
- No se arma CI ni pipeline automatizado de publish — sigue siendo manual, igual que hoy.

## Decisions

### Estructura de carpetas: plana, sin `features/`
Con solo 5 pantallas, se descartó una carpeta por feature con hooks colocados (considerada y descartada por indirección innecesaria: cada hook de datos se usa en un único lugar, igual que hoy cada función de `api.js` la llama una sola página). En su lugar:

```
LicenciasMedicas.Web.Client/src/
  main.tsx  App.tsx  app-sidebar.tsx  routes.tsx  index.css
  components/ui/        # primitivas shadcn generadas
  lib/api.ts            # fetch wrapper + tipos request/response colocados
  lib/query-client.ts    # onError global -> toast.error(err.message)
  lib/utils.ts
  pages/
    InicioPage.tsx  UnidadesPage.tsx  BuscarPage.tsx
    IngresarPage.tsx  RedactarCorreoPage.tsx
```

Cada página incluye sus propios `useQuery`/`useMutation` inline (sin archivo de hook separado) y sus tipos viven en `lib/api.ts`, no en una carpeta `types/` — mismo patrón que el backend, que colocaliza DTOs como `CrearUnidadRequest` directamente en el archivo de endpoint (`UnidadesEndpoints.cs`) en vez de una carpeta `Dtos/`.

`IngresarPage.tsx` es la pantalla con más lógica (tabla editable de pendientes con un `<Select>` de unidad por fila). El subcomponente de fila (`FilaPendiente`) se define localmente dentro de ese archivo; se extrae a un archivo propio solo si el archivo se vuelve incómodo de leer, no preventivamente.

### Nomenclatura de páginas y rutas
La API ya es sustantivo (`/api/licencias`, `/api/procesamiento`) por convención REST; la UI ya es verbo en el sidebar actual (`BUSCAR LICENCIA`, `INGRESAR LICENCIAS`, `REDACTAR CORREO`). Esa divergencia se mantiene a propósito: sirve audiencias distintas (API = recurso para quien toca el backend; UI = tarea para quien usa la herramienta). La regla implícita que ya sigue la app hoy y que se hace explícita para pantallas futuras: **sustantivo para catálogo/CRUD** (`Unidades`), **verbo para tarea/flujo** (`Buscar`, `Ingresar`, `Redactar correo`). Las rutas adoptan kebab-case (`/redactar-correo` en vez de `/redactarCorreo`) por ser la convención estándar de URLs; no hay costo por el cambio ya que nadie linkea esta app.

### Pipeline de build: proyecto Vite independiente + embedding vía MSBuild
`LicenciasMedicas.Web.Client` es un proyecto Vite autocontenido con su propio `dist/`, no escribe directamente dentro de `wwwroot/` de `Web` (mantiene cada proyecto independiente y compilable por separado). Un target de MSBuild en `Web.csproj`:
1. Corre `npm ci && npm run build` en `Web.Client` (gateado para no ejecutarse en cada `dotnet build`/F5 de Debug — solo en `publish`, para no romper el ciclo rápido de desarrollo).
2. Incorpora los archivos de `Web.Client/dist/**` como `EmbeddedResource` con un `Link` que los ubica bajo `wwwroot\`, de forma que el manifiesto generado los vea como si vivieran físicamente ahí.

**Actualización (tarea 6.1)**: el `Link` no puede armarse con `%(RecursiveDir)%(Filename)%(Extension)` como se planteó originalmente — esa metadata solo se calcula para `ItemGroup` estáticos evaluados al cargar el proyecto, no para items creados dentro de un `<Target>` (que es donde tiene que vivir, porque `dist/` no existe hasta que el target corre `npm run build`). Usarla ahí colapsa todos los archivos al mismo `Link`, y `GenerateEmbeddedResourcesManifest` falla. Se arma el `Link` con `$([System.IO.Path]::GetRelativePath(...))` sobre `%(FullPath)` en su lugar. El spike de la tarea 1.1 no detectó esto porque ahí el `ItemGroup` era estático, no dentro de un `Target`.

**Actualización (tarea 6.1)**: `Program.cs` sí cambia, a diferencia de lo planteado acá. Con `wwwroot` vacío en Debug (el cliente solo se embebe en Release), `ManifestEmbeddedFileProvider` explota al arrancar porque el manifiesto ni se genera sin `EmbeddedResource` items. La construcción de `ManifestEmbeddedFileProvider` y todo lo que depende de `wwwroot` (`UseDefaultFiles`, `UseStaticFiles`, `MapFallbackToFile`, el auto-open del navegador) ahora vive detrás de `if (!app.Environment.IsDevelopment())` — tiene sentido de todas formas, porque en dev el backend no necesita servir `wwwroot`: lo sirve `vite dev` por separado.

**Alternativa considerada**: que Vite escriba `build.outDir` directamente dentro de `Web/wwwroot/` (con `emptyOutDir: true`), dejando el glob `EmbeddedResource Include="wwwroot\**\*"` existente sin tocar. Es más simple pero acopla físicamente los dos proyectos (el build de `Web.Client` ensucia una carpeta de otro proyecto). Se prefiere `Link` por mantener cada proyecto autocontenido; ver Open Questions para el spike que decide entre ambas si `Link` no calza con `ManifestEmbeddedFileProvider`.

**Dev loop**: dos procesos en paralelo — `dotnet run` (puerto fijo en dev, distinto del puerto efímero de producción) + `vite dev` con `server.proxy` reenviando `/api/*` al backend. El comportamiento de puerto efímero en producción no se toca.

**Actualización (tarea 6.2)**: el puerto fijo de dev es `5244` (el mismo que ya traía `Properties/launchSettings.json` del scaffold original, que hasta ahora estaba siendo ignorado porque `Program.cs` llamaba `UseUrls` incondicionalmente). `Program.cs` elige el puerto según `builder.Environment.IsDevelopment()`.

### Theming: tokens portados + extensión `--success` + `--radius` unificado
La paleta de `site.css` (`--color-primario`, `--color-borde`, `--color-texto-suave`, etc., incluidos los colores hoy hardcodeados en `.sidebar`) se renombra 1:1 a las variables semánticas de shadcn (`--primary`, `--border`, `--muted-foreground`, `--sidebar`, `--sidebar-accent`, etc.) — es prácticamente un renombrado, la paleta en sí no cambia. Dos extensiones sobre el set de fábrica de shadcn:
- `--success`/`--success-foreground`: shadcn no incluye un semantic color de éxito de fábrica (solo `primary/secondary/destructive/muted/accent`); se agrega siguiendo el mismo patrón que `--destructive`, necesario para portar `badge-ok`/`mensaje.exito`.
- `--radius` único (del que derivan `sm/md/lg` por `calc()`), reemplazando la inconsistencia actual entre `.panel` (8px) y `.btn`/inputs (4-6px).

**Actualización (tarea 3.1)**: los tokens `--sidebar*` toman el mismo valor en `:root` y en `.dark` — el sidebar sigue siendo oscuro fijo en ambos modos, igual que en la app actual. El toggle de tema (capability `theme-toggle`) solo afecta el área de contenido (`--background`, `--card`, `--primary`, etc.), no la identidad visual del sidebar.

### Modo oscuro con toggle (capability `theme-toggle`)
Se agrega un bloque `.dark {}` con paleta propia (no una simple inversión — `--card`/`--sidebar` en oscuro necesitan grises con más luz que el fondo para dar sensación de elevación) y un `ThemeProvider` en `lib/` (context + `localStorage`), con un control en el pie del sidebar. Ver `specs/theme-toggle/spec.md` para el comportamiento exacto (persistencia, valor por defecto según preferencia del SO).

**Actualización (tarea 2.3)**: la inicialización de shadcn/ui trajo `next-themes` como dependencia automática. Pese a lo planteado originalmente aquí, no es una librería Next-específica — solo maneja la clase/atributo de tema, `localStorage` y la preferencia del sistema operativo del lado del cliente, y funciona igual en un proyecto Vite puro. Se usa `next-themes` para el `ThemeProvider` en vez de escribirlo a mano, ya que ya está instalado y hace exactamente lo que pedía este diseño.

### Tipografía: Inter auto-hospedada
Se reemplaza el system font stack (`"Segoe UI", Tahoma, Arial`) por Inter vía `@fontsource/inter`, que empaqueta los `.woff2` en el bundle de Vite en vez de depender de un CDN de Google Fonts en runtime — coherente con que la app puede correr sin conectividad (es loopback-only). Se incluyen solo los pesos usados (400/500/600) para no inflar el bundle.

## Risks / Trade-offs

- **[Riesgo] El wiring `Link`-based de `EmbeddedResource` desde `Web.Client/dist/` podría no producir el mismo manifiesto lógico que hoy genera `wwwroot/**` físico** → Mitigación: spike de ~15 minutos antes de construir el resto (ver Open Questions); si falla, fallback documentado a `outDir` directo.
- **[Riesgo] Primera dependencia de Node/npm en el repo** → sin CI que lo valide automáticamente, el build reproducible depende de que `package-lock.json` esté commiteado y de que quien publique tenga la versión de Node correcta instalada. Mitigación: fijar versión de Node (ej. `.nvmrc` o campo `engines` en `package.json`).
- **[Trade-off] Dos procesos en desarrollo en vez de uno** → más fricción de setup que hoy (un solo `dotnet run`), a cambio de HMR real durante la iteración de UI.

## Open Questions

- ~~¿El `Link="wwwroot\%(RecursiveDir)%(Filename)%(Extension)"` sobre archivos físicamente fuera de `wwwroot/` es reconocido correctamente por `ManifestEmbeddedFileProvider`?~~ **Resuelto (tarea 1.1)**: sí. Spike aislado bajo `wwwroot/spike/` confirmó que `index.html` y los assets del bundle de Vite se sirven con el content-type correcto vía `ManifestEmbeddedFileProvider`, sin interferir con el `wwwroot` existente. Se usa el mecanismo `Link` tal como estaba planteado, sin necesidad del fallback de `outDir` directo.
