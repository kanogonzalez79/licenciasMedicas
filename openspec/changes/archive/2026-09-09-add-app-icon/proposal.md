## Why

The published `.exe` uses the default .NET icon and the web favicon (`public/favicon.svg`) is a generic template asset (purple abstract shape) left over from scaffolding. Neither has any visual relationship to "Licencias Médicas," and the favicon's purple doesn't even match the app's own primary blue (`#1d4ed8`/`#3b82f6`, defined in `index.css`). Both should show a purpose-built icon that reads as "medical document" at a glance.

## What Changes

- Design one master SVG icon: a document silhouette (folded top-right corner, white fill, subtle gray outline/shadow for visibility on light backgrounds) with a solid red medical cross centered on it and light gray lines suggesting text below the cross. Classic red/white medical palette (deliberately not the app's brand blue, favoring instant "medical" recognizability over brand consistency).
- Replace `src/LicenciasMedicas.Web.Client/public/favicon.svg` with the new design. No `.ico` fallback is added — the favicon stays a single SVG file, same as today.
- Generate a multi-resolution `.ico` (16/32/48/256) from the same master design for the Windows executable, add it to `src/LicenciasMedicas.Web/`, and declare `<ApplicationIcon>` in `LicenciasMedicas.Web.csproj` so `dotnet publish` embeds it in the single-file exe.

## Capabilities

No spec-level behavior changes — this is a visual asset swap with no effect on application requirements or behavior. `skip_specs: true` is set in `.openspec.yaml`.

## Impact

- `src/LicenciasMedicas.Web.Client/public/favicon.svg` — replaced.
- `src/LicenciasMedicas.Web/LicenciasMedicas.Web.csproj` — adds `<ApplicationIcon>`.
- `src/LicenciasMedicas.Web/` — new `.ico` file added.
- No API, data, or runtime behavior changes.
