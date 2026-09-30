// Pure cart rules (node --test: tests/katalog-web/cart.test.mjs). Quantities are always pieces;
// a carton-only product moves in whole cartons. The server re-checks everything in cart/quote.

import { count } from './format.js';

export const MAX_QTY = 9999;
/** GOAL_MUSTERI_KATALOGU §5.2: an order carries at most 200 lines. */
export const MAX_LINES = 200;

/** The product's carton, or null. The server sends `box` only with qty >= 2; anything else is ignored. */
export function boxOf(product) {
    const box = product && product.box;
    return box && Number.isInteger(box.qty) && box.qty >= 2 ? box : null;
}

/** How far + and − move: one carton for a carton-only product, one piece otherwise. */
export function stepOf(product) {
    const box = boxOf(product);
    return box && box.only ? box.qty : 1;
}

/**
 * Turns any requested amount into one the cart accepts: a whole number, rounded UP to the carton
 * for a carton-only product, and never above MAX_QTY (rounded DOWN to a carton there). 0 = remove.
 */
export function normalizeQty(product, qty) {
    let n = Math.ceil(Number(qty));
    if (!Number.isFinite(n) || n <= 0) return 0;
    const step = stepOf(product);
    n = Math.ceil(n / step) * step;
    if (n > MAX_QTY) n = Math.floor(MAX_QTY / step) * step;
    return n;
}

export function increase(product, qty) {
    const next = normalizeQty(product, (qty || 0) + stepOf(product));
    return next > (qty || 0) ? next : qty || 0;
}

export function decrease(product, qty) {
    return normalizeQty(product, (qty || 0) - stepOf(product));
}

/** "+1 koli" on a product that also sells single pieces. */
export function addCarton(product, qty) {
    const box = boxOf(product);
    if (!box) return increase(product, qty);
    const next = normalizeQty(product, (qty || 0) + box.qty);
    return next > (qty || 0) ? next : qty || 0;
}

/** "adet" unless the product names its own unit. */
export function unitText(product) {
    const unit = product && typeof product.unit === 'string' ? product.unit.trim() : '';
    return unit ? unit.toLocaleLowerCase('tr-TR') : 'adet';
}

/** "2 koli · 48 adet" for a carton-only product, "5 adet" otherwise. */
export function qtyLabel(product, qty) {
    const pieces = count(qty) + ' ' + unitText(product);
    const box = boxOf(product);
    if (box && box.only && qty > 0) return count(qty / box.qty) + ' koli · ' + pieces;
    return pieces;
}

/** What the cart keeps of a product so the cart page can draw a line before cart/quote answers. */
export function snapshot(product) {
    return {
        key: product.key,
        code: product.code,
        name: product.name,
        unit: product.unit || '',
        box: boxOf(product),
        price: product.price || null,
        inStock: product.inStock !== false,
        thumb: product.thumb || null,
    };
}

export function lineQty(lines, key) {
    const line = lines.find(l => l.key === key);
    return line ? line.quantity : 0;
}

/**
 * Sets a product's quantity and returns { lines, quantity, full }. The array is new when something
 * changed. `full` = a new line was refused because the cart already holds MAX_LINES products.
 */
export function setQty(lines, product, qty) {
    const quantity = normalizeQty(product, qty);
    const index = lines.findIndex(l => l.key === product.key);
    if (quantity === 0) {
        return { lines: index < 0 ? lines : lines.filter((_, i) => i !== index), quantity: 0, full: false };
    }
    if (index >= 0) {
        if (lines[index].quantity === quantity) return { lines, quantity, full: false };
        const next = lines.slice();
        next[index] = { key: product.key, quantity, product: snapshot(product) };
        return { lines: next, quantity, full: false };
    }
    if (lines.length >= MAX_LINES) return { lines, quantity: 0, full: true };
    return { lines: lines.concat({ key: product.key, quantity, product: snapshot(product) }), quantity, full: false };
}

export function removeLine(lines, key) {
    return lines.some(l => l.key === key) ? lines.filter(l => l.key !== key) : lines;
}

/**
 * Reads lines back from storage: anything malformed (another version, a hand-edited value) is
 * dropped rather than trusted, and quantities are re-normalised against the stored carton.
 */
export function parseLines(value) {
    if (!Array.isArray(value)) return [];
    const out = [];
    const seen = new Set();
    for (const raw of value) {
        if (out.length >= MAX_LINES) break;
        if (!raw || typeof raw.key !== 'string' || !raw.key || seen.has(raw.key)) continue;
        const product = raw.product && typeof raw.product === 'object' ? raw.product : null;
        if (!product || product.key !== raw.key || typeof product.name !== 'string') continue;
        const quantity = normalizeQty(product, raw.quantity);
        if (quantity === 0) continue;
        seen.add(raw.key);
        out.push({ key: raw.key, quantity, product: snapshot(product) });
    }
    return out;
}
