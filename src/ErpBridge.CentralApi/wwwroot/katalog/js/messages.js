// Error codes from GOAL_MUSTERI_KATALOGU §5 in the customer's words. Server messages are English
// and never shown as they are (node --test: tests/katalog-web/messages.test.mjs).

import { duration } from './format.js';

const TEXT = {
    CATALOG_NOT_FOUND: 'Katalog bulunamadı. Bağlantıyı kontrol edin.',
    INVALID_CREDENTIALS: 'Kullanıcı adı veya şifre hatalı.',
    ACCOUNT_INACTIVE: 'Erişiminiz kapatılmış. Firmanızla görüşün.',
    CATALOG_UNAVAILABLE: 'Katalog şu an kullanıma kapalı. Firmanızla görüşün.',
    SUBSCRIPTION: 'Firmanın aboneliği etkin olmadığı için katalog şu an açılamıyor.',
    CSRF_REJECTED: 'Güvenlik denetimi geçilemedi. Sayfayı yenileyip tekrar deneyin.',
    INVALID_PASSWORD: 'Yeni şifre en az 8, en çok 72 karakter olmalı.',
    NOT_FOUND: 'Ürün bulunamadı ya da artık katalogda değil.',
    PRICE_CHANGED: 'Fiyatlar güncellendi. Güncel tutarı kontrol edip yeniden gönderin.',
    CART_INVALID: 'Sepetinizde düzeltilmesi gereken ürünler var.',
    ORDERING_DISABLED: 'Sipariş talebi gönderme hesabınızda kapalı.',
    TOO_MANY_OPEN_ORDERS: 'Açık talep sınırına ulaştınız. Firmanız mevcut talepleri işledikten sonra yeniden deneyin.',
    FEATURE_DISABLED: 'Bu bölüm hesabınızda açık değil.',
    SESSION_EXPIRED: 'Oturumunuz sona erdi, tekrar giriş yapın.',
    NETWORK: 'Bağlantı kurulamadı. İnternetinizi kontrol edip tekrar deneyin.',
    TIMEOUT: 'Sunucu zamanında yanıt vermedi. Biraz sonra tekrar deneyin.',
    UNKNOWN: 'Beklenmeyen bir sorun oluştu. Biraz sonra tekrar deneyin.',
};

const ISSUES = {
    NOT_AVAILABLE: 'Artık sunulmuyor',
    OUT_OF_STOCK: 'Stokta yok',
    CARTON_MULTIPLE: 'Koli katı olmalı',
    INVALID_QUANTITY: 'Miktar geçersiz',
};

// Codes that mean "this account cannot use the catalogue right now": shown as a full page, not a band.
const BLOCKING = new Set(['ACCOUNT_INACTIVE', 'CATALOG_UNAVAILABLE', 'TENANT_INACTIVE']);

function codeOf(err) {
    return err && typeof err.code === 'string' ? err.code : '';
}

/**
 * err = ApiError (or anything with code/status/retryAfter). context: 'login' | 'password' | undefined.
 */
export function errorMessage(err, context) {
    const code = codeOf(err);
    const status = err && err.status;
    // A 429 with a code of its own (TOO_MANY_OPEN_ORDERS) says that, not "too fast".
    if (code === 'RATE_LIMITED' || (status === 429 && !Object.prototype.hasOwnProperty.call(TEXT, code))) {
        const wait = err && err.retryAfter > 0 ? duration(err.retryAfter) : '';
        if (context === 'login') {
            return wait ? 'Çok fazla deneme yapıldı. ' + wait + ' sonra tekrar deneyin.'
                : 'Çok fazla deneme yapıldı. Biraz sonra tekrar deneyin.';
        }
        return wait ? 'Çok hızlı işlem yapıldı. ' + wait + ' sonra tekrar deneyin.'
            : 'Çok hızlı işlem yapıldı, birkaç saniye sonra deneyin.';
    }
    if (code === 'INVALID_CREDENTIALS' && context === 'password') return 'Mevcut şifreniz hatalı.';
    if (code.startsWith('SUBSCRIPTION_')) return TEXT.SUBSCRIPTION;
    if (code === 'TENANT_INACTIVE') return TEXT.CATALOG_UNAVAILABLE;
    if (Object.prototype.hasOwnProperty.call(TEXT, code)) return TEXT[code];
    if (status === 401) return TEXT.SESSION_EXPIRED;
    if (status === 404) return 'Aradığınız kayıt bulunamadı.';
    return TEXT.UNKNOWN;
}

/** cart/quote line issue -> short text, or '' for a line without an issue. */
export function issueMessage(issue) {
    if (!issue) return '';
    return Object.prototype.hasOwnProperty.call(ISSUES, issue) ? ISSUES[issue] : 'Şu an sipariş edilemiyor';
}

/** True when the error closes the catalogue for this account (inactive account, module off, subscription). */
export function isAccessBlocked(err) {
    const code = codeOf(err);
    return err && err.status === 403 && (BLOCKING.has(code) || code.startsWith('SUBSCRIPTION_'));
}

/** First 8 characters of the trace id, for "Destek kodu: …" under an error. */
export function supportCode(err) {
    const id = err && typeof err.traceId === 'string' ? err.traceId.replace(/[^0-9A-Za-z]/g, '') : '';
    return id ? id.slice(0, 8).toUpperCase() : '';
}
