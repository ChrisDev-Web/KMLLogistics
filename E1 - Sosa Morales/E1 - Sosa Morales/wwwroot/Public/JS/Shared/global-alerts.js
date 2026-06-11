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
    var currentEl = null;
    var displayTimer = null;
    var fadeTimer = null;
    var storageKey = 'kmlShownAlerts';

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
        document.body.appendChild(container);
        return container;
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

        if (!currentEl) {
            showing = false;
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
            if (done) done();
        }, config.fadeMs);
    }

    function showNext() {
        if (showing || queue.length === 0) return;

        showing = true;
        var item = queue.shift();
        markShown(item.key);

        var el = document.createElement('div');
        el.className = 'global-alert global-alert--' + (item.level || 'warning');
        el.innerHTML =
            '<i class="bi bi-bell-fill global-alert__icon"></i>' +
            '<span class="global-alert__text">' + escapeHtml(item.message) + '</span>';

        ensureContainer().appendChild(el);
        currentEl = el;

        window.requestAnimationFrame(function () {
            el.classList.add('is-visible');
        });

        displayTimer = window.setTimeout(function () {
            dismissCurrent(showNext);
        }, config.displayMs);
    }

    function enqueue(items) {
        if (!items || !items.length) return;

        items.forEach(function (item) {
            queue.push(item);
        });

        if (showing && queue.length > 0) {
            dismissCurrent(showNext);
            return;
        }

        showNext();
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
                    return item.key && shown.indexOf(item.key) === -1;
                });
                enqueue(fresh);
            })
            .catch(function () { });
    }

    function initGlobalAlerts() {
        var body = document.body;
        if (!body || body.getAttribute('data-auth') !== 'true') return;

        config.pollUrl = body.getAttribute('data-alerts-url') || '/AlertasStock/Notifications';

        poll();
        window.setInterval(poll, config.intervalMs);
    }

    document.addEventListener('DOMContentLoaded', initGlobalAlerts);
})();
