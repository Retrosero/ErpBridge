import test from 'node:test';
import assert from 'node:assert/strict';
import { ApiError, createApi, parseRetryAfter } from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/api.js';

function recorder(respond) {
    const calls = [];
    const fetch = async (url, init) => {
        calls.push({ url, init });
        return respond(url, init);
    };
    return { calls, fetch };
}

const jsonResponse = (status, body, headers = {}) =>
    new Response(body === undefined ? null : JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json', ...headers } });

test('GET: same-origin cookie, no X-Katalog, query string without empty values', async () => {
    const { calls, fetch } = recorder(() => jsonResponse(200, { items: [] }));
    const api = createApi('ABCD2345', { fetch });
    const res = await api.get('products', { query: { category: 'c1', q: null, page: 2, pageSize: 48 } });
    assert.deepEqual(res, { items: [] });
    assert.equal(calls[0].url, '/api/v1/catalog/ABCD2345/products?category=c1&page=2&pageSize=48');
    assert.equal(calls[0].init.method, 'GET');
    assert.equal(calls[0].init.credentials, 'same-origin');
    assert.equal(calls[0].init.headers['X-Katalog'], undefined);
    assert.equal(calls[0].init.body, undefined);
});

test('POST: X-Katalog: 1 and a JSON body; 204 gives null', async () => {
    const { calls, fetch } = recorder(() => new Response(null, { status: 204 }));
    const api = createApi('ABCD2345', { fetch });
    assert.equal(await api.post('logout'), null);
    assert.equal(calls[0].init.method, 'POST');
    assert.equal(calls[0].init.headers['X-Katalog'], '1');
    assert.equal(calls[0].init.headers['Content-Type'], 'application/json');
    assert.equal(calls[0].init.body, '{}');

    await api.post('login', { username: 'a', password: 'b', remember: true }, { anonymous: true });
    assert.deepEqual(JSON.parse(calls[1].init.body), { username: 'a', password: 'b', remember: true });
});

test('errors become ApiError with code, message, trace id', async () => {
    const { fetch } = recorder(() => jsonResponse(409, { errorCode: 'PRICE_CHANGED', message: 'Prices changed', traceId: 'abc123', quote: { totals: {} } }));
    const api = createApi('ABCD2345', { fetch });
    const err = await api.post('orders', {}).catch(e => e);
    assert.ok(err instanceof ApiError);
    assert.equal(err.status, 409);
    assert.equal(err.code, 'PRICE_CHANGED');
    assert.equal(err.traceId, 'abc123');
    assert.deepEqual(err.body.quote, { totals: {} });
});

test('an error without a JSON body still has a code and the correlation header', async () => {
    const { fetch } = recorder(() => new Response('', { status: 502, headers: { 'X-Correlation-Id': 'corr-1' } }));
    const err = await createApi('ABCD2345', { fetch }).get('me').catch(e => e);
    assert.equal(err.code, 'HTTP_502');
    assert.equal(err.traceId, 'corr-1');
});

test('401 calls onUnauthorized, except for anonymous calls', async () => {
    let lost = 0;
    const { fetch } = recorder(() => new Response('', { status: 401 }));
    const api = createApi('ABCD2345', { fetch, onUnauthorized: () => { lost += 1; } });
    const err = await api.get('me').catch(e => e);
    assert.equal(err.status, 401);
    assert.equal(lost, 1);
    await api.post('login', { username: 'x' }, { anonymous: true }).catch(() => {});
    assert.equal(lost, 1);
});

test('429 carries Retry-After seconds', async () => {
    const { fetch } = recorder(() => jsonResponse(429, { errorCode: 'RATE_LIMITED' }, { 'Retry-After': '42' }));
    const err = await createApi('ABCD2345', { fetch }).post('login', {}, { anonymous: true }).catch(e => e);
    assert.equal(err.code, 'RATE_LIMITED');
    assert.equal(err.retryAfter, 42);
});

test('parseRetryAfter: seconds, HTTP date, junk', () => {
    const now = Date.parse('2026-10-01T10:00:00Z');
    assert.equal(parseRetryAfter('30', now), 30);
    assert.equal(parseRetryAfter('Thu, 01 Oct 2026 10:01:30 GMT', now), 90);
    assert.equal(parseRetryAfter('Thu, 01 Oct 2026 09:00:00 GMT', now), 0);
    assert.equal(parseRetryAfter('-5', now), null);
    assert.equal(parseRetryAfter('soon', now), null);
    assert.equal(parseRetryAfter(null, now), null);
    assert.equal(parseRetryAfter('', now), null);
});

test('network failure -> NETWORK, slow server -> TIMEOUT', async () => {
    const offline = createApi('ABCD2345', { fetch: async () => { throw new TypeError('Failed to fetch'); } });
    const err = await offline.get('me').catch(e => e);
    assert.equal(err.status, 0);
    assert.equal(err.code, 'NETWORK');

    const hanging = (url, init) => new Promise((resolve, reject) => {
        init.signal.addEventListener('abort', () => reject(new DOMException('Aborted', 'AbortError')));
    });
    const slow = createApi('ABCD2345', { fetch: hanging, timeoutMs: 20 });
    const timeout = await slow.get('products').catch(e => e);
    assert.equal(timeout.code, 'TIMEOUT');
});

test("the caller's abort passes through untouched", async () => {
    const hanging = (url, init) => new Promise((resolve, reject) => {
        init.signal.addEventListener('abort', () => reject(new DOMException('Aborted', 'AbortError')));
    });
    const api = createApi('ABCD2345', { fetch: hanging });
    const controller = new AbortController();
    const pending = api.get('products', { signal: controller.signal });
    controller.abort();
    const err = await pending.catch(e => e);
    assert.equal(err.name, 'AbortError');
    assert.ok(!(err instanceof ApiError));
});

test('a 200 that is not JSON is an error, not silent data', async () => {
    const { fetch } = recorder(() => new Response('<html>', { status: 200 }));
    const err = await createApi('ABCD2345', { fetch }).get('me').catch(e => e);
    assert.equal(err.code, 'BAD_RESPONSE');
});
