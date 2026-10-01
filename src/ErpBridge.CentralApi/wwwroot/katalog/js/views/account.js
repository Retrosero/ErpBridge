// Hesabım (/{KOD}/hesap) and its pages (GOAL_MUSTERI_KATALOGU §5.2, §7): the menu follows
// me.features (statement, invoices, purchased); password change and sign-out are always there.
// Ekstre (/hesap/ekstre?bas=&bit=), Faturalarım (/hesap/faturalar) and one invoice
// (/hesap/fatura?key=), Daha önce aldıklarım (/hesap/aldiklarim), Şifremi değiştir (/hesap/sifre).

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import {
    balanceSense, count, date, dateRange, kindLabel, money, percent, vatLabel,
} from '../format.js';
import { qtyLabel } from '../cart.js';
import { errorMessage } from '../messages.js';
import { routePath } from '../route-parse.js';
import {
    band, canOrder, cartControl, changeCart, emptyState, errorState, pageHeader, priceBlock, productMedia, stockBadge,
} from '../ui/components.js';

const DAY = /^\d{4}-\d{2}-\d{2}$/;
const SEARCH_DELAY_MS = 300;
const MIN_QUERY = 2;
const PASSWORD_MIN = 8;
const PASSWORD_MAX = 72;

function backToAccount(ctx) {
    return { href: routePath(ctx.code, 'account'), label: 'Hesabıma dön' };
}

/** "12.345,67 TL Borç" in the balance's colour; the meaning is always written out too. */
function balanceText(amount, cls) {
    const sense = balanceSense(amount);
    return h('span', { class: [cls, 'amount--' + sense.tone] },
        sense.tone === 'neutral' ? money(0) : money(Math.abs(Number(amount))),
        h('span', { class: 'amount__sense' }, ' ' + sense.label));
}

function loadingBlock() {
    return h('div', { class: 'app-card loading-card', 'aria-hidden': 'true' },
        h('div', { class: 'skel skel--line skel--short' }), h('div', { class: 'skel skel--line' }), h('div', { class: 'skel skel--line' }));
}

/**
 * A server-paged list with "Daha fazla göster": opts { path, query(), row(item), empty(), label,
 * errorTitle }. Returns { node, reload(), destroy() }; a reload drops what was shown.
 */
function pagedList(ctx, opts) {
    const list = h('ul', { class: 'rlist', role: 'list', 'aria-label': opts.label });
    const status = h('div');
    const more = h('div', { class: 'more' });
    const node = h('div', { class: 'rlist-host' }, status, list, more);
    let items = 0;
    let total = 0;
    let page = 0;
    let loading = false;
    let controller = null;
    let destroyed = false;

    async function load(reset) {
        if (reset) {
            if (controller) controller.abort();
            items = 0;
            total = 0;
            page = 0;
            render(list);
            render(status, loadingBlock());
        } else if (loading) {
            return;
        }
        const mine = new AbortController();
        controller = mine;
        loading = true;
        drawMore();
        try {
            const res = await ctx.api.get(opts.path, { query: Object.assign(opts.query(), { page: page + 1 }), signal: mine.signal });
            if (destroyed || controller !== mine) return;
            const found = res && Array.isArray(res.items) ? res.items : [];
            page += 1;
            total = Number(res.total) || 0;
            items += found.length;
            list.append(...found.map(opts.row));
            render(status, items === 0 ? opts.empty() : null);
            if (found.length === 0) total = items;
        } catch (err) {
            if (destroyed || controller !== mine || mine.signal.aborted || err.status === 401) return;
            if (items === 0) render(status, errorState(err, () => load(true), opts.errorTitle));
            else ctx.notify(errorMessage(err));
        } finally {
            if (!destroyed && controller === mine) {
                controller = null;
                loading = false;
                drawMore();
            }
        }
    }

    function drawMore() {
        render(more, items > 0 && items < total
            ? h('button', {
                type: 'button',
                class: 'app-btn app-btn--ghost btn-touch more__button',
                'aria-disabled': loading ? 'true' : 'false',
                onclick: () => load(false),
            }, loading ? 'Yükleniyor…' : 'Daha fazla göster (' + count(items) + '/' + count(total) + ')')
            : null);
    }

    load(true);
    return {
        node,
        reload: () => load(true),
        destroy() {
            destroyed = true;
            if (controller) controller.abort();
        },
    };
}

// ---------- Hesabım ----------

export function accountView(ctx) {
    const me = ctx.me;
    const features = me.features || {};
    const customer = me.customer || {};
    const priceList = me.priceList || {};

    const logout = h('button', { type: 'button', class: 'app-btn app-btn--ghost btn-block', onclick: onLogout },
        icon('logout', { size: 20 }), 'Çıkış yap');

    async function onLogout() {
        if (logout.getAttribute('aria-disabled') === 'true') return;
        logout.setAttribute('aria-disabled', 'true');
        try {
            // anonymous: a 401 means the session is already gone, which is what we want anyway.
            await ctx.api.post('logout', undefined, { anonymous: true });
        } catch (err) {
            if (err.status !== 401) {
                logout.setAttribute('aria-disabled', 'false');
                ctx.notify(errorMessage(err));
                return;
            }
        }
        ctx.signedOut();
    }

    const pricing = 'Fiyat listeniz: ' + (priceList.name || 'Liste ' + (priceList.no || '')) + ' (' + vatLabel(priceList.includesVat) + ')'
        + (me.discountPercent > 0 ? ' · İskonto ' + percent(me.discountPercent) : '');

    const entries = [
        features.statement && { name: 'statement', icon: 'ledger', label: 'Ekstre ve bakiye', text: 'Hesap hareketleriniz ve bakiyeniz' },
        features.invoices && { name: 'invoices', icon: 'file', label: 'Faturalarım', text: 'Faturalarınız ve satırları' },
        features.purchased && { name: 'purchased', icon: 'history', label: 'Daha önce aldıklarım', text: 'Aldığınız ürünleri tekrar sepete ekleyin' },
        { name: 'password', icon: 'key', label: 'Şifremi değiştir' },
    ].filter(Boolean);

    const balance = features.statement && me.balance ? me.balance.amount : null;

    const node = h('div', { class: 'page account' },
        h('section', { class: 'app-card account__card', 'aria-label': 'Cari bilgileri' },
            h('h2', { class: 't-title' }, customer.name || ''),
            h('p', { class: 't-body-sm muted' }, [customer.code, me.username].filter(Boolean).join(' · ')),
            h('p', { class: 't-caption muted' }, pricing)),
        balance !== null && balance !== undefined
            ? h('section', { class: 'app-card account__card', 'aria-label': 'Bakiye' },
                h('p', { class: 't-label muted' }, 'Bakiye'),
                balanceText(balance, 't-amount-lg'))
            : null,
        h('nav', { 'aria-label': 'Hesap menüsü' },
            h('ul', { class: 'menu app-card', role: 'list' }, entries.map(e => h('li', null,
                h('a', { class: 'menu__item', href: routePath(ctx.code, e.name) },
                    h('span', { class: 'menu__icon', 'aria-hidden': 'true' }, icon(e.icon, { size: 22 })),
                    h('span', { class: 'menu__text' },
                        h('span', { class: 't-body-strong' }, e.label),
                        e.text ? h('span', { class: 't-caption muted' }, e.text) : null),
                    icon('next', { size: 20, class: 'menu__chevron' })))))),
        logout);

    return { name: 'account', title: 'Hesabım', node };
}

// ---------- Ekstre ----------

const PRESETS = [
    { id: '30g', label: 'Son 30 gün' },
    { id: '3a', label: 'Son 3 ay' },
    { id: 'yil', label: 'Bu yıl' },
];

function ledgerCell(label, content, cls, empty) {
    return h('td', { class: [cls, empty && 'ledger__empty'], 'data-label': label }, content);
}

function ledgerTable(rows, caption) {
    const th = (text, cls) => h('th', { scope: 'col', class: cls }, text);
    return h('table', { class: 'ledger' },
        h('caption', { class: 'sr-only' }, caption),
        h('thead', null, h('tr', null,
            th('Tarih'), th('İşlem'), th('Belge no'), th('Borç', 'num'), th('Alacak', 'num'), th('Bakiye', 'num'))),
        h('tbody', null, rows.map(r => h('tr', null,
            ledgerCell('Tarih', date(r.date), 'ledger__date'),
            ledgerCell('İşlem', kindLabel(r.kind)),
            ledgerCell('Belge no', r.documentNo || '—', 'tnum'),
            ledgerCell('Borç', r.debit ? money(r.debit) : '', 'num amount--debt', !r.debit),
            ledgerCell('Alacak', r.credit ? money(r.credit) : '', 'num amount--credit', !r.credit),
            ledgerCell('Bakiye', balanceText(r.balance), 'num')))));
}

export function statementView(ctx) {
    const query = ctx.route.query;
    const initial = DAY.test(query.bas || '') && DAY.test(query.bit || '') ? { from: query.bas, to: query.bit } : dateRange('3a');
    const today = dateRange('3a').to;
    const fromId = uid('bas');
    const toId = uid('bit');
    const fromInput = h('input', { id: fromId, class: 'app-input', type: 'date', value: initial.from, max: today });
    const toInput = h('input', { id: toId, class: 'app-input', type: 'date', value: initial.to, max: today });
    const chips = h('div', { class: 'chiprow', role: 'group', 'aria-label': 'Hazır tarih aralıkları' });
    const formBand = h('div');
    const body = h('div', { class: 'statement__body' });
    const controller = { current: null };
    let destroyed = false;

    const form = h('form', { class: 'range', onsubmit: e => { e.preventDefault(); apply(fromInput.value, toInput.value); } },
        h('div', { class: 'field' }, h('label', { class: 't-label', for: fromId }, 'Başlangıç'), fromInput),
        h('div', { class: 'field' }, h('label', { class: 't-label', for: toId }, 'Bitiş'), toInput),
        h('button', { type: 'submit', class: 'app-btn app-btn--secondary range__submit' }, 'Göster'));

    function drawChips(from, to) {
        render(chips, PRESETS.map(p => {
            const r = dateRange(p.id);
            return h('button', {
                type: 'button',
                class: 'chip',
                'aria-pressed': r.from === from && r.to === to ? 'true' : 'false',
                onclick: () => {
                    fromInput.value = r.from;
                    toInput.value = r.to;
                    apply(r.from, r.to);
                },
            }, p.label);
        }));
    }

    function apply(from, to) {
        if (!DAY.test(from) || !DAY.test(to)) {
            render(formBand, band('danger', 'Başlangıç ve bitiş tarihini seçin.'));
            return;
        }
        if (from > to) {
            render(formBand, band('danger', 'Başlangıç tarihi bitişten sonra olamaz.'));
            return;
        }
        render(formBand);
        ctx.update(routePath(ctx.code, 'statement', { bas: from, bit: to }));
        drawChips(from, to);
        load(from, to);
    }

    async function load(from, to) {
        if (controller.current) controller.current.abort();
        const mine = new AbortController();
        controller.current = mine;
        render(body, loadingBlock());
        try {
            const res = await ctx.api.get('statement', { query: { from, to }, signal: mine.signal });
            if (destroyed || controller.current !== mine) return;
            const rows = res && Array.isArray(res.rows) ? res.rows : [];
            const balance = res && res.balance !== null && typeof res.balance === 'object' ? res.balance.amount : res && res.balance;
            const range = date(from) + ' – ' + date(to);
            render(body,
                h('section', { class: 'app-card account__card', 'aria-label': 'Güncel bakiye' },
                    h('p', { class: 't-label muted' }, 'Güncel bakiye'),
                    balanceText(balance, 't-amount-lg')),
                rows.length
                    ? ledgerTable(rows, 'Hesap hareketleri, ' + range)
                    : emptyState({ icon: 'ledger', title: 'Bu tarih aralığında hareket yok.', text: range }));
        } catch (err) {
            if (destroyed || controller.current !== mine || mine.signal.aborted || err.status === 401) return;
            render(body, errorState(err, () => load(from, to), 'Ekstre yüklenemedi.'));
        }
    }

    drawChips(initial.from, initial.to);
    load(initial.from, initial.to);

    return {
        name: 'statement',
        title: 'Ekstre ve bakiye',
        header: pageHeader('Ekstre ve bakiye', backToAccount(ctx)),
        node: h('div', { class: 'page statement' }, chips, form, formBand, body),
        destroy() {
            destroyed = true;
            if (controller.current) controller.current.abort();
        },
    };
}

// ---------- Faturalar ----------

export function invoicesView(ctx) {
    const paged = pagedList(ctx, {
        path: 'invoices',
        label: 'Faturalarınız',
        errorTitle: 'Faturalar yüklenemedi.',
        query: () => ({}),
        empty: () => emptyState({ icon: 'file', title: 'Henüz faturanız yok.' }),
        row: inv => h('li', null,
            h('a', { class: 'rrow app-card', href: routePath(ctx.code, 'invoice', { key: inv.key }) },
                h('span', { class: 'rrow__main' },
                    h('span', { class: 't-title-sm tnum' }, inv.documentNo || '—'),
                    h('span', { class: 't-body-sm muted' }, date(inv.date) + ' · ' + kindLabel(inv.kind))),
                h('span', { class: 't-amount' }, money(Math.abs(Number(inv.total) || 0))),
                icon('next', { size: 20, class: 'menu__chevron' }))),
    });
    return {
        name: 'invoices',
        title: 'Faturalarım',
        header: pageHeader('Faturalarım', backToAccount(ctx)),
        node: h('div', { class: 'page' }, paged.node),
        destroy: paged.destroy,
    };
}

export function invoiceView(ctx) {
    const key = ctx.route.query.key || '';
    const body = h('div');
    const controller = new AbortController();
    const ordering = !!(ctx.me.features && ctx.me.features.order);

    /** The invoice line's quantity goes into the cart, rounded up to the carton where needed. */
    async function addToCart(line, button) {
        if (button.getAttribute('aria-disabled') === 'true') return;
        button.setAttribute('aria-disabled', 'true');
        try {
            const product = await ctx.api.get('products/detail', { query: { key: line.productKey } });
            if (!canOrder(ctx, product)) {
                ctx.notify(product.inStock ? 'Bu ürün şu an sipariş edilemiyor.' : 'Bu ürün şu an stokta yok.');
                return;
            }
            const before = ctx.cartQty(product.key);
            const after = changeCart(ctx, product, before + Math.max(1, Math.ceil(Number(line.quantity) || 1)));
            if (before > 0 && after > before) ctx.notify('Sepette artık ' + qtyLabel(product, after) + ' var.', { label: 'Sepete git', href: routePath(ctx.code, 'cart') });
        } catch (err) {
            if (err.status === 401) return;
            ctx.notify(err.status === 404 ? 'Bu ürün artık katalogda değil.' : errorMessage(err));
        } finally {
            button.setAttribute('aria-disabled', 'false');
        }
    }

    function show(inv) {
        const lines = Array.isArray(inv.lines) ? inv.lines : [];
        render(body,
            h('section', { class: 'app-card account__card', 'aria-label': 'Fatura bilgileri' },
                h('h2', { class: 't-title tnum' }, inv.documentNo || 'Fatura'),
                h('p', { class: 't-body-sm muted' }, date(inv.date) + ' · ' + kindLabel(inv.kind)),
                h('p', { class: 't-amount-lg' }, money(Math.abs(Number(inv.total) || 0)))),
            h('section', { class: 'app-card account__card', 'aria-label': 'Fatura satırları' },
                h('h2', { class: 't-title-sm' }, 'Satırlar (' + count(lines.length) + ')'),
                lines.length
                    ? h('ul', { class: 'olines', role: 'list' }, lines.map(line => {
                        const add = ordering && line.productKey
                            ? h('button', {
                                type: 'button',
                                class: 'app-btn app-btn--secondary app-btn--sm',
                                'aria-label': 'Sepete ekle: ' + (line.name || line.code),
                                onclick: e => addToCart(line, e.currentTarget),
                            }, icon('plus', { size: 18 }), 'Sepete ekle')
                            : null;
                        return h('li', { class: 'oline' },
                            h('div', { class: 'oline__main' },
                                h('p', { class: 't-body-strong' }, line.name || line.code || ''),
                                h('p', { class: 't-body-sm muted tnum' },
                                    [line.code, count(line.quantity) + ' × ' + money(line.unitPrice)].filter(Boolean).join(' · ')),
                                add),
                            h('p', { class: 't-amount' }, money(line.amount)));
                    }))
                    : h('p', { class: 't-body-sm muted' }, 'Bu faturada satır yok.')));
    }

    function load() {
        render(body, loadingBlock());
        ctx.api.get('invoices/detail', { query: { key }, signal: controller.signal })
            .then(show)
            .catch(err => {
                if (controller.signal.aborted || err.status === 401) return;
                render(body, err.status === 404
                    ? emptyState({
                        icon: 'file',
                        title: 'Fatura bulunamadı.',
                        action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'invoices') }, 'Faturalarıma dön'),
                    })
                    : errorState(err, load, 'Fatura yüklenemedi.'));
            });
    }
    load();

    return {
        name: 'invoice',
        title: 'Fatura',
        header: pageHeader('Fatura', { href: routePath(ctx.code, 'invoices'), label: 'Faturalarıma dön' }),
        node: h('div', { class: 'page' }, body),
        destroy() {
            controller.abort();
        },
    };
}

// ---------- Daha önce aldıklarım ----------

// Fewer than two characters search nothing, like the catalogue.
function effective(query) {
    return query.length >= MIN_QUERY ? query : '';
}

export function purchasedView(ctx) {
    const controls = new Map();
    let q = String(ctx.route.query.q || '').trim();
    let timer = 0;

    const searchId = uid('pq');
    const input = h('input', {
        id: searchId,
        class: 'app-input app-search',
        type: 'search',
        inputmode: 'search',
        enterkeyhint: 'search',
        autocomplete: 'off',
        spellcheck: 'false',
        placeholder: 'Ürün adı ya da kodu',
        value: q,
    });

    function row(item) {
        const p = item.product;
        const facts = 'Son alış ' + date(item.lastDate) + ' · ' + count(item.times) + ' kez · toplam ' + count(item.totalQuantity);
        let action = null;
        if (p && canOrder(ctx, p)) {
            const control = cartControl(ctx, p, { addLabel: 'Tekrar sepete ekle' });
            controls.set(p.key, control);
            action = h('div', { class: 'prow__action' }, control.node);
        }
        return h('li', { class: 'prow app-card' },
            h('div', { class: 'prow__media' }, productMedia(p ? p.thumb : null, '', false)),
            h('div', { class: 'prow__body' },
                h('p', { class: 't-title-sm prow__name' }, item.name || (p && p.name) || item.code),
                h('p', { class: 't-body-sm muted tnum' }, item.code),
                h('p', { class: 't-caption muted' }, facts),
                p ? priceBlock(p.price, { small: true }) : h('p', { class: 't-caption muted' }, 'Bu ürün şu an katalogda yok.'),
                p && !p.inStock ? stockBadge() : null),
            action);
    }

    const paged = pagedList(ctx, {
        path: 'purchased',
        label: 'Daha önce aldığınız ürünler',
        errorTitle: 'Liste yüklenemedi.',
        query: () => ({ q: effective(q) || null }),
        empty: () => effective(q)
            ? emptyState({ title: '“' + q + '” ile eşleşen ürün yok.', text: 'Farklı bir kelimeyle ya da ürün koduyla arayın.' })
            : emptyState({ icon: 'history', title: 'Henüz aldığınız ürün görünmüyor.', text: 'Faturalarınızdaki ürünler burada listelenir.' }),
        row,
    });

    function search(value) {
        const next = value.trim();
        const changed = effective(next) !== effective(q);
        q = next;
        if (!changed) return;
        ctx.update(routePath(ctx.code, 'purchased', { q: effective(q) || null }));
        controls.clear();
        paged.reload();
    }
    input.addEventListener('input', () => {
        clearTimeout(timer);
        timer = setTimeout(() => search(input.value), SEARCH_DELAY_MS);
    });
    input.addEventListener('keydown', e => {
        if (e.key !== 'Enter') return;
        clearTimeout(timer);
        search(input.value);
    });

    const unsubscribe = ctx.store.subscribe('cart', lines => {
        const quantities = new Map(lines.map(l => [l.key, l.quantity]));
        controls.forEach((control, key) => control.update(quantities.get(key) || 0));
    });

    return {
        name: 'purchased',
        title: 'Daha önce aldıklarım',
        header: pageHeader('Daha önce aldıklarım', backToAccount(ctx)),
        node: h('div', { class: 'page purchased' },
            h('div', { class: 'field', role: 'search' },
                h('label', { class: 't-label', for: searchId }, 'Aldıklarınızda arayın'), input),
            paged.node),
        destroy() {
            clearTimeout(timer);
            paged.destroy();
            unsubscribe();
        },
    };
}

// ---------- Şifremi değiştir ----------

function byteLength(text) {
    return new TextEncoder().encode(text).length;
}

export function passwordView(ctx) {
    const ids = { current: uid('pw'), next: uid('pw'), repeat: uid('pw'), hint: uid('pwh') };
    const field = (id, autocomplete) => h('input', {
        id,
        class: 'app-input',
        type: 'password',
        autocomplete,
        maxlength: String(PASSWORD_MAX),
        required: true,
    });
    const current = field(ids.current, 'current-password');
    const next = field(ids.next, 'new-password');
    const repeat = field(ids.repeat, 'new-password');
    next.setAttribute('aria-describedby', ids.hint);
    const result = h('div');
    const submit = h('button', { type: 'submit', class: 'app-btn app-btn--primary btn-block' }, 'Şifreyi değiştir');
    let busy = false;
    let destroyed = false;

    function fail(text, focus) {
        render(result, band('danger', text));
        if (focus) focus.focus();
    }

    async function onSubmit(e) {
        e.preventDefault();
        if (busy) return;
        if (!current.value || !next.value || !repeat.value) {
            fail('Üç alanı da doldurun.', !current.value ? current : !next.value ? next : repeat);
            return;
        }
        const size = byteLength(next.value);
        if (size < PASSWORD_MIN || size > PASSWORD_MAX) {
            fail(errorMessage({ code: 'INVALID_PASSWORD' }), next);
            return;
        }
        if (next.value !== repeat.value) {
            fail('Yeni şifre ile tekrarı aynı değil.', repeat);
            return;
        }
        busy = true;
        submit.setAttribute('aria-disabled', 'true');
        submit.textContent = 'Değiştiriliyor…';
        render(result);
        try {
            await ctx.api.post('password', { current: current.value, next: next.value });
            if (destroyed) return;
            current.value = '';
            next.value = '';
            repeat.value = '';
            render(result, band('success', 'Şifreniz değiştirildi.', 'Diğer cihazlarda yeni şifrenizle yeniden giriş yapmanız gerekir.'));
        } catch (err) {
            if (destroyed || err.status === 401) return;
            if (err.code === 'INVALID_CREDENTIALS') {
                current.value = '';
                fail(errorMessage(err, 'password'), current);
            } else {
                fail(errorMessage(err, 'password'), err.code === 'INVALID_PASSWORD' ? next : null);
            }
        } finally {
            busy = false;
            submit.setAttribute('aria-disabled', 'false');
            submit.textContent = 'Şifreyi değiştir';
        }
    }

    const node = h('div', { class: 'page narrow' },
        h('form', { class: 'app-card form-card', novalidate: true, onsubmit: onSubmit },
            h('div', { class: 'field' }, h('label', { class: 't-label', for: ids.current }, 'Mevcut şifre'), current),
            h('div', { class: 'field' }, h('label', { class: 't-label', for: ids.next }, 'Yeni şifre'), next,
                h('p', { class: 't-caption muted', id: ids.hint }, 'En az ' + PASSWORD_MIN + ', en çok ' + PASSWORD_MAX + ' karakter.')),
            h('div', { class: 'field' }, h('label', { class: 't-label', for: ids.repeat }, 'Yeni şifre (tekrar)'), repeat),
            result,
            submit));

    return {
        name: 'password',
        title: 'Şifremi değiştir',
        header: pageHeader('Şifremi değiştir', backToAccount(ctx)),
        node,
        destroy() {
            destroyed = true;
        },
    };
}
