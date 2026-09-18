// Compila docs/manual-usuario/src/*.md + imagenes/ -> dist/Manual-Usuario-LicenciasMedicas.pdf
//
// Dos pasadas de impresión:
//   1) "medición": arma el HTML con un marcador invisible al inicio de cada sección,
//      lo imprime a PDF sin encabezado/pie, y busca en qué página cayó cada marcador
//      (extrayendo el texto de cada página del PDF con pdf-parse). Esto da el número
//      de página real de cada entrada del índice sin adivinarlo por geometría CSS.
//   2) "final": arma el mismo HTML pero sin marcadores y con el índice ya completo,
//      y lo imprime con encabezado/pie (numeración real que entrega Chrome). Ese es
//      el PDF que se entrega.
//
// Ver README.md de esta carpeta para el flujo de actualización del contenido.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

import matter from "gray-matter";
import MarkdownIt from "markdown-it";
import anchor from "markdown-it-anchor";
import container from "markdown-it-container";
import pdfParse from "pdf-parse";
import puppeteer from "puppeteer-core";

import config from "./manual.config.mjs";

const rootDir = path.dirname(fileURLToPath(import.meta.url));
const srcDir = path.join(rootDir, "src");
const imagenesDir = path.join(rootDir, "imagenes");
const estilosDir = path.join(rootDir, "estilos");
const plantillasDir = path.join(rootDir, "plantillas");
const distDir = path.join(rootDir, "dist");
const buildDir = path.join(distDir, "_build");

const CALLOUTS = [
  ["nota", "Nota"],
  ["importante", "Importante"],
  ["consejo", "Consejo"],
];

function escapeHtml(texto) {
  return String(texto)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");
}

// ---------------------------------------------------------------------------
// 1. Leer y renderizar las secciones fuente (docs/manual-usuario/src/*.md)
// ---------------------------------------------------------------------------

let seccionActual = null;

const md = new MarkdownIt({ html: false, linkify: true, typographer: true });
md.use(anchor, { level: [2, 3] });
for (const [nombre, etiqueta] of CALLOUTS) {
  md.use(container, nombre, {
    render(tokens, idx) {
      return tokens[idx].nesting === 1
        ? `<div class="callout callout-${nombre}"><p class="callout-titulo">${etiqueta}</p>\n`
        : "</div>\n";
    },
  });
}
// Cada `![texto](archivo.png)` en una sección se busca en imagenes/{slug-de-la-sección}/
// y queda envuelto en una <figure> con el alt-text como pie de foto.
md.renderer.rules.image = (tokens, idx, options, env, self) => {
  const token = tokens[idx];
  const alt = self.renderInlineAsText(token.children, options, env);
  const relSrc = token.attrs[token.attrIndex("src")][1];
  const absSrc = pathToFileURL(path.resolve(imagenesDir, seccionActual, relSrc)).href;
  return `<figure class="captura"><img src="${absSrc}" alt="${escapeHtml(alt)}" /><figcaption>${escapeHtml(alt)}</figcaption></figure>`;
};

function leerSecciones() {
  const archivos = fs
    .readdirSync(srcDir)
    .filter((f) => f.endsWith(".md"))
    .sort();

  if (archivos.length === 0) {
    throw new Error(`No hay archivos .md en ${srcDir}`);
  }

  return archivos.map((archivo) => {
    const slug = archivo.replace(/\.md$/, "");
    const bruto = fs.readFileSync(path.join(srcDir, archivo), "utf8");
    const { data, content } = matter(bruto);

    const tituloEncontrado = content.match(/^#\s+(.+)$/m);
    if (!tituloEncontrado) {
      throw new Error(`${archivo}: falta un encabezado "# Título" al inicio del contenido.`);
    }

    seccionActual = slug;
    const html = md.render(content);

    return {
      slug,
      titulo: tituloEncontrado[1].trim(),
      grupo: data.grupo ?? null,
      html,
    };
  });
}

// ---------------------------------------------------------------------------
// 2. Índice: agrupa por "grupo" del frontmatter, en el orden de los archivos
// ---------------------------------------------------------------------------

function construirFilasIndice(secciones) {
  const filas = [];
  let grupoActual = null;
  for (const seccion of secciones) {
    if (seccion.grupo !== grupoActual) {
      grupoActual = seccion.grupo;
      if (grupoActual) filas.push({ tipo: "grupo", texto: grupoActual });
    }
    filas.push({ tipo: "item", slug: seccion.slug, texto: seccion.titulo, esSubitem: !!seccion.grupo });
  }
  return filas;
}

function renderizarIndiceHtml(filas, paginaPorSlug) {
  return filas
    .map((fila) => {
      if (fila.tipo === "grupo") {
        return `<div class="indice-grupo">${escapeHtml(fila.texto)}</div>`;
      }
      const pagina = paginaPorSlug?.[fila.slug] ?? "";
      return (
        `<a class="indice-fila${fila.esSubitem ? " nivel-grupo-item" : ""}" href="#sec-${fila.slug}">` +
        `<span class="indice-titulo">${escapeHtml(fila.texto)}</span>` +
        `<span class="indice-relleno"></span>` +
        `<span class="indice-pagina">${pagina}</span>` +
        `</a>`
      );
    })
    .join("\n");
}

// ---------------------------------------------------------------------------
// 3. Armar el documento HTML completo (portada + índice + contenido)
// ---------------------------------------------------------------------------

function cssFuentes() {
  const fontsDir = path.join(rootDir, "node_modules", "@fontsource", "inter", "files");
  const cara = (peso, archivo) => {
    const url = pathToFileURL(path.join(fontsDir, archivo)).href;
    return `@font-face{font-family:'Inter';font-style:normal;font-weight:${peso};src:url('${url}') format('woff2');font-display:swap;}`;
  };
  return [cara(400, "inter-latin-400-normal.woff2"), cara(600, "inter-latin-600-normal.woff2")].join("\n");
}

function construirHtml({ secciones, filasIndice, incluirMarcadores, paginaPorSlug, fechaGeneracion }) {
  const plantilla = fs.readFileSync(path.join(plantillasDir, "documento.html"), "utf8");
  const estilosManual = fs.readFileSync(path.join(estilosDir, "manual.css"), "utf8");
  const logoAbs = pathToFileURL(path.resolve(rootDir, config.logo)).href;

  const altoContenidoMm = (config.paginaCm.alto - config.margenCm.arriba - config.margenCm.abajo) * 10;

  const indiceHtml = renderizarIndiceHtml(filasIndice, paginaPorSlug);
  const contenidoHtml = secciones
    .map((seccion) => {
      const marcador = incluirMarcadores
        ? `<span class="marcador-pagina">MARCA_PAGINA_${seccion.slug}_FIN</span>`
        : "";
      return `<section class="seccion" id="sec-${seccion.slug}">${marcador}${seccion.html}</section>`;
    })
    .join("\n");

  return plantilla
    .replaceAll("{{TITULO}}", escapeHtml(config.titulo))
    .replaceAll("{{SUBTITULO}}", escapeHtml(config.subtitulo))
    .replaceAll("{{VERSION}}", escapeHtml(config.version))
    .replaceAll("{{FECHA}}", escapeHtml(fechaGeneracion))
    .replaceAll("{{LOGO_SRC}}", logoAbs)
    .replaceAll("{{ALTO_CONTENIDO_MM}}", altoContenidoMm.toFixed(2))
    .replace("{{FUENTES_CSS}}", cssFuentes())
    .replace("{{ESTILOS_MANUAL}}", estilosManual)
    .replace("{{INDICE}}", indiceHtml)
    .replace("{{CONTENIDO}}", contenidoHtml);
}

// ---------------------------------------------------------------------------
// 4. Ubicar en qué página del PDF de medición cayó cada marcador de sección
// ---------------------------------------------------------------------------

async function ubicarPaginasDeMarcadores(bufferPdf, slugs) {
  const textoPorPagina = [];
  await pdfParse(bufferPdf, {
    pagerender: async (paginaPdf) => {
      const contenido = await paginaPdf.getTextContent();
      // Sin separador: Chrome a veces parte el texto de un mismo span en varios
      // "items" de PDF (p. ej. "dashbo" + "ard_FIN") sin que haya un espacio real
      // entre ellos; unir con espacio rompería la búsqueda exacta del marcador.
      const texto = contenido.items.map((item) => item.str).join("");
      textoPorPagina.push(texto);
      return texto;
    },
  });

  const resultado = {};
  for (const slug of slugs) {
    const marca = `MARCA_PAGINA_${slug}_FIN`;
    const indice = textoPorPagina.findIndex((texto) => texto.includes(marca));
    if (indice === -1) {
      console.warn(`  ! No se encontró el marcador de la sección "${slug}" en el PDF de medición.`);
    }
    resultado[slug] = indice === -1 ? null : indice + 1;
  }
  return resultado;
}

// ---------------------------------------------------------------------------
// 5. Chrome del sistema
// ---------------------------------------------------------------------------

function ubicarChrome() {
  const forzado = process.env.CHROME_PATH;
  if (forzado) {
    if (!fs.existsSync(forzado)) throw new Error(`CHROME_PATH="${forzado}" no existe.`);
    return forzado;
  }
  const encontrado = config.chromeCandidatos.find((ruta) => fs.existsSync(ruta));
  if (!encontrado) {
    throw new Error(
      "No se encontró Chrome instalado en las rutas habituales. " +
        "Defina la variable de entorno CHROME_PATH con la ruta al chrome.exe.",
    );
  }
  return encontrado;
}

// ---------------------------------------------------------------------------
// main
// ---------------------------------------------------------------------------

async function main() {
  const inicio = Date.now();
  console.log("Manual de Usuario — Licencias Médicas: compilando...\n");

  fs.mkdirSync(buildDir, { recursive: true });
  fs.mkdirSync(distDir, { recursive: true });

  const secciones = leerSecciones();
  const filasIndice = construirFilasIndice(secciones);
  const totalImagenes = secciones.reduce((n, s) => n + (s.html.match(/<figure class="captura"/g)?.length ?? 0), 0);
  console.log(`  ${secciones.length} sección(es), ${totalImagenes} captura(s) referenciadas.`);

  const chromePath = ubicarChrome();
  console.log(`  Usando Chrome: ${chromePath}`);

  const anchoContenidoPx = Math.round(((config.paginaCm.ancho - config.margenCm.izquierda - config.margenCm.derecha) * 96) / 2.54);
  const pdfOpts = {
    width: `${config.paginaCm.ancho}cm`,
    height: `${config.paginaCm.alto}cm`,
    margin: {
      top: `${config.margenCm.arriba}cm`,
      bottom: `${config.margenCm.abajo}cm`,
      left: `${config.margenCm.izquierda}cm`,
      right: `${config.margenCm.derecha}cm`,
    },
    printBackground: true,
  };

  const browser = await puppeteer.launch({ executablePath: chromePath, headless: true });
  try {
    const page = await browser.newPage();
    await page.setViewport({ width: anchoContenidoPx, height: 1400 });
    await page.emulateMediaType("print");

    const fechaGeneracion = new Date().toLocaleDateString("es-CL", { year: "numeric", month: "long", day: "numeric" });

    // --- Pasada 1: medición ---
    console.log("  Pasada 1/2: midiendo paginación real...");
    const htmlMedicion = construirHtml({
      secciones,
      filasIndice,
      incluirMarcadores: true,
      paginaPorSlug: null,
      fechaGeneracion,
    });
    const rutaMedicion = path.join(buildDir, "manual.medicion.html");
    fs.writeFileSync(rutaMedicion, htmlMedicion, "utf8");
    await page.goto(pathToFileURL(rutaMedicion).href, { waitUntil: "networkidle0" });
    const bufferMedicion = await page.pdf({ ...pdfOpts, displayHeaderFooter: false });

    const paginaPorSlug = await ubicarPaginasDeMarcadores(bufferMedicion, secciones.map((s) => s.slug));

    // --- Pasada 2: final ---
    console.log("  Pasada 2/2: generando PDF final con índice completo...");
    const htmlFinal = construirHtml({
      secciones,
      filasIndice,
      incluirMarcadores: false,
      paginaPorSlug,
      fechaGeneracion,
    });
    const rutaFinal = path.join(buildDir, "manual.html");
    fs.writeFileSync(rutaFinal, htmlFinal, "utf8");
    await page.goto(pathToFileURL(rutaFinal).href, { waitUntil: "networkidle0" });

    const footerTemplate = fs.readFileSync(path.join(plantillasDir, "footer.html"), "utf8");
    const bufferFinal = await page.pdf({
      ...pdfOpts,
      displayHeaderFooter: true,
      headerTemplate: "<span></span>",
      footerTemplate,
    });

    const salidaPdf = path.join(rootDir, ...config.salidaPdf.split("/"));
    fs.mkdirSync(path.dirname(salidaPdf), { recursive: true });
    fs.writeFileSync(salidaPdf, bufferFinal);

    const kb = (bufferFinal.length / 1024).toFixed(0);
    const segundos = ((Date.now() - inicio) / 1000).toFixed(1);
    console.log(`\n✔ PDF generado: ${salidaPdf} (${kb} KB) en ${segundos}s`);

    if (process.argv.includes("--open")) {
      const { execFile } = await import("node:child_process");
      execFile("cmd", ["/c", "start", "", salidaPdf]);
    }
  } finally {
    await browser.close();
  }
}

main().catch((error) => {
  console.error("\n✘ Falló la compilación del manual:\n");
  console.error(error);
  process.exitCode = 1;
});
