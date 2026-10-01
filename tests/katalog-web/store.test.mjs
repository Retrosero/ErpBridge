import test from 'node:test';
import assert from 'node:assert/strict';
import {
    bindCart, cartKey, createStore, loadCart, safeStorage, saveCart,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/store.js';
import { setQty } from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/cart.js';

function memoryStorage() {
    const map = new Map();
    return {
        map,
        getItem: k => (map.has(k) ? map.get(k) : null),
        setItem: (k, v) => { map.set(k, String(v)); },
        removeItem: k => { map.delete(k); },
    };
}

const throwingStorage = {
    getItem() { throw new Error('SecurityError'); },
    setItem() { throw new Error('QuotaExceededError'); },
    removeItem() { throw new Error('SecurityError'); },
};

function fakeWindow() {
    const listeners = [];
    return {
        addEventListener: (type, fn) => { if (type === 'storage') listeners.push(fn); },
        removeEventListener: (type, fn) => { const i = listeners.indexOf(fn); if (i >= 0) listeners.splice(i, 1); },
        fire: e => listeners.slice().forEach(fn => fn(e)),
        listeners,
    };
}

const product = { key: 'p1', code: '1', name: 'Simit', unit: 'ADET', box: null, price: { list: 5, net: 5, discountPercent: 0, includesVat: true }, inStock: true };

test('store: get / set / subscribe / unsubscribe', () => {
    const store = createStore({ n: 1 });
    const seen = [];
    const off = store.subscribe('n', v => seen.push(v));
    store.set('n', 2);
    store.set('n', 2); // same value: no event
    off();
    store.set('n', 3);
    assert.deepEqual(seen, [2]);
    assert.equal(store.get('n'), 3);
});

test('cartKey separates company and user', () => {
    assert.equal(cartKey('ABCD2345', 'Demo'), 'katalog:v1:ABCD2345:demo:sepet');
    assert.notEqual(cartKey('ABCD2345', 'demo'), cartKey('WXYZ2345', 'demo'));
});

test('save / load round trip; an empty cart removes the key', () => {
    const storage = memoryStorage();
    const key = cartKey('ABCD2345', 'demo');
    const lines = setQty([], product, 3).lines;
    assert.equal(saveCart(storage, key, lines), true);
    assert.deepEqual(loadCart(storage, key), lines);
    saveCart(storage, key, []);
    assert.equal(storage.map.has(key), false);
});

test('broken or blocked storage never throws', () => {
    const key = cartKey('ABCD2345', 'demo');
    assert.deepEqual(loadCart(throwingStorage, key), []);
    assert.equal(saveCart(throwingStorage, key, setQty([], product, 1).lines), false);
    assert.deepEqual(loadCart(null, key), []);
    assert.equal(saveCart(null, key, []), false);

    const storage = memoryStorage();
    storage.setItem(key, '{not json');
    assert.deepEqual(loadCart(storage, key), []);

    const blocked = {};
    Object.defineProperty(blocked, 'localStorage', { get() { throw new Error('SecurityError'); } });
    assert.equal(safeStorage(blocked), null);
    assert.equal(safeStorage(null), null);
});

test('bindCart loads, persists changes and follows other tabs', () => {
    const storage = memoryStorage();
    const win = fakeWindow();
    const key = cartKey('ABCD2345', 'demo');
    storage.setItem(key, JSON.stringify(setQty([], product, 2).lines));

    const store = createStore({ cart: [] });
    const unbind = bindCart(store, storage, key, win);
    assert.equal(store.get('cart')[0].quantity, 2);

    store.set('cart', setQty(store.get('cart'), product, 5).lines);
    assert.equal(JSON.parse(storage.getItem(key))[0].quantity, 5);

    // Another tab wrote 7.
    let writes = 0;
    const setItem = storage.setItem;
    storage.setItem = (k, v) => { writes += 1; setItem(k, v); };
    storage.map.set(key, JSON.stringify(setQty([], product, 7).lines));
    win.fire({ key });
    assert.equal(store.get('cart')[0].quantity, 7);
    assert.equal(writes, 0, 'a value read from another tab is not written back');

    win.fire({ key: 'something-else' });
    assert.equal(store.get('cart')[0].quantity, 7);

    unbind();
    assert.equal(win.listeners.length, 0);
    store.set('cart', []);
    assert.equal(JSON.parse(storage.getItem(key))[0].quantity, 7, 'unbound store no longer saves');
});

test('bindCart works in memory when storage is blocked', () => {
    const store = createStore({ cart: [] });
    bindCart(store, throwingStorage, 'k', null);
    store.set('cart', setQty([], product, 1).lines);
    assert.equal(store.get('cart').length, 1);
});
