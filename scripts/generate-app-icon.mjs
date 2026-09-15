// Rasterizes the app icon (same geometry as ../assets/app-icon.svg, on a 64x64
// viewBox) into PNGs and packs them into a multi-resolution .ico. No external
// dependencies: PNG encoding uses Node's built-in zlib (deflateSync produces
// the zlib stream PNG's IDAT expects), and shapes are simple polygons/rects
// rasterized with subpixel supersampling for anti-aliasing.
//
// Usage: node generate-app-icon.mjs <out.ico> [size1 size2 ...]
//        node generate-app-icon.mjs --png <out.png> <size>

import { deflateSync } from "node:zlib";
import { writeFileSync } from "node:fs";

const VB = 64;
const SAMPLES = 4; // 4x4 subpixel supersampling

// --- geometry (mirrors assets/app-icon.svg) ---------------------------------

const pentagon = (left, right, top, bottom, fold) => [
  [left, top],
  [right - fold, top],
  [right, top + fold],
  [right, bottom],
  [left, bottom],
];

const pageOuter = pentagon(8, 56, 4, 60, 12); // stroke color
const pageInner = pentagon(10, 54, 6, 58, 10); // white fill

const flapOuter = [
  [42.5, 4.5],
  [55.5, 4.5],
  [55.5, 17.5],
];
const flapInner = [
  [44, 6],
  [54, 6],
  [54, 16],
];

const rects = {
  crossV: [27, 14, 10, 24],
  crossH: [20, 21, 24, 10],
  line1: [16, 41, 32, 3],
  line2: [16, 47, 32, 3],
  line3: [16, 53, 24, 3],
};

const GRAY = [148, 163, 184, 255]; // #94a3b8
const WHITE = [255, 255, 255, 255];
const FLAP = [226, 232, 240, 255]; // #e2e8f0
const RED = [220, 38, 38, 255]; // #dc2626
const LINE_GRAY = [203, 213, 225, 255]; // #cbd5e1

// Layers drawn back-to-front; last hit wins per sample.
const shapes = [
  { test: (x, y) => pointInPolygon(x, y, pageOuter), color: GRAY },
  { test: (x, y) => pointInPolygon(x, y, pageInner), color: WHITE },
  { test: (x, y) => pointInPolygon(x, y, flapOuter), color: GRAY },
  { test: (x, y) => pointInPolygon(x, y, flapInner), color: FLAP },
  { test: (x, y) => pointInRect(x, y, rects.crossV), color: RED },
  { test: (x, y) => pointInRect(x, y, rects.crossH), color: RED },
  { test: (x, y) => pointInRect(x, y, rects.line1), color: LINE_GRAY },
  { test: (x, y) => pointInRect(x, y, rects.line2), color: LINE_GRAY },
  { test: (x, y) => pointInRect(x, y, rects.line3), color: LINE_GRAY },
];

function pointInPolygon(x, y, poly) {
  let inside = false;
  for (let i = 0, j = poly.length - 1; i < poly.length; j = i++) {
    const [xi, yi] = poly[i];
    const [xj, yj] = poly[j];
    const intersects = yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi;
    if (intersects) inside = !inside;
  }
  return inside;
}

function pointInRect(x, y, [rx, ry, rw, rh]) {
  return x >= rx && x <= rx + rw && y >= ry && y <= ry + rh;
}

// --- rasterizer --------------------------------------------------------------

function renderPixels(size) {
  const pixels = Buffer.alloc(size * size * 4);
  for (let py = 0; py < size; py++) {
    for (let px = 0; px < size; px++) {
      let rSum = 0, gSum = 0, bSum = 0, aSum = 0;
      for (let sy = 0; sy < SAMPLES; sy++) {
        for (let sx = 0; sx < SAMPLES; sx++) {
          const vx = ((px + (sx + 0.5) / SAMPLES) / size) * VB;
          const vy = ((py + (sy + 0.5) / SAMPLES) / size) * VB;
          let color = null;
          for (const shape of shapes) {
            if (shape.test(vx, vy)) color = shape.color;
          }
          if (color) {
            const a = color[3] / 255;
            rSum += color[0] * a;
            gSum += color[1] * a;
            bSum += color[2] * a;
            aSum += a;
          }
        }
      }
      const total = SAMPLES * SAMPLES;
      const alpha = aSum / total;
      const idx = (py * size + px) * 4;
      if (alpha > 0) {
        pixels[idx] = Math.round(rSum / aSum);
        pixels[idx + 1] = Math.round(gSum / aSum);
        pixels[idx + 2] = Math.round(bSum / aSum);
        pixels[idx + 3] = Math.round(alpha * 255);
      }
    }
  }
  return pixels;
}

// --- PNG encoding ------------------------------------------------------------

const CRC_TABLE = (() => {
  const table = new Uint32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    table[n] = c >>> 0;
  }
  return table;
})();

function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}

function chunk(type, data) {
  const typeBuf = Buffer.from(type, "ascii");
  const lenBuf = Buffer.alloc(4);
  lenBuf.writeUInt32BE(data.length, 0);
  const crcBuf = Buffer.alloc(4);
  crcBuf.writeUInt32BE(crc32(Buffer.concat([typeBuf, data])), 0);
  return Buffer.concat([lenBuf, typeBuf, data, crcBuf]);
}

function encodePng(pixels, size) {
  const signature = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);

  const ihdrData = Buffer.alloc(13);
  ihdrData.writeUInt32BE(size, 0);
  ihdrData.writeUInt32BE(size, 4);
  ihdrData[8] = 8; // bit depth
  ihdrData[9] = 6; // color type RGBA
  ihdrData[10] = 0;
  ihdrData[11] = 0;
  ihdrData[12] = 0;
  const ihdr = chunk("IHDR", ihdrData);

  const raw = Buffer.alloc(size * (1 + size * 4));
  for (let y = 0; y < size; y++) {
    const rowStart = y * (1 + size * 4);
    raw[rowStart] = 0; // filter type: none
    pixels.copy(raw, rowStart + 1, y * size * 4, (y + 1) * size * 4);
  }
  const idat = chunk("IDAT", deflateSync(raw));

  const iend = chunk("IEND", Buffer.alloc(0));

  return Buffer.concat([signature, ihdr, idat, iend]);
}

// --- ICO packing ---------------------------------------------------------

function packIco(pngBuffers) {
  const count = pngBuffers.length;
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0);
  header.writeUInt16LE(1, 2); // type: icon
  header.writeUInt16LE(count, 4);

  const entries = [];
  let offset = 6 + 16 * count;
  const dataBufs = [];
  for (const { size, png } of pngBuffers) {
    const entry = Buffer.alloc(16);
    entry[0] = size >= 256 ? 0 : size;
    entry[1] = size >= 256 ? 0 : size;
    entry[2] = 0; // color count
    entry[3] = 0; // reserved
    entry.writeUInt16LE(1, 4); // color planes
    entry.writeUInt16LE(32, 6); // bits per pixel
    entry.writeUInt32LE(png.length, 8);
    entry.writeUInt32LE(offset, 12);
    entries.push(entry);
    dataBufs.push(png);
    offset += png.length;
  }
  return Buffer.concat([header, ...entries, ...dataBufs]);
}

// --- CLI ---------------------------------------------------------------------

const args = process.argv.slice(2);

if (args[0] === "--png") {
  const [, outPath, sizeArg] = args;
  const size = parseInt(sizeArg, 10);
  const png = encodePng(renderPixels(size), size);
  writeFileSync(outPath, png);
  console.log(`Wrote ${outPath} (${size}x${size})`);
} else {
  const outPath = args[0] ?? "app-icon.ico";
  const sizes = (args.slice(1).length ? args.slice(1) : [16, 32, 48, 256]).map((s) => parseInt(s, 10));
  const pngBuffers = sizes.map((size) => ({ size, png: encodePng(renderPixels(size), size) }));
  writeFileSync(outPath, packIco(pngBuffers));
  console.log(`Wrote ${outPath} with sizes: ${sizes.join(", ")}`);
}
