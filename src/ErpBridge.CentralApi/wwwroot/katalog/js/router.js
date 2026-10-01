// History API router. Real <a href> links stay links (long press, "open in new tab"); only a plain
// left click on a same-origin catalogue link is taken over. The server returns the same shell for
// every /{KOD}/… path, so reloading any address works.

import { parseLocation } from './route-parse.js';

export function createRouter(onChange) {
    let started = false;

    function current() {
        return parseLocation(location.pathname, location.search);
    }

    /** push (default) or replace; `state` is kept in history.state (e.g. { overlay: true }). */
    function navigate(url, opts = {}) {
        const state = opts.state || null;
        if (opts.replace) history.replaceState(state, '', url);
        else history.pushState(state, '', url);
        onChange(current(), { pop: false, state });
    }

    /** Rewrites the address without routing (search box, category chip). */
    function update(url) {
        history.replaceState(history.state, '', url);
    }

    function onClick(e) {
        if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        const link = e.target instanceof Element ? e.target.closest('a[href]') : null;
        if (!link || link.hasAttribute('download') || (link.target && link.target !== '_self')) return;
        const href = link.getAttribute('href');
        if (!href || href.charAt(0) === '#') return;
        const url = new URL(link.href, location.href);
        if (url.origin !== location.origin) return;
        const route = parseLocation(url.pathname, url.search);
        if (!route.code) return;
        e.preventDefault();
        const target = url.pathname + url.search;
        navigate(target, {
            // Following a link to the page already shown must not stack a duplicate history entry.
            replace: target === location.pathname + location.search,
            state: link.dataset.overlay ? { overlay: true } : null,
        });
    }

    function onPop() {
        onChange(current(), { pop: true, state: history.state });
    }

    function start() {
        if (started) return;
        started = true;
        if ('scrollRestoration' in history) history.scrollRestoration = 'manual';
        document.addEventListener('click', onClick);
        window.addEventListener('popstate', onPop);
        onChange(current(), { pop: false, state: history.state });
    }

    return { start, navigate, update, current };
}
