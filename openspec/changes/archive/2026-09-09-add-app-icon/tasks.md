## 1. Master icon asset

- [x] 1.1 Author the master SVG icon (document with folded corner, centered red medical cross, gray text lines, subtle outline/shadow) and verify it renders correctly when opened directly in a browser
- [x] 1.2 Visually check the design scaled down to 16x16 (e.g. browser zoom or an image viewer) and simplify shapes if the cross or document outline become illegible at that size

## 2. Favicon

- [x] 2.1 Replace `src/LicenciasMedicas.Web.Client/public/favicon.svg` with the new master SVG and verify `npm run dev` shows the new icon in the browser tab
- [x] 2.2 Verify `npm run build` completes and the built `dist/favicon.svg` matches the new design

## 3. Exe icon

- [x] 3.1 Export the master SVG to PNGs at 16/32/48/256px and pack them into a single multi-resolution `.ico` file, placed under `src/LicenciasMedicas.Web/`
- [x] 3.2 Add `<ApplicationIcon>` to `src/LicenciasMedicas.Web/LicenciasMedicas.Web.csproj` pointing at the new `.ico`
- [x] 3.3 Run `dotnet publish -c Release` for `LicenciasMedicas.Web` and verify the produced `.exe` shows the new icon in Windows Explorer (16/32/48/256 sizes look correct, no blank/default icon)
