// Keyboard shortcuts for the approval desk (plan step 5). One document-level listener hands the
// desk's keys to .NET; typing in a field is left alone, except Enter in a one-line field and Escape.
window.portalKeys = (() => {
    const handled = new Set(['ArrowUp', 'ArrowDown', 'j', 'k', 'a', 'A', 'r', 'R', 'n', 'N', ' ', '/', 'f', 'F', '?', 'Escape', 'Enter']);
    let listener = null;
    let owner = 0;

    const isField = el => !!el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.tagName === 'SELECT' || el.isContentEditable);

    return {
        /** Replaces any earlier listener; returns a handle only this attachment can detach with. */
        attach(dotnet) {
            if (listener) document.removeEventListener('keydown', listener);
            listener = e => {
                if (e.defaultPrevented || e.isComposing || e.altKey || e.metaKey) return;
                // Older browsers name the space bar "Spacebar"; the physical key is always "Space".
                const key = e.key === 'Spacebar' || e.code === 'Space' ? ' ' : e.key;
                if (!handled.has(key) || (e.ctrlKey && key !== 'Enter')) return;
                const field = isField(e.target);
                if (field) {
                    const oneLine = e.target.tagName === 'INPUT' && e.target.type !== 'checkbox';
                    if (!(key === 'Escape' || (key === 'Enter' && (oneLine || e.ctrlKey)))) return;
                } else if (e.target.tagName === 'BUTTON' && (key === 'Enter' || key === ' ')) {
                    return; // a focused button keeps its own Enter and Space
                }
                e.preventDefault();
                dotnet.invokeMethodAsync('OnKey', key, e.shiftKey, e.ctrlKey, field);
            };
            document.addEventListener('keydown', listener);
            return ++owner;
        },
        /** A page instance that was replaced must not remove its successor's listener. */
        detach(handle) {
            if (handle !== owner || !listener) return;
            document.removeEventListener('keydown', listener);
            listener = null;
        },
        focus(selector) {
            const el = document.querySelector(selector);
            if (el) { el.focus(); if (typeof el.select === 'function') el.select(); }
        },
        blur() {
            const el = document.activeElement;
            if (el && typeof el.blur === 'function') el.blur();
        },
        reveal(selector) {
            const el = document.querySelector(selector);
            if (el) el.scrollIntoView({ block: 'nearest' });
        },
    };
})();
