// Catalogue: sticky search (200 ms debounce), category chips + "all categories" sheet under 840px,
// a category panel from 840px, and a paged grid that loads on scroll with a "Daha fazla göster"
// button for keyboards. Leaving and coming back within 5 minutes restores the list and position.

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import { count, foldText } from '../format.js';
import { routePath } from '../route-parse.js';
import {
    emptyState, errorState, openSheet, productCard, sheetHeader, skeletonCards,
} from '../ui/components.js';

const PAGE_SIZE = 48;
const MIN_QUERY = 2;
const DEBOUNCE_MS = 200;
const CACHE_MS = 5 * 60 * 1000;
const PANEL_FILTER_FROM = 12;

let categoryCache = null;
let listCache = null;

/** Sign-in and sign-out drop everything another account may have loaded. */
export function resetCatalogCache() {
    categoryCache = null;
    listCache = null;
}

function effectiveQuery(value) {
    const q = String(value || '').trim();
    return q.length >= MIN_QUERY ? q : '';
}

export function catalogView(ctx, opts = {}) {
    const owner = ctx.me.username;
    const cached = listCache && listCache.owner === owner && Date.now() - listCache.at < CACHE_MS ? listCache : null;
    // Under a deep-linked product dialog the list keeps what the customer last looked at.
    let q = opts.underlay && cached ? cached.q : effectiveQuery(ctx.route.query.q);
    let category = opts.underlay && cached ? cached.category : ctx.route.query.kategori || '';
    let categories = categoryCache && categoryCache.owner === owner ? categoryCache.items : null;

    let items = [];
    let total = 0;
    let page = 0;
    let done = false;
    let loading = false;
    let error = null;
    let controller = null;
    let destroyed = false;
    let searchTimer = 0;
    let restoreScroll = null;
    let sheet = null;
    const cards = new Map();

    // Search box (lives in the top bar).
    const searchId = uid('q');
    const input = h('input', {
        id: searchId,
        class: 'app-input app-search searchbar__input',
        type: 'search',
        inputmode: 'search',
        enterkeyhint: 'search',
        autocomplete: 'off',
        autocapitalize: 'none',
        spellcheck: 'false',
        placeholder: 'Ürün adı, kod, barkod',
        'aria-label': 'Ürün ara',
        value: q,
    });
    const clearButton = h('button', {
        type: 'button',
        class: 'icon-btn searchbar__clear',
        'aria-label': 'Aramayı temizle',
        hidden: !q,
        onclick: () => {
            input.value = '';
            clearButton.hidden = true;
            applySearch('');
            input.focus();
        },
    }, icon('close', { size: 20 }));
    input.addEventListener('input', () => {
        clearButton.hidden = !input.value;
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => applySearch(input.value), DEBOUNCE_MS);
    });
    input.addEventListener('keydown', e => {
        if (e.key !== 'Enter') return;
        clearTimeout(searchTimer);
        applySearch(input.value);
        input.blur();
    });
    const header = h('div', { class: 'searchbar', role: 'search' },
        h('span', { class: 'searchbar__icon', 'aria-hidden': 'true' }, icon('search', { size: 20 })),
        input, clearButton);

    const chips = h('div', { class: 'chips', role: 'group', 'aria-label': 'Kategoriler' });
    const panel = h('nav', { class: 'catpanel', 'aria-label': 'Kategoriler' });
    const countLine = h('p', { class: 't-caption muted result-count', 'aria-live': 'polite' });
    const status = h('div', { class: 'catalog__status' });
    const grid = h('div', { class: 'grid' });
    const sentinel = h('div', { class: 'sentinel', 'aria-hidden': 'true' });
    const more = h('div', { class: 'more' });
    const node = h('div', { class: 'catalog' },
        h('h1', { class: 'sr-only' }, 'Ürün kataloğu'),
        panel,
        h('section', { class: 'catalog__main', 'aria-label': 'Ürünler' }, chips, countLine, status, grid, sentinel, more));

    const observer = 'IntersectionObserver' in window
        ? new IntersectionObserver(entries => {
            if (entries.some(e => e.isIntersecting) && canLoadMore()) load(false);
        }, { rootMargin: '0px 0px 800px 0px' })
        : null;

    const unsubscribe = ctx.store.subscribe('cart', lines => {
        const quantities = new Map(lines.map(l => [l.key, l.quantity]));
        cards.forEach((card, key) => card.update(quantities.get(key) || 0));
    });

    function canLoadMore() {
        return !destroyed && !loading && !done && !error && items.length > 0;
    }

    function currentUrl() {
        return routePath(ctx.code, 'catalog', { q: q || null, kategori: category || null });
    }

    function applySearch(value) {
        const next = effectiveQuery(value);
        if (next === q) return;
        q = next;
        ctx.update(currentUrl());
        load(true);
    }

    function selectCategory(id) {
        if (sheet) closeSheet();
        if (id === category) return;
        category = id;
        ctx.update(currentUrl());
        drawCategories();
        window.scrollTo(0, 0);
        load(true);
    }

    function appendCards(list) {
        const eagerLeft = Math.max(0, 4 - items.length);
        const nodes = [];
        list.forEach((product, i) => {
            if (cards.has(product.key)) return;
            const card = productCard(ctx, product, i < eagerLeft);
            cards.set(product.key, card);
            items.push(product);
            nodes.push(card.node);
        });
        if (nodes.length) grid.append(...nodes);
    }

    async function load(reset) {
        if (reset) {
            if (controller) controller.abort();
            items = [];
            total = 0;
            page = 0;
            done = false;
            error = null;
            cards.clear();
            render(grid, skeletonCards(6));
            grid.setAttribute('aria-busy', 'true');
        } else if (loading) {
            return;
        }
        const mine = new AbortController();
        controller = mine;
        loading = true;
        error = null;
        draw();
        try {
            const res = await ctx.api.get('products', {
                query: { category: category || null, q: q || null, page: page + 1, pageSize: PAGE_SIZE },
                signal: mine.signal,
            });
            if (destroyed || controller !== mine) return;
            if (page === 0) render(grid);
            const list = Array.isArray(res.items) ? res.items : [];
            page = res.page || page + 1;
            total = Number(res.total) || 0;
            appendCards(list);
            done = list.length === 0 || items.length >= total;
        } catch (err) {
            if (destroyed || controller !== mine || mine.signal.aborted) return;
            if (page === 0) render(grid);
            error = err;
        } finally {
            if (!destroyed && controller === mine) {
                loading = false;
                grid.removeAttribute('aria-busy');
                draw();
                // Re-arm: a tall screen may still show the sentinel after a short page.
                if (observer && !done && !error) {
                    observer.unobserve(sentinel);
                    observer.observe(sentinel);
                }
            }
        }
    }

    function draw() {
        const settled = page > 0 && items.length > 0;
        countLine.textContent = settled ? (q ? '“' + q + '” için ' : '') + count(total) + ' ürün' : '';

        if (error && items.length === 0) {
            render(status, errorState(error, () => load(true), 'Ürünler yüklenemedi.'));
        } else if (!loading && items.length === 0) {
            render(status, emptyView());
        } else {
            render(status);
        }

        if (error && items.length > 0) {
            render(more, errorState(error, () => {
                error = null;
                load(false);
            }, 'Devamı yüklenemedi.'));
        } else if (!done && items.length > 0) {
            render(more, h('button', {
                type: 'button',
                class: 'app-btn app-btn--ghost btn-touch more__button',
                disabled: loading,
                onclick: () => load(false),
            }, loading ? 'Yükleniyor…' : 'Daha fazla göster (' + count(items.length) + '/' + count(total) + ')'));
        } else {
            render(more);
        }
    }

    function emptyView() {
        if (q) {
            return emptyState({
                title: '“' + q + '” için ürün bulunamadı',
                text: 'Farklı bir kelimeyle ya da ürün koduyla arayın.',
                action: h('button', {
                    type: 'button',
                    class: 'app-btn app-btn--secondary btn-touch',
                    onclick: () => {
                        input.value = '';
                        clearButton.hidden = true;
                        applySearch('');
                    },
                }, 'Aramayı temizle'),
            });
        }
        if (category) {
            return emptyState({
                icon: 'box',
                title: 'Bu kategoride şu an ürün yok.',
                action: h('button', { type: 'button', class: 'app-btn app-btn--secondary btn-touch', onclick: () => selectCategory('') }, 'Tüm ürünler'),
            });
        }
        return emptyState({ icon: 'box', title: 'Kataloğunuzda henüz ürün yok.', text: 'Firmanızla iletişime geçin.' });
    }

    // Categories: chips + sheet under 840px, side panel from 840px (CSS picks one).
    function categoryName(id) {
        const found = (categories || []).find(c => c.id === id);
        return found ? found.name : 'Kategori';
    }

    function categoryList(filter, onPick) {
        const folded = foldText(filter);
        const all = (categories || []).reduce((sum, c) => sum + (Number(c.count) || 0), 0);
        const rows = [{ id: '', name: 'Tüm ürünler', count: all }].concat(categories || [])
            .filter(c => !folded || c.id === '' || foldText(c.name).indexOf(folded) >= 0);
        return h('ul', { class: 'catlist', role: 'list' }, rows.map(c => h('li', null,
            h('button', {
                type: 'button',
                class: 'catlist__item',
                'aria-current': c.id === category ? 'true' : null,
                onclick: () => onPick(c.id),
            },
            h('span', { class: 'catlist__name' }, c.name),
            h('span', { class: 'catlist__count t-caption' }, count(c.count)),
            c.id === category ? icon('check', { size: 18, class: 'catlist__check' }) : null))));
    }

    function drawCategories() {
        if (!categories || categories.length === 0) {
            render(chips);
            render(panel);
            chips.hidden = true;
            return;
        }
        chips.hidden = false;
        const selectedChip = category ? categoryName(category) : 'Tümü';
        render(chips,
            h('button', { type: 'button', class: 'chip chip--menu', 'aria-haspopup': 'dialog', onclick: openCategorySheet },
                icon('menu', { size: 18 }), h('span', { class: 'sr-only' }, 'Kategoriler: '), selectedChip, icon('chevronDown', { size: 18 })),
            categories.map(c => h('button', {
                type: 'button',
                class: 'chip',
                'aria-pressed': c.id === category ? 'true' : 'false',
                onclick: () => selectCategory(c.id === category ? '' : c.id),
            }, c.name)));

        const filterInput = categories.length > PANEL_FILTER_FROM
            ? h('input', { class: 'app-input app-search catpanel__filter', type: 'search', placeholder: 'Kategori ara', 'aria-label': 'Kategori ara' })
            : null;
        const listHost = h('div', null, categoryList('', selectCategory));
        if (filterInput) filterInput.addEventListener('input', () => render(listHost, categoryList(filterInput.value, selectCategory)));
        render(panel, h('h2', { class: 't-label catpanel__title' }, 'Kategoriler'), filterInput, listHost);
    }

    function openCategorySheet() {
        const titleId = uid('cat');
        const filter = h('input', { class: 'app-input app-search', type: 'search', placeholder: 'Kategori ara', 'aria-label': 'Kategori ara' });
        const listHost = h('div', { class: 'sheet__content' }, categoryList('', selectCategory));
        filter.addEventListener('input', () => render(listHost, categoryList(filter.value, selectCategory)));
        sheet = openSheet({ className: 'sheet--categories', labelledBy: titleId, onRequestClose: closeSheet });
        render(sheet.dialog, sheetHeader('Kategoriler', titleId, closeSheet), h('div', { class: 'sheet__filter' }, filter), listHost);
    }

    function closeSheet() {
        if (!sheet) return;
        sheet.close();
        sheet = null;
    }

    async function loadCategories() {
        if (categories) {
            drawCategories();
            return;
        }
        try {
            const res = await ctx.api.get('categories');
            if (destroyed) return;
            categories = Array.isArray(res.items) ? res.items : [];
            categoryCache = { owner, items: categories };
        } catch (err) {
            // Products still load without categories; the chips just stay hidden until the next visit.
            if (destroyed) return;
            categories = null;
        }
        drawCategories();
    }

    // Start: from the cache when the same list was open a moment ago, otherwise from page 1.
    drawCategories();
    loadCategories();
    if (cached && cached.q === q && cached.category === category) {
        appendCards(cached.items);
        total = cached.total;
        page = cached.page;
        done = cached.done;
        restoreScroll = cached.scrollY;
        draw();
    } else {
        load(true);
    }

    return {
        name: 'catalog',
        title: 'Katalog',
        node,
        header,
        wideHeader: true,
        mounted() {
            if (observer) observer.observe(sentinel);
            if (restoreScroll !== null) {
                const y = restoreScroll;
                restoreScroll = null;
                requestAnimationFrame(() => window.scrollTo(0, y));
            }
        },
        /** True when this view already shows the list `route` asks for (back from a product). */
        matches(route) {
            return route.name === 'catalog'
                && effectiveQuery(route.query.q) === q
                && (route.query.kategori || '') === category;
        },
        url: currentUrl,
        destroy() {
            if (items.length) {
                listCache = { owner, q, category, items: items.slice(), total, page, done, scrollY: window.scrollY, at: Date.now() };
            }
            destroyed = true;
            clearTimeout(searchTimer);
            if (controller) controller.abort();
            if (observer) observer.disconnect();
            closeSheet();
            unsubscribe();
        },
    };
}
