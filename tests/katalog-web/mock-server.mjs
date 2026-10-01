// Dependency-free stand-in for the CentralApi catalogue host while the server side is being built
// (GOAL_MUSTERI_KATALOGU §5.2, §6, §7). Serves wwwroot/katalog under /assets/dev/, the shell for
// /{KOD} and /{KOD}/**, and fakes every customer endpoint with a cookie session.
//
//   node tests/katalog-web/mock-server.mjs         -> http://localhost:5173/DEMO1234
//   PORT=8080 MOCK_DELAY_MS=0 node tests/katalog-web/mock-server.mjs
//   MOCK_AUTO_LOGIN=demo node tests/katalog-web/mock-server.mjs   (signed in as demo without the form)
//
// Test accounts (mock only): demo / demo1234 (10% discount, VAT-inclusive list, every feature),
// tek / tek12345 (no discount, VAT-exclusive list, ordering and account pages off),
// pasif / pasif1234 (inactive -> 403 ACCOUNT_INACTIVE).
//
// Scenarios: the first POST orders of a request holding "Türk Kahvesi 100 g" raises that product's
// price by 2,50 TL first, so it answers 409 PRICE_CHANGED with the new quote; sending again (same
// requestId, the new total) goes through. "Eski Ambalaj Çay 500 g" appears on some invoices but is
// no longer in the catalogue: invoices/detail gives productKey null, purchased gives product null.

import { createServer } from 'node:http';
import { createHash, randomBytes, randomUUID } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { fileURLToPath, pathToFileURL } from 'node:url';
import path from 'node:path';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../src/ErpBridge.CentralApi/wwwroot/katalog');
const VERSION = 'dev';
const CODE = 'DEMO1234';
const COMPANY = 'Demo Gıda Dağıtım';
const CSP = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' https: data:; connect-src 'self'; "
    + "object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";
const TYPES = {
    '.js': 'text/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.html': 'text/html; charset=utf-8',
    '.svg': 'image/svg+xml',
    '.txt': 'text/plain; charset=utf-8',
    '.png': 'image/png',
};
const DAY_MS = 24 * 60 * 60 * 1000;
const MAX_QTY = 9999;
const MAX_LINES = 200;
const MAX_OPEN_ORDERS = 20;
const THROTTLE_FAILS = 5;
const THROTTLE_MS = 15 * 60 * 1000;

// ---------- Data ----------

const CATEGORY_DATA = [
    ['İçecek', 10, ['Maden Suyu 200 ml', 'Kola 1 L', 'Limonata 1 L', 'Ayran 200 ml', 'Vişne Suyu 1 L', 'Soğuk Çay Şeftali 330 ml',
        'Su 0,5 L', 'Su 5 L', 'Türk Kahvesi 100 g', 'Siyah Çay 1 kg', 'Enerji İçeceği 250 ml']],
    ['Atıştırmalık', 10, ['Simit Kraker 150 g', 'Patates Cipsi 110 g', 'Çikolatalı Gofret 36 g', 'Fındıklı Çikolata 80 g',
        'Kavrulmuş Fıstık 200 g', 'Leblebi 250 g', 'Sade Bisküvi 175 g', 'Kakaolu Kek 45 g', 'Güllü Lokum 350 g', 'Mısır Cipsi 125 g',
        'Mini Poğaça 10\'lu']],
    ['Temel Gıda', 1, ['Baldo Pirinç 1 kg', 'Pilavlık Bulgur 1 kg', 'Kırmızı Mercimek 1 kg', 'Spagetti Makarna 500 g',
        'Ayçiçek Yağı 5 L', 'Zeytinyağı 1 L', 'Toz Şeker 5 kg', 'Buğday Unu 5 kg', 'Domates Salçası 830 g', 'Nohut 1 kg', 'İyotlu Tuz 750 g']],
    ['Kahvaltılık', 1, ['Beyaz Peynir 500 g', 'Kaşar Peyniri 400 g', 'Siyah Zeytin 1 kg', 'Yeşil Zeytin 500 g', 'Süzme Bal 850 g',
        'Tahin Pekmez 700 g', 'Çilek Reçeli 380 g', 'Tereyağı 250 g', 'Yumurta 30\'lu', 'Kasap Sucuk 250 g', 'Fındık Ezmesi 350 g']],
    ['Temizlik', 20, ['Bulaşık Deterjanı 750 ml', 'Çamaşır Suyu 4 L', 'Yüzey Temizleyici 1 L', 'Toz Deterjan 6 kg', 'Yumuşatıcı 3 L',
        'Cam Temizleyici 500 ml', 'Tuvalet Kağıdı 32\'li', 'Kağıt Havlu 12\'li', 'Büyük Çöp Torbası', 'Bulaşık Süngeri 10\'lu',
        'IŞIK Parlatıcı 500 ml']],
    ['Kişisel Bakım', 20, ['Şampuan 500 ml', 'Sıvı Sabun 1,5 L', 'Diş Macunu 75 ml', 'Deodorant 150 ml', 'Islak Mendil 90\'lı',
        'Duş Jeli 500 ml', 'Tıraş Köpüğü 200 ml', 'Limon Kolonyası 400 ml', 'Pamuk 100 g', 'El Kremi 75 ml', 'Katı Sabun 4\'lü']],
];
const BRANDS = ['Anadolu', 'Ege', 'Toros', 'Marmara', 'Uno', 'Karadeniz'];

const r2 = x => Math.sign(x) * Math.round((Math.abs(x) + Number.EPSILON) * 100) / 100;
const hex = (value, n) => createHash('sha256').update(value).digest('hex').slice(0, n);
const fold = value => String(value || '').toLocaleLowerCase('tr-TR').trim();

const CATEGORIES = CATEGORY_DATA.map(([name]) => ({ id: hex(name, 12), name }));

const PRODUCTS = [];
CATEGORY_DATA.forEach(([categoryName, vatRate, names], c) => {
    names.forEach((name, j) => {
        const i = PRODUCTS.length;
        const code = String(36100 + i);
        const box = i % 7 === 0 ? { qty: 24, only: true }
            : i % 5 === 1 ? { qty: 12, only: true }
                : i % 5 === 3 ? { qty: 6, only: false } : null;
        const imageCount = i % 3 === 0 ? 3 : i % 3 === 1 ? 1 : 0;
        PRODUCTS.push({
            key: 'k' + hex('product:' + code, 15),
            code,
            name,
            unit: j % 4 === 3 ? 'PAKET' : 'ADET',
            brand: BRANDS[(i + c) % BRANDS.length],
            barcode: '8690000' + String(100000 + i),
            categoryId: CATEGORIES[c].id,
            vatRate,
            box,
            noDiscount: i % 9 === 4,
            inStock: i % 8 !== 6,
            prices: {
                1: r2(12.5 + ((i * 37) % 400) + (i % 2) * 0.45),
                // List 2 lacks a few products: those stay invisible to its accounts (T9).
                2: i % 13 === 5 ? null : r2((12.5 + ((i * 37) % 400)) * 0.85),
            },
            // Product 9's only image is missing on the server, so the placeholder path gets exercised.
            images: i === 9
                ? [{ id: randomUUID(), missing: true }]
                : Array.from({ length: imageCount }, (_, n) => ({ id: hex('img:' + code + ':' + n, 32), n })),
        });
    });
});
const IMAGES = new Map();
for (const p of PRODUCTS) for (const img of p.images) if (!img.missing) IMAGES.set(img.id, { product: p, n: img.n });

const DRIFT = PRODUCTS.find(p => p.name === 'Türk Kahvesi 100 g');
const DRIFT_STEP = 2.5;
const drifted = new Set();
// On old invoices only: never in PRODUCTS, so nothing can add it to the cart.
const DISCONTINUED = { code: '35999', name: 'Eski Ambalaj Çay 500 g', price: 41.9 };

// Banners (S12): live ones in order; a link to a product the account's list does not price is dropped (as the server
// drops links to what the customer does not see). The ended one never shows.
const NO_LIST2 = PRODUCTS.find(p => p.prices[2] === null);
const BANNERS = [
    { id: randomUUID(), title: 'Yaz fırsatları', text: 'Seçili içeceklerde %15’e varan indirim', image: hex('banner:yaz', 32),
        link: { type: 'category', value: CATEGORIES[0].name, categoryId: CATEGORIES[0].id } },
    { id: randomUUID(), title: 'Yeni: ' + DRIFT.name, text: 'Taze çekilmiş, 100 g paketlerde', image: hex('banner:kahve', 32),
        link: { type: 'product', value: DRIFT.code, productKey: DRIFT.key } },
    { id: randomUUID(), title: NO_LIST2.name + ' yalnız toptan listede', text: '', image: null,
        link: { type: 'product', value: NO_LIST2.code, productKey: NO_LIST2.key } },
    { id: randomUUID(), title: 'Web sitemizi ziyaret edin', text: 'Kampanya koşulları ve iletişim bilgileri', image: null,
        link: { type: 'url', value: 'https://example.com/kampanya' } },
    { id: randomUUID(), title: 'Bitmiş kampanya', text: '', image: null, link: null, endsAtMs: Date.now() - DAY_MS },
];
const BANNER_IMAGES = new Map(BANNERS.filter(b => b.image).map((b, n) => [b.image, { label: b.title, n }]));

function bannersFor(account) {
    const now = Date.now();
    return BANNERS.filter(b => !(b.endsAtMs <= now)).map(b => {
        const product = b.link && b.link.type === 'product' ? PRODUCTS.find(p => p.key === b.link.productKey) : null;
        const hidden = product && listPrice(account, product) === null;
        return {
            id: b.id, title: b.title, text: b.text,
            image: b.image ? { thumb: imageUrl(b.image, 's'), full: imageUrl(b.image, 'l') } : null,
            link: hidden ? null : b.link,
        };
    });
}

const PRICE_LISTS = { 1: { no: 1, name: 'Toptan', includesVat: true }, 2: { no: 2, name: 'Perakende', includesVat: false } };

const ACCOUNTS = [
    {
        id: randomUUID(), username: 'demo', password: 'demo1234', isActive: true, tokenVersion: 1,
        customer: { code: '120.01.001', name: 'Yıldız Market' }, discountPercent: 10, priceListNo: 1,
        features: { order: true, statement: true, invoices: true, purchased: true }, balance: 12345.67,
    },
    {
        id: randomUUID(), username: 'tek', password: 'tek12345', isActive: true, tokenVersion: 1,
        customer: { code: '120.01.002', name: 'Kardeşler Bakkal' }, discountPercent: 0, priceListNo: 2,
        features: { order: false, statement: false, invoices: false, purchased: false }, balance: null,
    },
    {
        id: randomUUID(), username: 'pasif', password: 'pasif1234', isActive: false, tokenVersion: 1,
        customer: { code: '120.01.003', name: 'Kapalı Hesap Ltd.' }, discountPercent: 0, priceListNo: 1,
        features: { order: true, statement: true, invoices: true, purchased: true }, balance: 0,
    },
];

// ---------- Catalogue rules (GOAL_MUSTERI_KATALOGU §4) ----------

function listPrice(account, product) {
    return product.prices[account.priceListNo] ?? null;
}

function visibleProducts(account) {
    return PRODUCTS.filter(p => listPrice(account, p) !== null);
}

function discountOf(account, product) {
    return product.noDiscount ? 0 : account.discountPercent;
}

function imageUrl(id, variant) {
    return '/api/v1/catalog/img/' + id + '/' + variant + '?h=' + id.slice(0, 8);
}

function toCProduct(account, p) {
    const list = listPrice(account, p);
    const d = discountOf(account, p);
    const first = p.images[0];
    return {
        key: p.key,
        code: p.code,
        name: p.name,
        unit: p.unit,
        brand: p.brand,
        categoryId: p.categoryId,
        price: { list, net: r2(list * (1 - d / 100)), discountPercent: d, includesVat: PRICE_LISTS[account.priceListNo].includesVat },
        box: p.box,
        inStock: p.inStock,
        thumb: first ? imageUrl(first.id, 's') : null,
    };
}

function sortProducts(list) {
    const order = new Map(CATEGORIES.map((c, i) => [c.id, i]));
    return list.slice().sort((a, b) => (order.get(a.categoryId) - order.get(b.categoryId))
        || a.name.localeCompare(b.name, 'tr-TR') || a.code.localeCompare(b.code));
}

function quoteLine(account, product, quantity) {
    const vat = product.vatRate;
    const includesVat = PRICE_LISTS[account.priceListNo].includesVat;
    const list = listPrice(account, product);
    const d = discountOf(account, product);
    const unitExVat = includesVat && vat !== 0 ? list / (1 + vat / 100) : list;
    const gross = r2(unitExVat * quantity);
    const discount = r2(gross * d / 100);
    const vatAmount = r2((gross - discount) * vat / 100);
    return { gross, discount, vat: vatAmount, total: r2(gross - discount + vatAmount) };
}

function quote(account, lines) {
    const visible = new Map(visibleProducts(account).map(p => [p.key, p]));
    const totals = { gross: 0, discount: 0, vat: 0, total: 0 };
    const out = lines.map(line => {
        const product = visible.get(line.key);
        const quantity = Number(line.quantity);
        let issue = null;
        if (!product) issue = 'NOT_AVAILABLE';
        else if (!Number.isInteger(quantity) || quantity < 1 || quantity > MAX_QTY) issue = 'INVALID_QUANTITY';
        else if (!product.inStock) issue = 'OUT_OF_STOCK';
        else if (product.box && product.box.only && quantity % product.box.qty !== 0) issue = 'CARTON_MULTIPLE';
        const cp = product ? toCProduct(account, product) : null;
        const amounts = issue ? { gross: 0, discount: 0, vat: 0, total: 0 } : quoteLine(account, product, quantity);
        if (!issue) for (const k of Object.keys(totals)) totals[k] = r2(totals[k] + amounts[k]);
        return {
            key: line.key,
            code: product ? product.code : null,
            name: product ? product.name : null,
            unit: product ? product.unit : null,
            quantity: Number.isFinite(quantity) ? quantity : 0,
            box: product ? product.box : null,
            price: cp ? cp.price : null,
            vatRate: product ? product.vatRate : null,
            ...amounts,
            issue,
        };
    });
    return { lines: out, totals };
}

// ---------- Orders, ledger (seeded for demo) ----------

const ORDERS = [];
let orderSeq = 122;

function newOrder(account, lines, note, status, submittedAtMs, rejectReason) {
    const q = quote(account, lines);
    const order = {
        id: randomUUID(),
        accountId: account.id,
        no: 'KT-' + String(++orderSeq).padStart(6, '0'),
        status,
        total: q.totals.total,
        lineCount: lines.length,
        submittedAtMs,
        rejectReason: rejectReason || null,
        note: note || null,
        lines: q.lines.map(l => ({ key: l.key, code: l.code, name: l.name, quantity: l.quantity, net: l.price ? l.price.net : 0, total: l.total })),
    };
    ORDERS.unshift(order);
    return order;
}

{
    const demo = ACCOUNTS[0];
    const pick = (n, qty) => ({ key: PRODUCTS[n].key, quantity: qty });
    const now = Date.now();
    newOrder(demo, [pick(2, 10), pick(4, 5)], 'Sabah teslim olursa sevinirim.', 'COMPLETED', now - 9 * DAY_MS);
    newOrder(demo, [pick(12, 6), pick(24, 3)], null, 'REJECTED', now - 6 * DAY_MS, 'Ürün tedarik edilemedi.');
    newOrder(demo, [pick(33, 2)], null, 'CLAIMED', now - 2 * DAY_MS);
    newOrder(demo, [pick(40, 4), pick(41, 1), pick(43, 3)], null, 'NEW', now - 3 * 60 * 60 * 1000);
}

function isoDate(ms) {
    return new Date(ms).toISOString().slice(0, 10);
}

const INVOICES = Array.from({ length: 14 }, (_, i) => {
    const lines = [0, 1, 2].map(n => {
        const p = PRODUCTS[(i * 5 + n * 7) % PRODUCTS.length];
        const quantity = 2 + ((i + n) % 5) * 2;
        const unitPrice = p.prices[1];
        return { code: p.code, name: p.name, quantity, unitPrice, amount: r2(unitPrice * quantity), product: p };
    });
    if (i % 4 === 1) {
        const d = DISCONTINUED;
        lines.push({ code: d.code, name: d.name, quantity: 3, unitPrice: d.price, amount: r2(d.price * 3), product: null });
    }
    return {
        key: 'inv' + hex('invoice:' + i, 16),
        date: isoDate(Date.now() - (i * 6 + 1) * DAY_MS),
        documentNo: 'A-' + String(4120 - i),
        kind: i % 6 === 5 ? 'sale_return' : 'sale',
        lines,
        total: r2(lines.reduce((s, l) => s + l.amount, 0) * (i % 6 === 5 ? -1 : 1)),
    };
});

// ---------- Sessions ----------

const SESSIONS = new Map();
const FAILURES = new Map();

function cookieName(code) {
    // Real server: __Host-kt_{CODE} (Secure). The __Host- prefix needs https, so the mock drops it.
    return 'kt_' + code;
}

function readCookie(req, name) {
    for (const part of String(req.headers.cookie || '').split(';')) {
        const i = part.indexOf('=');
        if (i > 0 && part.slice(0, i).trim() === name) return part.slice(i + 1).trim();
    }
    return null;
}

function sessionOf(req, autoLogin) {
    const token = readCookie(req, cookieName(CODE));
    if (!token && autoLogin) {
        // Screenshot / layout runs: every request is this account without a sign-in (never on a real server).
        const account = ACCOUNTS.find(a => a.username === autoLogin && a.isActive);
        if (account) return { token: null, session: { tokenVersion: account.tokenVersion }, account };
    }
    const session = token ? SESSIONS.get(token) : null;
    if (!session || session.expiresAt < Date.now()) return null;
    const account = ACCOUNTS.find(a => a.id === session.accountId);
    if (!account || !account.isActive || account.tokenVersion !== session.tokenVersion) return null;
    return { token, session, account };
}

function throttled(username) {
    const entry = FAILURES.get(username);
    if (!entry) return 0;
    const fresh = entry.times.filter(t => Date.now() - t < THROTTLE_MS);
    entry.times = fresh;
    if (fresh.length < THROTTLE_FAILS) return 0;
    return Math.ceil((fresh[0] + THROTTLE_MS - Date.now()) / 1000);
}

function recordFailure(username) {
    const entry = FAILURES.get(username) || { times: [] };
    entry.times.push(Date.now());
    FAILURES.set(username, entry);
}

// ---------- HTTP helpers ----------

function send(res, status, body, headers = {}) {
    res.writeHead(status, headers);
    res.end(body);
}

function json(res, status, value, headers = {}) {
    send(res, status, value === null ? '' : JSON.stringify(value), {
        'Content-Type': 'application/json; charset=utf-8',
        'Cache-Control': 'private, no-store',
        'X-Correlation-Id': randomUUID(),
        ...headers,
    });
}

function apiError(res, status, errorCode, extra = {}, headers = {}) {
    json(res, status, { errorCode, message: errorCode.toLowerCase().replace(/_/g, ' '), traceId: randomUUID(), ...extra }, headers);
}

async function readBody(req) {
    const chunks = [];
    let size = 0;
    for await (const chunk of req) {
        size += chunk.length;
        if (size > 256 * 1024) throw new Error('too large');
        chunks.push(chunk);
    }
    const text = Buffer.concat(chunks).toString('utf8');
    return text ? JSON.parse(text) : {};
}

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', '\'': '&#39;' }[c]));
}

function paging(query, fallback, max) {
    const page = Math.max(1, parseInt(query.get('page'), 10) || 1);
    const pageSize = Math.min(max, Math.max(1, parseInt(query.get('pageSize'), 10) || fallback));
    return { page, pageSize, start: (page - 1) * pageSize };
}

function inRange(date, query) {
    const from = query.get('from');
    const to = query.get('to');
    return (!from || date >= from) && (!to || date <= to);
}

// ---------- Static files and shell ----------

async function serveFile(res, relative) {
    const full = path.resolve(ROOT, relative);
    if (!full.startsWith(ROOT + path.sep)) return send(res, 404, 'Not found');
    try {
        const data = await readFile(full);
        send(res, 200, data, {
            'Content-Type': TYPES[path.extname(full)] || 'application/octet-stream',
            'Cache-Control': 'no-cache',
            'X-Content-Type-Options': 'nosniff',
        });
    } catch {
        send(res, 404, 'Not found', { 'Cache-Control': 'no-store' });
    }
}

async function serveShell(res, known) {
    const template = await readFile(path.join(ROOT, 'index.html'), 'utf8');
    const title = escapeHtml(known ? COMPANY + ' · Müşteri Kataloğu' : 'Müşteri Kataloğu');
    const html = template.split('%V%').join(VERSION).split('%TITLE%').join(title);
    send(res, known ? 200 : 404, html, {
        'Content-Type': 'text/html; charset=utf-8',
        'Cache-Control': 'no-cache',
        'Content-Security-Policy': CSP,
        'X-Content-Type-Options': 'nosniff',
        'X-Robots-Tag': 'noindex',
        'Referrer-Policy': 'same-origin',
    });
}

function serveBanner(res, banner, variant) {
    const width = variant === 'l' ? 1920 : 800;
    const hue = 200 + banner.n * 70;
    const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="' + width + '" height="' + (width * 6 / 16) + '" viewBox="0 0 1600 600">'
        + '<rect width="1600" height="600" fill="hsl(' + hue + ',55%,45%)"/>'
        + '<circle cx="1250" cy="260" r="240" fill="hsl(' + hue + ',60%,62%)"/>'
        + '<circle cx="1450" cy="120" r="90" fill="hsl(' + hue + ',65%,75%)"/></svg>';
    send(res, 200, svg, {
        'Content-Type': 'image/svg+xml',
        'Cache-Control': 'public, max-age=31536000, immutable',
        'X-Content-Type-Options': 'nosniff',
        'Cross-Origin-Resource-Policy': 'same-site',
    });
}

function serveImage(res, id, variant) {
    const banner = BANNER_IMAGES.get(id);
    if (banner) return serveBanner(res, banner, variant);
    const entry = IMAGES.get(id);
    if (!entry) return send(res, 404, 'Not found');
    const size = variant === 'l' ? 1280 : 400;
    const hue = (parseInt(id.slice(0, 4), 16) % 360);
    const label = escapeHtml(entry.product.name.split(' ').slice(0, 2).join(' '));
    const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="' + size + '" height="' + size + '" viewBox="0 0 400 400">'
        + '<rect width="400" height="400" fill="hsl(' + hue + ',60%,92%)"/>'
        + '<circle cx="200" cy="170" r="' + (70 + entry.n * 12) + '" fill="hsl(' + hue + ',55%,62%)"/>'
        + '<text x="200" y="330" font-family="sans-serif" font-size="30" text-anchor="middle" fill="hsl(' + hue + ',40%,25%)">'
        + label + '</text></svg>';
    send(res, 200, svg, {
        'Content-Type': 'image/svg+xml',
        'Cache-Control': 'public, max-age=31536000, immutable',
        'X-Content-Type-Options': 'nosniff',
        'Cross-Origin-Resource-Policy': 'same-site',
    });
}

// ---------- Customer API ----------

async function handleApi(req, res, code, rest, query, autoLogin) {
    const method = req.method;
    const known = code.toUpperCase() === CODE;

    if (method !== 'GET' && method !== 'HEAD') {
        const origin = req.headers.origin;
        if (req.headers['x-katalog'] !== '1' || (origin && origin !== 'http://' + req.headers.host)) {
            return apiError(res, 403, 'CSRF_REJECTED');
        }
    }

    if (rest === 'info' && method === 'GET') {
        return known ? json(res, 200, { companyName: COMPANY, code: CODE }) : apiError(res, 404, 'CATALOG_NOT_FOUND');
    }

    if (rest === 'login' && method === 'POST') {
        if (!known) return apiError(res, 404, 'CATALOG_NOT_FOUND');
        const body = await readBody(req);
        const username = fold(body.username);
        const wait = throttled(username);
        if (wait > 0) return apiError(res, 429, 'RATE_LIMITED', {}, { 'Retry-After': String(wait) });
        const account = ACCOUNTS.find(a => a.username === username);
        if (!account || account.password !== body.password) {
            recordFailure(username);
            return apiError(res, 401, 'INVALID_CREDENTIALS');
        }
        if (!account.isActive) return apiError(res, 403, 'ACCOUNT_INACTIVE');
        FAILURES.delete(username);
        const token = randomBytes(24).toString('base64url');
        const remember = body.remember === true;
        SESSIONS.set(token, {
            accountId: account.id,
            tokenVersion: account.tokenVersion,
            expiresAt: Date.now() + (remember ? 30 * DAY_MS : 12 * 60 * 60 * 1000),
        });
        const cookie = cookieName(CODE) + '=' + token + '; Path=/; HttpOnly; SameSite=Strict' + (remember ? '; Max-Age=' + 30 * 24 * 3600 : '');
        return json(res, 200, { me: meOf(account) }, { 'Set-Cookie': cookie });
    }

    const auth = known ? sessionOf(req, autoLogin) : null;
    if (!auth) return apiError(res, 401, 'INVALID_TOKEN');
    const account = auth.account;

    if (rest === 'logout' && method === 'POST') {
        if (auth.token) SESSIONS.delete(auth.token);
        return send(res, 204, '', { 'Set-Cookie': cookieName(CODE) + '=; Path=/; HttpOnly; SameSite=Strict; Max-Age=0' });
    }
    if (rest === 'me' && method === 'GET') return json(res, 200, meOf(account));

    if (rest === 'password' && method === 'POST') {
        const body = await readBody(req);
        if (body.current !== account.password) return apiError(res, 400, 'INVALID_CREDENTIALS');
        const bytes = Buffer.byteLength(String(body.next || ''), 'utf8');
        if (bytes < 8 || bytes > 72) return apiError(res, 400, 'INVALID_PASSWORD');
        account.password = body.next;
        account.tokenVersion += 1;
        auth.session.tokenVersion = account.tokenVersion; // this browser stays signed in, every other one is out
        return send(res, 204, '');
    }

    if (rest === 'banners' && method === 'GET') return json(res, 200, { items: bannersFor(account) });

    if (rest === 'categories' && method === 'GET') {
        const counts = new Map();
        for (const p of visibleProducts(account)) counts.set(p.categoryId, (counts.get(p.categoryId) || 0) + 1);
        return json(res, 200, { items: CATEGORIES.filter(c => counts.get(c.id)).map(c => ({ ...c, count: counts.get(c.id) })) });
    }

    if (rest === 'products' && method === 'GET') {
        let list = sortProducts(visibleProducts(account));
        const category = query.get('category');
        const q = fold(query.get('q'));
        if (category) list = list.filter(p => p.categoryId === category);
        if (q.length >= 2) list = list.filter(p => [p.name, p.code, p.barcode, p.brand].some(v => fold(v).includes(q)));
        const brands = [...new Set(list.map(p => p.brand).filter(Boolean))].sort((a, b) => a.localeCompare(b, 'tr'));
        list = list.map(p => toCProduct(account, p));
        const min = query.get('minPrice'), max = query.get('maxPrice');
        if ((min && (!Number.isFinite(Number(min)) || Number(min) < 0)) || (max && (!Number.isFinite(Number(max)) || Number(max) < 0)) || (min && max && Number(min) > Number(max))) return apiError(res, 400, 'INVALID_BODY');
        if (query.get('brand')) list = list.filter(p => fold(p.brand) === fold(query.get('brand')));
        if (query.get('stock') === 'in') list = list.filter(p => p.inStock);
        if (query.get('stock') === 'out') list = list.filter(p => !p.inStock);
        if (min) list = list.filter(p => p.price.net >= Number(min));
        if (max) list = list.filter(p => p.price.net <= Number(max));
        if (query.get('discounted') === 'true') list = list.filter(p => p.price.discountPercent > 0 && p.price.net < p.price.list);
        if (query.get('cartonOnly') === 'true') list = list.filter(p => p.box && p.box.only);
        if (query.get('hasImage') === 'true') list = list.filter(p => p.thumb);
        const sort = query.get('sort');
        if (sort && sort !== 'recommended') list.sort((a, b) => {
            const byCode = a.code.localeCompare(b.code, 'tr');
            if (sort === 'price-asc') return a.price.net - b.price.net || byCode;
            if (sort === 'price-desc') return b.price.net - a.price.net || byCode;
            if (sort === 'name-asc') return a.name.localeCompare(b.name, 'tr') || byCode;
            if (sort === 'name-desc') return b.name.localeCompare(a.name, 'tr') || byCode;
            return byCode;
        });
        const { page, pageSize, start } = paging(query, 48, 60);
        return json(res, 200, { items: list.slice(start, start + pageSize), brands, total: list.length, page, pageSize });
    }

    if (rest === 'products/detail' && method === 'GET') {
        const p = visibleProducts(account).find(x => x.key === query.get('key'));
        if (!p) return apiError(res, 404, 'NOT_FOUND');
        return json(res, 200, {
            ...toCProduct(account, p),
            images: p.images.map(img => ({ thumb: imageUrl(img.id, 's'), full: imageUrl(img.id, 'l') })),
        });
    }

    if (rest === 'cart/quote' && method === 'POST') {
        const body = await readBody(req);
        const lines = Array.isArray(body.lines) ? body.lines.slice(0, MAX_LINES) : [];
        return json(res, 200, quote(account, lines));
    }

    if (rest === 'orders' && method === 'POST') {
        if (!account.features.order) return apiError(res, 403, 'ORDERING_DISABLED');
        const body = await readBody(req);
        const existing = ORDERS.find(o => o.id === body.requestId && o.accountId === account.id);
        if (existing) return json(res, 201, { order: summary(existing) });
        const lines = Array.isArray(body.lines) ? body.lines : [];
        if (!lines.length || lines.length > MAX_LINES) return apiError(res, 400, 'INVALID_BODY');
        if (lines.some(l => l && l.key === DRIFT.key) && !drifted.has(body.requestId)) {
            // The price moved between the customer's quote and this order (scenario in the header).
            drifted.add(body.requestId);
            for (const no of Object.keys(DRIFT.prices)) {
                if (DRIFT.prices[no] !== null) DRIFT.prices[no] = r2(DRIFT.prices[no] + DRIFT_STEP);
            }
        }
        const q = quote(account, lines);
        if (q.lines.some(l => l.issue)) return apiError(res, 422, 'CART_INVALID', { quote: q });
        if (Math.abs(Number(body.expectedTotal) - q.totals.total) > 0.05) return apiError(res, 409, 'PRICE_CHANGED', { quote: q });
        const open = ORDERS.filter(o => o.accountId === account.id && (o.status === 'NEW' || o.status === 'CLAIMED')).length;
        if (open >= MAX_OPEN_ORDERS) return apiError(res, 429, 'TOO_MANY_OPEN_ORDERS');
        const order = newOrder(account, lines, String(body.note || '').slice(0, 1000), 'NEW', Date.now());
        if (typeof body.requestId === 'string') order.id = body.requestId;
        return json(res, 201, { order: summary(order) });
    }
    if (rest === 'orders' && method === 'GET') {
        // As the server: the customer's requests are listed whether or not ordering is on now.
        return json(res, 200, { items: ORDERS.filter(o => o.accountId === account.id).map(summary) });
    }
    if (rest === 'orders/detail' && method === 'GET') {
        const order = ORDERS.find(o => o.id === query.get('id') && o.accountId === account.id);
        if (!order) return apiError(res, 404, 'NOT_FOUND');
        return json(res, 200, { ...summary(order), note: order.note, lines: order.lines.map(line => { const product = PRODUCTS.find(p => p.key === line.key); return { ...line, thumb: product ? toCProduct(account, product).thumb : null }; }) });
    }

    if (rest === 'statement' && method === 'GET') {
        if (!account.features.statement) return apiError(res, 403, 'FEATURE_DISABLED');
        let balance = 0;
        const rows = INVOICES.slice().reverse().filter(inv => inRange(inv.date, query)).flatMap((inv, i) => {
            const out = [];
            balance = r2(balance + inv.total);
            out.push({ date: inv.date, kind: inv.kind, documentNo: inv.documentNo, debit: Math.max(0, inv.total), credit: Math.max(0, -inv.total), balance });
            if (i % 3 === 2) {
                balance = r2(balance - 500);
                out.push({ date: inv.date, kind: 'collection', documentNo: 'T-' + inv.documentNo.slice(2), debit: 0, credit: 500, balance });
            }
            return out;
        });
        return json(res, 200, { balance: account.balance, rows });
    }
    if (rest === 'invoices' && method === 'GET') {
        if (!account.features.invoices) return apiError(res, 403, 'FEATURE_DISABLED');
        const list = INVOICES.filter(inv => inRange(inv.date, query));
        const { start, pageSize } = paging(query, 20, 50);
        return json(res, 200, {
            items: list.slice(start, start + pageSize).map(inv => ({ key: inv.key, date: inv.date, documentNo: inv.documentNo, kind: inv.kind, total: inv.total })),
            total: list.length,
        });
    }
    if (rest === 'invoices/detail' && method === 'GET') {
        if (!account.features.invoices) return apiError(res, 403, 'FEATURE_DISABLED');
        const inv = INVOICES.find(x => x.key === query.get('key'));
        if (!inv) return apiError(res, 404, 'NOT_FOUND');
        const visible = new Set(visibleProducts(account).map(p => p.key));
        return json(res, 200, {
            key: inv.key, date: inv.date, documentNo: inv.documentNo, kind: inv.kind, total: inv.total,
            lines: inv.lines.map(l => ({
                code: l.code, name: l.name, quantity: l.quantity, unitPrice: l.unitPrice, amount: l.amount,
                productKey: l.product && visible.has(l.product.key) ? l.product.key : null,
            })),
        });
    }
    if (rest === 'purchased' && method === 'GET') {
        if (!account.features.purchased) return apiError(res, 403, 'FEATURE_DISABLED');
        const byCode = new Map();
        for (const inv of INVOICES) {
            if (inv.kind !== 'sale') continue;
            for (const l of inv.lines) {
                const row = byCode.get(l.code) || { code: l.code, name: l.name, lastDate: inv.date, totalQuantity: 0, times: 0, p: l.product };
                row.totalQuantity += l.quantity;
                row.times += 1;
                if (inv.date > row.lastDate) row.lastDate = inv.date;
                byCode.set(l.code, row);
            }
        }
        const q = fold(query.get('q'));
        const visible = new Set(visibleProducts(account).map(p => p.key));
        const list = [...byCode.values()]
            .filter(r => q.length < 2 || fold(r.name).includes(q) || fold(r.code).includes(q))
            .sort((a, b) => b.lastDate.localeCompare(a.lastDate));
        const { start, pageSize } = paging(query, 20, 50);
        return json(res, 200, {
            items: list.slice(start, start + pageSize).map(({ p, ...r }) => ({ ...r, product: p && visible.has(p.key) ? toCProduct(account, p) : null })),
            total: list.length,
        });
    }

    return apiError(res, 404, 'NOT_FOUND');
}

function meOf(account) {
    return {
        companyName: COMPANY,
        code: CODE,
        customer: account.customer,
        username: account.username,
        discountPercent: account.discountPercent,
        priceList: PRICE_LISTS[account.priceListNo],
        features: account.features,
        balance: account.features.statement ? { amount: account.balance } : null,
    };
}

function summary(order) {
    return {
        id: order.id, no: order.no, status: order.status, total: order.total, lineCount: order.lineCount,
        submittedAtMs: order.submittedAtMs, rejectReason: order.rejectReason,
    };
}

// ---------- Server ----------

/** createMockServer({ delayMs, autoLogin }) -> http.Server (not listening yet). */
export function createMockServer(options = {}) {
    const delayMs = options.delayMs ?? 0;
    const autoLogin = options.autoLogin || null;
    return createServer(async (req, res) => {
        try {
            const url = new URL(req.url, 'http://localhost');
            const parts = url.pathname.split('/').filter(Boolean);

            if (parts[0] === 'assets') {
                if (parts[1] !== VERSION || parts.length < 3) return send(res, 404, 'Not found', { 'Cache-Control': 'no-store' });
                return serveFile(res, parts.slice(2).map(decodeURIComponent).join('/'));
            }
            if (url.pathname === '/robots.txt' || url.pathname === '/favicon.svg') return serveFile(res, url.pathname.slice(1));
            if (parts[0] === 'api' && parts[1] === 'v1' && parts[2] === 'catalog') {
                if (delayMs > 0) await new Promise(r => setTimeout(r, delayMs / 2 + Math.random() * delayMs));
                if (parts[3] === 'img' && parts.length === 6) return serveImage(res, parts[4], parts[5]);
                if (parts.length < 5) return apiError(res, 404, 'NOT_FOUND');
                return await handleApi(req, res, parts[3], parts.slice(4).join('/'), url.searchParams, autoLogin);
            }
            if (parts[0] === 'api' || parts[0] === 'health') return send(res, 404, 'Not found');
            if (req.method !== 'GET' && req.method !== 'HEAD') return send(res, 405, 'Method not allowed');
            const code = parts[0] || '';
            return serveShell(res, /^[A-Za-z0-9]{4,16}$/.test(code) && code.toUpperCase() === CODE);
        } catch (err) {
            if (!res.headersSent) apiError(res, 500, 'INTERNAL_ERROR');
            else res.end();
        }
    });
}

const isMain = process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href;
if (isMain) {
    const port = Number(process.env.PORT) || 5173;
    const delayMs = process.env.MOCK_DELAY_MS === undefined ? 250 : Number(process.env.MOCK_DELAY_MS) || 0;
    const server = createMockServer({ delayMs, autoLogin: process.env.MOCK_AUTO_LOGIN || null });
    server.listen(port, '127.0.0.1', () => {
        console.log('Müşteri kataloğu sahte sunucusu: http://localhost:' + port + '/' + CODE + '  (demo / demo1234)');
    });
    const stop = () => server.close(() => process.exit(0));
    process.on('SIGINT', stop);
    process.on('SIGTERM', stop);
}
