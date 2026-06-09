(function () {
    'use strict';

    var state = {
        page: 1, pageSize: 10, search: '',
        idTransfer: '', idProduct: '', idWarehouseOrigin: '', idWarehouseDestination: '', idStatusTransfer: '',
        filterOptionsLoaded: false
    };
    var colCount = 8;

    function urls() { return window.dtrfUrls || {}; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('dtrfToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'dtrf__toast is-visible ' + (isSuccess ? 'dtrf__toast--success' : 'dtrf__toast--error');
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

    function statusBadgeHtml(name) {
        var label = escapeHtml(name || '');
        var n = (name || '').toLowerCase();
        var cls = 'dtrf__status';
        if (n.indexOf('cancel') >= 0) cls += ' dtrf__status--cancel';
        else if (n.indexOf('complet') >= 0) cls += ' dtrf__status--done';
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
            populateSelect('dtrfFilterProduct', data.products || []);
            populateSelect('dtrfFilterOrigin', data.warehouses || []);
            populateSelect('dtrfFilterDestination', data.warehouses || []);
            populateSelect('dtrfFilterStatus', data.statuses || []);
            state.filterOptionsLoaded = true;
        });
    }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="dtrf__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            return '<tr data-id="' + row.id + '"><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.idTransfer) + '</td><td>' + escapeHtml(row.productName) + '</td><td>' + escapeHtml(row.quantity) + '</td><td>' + escapeHtml(row.warehouseOriginName) + '</td><td>' + escapeHtml(row.warehouseDestinationName) + '</td><td>' + statusBadgeHtml(row.statusTransferName) + '</td><td>' + escapeHtml(row.fecTransfer) + '</td><td class="dtrf__td-actions"><div class="dtrf__row-actions"><button type="button" class="dtrf__icon-btn dtrf__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button></div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function loadList() {
        var tbody = qs('dtrfActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="dtrf__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, {
            search: state.search,
            idTransfer: state.idTransfer,
            idProduct: state.idProduct,
            idWarehouseOrigin: state.idWarehouseOrigin,
            idWarehouseDestination: state.idWarehouseDestination,
            idStatusTransfer: state.idStatusTransfer,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('dtrfPageInfo'), qs('dtrfPrevBtn'), qs('dtrfNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="dtrf__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar.</td></tr>';
            showToast(err.message || 'Error al cargar.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID detalle', value: d.id },
                { label: 'ID transferencia', value: d.idTransfer },
                { label: 'Producto', value: d.productName },
                { label: 'Cantidad', value: d.quantity },
                { label: 'Almacen origen', value: d.warehouseOriginName },
                { label: 'Almacen destino', value: d.warehouseDestinationName },
                { label: 'Estado transferencia', value: d.statusTransferName, badge: true },
                { label: 'Fecha transferencia', value: d.fecTransfer },
                { label: 'Empleado', value: d.employeeName + ' (' + d.employeeUsername + ')' },
                { label: 'Registro transferencia', value: d.transferCreatedAt }
            ];
            qs('dtrfDetailBody').innerHTML = rows.map(function (r) {
                var val = r.badge ? statusBadgeHtml(r.value) : escapeHtml(r.value);
                return '<div class="dtrf-detail__row"><span class="dtrf-detail__label">' + escapeHtml(r.label) + '</span><span class="dtrf-detail__value">' + val + '</span></div>';
            }).join('');
            openModal('dtrfDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function updateTransferClearBtn() {
        var btn = qs('dtrfClearTransferBtn');
        var input = qs('dtrfFilterTransfer');
        if (!btn || !input) return;
        btn.classList.toggle('is-visible', !!input.value);
    }

    function bindEvents() {
        var searchInput = qs('dtrfSearchInput');
        var transferInput = qs('dtrfFilterTransfer');
        var searchTimer = null;
        var transferTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadList(); }, 350);
        });
        if (transferInput) transferInput.addEventListener('input', function () {
            clearTimeout(transferTimer);
            updateTransferClearBtn();
            transferTimer = setTimeout(function () {
                state.idTransfer = transferInput.value.trim();
                state.page = 1;
                loadList();
            }, 350);
        });
        qs('dtrfClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadList(); });
        qs('dtrfClearTransferBtn')?.addEventListener('click', function () {
            state.idTransfer = '';
            if (transferInput) transferInput.value = '';
            updateTransferClearBtn();
            state.page = 1;
            loadList();
        });
        qs('dtrfFilterProduct')?.addEventListener('change', function (e) { state.idProduct = e.target.value; state.page = 1; loadList(); });
        qs('dtrfFilterOrigin')?.addEventListener('change', function (e) { state.idWarehouseOrigin = e.target.value; state.page = 1; loadList(); });
        qs('dtrfFilterDestination')?.addEventListener('change', function (e) { state.idWarehouseDestination = e.target.value; state.page = 1; loadList(); });
        qs('dtrfFilterStatus')?.addEventListener('change', function (e) { state.idStatusTransfer = e.target.value; state.page = 1; loadList(); });
        qs('dtrfPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('dtrfPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('dtrfNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        document.querySelectorAll('.dtrf-modal [data-dismiss="modal"]').forEach(function (btn) {
            btn.addEventListener('click', function () { var modal = btn.closest('.dtrf-modal'); if (modal) closeModal(modal.id); });
        });
        document.querySelectorAll('.dtrf-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', function () { var modal = backdrop.closest('.dtrf-modal'); if (modal) closeModal(modal.id); });
        });
        qs('dtrfActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action="view"]'); if (!btn) return;
            openDetailModal(parseInt(btn.getAttribute('data-id'), 10));
        });
        document.addEventListener('transfer:cancelled', function () { loadList(); });
    }

    function init() {
        var root = document.getElementById('dtrfRoot');
        if (!root || root.dataset.initialized === 'true' || !window.dtrfUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions().then(loadList).catch(function () { loadList(); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
