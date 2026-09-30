// Pure route table (node --test: tests/katalog-web/route-parse.test.mjs). Paths follow
// GOAL_MUSTERI_KATALOGU §7; identifiers travel in the query string, never in the path.

// Tenant codes are 8 characters today (MobileSeatService.CodeAlphabet); the client only needs to
// tell a code from junk, the server decides whether it exists.
const CODE = /^[A-Za-z0-9]{4,16}$/;

const ROUTES = {
    '': 'catalog',
    'giris': 'login',
    'urun': 'product',
    'sepet': 'cart',
    'siparisler': 'orders',
    'siparisler/detay': 'order',
    'hesap': 'account',
    'hesap/ekstre': 'statement',
    'hesap/faturalar': 'invoices',
    'hesap/fatura': 'invoice',
    'hesap/aldiklarim': 'purchased',
    'hesap/sifre': 'password',
};

const PATHS = {};
for (const rest of Object.keys(ROUTES)) PATHS[ROUTES[rest]] = rest;

export function parseQuery(search) {
    const out = {};
    new URLSearchParams(search || '').forEach((value, key) => {
        if (!(key in out)) out[key] = value;
    });
    return out;
}

/**
 * '/abcd2345/urun', '?kod=X' -> { name: 'product', code: 'ABCD2345', query: { kod: 'X' },
 * canonical: '/ABCD2345/urun?kod=X' }. `canonical` is set only when the address should be rewritten
 * (lower-case code, trailing slash). name: root (no code) | notFound | one of ROUTES.
 */
export function parseLocation(pathname, search) {
    const query = parseQuery(search);
    const parts = String(pathname || '/').split('/').filter(Boolean);
    if (!parts.length) return { name: 'root', code: null, query, canonical: null };
    if (!CODE.test(parts[0])) return { name: 'notFound', code: null, query, canonical: null };

    const code = parts[0].toUpperCase();
    const rest = parts.slice(1).join('/');
    const name = Object.prototype.hasOwnProperty.call(ROUTES, rest) ? ROUTES[rest] : 'notFound';
    const path = '/' + code + (rest ? '/' + rest : '');
    const canonical = path !== pathname ? path + (search || '') : null;
    return { name, code, query, canonical };
}

/** routePath('ABCD2345', 'product', { kod: 'X' }) -> '/ABCD2345/urun?kod=X'. Empty values are left out. */
export function routePath(code, name, query) {
    const rest = Object.prototype.hasOwnProperty.call(PATHS, name) ? PATHS[name] : '';
    const params = new URLSearchParams();
    if (query) {
        for (const key of Object.keys(query)) {
            const value = query[key];
            if (value !== null && value !== undefined && value !== '') params.set(key, String(value));
        }
    }
    const qs = params.toString();
    return '/' + code + (rest ? '/' + rest : '') + (qs ? '?' + qs : '');
}

/**
 * Where to go after signing in. Only a path inside this company's catalogue is accepted (no other
 * origin, no other company, no protocol-relative "//", no login loop); anything else gives null.
 */
export function safeReturnPath(code, value) {
    if (!code || typeof value !== 'string' || value.length > 2000) return null;
    const base = '/' + code;
    if (!(value === base || value.startsWith(base + '/') || value.startsWith(base + '?'))) return null;
    if (value.indexOf('//') >= 0 || value.indexOf('\\') >= 0 || /[\u0000-\u001f\u007f]/.test(value)) return null;
    const cut = value.search(/[?#]/);
    const route = parseLocation(cut < 0 ? value : value.slice(0, cut), '');
    if (route.code !== code || route.name === 'login' || route.name === 'notFound') return null;
    return value;
}

/** The login address that brings the customer back to `returnPath` afterwards. */
export function loginPath(code, returnPath) {
    const back = safeReturnPath(code, returnPath);
    return routePath(code, 'login', back && back !== '/' + code ? { r: back } : null);
}
