// Generates the demo product pictures in public/images/products as simple SVG illustrations.
// Run with: node scripts/generate-product-images.mjs
// The SKU list mirrors backend/SkirtPling.DAL/Seed/CatalogSeedData.cs.
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const products = [
  ['SP-PL-001', '#24305e'], ['SP-PL-002', '#e3cfa6'], ['SP-PL-003', '#d4a5a5'], ['SP-PL-004', '#26262b'],
  ['SP-AL-001', '#45484f'], ['SP-AL-002', '#d8c3a0'], ['SP-AL-003', '#b98b57'], ['SP-AL-004', '#a3b18a'],
  ['SP-PE-001', '#26262b'], ['SP-PE-002', '#6d2335'], ['SP-PE-003', '#1f6f54'],
  ['SP-MX-001', '#6b6b3a'], ['SP-MX-002', '#1b2240'], ['SP-MX-003', '#a7a9ac'], ['SP-MX-004', '#c0643f'],
  ['SP-DN-001', '#34466e'], ['SP-DN-002', '#8fa9c9'], ['SP-DN-003', '#3a3a3d'], ['SP-DN-004', '#5677a3'],
  ['SP-PL-005', '#c9a548'],
];

const outDir = join(dirname(fileURLToPath(import.meta.url)), '..', 'public', 'images', 'products');
mkdirSync(outDir, { recursive: true });

/** Mixes a hex color with white; amount 0..1 (1 = white). */
function tint(hex, amount) {
  const n = parseInt(hex.slice(1), 16);
  const mix = (c) => Math.round(c + (255 - c) * amount);
  return `rgb(${mix(n >> 16)}, ${mix((n >> 8) & 255)}, ${mix(n & 255)})`;
}

function shade(hex, amount) {
  const n = parseInt(hex.slice(1), 16);
  const mix = (c) => Math.round(c * (1 - amount));
  return `rgb(${mix(n >> 16)}, ${mix((n >> 8) & 255)}, ${mix(n & 255)})`;
}

function pleatLines(topL, topR, botL, botR, topY, botY, count, color) {
  let lines = '';
  for (let i = 1; i < count; i++) {
    const t = i / count;
    lines += `<line x1="${topL + (topR - topL) * t}" y1="${topY}" x2="${botL + (botR - botL) * t}" y2="${botY}" stroke="${color}" stroke-width="2" opacity="0.55"/>`;
  }
  return lines;
}

function skirtShape(style, color) {
  const dark = shade(color, 0.25);
  const waist = `<rect x="140" y="78" width="120" height="24" rx="4" fill="${dark}"/>`;
  switch (style) {
    case 'PL':
      return waist +
        `<path d="M140 102 L260 102 L318 370 Q200 385 82 370 Z" fill="${color}"/>` +
        pleatLines(140, 260, 82, 318, 102, 372, 12, dark);
    case 'AL':
      return waist + `<path d="M140 102 L260 102 L305 345 Q200 362 95 345 Z" fill="${color}"/>` +
        `<path d="M200 102 L200 352" stroke="${dark}" stroke-width="2" opacity="0.4"/>`;
    case 'PE':
      return waist + `<path d="M140 102 L260 102 L252 360 L148 360 Z" fill="${color}"/>` +
        `<path d="M200 330 L200 360" stroke="${dark}" stroke-width="3"/>`;
    case 'MX':
      return waist + `<path d="M140 102 L260 102 L330 440 Q200 455 70 440 Z" fill="${color}"/>` +
        `<path d="M114 230 Q200 240 286 230 M92 335 Q200 348 308 335" stroke="${dark}" stroke-width="3" fill="none" opacity="0.6"/>`;
    case 'DN':
      return waist + `<path d="M140 102 L260 102 L300 355 Q200 368 100 355 Z" fill="${color}"/>` +
        `<path d="M150 112 L108 345 M250 112 L292 345" stroke="#e8b04b" stroke-width="2" stroke-dasharray="6 5"/>` +
        `<path d="M155 130 Q170 165 195 160 M245 130 Q230 165 205 160" stroke="#e8b04b" stroke-width="2" fill="none" stroke-dasharray="5 4"/>` +
        `<circle cx="200" cy="90" r="5" fill="#c9a548"/>`;
  }
}

function frontView(style, color) {
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 500" width="400" height="500">
  <rect width="400" height="500" fill="${tint(color, 0.85)}"/>
  <ellipse cx="200" cy="470" rx="140" ry="12" fill="${shade(color, 0.2)}" opacity="0.15"/>
  ${skirtShape(style, color)}
</svg>
`;
}

function fabricDetail(style, color) {
  const dark = shade(color, 0.22);
  const light = tint(color, 0.25);
  let pattern;
  if (style === 'PL') pattern = Array.from({ length: 14 }, (_, i) => `<rect x="${i * 30}" y="0" width="14" height="500" fill="${dark}" opacity="0.5"/>`).join('');
  else if (style === 'DN') pattern = Array.from({ length: 60 }, (_, i) => `<line x1="${i * 14 - 500}" y1="0" x2="${i * 14}" y2="500" stroke="${light}" stroke-width="3" opacity="0.5"/>`).join('');
  else pattern = Array.from({ length: 50 }, (_, i) => `<line x1="0" y1="${i * 10}" x2="400" y2="${i * 10}" stroke="${dark}" stroke-width="1" opacity="0.35"/>`).join('') +
    Array.from({ length: 40 }, (_, i) => `<line x1="${i * 10}" y1="0" x2="${i * 10}" y2="500" stroke="${light}" stroke-width="1" opacity="0.25"/>`).join('');
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 500" width="400" height="500">
  <rect width="400" height="500" fill="${color}"/>
  ${pattern}
  <circle cx="200" cy="250" r="120" fill="none" stroke="#ffffff" stroke-width="3" opacity="0.6"/>
</svg>
`;
}

for (const [sku, color] of products) {
  const style = sku.split('-')[1];
  const base = sku.toLowerCase();
  writeFileSync(join(outDir, `${base}-1.svg`), frontView(style, color));
  writeFileSync(join(outDir, `${base}-2.svg`), fabricDetail(style, color));
}

writeFileSync(join(outDir, '..', 'placeholder.svg'), `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 500" width="400" height="500">
  <rect width="400" height="500" fill="#f5e9e4"/>
  <path d="M150 140 L250 140 L290 360 Q200 372 110 360 Z" fill="none" stroke="#9b4d3a" stroke-width="4" opacity="0.4"/>
</svg>
`);

console.log(`Wrote ${products.length * 2} product images to ${outDir}`);
