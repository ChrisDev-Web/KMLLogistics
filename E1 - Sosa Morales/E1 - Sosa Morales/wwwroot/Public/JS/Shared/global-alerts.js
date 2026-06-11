(function () {
    'use strict';

    var config = {
        pollUrl: '',
        intervalMs: 20000,
        displayMs: 5000,
        fadeMs: 400
    };

    var queue = [];
    var showing = false;
    var container = null;
    var closeAllBtn = null;
    var currentEl = null;
    var currentItem = null;
    var currentExpiresAt = 0;
    var displayTimer = null;
    var fadeTimer = null;
    var storageKey = 'kmlShownAlerts';
    var activeStorageKey = 'kmlActiveAlerts';

    function getShownKeys() {
        try {
            return JSON.parse(sessionStorage.getItem(storageKey) || '[]');
        } catch (e) {
            return [];
        }
    }

    function markShown(key) {
        var keys = getShownKeys();
        if (keys.indexOf(key) !== -1) return;
        keys.push(key);
        if (keys.length > 200) {
            keys = keys.slice(-200);
        }
        sessionStorage.setItem(storageKey, JSON.stringify(keys));
    }

    function isValidAlertItem(item) {
        return item && item.key && item.message;
    }

    function readActiveState() {
        try {
            return JSON.parse(sessionStorage.getItem(activeStorageKey) || 'null');
        } catch (e) {
            return null;
        }
    }

    function clearActiveState() {
        try {
            sessionStorage.removeItem(activeStorageKey);
        } catch (e) { }
    }

    function persistActiveState() {
        try {
            var now = Date.now();
            var state = {
                current: currentItem && currentExpiresAt > now
                    ? { item: currentItem, expiresAt: currentExpiresAt }
                    : null,
                queue: queue.filter(isValidAlertItem)
            };

            if (!state.current && state.queue.length === 0) {
                clearActiveState();
                return;
            }

            sessionStorage.setItem(activeStorageKey, JSON.stringify(state));
        } catch (e) { }
    }

    function hasPendingKey(key) {
        if (!key) return false;
        if (currentItem && currentItem.key === key) return true;
        return queue.some(function (item) {
            return item.key === key;
        });
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function ensureContainer() {
        if (container) return container;
        container = document.createElement('div');
        container.id = 'globalAlertsContainer';
        container.className = 'global-alerts';
        container.setAttribute('aria-live', 'polite');
        container.setAttribute('aria-atomic', 'true');
        closeAllBtn = document.createElement('button');
        closeAllBtn.type = 'button';
        closeAllBtn.className = 'global-alerts__close-all';
        closeAllBtn.innerHTML = '<i class="bi bi-x-lg"></i><span>Cerrar todas</span>';
        closeAllBtn.addEventListener('click', dismissAll);
        container.appendChild(closeAllBtn);
        document.body.appendChild(container);
        updateCloseAllVisibility();
        return container;
    }

    function updateCloseAllVisibility() {
        if (!closeAllBtn) return;
        closeAllBtn.hidden = !currentEl && queue.length === 0;
    }

    function clearTimers() {
        if (displayTimer) {
            window.clearTimeout(displayTimer);
            displayTimer = null;
        }
        if (fadeTimer) {
            window.clearTimeout(fadeTimer);
            fadeTimer = null;
        }
    }

    function dismissCurrent(done) {
        clearTimers();
        currentItem = null;
        currentExpiresAt = 0;
        persistActiveState();

        if (!currentEl) {
            showing = false;
            updateCloseAllVisibility();
            if (done) done();
            return;
        }

        var el = currentEl;
        currentEl = null;
        el.classList.add('is-leaving');

        fadeTimer = window.setTimeout(function () {
            el.remove();
            showing = false;
            fadeTimer = null;
            updateCloseAllVisibility();
            if (done) done();
        }, config.fadeMs);
    }

    function dismissAll() {
        if (currentItem && currentItem.key) {
            markShown(currentItem.key);
        }
        queue.forEach(function (item) {
            if (item.key) markShown(item.key);
        });
        queue = [];
        currentItem = null;
        currentExpiresAt = 0;
        clearActiveState();
        dismissCurrent();
    }

    function showImmediately(item) {
        if (!isValidAlertItem(item)) return;

        queue = queue.filter(function (queuedItem) {
            return queuedItem.key !== item.key;
        });
        queue.unshift(item);
        ensureContainer();
        updateCloseAllVisibility();
        persistActiveState();

        if (showing) {
            dismissCurrent(showNext);
            return;
        }

        showNext();
    }

    function displayItem(item, durationMs, expiresAt) {
        if (!isValidAlertItem(item)) {
            showNext();
            return;
        }

        showing = true;
        currentItem = item;
        currentExpiresAt = expiresAt || (Date.now() + durationMs);
        markShown(item.key);
        persistActiveState();

        var el = document.createElement('div');
        el.className = 'global-alert global-alert--' + (item.level || 'warning');
        el.innerHTML =
            '<i class="bi bi-bell-fill global-alert__icon"></i>' +
            '<span class="global-alert__text">' + escapeHtml(item.message) + '</span>';

        ensureContainer().appendChild(el);
        currentEl = el;
        updateCloseAllVisibility();

        window.requestAnimationFrame(function () {
            el.classList.add('is-visible');
        });

        displayTimer = window.setTimeout(function () {
            dismissCurrent(showNext);
        }, Math.max(0, currentExpiresAt - Date.now()));
    }

    function showNext() {
        if (showing || queue.length === 0) return;

        var item = queue.shift();
        displayItem(item, config.displayMs);
    }

    function enqueue(items) {
        if (!items || !items.length) return;

        items.forEach(function (item) {
            if (isValidAlertItem(item) && !hasPendingKey(item.key)) {
                queue.push(item);
            }
        });

        if (queue.length === 0) return;

        ensureContainer();
        updateCloseAllVisibility();
        persistActiveState();

        if (!showing) {
            showNext();
        }
    }

    function poll() {
        if (!config.pollUrl) return;

        fetch(config.pollUrl, {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (response) {
                if (!response.ok) throw new Error('poll failed');
                return response.json();
            })
            .then(function (data) {
                var shown = getShownKeys();
                var fresh = (data.items || []).filter(function (item) {
                    return item.key &&
                        shown.indexOf(item.key) === -1 &&
                        !hasPendingKey(item.key);
                });
                enqueue(fresh);
            })
            .catch(function () { });
    }

    function restoreActiveState() {
        var state = readActiveState();
        if (!state) return false;

        queue = Array.isArray(state.queue)
            ? state.queue.filter(isValidAlertItem)
            : [];

        if (state.current &&
            isValidAlertItem(state.current.item) &&
            Number(state.current.expiresAt) > Date.now()) {
            ensureContainer();
            displayItem(
                state.current.item,
                Number(state.current.expiresAt) - Date.now(),
                Number(state.current.expiresAt));
            return true;
        }

        if (queue.length > 0) {
            persistActiveState();
            ensureContainer();
            showNext();
            return true;
        }

        clearActiveState();
        return false;
    }

    function initGlobalAlerts() {
        var body = document.body;
        if (!body || body.getAttribute('data-auth') !== 'true') return;

        config.pollUrl = body.getAttribute('data-alerts-url') || '/AlertasStock/Notifications';

        restoreActiveState();
        poll();
        window.setInterval(poll, config.intervalMs);
    }

    window.kmlGlobalAlerts = window.kmlGlobalAlerts || {};
    window.kmlGlobalAlerts.showImmediately = showImmediately;

    document.addEventListener('DOMContentLoaded', initGlobalAlerts);
})();
