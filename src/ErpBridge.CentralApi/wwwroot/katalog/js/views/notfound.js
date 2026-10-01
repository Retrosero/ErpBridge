// Full-page messages without the app frame: unknown address, unknown catalogue, closed account,
// no company code, unsupported browser.

import { h } from '../dom.js';
import { icon } from '../icons.js';

/** messagePage({ title, text, icon, action: { label, href } | Node }). */
export function messagePage(opts) {
    let action = opts.action || null;
    if (action && !(action instanceof Node)) {
        action = h('a', { class: 'app-btn app-btn--secondary btn-touch', href: action.href }, action.label);
    }
    return h('div', { class: 'page-message' },
        h('div', { class: 'page-message__card' },
            h('span', { class: 'state__icon', 'aria-hidden': 'true' }, icon(opts.icon || 'alert', { size: 40 })),
            h('h1', { class: 't-heading' }, opts.title),
            opts.text ? h('p', { class: 't-body muted' }, opts.text) : null,
            action));
}
