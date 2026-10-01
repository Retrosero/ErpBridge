// Siparişlerim (/{KOD}/siparisler) and one request (/{KOD}/siparisler/detay?id=): the customer's
// requests with their status in the customer's words (GOAL_MUSTERI_KATALOGU §5.3). Colour always
// comes with an icon and text. The list refreshes when the tab comes back after a minute away.

import { h, render } from '../dom.js';
import { icon } from '../icons.js';
import { count, dateTime, money } from '../format.js';
import { orderStatus } from '../order.js';
import { routePath } from '../route-parse.js';
import { band, emptyState, errorState, pageHeader } from '../ui/components.js';

const STALE_MS = 60 * 1000;

function statusBadge(status) {
    const s = orderStatus(status);
    return h('span', { class: ['app-badge', 'app-badge--' + s.tone] }, icon(s.icon, { size: 14 }), s.label);
}

function loadingRows(n) {
    const out = [];
    for (let i = 0; i < n; i++) {
        out.push(h('li', { class: 'orow app-card', 'aria-hidden': 'true' },
            h('div', { class: 'skel skel--line skel--short' }), h('div', { class: 'skel skel--line' })));
    }
    return h('ul', { class: 'olist', role: 'list' }, out);
}

function orderRow(ctx, order) {
    return h('li', null,
        h('a', { class: 'orow app-card', href: routePath(ctx.code, 'order', { id: order.id }) },
            h('span', { class: 'orow__head' },
                h('span', { class: 't-title-sm tnum' }, order.no || 'Talep'),
                statusBadge(order.status)),
            h('span', { class: 't-body-sm muted' }, dateTime(order.submittedAtMs) + ' · ' + count(order.lineCount) + ' kalem'),
            h('span', { class: 't-amount orow__total' }, money(order.total)),
            order.status === 'REJECTED' && order.rejectReason
                ? h('span', { class: 't-body-sm orow__reason' }, 'Red nedeni: ' + order.rejectReason)
                : null));
}

export function ordersView(ctx) {
    // Arriving from the cart right after sending: say so once, then forget it (a reload must not repeat it).
    const placed = history.state && history.state.placed;
    if (placed) history.replaceState(null, '', location.pathname + location.search);

    const head = placed
        ? band('success', 'Talebiniz alındı' + (placed !== '-' ? ': ' + placed : ''),
            'Firmanız talebinizi inceleyip siparişe çevirdiğinde durumu burada görünür.')
        : null;
    const body = h('div', { class: 'orders__body' });
    const node = h('div', { class: 'page orders' }, head, body);
    let destroyed = false;
    let controller = null;
    let loadedAt = 0;

    async function load(quiet) {
        if (controller) controller.abort();
        const mine = new AbortController();
        controller = mine;
        if (!quiet) render(body, loadingRows(3));
        body.setAttribute('aria-busy', 'true');
        try {
            const res = await ctx.api.get('orders', { signal: mine.signal });
            if (destroyed || controller !== mine) return;
            loadedAt = Date.now();
            const items = res && Array.isArray(res.items) ? res.items : [];
            render(body, items.length
                ? h('ul', { class: 'olist', role: 'list', 'aria-label': 'Sipariş talepleriniz' }, items.map(o => orderRow(ctx, o)))
                : emptyState({
                    icon: 'orders',
                    title: 'Henüz sipariş talebiniz yok.',
                    text: 'Katalogdan ürün seçip sepetinizden talep gönderebilirsiniz.',
                    action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'catalog') }, 'Kataloğa göz at'),
                }));
        } catch (err) {
            if (destroyed || controller !== mine || mine.signal.aborted || err.status === 401) return;
            // A quiet refresh keeps the list it already shows.
            if (!quiet) render(body, errorState(err, () => load(false), 'Siparişleriniz yüklenemedi.'));
        } finally {
            if (controller === mine) {
                controller = null;
                body.removeAttribute('aria-busy');
            }
        }
    }

    const onVisible = () => {
        if (document.visibilityState === 'visible' && loadedAt && Date.now() - loadedAt > STALE_MS) load(true);
    };
    document.addEventListener('visibilitychange', onVisible);
    load(false);

    return {
        name: 'orders',
        title: 'Siparişlerim',
        node,
        destroy() {
            destroyed = true;
            if (controller) controller.abort();
            document.removeEventListener('visibilitychange', onVisible);
        },
    };
}

export function orderView(ctx) {
    const id = ctx.route.query.id || '';
    const body = h('div', { class: 'orders__body' });
    const node = h('div', { class: 'page odetail' }, body);
    const controller = new AbortController();

    function show(order) {
        const lines = Array.isArray(order.lines) ? order.lines : [];
        render(body,
            h('section', { class: 'app-card odetail__card', 'aria-label': 'Talep bilgileri' },
                h('div', { class: 'orow__head' }, h('h2', { class: 't-title tnum' }, order.no || 'Talep'), statusBadge(order.status)),
                h('p', { class: 't-body-sm muted' }, 'Gönderildi: ' + dateTime(order.submittedAtMs)),
                order.status === 'REJECTED'
                    ? band('danger', 'Talebiniz reddedildi.', order.rejectReason ? 'Red nedeni: ' + order.rejectReason : null)
                    : null,
                order.status === 'COMPLETED' ? band('success', 'Talebiniz siparişe çevrildi.') : null,
                order.note ? h('div', { class: 'odetail__note' },
                    h('p', { class: 't-label' }, 'Notunuz'),
                    h('p', { class: 't-body' }, order.note)) : null),
            h('section', { class: 'app-card odetail__card', 'aria-label': 'Ürünler' },
                h('h2', { class: 't-title-sm' }, 'Ürünler (' + count(lines.length) + ')'),
                h('ul', { class: 'olines', role: 'list' }, lines.map(line => h('li', { class: 'oline' },
                    h('div', { class: 'oline__main' },
                        h('p', { class: 't-body-strong' }, line.name || line.code || ''),
                        h('p', { class: 't-body-sm muted tnum' },
                            [line.code, count(line.quantity) + ' × ' + money(line.net)].filter(Boolean).join(' · '))),
                    h('p', { class: 't-amount' }, money(line.total))))),
                h('div', { class: 'odetail__total' },
                    h('span', { class: 't-title-sm' }, 'Genel toplam'),
                    h('span', { class: 't-amount-lg' }, money(order.total))),
                h('p', { class: 't-caption muted' }, 'Tutarlar talep anındaki fiyatlarla hesaplanmıştır; kesin tutar firma onayıyla belirlenir.')));
    }

    function load() {
        render(body, loadingRows(2));
        ctx.api.get('orders/detail', { query: { id }, signal: controller.signal })
            .then(show)
            .catch(err => {
                if (controller.signal.aborted || err.status === 401) return;
                render(body, err.status === 404
                    ? emptyState({
                        icon: 'orders',
                        title: 'Talep bulunamadı.',
                        action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'orders') }, 'Siparişlerime dön'),
                    })
                    : errorState(err, load, 'Talep yüklenemedi.'));
            });
    }
    load();

    return {
        name: 'order',
        title: 'Sipariş talebi',
        header: pageHeader('Sipariş talebi', { href: routePath(ctx.code, 'orders'), label: 'Siparişlerime dön' }),
        node,
        destroy() {
            controller.abort();
        },
    };
}
