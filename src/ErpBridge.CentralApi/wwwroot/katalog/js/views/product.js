// Product detail in a modal <dialog> over the catalogue (/{KOD}/urun?kod=): scroll-snap gallery,
// price, carton and stock, and the cart control in a sticky footer. The back button closes it.

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import { count } from '../format.js';
import { boxOf, unitText } from '../cart.js';
import { routePath } from '../route-parse.js';
import {
    canOrder, cartControl, emptyState, errorState, iconButton, mediaPlaceholder, openSheet, priceBlock, stockBadge,
} from '../ui/components.js';

// A srcset candidate URL may not contain spaces or commas (external https links can).
function srcsetSafe(url) {
    return typeof url === 'string' && url !== '' && !/[\s,]/.test(url);
}

function slideImage(image, alt, eager) {
    const small = image.thumb || image.full;
    const large = image.full || image.thumb;
    const img = h('img', {
        loading: eager ? 'eager' : 'lazy',
        decoding: 'async',
        referrerpolicy: 'no-referrer',
        width: 1280,
        height: 1280,
        alt,
        sizes: '(min-width: 768px) 336px, 100vw',
        srcset: small !== large && srcsetSafe(small) && srcsetSafe(large) ? small + ' 400w, ' + large + ' 1280w' : null,
        src: large,
    });
    img.addEventListener('error', () => img.replaceWith(mediaPlaceholder()), { once: true });
    return img;
}

function gallery(product) {
    let images = Array.isArray(product.images) ? product.images.filter(i => i && (i.full || i.thumb)) : [];
    if (!images.length && product.thumb) images = [{ thumb: product.thumb, full: product.thumb }];
    if (!images.length) return h('div', { class: 'gallery' }, h('div', { class: 'gallery__slide' }, mediaPlaceholder()));

    const n = images.length;
    const track = h('div', { class: 'gallery__track', role: 'group', 'aria-label': 'Ürün görselleri', tabindex: '0' },
        images.map((image, i) => h('div', { class: 'gallery__slide' },
            slideImage(image, product.name + ', görsel ' + (i + 1) + '/' + n, i === 0))));
    if (n === 1) return h('div', { class: 'gallery' }, track);

    const counter = h('span', { class: 'gallery__count t-caption', 'aria-hidden': 'true' }, '1/' + n);
    const smooth = !window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const go = dir => track.scrollBy({ left: dir * track.clientWidth, behavior: smooth ? 'smooth' : 'auto' });
    track.addEventListener('scroll', () => {
        const i = Math.min(n - 1, Math.max(0, Math.round(track.scrollLeft / Math.max(1, track.clientWidth))));
        counter.textContent = (i + 1) + '/' + n;
    }, { passive: true });
    return h('div', { class: 'gallery' }, track, counter,
        iconButton('back', 'Önceki görsel', () => go(-1), 'gallery__nav gallery__nav--prev'),
        iconButton('next', 'Sonraki görsel', () => go(1), 'gallery__nav gallery__nav--next'));
}

function badges(product) {
    const box = boxOf(product);
    if (!box) return null;
    return h('div', { class: 'pd__badges' },
        h('span', { class: 'app-badge app-badge--neutral' }, icon('box', { size: 14 }), 'Koli içi ' + count(box.qty) + ' ' + unitText(product)),
        box.only ? h('span', { class: 'app-badge app-badge--brand' }, 'Yalnız koli ile satılır') : null);
}

/**
 * Opens the dialog and loads products/detail?key=. onRequestClose decides what closing means for
 * history (back or replace); the caller then calls close().
 */
export function openProduct(ctx, key, onRequestClose) {
    const titleId = uid('pd');
    const sheet = openSheet({ className: 'sheet--product', labelledBy: titleId, onRequestClose });
    const title = h('h2', { class: 't-title pd__title', id: titleId }, 'Ürün');
    const content = h('div', { class: 'sheet__content' });
    const footer = h('div', { class: 'sheet__foot', hidden: true });
    const controller = new AbortController();
    let unsubscribe = null;

    // The snackbar sits under the modal's top layer, so notices show inside the dialog instead.
    const notice = h('p', { class: 't-caption pd__notice', role: 'status' });
    const localCtx = Object.assign({}, ctx, { notify: text => { notice.textContent = text; } });

    render(sheet.dialog,
        h('div', { class: 'sheet__head sheet__head--bare' }, iconButton('back', 'Kapat', onRequestClose)),
        content, footer);
    render(content, h('div', { class: 'pd' },
        h('div', { class: 'gallery' }, h('div', { class: 'gallery__slide skel' })),
        h('div', { class: 'pd__info', 'aria-busy': 'true' }, title,
            h('div', { class: 'skel skel--line' }), h('div', { class: 'skel skel--line skel--short' }))));

    function show(product) {
        title.textContent = product.name;
        title.classList.remove('sr-only');
        const meta = [product.code, product.brand].filter(Boolean).join(' · ');
        render(content, h('div', { class: 'pd' },
            gallery(product),
            h('div', { class: 'pd__info' },
                meta ? h('p', { class: 't-body-sm muted' }, meta) : null,
                title,
                badges(product),
                priceBlock(product.price, { large: true }),
                product.inStock ? null : stockBadge())));

        if (!(ctx.me.features && ctx.me.features.order)) return;
        footer.hidden = false;
        if (!canOrder(ctx, product)) {
            render(footer, h('p', { class: 't-body-sm muted' },
                product.inStock ? 'Bu ürün şu an sipariş edilemiyor.' : 'Stokta olmadığı için şu an sipariş edilemiyor.'));
            return;
        }
        const toCart = h('a', { class: 'app-btn app-btn--ghost btn-touch pd__tocart', href: routePath(ctx.code, 'cart'), hidden: true }, 'Sepete git');
        const control = cartControl(localCtx, product, {
            primary: true,
            addLabel: 'Sepete ekle',
            onQty: q => { toCart.hidden = q === 0; },
        });
        render(footer, h('div', { class: 'pd__actions' }, control.node, toCart), notice);
        unsubscribe = ctx.store.subscribe('cart', lines => {
            const line = lines.find(l => l.key === product.key);
            control.update(line ? line.quantity : 0);
        });
    }

    function load() {
        ctx.api.get('products/detail', { query: { key }, signal: controller.signal })
            .then(show)
            .catch(err => {
                if (controller.signal.aborted) return;
                // The title stays in the dialog (for aria-labelledby) but only for screen readers.
                title.textContent = 'Ürün';
                title.classList.add('sr-only');
                render(content, title, err.status === 404
                    ? emptyState({
                        icon: 'box',
                        title: 'Ürün bulunamadı ya da artık katalogda değil.',
                        action: h('button', { type: 'button', class: 'app-btn app-btn--secondary btn-touch', onclick: onRequestClose }, 'Kataloğa dön'),
                    })
                    : errorState(err, () => {
                        render(content);
                        load();
                    }, 'Ürün yüklenemedi.'));
            });
    }
    load();

    return {
        key,
        close() {
            controller.abort();
            if (unsubscribe) unsubscribe();
            sheet.close();
        },
    };
}
