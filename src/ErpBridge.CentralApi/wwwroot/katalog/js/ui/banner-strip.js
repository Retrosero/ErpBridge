// The banner strip under the catalogue's intro (GOAL_MUSTERI_KATALOGU S12/W6): one 16:6 slide per banner in a
// CSS scroll-snap track, dots underneath. Nothing moves by itself; the customer swipes, clicks a dot or uses the
// arrow keys on the focused strip. Pictures load lazily (only the first one eagerly) and the text sits on a scrim.

import { h, uid } from '../dom.js';
import { bannerTarget, slideIndex, usableBanners } from '../banners.js';

/** The strip, or null when there is nothing to show. onCategory(id) switches the list to that category. */
export function bannerStrip(code, items, onCategory) {
    const banners = usableBanners(items);
    if (!banners.length) return null;
    const n = banners.length;
    const smooth = !window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const slides = banners.map((banner, i) => slide(code, banner, i, n, onCategory));
    const track = h('div', {
        class: 'banners__track',
        id: uid('banners'),
        tabindex: n > 1 ? '0' : null,
        'aria-label': n > 1 ? 'Kampanyalar; ok tuşlarıyla geçin' : null,
    }, slides);
    const node = h('section', { class: 'banners', 'aria-roledescription': 'carousel', 'aria-label': 'Kampanyalar' }, track);
    if (n === 1) return node;

    let current = 0;
    const dots = banners.map((banner, i) => h('button', {
        type: 'button',
        class: 'banners__dot',
        'aria-controls': track.id,
        'aria-label': (i + 1) + '. kampanya' + (banner.title ? ': ' + banner.title : ''),
        'aria-current': i === 0 ? 'true' : null,
        onclick: () => go(i),
    }));
    function go(i) {
        const to = Math.min(n - 1, Math.max(0, i));
        track.scrollTo({ left: slides[to].offsetLeft, behavior: smooth ? 'smooth' : 'auto' });
    }
    function mark(i) {
        if (i === current) return;
        dots[current].removeAttribute('aria-current');
        dots[i].setAttribute('aria-current', 'true');
        current = i;
    }
    let frame = 0;
    track.addEventListener('scroll', () => {
        if (frame) return;
        frame = requestAnimationFrame(() => {
            frame = 0;
            mark(slideIndex(track.scrollLeft, track.clientWidth, n));
        });
    }, { passive: true });
    track.addEventListener('keydown', e => {
        if (e.target !== track) return;
        const to = e.key === 'ArrowRight' ? current + 1 : e.key === 'ArrowLeft' ? current - 1
            : e.key === 'Home' ? 0 : e.key === 'End' ? n - 1 : null;
        if (to === null) return;
        e.preventDefault();
        go(to);
    });
    node.append(h('div', { class: 'banners__dots', role: 'group', 'aria-label': 'Kampanya seçimi' }, dots));
    return node;
}

function slide(code, banner, i, n, onCategory) {
    const target = bannerTarget(code, banner.link);
    const title = String(banner.title || '').trim();
    const text = String(banner.text || '').trim();
    const picture = banner.image && (banner.image.full || banner.image.thumb);
    const body = [
        picture ? image(banner.image, title ? '' : 'Kampanya ' + (i + 1), i === 0) : null,
        title || text ? h('div', { class: 'banner__text' },
            title ? h('p', { class: 'banner__title' }, title) : null,
            text ? h('p', { class: 'banner__body' }, text) : null,
            target && target.kind === 'url' ? h('span', { class: 'sr-only' }, ' (yeni sekmede açılır)') : null) : null,
    ];
    const attrs = {
        class: ['banner', picture ? 'banner--image' : 'banner--plain'],
        role: 'group',
        'aria-roledescription': 'kampanya',
        'aria-label': (i + 1) + ' / ' + n,
    };
    if (!target) return h('div', attrs, body);

    const link = { class: 'banner__link', href: target.href };
    if (target.kind === 'url') {
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
    } else if (target.kind === 'product') {
        // The router opens the product dialog over the list and closing it comes back here.
        link.dataset = { overlay: '1' };
    } else if (target.kind === 'category') {
        // Stays a real link (open in a new tab works); a plain click switches the list in place.
        link.onclick = e => {
            if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
            e.preventDefault();
            onCategory(target.id);
        };
    }
    if (!title) link['aria-label'] = 'Kampanya ' + (i + 1);
    return h('div', attrs, h('a', link, body));
}

function image(picture, alt, eager) {
    const full = picture.full || picture.thumb;
    const thumb = picture.thumb && picture.thumb !== full ? picture.thumb : null;
    // loading/decoding/referrerpolicy/srcset go before src: an <img> starts fetching as soon as src is set.
    const img = h('img', {
        class: 'banner__img',
        loading: eager ? 'eager' : 'lazy',
        decoding: 'async',
        referrerpolicy: 'no-referrer',
        width: 1920,
        height: 720,
        alt,
        srcset: thumb ? thumb + ' 800w, ' + full + ' 1920w' : null,
        sizes: thumb ? '(min-width: 840px) 75vw, 100vw' : null,
        src: full,
    });
    // A picture that cannot load leaves the coloured banner with its text.
    img.addEventListener('error', () => {
        const box = img.closest('.banner');
        img.remove();
        if (box) box.classList.replace('banner--image', 'banner--plain');
    }, { once: true });
    return img;
}
