import test from 'node:test';
import assert from 'node:assert/strict';
import {
    count, date, dateTime, duration, foldText, initials, money, percent, vatLabel,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/format.js';

test('money: tr-TR grouping, two decimals, TL suffix', () => {
    assert.equal(money(1234.5), '1.234,50 TL');
    assert.equal(money(0), '0,00 TL');
    assert.equal(money(1234567.891), '1.234.567,89 TL');
    assert.equal(money('112.95'), '112,95 TL');
    assert.equal(money(-1234.5), '-1.234,50 TL');
    assert.equal(money(12, ''), '12,00');
});

test('money: zero never carries a sign, non-numbers give empty text', () => {
    assert.equal(money(-0), '0,00 TL');
    assert.equal(money(-0.001), '0,00 TL');
    assert.equal(money(null), '');
    assert.equal(money(undefined), '');
    assert.equal(money(''), '');
    assert.equal(money('abc'), '');
    assert.equal(money(Infinity), '');
});

test('count and percent', () => {
    assert.equal(count(1234), '1.234');
    assert.equal(count(48), '48');
    assert.equal(percent(10), '%10');
    assert.equal(percent(12.5), '%12,5');
    assert.equal(percent(null), '');
});

test('date: read from the text, no time-zone shift', () => {
    assert.equal(date('2026-09-12'), '12.09.2026');
    assert.equal(date('2026-01-31T23:30:00Z'), '31.01.2026');
    assert.equal(date('12.09.2026'), '');
    assert.equal(date(null), '');
});

test('dateTime: Türkiye time', () => {
    assert.equal(dateTime(Date.UTC(2026, 9, 1, 11, 32)), '01.10.2026 14:32');
    assert.equal(dateTime(null), '');
});

test('duration: Retry-After in words', () => {
    assert.equal(duration(1), '1 saniye');
    assert.equal(duration(30), '30 saniye');
    assert.equal(duration(60), '1 dakika');
    assert.equal(duration(61), '2 dakika');
    assert.equal(duration(900), '15 dakika');
    assert.equal(duration(3600), '1 saat');
    assert.equal(duration(0), '1 saniye');
});

test('vatLabel follows the price list', () => {
    assert.equal(vatLabel(true), 'KDV dahil');
    assert.equal(vatLabel(false), 'KDV hariç');
});

test('initials: Turkish upper case, two words at most', () => {
    assert.equal(initials('abc gıda dağıtım'), 'AG');
    assert.equal(initials('işık market'), 'İM');
    assert.equal(initials('Yıldız'), 'Y');
    assert.equal(initials('  '), '?');
    assert.equal(initials(null), '?');
});

test('foldText: Turkish lower case', () => {
    assert.equal(foldText('IŞIK'), 'ışık');
    assert.equal(foldText(' İçecek '), 'içecek');
});
