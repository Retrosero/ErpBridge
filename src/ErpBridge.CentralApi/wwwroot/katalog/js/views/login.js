// Sign-in (/{KOD}/giris?r=). The company name comes from the anonymous /info call; a valid session
// cookie skips the form. The server sets the HttpOnly session cookie; nothing is stored here.

import { h, uid } from '../dom.js';
import { icon } from '../icons.js';
import { initials } from '../format.js';
import { errorMessage } from '../messages.js';

/**
 * ctx: { api, expired, onSignedIn(me), onNotFound() }. Returns { name, node, mounted, destroy }.
 */
export function loginView(ctx) {
    let busy = false;
    let destroyed = false;

    const badge = h('span', { class: 'login__badge', 'aria-hidden': 'true' }, '');
    const company = h('h1', { class: 't-heading login__company' }, 'Müşteri Kataloğu');
    const band = h('div', { class: 'band band--danger', role: 'alert', hidden: true });
    const expired = ctx.expired
        ? h('div', { class: 'band band--warning', role: 'status' }, icon('alert', { size: 20 }), h('span', null, 'Oturumunuz sona erdi, tekrar giriş yapın.'))
        : null;

    const userId = uid('u');
    const passId = uid('p');
    const username = h('input', {
        id: userId,
        class: 'app-input',
        name: 'username',
        type: 'text',
        autocomplete: 'username',
        autocapitalize: 'none',
        autocorrect: 'off',
        spellcheck: 'false',
        maxlength: '64',
        required: true,
    });
    const password = h('input', {
        id: passId,
        class: 'app-input field__password',
        name: 'password',
        type: 'password',
        autocomplete: 'current-password',
        maxlength: '72',
        required: true,
    });
    const reveal = h('button', {
        type: 'button',
        class: 'icon-btn field__reveal',
        'aria-label': 'Şifreyi göster',
        'aria-pressed': 'false',
        'aria-controls': passId,
        onclick: () => {
            const show = password.type === 'password';
            password.type = show ? 'text' : 'password';
            reveal.setAttribute('aria-pressed', show ? 'true' : 'false');
            reveal.setAttribute('aria-label', show ? 'Şifreyi gizle' : 'Şifreyi göster');
            reveal.replaceChildren(icon(show ? 'eyeOff' : 'eye'));
        },
    }, icon('eye'));
    const remember = h('input', { type: 'checkbox', name: 'remember', class: 'check__box' });
    const submit = h('button', { type: 'submit', class: 'app-btn app-btn--primary btn-block' }, 'Giriş yap');

    const form = h('form', { class: 'login__form', novalidate: true, onsubmit: onSubmit },
        h('div', { class: 'field' }, h('label', { class: 't-label', for: userId }, 'Kullanıcı adı'), username),
        h('div', { class: 'field' }, h('label', { class: 't-label', for: passId }, 'Şifre'),
            h('div', { class: 'field__row' }, password, reveal)),
        h('label', { class: 'check' }, remember, h('span', { class: 't-body' }, 'Beni hatırla')),
        submit);

    const node = h('div', { class: 'login' },
        h('div', { class: 'login__card' },
            h('div', { class: 'login__head' }, badge, company, h('p', { class: 't-body-sm muted' }, 'Müşteri kataloğuna giriş')),
            expired, band, form,
            h('p', { class: 't-caption muted login__foot' }, 'Şifrenizi unuttuysanız firmanızla iletişime geçin.')));

    function showError(text) {
        band.replaceChildren(icon('alert', { size: 20 }), h('span', null, text));
        band.hidden = false;
    }

    function setBusy(on) {
        busy = on;
        submit.disabled = on;
        submit.setAttribute('aria-busy', on ? 'true' : 'false');
        submit.textContent = on ? 'Giriş yapılıyor…' : 'Giriş yap';
    }

    async function onSubmit(e) {
        e.preventDefault();
        if (busy) return;
        const name = username.value.trim();
        if (!name || !password.value) {
            showError('Kullanıcı adı ve şifrenizi yazın.');
            (name ? password : username).focus();
            return;
        }
        band.hidden = true;
        setBusy(true);
        try {
            const res = await ctx.api.post('login', { username: name, password: password.value, remember: remember.checked }, { anonymous: true });
            if (destroyed) return;
            ctx.onSignedIn(res.me);
        } catch (err) {
            if (destroyed) return;
            setBusy(false);
            if (err.code === 'CATALOG_NOT_FOUND') {
                ctx.onNotFound();
                return;
            }
            showError(errorMessage(err, 'login'));
            if (err.code === 'INVALID_CREDENTIALS') {
                password.value = '';
                password.focus();
            }
        }
    }

    ctx.api.get('info', { anonymous: true })
        .then(info => {
            if (destroyed || !info) return;
            company.textContent = info.companyName || 'Müşteri Kataloğu';
            badge.textContent = initials(info.companyName);
            document.title = (info.companyName ? info.companyName + ' · ' : '') + 'Müşteri Kataloğu';
        })
        .catch(err => {
            if (!destroyed && err.code === 'CATALOG_NOT_FOUND') ctx.onNotFound();
        });

    // Still signed in (the cookie outlived what this tab knew): go straight back.
    ctx.api.get('me', { anonymous: true })
        .then(me => {
            if (!destroyed && me && !busy) ctx.onSignedIn(me);
        })
        .catch(() => {
            // Deliberately quiet: a 401 here only means "not signed in", which is why the form is showing.
        });

    return {
        name: 'login',
        node,
        mounted() {
            username.focus();
        },
        destroy() {
            destroyed = true;
        },
    };
}
