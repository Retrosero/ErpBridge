// Catalogue banners (GOAL_MUSTERI_KATALOGU S12/W6): which banners are worth drawing and what a click on one does.
// Pure, so node --test checks it (tests/katalog-web/banners.test.mjs); the strip itself is ui/banner-strip.js.

import { routePath } from './route-parse.js';

/** An absolute https address, the only kind a banner may open in a new tab (the server checks it too). */
export function isHttpsUrl(value) {
    if (typeof value !== 'string' || !value) return false;
    try {
        return new URL(value).protocol === 'https:';
    } catch (e) {
        return false;
    }
}

/** Banners with something to show: a title, or a picture. */
export function usableBanners(items) {
    return Array.isArray(items)
        ? items.filter(b => b && typeof b === 'object' && (String(b.title || '').trim() || (b.image && (b.image.full || b.image.thumb))))
        : [];
}

/**
 * Where a banner goes: { kind: 'category', id, href } (the list switches to it), { kind: 'product', href } (the
 * product dialog), { kind: 'url', href } (a new tab), or null (nowhere). The server already left out links to what
 * this customer does not see.
 */
export function bannerTarget(code, link) {
    if (!link || typeof link !== 'object') return null;
    if (link.type === 'category' && link.categoryId) {
        const id = String(link.categoryId);
        return { kind: 'category', id, href: routePath(code, 'catalog', { kategori: id }) };
    }
    if (link.type === 'product' && link.productKey) return { kind: 'product', href: routePath(code, 'product', { kod: String(link.productKey) }) };
    if (link.type === 'url' && isHttpsUrl(link.value)) return { kind: 'url', href: link.value };
    return null;
}

/** The slide a horizontal scroll position shows (each slide is the track's full width). */
export function slideIndex(scrollLeft, width, count) {
    if (!(count > 0)) return 0;
    return Math.min(count - 1, Math.max(0, Math.round((Number(scrollLeft) || 0) / Math.max(1, Number(width) || 0))));
}
