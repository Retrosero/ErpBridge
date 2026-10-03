// Pure order rules (node --test: tests/katalog-web/order.test.mjs): what cart/quote and POST orders
// carry (GOAL_MUSTERI_KATALOGU §5.2), how a quote refreshes the cart, one requestId per cart content,
// one submission at a time, and an order's status in the customer's words (§5.3).

import { snapshot } from './cart.js';

/** The lines both cart/quote and orders take: key + quantity in pieces, nothing else. */
export function requestLines(lines) {
    return lines.map(l => ({ key: l.key, quantity: l.quantity }));
}

/** Names the cart's content: a quote and a requestId belong to exactly these keys and quantities. */
export function cartSignature(lines) {
    return lines.map(l => l.key + '*' + l.quantity).join('|');
}

/** Quote lines by product key. */
export function quoteLinesByKey(quote) {
    const map = new Map();
    for (const line of (quote && Array.isArray(quote.lines) ? quote.lines : [])) {
        if (line && typeof line.key === 'string') map.set(line.key, line);
    }
    return map;
}

/** How many quote lines carry an issue (the order cannot be sent while any does). */
export function issueCount(quote) {
    return quote && Array.isArray(quote.lines) ? quote.lines.filter(l => l && l.issue).length : 0;
}

function samePrice(a, b) {
    if (!a || !b) return a === b;
    return a.list === b.list && a.net === b.net && a.discountPercent === b.discountPercent && a.includesVat === b.includesVat;
}

function sameBox(a, b) {
    if (!a || !b) return a === b;
    return a.qty === b.qty && !!a.only === !!b.only;
}

/**
 * Takes what the server says now (name, code, unit, carton, price, stock, picture) into the cart's
 * product snapshots, so the cart page and the catalogue show today's figures. Quantities never change here.
 * A quote without `thumb` (an older server) keeps the snapshot's picture.
 * Lines the server no longer offers keep their snapshot. Returns { lines, priceChanged } where
 * `lines` is the same array when nothing differs and `priceChanged` counts lines whose price moved.
 */
export function refreshLines(lines, quote) {
    const byKey = quoteLinesByKey(quote);
    let changed = false;
    let priceChanged = 0;
    const next = lines.map(line => {
        const q = byKey.get(line.key);
        if (!q || q.issue === 'NOT_AVAILABLE' || !q.price) return line;
        const old = line.product;
        const product = snapshot({
            key: line.key,
            code: q.code || old.code,
            name: q.name || old.name,
            unit: q.unit || '',
            box: q.box || null,
            price: q.price,
            inStock: q.issue !== 'OUT_OF_STOCK',
            thumb: q.thumb !== undefined ? q.thumb : old.thumb,
        });
        const priceMoved = !!old.price && !samePrice(old.price, product.price);
        if (priceMoved) priceChanged += 1;
        if (!priceMoved && !!old.price === !!product.price && sameBox(old.box, product.box) && old.name === product.name
            && old.code === product.code && old.unit === product.unit && old.inStock === product.inStock
            && (old.thumb || null) === product.thumb) {
            return line;
        }
        changed = true;
        return { key: line.key, quantity: line.quantity, product };
    });
    return { lines: changed ? next : lines, priceChanged };
}

/**
 * A version 4 UUID for POST orders. crypto.randomUUID exists only on https (and localhost); a page
 * opened over plain http on the LAN still gets a valid id from getRandomValues.
 */
export function newRequestId(cryptoImpl = globalThis.crypto) {
    if (typeof cryptoImpl.randomUUID === 'function') return cryptoImpl.randomUUID();
    const bytes = cryptoImpl.getRandomValues(new Uint8Array(16));
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('');
    return hex.slice(0, 8) + '-' + hex.slice(8, 12) + '-' + hex.slice(12, 16) + '-' + hex.slice(16, 20) + '-' + hex.slice(20);
}

/**
 * One requestId per cart content: the same lines keep the same id until the order is accepted, so a
 * retry after a lost answer (or a second tap) finds the same order on the server instead of a second.
 */
export function createRequestIds(newId) {
    let current = null;
    return {
        idFor(signature) {
            if (!current || current.signature !== signature) current = { signature, id: newId() };
            return current.id;
        },
        forget() {
            current = null;
        },
    };
}

/**
 * Wraps an async action so a call made while the previous one is still running gets that same
 * promise instead of starting another (a double tap sends one order).
 */
export function singleFlight(fn) {
    let running = null;
    function run(...args) {
        if (!running) {
            running = (async () => {
                try {
                    return await fn(...args);
                } finally {
                    running = null;
                }
            })();
        }
        return running;
    }
    run.busy = () => running !== null;
    return run;
}

// §5.3: the customer's labels for NEW / CLAIMED / COMPLETED / REJECTED. Colour always comes with text.
const STATUS = {
    NEW: { label: 'Alındı', tone: 'brand', icon: 'check' },
    CLAIMED: { label: 'İnceleniyor', tone: 'warning', icon: 'orders' },
    COMPLETED: { label: 'Siparişe çevrildi', tone: 'success', icon: 'check' },
    REJECTED: { label: 'Reddedildi', tone: 'danger', icon: 'close' },
};

export function orderStatus(status) {
    return Object.prototype.hasOwnProperty.call(STATUS, status) ? STATUS[status] : { label: 'Durumu bilinmiyor', tone: 'neutral', icon: 'alert' };
}
