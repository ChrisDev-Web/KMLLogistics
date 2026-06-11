(function () {
    'use strict';

    var config = window.alertasStockConfig || {};
    var state = {
        page: 1,
        pageSize: 10,
        search: '',
        idProduct: '',
        idWarehouse: '',
        status: 'ACTIVE'
    };

    var searchTimer = null;
    var initialized = false;

    function qs(id) { return document.getElementById(id); }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text == null ? '' : String(text);
        return div.innerHTML;
    }

    function buildQuery(params) {
        var url = new URL(config.listUrl, window.location.origin);
        Object.keys(params).forEach(function (key) {
            var value = params[key];
            if (value !== undefined && value !== null && value !== '') {
                url.searchParams.set(key, value);
            }
        });
        return url.toString();
    }

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function showToast(message, type) {
        var toast = qs('alertsToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'alerts-toast is-visible alerts-toast--' + (type || 'info');
        window.setTimeout(function () {
            toast.classList.remove('is-visible');
        }, 4000);
    }

    function formatDateTime(value) {
        var date = new Date(value);
        if (Number.isNaN(date.getTime())) return { date: '—', time: '' };
        return {
            date: date.toLocaleDateString('es-PE'),
            time: date.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })
        };
    }

    function renderRow(alert) {
        var isStock = String(alert.kind || '').toUpperCase() === 'STOCK';
        var iconClass = alert.isActive
            ? (isStock ? 'alert-cell__icon--warning' : 'alert-cell__icon--logistics')
            : 'alert-cell__icon--resolved';
        var levelClass = alert.isActive
            ? 'level-badge--' + (alert.level || 'warning')
            : 'level-badge--resolved';
        var iconName = alert.isActive
            ? (isStock ? 'bi-exclamation-triangle-fill' : 'bi-truck')
            : 'bi-check-circle-fill';
        var dt = formatDateTime(alert.eventAt);
        var resendBtn = alert.isActive
            ? '<button type="button" class="alerts-action-btn btn-resend-alert" data-alert-kind="' + escapeHtml(alert.kind) + '" data-alert-id="' + alert.id + '" title="Reenviar alerta"><i class="bi bi-send"></i></button>'
            : '';
        var userHint = alert.lastSentByUsername
            ? '<span class="alerts-action-hint" title="' + escapeHtml(alert.lastSentByUsername) + '"><i class="bi bi-person"></i></span>'
            : '';

        return '<tr data-alert-kind="' + escapeHtml(alert.kind) + '" data-alert-id="' + alert.id + '" class="' + (alert.isActive ? 'row-active' : 'row-resolved') + '">' +
            '<td><div class="alert-cell">' +
            '<span class="alert-cell__icon ' + iconClass + '"><i class="bi ' + iconName + '"></i></span>' +
            '<div><span class="alert-cell__title">' + escapeHtml(alert.title) + '</span>' +
            '<span class="alert-cell__meta">' + escapeHtml(alert.subtitle) + '</span></div></div></td>' +
            '<td><span class="module-badge ' + (isStock ? 'module-badge--stock' : 'module-badge--logistics') + '">' + (isStock ? 'Stock' : 'Logística') + '</span></td>' +
            '<td><span class="level-badge ' + levelClass + '">' + escapeHtml(alert.levelLabel) + '</span></td>' +
            '<td class="cell-last-notified"><span class="date-cell__date">' + escapeHtml(dt.date) + '</span>' +
            '<span class="date-cell__time">' + escapeHtml(dt.time) + '</span></td>' +
            '<td><span class="state-text ' + (alert.isActive ? 'state-text--active' : 'state-text--resolved') + '">' + (alert.isActive ? 'Activa' : 'Resuelta') + '</span></td>' +
            '<td class="cell-notification-count">' + (isStock ? String(alert.notificationCount || 0) : '—') + '</td>' +
            '<td><div class="alerts-actions">' + resendBtn + userHint + '</div></td>' +
            '</tr>';
    }

    function renderRows(items) {
        var tbody = qs('alertsTableBody');
        if (!tbody) return;

        if (!items || items.length === 0) {
            tbody.innerHTML = '<tr class="alerts-table__empty-row"><td colspan="7">No se encontraron alertas con los filtros seleccionados.</td></tr>';
            return;
        }

        tbody.innerHTML = items.map(renderRow).join('');
    }

    function updateSummary(summary) {
        if (!summary) return;
        if (qs('kpiActive')) qs('kpiActive').textContent = String(summary.activeCount || 0);
        if (qs('kpiHighNotify')) qs('kpiHighNotify').textContent = String(summary.highNotifyCount || 0);
        if (qs('kpiResolved')) qs('kpiResolved').textContent = String(summary.resolvedCount || 0);
        if (qs('kpiTotal')) qs('kpiTotal').textContent = String(summary.totalCount || 0);
        if (qs('tabCountAll')) qs('tabCountAll').textContent = '(' + (summary.totalCount || 0) + ')';
        if (qs('tabCountActive')) qs('tabCountActive').textContent = '(' + (summary.activeCount || 0) + ')';
        if (qs('tabCountResolved')) qs('tabCountResolved').textContent = '(' + (summary.resolvedCount || 0) + ')';
    }

    function updatePagination(data) {
        var page = data.page || 1;
        var totalPages = data.totalPages || 1;
        var info = qs('alertsTableInfo');
        var pageInfo = qs('alertsPageInfo');
        var prevBtn = qs('alertsPrevBtn');
        var nextBtn = qs('alertsNextBtn');

        if (info) {
            if (!data.totalCount) {
                info.textContent = 'Mostrando 0 registros';
            } else {
                info.textContent = 'Mostrando ' + data.showingFrom + ' a ' + data.showingTo + ' de ' + data.totalCount + ' registros';
            }
        }

        if (pageInfo) pageInfo.textContent = 'Página ' + page + ' de ' + totalPages;
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages;
    }

    function setLoading(isLoading) {
        var scroll = qs('alertsTableScroll');
        if (scroll) scroll.classList.toggle('is-loading', isLoading);
    }

    function loadList() {
        if (!config.listUrl) return;

        setLoading(true);

        fetch(buildQuery({
            search: state.search,
            idProduct: state.idProduct,
            idWarehouse: state.idWarehouse,
            status: state.status,
            page: state.page,
            pageSize: state.pageSize
        }), {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (response) {
                if (!response.ok) throw new Error('Error al cargar');
                return response.json();
            })
            .then(function (data) {
                renderRows(data.items || []);
                updatePagination(data);
                updateSummary(data.summary);
                var scroll = qs('alertsTableScroll');
                if (scroll) scroll.scrollTop = 0;
            })
            .catch(function () {
                var tbody = qs('alertsTableBody');
                if (tbody) {
                    tbody.innerHTML = '<tr class="alerts-table__empty-row"><td colspan="7">Error al cargar las alertas.</td></tr>';
                }
                showToast('No se pudieron cargar las alertas.', 'error');
            })
            .finally(function () {
                setLoading(false);
            });
    }

    function readFiltersFromDom() {
        var searchInput = qs('search');
        var productSelect = qs('idProduct');
        var warehouseSelect = qs('idWarehouse');
        var root = qs('alertsPageRoot');

        state.search = searchInput ? searchInput.value.trim() : '';
        state.idProduct = productSelect ? productSelect.value : '';
        state.idWarehouse = warehouseSelect ? warehouseSelect.value : '';
        state.status = root ? (root.getAttribute('data-status') || 'ACTIVE') : 'ACTIVE';
    }

    function setActiveTab(status) {
        state.status = status;
        var root = qs('alertsPageRoot');
        if (root) root.setAttribute('data-status', status);

        document.querySelectorAll('#alertsTabs .alerts-tab').forEach(function (tab) {
            tab.classList.toggle('active', tab.getAttribute('data-status') === status);
        });
    }

    function clearFilters() {
        state.search = '';
        state.idProduct = '';
        state.idWarehouse = '';
        state.page = 1;

        if (qs('search')) qs('search').value = '';
        if (qs('idProduct')) qs('idProduct').value = '';
        if (qs('idWarehouse')) qs('idWarehouse').value = '';

        loadList();
    }

    function bindResendButtons() {
        var tbody = qs('alertsTableBody');
        if (!tbody || tbody.dataset.resendBound === 'true') return;
        tbody.dataset.resendBound = 'true';

        tbody.addEventListener('click', function (event) {
            var btn = event.target.closest('.btn-resend-alert');
            if (!btn || !config.resendUrl) return;

            var alertId = btn.getAttribute('data-alert-id');
            var alertKind = btn.getAttribute('data-alert-kind') || 'STOCK';
            var row = btn.closest('tr');
            if (!alertId) return;

            btn.disabled = true;

            var body = new URLSearchParams();
            body.append('kind', alertKind);
            body.append('id', alertId);
            body.append('__RequestVerificationToken', getAntiForgeryToken());

            fetch(config.resendUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin',
                body: body.toString()
            })
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    showToast(data.message, data.success ? 'success' : 'error');
                    if (!data.success) return;

                    if (data.message.indexOf('cerr') !== -1) {
                        if (row) row.remove();
                        loadList();
                    } else if (row) {
                        var countCell = row.querySelector('.cell-notification-count');
                        var lastNotifiedCell = row.querySelector('.cell-last-notified');
                        if (countCell && countCell.textContent !== '—') {
                            countCell.textContent = String(parseInt(countCell.textContent, 10) + 1);
                        }
                        if (lastNotifiedCell) {
                            var now = new Date();
                            lastNotifiedCell.innerHTML =
                                '<span class="date-cell__date">' + now.toLocaleDateString('es-PE') + '</span>' +
                                '<span class="date-cell__time">' + now.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' }) + '</span>';
                        }
                    }
                })
                .catch(function () {
                    showToast('No se pudo reenviar la alerta.', 'error');
                })
                .finally(function () {
                    btn.disabled = false;
                });
        });
    }

    function bindEvents() {
        var searchInput = qs('search');
        var productSelect = qs('idProduct');
        var warehouseSelect = qs('idWarehouse');
        var pageSizeSelect = qs('alertsPageSize');
        var clearBtn = qs('alertsClearBtn');
        var prevBtn = qs('alertsPrevBtn');
        var nextBtn = qs('alertsNextBtn');

        if (searchInput) {
            state.search = searchInput.value.trim();
            searchInput.addEventListener('input', function () {
                window.clearTimeout(searchTimer);
                searchTimer = window.setTimeout(function () {
                    state.search = searchInput.value.trim();
                    state.page = 1;
                    loadList();
                }, 350);
            });
        }

        if (productSelect) {
            state.idProduct = productSelect.value;
            productSelect.addEventListener('change', function () {
                state.idProduct = productSelect.value;
                state.page = 1;
                loadList();
            });
        }

        if (warehouseSelect) {
            state.idWarehouse = warehouseSelect.value;
            warehouseSelect.addEventListener('change', function () {
                state.idWarehouse = warehouseSelect.value;
                state.page = 1;
                loadList();
            });
        }

        if (pageSizeSelect) {
            pageSizeSelect.addEventListener('change', function () {
                state.pageSize = parseInt(pageSizeSelect.value, 10) || 10;
                state.page = 1;
                loadList();
            });
        }

        if (clearBtn) {
            clearBtn.addEventListener('click', clearFilters);
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', function () {
                if (state.page > 1) {
                    state.page--;
                    loadList();
                }
            });
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', function () {
                state.page++;
                loadList();
            });
        }

        document.querySelectorAll('#alertsTabs .alerts-tab').forEach(function (tab) {
            tab.addEventListener('click', function () {
                var status = tab.getAttribute('data-status') || 'ACTIVE';
                setActiveTab(status);
                state.page = 1;
                loadList();
            });
        });
    }

    function initAlertsPage() {
        var root = qs('alertsPageRoot');
        if (!root || !config.listUrl) return;
        if (initialized && root.dataset.alertsInit === 'true') return;

        root.dataset.alertsInit = 'true';
        initialized = true;

        readFiltersFromDom();
        bindEvents();
        bindResendButtons();
        loadList();
    }

    document.addEventListener('DOMContentLoaded', initAlertsPage);
    document.addEventListener('dashboard:contentLoaded', initAlertsPage);
})();
