// fetch wrapper for /api/v1/catalog/{code}/… (GOAL_MUSTERI_KATALOGU §5.2). The session is the
// HttpOnly cookie the server sets at login, so nothing here ever sees a token: requests are
// same-origin with credentials, and every state-changing call carries X-Katalog: 1 (CSRF guard).

export const TIMEOUT_MS = 20000;

export class ApiError extends Error {
    constructor(status, code, message, extra) {
        super(message || code);
        this.name = 'ApiError';
        this.status = status;
        this.code = code;
        this.traceId = (extra && extra.traceId) || null;
        this.retryAfter = (extra && extra.retryAfter) || null;
        this.body = (extra && extra.body) || null;
    }
}

/** Retry-After as seconds (the header may be seconds or an HTTP date); null when absent or unreadable. */
export function parseRetryAfter(value, now = Date.now()) {
    if (value === null || value === undefined || String(value).trim() === '') return null;
    const seconds = Number(value);
    if (Number.isFinite(seconds)) return seconds >= 0 ? Math.ceil(seconds) : null;
    const at = Date.parse(value);
    return Number.isNaN(at) ? null : Math.max(0, Math.ceil((at - now) / 1000));
}

function queryString(query) {
    if (!query) return '';
    const params = new URLSearchParams();
    for (const key of Object.keys(query)) {
        const value = query[key];
        if (value !== null && value !== undefined && value !== '') params.set(key, String(value));
    }
    const qs = params.toString();
    return qs ? '?' + qs : '';
}

async function readJson(response) {
    if (response.status === 204 || response.status === 205) return null;
    const text = await response.text();
    if (!text) return null;
    try {
        return JSON.parse(text);
    } catch (e) {
        return undefined;
    }
}

/**
 * createApi('ABCD2345', { onUnauthorized }) -> { get(path, opts), post(path, body, opts) }.
 * opts: { query, signal, anonymous }. `anonymous` calls (info, login) never trigger onUnauthorized:
 * their 401 is a wrong password, not an expired session. A caller's own abort rethrows the
 * AbortError untouched; everything else fails as ApiError (status 0 + NETWORK/TIMEOUT offline).
 */
export function createApi(code, options = {}) {
    const fetchImpl = options.fetch || ((url, init) => fetch(url, init));
    const onUnauthorized = options.onUnauthorized || (() => {});
    const timeoutMs = options.timeoutMs || TIMEOUT_MS;
    const base = '/api/v1/catalog/' + encodeURIComponent(code) + '/';

    async function request(method, path, body, opts = {}) {
        const headers = { Accept: 'application/json' };
        if (method !== 'GET') headers['X-Katalog'] = '1';
        if (body !== undefined) headers['Content-Type'] = 'application/json';

        const controller = new AbortController();
        let timedOut = false;
        const timer = setTimeout(() => {
            timedOut = true;
            controller.abort();
        }, timeoutMs);
        const relay = () => controller.abort();
        const signal = opts.signal;
        if (signal) {
            if (signal.aborted) controller.abort();
            else signal.addEventListener('abort', relay, { once: true });
        }

        let response;
        let data;
        try {
            response = await fetchImpl(base + path + queryString(opts.query), {
                method,
                headers,
                body: body === undefined ? undefined : JSON.stringify(body),
                credentials: 'same-origin',
                cache: 'no-store',
                signal: controller.signal,
            });
            data = await readJson(response);
        } catch (e) {
            if (signal && signal.aborted) throw e;
            throw new ApiError(0, timedOut ? 'TIMEOUT' : 'NETWORK', timedOut ? 'Request timed out' : 'Network error');
        } finally {
            clearTimeout(timer);
            if (signal) signal.removeEventListener('abort', relay);
        }

        if (!response.ok) {
            const err = new ApiError(
                response.status,
                (data && typeof data.errorCode === 'string' && data.errorCode) || 'HTTP_' + response.status,
                data && data.message,
                {
                    traceId: (data && data.traceId) || response.headers.get('X-Correlation-Id'),
                    retryAfter: parseRetryAfter(response.headers.get('Retry-After')),
                    body: data || null,
                });
            if (response.status === 401 && !opts.anonymous) onUnauthorized(err);
            throw err;
        }
        if (data === undefined) throw new ApiError(response.status, 'BAD_RESPONSE', 'Response is not JSON');
        return data;
    }

    return {
        code,
        get: (path, opts) => request('GET', path, undefined, opts),
        post: (path, body, opts) => request('POST', path, body === undefined ? {} : body, opts),
    };
}
