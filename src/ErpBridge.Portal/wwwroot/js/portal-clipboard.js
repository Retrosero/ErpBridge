// Customer catalog (GOAL_MUSTERI_KATALOGU P2): copies the catalog address. A refused clipboard (no permission, no
// focus) answers false and the page says so instead of pretending.
window.portalClipboard = {
    async copy(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            return false;
        }
    },
};
