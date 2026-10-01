// Banner strip logic (GOAL_MUSTERI_KATALOGU S12/W6): which banners are drawn and where a click goes.
import test from 'node:test';
import assert from 'node:assert/strict';
import { bannerTarget, isHttpsUrl, slideIndex, usableBanners } from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/banners.js';

test('a category link switches the list and stays a real catalogue address', () => {
    assert.deepEqual(bannerTarget('ABCD2345', { type: 'category', value: 'Çay', categoryId: '0123456789ab' }),
        { kind: 'category', id: '0123456789ab', href: '/ABCD2345?kategori=0123456789ab' });
});

test('a product link opens the product dialog by its key', () => {
    assert.deepEqual(bannerTarget('ABCD2345', { type: 'product', value: 'A B', productKey: 'A B' }),
        { kind: 'product', href: '/ABCD2345/urun?kod=A+B' });
});

test('only an https address becomes an outside link', () => {
    assert.deepEqual(bannerTarget('ABCD2345', { type: 'url', value: 'https://ornek.com/kampanya' }),
        { kind: 'url', href: 'https://ornek.com/kampanya' });
    for (const value of ['http://ornek.com', 'javascript:alert(1)', '//ornek.com', '/ABCD2345/sepet', '', null]) {
        assert.equal(bannerTarget('ABCD2345', { type: 'url', value }), null, String(value));
    }
    assert.equal(isHttpsUrl('https://ornek.com'), true);
    assert.equal(isHttpsUrl('data:image/png;base64,AA'), false);
});

test('no link, an unknown type or a missing target goes nowhere', () => {
    assert.equal(bannerTarget('ABCD2345', null), null);
    assert.equal(bannerTarget('ABCD2345', { type: 'none', value: '' }), null);
    assert.equal(bannerTarget('ABCD2345', { type: 'category', value: 'Çay' }), null, 'no category id');
    assert.equal(bannerTarget('ABCD2345', { type: 'product', value: 'A' }), null, 'no product key');
    assert.equal(bannerTarget('ABCD2345', { type: 'page', value: 'x' }), null);
});

test('a banner is drawn when it has a title or a picture', () => {
    const items = [
        { id: 1, title: 'Başlık', image: null },
        { id: 2, title: '', image: { thumb: '/s', full: '/l' } },
        { id: 3, title: '  ', text: 'yalnız metin', image: null },
        { id: 4, title: '', image: { thumb: null, full: null } },
        null,
    ];
    assert.deepEqual(usableBanners(items).map(b => b.id), [1, 2]);
    assert.deepEqual(usableBanners(undefined), []);
});

test('the dot follows the slide the scroll position shows', () => {
    assert.equal(slideIndex(0, 360, 3), 0);
    assert.equal(slideIndex(359, 360, 3), 1);
    assert.equal(slideIndex(720, 360, 3), 2);
    assert.equal(slideIndex(5000, 360, 3), 2);
    assert.equal(slideIndex(-10, 360, 3), 0);
    assert.equal(slideIndex(100, 0, 3), 2, 'a zero width does not divide by zero');
    assert.equal(slideIndex(100, 360, 0), 0);
});
