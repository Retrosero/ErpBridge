// Warehouse TV board (plan step 7): keep the screen awake and recover when the Blazor connection is lost for good.
window.portalKiosk = (() => {
    let lock = null;
    let watcher = null;

    async function keepAwake() {
        try {
            if ('wakeLock' in navigator && document.visibilityState === 'visible') {
                lock = await navigator.wakeLock.request('screen');
            }
        } catch {
            // Not allowed (no user gesture, old browser): the TV's own settings decide.
        }
    }

    function onVisible() {
        if (document.visibilityState === 'visible') keepAwake();
    }

    return {
        start() {
            keepAwake();
            document.addEventListener('visibilitychange', onVisible);
            if (watcher) return;
            // Blazor marks its reconnect dialog "failed" or "rejected" when the circuit cannot come back
            // (server restarted, long network loss). A wall screen nobody touches reloads itself instead.
            watcher = setInterval(() => {
                const modal = document.getElementById('components-reconnect-modal');
                if (modal && (modal.classList.contains('components-reconnect-failed') || modal.classList.contains('components-reconnect-rejected'))) {
                    location.reload();
                }
            }, 5000);
        },
        stop() {
            document.removeEventListener('visibilitychange', onVisible);
            if (watcher) clearInterval(watcher);
            watcher = null;
            if (lock) lock.release().catch(() => {});
            lock = null;
        },
    };
})();
