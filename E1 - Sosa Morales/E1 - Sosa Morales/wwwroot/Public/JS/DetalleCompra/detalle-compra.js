(function () {
    'use strict';

    var state = {
        page: 1, pageSize: 10, search: '',
        idPurchase: '', idProduct: '', idSupplier: '', idPurchaseStatus: '',
        filterOptionsLoaded: false
    };
    var colCount = 8;

    function urls() { return window.dcmpUrls || {}; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('dcmpToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'dcmp__toast is-visible ' + (isSuccess ? 'dcmp__toast--success' : 'dcmp__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }

    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (key) { if (params[key] !== undefined && params[key] !== null && params[key] !== '') url.searchParams.set(key, params[key]); });
        return url.toString();
    }

    function fetchJson(url, options) {
        options = options || {};
        options.headers = options.headers || {};
        options.headers['X-Requested-With'] = 'XMLHttpRequest';
        options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) {
            return r.json().then(function (data) {
                if (!r.ok) throw new Error(data.message || data.title || 'Error de red');
                return data;
            }).catch(function (err) {
                if (!r.ok) throw new Error('Error de red');
                throw err;
            });
        });
    }

    function escapeHtml(text) { var div = document.createElement('div'); div.textContent = text == null ? '' : String(text); return div.innerHTML; }

    function formatMoney(value) {
        if (value == null || value === '') return 'S/ 0.00';
        var num = parseFloat(value);
        if (isNaN(num)) return 'S/ ' + escapeHtml(value);
        return 'S/ ' + num.toFixed(2);
    }

    function statusBadgeHtml(name) {
        var label = escapeHtml(name || '');
        var n = (name || '').toLowerCase();
        var cls = 'dcmp__status';
        if (n.indexOf('cancel') >= 0) cls += ' dcmp__status--cancel';
        else if (n.indexOf('complet') >= 0) cls += ' dcmp__status--done';
        return '<span class="' + cls + '">' + label + '</span>';
    }

    function populateSelect(selectId, options) {
        var select = qs(selectId);
        if (!select) return;
        var current = select.value;
        select.innerHTML = '<option value="">Todos</option>';
        (options || []).forEach(function (opt) {
            var option = document.createElement('option');
            option.value = opt.id;
            option.textContent = opt.name;
            select.appendChild(option);
        });
        if (current) select.value = current;
    }

    function loadFilterOptions() {
        return fetchJson(urls().filterOptions).then(function (data) {
            if (!data.success) throw new Error(data.message || 'Error al cargar opciones.');
            populateSelect('dcmpFilterProduct', data.products || []);
            populateSelect('dcmpFilterSupplier', data.suppliers || []);
            populateSelect('dcmpFilterStatus', data.statuses || []);
            state.filterOptionsLoaded = true;
        });
    }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="dcmp__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            return '<tr data-id="' + row.id + '"><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.idPurchase) + '</td><td>' + escapeHtml(row.productName) + '</td><td>' + escapeHtml(row.supplierName) + '</td><td>' + escapeHtml(row.quantity) + '</td><td class="dcmp__col-money">' + formatMoney(row.unitCost) + '</td><td class="dcmp__col-money">' + formatMoney(row.subtotal) + '</td><td>' + statusBadgeHtml(row.purchaseStatusName) + '</td><td>' + escapeHtml(row.fecPurchase) + '</td><td class="dcmp__td-actions"><div class="dcmp__row-actions"><button type="button" class="dcmp__icon-btn dcmp__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button></div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function loadList() {
        var tbody = qs('dcmpActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="dcmp__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, {
            search: state.search,
            idPurchase: state.idPurchase,
            idProduct: state.idProduct,
            idSupplier: state.idSupplier,
            idPurchaseStatus: state.idPurchaseStatus,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('dcmpPageInfo'), qs('dcmpPrevBtn'), qs('dcmpNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="dcmp__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar.</td></tr>';
            showToast(err.message || 'Error al cargar.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID detalle', value: d.id },
                { label: 'N° compra', value: d.idPurchase },
                { label: 'Producto', value: d.productName },
                { label: 'Cantidad', value: d.quantity },
                { label: 'Costo unitario', value: formatMoney(d.unitCost) },
                { label: 'Subtotal', value: formatMoney(d.subtotal) },
                { label: 'Proveedor', value: d.supplierName },
                { label: 'Estado compra', value: d.purchaseStatusName, badge: true },
                { label: 'Fecha compra', value: d.fecPurchase },
                { label: 'Empleado', value: d.employeeName + ' (' + d.employeeUsername + ')' },
                { label: 'Registro compra', value: d.purchaseCreatedAt }
            ];
            qs('dcmpDetailBody').innerHTML = rows.map(function (r) {
                var val = r.badge ? statusBadgeHtml(r.value) : escapeHtml(r.value);
                return '<div class="dcmp-detail__row"><span class="dcmp-detail__label">' + escapeHtml(r.label) + '</span><span class="dcmp-detail__value">' + val + '</span></div>';
            }).join('');
            openModal('dcmpDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function updatePurchaseClearBtn() {
        var btn = qs('dcmpClearPurchaseBtn');
        var input = qs('dcmpFilterPurchase');
        if (!btn || !input) return;
        btn.classList.toggle('is-visible', !!input.value);
    }

    function bindEvents() {
        var searchInput = qs('dcmpSearchInput');
        var purchaseInput = qs('dcmpFilterPurchase');
        var searchTimer = null;
        var purchaseTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadList(); }, 350);
        });
        if (purchaseInput) purchaseInput.addEventListener('input', function () {
            clearTimeout(purchaseTimer);
            updatePurchaseClearBtn();
            purchaseTimer = setTimeout(function () {
                state.idPurchase = purchaseInput.value.trim();
                state.page = 1;
                loadList();
            }, 350);
        });
        qs('dcmpClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadList(); });
        qs('dcmpClearPurchaseBtn')?.addEventListener('click', function () {
            state.idPurchase = '';
            if (purchaseInput) purchaseInput.value = '';
            updatePurchaseClearBtn();
            state.page = 1;
            loadList();
        });
        qs('dcmpFilterProduct')?.addEventListener('change', function (e) { state.idProduct = e.target.value; state.page = 1; loadList(); });
        qs('dcmpFilterSupplier')?.addEventListener('change', function (e) { state.idSupplier = e.target.value; state.page = 1; loadList(); });
        qs('dcmpFilterStatus')?.addEventListener('change', function (e) { state.idPurchaseStatus = e.target.value; state.page = 1; loadList(); });
        qs('dcmpPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('dcmpPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('dcmpNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        document.querySelectorAll('.dcmp-modal [data-dismiss="modal"]').forEach(function (btn) {
            btn.addEventListener('click', function () { var modal = btn.closest('.dcmp-modal'); if (modal) closeModal(modal.id); });
        });
        document.querySelectorAll('.dcmp-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', function () { var modal = backdrop.closest('.dcmp-modal'); if (modal) closeModal(modal.id); });
        });
        qs('dcmpActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action="view"]'); if (!btn) return;
            openDetailModal(parseInt(btn.getAttribute('data-id'), 10));
        });
        document.addEventListener('purchase:cancelled', function () { loadList(); });
        document.addEventListener('purchase:completed', function () { loadList(); });
    }

    function init() {
        var root = document.getElementById('dcmpRoot');
        if (!root || root.dataset.initialized === 'true' || !window.dcmpUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions().then(loadList).catch(function () { loadList(); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
