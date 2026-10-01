import test from 'node:test';
import assert from 'node:assert/strict';
import {
    errorMessage, isAccessBlocked, issueMessage, supportCode,
} from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/messages.js';

// Every error code GOAL_MUSTERI_KATALOGU §5.2 lets the customer API return.
const CONTRACT_CODES = [
    'CATALOG_NOT_FOUND', 'INVALID_CREDENTIALS', 'RATE_LIMITED', 'ACCOUNT_INACTIVE', 'CATALOG_UNAVAILABLE',
    'SUBSCRIPTION_REQUIRED', 'SUBSCRIPTION_EXPIRED', 'CSRF_REJECTED', 'INVALID_PASSWORD', 'NOT_FOUND', 'PRICE_CHANGED',
    'CART_INVALID', 'ORDERING_DISABLED', 'TOO_MANY_OPEN_ORDERS', 'FEATURE_DISABLED',
];
const GENERIC = errorMessage({ code: 'SOMETHING_NEW', status: 500 });

test('every contract error code has its own Turkish text', () => {
    for (const code of CONTRACT_CODES) {
        const text = errorMessage({ code, status: 400 });
        assert.ok(text && text !== GENERIC, code);
        assert.doesNotMatch(text, /[A-Z]{2,}_[A-Z]/, code);
    }
});

test('rate limit mentions Retry-After when present', () => {
    assert.equal(errorMessage({ code: 'RATE_LIMITED', status: 429, retryAfter: 120 }, 'login'),
        'Çok fazla deneme yapıldı. 2 dakika sonra tekrar deneyin.');
    assert.equal(errorMessage({ code: 'RATE_LIMITED', status: 429 }, 'login'),
        'Çok fazla deneme yapıldı. Biraz sonra tekrar deneyin.');
    assert.equal(errorMessage({ code: 'HTTP_429', status: 429, retryAfter: 5 }),
        'Çok hızlı işlem yapıldı. 5 saniye sonra tekrar deneyin.');
});

test('context changes INVALID_CREDENTIALS on the password page', () => {
    assert.equal(errorMessage({ code: 'INVALID_CREDENTIALS', status: 401 }, 'login'), 'Kullanıcı adı veya şifre hatalı.');
    assert.equal(errorMessage({ code: 'INVALID_CREDENTIALS', status: 400 }, 'password'), 'Mevcut şifreniz hatalı.');
});

test('network, timeout, bare 401 and unknown codes', () => {
    assert.match(errorMessage({ code: 'NETWORK', status: 0 }), /Bağlantı kurulamadı/);
    assert.match(errorMessage({ code: 'TIMEOUT', status: 0 }), /zamanında yanıt vermedi/);
    assert.equal(errorMessage({ code: 'HTTP_401', status: 401 }), 'Oturumunuz sona erdi, tekrar giriş yapın.');
    assert.equal(GENERIC, 'Beklenmeyen bir sorun oluştu. Biraz sonra tekrar deneyin.');
    assert.equal(errorMessage(null), GENERIC);
});

test('quote issues', () => {
    for (const issue of ['NOT_AVAILABLE', 'OUT_OF_STOCK', 'CARTON_MULTIPLE', 'INVALID_QUANTITY']) {
        assert.ok(issueMessage(issue).length > 0, issue);
        assert.notEqual(issueMessage(issue), issueMessage('NEW_ISSUE'), issue);
    }
    assert.equal(issueMessage(null), '');
    assert.equal(issueMessage('NEW_ISSUE'), 'Şu an sipariş edilemiyor');
    assert.equal(issueMessage('NOT_AVAILABLE'), 'Artık sunulmuyor');
    assert.equal(issueMessage('OUT_OF_STOCK'), 'Stokta yok');
    assert.equal(issueMessage('CARTON_MULTIPLE'), 'Koli katı olmalı');
});

test('access-blocking errors', () => {
    assert.equal(isAccessBlocked({ status: 403, code: 'ACCOUNT_INACTIVE' }), true);
    assert.equal(isAccessBlocked({ status: 403, code: 'CATALOG_UNAVAILABLE' }), true);
    assert.equal(isAccessBlocked({ status: 403, code: 'SUBSCRIPTION_EXPIRED' }), true);
    assert.equal(isAccessBlocked({ status: 403, code: 'CSRF_REJECTED' }), false);
    assert.equal(isAccessBlocked({ status: 401, code: 'ACCOUNT_INACTIVE' }), false);
});

test('supportCode: first 8 characters of the trace id', () => {
    assert.equal(supportCode({ traceId: '3f2a9c1e-77aa-4b1b-9f00-1234567890ab' }), '3F2A9C1E');
    assert.equal(supportCode({ traceId: null }), '');
    assert.equal(supportCode(null), '');
});
