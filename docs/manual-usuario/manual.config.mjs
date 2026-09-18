// Configuración del manual. Cambiar aquí título/versión no requiere tocar build.mjs.
export default {
  titulo: "Manual de Usuario",
  subtitulo: "Sistema de Licencias Médicas",
  version: "1.0",
  logo: "../../assets/app-icon.svg",
  salidaPdf: "dist/Manual-Usuario-LicenciasMedicas.pdf",

  // Formato de página A4 en centímetros. Se usan tanto para el CSS de portada/índice
  // como para los márgenes reales que le pasamos a Puppeteer al imprimir.
  paginaCm: { ancho: 21, alto: 29.7 },
  margenCm: { arriba: 2.4, abajo: 1.8, izquierda: 2, derecha: 2 },

  // Rutas candidatas del ejecutable de Chrome instalado en el sistema (Windows).
  // Se puede forzar una ruta específica con la variable de entorno CHROME_PATH.
  chromeCandidatos: [
    "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
    "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
    process.env.LOCALAPPDATA ? `${process.env.LOCALAPPDATA}\\Google\\Chrome\\Application\\chrome.exe` : null,
  ].filter(Boolean),
};
