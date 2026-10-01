import test from 'node:test';
import assert from 'node:assert/strict';
import {
    loginPath, parseLocation, parseQuery, routePath, safeReturnPath,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/route-parse.js';

test('every §7 route resolves', () => {
    const cases = {
        '/ABCD2345': 'catalog',
        '/ABCD2345/giris': 'login',
        '/ABCD2345/urun': 'product',
        '/ABCD2345/sepet': 'cart',
        '/ABCD2345/siparisler': 'orders',
        '/ABCD2345/siparisler/detay': 'order',
        '/ABCD2345/hesap': 'account',
        '/ABCD2345/hesap/ekstre': 'statement',
        '/ABCD2345/hesap/faturalar': 'invoices',
        '/ABCD2345/hesap/fatura': 'invoice',
        '/ABCD2345/hesap/aldiklarim': 'purchased',
        '/ABCD2345/hesap/sifre': 'password',
    };
    for (const [path, name] of Object.entries(cases)) {
        const route = parseLocation(path, '');
        assert.equal(route.name, name, path);
        assert.equal(route.code, 'ABCD2345');
        assert.equal(route.canonical, null, path);
    }
});

test('lower-case code and trailing slash get a canonical address', () => {
    const r = parseLocation('/abcd2345/urun/', '?kod=X1');
    assert.equal(r.code, 'ABCD2345');
    assert.equal(r.name, 'product');
    assert.equal(r.canonical, '/ABCD2345/urun?kod=X1');
    assert.deepEqual(r.query, { kod: 'X1' });
    assert.equal(parseLocation('/ABCD2345/', '').canonical, '/ABCD2345');
});

test('root, junk and unknown sub-paths', () => {
    assert.equal(parseLocation('/', '').name, 'root');
    assert.equal(parseLocation('', '').name, 'root');
    assert.equal(parseLocation('/a-b', '').name, 'notFound');
    assert.equal(parseLocation('/ABC', '').name, 'notFound');
    assert.equal(parseLocation('/ABCD2345/urun/123', '').name, 'notFound');
    assert.equal(parseLocation('/ABCD2345/constructor', '').name, 'notFound');
    assert.equal(parseLocation('/ABCD2345/giris/../sepet', '').name, 'notFound');
});

test('parseQuery keeps the first value', () => {
    assert.deepEqual(parseQuery('?q=%C4%B1%C5%9F%C4%B1k&q=b&kategori=abc'), { q: 'ışık', kategori: 'abc' });
    assert.deepEqual(parseQuery(''), {});
});

test('routePath builds addresses and drops empty values', () => {
    assert.equal(routePath('ABCD2345', 'catalog'), '/ABCD2345');
    assert.equal(routePath('ABCD2345', 'catalog', { q: '', kategori: null }), '/ABCD2345');
    assert.equal(routePath('ABCD2345', 'catalog', { q: 'su 5 l', kategori: 'ab12' }), '/ABCD2345?q=su+5+l&kategori=ab12');
    assert.equal(routePath('ABCD2345', 'product', { kod: 'k&1' }), '/ABCD2345/urun?kod=k%261');
    assert.equal(routePath('ABCD2345', 'invoice', { key: 'r1' }), '/ABCD2345/hesap/fatura?key=r1');
    for (const name of ['login', 'cart', 'orders', 'order', 'account', 'statement', 'invoices', 'purchased', 'password']) {
        assert.equal(parseLocation(routePath('ABCD2345', name), '').name, name);
    }
});

test('safeReturnPath accepts only this catalogue', () => {
    const code = 'ABCD2345';
    assert.equal(safeReturnPath(code, '/ABCD2345'), '/ABCD2345');
    assert.equal(safeReturnPath(code, '/ABCD2345?q=su'), '/ABCD2345?q=su');
    assert.equal(safeReturnPath(code, '/ABCD2345/urun?kod=X'), '/ABCD2345/urun?kod=X');
    assert.equal(safeReturnPath(code, '/ABCD2345/hesap/fatura?key=r1'), '/ABCD2345/hesap/fatura?key=r1');

    const rejected = [
        'https://evil.example/ABCD2345/',
        '//evil.example/ABCD2345/',
        '/ABCD2345//evil.example',
        '/ABCD23456/sepet',
        '/WXYZ2345/sepet',
        '/abcd2345/sepet',
        '/ABCD2345/giris?r=/ABCD2345',
        '/ABCD2345/bilinmeyen',
        '/ABCD2345\\evil',
        '/ABCD2345/sepet\nx',
        'ABCD2345/sepet',
        '',
        null,
        42,
    ];
    for (const bad of rejected) assert.equal(safeReturnPath(code, bad), null, String(bad));
    assert.equal(safeReturnPath(null, '/ABCD2345'), null);
});

test('loginPath carries a safe return path only', () => {
    assert.equal(loginPath('ABCD2345', '/ABCD2345/sepet'), '/ABCD2345/giris?r=%2FABCD2345%2Fsepet');
    assert.equal(loginPath('ABCD2345', '/ABCD2345'), '/ABCD2345/giris');
    assert.equal(loginPath('ABCD2345', '//evil.example'), '/ABCD2345/giris');
    assert.equal(loginPath('ABCD2345'), '/ABCD2345/giris');
});
