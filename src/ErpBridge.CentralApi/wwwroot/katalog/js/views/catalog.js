// Catalogue: server-side filters and sorting in a collapsible sidebar.
// The full-width paged grid keeps keyboard load-more,
// URL filters and a five-minute cache when returning from a product or another page.

import { h, render, uid } from '../dom.js';
import { icon } from '../icons.js';
import { count, foldText } from '../format.js';
import { routePath } from '../route-parse.js';
import {
    emptyState, errorState, productCard, skeletonCards,
} from '../ui/components.js';
import { bannerStrip } from '../ui/banner-strip.js';

const PAGE_SIZE = 48;
const MIN_QUERY = 2;
const DEBOUNCE_MS = 200;
const CACHE_MS = 5 * 60 * 1000;
const PANEL_FILTER_FROM = 12;

const SORTS = [
    ['recommended', 'Önerilen sıralama'], ['name-asc', 'Ürün adı: A–Z'], ['name-desc', 'Ürün adı: Z–A'],
    ['price-asc', 'Fiyat: düşükten yükseğe'], ['price-desc', 'Fiyat: yüksekten düşüğe'], ['code-asc', 'Ürün kodu'],
];

export function catalogFilters(query = {}) {
    const price = value => value !== '' && value != null && Number.isFinite(Number(value)) && Number(value) >= 0 ? String(Number(value)) : '';
    return {
        brand: String(query.brand || '').trim(), stock: ['in', 'out'].includes(query.stock) ? query.stock : '',
        minPrice: price(query.minPrice), maxPrice: price(query.maxPrice),
        discounted: String(query.discounted) === 'true' ? 'true' : '',
        cartonOnly: String(query.cartonOnly) === 'true' ? 'true' : '',
        hasImage: String(query.hasImage) === 'true' ? 'true' : '',
        sort: SORTS.some(s => s[0] === query.sort) ? query.sort : 'recommended',
    };
}

let categoryCache = null;
let bannerCache = null;
let listCache = null;

/** Sign-in and sign-out drop everything another account may have loaded. */
export function resetCatalogCache() {
    categoryCache = null;
    bannerCache = null;
    listCache = null;
}

function effectiveQuery(value) {
    const q = String(value || '').trim();
    return q.length >= MIN_QUERY ? q : '';
}

export function catalogView(ctx, opts = {}) {
    const owner = ctx.code + ':' + ctx.me.username;
    const cached = listCache && listCache.owner === owner && Date.now() - listCache.at < CACHE_MS ? listCache : null;
    // Under a deep-linked product dialog the list keeps what the customer last looked at.
    let q = opts.underlay && cached ? cached.q : effectiveQuery(ctx.route.query.q);
    let category = opts.underlay && cached ? cached.category : ctx.route.query.kategori || '';
    let filters = catalogFilters(opts.underlay && cached ? cached.filters : ctx.route.query);
    let brands = cached ? cached.brands || [] : [];
    let categories = categoryCache && categoryCache.owner === owner ? categoryCache.items : null;
    const banners = bannerCache && bannerCache.owner === owner && Date.now() - bannerCache.at < CACHE_MS ? bannerCache.items : null;

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

    // Banners (S12) under the intro; the strip is drawn only when there is at least one.
    const bannerHost = h('div', { class: 'catalog-banners', hidden: true });
    const panel = h('nav', { class: 'catpanel', 'aria-label': 'Kategoriler' });
    const categoryPanel = h('details', { class: 'catalog-section', open: true },
        h('summary', null, h('span', null, 'Kategoriler'), icon('chevronDown', { size: 18 })), panel);
    const countLine = h('p', { class: 't-caption muted result-count', 'aria-live': 'polite' });
    const status = h('div', { class: 'catalog__status' });
    const grid = h('div', { class: 'grid' });
    const sentinel = h('div', { class: 'sentinel', 'aria-hidden': 'true' });
    const more = h('div', { class: 'more' });
    const fields = {};
    function field(label, control) {
        return h('label', { class: 'catalog-field' }, h('span', { class: 't-label' }, label), control);
    }
    function select(name, values) {
        const el = h('select', { class: 'app-input', name }, values.map(([value, label]) => h('option', { value }, label)));
        el.value = filters[name];
        fields[name] = el;
        return el;
    }
    const brandSelect = select('brand', [['', 'Tüm markalar']]);
    const min = h('input', { class: 'app-input', type: 'number', inputmode: 'decimal', min: '0', step: '0.01', name: 'minPrice', value: filters.minPrice, placeholder: 'En az' });
    const max = h('input', { class: 'app-input', type: 'number', inputmode: 'decimal', min: '0', step: '0.01', name: 'maxPrice', value: filters.maxPrice, placeholder: 'En çok' });
    fields.minPrice = min;
    fields.maxPrice = max;
    function check(name, label) {
        const el = h('input', { type: 'checkbox', name, checked: filters[name] === 'true' });
        fields[name] = el;
        return h('label', { class: 'catalog-check' }, el, label);
    }
    const filterForm = h('form', { class: 'catalog-filters__body' },
        field('Marka', brandSelect),
        field('Stok durumu', select('stock', [['', 'Tüm ürünler'], ['in', 'Stokta olanlar'], ['out', 'Stokta olmayanlar']])),
        h('div', { class: 'catalog-price-range' }, field('En az (₺)', min), field('En çok (₺)', max)),
        h('div', { class: 'catalog-checks' }, check('discounted', 'İndirimli ürünler'), check('cartonOnly', 'Yalnız koli satılanlar'), check('hasImage', 'Görselli ürünler')),
        h('button', { type: 'submit', class: 'app-btn app-btn--primary btn-touch btn-block' }, 'Filtreleri uygula'),
        h('button', { type: 'button', class: 'app-btn app-btn--ghost btn-touch btn-block', onclick: resetFilters }, 'Tümünü temizle'));
    function validatePrices() {
        max.setCustomValidity(min.value !== '' && max.value !== '' && Number(min.value) > Number(max.value) ? 'En çok fiyat, en az fiyattan küçük olamaz.' : '');
    }
    min.addEventListener('input', validatePrices);
    max.addEventListener('input', validatePrices);
    filterForm.addEventListener('submit', e => {
        e.preventDefault();
        validatePrices();
        if (!filterForm.reportValidity()) return;
        for (const [key, el] of Object.entries(fields)) filters[key] = el.type === 'checkbox' ? (el.checked ? 'true' : '') : el.value;
        closeMobileMenu();
        applyFilters();
    });
    const filterPanel = h('details', { class: 'catalog-filters catalog-section', open: window.matchMedia('(min-width: 840px)').matches },
        h('summary', null, h('span', null, 'Filtreler'), icon('chevronDown', { size: 18 })), filterForm);
    const sortSelect = select('sort', SORTS);
    // Sorting applies immediately; draft filter fields are only committed with the form.
    delete fields.sort;
    sortSelect.addEventListener('change', () => { filters.sort = sortSelect.value; applyFilters(); });
    const activeFilters = h('div', { class: 'catalog-active', 'aria-label': 'Etkin filtreler' });
    const sidebar = h('details', { class: 'catalog__sidebar app-card', open: window.matchMedia('(min-width: 840px)').matches },
        h('summary', { class: 'catalog__menu-toggle', 'aria-label': 'Katalog menüsü' },
            icon('menu', { size: 20 }), h('span', null, 'Katalog menüsü'), icon('chevronDown', { size: 18 })),
        h('div', { class: 'catalog__menu-body' }, categoryPanel, filterPanel));
    const node = h('div', { class: 'catalog' },
        h('header', { class: 'catalog-intro' },
            h('div', null, h('p', { class: 'catalog-intro__eyebrow' }, 'SİZE ÖZEL KATALOG'), h('h1', null, 'İhtiyacınız olan ürünler, bir arada.'),
                h('p', { class: 'muted' }, 'Ürünleri keşfedin, size özel fiyatlarla siparişinizi hazırlayın.')),
            h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'orders') }, icon('orders', { size: 20 }), 'Siparişlerim')),
        bannerHost,
        sidebar,
        h('section', { class: 'catalog__main', 'aria-label': 'Ürünler' },
            h('div', { class: 'catalog-toolbar' }, countLine, field('Sıralama', sortSelect)), activeFilters, status, grid, sentinel, more));

    function closeMobileMenu() {
        if (window.matchMedia('(min-width: 840px)').matches) return;
        sidebar.open = false;
        sidebar.firstElementChild.focus();
    }

    function syncFields() {
        for (const [key, el] of Object.entries(fields)) {
            if (el.type === 'checkbox') el.checked = filters[key] === 'true';
            else el.value = filters[key];
        }
        sortSelect.value = filters.sort;
        max.setCustomValidity('');
    }
    function drawBrands(draft = brandSelect.value) {
        const values = brands.includes(draft) || !draft ? brands : [draft, ...brands];
        render(brandSelect, h('option', { value: '' }, 'Tüm markalar'), values.map(b => h('option', { value: b }, b)));
        brandSelect.value = draft;
    }
    function applyFilters() {
        ctx.update(currentUrl());
        drawActiveFilters();
        load(true);
    }
    function resetFilters() {
        filters = catalogFilters();
        category = '';
        q = '';
        input.value = '';
        clearButton.hidden = true;
        clearTimeout(searchTimer);
        syncFields();
        drawCategories();
        applyFilters();
    }
    function drawActiveFilters() {
        const labels = { brand: filters.brand, stock: filters.stock === 'in' ? 'Stokta olanlar' : 'Stokta olmayanlar',
            minPrice: 'En az ' + filters.minPrice + ' ₺', maxPrice: 'En çok ' + filters.maxPrice + ' ₺',
            discounted: 'İndirimli', cartonOnly: 'Yalnız koli', hasImage: 'Görselli' };
        const selected = Object.keys(labels).filter(key => filters[key]);
        render(activeFilters, selected.map(key => h('button', { type: 'button', class: 'chip',
            'aria-label': labels[key] + ' filtresini kaldır', onclick: () => { filters[key] = ''; syncFields(); applyFilters(); } },
            labels[key], icon('close', { size: 16 }))));
    }
    drawBrands(filters.brand);
    drawActiveFilters();

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
        return routePath(ctx.code, 'catalog', { q: q || null, kategori: category || null, ...filters, sort: filters.sort === 'recommended' ? null : filters.sort });
    }

    function applySearch(value) {
        const next = effectiveQuery(value);
        if (next === q) return;
        q = next;
        ctx.update(currentUrl());
        load(true);
    }

    function selectCategory(id) {
        closeMobileMenu();
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
                query: { category: category || null, q: q || null, page: page + 1, pageSize: PAGE_SIZE, ...filters },
                signal: mine.signal,
            });
            if (destroyed || controller !== mine) return;
            if (page === 0) render(grid);
            const list = Array.isArray(res.items) ? res.items : [];
            page = res.page || page + 1;
            total = Number(res.total) || 0;
            brands = Array.isArray(res.brands) ? res.brands : [];
            drawBrands();
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
        countLine.textContent = loading && page === 0 ? 'Ürünler yükleniyor…' : (q ? '“' + q + '” için ' : '') + count(total) + ' ürün';

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
        if (Object.entries(filters).some(([key, value]) => key !== 'sort' && value)) {
            return emptyState({ icon: 'search', title: 'Bu filtrelere uygun ürün bulunamadı.',
                text: 'Fiyat aralığını genişletin veya bazı filtreleri kaldırın.',
                action: h('button', { type: 'button', class: 'app-btn app-btn--secondary btn-touch', onclick: resetFilters }, 'Filtreleri temizle') });
        }
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

    // The same category navigation is used at every viewport size.
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
        categoryPanel.hidden = !categories || categories.length === 0;
        if (categoryPanel.hidden) {
            render(panel);
            return;
        }
        const filterInput = categories.length > PANEL_FILTER_FROM
            ? h('input', { class: 'app-input app-search catpanel__filter', type: 'search', placeholder: 'Kategori ara', 'aria-label': 'Kategori ara' })
            : null;
        const listHost = h('div', null, categoryList('', selectCategory));
        if (filterInput) filterInput.addEventListener('input', () => render(listHost, categoryList(filterInput.value, selectCategory)));
        render(panel, filterInput, listHost);
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
            // Products still load without categories; the section stays hidden until the next visit.
            if (destroyed) return;
            categories = null;
        }
        drawCategories();
    }

    function drawBanners(items) {
        const strip = bannerStrip(ctx.code, items, selectCategory);
        bannerHost.hidden = !strip;
        render(bannerHost, strip);
    }

    async function loadBanners() {
        if (banners) {
            drawBanners(banners);
            return;
        }
        try {
            const res = await ctx.api.get('banners');
            if (destroyed) return;
            const items = Array.isArray(res.items) ? res.items : [];
            bannerCache = { owner, items, at: Date.now() };
            drawBanners(items);
        } catch (err) {
            // Banners are extra: the catalogue works without them, so a failure leaves the strip out.
        }
    }

    // Start: from the cache when the same list was open a moment ago, otherwise from page 1.
    loadBanners();
    drawCategories();
    loadCategories();
    if (cached && cached.q === q && cached.category === category && JSON.stringify(cached.filters) === JSON.stringify(filters)) {
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
                && (route.query.kategori || '') === category
                && JSON.stringify(catalogFilters(route.query)) === JSON.stringify(filters);
        },
        url: currentUrl,
        destroy() {
            if (items.length) {
                listCache = { owner, q, category, filters: { ...filters }, brands, items: items.slice(), total, page, done, scrollY: window.scrollY, at: Date.now() };
            }
            destroyed = true;
            clearTimeout(searchTimer);
            if (controller) controller.abort();
            if (observer) observer.disconnect();
            unsubscribe();
        },
    };
}
