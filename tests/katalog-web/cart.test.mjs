import test from 'node:test';
import assert from 'node:assert/strict';
import {
    MAX_LINES, MAX_QTY, addCarton, boxOf, decrease, increase, lineQty, normalizeQty, parseLines, qtyLabel,
    removeLine, setQty, stepOf, unitText,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/cart.js';

const plain = {
    key: 'a', code: '1', name: 'Simit', unit: 'ADET', box: null,
    price: { list: 10, net: 9, discountPercent: 10, includesVat: true }, inStock: true,
};
const cartonOnly = { key: 'b', code: '2', name: 'Kola', unit: 'ADET', box: { qty: 24, only: true }, inStock: true };
const cartonOptional = { key: 'c', code: '3', name: 'Su', unit: '', box: { qty: 6, only: false }, inStock: true };

test('boxOf ignores cartons below 2 and non-integers', () => {
    assert.equal(boxOf(plain), null);
    assert.deepEqual(boxOf(cartonOnly), { qty: 24, only: true });
    assert.equal(boxOf({ box: { qty: 1, only: true } }), null);
    assert.equal(boxOf({ box: { qty: 2.5, only: true } }), null);
    assert.equal(boxOf(null), null);
});

test('stepOf: one carton for carton-only, one piece otherwise', () => {
    assert.equal(stepOf(plain), 1);
    assert.equal(stepOf(cartonOnly), 24);
    assert.equal(stepOf(cartonOptional), 1);
});

test('normalizeQty rounds carton-only UP to the carton', () => {
    assert.equal(normalizeQty(cartonOnly, 1), 24);
    assert.equal(normalizeQty(cartonOnly, 24), 24);
    assert.equal(normalizeQty(cartonOnly, 25), 48);
    assert.equal(normalizeQty(cartonOnly, 47.2), 48);
    assert.equal(normalizeQty(cartonOptional, 7), 7);
});

test('normalizeQty: whole numbers, 0 removes, cap 9999 (down to a carton there)', () => {
    assert.equal(normalizeQty(plain, 2.1), 3);
    assert.equal(normalizeQty(plain, 0), 0);
    assert.equal(normalizeQty(plain, -5), 0);
    assert.equal(normalizeQty(plain, 'abc'), 0);
    assert.equal(normalizeQty(plain, NaN), 0);
    assert.equal(normalizeQty(plain, MAX_QTY + 1), MAX_QTY);
    assert.equal(normalizeQty(plain, MAX_QTY), MAX_QTY);
    assert.equal(normalizeQty(cartonOnly, 20000), 9984);
    assert.equal(9984 % 24, 0);
});

test('increase / decrease / addCarton', () => {
    assert.equal(increase(plain, 0), 1);
    assert.equal(increase(plain, 4), 5);
    assert.equal(increase(plain, MAX_QTY), MAX_QTY);
    assert.equal(increase(cartonOnly, 0), 24);
    assert.equal(increase(cartonOnly, 24), 48);
    assert.equal(increase(cartonOnly, 9984), 9984);
    assert.equal(decrease(cartonOnly, 48), 24);
    assert.equal(decrease(cartonOnly, 24), 0);
    assert.equal(decrease(plain, 1), 0);
    assert.equal(addCarton(cartonOptional, 2), 8);
    assert.equal(addCarton(plain, 2), 3);
    assert.equal(addCarton(cartonOnly, 24), 48);
});

test('qtyLabel and unitText', () => {
    assert.equal(unitText(plain), 'adet');
    assert.equal(unitText(cartonOptional), 'adet');
    assert.equal(unitText({ unit: 'PAKET' }), 'paket');
    assert.equal(qtyLabel(cartonOnly, 48), '2 koli · 48 adet');
    assert.equal(qtyLabel(plain, 5), '5 adet');
    assert.equal(qtyLabel(plain, 1200), '1.200 adet');
    assert.equal(qtyLabel(cartonOptional, 12), '12 adet');
    assert.equal(qtyLabel(cartonOnly, 26), '26 adet', 'not a whole number of cartons: pieces only');
});

test('setQty adds, updates, removes and keeps a snapshot', () => {
    let r = setQty([], plain, 2);
    assert.equal(r.quantity, 2);
    assert.equal(r.lines.length, 1);
    assert.equal(r.lines[0].product.name, 'Simit');
    assert.deepEqual(r.lines[0].product.price, plain.price);

    const same = setQty(r.lines, plain, 2);
    assert.equal(same.lines, r.lines, 'unchanged quantity keeps the same array');

    r = setQty(r.lines, cartonOnly, 30);
    assert.equal(r.quantity, 48);
    assert.equal(lineQty(r.lines, 'b'), 48);

    r = setQty(r.lines, plain, 0);
    assert.equal(lineQty(r.lines, 'a'), 0);
    assert.equal(r.lines.length, 1);
    assert.deepEqual(removeLine(r.lines, 'b'), []);
    assert.equal(removeLine(r.lines, 'zzz'), r.lines);
});

test('setQty refuses a new line past MAX_LINES but still updates existing ones', () => {
    const lines = Array.from({ length: MAX_LINES }, (_, i) => ({ key: 'k' + i, quantity: 1, product: { key: 'k' + i, name: 'x' } }));
    const refused = setQty(lines, plain, 1);
    assert.equal(refused.full, true);
    assert.equal(refused.lines, lines);
    const updated = setQty(lines, { key: 'k3', name: 'x' }, 5);
    assert.equal(updated.full, false);
    assert.equal(lineQty(updated.lines, 'k3'), 5);
});

test('parseLines drops malformed rows and re-normalises quantities', () => {
    const good = setQty([], cartonOnly, 24).lines[0];
    const parsed = parseLines([
        good,
        { ...good }, // duplicate key
        { key: 'x', quantity: 3 }, // no product
        { key: 'y', quantity: 3, product: { key: 'other', name: 'Y' } },
        { key: 'z', quantity: 0, product: { key: 'z', name: 'Z' } },
        { key: 'w', quantity: 30, product: { key: 'w', name: 'W', box: { qty: 24, only: true } } },
        null,
        'text',
    ]);
    assert.deepEqual(parsed.map(l => [l.key, l.quantity]), [['b', 24], ['w', 48]]);
    assert.deepEqual(parseLines({ not: 'an array' }), []);
    const many = Array.from({ length: MAX_LINES + 5 }, (_, i) => ({ key: 'k' + i, quantity: 1, product: { key: 'k' + i, name: 'n' } }));
    assert.equal(parseLines(many).length, MAX_LINES);
});
