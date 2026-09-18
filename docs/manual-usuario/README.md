# Manual de Usuario — Sistema de Licencias Médicas

Fuente editable del manual de usuario en PDF (`dist/Manual-Usuario-LicenciasMedicas.pdf`). Este `README` explica cómo compilarlo y cómo actualizarlo cuando el sistema cambie.

## Estructura

```
src/         Contenido en Markdown, un archivo por sección (una capability/pantalla)
imagenes/    Capturas de pantalla, en subcarpetas con el mismo nombre que el .md
estilos/     Hoja de estilos del PDF (manual.css)
plantillas/  Shell HTML del documento + encabezado/pie de página
capturas/    Checklist para (re)tomar las capturas de pantalla
manual.config.mjs   Título, versión, formato de página
build.mjs            Script de compilación (Markdown + imágenes -> PDF)
dist/                 PDF final (se versiona) y HTML intermedio (no se versiona)
```

## Compilar el PDF

Requiere Node.js ≥20 y Google Chrome instalado en el equipo (usa el Chrome ya instalado del sistema vía `puppeteer-core`, no descarga uno nuevo).

```bash
cd docs/manual-usuario
npm install        # solo la primera vez, o cuando cambien las dependencias
npm run build       # genera dist/Manual-Usuario-LicenciasMedicas.pdf
npm run build:open  # además abre el PDF resultante
```

Si Chrome no está en la ruta estándar (`C:\Program Files\Google\Chrome\Application\chrome.exe`), definir la variable de entorno `CHROME_PATH` con la ruta correcta antes de compilar.

## Cómo actualizar el manual cuando el sistema cambia

1. **Ubicar la sección a tocar.** Cada archivo de `src/*.md` tiene en su frontmatter un campo `capabilities_openspec` que lista las capabilities de `openspec/specs/` que documenta. Si el cambio afecta una capability ya listada ahí, edita ese archivo. Si es una funcionalidad nueva sin sección propia, crea un `NN-nombre-seccion.md` nuevo siguiendo el mismo formato que los existentes (frontmatter `grupo` + `capabilities_openspec`, un `# Título` como primera línea, texto en lenguaje de usuario final).
2. **Redactar en prosa**, tomando como base el spec actualizado en `openspec/specs/{capability}/spec.md` (sus escenarios Given/When/Then) pero explicado como instrucciones de uso, no como spec literal.
3. **Recapturar solo las imágenes que cambiaron**, siguiendo `capturas/README-captura-datos-ficticios.md`. El nombre del archivo no cambia: basta con sobrescribir el PNG/JPG existente en `imagenes/{seccion}/`.
4. **Recompilar** con `npm run build`.
5. Revisar el PDF resultante (portada, índice con números de página correctos, secciones en el orden esperado) y luego commitear junto: el/los `.md` tocados, las imágenes nuevas o actualizadas, y `dist/Manual-Usuario-LicenciasMedicas.pdf` regenerado.

No hace falta tocar `build.mjs` ni `manual.config.mjs` salvo que cambie la estructura general (por ejemplo, un grupo nuevo en el menú lateral, junto a "Licencias" y "Sistema").

## Notas sobre el contenido de cada `.md`

- El **título de la sección** es el primer `# Encabezado` del archivo (no hay un campo de título separado en el frontmatter, para no duplicar la fuente de verdad).
- `grupo: Licencias` o `grupo: Sistema` en el frontmatter agrupa la fila correspondiente en el índice, igual que el menú lateral de la app (`app-sidebar.tsx`). Sin `grupo`, la sección aparece como ítem suelto (como "Introducción" o "Glosario").
- Los bloques `::: nota`, `::: importante` y `::: consejo` (y su cierre `:::`) se renderizan como recuadros de aviso.
- Las imágenes se referencian solo por su nombre de archivo (`![texto alternativo](archivo.png)`), sin repetir la carpeta de la sección — el build ya resuelve la ruta a `imagenes/{sección}/{archivo}` automáticamente. El texto alternativo se usa también como pie de foto.
- Los enlaces internos a otra sección usan el id `#sec-{nombre-del-archivo-sin-extensión}`, por ejemplo `[Buscar licencia](#sec-03-buscar-licencia)`.

## Cómo funciona la compilación (para referencia)

`build.mjs` renderiza el Markdown a HTML, arma portada + índice + contenido a partir de `plantillas/documento.html`, y usa Chrome (`puppeteer-core`) para imprimir a PDF. Como no hay forma directa de saber de antemano en qué página imprimirá cada sección, el build hace **dos pasadas**: la primera imprime el documento con un marcador invisible al inicio de cada sección y usa `pdf-parse` para detectar en qué página cayó cada uno; la segunda pasada arma el índice ya con esos números de página reales y genera el PDF final (con encabezado/pie de página, que sí trae el número de página exacto directamente de Chrome). El HTML intermedio de ambas pasadas queda en `dist/_build/` solo para depuración, y no se versiona.
