// Pure formatting helpers (node --test: tests/katalog-web/format.test.mjs). Amounts read like the
// phone (AppFeedback.formatAmount) and the panel (PortalMessages.Money): "1.234,50 TL".

const amountFormat = new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const countFormat = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 });
const percentFormat = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 });
const dateTimeFormat = new Intl.DateTimeFormat('tr-TR', {
    timeZone: 'Europe/Istanbul',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
});

function toNumber(value) {
    if (value === null || value === undefined || value === '') return NaN;
    return Number(value);
}

/** 1234.5 -> "1.234,50 TL". A zero never carries a sign; a non-number gives "". */
export function money(amount, currency = 'TL') {
    const n = toNumber(amount);
    if (!Number.isFinite(n)) return '';
    const text = amountFormat.format(Math.abs(n) < 0.005 ? 0 : n);
    return currency ? text + ' ' + currency : text;
}

/** 1234 -> "1.234". */
export function count(value) {
    const n = toNumber(value);
    return Number.isFinite(n) ? countFormat.format(n) : '';
}

/** 10 -> "%10", 12.5 -> "%12,5" (Turkish puts the sign first). */
export function percent(value) {
    const n = toNumber(value);
    return Number.isFinite(n) ? '%' + percentFormat.format(n) : '';
}

/** "2026-09-12" (or an ISO timestamp) -> "12.09.2026", read from the text so no time zone can shift the day. */
export function date(iso) {
    const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(typeof iso === 'string' ? iso : '');
    return m ? m[3] + '.' + m[2] + '.' + m[1] : '';
}

/** Unix ms -> "01.10.2026 14:32" in Türkiye time. */
export function dateTime(ms) {
    const n = toNumber(ms);
    return Number.isFinite(n) ? dateTimeFormat.format(new Date(n)) : '';
}

/** Retry-After seconds -> "30 saniye" / "2 dakika" / "1 saat". */
export function duration(seconds) {
    const s = Math.max(1, Math.ceil(toNumber(seconds) || 0));
    if (s < 60) return s + ' saniye';
    if (s < 3600) return Math.ceil(s / 60) + ' dakika';
    return Math.ceil(s / 3600) + ' saat';
}

function pad2(n) {
    return (n < 10 ? '0' : '') + n;
}

/** A local Date -> "2026-10-01" (the device's own calendar day, as statement?from=&to= expect). */
export function isoDay(d) {
    return d.getFullYear() + '-' + pad2(d.getMonth() + 1) + '-' + pad2(d.getDate());
}

/**
 * Statement presets -> { from, to } ending today: '30g' the last 30 days, '3a' the last three
 * months (the default), 'yil' since 1 January. Anything else falls back to '3a'.
 */
export function dateRange(preset, now = new Date()) {
    const to = isoDay(now);
    if (preset === '30g') return { from: isoDay(new Date(now.getFullYear(), now.getMonth(), now.getDate() - 29)), to };
    if (preset === 'yil') return { from: now.getFullYear() + '-01-01', to };
    return { from: isoDay(new Date(now.getFullYear(), now.getMonth() - 3, now.getDate())), to };
}

/**
 * A balance in the customer's words (DESIGN_SYSTEM §2.5): positive = the customer owes ("Borç",
 * --color-debt), negative = the firm owes ("Alacak", --color-credit), zero = "Bakiye yok".
 */
export function balanceSense(amount) {
    const n = toNumber(amount);
    if (!Number.isFinite(n) || Math.abs(n) < 0.005) return { label: 'Bakiye yok', tone: 'neutral' };
    return n > 0 ? { label: 'Borç', tone: 'debt' } : { label: 'Alacak', tone: 'credit' };
}

const KINDS = {
    sale: 'Satış faturası',
    sale_return: 'İade faturası',
    purchase: 'Alış faturası',
    purchase_return: 'Alış iadesi',
    collection: 'Tahsilat',
    payment: 'Ödeme',
};

/** Ledger / invoice kind (PortalLedger.Kinds) -> what the customer reads. */
export function kindLabel(kind) {
    return Object.prototype.hasOwnProperty.call(KINDS, kind) ? KINDS[kind] : 'Diğer';
}

/** The price list's VAT mode as the customer reads it under every price (K5). */
export function vatLabel(includesVat) {
    return includesVat ? 'KDV dahil' : 'KDV hariç';
}

/** "abc gıda dağıtım" -> "AG": the company badge. */
export function initials(name) {
    const words = String(name || '').trim().split(/[^0-9A-Za-zÇĞİÖŞÜçğıöşü]+/).filter(Boolean);
    const letters = words.slice(0, 2).map(w => w.charAt(0)).join('');
    return letters ? letters.toLocaleUpperCase('tr-TR') : '?';
}

/** Turkish-aware lower case for client-side filtering ("IŞIK" -> "ışık"). */
export function foldText(value) {
    return String(value || '').toLocaleLowerCase('tr-TR').trim();
}
