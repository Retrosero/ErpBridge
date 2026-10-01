// App frame: top bar (company badge + a slot the view fills, e.g. the search box), bottom navigation
// under 840px / top navigation from 840px, offline band and the snackbar (role=status).

import { h, render } from '../dom.js';
import { icon } from '../icons.js';
import { initials } from '../format.js';
import { routePath } from '../route-parse.js';

const NAV = [
    { name: 'catalog', label: 'Katalog', icon: 'grid', match: ['catalog', 'product'] },
    { name: 'cart', label: 'Sepet', icon: 'cart', match: ['cart'], order: true },
    { name: 'orders', label: 'Siparişlerim', icon: 'orders', match: ['orders', 'order'], order: true },
    { name: 'account', label: 'Hesabım', icon: 'user', match: ['account', 'statement', 'invoices', 'invoice', 'purchased', 'password'] },
];

const SNACKBAR_MS = 3000;
const SNACKBAR_ACTION_MS = 6000;

export function createShell(code) {
    const brandBadge = h('span', { class: 'brand__badge', 'aria-hidden': 'true' });
    const brandName = h('span', { class: 'brand__name' });
    const brand = h('a', { class: 'brand', href: routePath(code, 'catalog') }, brandBadge, brandName);
    const slot = h('div', { class: 'topbar__slot' });
    const topNav = h('nav', { class: 'topnav', 'aria-label': 'Ana menü' });
    const offline = h('div', { class: 'offline', role: 'status', hidden: true },
        icon('offline', { size: 18 }),
        h('span', null, 'İnternet bağlantısı yok. Sepetiniz bu cihazda saklı.'));
    const header = h('header', { class: 'topbar' }, h('div', { class: 'topbar__row' }, brand, slot, topNav), offline);
    const main = h('main', { id: 'main', class: 'main', tabindex: '-1' });
    const bottomNav = h('nav', { class: 'bottomnav', 'aria-label': 'Ana menü' });
    const snackHost = h('div', { class: 'snackbar-host', role: 'status', 'aria-live': 'polite' });
    // The cart badge is aria-hidden; this tells screen readers when the number of products changes.
    const cartLive = h('p', { class: 'sr-only', role: 'status' });
    // Handled here: a #fragment navigation would fire popstate and re-route the page.
    const skip = h('a', {
        class: 'skip-link',
        href: '#main',
        onclick: e => {
            e.preventDefault();
            main.focus();
        },
    }, 'İçeriğe geç');
    const root = h('div', { class: 'shell' }, skip, header, main, bottomNav, snackHost, cartLive);

    let features = { order: false };
    let active = 'catalog';
    let cartCount = 0;
    let cartKnown = false;
    let snackTimer = 0;

    function closeSnackbar() {
        clearTimeout(snackTimer);
        render(snackHost);
    }

    function navLinks(variant) {
        return NAV.filter(item => !item.order || features.order).map(item => {
            const isActive = item.match.indexOf(active) >= 0;
            const badge = item.name === 'cart' && cartCount > 0
                ? h('span', { class: 'navbadge', 'aria-hidden': 'true' }, cartCount > 99 ? '99+' : String(cartCount))
                : null;
            const label = item.name === 'cart' && cartCount > 0 ? 'Sepet, ' + cartCount + ' ürün' : null;
            return h('a', {
                class: variant === 'bottom' ? ['app-bottombar__item', 'navitem', isActive && 'app-bottombar__item--active'] : 'topnav__item',
                href: routePath(code, item.name),
                'aria-current': isActive ? 'page' : null,
                'aria-label': label,
            }, h('span', { class: 'navitem__icon' }, icon(item.icon, { size: 22 }), badge), h('span', null, item.label));
        });
    }

    function drawNav() {
        render(topNav, navLinks('top'));
        render(bottomNav, navLinks('bottom'));
    }

    function setOnline(online) {
        offline.hidden = online;
        document.documentElement.classList.toggle('is-offline', !online);
    }
    window.addEventListener('online', () => setOnline(true));
    window.addEventListener('offline', () => setOnline(false));
    setOnline(navigator.onLine !== false);

    return {
        root,
        main,
        /** Company name + features from /me. */
        setAccount(me) {
            brandBadge.textContent = initials(me.companyName);
            brandName.textContent = me.companyName || '';
            brand.setAttribute('aria-label', (me.companyName || 'Katalog') + ', kataloğa dön');
            features = me.features || { order: false };
            drawNav();
        },
        setActive(routeName) {
            active = routeName;
            drawNav();
        },
        setCartCount(n) {
            if (n === cartCount && cartKnown) return;
            // The first value is what the page opened with, not a change worth announcing.
            if (cartKnown && features.order) cartLive.textContent = n > 0 ? 'Sepette ' + n + ' ürün var.' : 'Sepet boş.';
            cartKnown = true;
            cartCount = n;
            drawNav();
        },
        /** Fills the top bar's middle (null = empty); `wide` hides the company name on phones. */
        setHeader(node, wide) {
            render(slot, node);
            header.classList.toggle('topbar--wide-slot', !!wide);
        },
        /**
         * snackbar('Sepete eklendi', { label: 'Sepete git', href }) or { label: 'Geri al', onClick } —
         * one at a time; 3 s, or 6 s when it offers an action so there is time to reach it.
         */
        snackbar(text, action) {
            clearTimeout(snackTimer);
            let actionNode = null;
            if (action && action.href) {
                actionNode = h('a', { class: 'snackbar__action', href: action.href }, action.label);
            } else if (action) {
                actionNode = h('button', {
                    type: 'button',
                    class: 'snackbar__action',
                    onclick: () => {
                        closeSnackbar();
                        action.onClick();
                    },
                }, action.label);
            }
            render(snackHost, h('div', { class: 'snackbar' }, h('span', { class: 'snackbar__text' }, text), actionNode));
            snackTimer = setTimeout(() => render(snackHost), action ? SNACKBAR_ACTION_MS : SNACKBAR_MS);
        },
    };
}
