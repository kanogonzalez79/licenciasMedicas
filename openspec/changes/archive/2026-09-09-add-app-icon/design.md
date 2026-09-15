## Context

See proposal.md - Why. Two independent places need updating: the Vite/React client's SVG favicon (`public/favicon.svg`, referenced from `index.html` as `<link rel="icon" type="image/svg+xml">`) and the ASP.NET Core `Web` project's published single-file `.exe`, which has no `<ApplicationIcon>` set today. Windows executable icons must be `.ico` (multi-resolution); the favicon can stay pure SVG as it is today.

## Goals / Non-Goals

**Goals:**
- One master SVG design used as the source for both the favicon and the generated `.ico`, so the two stay visually identical.
- Icon reads clearly as "medical document" at both favicon size (16px tab) and exe icon sizes (16/32/48/256, viewed in Explorer/taskbar).

**Non-Goals:**
- No `favicon.ico` fallback, `apple-touch-icon`, or `site.webmanifest` — out of scope, matches today's single-file setup (decided during exploration).
- No rebrand of the app's primary blue color — the icon intentionally uses the classic red/white medical palette instead (decided during exploration).

## Decisions

- **Composition**: document silhouette with a folded top-right corner (white fill, subtle gray outline/drop-shadow so it stays visible on light backgrounds), a solid red medical cross centered in the upper portion, and 2-3 light gray horizontal lines below suggesting body text. Chosen over a rounded-square "app badge" or a cross-dominant/document-as-outline treatment (both considered and rejected in favor of this one for reading more clearly as a specific document type rather than a generic health app).
- **Color**: classic medical red (`#e02424`-range red, exact hex to be picked during asset creation) and white, not the app's brand blue (`#1d4ed8`/`#3b82f6`). Deliberate: recognizability as "medical" outranks brand consistency for an icon that lives outside the app UI.
- **Single master source**: author one SVG (e.g. `assets/app-icon.svg` at repo root, or alongside the client's `public/` - exact location decided during implementation) and derive both outputs from it:
  - Favicon: the master SVG copied/adapted to `src/LicenciasMedicas.Web.Client/public/favicon.svg`.
  - Exe icon: raster the master SVG at 16/32/48/256px and pack into a single `.ico` (e.g. via ImageMagick or an equivalent conversion tool available in the dev environment), placed in `src/LicenciasMedicas.Web/` and wired via `<ApplicationIcon>` in `LicenciasMedicas.Web.csproj`.
- **No fallback formats**: favicon stays single-file SVG, matching current scope exactly (see Non-Goals).

## Risks / Trade-offs

- [Red/white palette clashes with the app's blue UI] → Accepted trade-off, explicitly chosen by the user for recognizability; no mitigation needed.
- [Raster export of an SVG at 16px can lose detail (thin cross arms, doubled outline)] → Verify the `.ico`'s 16px frame visually after generation; simplify/thicken shapes at small sizes if needed rather than reusing the exact same paths as the 256px frame.
- [`<ApplicationIcon>` requires a valid Windows `.ico`, not a renamed PNG] → Use a real ICO-packing step (e.g. ImageMagick `convert`) rather than a file extension rename.

## Migration Plan

Straightforward asset swap, no data or runtime migration. Rollback is reverting the favicon file and removing the `<ApplicationIcon>` property/`.ico` file.
