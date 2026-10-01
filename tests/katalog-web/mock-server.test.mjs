// The mock server is what the web is checked against until the CentralApi side exists, so it is
// held to the contract here: shell and assets, headers, cookie session, CSRF, pricing and paging.
import test from 'node:test';
import assert from 'node:assert/strict';
import { randomUUID } from 'node:crypto';
import { createMockServer } from './mock-server.mjs';

let server;
let base;

test.before(async () => {
    server = createMockServer({ delayMs: 0 });
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    base = 'http://127.0.0.1:' + server.address().port;
});

test.after(() => new Promise(resolve => server.close(resolve)));

async function login(username, password, extra = {}) {
    const res = await fetch(base + '/api/v1/catalog/DEMO1234/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Katalog': '1', ...extra },
        body: JSON.stringify({ username, password, remember: false }),
    });
    const cookie = (res.headers.get('set-cookie') || '').split(';')[0];
    return { res, cookie, body: res.status === 200 ? await res.json() : await res.json().catch(() => null) };
}

test('shell: known code 200 with version and title filled in, CSP and no-cache', async () => {
    const res = await fetch(base + '/DEMO1234/urun?kod=x');
    const html = await res.text();
    assert.equal(res.status, 200);
    assert.match(res.headers.get('content-type'), /text\/html/);
    assert.equal(res.headers.get('cache-control'), 'no-cache');
    assert.match(res.headers.get('content-security-policy'), /script-src 'self'; style-src 'self'; img-src 'self' https: data:/);
    assert.match(html, /\/assets\/dev\/js\/main\.js/);
    assert.match(html, /<title>Demo Gıda Dağıtım · Müşteri Kataloğu<\/title>/);
    assert.doesNotMatch(html, /%V%|%TITLE%/);
});

test('shell: lower-case code is served, unknown code gets the same HTML with 404', async () => {
    assert.equal((await fetch(base + '/demo1234')).status, 200);
    const unknown = await fetch(base + '/ZZZZ9999/sepet');
    assert.equal(unknown.status, 404);
    assert.match(await unknown.text(), /<title>Müşteri Kataloğu<\/title>/);
});

test('assets under /assets/dev/, other versions and traversal 404', async () => {
    const js = await fetch(base + '/assets/dev/js/main.js');
    assert.equal(js.status, 200);
    assert.match(js.headers.get('content-type'), /text\/javascript/);
    const css = await fetch(base + '/assets/dev/css/tokens.css');
    assert.equal(css.status, 200);
    assert.equal((await fetch(base + '/assets/old/js/main.js')).status, 404);
    assert.equal((await fetch(base + '/assets/dev/..%2F..%2FProgram.cs')).status, 404);
    assert.equal((await fetch(base + '/robots.txt')).status, 200);
});

test('info is anonymous; unknown code 404 CATALOG_NOT_FOUND', async () => {
    const ok = await fetch(base + '/api/v1/catalog/DEMO1234/info');
    assert.deepEqual(await ok.json(), { companyName: 'Demo Gıda Dağıtım', code: 'DEMO1234' });
    const missing = await fetch(base + '/api/v1/catalog/ZZZZ9999/info');
    assert.equal(missing.status, 404);
    assert.equal((await missing.json()).errorCode, 'CATALOG_NOT_FOUND');
});

test('login needs X-Katalog; wrong password 401; inactive 403; me needs the cookie', async () => {
    const noHeader = await fetch(base + '/api/v1/catalog/DEMO1234/login', { method: 'POST', body: '{}' });
    assert.equal(noHeader.status, 403);
    assert.equal((await noHeader.json()).errorCode, 'CSRF_REJECTED');

    assert.equal((await login('demo', 'yanlis')).res.status, 401);
    const inactive = await login('pasif', 'pasif1234');
    assert.equal(inactive.res.status, 403);
    assert.equal(inactive.body.errorCode, 'ACCOUNT_INACTIVE');

    assert.equal((await fetch(base + '/api/v1/catalog/DEMO1234/me')).status, 401);

    const ok = await login('demo', 'demo1234');
    assert.equal(ok.res.status, 200);
    assert.match(ok.res.headers.get('set-cookie'), /HttpOnly; SameSite=Strict/);
    assert.equal(ok.body.me.priceList.includesVat, true);
    const me = await fetch(base + '/api/v1/catalog/DEMO1234/me', { headers: { cookie: ok.cookie } });
    assert.equal(me.status, 200);
    assert.equal(me.headers.get('cache-control'), 'private, no-store');
    assert.equal((await me.json()).features.order, true);
});

test('five wrong passwords throttle the username, even the right one gets 429 + Retry-After', async () => {
    // 'pasif' would answer 403 to the right password; while throttled it must not reveal that.
    for (let i = 0; i < 5; i++) await login('pasif', 'yanlis');
    const blocked = await login('pasif', 'pasif1234');
    assert.equal(blocked.res.status, 429);
    assert.equal(blocked.body.errorCode, 'RATE_LIMITED');
    assert.ok(Number(blocked.res.headers.get('retry-after')) > 0);
});

test('products: 60+ items, paging, pageSize cap, discount and noDiscount prices', async () => {
    const { cookie } = await login('demo', 'demo1234');
    const get = async path => (await fetch(base + '/api/v1/catalog/DEMO1234/' + path, { headers: { cookie } })).json();

    const categories = await get('categories');
    assert.equal(categories.items.length, 6);

    const first = await get('products?page=1&pageSize=48');
    assert.ok(first.total >= 60);
    assert.equal(first.items.length, 48);
    const second = await get('products?page=2&pageSize=48');
    assert.equal(second.items.length, first.total - 48);
    assert.equal((await get('products?pageSize=500')).pageSize, 60);

    const all = first.items.concat(second.items);
    const discounted = all.find(p => p.price.discountPercent === 10);
    assert.equal(discounted.price.net, Math.round(discounted.price.list * 0.9 * 100) / 100);
    assert.ok(all.some(p => p.price.discountPercent === 0), 'noDiscount product');
    assert.ok(all.some(p => p.box && p.box.only), 'carton-only product');
    assert.ok(all.some(p => !p.inStock), 'out-of-stock product');
    assert.ok(all.some(p => p.thumb) && all.some(p => !p.thumb), 'with and without images');

    const search = await get('products?q=' + encodeURIComponent('ışık'));
    assert.equal(search.items.length, 1, 'Turkish-aware search finds IŞIK');

    const detail = await get('products/detail?key=' + all[0].key);
    assert.ok(Array.isArray(detail.images));
    const missing = await fetch(base + '/api/v1/catalog/DEMO1234/products/detail?key=nope', { headers: { cookie } });
    assert.equal(missing.status, 404);
});

test('quote and order: carton multiple 422, stale total 409, same requestId one order', async () => {
    const { cookie } = await login('demo', 'demo1234');
    const post = (path, body) => fetch(base + '/api/v1/catalog/DEMO1234/' + path, {
        method: 'POST',
        headers: { cookie, 'Content-Type': 'application/json', 'X-Katalog': '1' },
        body: JSON.stringify(body),
    });
    const list = await (await fetch(base + '/api/v1/catalog/DEMO1234/products?pageSize=60', { headers: { cookie } })).json();
    const cartonOnly = list.items.find(p => p.box && p.box.only && p.inStock);
    const plain = list.items.find(p => !p.box && p.inStock);

    const bad = await (await post('cart/quote', { lines: [{ key: cartonOnly.key, quantity: cartonOnly.box.qty + 1 }] })).json();
    assert.equal(bad.lines[0].issue, 'CARTON_MULTIPLE');

    const q = await (await post('cart/quote', { lines: [{ key: plain.key, quantity: 3 }] })).json();
    assert.equal(q.lines[0].issue, null);
    assert.ok(q.totals.total > 0);

    const stale = await post('orders', { requestId: 'r-1', lines: [{ key: plain.key, quantity: 3 }], note: '', expectedTotal: q.totals.total + 0.06 });
    assert.equal(stale.status, 409);
    assert.equal((await stale.json()).errorCode, 'PRICE_CHANGED');

    const body = { requestId: 'r-2', lines: [{ key: plain.key, quantity: 3 }], note: '', expectedTotal: q.totals.total };
    const created = await (await post('orders', body)).json();
    const again = await (await post('orders', body)).json();
    assert.equal(created.order.id, again.order.id);
    assert.equal(created.order.status, 'NEW');
});

test('features off: account pages 403 FEATURE_DISABLED, ordering 403 ORDERING_DISABLED, own requests still listed', async () => {
    const tek = await login('tek', 'tek12345');
    assert.equal(tek.body.me.features.order, false);
    assert.equal(tek.body.me.priceList.includesVat, false);
    assert.equal(tek.body.me.balance, null);
    const statement = await fetch(base + '/api/v1/catalog/DEMO1234/statement', { headers: { cookie: tek.cookie } });
    assert.equal(statement.status, 403);
    assert.equal((await statement.json()).errorCode, 'FEATURE_DISABLED');
    const order = await fetch(base + '/api/v1/catalog/DEMO1234/orders', {
        method: 'POST',
        headers: { cookie: tek.cookie, 'Content-Type': 'application/json', 'X-Katalog': '1' },
        body: JSON.stringify({ requestId: 'r-9', lines: [], note: '', expectedTotal: 0 }),
    });
    assert.equal(order.status, 403);
    assert.equal((await order.json()).errorCode, 'ORDERING_DISABLED');
    const listed = await fetch(base + '/api/v1/catalog/DEMO1234/orders', { headers: { cookie: tek.cookie } });
    assert.equal(listed.status, 200, 'GET orders does not depend on CanOrder (the server lists them too)');
    assert.ok(Array.isArray((await listed.json()).items));

    const demo = await login('demo', 'demo1234');
    const rows = await fetch(base + '/api/v1/catalog/DEMO1234/statement', { headers: { cookie: demo.cookie } });
    assert.equal(rows.status, 200);
    assert.ok(Array.isArray((await rows.json()).rows));
});

test('logout ends the session', async () => {
    const { cookie } = await login('demo', 'demo1234');
    const out = await fetch(base + '/api/v1/catalog/DEMO1234/logout', { method: 'POST', headers: { cookie, 'X-Katalog': '1' } });
    assert.equal(out.status, 204);
    assert.equal((await fetch(base + '/api/v1/catalog/DEMO1234/me', { headers: { cookie } })).status, 401);
});

function client(cookie) {
    const url = path => base + '/api/v1/catalog/DEMO1234/' + path;
    return {
        get: async path => (await fetch(url(path), { headers: { cookie } })).json(),
        raw: path => fetch(url(path), { headers: { cookie } }),
        post: (path, body) => fetch(url(path), {
            method: 'POST',
            headers: { cookie, 'Content-Type': 'application/json', 'X-Katalog': '1' },
            body: JSON.stringify(body),
        }),
    };
}

test('price drift scenario: 409 PRICE_CHANGED with the new quote, the same requestId then goes through', async () => {
    const demo = client((await login('demo', 'demo1234')).cookie);
    const coffee = (await demo.get('products?q=' + encodeURIComponent('türk kahvesi'))).items[0];
    assert.equal(coffee.name, 'Türk Kahvesi 100 g');
    const lines = [{ key: coffee.key, quantity: 2 }];
    const before = await (await demo.post('cart/quote', { lines })).json();

    const requestId = randomUUID();
    const first = await demo.post('orders', { requestId, lines, note: null, expectedTotal: before.totals.total });
    assert.equal(first.status, 409);
    const conflict = await first.json();
    assert.equal(conflict.errorCode, 'PRICE_CHANGED');
    assert.ok(conflict.quote.totals.total > before.totals.total, 'the quote in the 409 carries the new price');
    assert.equal(conflict.quote.lines[0].issue, null);

    const second = await demo.post('orders', { requestId, lines, note: 'Kapıya bırakın', expectedTotal: conflict.quote.totals.total });
    assert.equal(second.status, 201);
    const order = (await second.json()).order;
    assert.equal(order.id, requestId);
    assert.match(order.no, /^KT-\d{6}$/);
    assert.equal(order.status, 'NEW');

    const list = await demo.get('orders');
    assert.equal(list.items[0].id, requestId, 'newest first');
    assert.deepEqual(Object.keys(list.items[0]).sort(),
        ['id', 'lineCount', 'no', 'rejectReason', 'status', 'submittedAtMs', 'total'], 'COrder fields of §5.2');
    assert.ok(list.items.some(o => o.status === 'REJECTED' && o.rejectReason));

    const detail = await demo.get('orders/detail?id=' + requestId);
    assert.equal(detail.note, 'Kapıya bırakın');
    assert.deepEqual(Object.keys(detail.lines[0]).sort(), ['code', 'key', 'name', 'net', 'quantity', 'thumb', 'total']);
    assert.equal(detail.lines[0].quantity, 2);

    const tek = client((await login('tek', 'tek12345')).cookie);
    assert.equal((await tek.raw('orders/detail?id=' + requestId)).status, 404, 'another account never sees it');
});

test('invoices, invoice detail, purchased and statement: discontinued products cannot be added', async () => {
    const demo = client((await login('demo', 'demo1234')).cookie);
    const invoices = await demo.get('invoices?page=1');
    assert.equal(invoices.total, 14);
    assert.deepEqual(Object.keys(invoices.items[0]).sort(), ['date', 'documentNo', 'key', 'kind', 'total']);

    const detail = await demo.get('invoices/detail?key=' + invoices.items[1].key);
    assert.ok(detail.lines.some(l => l.code === '35999' && l.productKey === null), 'discontinued line');
    assert.ok(detail.lines.some(l => typeof l.productKey === 'string'), 'catalogue line');
    assert.equal((await demo.raw('invoices/detail?key=nope')).status, 404);

    const purchased = await demo.get('purchased');
    assert.ok(purchased.items.some(i => i.product === null));
    assert.ok(purchased.items.some(i => i.product && i.product.price && i.product.key));
    const old = await demo.get('purchased?q=' + encodeURIComponent('eski ambalaj'));
    assert.equal(old.items.length, 1);
    assert.equal(old.items[0].product, null);

    const statement = await demo.get('statement?from=2000-01-01&to=2100-12-31');
    assert.ok(statement.rows.length > 0);
    for (const key of ['date', 'kind', 'documentNo', 'debit', 'credit', 'balance']) assert.ok(key in statement.rows[0], key);
    assert.equal((await demo.get('statement?from=2000-01-01&to=2000-01-31')).rows.length, 0, 'an empty range');
});

test('password: wrong current, weak (in bytes), success signs other browsers out', async () => {
    const mine = await login('demo', 'demo1234');
    const other = await login('demo', 'demo1234');
    const demo = client(mine.cookie);

    const wrong = await demo.post('password', { current: 'yanlis', next: 'yeni-sifre-1' });
    assert.equal(wrong.status, 400);
    assert.equal((await wrong.json()).errorCode, 'INVALID_CREDENTIALS');
    const weak = await demo.post('password', { current: 'demo1234', next: 'kisa' });
    assert.equal((await weak.json()).errorCode, 'INVALID_PASSWORD');
    const long = await demo.post('password', { current: 'demo1234', next: 'ş'.repeat(37) });
    assert.equal(long.status, 400, '37 characters but 74 bytes');

    assert.equal((await demo.post('password', { current: 'demo1234', next: 'yeni-sifre-1' })).status, 204);
    assert.equal((await demo.raw('me')).status, 200, 'this browser stays signed in');
    assert.equal((await client(other.cookie).raw('me')).status, 401, 'every other one is out');

    // Put it back for anything that signs in as demo later.
    assert.equal((await demo.post('password', { current: 'yeni-sifre-1', next: 'demo1234' })).status, 204);
});


test('advanced filters and sorting cover the complete catalogue before paging', async () => {
    const { cookie } = await login('demo', 'demo1234');
    const get = async query => (await fetch(base + '/api/v1/catalog/DEMO1234/products?' + query, { headers: { Cookie: cookie } })).json();
    const all = await get('sort=price-asc&pageSize=60');
    assert.ok(all.total > 48);
    assert.ok(all.brands.length > 1);
    assert.ok(all.items.every((p, i) => !i || all.items[i - 1].price.net <= p.price.net));
    const page = await get('sort=price-asc&pageSize=1&page=2');
    assert.equal(page.items[0].key, all.items[1].key);
    const filtered = await get('stock=in&discounted=true&minPrice=30&maxPrice=250&hasImage=true');
    assert.ok(filtered.items.length > 0);
    assert.ok(filtered.items.every(p => p.inStock && p.thumb && p.price.discountPercent > 0 && p.price.net >= 30 && p.price.net <= 250));
    const orders = await (await fetch(base + '/api/v1/catalog/DEMO1234/orders', { headers: { Cookie: cookie } })).json();
    const detail = await (await fetch(base + '/api/v1/catalog/DEMO1234/orders/detail?id=' + orders.items[0].id, { headers: { Cookie: cookie } })).json();
    assert.ok(detail.lines.every(l => 'thumb' in l));
});
