// The only way the catalogue builds DOM. Text always goes in as text nodes, so data from the server
// can never become markup; the page's CSP blocks inline style attributes, so styling is by class.

const PROPERTIES = new Set(['value', 'checked', 'disabled', 'hidden', 'selected', 'indeterminate']);
const SVG_NS = 'http://www.w3.org/2000/svg';

let lastId = 0;

/** A document-unique id for aria-labelledby / label[for] pairs. */
export function uid(prefix) {
    lastId += 1;
    return (prefix || 'k') + '-' + lastId;
}

/**
 * h('button', { class: ['a', on && 'b'], type: 'button', onclick: fn, 'aria-label': '…' }, 'metin', child)
 * Keys: class (string or array), dataset (object), on<event> (function), a few live properties
 * (value, checked, disabled, hidden …); everything else becomes an attribute. null/undefined/false skip.
 * Attribute order is kept, so an <img> gets loading="lazy" before its src when written that way.
 */
export function h(tag, props, ...children) {
    const el = document.createElement(tag);
    applyProps(el, props);
    appendChildren(el, children);
    return el;
}

/** Same as h() for inline SVG (icons). */
export function svg(tag, attrs, ...children) {
    const el = document.createElementNS(SVG_NS, tag);
    if (attrs) {
        for (const key of Object.keys(attrs)) {
            if (attrs[key] !== null && attrs[key] !== undefined) el.setAttribute(key, String(attrs[key]));
        }
    }
    appendChildren(el, children);
    return el;
}

/** Replaces the container's children. */
export function render(container, ...children) {
    container.replaceChildren(...toNodes(children));
}

function applyProps(el, props) {
    if (!props) return;
    for (const key of Object.keys(props)) {
        const value = props[key];
        if (value === null || value === undefined || value === false) continue;
        if (key === 'class') {
            el.className = Array.isArray(value) ? value.filter(Boolean).join(' ') : value;
        } else if (key === 'dataset') {
            Object.assign(el.dataset, value);
        } else if (key.startsWith('on') && typeof value === 'function') {
            el.addEventListener(key.slice(2).toLowerCase(), value);
        } else if (PROPERTIES.has(key)) {
            el[key] = value;
        } else {
            el.setAttribute(key, value === true ? '' : String(value));
        }
    }
}

function appendChildren(el, children) {
    const nodes = toNodes(children);
    if (nodes.length) el.append(...nodes);
}

function toNodes(children) {
    const out = [];
    const walk = list => {
        for (const child of list) {
            if (child === null || child === undefined || child === false || child === true) continue;
            if (Array.isArray(child)) walk(child);
            else if (typeof child === 'string' || typeof child === 'number') out.push(document.createTextNode(String(child)));
            else out.push(child);
        }
    };
    walk(children);
    return out;
}
