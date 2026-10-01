// Inline 24×24 stroke icons, drawn with currentColor. Decorative unless a label is given.

import { svg } from './dom.js';

const PATHS = {
    search: ['M18 11a7 7 0 1 1-14 0a7 7 0 1 1 14 0z', 'M21 21l-5-5'],
    cart: ['M3 4h2l2.4 11.2a1 1 0 0 0 1 .8h8.8a1 1 0 0 0 1-.8L20 8H6.2', 'M10.5 20a1.5 1.5 0 1 1-3 0a1.5 1.5 0 1 1 3 0z', 'M18.5 20a1.5 1.5 0 1 1-3 0a1.5 1.5 0 1 1 3 0z'],
    orders: ['M9 3h6v4H9z', 'M9 5H6a1 1 0 0 0-1 1v14a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1h-3', 'M9 12h6', 'M9 16h6'],
    user: ['M16 8a4 4 0 1 1-8 0a4 4 0 1 1 8 0z', 'M4 21a8 8 0 0 1 16 0'],
    grid: ['M4 4h7v7H4z', 'M13 4h7v7h-7z', 'M4 13h7v7H4z', 'M13 13h7v7h-7z'],
    menu: ['M4 6h16', 'M4 12h16', 'M4 18h16'],
    plus: ['M12 5v14', 'M5 12h14'],
    minus: ['M5 12h14'],
    close: ['M6 6l12 12', 'M18 6L6 18'],
    back: ['M15 5l-7 7l7 7'],
    next: ['M9 5l7 7l-7 7'],
    chevronDown: ['M6 9l6 6l6-6'],
    box: ['M3 7l9-4l9 4v10l-9 4l-9-4z', 'M3 7l9 4l9-4', 'M12 11v10'],
    alert: ['M12 3l10 18H2z', 'M12 10v4', 'M12 17.5v.5'],
    check: ['M5 12l5 5l9-10'],
    eye: ['M2 12s3.5-7 10-7s10 7 10 7s-3.5 7-10 7s-10-7-10-7z', 'M15 12a3 3 0 1 1-6 0a3 3 0 1 1 6 0z'],
    eyeOff: ['M2 12s3.5-7 10-7s10 7 10 7s-3.5 7-10 7s-10-7-10-7z', 'M15 12a3 3 0 1 1-6 0a3 3 0 1 1 6 0z', 'M3 3l18 18'],
    offline: ['M2 8.5a15 15 0 0 1 20 0', 'M5 12a10 10 0 0 1 14 0', 'M8.5 15.5a5 5 0 0 1 7 0', 'M12 19h.01', 'M3 3l18 18'],
    logout: ['M9 21H5a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h4', 'M16 17l5-5l-5-5', 'M21 12H9'],
    trash: ['M4 7h16', 'M10 11v6', 'M14 11v6', 'M6 7l1 13a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1l1-13', 'M9 7V4h6v3'],
    file: ['M14 3H7a1 1 0 0 0-1 1v16a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V7z', 'M14 3v4h4', 'M9 13h6', 'M9 17h6'],
    ledger: ['M4 19h16', 'M7 15v-4', 'M12 15V7', 'M17 15v-6'],
    history: ['M3 12a9 9 0 1 0 3-6.7', 'M3 4v5h5', 'M12 8v4l3 2'],
    key: ['M12 15a4 4 0 1 1-8 0a4 4 0 1 1 8 0z', 'M11 12l9-9', 'M17 6l3 3', 'M15 8l2 2'],
};

/** icon('cart') / icon('close', { label: 'Kapat', size: 20 }). */
export function icon(name, opts = {}) {
    const size = opts.size || 24;
    const paths = PATHS[name] || PATHS.box;
    return svg('svg', {
        class: ['icon', opts.class].filter(Boolean).join(' '),
        width: size,
        height: size,
        viewBox: '0 0 24 24',
        fill: 'none',
        stroke: 'currentColor',
        'stroke-width': 2,
        'stroke-linecap': 'round',
        'stroke-linejoin': 'round',
        focusable: 'false',
        'aria-hidden': opts.label ? null : 'true',
        role: opts.label ? 'img' : null,
        'aria-label': opts.label || null,
    }, paths.map(d => svg('path', { d })));
}
