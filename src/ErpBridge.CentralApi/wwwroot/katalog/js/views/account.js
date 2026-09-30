// Hesabım (/{KOD}/hesap): who is signed in, which price list applies, and sign-out. Balance,
// statement, invoices, purchased products and password change arrive with W4.

import { h } from '../dom.js';
import { icon } from '../icons.js';
import { percent, vatLabel } from '../format.js';
import { errorMessage } from '../messages.js';

export function accountView(ctx) {
    const me = ctx.me;
    const customer = me.customer || {};
    const priceList = me.priceList || {};

    const logout = h('button', { type: 'button', class: 'app-btn app-btn--ghost btn-block', onclick: onLogout },
        icon('logout', { size: 20 }), 'Çıkış yap');

    async function onLogout() {
        logout.disabled = true;
        try {
            // anonymous: a 401 means the session is already gone, which is what we want anyway.
            await ctx.api.post('logout', undefined, { anonymous: true });
        } catch (err) {
            if (err.status !== 401) {
                logout.disabled = false;
                ctx.notify(errorMessage(err));
                return;
            }
        }
        ctx.signedOut();
    }

    const pricing = 'Fiyat listeniz: ' + (priceList.name || 'Liste ' + (priceList.no || '')) + ' (' + vatLabel(priceList.includesVat) + ')'
        + (me.discountPercent > 0 ? ' · İskonto ' + percent(me.discountPercent) : '');

    const node = h('div', { class: 'account' },
        h('section', { class: 'app-card account__card', 'aria-label': 'Cari bilgileri' },
            h('h2', { class: 't-title' }, customer.name || ''),
            h('p', { class: 't-body-sm muted' }, [customer.code, me.username].filter(Boolean).join(' · ')),
            h('p', { class: 't-caption muted' }, pricing)),
        logout);

    return { name: 'account', title: 'Hesabım', node };
}
