// Entry point: reads the company code from the address, restores the session (GET me; the HttpOnly
// cookie decides), mounts the frame and routes. Only the catalogue loads up front; other screens
// are imported on first use.

import { createApi } from './api.js';
import { createRouter } from './router.js';
import { bindCart, cartKey, createStore, safeStorage } from './store.js';
import { loginPath, parseLocation, routePath, safeReturnPath } from './route-parse.js';
import { h, render } from './dom.js';
import { lineQty } from './cart.js';
import { errorMessage, isAccessBlocked } from './messages.js';
import { createShell } from './ui/shell.js';
import { catalogView, resetCatalogCache } from './views/catalog.js';
import { messagePage } from './views/notfound.js';

const TITLE = 'Müşteri Kataloğu';

// Routes that need an account feature (GOAL_MUSTERI_KATALOGU §5.2 Me.features).
const FEATURE = {
    cart: 'order',
    orders: 'order',
    order: 'order',
    statement: 'statement',
    invoices: 'invoices',
    invoice: 'invoices',
    purchased: 'purchased',
};

const pending = () => import('./views/pending.js').then(m => m.pendingView);
const LAZY = {
    account: () => import('./views/account.js').then(m => m.accountView),
    cart: pending,
    orders: pending,
    order: pending,
    statement: pending,
    invoices: pending,
    invoice: pending,
    purchased: pending,
    password: pending,
};

const RELOAD_KEY = 'katalog:v1:reloaded-at';

/**
 * Loads a lazy module. After a deploy the old /assets/{v}/ path answers 404, so a failed import
 * reloads the page once to pick up the new version (a second failure within a minute is reported).
 */
async function lazy(load) {
    try {
        return await load();
    } catch (err) {
        let last = 0;
        try {
            last = Number(sessionStorage.getItem(RELOAD_KEY)) || 0;
            sessionStorage.setItem(RELOAD_KEY, String(Date.now()));
        } catch (e) {
            last = Date.now(); // no storage: never risk a reload loop
        }
        if (Date.now() - last > 60000) {
            location.reload();
            return new Promise(() => {});
        }
        throw err;
    }
}

const app = document.getElementById('app');
const store = createStore({ me: null, cart: [] });

let code = null;
let api = null;
let router = null;
let shell = null;
let view = null;
let overlay = null;
let unbindCart = null;
let seq = 0;
let routed = false;

function renderBare(node) {
    destroyView();
    shell = null;
    render(app, node);
}

function destroyView() {
    closeOverlay();
    if (view && view.destroy) view.destroy();
    view = null;
}

function closeOverlay() {
    if (!overlay) return;
    overlay.close();
    overlay = null;
}

function setMe(me) {
    if (unbindCart) {
        unbindCart();
        unbindCart = null;
    }
    store.set('me', me);
    if (!me) {
        store.set('cart', []);
        return;
    }
    unbindCart = bindCart(store, safeStorage(window), cartKey(code, me.username), window);
    document.title = (me.companyName ? me.companyName + ' · ' : '') + TITLE;
    if (shell) shell.setAccount(me);
}

function ensureShell() {
    if (shell) return;
    shell = createShell(code);
    shell.setAccount(store.get('me'));
    shell.setCartCount((store.get('cart') || []).length);
    render(app, shell.root);
}

store.subscribe('cart', lines => {
    if (shell) shell.setCartCount(lines.length);
});

function context(route) {
    return {
        code,
        api,
        store,
        route,
        me: store.get('me'),
        navigate: router.navigate,
        update: router.update,
        notify: (text, action) => {
            if (shell) shell.snackbar(text, action);
        },
        cartQty: key => lineQty(store.get('cart') || [], key),
        signedOut,
    };
}

function signedOut() {
    setMe(null);
    resetCatalogCache();
    router.navigate(routePath(code, 'login'), { replace: true });
}

/** 401 from any authenticated call: back to the login page, then to where the customer was. */
function sessionLost() {
    const hadSession = !!store.get('me');
    setMe(null);
    resetCatalogCache();
    if (router.current().name === 'login') return;
    router.navigate(loginPath(code, location.pathname + location.search), {
        replace: true,
        state: hadSession ? { expired: true } : null,
    });
}

function mountView(next, route) {
    destroyView();
    ensureShell();
    shell.setActive(route.name);
    shell.setHeader(next.header || h('h1', { class: 't-title topbar__title' }, next.title || TITLE), !!next.wideHeader);
    render(shell.main, next.node);
    view = next;
    if (next.mounted) next.mounted();
}

function showMessage(opts) {
    renderBare(messagePage(opts));
}

/** Something the customer can act on when a screen could not even start (offline, server error). */
function failPage(err) {
    showMessage({
        title: 'Katalog açılamadı',
        text: errorMessage(err),
        action: h('button', { type: 'button', class: 'app-btn app-btn--secondary btn-touch', onclick: () => dispatch(router.current(), { pop: false, state: history.state }) }, 'Tekrar dene'),
    });
}

function catalogNotFound() {
    document.title = TITLE;
    showMessage({ title: 'Katalog bulunamadı', text: 'Bağlantıyı kontrol edin ya da firmanızdan yeni bağlantı isteyin.' });
}

async function showLogin(route, my) {
    const back = safeReturnPath(code, route.query.r);
    if (store.get('me')) {
        router.navigate(back || routePath(code, 'catalog'), { replace: true });
        return;
    }
    const { loginView } = await lazy(() => import('./views/login.js'));
    if (my !== seq) return;
    const next = loginView({
        api,
        expired: !!(history.state && history.state.expired),
        onSignedIn(me) {
            resetCatalogCache();
            setMe(me);
            router.navigate(back || routePath(code, 'catalog'), { replace: true });
        },
        onNotFound: catalogNotFound,
    });
    renderBare(next.node);
    view = next;
    next.mounted();
}

async function openOverlay(route, info, my) {
    const key = route.query.kod || '';
    if (overlay && overlay.key === key) return;
    closeOverlay();
    // Opened from a card (pushState with { overlay: true }): closing goes back to that list.
    const pushed = !!(info.state && info.state.overlay);
    const { openProduct } = await lazy(() => import('./views/product.js'));
    if (my !== seq) return;
    overlay = openProduct(context(route), key, () => {
        if (pushed) history.back();
        else router.navigate(view && view.url ? view.url() : routePath(code, 'catalog'), { replace: true });
    });
}

async function onRoute(route, info) {
    const my = ++seq;
    if (route.name === 'login') {
        await showLogin(route, my);
        return;
    }
    if (route.name === 'notFound') {
        showMessage({ title: 'Sayfa bulunamadı', text: 'Adres hatalı olabilir.', action: { label: 'Kataloğa dön', href: routePath(code, 'catalog') } });
        return;
    }

    if (!store.get('me')) {
        let me;
        try {
            me = await api.get('me');
        } catch (err) {
            if (my !== seq || err.status === 401) return; // sessionLost has already moved to the login page
            if (isAccessBlocked(err)) {
                showMessage({ title: 'Katalog açılamıyor', text: errorMessage(err), action: { label: 'Giriş ekranına dön', href: routePath(code, 'login') } });
            } else if (err.code === 'CATALOG_NOT_FOUND') {
                catalogNotFound();
            } else {
                failPage(err);
            }
            return;
        }
        if (my !== seq) return;
        setMe(me);
    }

    if (route.name === 'product') {
        if (!(view && view.name === 'catalog' && shell)) mountView(catalogView(context(route), { underlay: true }), route);
        else shell.setActive('product');
        await openOverlay(route, info, my);
        return;
    }
    if (route.name === 'catalog' && view && view.name === 'catalog' && shell && view.matches(route)) {
        // Back from the product dialog: the grid and its scroll position stay as they were.
        // A "Katalog" tap on the list already shown goes back to its top.
        if (overlay) closeOverlay();
        else if (!info.pop) window.scrollTo(0, 0);
        return;
    }

    const me = store.get('me');
    const feature = FEATURE[route.name];
    if (feature && !(me.features && me.features[feature])) {
        mountView({
            name: 'disabled',
            header: h('span'), // the message below carries the page heading
            node: messagePage({
                title: 'Bu bölüm açık değil',
                text: errorMessage({ code: 'FEATURE_DISABLED' }),
                action: { label: 'Kataloğa dön', href: routePath(code, 'catalog') },
            }),
        }, route);
        return;
    }

    const factory = route.name === 'catalog' ? catalogView : await lazy(LAZY[route.name]);
    if (my !== seq) return;
    mountView(factory(context(route)), route);
    if (!info.pop) window.scrollTo(0, 0);
    // Screen readers start reading the new screen instead of staying on the link that was followed.
    if (routed && !info.pop) shell.main.focus({ preventScroll: true });
    routed = true;
}

function dispatch(next, info) {
    onRoute(next, info).catch(err => {
        if (shell && view) shell.snackbar(errorMessage(err));
        else failPage(err);
    });
}

function start() {
    if (typeof HTMLDialogElement !== 'function' || typeof HTMLDialogElement.prototype.showModal !== 'function') {
        render(app, messagePage({ title: 'Tarayıcınız desteklenmiyor', text: 'Kataloğu açmak için tarayıcınızı güncelleyin ya da güncel Chrome veya Safari kullanın.' }));
        return;
    }
    const first = parseLocation(location.pathname, location.search);
    if (first.canonical) history.replaceState(history.state, '', first.canonical);
    if (!first.code) {
        render(app, messagePage({
            title: first.name === 'root' ? TITLE : 'Katalog bulunamadı',
            text: 'Firmanızın gönderdiği bağlantıyla girin.',
        }));
        return;
    }
    code = first.code;
    api = createApi(code, { onUnauthorized: sessionLost });
    router = createRouter(dispatch);
    router.start();
}

start();
