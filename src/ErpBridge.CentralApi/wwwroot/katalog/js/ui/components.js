// Shared pieces: price block, stock badge, cart stepper, product card, empty / error / skeleton
// states and the <dialog> sheet. Class names follow tokens.css (.app-*) and app.css.

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import { count, money, percent, vatLabel } from '../format.js';
import {
    MAX_LINES, addCarton, boxOf, decrease, increase, lineQty, qtyLabel, setQty, stepOf, unitText,
} from '../cart.js';
import { errorMessage, supportCode } from '../messages.js';
import { routePath } from '../route-parse.js';

/**
 * List price struck through + discount badge + net price when a discount applies, otherwise only
 * the net price; the VAT mode of the price list always underneath (K5). The server computes
 * every figure; nothing is recalculated here.
 */
export function priceBlock(price, opts = {}) {
    if (!price) return h('p', { class: 't-caption muted price' }, 'Fiyat için firmanıza danışın');
    const discounted = price.discountPercent > 0 && price.net < price.list;
    return h('div', { class: ['price', opts.large && 'price--large'] },
        discounted ? h('div', { class: 'price__row' },
            h('span', { class: 'sr-only' }, 'Liste fiyatı:'),
            h('s', { class: 't-amount-sm price__list' }, money(price.list)),
            h('span', { class: 'app-badge app-badge--success badge--compact' },
                percent(price.discountPercent), h('span', { class: 'sr-only' }, ' iskonto'))) : null,
        h('div', { class: 'price__row' },
            discounted ? h('span', { class: 'sr-only' }, 'İskontolu fiyat:') : null,
            h('span', { class: [opts.large ? 't-display tnum' : opts.small ? 't-amount' : 't-amount-lg', 'price__net'] }, money(price.net))),
        h('span', { class: 't-caption muted' }, vatLabel(price.includesVat)));
}

/** K6: the quantity is never shown, only that there is none. */
export function stockBadge() {
    return h('span', { class: 'app-badge app-badge--danger' }, icon('alert', { size: 14 }), 'Stokta yok');
}

export function iconButton(name, label, onclick, extraClass) {
    return h('button', { type: 'button', class: ['icon-btn', extraClass], 'aria-label': label, onclick }, icon(name));
}

/** Product image in a square box; no image or a broken one shows the carton placeholder. */
export function productMedia(src, alt, eager) {
    const wrap = h('div', { class: 'media' });
    if (!src) {
        wrap.append(mediaPlaceholder());
        return wrap;
    }
    // loading/decoding/referrerpolicy go before src: an <img> starts fetching as soon as src is set.
    const img = h('img', {
        loading: eager ? 'eager' : 'lazy',
        decoding: 'async',
        referrerpolicy: 'no-referrer',
        width: 400,
        height: 400,
        alt: alt || '',
        src,
    });
    img.addEventListener('error', () => img.replaceWith(mediaPlaceholder()), { once: true });
    wrap.append(img);
    return wrap;
}

export function mediaPlaceholder() {
    return h('span', { class: 'media__empty', 'aria-hidden': 'true' }, icon('box', { size: 40 }));
}

/** canAdd = ordering on for this account && in stock && priced. */
export function canOrder(ctx, product) {
    const features = ctx.me && ctx.me.features;
    return !!(features && features.order && product.inStock && product.price);
}

/**
 * Applies a requested quantity to the cart and tells the customer what happened (rounded to the
 * carton, capped, cart full, added). Returns the quantity now in the cart.
 */
export function changeCart(ctx, product, requested) {
    const lines = ctx.store.get('cart') || [];
    const before = lineQty(lines, product.key);
    const result = setQty(lines, product, requested);
    if (result.full) {
        ctx.notify('Sepete en çok ' + count(MAX_LINES) + ' farklı ürün eklenebilir.');
        return before;
    }
    ctx.store.set('cart', result.lines);
    const q = result.quantity;
    if (q > 0 && q < requested) {
        ctx.notify('En fazla ' + count(q) + ' ' + unitText(product) + ' eklenebilir.');
    } else if (q > requested && requested > 0) {
        ctx.notify('Miktar koli katına yuvarlandı: ' + qtyLabel(product, q) + '.');
    } else if (before === 0 && q > 0) {
        ctx.notify('Sepete eklendi', { label: 'Sepete git', href: routePath(ctx.code, 'cart') });
    }
    return q;
}

/** [−] [qty] [+] bound to one product; set() updates it in place so focus is never lost. */
export function createStepper(product, name, onRequest) {
    let qty = 0;
    const input = h('input', {
        class: 'stepper__qty tnum',
        type: 'text',
        inputmode: 'numeric',
        autocomplete: 'off',
        enterkeyhint: 'done',
        maxlength: '4',
        'aria-label': 'Miktar (' + unitText(product) + '), ' + name,
    });
    const minus = h('button', { type: 'button', class: 'stepper__btn', onclick: () => onRequest(decrease(product, qty)) },
        icon('minus', { size: 20 }));
    const plus = h('button', { type: 'button', class: 'stepper__btn', 'aria-label': 'Artır', onclick: () => onRequest(increase(product, qty)) },
        icon('plus', { size: 20 }));
    input.addEventListener('focus', () => input.select());
    input.addEventListener('keydown', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            input.blur();
        }
    });
    input.addEventListener('change', () => {
        const digits = input.value.replace(/[^0-9]/g, '');
        const requested = digits ? Number(digits) : 0;
        if (requested === qty) input.value = String(qty);
        else onRequest(requested);
    });
    const node = h('div', { class: 'stepper', role: 'group', 'aria-label': 'Miktar, ' + name }, minus, input, plus);
    return {
        node,
        set(next) {
            qty = next;
            input.value = String(next);
            minus.setAttribute('aria-label', next <= stepOf(product) ? 'Sepetten çıkar' : 'Azalt');
            plus.disabled = increase(product, next) === next;
        },
        focus() {
            plus.focus();
        },
    };
}

/**
 * "Ekle" (or "Koli ekle (24)") until the product is in the cart, then the stepper; a product that
 * also sells single pieces gets a "+1 koli" button. opts: { primary, addLabel, onQty }.
 */
export function cartControl(ctx, product, opts = {}) {
    const box = boxOf(product);
    const node = h('div', { class: 'cartctl' });
    const note = h('p', { class: 'cartctl__note t-caption muted', 'aria-live': 'polite' });
    const stepper = createStepper(product, product.name, requested => apply(requested));
    // Labels stay short enough for a 160px card; the carton size is in the card text and aria-label.
    const addButton = h('button', {
        type: 'button',
        class: ['app-btn', opts.primary ? 'app-btn--primary' : 'app-btn--secondary', 'btn-touch', 'btn-block'],
        'aria-label': box && box.only ? '1 koli ekle (' + count(box.qty) + ' ' + unitText(product) + ')' : null,
        onclick: () => apply(increase(product, 0)),
    }, icon('plus', { size: 20 }), box && box.only ? 'Koli ekle' : opts.addLabel || 'Ekle');
    const cartonButton = box && !box.only
        ? h('button', {
            type: 'button',
            class: 'app-btn app-btn--ghost btn-touch btn-block',
            'aria-label': '1 koli ekle (' + count(box.qty) + ' ' + unitText(product) + ')',
            onclick: () => apply(addCarton(product, qty)),
        }, icon('box', { size: 20 }), '+1 koli')
        : null;
    let qty = -1;

    function apply(requested) {
        const hadFocus = node.contains(document.activeElement);
        show(changeCart(ctx, product, requested), hadFocus);
    }

    function show(next, keepFocus) {
        if (next === qty) {
            stepper.set(next);
            return;
        }
        const wasEmpty = qty <= 0;
        qty = next;
        if (qty === 0) {
            render(node, addButton, cartonButton, note);
            if (keepFocus) addButton.focus();
        } else {
            stepper.set(qty);
            if (wasEmpty) {
                render(node, stepper.node, cartonButton, note);
                if (keepFocus) stepper.focus();
            }
        }
        // A carton-only product always says how many cartons; otherwise the note is for screen readers.
        const cartonOnly = !!(box && box.only);
        note.textContent = qty > 0 ? (cartonOnly ? qtyLabel(product, qty) : 'Sepette ' + qtyLabel(product, qty)) : '';
        note.classList.toggle('sr-only', !cartonOnly);
        if (opts.onQty) opts.onQty(qty);
    }

    show(lineQty(ctx.store.get('cart') || [], product.key), false);
    return { node, update: next => show(next, false) };
}

/** Grid card. Returns { node, update(qty) } so a cart change redraws only this card's controls. */
export function productCard(ctx, product, eager) {
    const titleId = uid('p');
    const box = boxOf(product);
    const node = h('article', { class: 'pcard app-card', 'aria-labelledby': titleId },
        h('a', { class: 'pcard__link', href: routePath(ctx.code, 'product', { kod: product.key }), dataset: { overlay: '1' } },
            productMedia(product.thumb, '', eager),
            h('span', { class: 't-caption muted pcard__code' }, product.code),
            h('span', { class: 'pcard__name', id: titleId }, product.name)),
        h('div', { class: 'pcard__info' },
            box ? h('span', { class: 't-caption muted' },
                (box.only ? 'Yalnız koli · ' : 'Koli: ') + count(box.qty) + ' ' + unitText(product)) : null,
            priceBlock(product.price),
            product.inStock ? null : stockBadge()));
    if (!canOrder(ctx, product)) return { node, update() {} };
    const control = cartControl(ctx, product, { onQty: q => node.classList.toggle('pcard--in-cart', q > 0) });
    node.append(h('div', { class: 'pcard__actions' }, control.node));
    return { node, update: control.update };
}

/** Top-bar heading of an inner page, with a back link to its parent ({ href, label }) when given. */
export function pageHeader(title, back) {
    return h('div', { class: 'pagehead' },
        back ? h('a', { class: 'icon-btn', href: back.href, 'aria-label': back.label }, icon('back')) : null,
        h('h1', { class: 't-title topbar__title' }, title));
}

/**
 * Coloured notice inside a page: band('warning', 'Fiyatlar güncellendi', 'Yeni toplam …', action).
 * Errors are announced at once (role=alert), everything else politely (role=status).
 */
export function band(tone, title, text, action) {
    return h('div', { class: ['band', 'band--' + tone], role: tone === 'danger' ? 'alert' : 'status' },
        icon(tone === 'success' ? 'check' : 'alert', { size: 20 }),
        h('div', { class: 'band__body' },
            h('p', { class: 'band__title' }, title),
            text ? h('p', null, text) : null,
            action || null));
}

export function emptyState(opts) {
    return h('div', { class: 'state' },
        h('span', { class: 'state__icon', 'aria-hidden': 'true' }, icon(opts.icon || 'search', { size: 40 })),
        h('p', { class: 't-title' }, opts.title),
        opts.text ? h('p', { class: 't-body muted' }, opts.text) : null,
        opts.action || null);
}

/** Always offers a way out (DESIGN_SYSTEM §10.6) and a support code when the server sent one. */
export function errorState(err, onRetry, title) {
    const code = supportCode(err);
    return h('div', { class: 'state', role: 'alert' },
        h('span', { class: 'state__icon state__icon--danger', 'aria-hidden': 'true' }, icon('alert', { size: 40 })),
        h('p', { class: 't-title' }, title || errorMessage(err)),
        title ? h('p', { class: 't-body muted' }, errorMessage(err)) : null,
        onRetry ? h('button', { type: 'button', class: 'app-btn app-btn--secondary btn-touch', onclick: onRetry }, 'Tekrar dene') : null,
        code ? h('p', { class: 't-caption muted' }, 'Destek kodu: ' + code) : null);
}

/** Static placeholder cards while the first page loads (no animation, DESIGN_SYSTEM §9.1). */
export function skeletonCards(n) {
    const cards = [];
    for (let i = 0; i < n; i++) {
        cards.push(h('div', { class: 'pcard app-card skel-card', 'aria-hidden': 'true' },
            h('div', { class: 'media skel' }),
            h('div', { class: 'skel skel--line skel--short' }),
            h('div', { class: 'skel skel--line' }),
            h('div', { class: 'skel skel--line skel--price' })));
    }
    return cards;
}

/**
 * Modal <dialog>: a bottom sheet on phones, a centred panel from 768px. Esc, the backdrop and a
 * close forced by the browser all go through onRequestClose so the caller keeps history in step.
 */
export function openSheet(opts) {
    let closing = false;
    const dialog = h('dialog', {
        class: ['sheet', opts.className],
        'aria-label': opts.label,
        'aria-labelledby': opts.labelledBy,
    });
    dialog.addEventListener('cancel', e => {
        e.preventDefault();
        opts.onRequestClose();
    });
    dialog.addEventListener('close', () => {
        if (!closing) opts.onRequestClose();
    });
    dialog.addEventListener('click', e => {
        if (e.target === dialog) opts.onRequestClose();
    });
    document.body.append(dialog);
    document.documentElement.classList.add('is-modal');
    dialog.showModal();
    return {
        dialog,
        close() {
            if (closing) return;
            closing = true;
            if (dialog.open) dialog.close();
            dialog.remove();
            if (!document.querySelector('dialog[open]')) document.documentElement.classList.remove('is-modal');
        },
    };
}

export function sheetHeader(title, titleId, onClose) {
    return h('div', { class: 'sheet__head' },
        h('h2', { class: 't-title sheet__title', id: titleId }, title),
        iconButton('close', 'Kapat', onClose));
}
