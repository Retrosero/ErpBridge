import test from 'node:test';
import assert from 'node:assert/strict';
import {
    cartSignature, createRequestIds, issueCount, newRequestId, orderStatus, quoteLinesByKey, refreshLines, requestLines,
    singleFlight,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/order.js';
import { setQty } from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/cart.js';

const UUID_V4 = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;

const simit = {
    key: 'a', code: '1', name: 'Simit', unit: 'ADET', box: null,
    price: { list: 10, net: 9, discountPercent: 10, includesVat: true }, inStock: true, thumb: '/img/a',
};
const kola = {
    key: 'b', code: '2', name: 'Kola', unit: 'ADET', box: { qty: 24, only: true },
    price: { list: 20, net: 18, discountPercent: 10, includesVat: true }, inStock: true, thumb: null,
};

function cartOf(...pairs) {
    let lines = [];
    for (const [product, qty] of pairs) lines = setQty(lines, product, qty).lines;
    return lines;
}

function quoteLine(product, quantity, overrides = {}) {
    return {
        key: product.key, code: product.code, name: product.name, unit: product.unit, quantity, box: product.box,
        price: product.price, vatRate: 10, gross: 0, discount: 0, vat: 0, total: 0, issue: null, ...overrides,
    };
}

test('requestLines and cartSignature carry only key and quantity', () => {
    const lines = cartOf([simit, 3], [kola, 48]);
    assert.deepEqual(requestLines(lines), [{ key: 'a', quantity: 3 }, { key: 'b', quantity: 48 }]);
    assert.equal(cartSignature(lines), 'a*3|b*48');
    assert.equal(cartSignature([]), '');
    assert.notEqual(cartSignature(cartOf([simit, 4], [kola, 48])), cartSignature(lines));
});

test('quoteLinesByKey and issueCount', () => {
    const quote = { lines: [quoteLine(simit, 3), quoteLine(kola, 30, { issue: 'CARTON_MULTIPLE' }), null], totals: {} };
    assert.equal(quoteLinesByKey(quote).get('b').issue, 'CARTON_MULTIPLE');
    assert.equal(quoteLinesByKey(null).size, 0);
    assert.equal(issueCount(quote), 1);
    assert.equal(issueCount(null), 0);
});

test('refreshLines: same figures keep the same array', () => {
    const lines = cartOf([simit, 3], [kola, 48]);
    const quote = { lines: [quoteLine(simit, 3), quoteLine(kola, 48)] };
    const result = refreshLines(lines, quote);
    assert.equal(result.lines, lines);
    assert.equal(result.priceChanged, 0);
});

test('refreshLines: a moved price is taken and counted, quantities never change', () => {
    const lines = cartOf([simit, 3], [kola, 48]);
    const newPrice = { list: 11, net: 9.9, discountPercent: 10, includesVat: true };
    const result = refreshLines(lines, { lines: [quoteLine(simit, 3, { price: newPrice }), quoteLine(kola, 48)] });
    assert.notEqual(result.lines, lines);
    assert.equal(result.priceChanged, 1);
    assert.deepEqual(result.lines[0].product.price, newPrice);
    assert.equal(result.lines[0].product.thumb, '/img/a', 'the thumbnail is kept from the snapshot');
    assert.equal(result.lines[0].quantity, 3);
    assert.equal(result.lines[1], lines[1], 'an unchanged line keeps its object');
});

test('refreshLines: carton and stock from the server; NOT_AVAILABLE keeps the snapshot', () => {
    const lines = cartOf([simit, 5], [kola, 24]);
    const result = refreshLines(lines, {
        lines: [
            quoteLine(simit, 5, { box: { qty: 6, only: true }, issue: 'CARTON_MULTIPLE' }),
            quoteLine(kola, 24, { issue: 'NOT_AVAILABLE', price: null, name: null }),
        ],
    });
    assert.deepEqual(result.lines[0].product.box, { qty: 6, only: true });
    assert.equal(result.lines[0].quantity, 5);
    assert.equal(result.lines[1], lines[1]);
    assert.equal(result.priceChanged, 0);

    const out = refreshLines(lines, { lines: [quoteLine(simit, 5, { issue: 'OUT_OF_STOCK' }), quoteLine(kola, 24)] });
    assert.equal(out.lines[0].product.inStock, false);
});

test('createRequestIds: one id per cart content until forgotten', () => {
    let n = 0;
    const ids = createRequestIds(() => 'id-' + (++n));
    assert.equal(ids.idFor('a*3'), 'id-1');
    assert.equal(ids.idFor('a*3'), 'id-1', 'a retry of the same content keeps the id');
    assert.equal(ids.idFor('a*4'), 'id-2', 'changed content is another request');
    assert.equal(ids.idFor('a*3'), 'id-3');
    ids.forget();
    assert.equal(ids.idFor('a*3'), 'id-4', 'after an accepted order the same content is a new request');
});

test('singleFlight: a second call while running shares the first; afterwards a new one starts', async () => {
    let calls = 0;
    let release;
    const run = singleFlight(() => {
        calls += 1;
        return new Promise(resolve => { release = resolve; });
    });
    const first = run();
    const second = run();
    assert.equal(first, second);
    assert.equal(run.busy(), true);
    release('ok');
    assert.equal(await first, 'ok');
    assert.equal(calls, 1);
    assert.equal(run.busy(), false);

    const failing = singleFlight(async () => {
        calls += 1;
        throw new Error('boom');
    });
    await assert.rejects(failing(), /boom/);
    assert.equal(failing.busy(), false, 'a failure frees it too');
});

test('newRequestId: randomUUID when present, a valid v4 from getRandomValues otherwise', () => {
    assert.match(newRequestId(), UUID_V4);
    assert.equal(newRequestId({ randomUUID: () => 'from-browser' }), 'from-browser');
    const fallback = newRequestId({ getRandomValues: bytes => bytes.fill(0xff) });
    assert.match(fallback, UUID_V4);
    assert.equal(fallback, 'ffffffff-ffff-4fff-bfff-ffffffffffff');
});

test('orderStatus: the customer labels of §5.3, unknown stays readable', () => {
    assert.equal(orderStatus('NEW').label, 'Alındı');
    assert.equal(orderStatus('CLAIMED').label, 'İnceleniyor');
    assert.equal(orderStatus('COMPLETED').label, 'Siparişe çevrildi');
    assert.equal(orderStatus('REJECTED').label, 'Reddedildi');
    assert.equal(orderStatus('REJECTED').tone, 'danger');
    assert.equal(orderStatus('toString').label, 'Durumu bilinmiyor');
    assert.equal(orderStatus(undefined).tone, 'neutral');
});
