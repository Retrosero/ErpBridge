// Small observable state (me, cart, online) and the cart's persistence. The cart is kept in
// localStorage per company code and signed-in user, so two catalogues or two customers sharing a
// browser never see each other's cart; other tabs follow through the `storage` event.
// Storage may be missing or throw (private mode, blocked site data): the cart then lives in memory.

import { parseLines } from './cart.js';

export function createStore(initial) {
    const state = Object.assign({}, initial);
    const listeners = new Map();
    return {
        get: key => state[key],
        set(key, value) {
            if (state[key] === value) return;
            state[key] = value;
            for (const fn of (listeners.get(key) || []).slice()) fn(value);
        },
        subscribe(key, fn) {
            if (!listeners.has(key)) listeners.set(key, []);
            listeners.get(key).push(fn);
            return () => {
                const list = listeners.get(key) || [];
                const i = list.indexOf(fn);
                if (i >= 0) list.splice(i, 1);
            };
        },
    };
}

export function cartKey(code, username) {
    return 'katalog:v1:' + code + ':' + String(username || '').toLowerCase() + ':sepet';
}

/** window.localStorage, or null where even touching it throws. */
export function safeStorage(win) {
    try {
        return win && win.localStorage ? win.localStorage : null;
    } catch (e) {
        return null;
    }
}

export function loadCart(storage, key) {
    try {
        const raw = storage ? storage.getItem(key) : null;
        return raw ? parseLines(JSON.parse(raw)) : [];
    } catch (e) {
        return [];
    }
}

/** false when the write failed (quota, blocked); the in-memory cart still works for this tab. */
export function saveCart(storage, key, lines) {
    if (!storage) return false;
    try {
        if (lines.length) storage.setItem(key, JSON.stringify(lines));
        else storage.removeItem(key);
        return true;
    } catch (e) {
        return false;
    }
}

/**
 * Loads the cart into store.cart, saves every change and follows other tabs. Returns an unbind
 * function (called when the signed-in user changes).
 */
export function bindCart(store, storage, key, win) {
    // Values that came from storage are not written straight back.
    let external = true;
    store.set('cart', loadCart(storage, key));
    external = false;

    const unsubscribe = store.subscribe('cart', lines => {
        if (!external) saveCart(storage, key, lines);
    });
    const onStorage = e => {
        if (e.key !== key && e.key !== null) return;
        external = true;
        store.set('cart', loadCart(storage, key));
        external = false;
    };
    if (win) win.addEventListener('storage', onStorage);
    return () => {
        unsubscribe();
        if (win) win.removeEventListener('storage', onStorage);
    };
}
