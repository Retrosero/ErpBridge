// Sepet (/{KOD}/sepet): the cart from the store, checked with POST cart/quote when the page opens and
// after every change (GOAL_MUSTERI_KATALOGU §5.2). Lines show the server's prices and issues, the
// totals are the quote's, and "Siparişi gönder" sends the last quote's total as expectedTotal with a
// requestId tied to the cart's content: a double tap or a retry after a lost answer finds the same
// request on the server instead of creating a second one.

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import { count, money } from '../format.js';
import { MAX_LINES, boxOf, normalizeQty, qtyLabel, removeLine, unitText } from '../cart.js';
import {
    cartSignature, createRequestIds, issueCount, newRequestId, quoteLinesByKey, refreshLines, requestLines, singleFlight,
} from '../order.js';
import { errorMessage, issueMessage, supportCode } from '../messages.js';
import { routePath } from '../route-parse.js';
import {
    band, changeCart, createStepper, emptyState, priceBlock, productMedia,
} from '../ui/components.js';

const QUOTE_DELAY_MS = 300;
const NOTE_MAX = 1000;
// Issues only taking the product out can fix.
const REMOVE_ONLY = new Set(['NOT_AVAILABLE', 'OUT_OF_STOCK']);

// These outlive the page: leaving the cart and coming back keeps the request id and the note.
const requestIds = createRequestIds(() => newRequestId());
let draft = { owner: null, note: '' };

export function cartView(ctx) {
    const owner = ctx.code + ':' + ctx.me.username;
    if (draft.owner !== owner) draft = { owner, note: '' };
    const includesVat = !!(ctx.me.priceList && ctx.me.priceList.includesVat);

    let lines = ctx.store.get('cart') || [];
    let quote = null;
    let quoteSig = null;
    let quoteError = null;
    let quoting = false;
    let controller = null;
    let timer = 0;
    let sending = false;
    let retry = false;
    let closed = false;
    let placedNo = null;
    let online = navigator.onLine !== false;
    let destroyed = false;
    const rows = new Map();

    // ---------- Layout ----------
    const linesTitleId = uid('ct');
    const countLine = h('p', { class: 't-body-sm muted' });
    const topBand = h('div', { class: 'cart__band' });
    const list = h('ul', { class: 'clines', role: 'list' });
    // Disabled while the order is on its way, so the content cannot change under it.
    const fieldset = h('fieldset', { class: 'cart__lines' }, h('legend', { class: 'sr-only' }, 'Sepetteki ürünler'), list);

    const totalsHost = h('div', { class: 'ctotals-host' });
    const totalLive = h('p', { class: 'sr-only', role: 'status' });
    const noteId = uid('note');
    const noteCountId = uid('nc');
    const noteCount = h('p', { class: 't-caption muted note__count', id: noteCountId });
    const noteInput = h('textarea', {
        id: noteId,
        class: 'app-input note__input',
        rows: '3',
        maxlength: String(NOTE_MAX),
        'aria-describedby': noteCountId,
        value: draft.note,
    });
    noteInput.addEventListener('input', () => {
        draft = { owner, note: noteInput.value };
        drawNoteCount();
    });

    const reasonId = uid('why');
    const notice = h('div', { class: 'submitbar__notice' });
    const reason = h('p', { class: 't-caption submitbar__reason', id: reasonId });
    const sendButton = h('button', {
        type: 'button',
        class: 'app-btn app-btn--primary btn-block submitbar__btn',
        'aria-describedby': reasonId,
        onclick: () => submit(),
    }, 'Siparişi gönder');

    const layout = h('div', { class: 'cart' },
        h('section', { class: 'cart__main', 'aria-labelledby': linesTitleId },
            h('div', { class: 'cart__head' }, h('h2', { class: 't-title', id: linesTitleId }, 'Ürünler'), countLine),
            topBand, fieldset),
        h('div', { class: 'cart__side' },
            h('section', { class: 'app-card csummary', 'aria-label': 'Tutar özeti' },
                h('h2', { class: 't-title-sm' }, 'Tutar özeti'),
                totalsHost, totalLive,
                h('p', { class: 't-caption muted' },
                    'Birim fiyatlar ' + (includesVat ? 'KDV dahildir' : 'KDV hariçtir')
                    + '. Tutarlar tahminidir; talebiniz firma onayıyla kesinleşir.')),
            h('div', { class: 'field note' },
                h('label', { class: 't-label', for: noteId }, 'Sipariş notu (isteğe bağlı)'),
                noteInput, noteCount),
            h('div', { class: 'submitbar' }, notice, reason, sendButton)));
    const node = h('div', { class: 'cart-page' });

    // ---------- Lines ----------
    function fresh() {
        return !!quote && quoteSig === cartSignature(lines);
    }

    function createRow() {
        const titleId = uid('cl');
        const media = h('div', { class: 'cline__media' });
        const name = h('h3', { class: 't-title-sm cline__name', id: titleId });
        const meta = h('p', { class: 't-body-sm muted' });
        const price = h('div', { class: 'cline__price' });
        const issue = h('div', { class: 'cline__issue' });
        const stepHost = h('div', { class: 'cline__step' });
        const qtyText = h('p', { class: 't-caption muted cline__qty' });
        const total = h('p', { class: 't-amount cline__total' });
        const remove = h('button', { type: 'button', class: 'icon-btn cline__remove', onclick: () => removeWithUndo(row.line) },
            icon('trash'));
        const row = {
            node: h('li', { class: 'cline app-card', 'aria-labelledby': titleId },
                media,
                h('div', { class: 'cline__body' }, name, meta, price, issue),
                remove,
                h('div', { class: 'cline__foot' }, h('div', { class: 'cline__qtycol' }, stepHost, qtyText), total)),
            line: null,
            remove,
            update,
        };
        let stepper = null;
        let stepperBox = null;
        let thumb;

        function update(line, ql, isFresh) {
            const p = line.product;
            row.line = line;
            if (thumb !== p.thumb) {
                thumb = p.thumb;
                render(media, productMedia(p.thumb, '', false));
            }
            name.textContent = p.name;
            remove.setAttribute('aria-label', 'Sepetten çıkar: ' + p.name);
            const box = boxOf(p);
            meta.textContent = [p.code, box ? (box.only ? 'Yalnız koli' : 'Koli') + ' · ' + count(box.qty) + ' ' + unitText(p) : '']
                .filter(Boolean).join(' · ');
            render(price, priceBlock(p.price, { small: true }));

            const code = isFresh && ql ? ql.issue : null;
            row.node.classList.toggle('cline--issue', !!code);
            render(issue, code ? issueNode(line, code) : null);

            // A new carton rule from the server needs a stepper that moves by the new step.
            const boxKey = box ? box.qty + (box.only ? '!' : '') : '';
            if (!stepper || stepperBox !== boxKey) {
                stepperBox = boxKey;
                stepper = createStepper(p, p.name, requested => changeQty(row.line, requested));
                render(stepHost, stepper.node);
            }
            const removeOnly = !!code && REMOVE_ONLY.has(code);
            if (removeOnly && stepHost.contains(document.activeElement)) remove.focus();
            stepHost.hidden = removeOnly;
            stepper.set(line.quantity);
            qtyText.textContent = qtyLabel(p, line.quantity);

            total.textContent = code ? '—' : ql ? money(ql.total) : '…';
            total.classList.toggle('is-stale', !isFresh);
        }

        return row;
    }

    function issueNode(line, code) {
        const fixed = normalizeQty(line.product, line.quantity);
        const canFix = !REMOVE_ONLY.has(code) && fixed > 0 && fixed !== line.quantity;
        return h('div', { class: 'cline__issuebox' },
            h('p', { class: 'cline__issuetext' }, icon('alert', { size: 16 }), issueMessage(code)),
            canFix
                ? h('button', {
                    type: 'button',
                    class: 'app-btn app-btn--secondary app-btn--sm',
                    onclick: () => changeCart(ctx, line.product, fixed),
                }, code === 'CARTON_MULTIPLE' ? 'Koli katına yuvarla (' + qtyLabel(line.product, fixed) + ')' : 'Miktarı düzelt')
                : null,
            REMOVE_ONLY.has(code)
                ? h('button', { type: 'button', class: 'app-btn app-btn--ghost app-btn--sm', onclick: () => removeWithUndo(line) }, 'Sepetten çıkar')
                : null);
    }

    function drawLines() {
        const byKey = quoteLinesByKey(quote);
        const isFresh = fresh();
        const keep = new Set(lines.map(l => l.key));
        rows.forEach((row, key) => {
            if (!keep.has(key)) {
                row.node.remove();
                rows.delete(key);
            }
        });
        let previous = null;
        for (const line of lines) {
            let row = rows.get(line.key);
            if (!row) {
                row = createRow();
                rows.set(line.key, row);
            }
            row.update(line, byKey.get(line.key) || null, isFresh);
            const expected = previous ? previous.nextSibling : list.firstChild;
            if (expected !== row.node) list.insertBefore(row.node, expected);
            previous = row.node;
        }
        countLine.textContent = count(lines.length) + ' ürün';
    }

    function changeQty(line, requested) {
        if (requested <= 0) removeWithUndo(line);
        else changeCart(ctx, line.product, requested);
    }

    /** Takes the line out at once; "Geri al" puts it back where it was (DESIGN_SYSTEM §10.2: no dialog). */
    function removeWithUndo(line) {
        const current = ctx.store.get('cart') || [];
        const index = current.findIndex(l => l.key === line.key);
        if (index < 0) return;
        const row = rows.get(line.key);
        const hadFocus = !!row && row.node.contains(document.activeElement);
        ctx.store.set('cart', removeLine(current, line.key));
        if (hadFocus) focusAfterRemoval(index);
        ctx.notify(line.product.name + ' sepetten çıkarıldı.', {
            label: 'Geri al',
            onClick: () => {
                const now = ctx.store.get('cart') || [];
                if (now.some(l => l.key === line.key) || now.length >= MAX_LINES) return;
                const next = now.slice();
                next.splice(Math.min(index, next.length), 0, line);
                ctx.store.set('cart', next);
            },
        });
    }

    function focusAfterRemoval(index) {
        const next = lines[Math.min(index, lines.length - 1)];
        const row = next ? rows.get(next.key) : null;
        if (row) row.remove.focus();
        else {
            const link = node.querySelector('a');
            if (link) link.focus();
        }
    }

    // ---------- Totals, note, send ----------
    function totalRow(label, value, cls) {
        return h('div', { class: ['ctotals__row', cls] }, h('dt', null, label), h('dd', { class: 'tnum' }, value));
    }

    function drawSummary() {
        const isFresh = fresh();
        const totals = quote && quote.totals;
        let content;
        if (quoteError && !quoting && !isFresh) {
            content = band('danger', 'Tutarlar hesaplanamadı.', errorMessage(quoteError),
                h('button', {
                    type: 'button',
                    class: 'app-btn app-btn--secondary app-btn--sm',
                    onclick: () => {
                        scheduleQuote(0);
                        drawSummary();
                    },
                }, 'Tekrar dene'));
        } else if (!totals) {
            content = h('p', { class: 't-body-sm muted' }, 'Tutarlar hesaplanıyor…');
        } else {
            content = h('dl', { class: ['ctotals', !isFresh && 'is-stale'], 'aria-busy': isFresh ? null : 'true' },
                totalRow('Brüt tutar (KDV hariç)', money(totals.gross)),
                totals.discount > 0 ? totalRow('İskonto', '−' + money(totals.discount), 'ctotals__row--discount') : null,
                totalRow('KDV', money(totals.vat)),
                totalRow('Genel toplam (KDV dahil)', money(totals.total), 'ctotals__row--total'));
        }
        render(totalsHost, content);

        const why = sending ? '' : blocker();
        reason.textContent = why;
        reason.hidden = !why;
        sendButton.setAttribute('aria-disabled', sending || why ? 'true' : 'false');
        sendButton.setAttribute('aria-busy', sending ? 'true' : 'false');
        sendButton.textContent = sending ? 'Gönderiliyor…' : retry ? 'Tekrar dene' : 'Siparişi gönder';
    }

    /** Why the order cannot go out right now, or '' when it can. */
    function blocker() {
        if (closed) return errorMessage({ code: 'ORDERING_DISABLED' });
        if (!online) return 'İnternet bağlantısı yok. Bağlantı gelince gönderebilirsiniz.';
        if (!fresh()) return quoteError && !quoting ? 'Tutarlar hesaplanamadı; yeniden deneyin.' : 'Tutarlar hesaplanıyor…';
        const n = issueCount(quote);
        return n ? 'Sepette düzeltilmesi gereken ' + count(n) + ' ürün var.' : '';
    }

    function drawNoteCount() {
        noteCount.textContent = count(noteInput.value.length) + '/' + count(NOTE_MAX);
    }

    function draw() {
        if (destroyed) return;
        if (placedNo !== null) {
            render(node, emptyState({
                icon: 'check',
                title: 'Talebiniz alındı',
                text: placedNo ? 'Talep no: ' + placedNo : null,
                action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'orders') }, 'Siparişlerim'),
            }));
            return;
        }
        if (!lines.length) {
            render(node, emptyState({
                icon: 'cart',
                title: 'Sepetiniz boş',
                text: 'Ürün eklemek için kataloğa göz atın.',
                action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'catalog') }, 'Kataloğa git'),
            }));
            return;
        }
        if (layout.parentNode !== node) render(node, layout);
        drawLines();
        drawSummary();
    }

    // ---------- Quote ----------
    function scheduleQuote(delay = QUOTE_DELAY_MS) {
        clearTimeout(timer);
        if (!lines.length || fresh()) {
            quoting = false;
            return;
        }
        quoting = true;
        quoteError = null;
        timer = setTimeout(runQuote, delay);
    }

    async function runQuote() {
        if (controller) controller.abort();
        const sent = lines;
        if (!sent.length) return;
        const sig = cartSignature(sent);
        const mine = new AbortController();
        controller = mine;
        quoting = true;
        drawSummary();
        try {
            const res = await ctx.api.post('cart/quote', { lines: requestLines(sent) }, { signal: mine.signal });
            if (destroyed || controller !== mine) return;
            acceptQuote(res, sig, false);
        } catch (err) {
            if (destroyed || controller !== mine || mine.signal.aborted || err.status === 401) return;
            quoteError = err;
        } finally {
            if (!destroyed && controller === mine) {
                controller = null;
                quoting = false;
                draw();
            }
        }
    }

    /**
     * Takes a quote for the content `sig` (from cart/quote, or from a 409/422 answer to the order):
     * the cart's snapshots follow the server's figures; quantities are never changed here.
     */
    function acceptQuote(res, sig, fromOrder) {
        if (!res || !Array.isArray(res.lines) || !res.totals) return;
        quote = res;
        quoteSig = sig;
        quoteError = null;
        totalLive.textContent = 'Genel toplam ' + money(res.totals.total);
        const current = ctx.store.get('cart') || [];
        if (cartSignature(current) !== sig) return;
        const refreshed = refreshLines(current, res);
        if (refreshed.priceChanged > 0 && !fromOrder) {
            render(topBand, band('warning', 'Fiyatlar güncellendi',
                count(refreshed.priceChanged) + ' ürünün fiyatı değişti; aşağıdaki tutarlar günceldir.'));
        }
        if (refreshed.lines !== current) ctx.store.set('cart', refreshed.lines);
        else draw();
    }

    // ---------- Send ----------
    function showNotice(content) {
        render(notice, content);
    }

    const submit = singleFlight(async () => {
        if (sending || blocker()) return;
        const sent = lines;
        const sig = cartSignature(sent);
        const body = {
            requestId: requestIds.idFor(owner + '|' + sig),
            lines: requestLines(sent),
            note: noteInput.value.trim().slice(0, NOTE_MAX) || null,
            expectedTotal: quote.totals.total,
        };
        setSending(true);
        showNotice(null);
        try {
            const res = await ctx.api.post('orders', body);
            placed(res && res.order);
        } catch (err) {
            failed(err, sig);
        } finally {
            setSending(false);
        }
    });

    function setSending(on) {
        sending = on;
        fieldset.disabled = on;
        noteInput.readOnly = on;
        if (!destroyed && placedNo === null) drawSummary();
    }

    function placed(order) {
        const no = order && typeof order.no === 'string' ? order.no : '';
        requestIds.forget();
        draft = { owner, note: '' };
        const me = ctx.store.get('me');
        placedNo = no;
        if (me && ctx.code + ':' + me.username === owner) ctx.store.set('cart', []);
        if (destroyed) {
            // The customer moved on while it was being sent: tell them where it went.
            ctx.notify('Talebiniz alındı' + (no ? ': ' + no : '') + '.', { label: 'Siparişlerim', href: routePath(ctx.code, 'orders') });
            return;
        }
        draw();
        ctx.navigate(routePath(ctx.code, 'orders'), { state: { placed: no || '-' } });
    }

    function failed(err, sig) {
        if (err.status === 401) return; // the login page is already on its way; the cart is kept
        if (destroyed) {
            ctx.notify(errorMessage(err));
            return;
        }
        retry = false;
        const q = err.body && err.body.quote;
        if ((err.code === 'PRICE_CHANGED' || err.code === 'CART_INVALID') && q) {
            acceptQuote(q, sig, true);
            showNotice(err.code === 'PRICE_CHANGED'
                ? band('warning', 'Fiyatlar güncellendi', q.totals ? 'Yeni genel toplam ' + money(q.totals.total) + '. Kontrol edip yeniden gönderin.' : null)
                : band('danger', errorMessage(err), 'İşaretli ürünleri düzeltin ya da sepetten çıkarın.'));
            return;
        }
        if (err.code === 'ORDERING_DISABLED') closed = true;
        // No answer, or the server failed: the same requestId goes again, so a retry cannot double the order.
        retry = err.status === 0 || err.status >= 500;
        const support = supportCode(err);
        showNotice(band('danger', errorMessage(err), support ? 'Destek kodu: ' + support : null,
            err.code === 'TOO_MANY_OPEN_ORDERS'
                ? h('a', { class: 'band__link', href: routePath(ctx.code, 'orders') }, 'Siparişlerime git')
                : null));
    }

    // ---------- Wiring ----------
    const unsubscribe = ctx.store.subscribe('cart', next => {
        if (!ctx.store.get('me') || placedNo !== null) return;
        const changed = cartSignature(next || []) !== cartSignature(lines);
        lines = next || [];
        if (changed) {
            // A notice about the content that was sent no longer applies.
            render(topBand);
            showNotice(null);
            retry = false;
        }
        scheduleQuote();
        draw();
    });

    const onOnline = () => {
        online = true;
        scheduleQuote(0);
        drawSummary();
    };
    const onOffline = () => {
        online = false;
        drawSummary();
    };
    window.addEventListener('online', onOnline);
    window.addEventListener('offline', onOffline);

    drawNoteCount();
    scheduleQuote(0);
    draw();

    return {
        name: 'cart',
        title: 'Sepet',
        node,
        destroy() {
            destroyed = true;
            clearTimeout(timer);
            if (controller) controller.abort();
            unsubscribe();
            window.removeEventListener('online', onOnline);
            window.removeEventListener('offline', onOffline);
        },
    };
}
