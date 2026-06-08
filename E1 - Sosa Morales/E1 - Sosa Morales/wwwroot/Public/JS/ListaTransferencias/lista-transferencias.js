(function () {
    'use strict';

    var state = {
        page: 1, pageSize: 10, search: '',
        idWarehouseOrigin: '', idWarehouseDestination: '', idStatusTransfer: '', idEmployee: '',
        confirmCallback: null, filterOptionsLoaded: false,
        productOptions: [], lineCounter: 0, warehouses: []
    };
    var colCount = 6;

    function urls() { return window.ltrfUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('ltrfToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'ltrf__toast is-visible ' + (isSuccess ? 'ltrf__toast--success' : 'ltrf__toast--error');
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

    function postAction(url, data) {
        var token = getToken();
        var body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        Object.keys(data).forEach(function (key) { if (data[key] !== undefined && data[key] !== null) body.append(key, data[key]); });
        return fetchJson(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token }, body: body.toString() });
    }

    function escapeHtml(text) { var div = document.createElement('div'); div.textContent = text == null ? '' : String(text); return div.innerHTML; }

    function statusBadgeHtml(name, prefix) {
        prefix = prefix || 'ltrf';
        var label = escapeHtml(name || '');
        var n = (name || '').toLowerCase();
        var cls = prefix + '__status';
        if (n.indexOf('cancel') >= 0) cls += ' ' + prefix + '__status--cancel';
        else if (n.indexOf('complet') >= 0) cls += ' ' + prefix + '__status--done';
        return '<span class="' + cls + '">' + label + '</span>';
    }

    function populateSelect(selectId, options, placeholder) {
        var select = qs(selectId);
        if (!select) return;
        var current = select.value;
        select.innerHTML = '<option value="">' + (placeholder || 'Todos') + '</option>';
        (options || []).forEach(function (opt) {
            var option = document.createElement('option');
            option.value = opt.id;
            option.textContent = opt.name;
            select.appendChild(option);
        });
        if (current) select.value = current;
    }

    function refreshDestinationOptions() {
        var originId = qs('ltrfOrigin') ? qs('ltrfOrigin').value : '';
        var destSelect = qs('ltrfDestination');
        if (!destSelect) return;
        var current = destSelect.value;
        var filtered = (state.warehouses || []).filter(function (w) {
            return !originId || String(w.id) !== String(originId);
        });
        populateSelect('ltrfDestination', filtered, 'Seleccione...');
        if (current && String(current) !== String(originId)) destSelect.value = current;
    }

    function loadFilterOptions() {
        return fetchJson(urls().filterOptions).then(function (data) {
            if (!data.success) throw new Error(data.message || 'Error al cargar opciones.');
            var warehouses = data.warehouses || [];
            var employees = data.employees || [];
            var statuses = data.statuses || [];
            state.warehouses = warehouses;
            populateSelect('ltrfFilterOrigin', warehouses, 'Todos');
            populateSelect('ltrfFilterDestination', warehouses, 'Todos');
            populateSelect('ltrfFilterStatus', statuses, 'Todos');
            populateSelect('ltrfFilterEmployee', employees, 'Todos');
            populateSelect('ltrfOrigin', warehouses, 'Seleccione...');
            refreshDestinationOptions();
            populateSelect('ltrfEmployee', employees, 'Seleccione...');
            state.filterOptionsLoaded = true;
        });
    }

    function ensureFilterOptions() {
        if (state.filterOptionsLoaded) return Promise.resolve();
        return loadFilterOptions();
    }

    function loadProductsForOrigin() {
        var originId = qs('ltrfOrigin') ? qs('ltrfOrigin').value : '';
        if (!originId) { state.productOptions = []; return Promise.resolve([]); }
        return fetchJson(buildQuery(urls().productOptions, { idWarehouse: originId })).then(function (data) {
            state.productOptions = (data.products || []).filter(function (p) { return (p.stock || 0) > 0; });
            return state.productOptions;
        });
    }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="ltrf__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            var actions = '<button type="button" class="ltrf__icon-btn ltrf__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button>';
            if (row.canCancel) actions += '<button type="button" class="ltrf__icon-btn ltrf__icon-btn--delete" data-action="cancel" data-id="' + row.id + '" title="Cancelar"><i class="bi bi-x-circle"></i></button>';
            return '<tr data-id="' + row.id + '"><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.fecTransfer) + '</td><td>' + escapeHtml(row.warehouseOriginName) + '</td><td>' + escapeHtml(row.warehouseDestinationName) + '</td><td>' + statusBadgeHtml(row.statusTransferName) + '</td><td>' + escapeHtml(row.employeeName) + '</td><td class="ltrf__td-actions"><div class="ltrf__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function loadList() {
        var u = urls();
        var tbody = qs('ltrfActiveBody');
        if (tbody) tbody.innerHTML = '<tr class="ltrf__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, {
            search: state.search,
            idWarehouseOrigin: state.idWarehouseOrigin,
            idWarehouseDestination: state.idWarehouseDestination,
            idStatusTransfer: state.idStatusTransfer,
            idEmployee: state.idEmployee,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('ltrfPageInfo'), qs('ltrfPrevBtn'), qs('ltrfNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="ltrf__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar.</td></tr>';
            showToast(err.message || 'Error al cargar.', false);
        });
    }

    function getProductStock(productId) {
        var p = (state.productOptions || []).find(function (x) { return String(x.id) === String(productId); });
        return p ? p.stock : 0;
    }

    function buildProductSelectOptions(selectedId) {
        return (state.productOptions || []).map(function (p) {
            return '<option value="' + p.id + '"' + (String(p.id) === String(selectedId) ? ' selected' : '') + '>' + escapeHtml(p.name) + ' (disp: ' + p.stock + ')</option>';
        }).join('');
    }

    function addLineRow(productId, quantity) {
        if (!qs('ltrfOrigin') || !qs('ltrfOrigin').value) {
            showToast('Seleccione primero el almacen de origen.', false);
            return;
        }
        if (!state.productOptions || state.productOptions.length === 0) {
            showToast('No hay productos con stock en el almacen de origen.', false);
            return;
        }
        var tbody = qs('ltrfLinesBody');
        if (!tbody) return;
        state.lineCounter++;
        var rowId = 'line-' + state.lineCounter;
        var tr = document.createElement('tr');
        tr.setAttribute('data-line-id', rowId);
        tr.innerHTML = '<td><select class="ltrf-line-product" required><option value="">Seleccione...</option>' + buildProductSelectOptions(productId || '') + '</select></td>'
            + '<td class="ltrf-line-stock">0</td>'
            + '<td><input type="number" class="ltrf-line-qty" min="1" value="' + (quantity || 1) + '" required /></td>'
            + '<td><button type="button" class="ltrf__icon-btn ltrf__icon-btn--delete ltrf-line-remove" title="Quitar"><i class="bi bi-trash"></i></button></td>';
        tbody.appendChild(tr);
        var select = tr.querySelector('.ltrf-line-product');
        if (productId) select.value = productId;
        updateLineStock(tr);
    }

    function updateLineStock(row) {
        var select = row.querySelector('.ltrf-line-product');
        var stockCell = row.querySelector('.ltrf-line-stock');
        if (select && stockCell) stockCell.textContent = getProductStock(select.value);
    }

    function resetLines() {
        state.lineCounter = 0;
        var tbody = qs('ltrfLinesBody');
        if (tbody) tbody.innerHTML = '';
    }

    function openCreateModal() {
        ensureFilterOptions().then(function () {
            qs('ltrfForm').reset();
            resetLines();
            var now = new Date();
            now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
            qs('ltrfDate').value = now.toISOString().slice(0, 16);
            state.productOptions = [];
            refreshDestinationOptions();
            openModal('ltrfFormModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar opciones.', false); });
    }

    function collectLines() {
        var lines = [];
        var rows = qs('ltrfLinesBody') ? qs('ltrfLinesBody').querySelectorAll('tr') : [];
        rows.forEach(function (row) {
            var productId = row.querySelector('.ltrf-line-product') ? row.querySelector('.ltrf-line-product').value : '';
            var qty = row.querySelector('.ltrf-line-qty') ? parseInt(row.querySelector('.ltrf-line-qty').value, 10) : 0;
            if (productId && qty > 0) lines.push({ idProduct: parseInt(productId, 10), quantity: qty });
        });
        return lines;
    }

    function saveForm() {
        var form = qs('ltrfForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var origin = qs('ltrfOrigin').value;
        var dest = qs('ltrfDestination').value;
        if (origin === dest) { showToast('Origen y destino deben ser diferentes.', false); return; }
        var lines = collectLines();
        if (lines.length === 0) { showToast('Agregue al menos un producto.', false); return; }
        var ids = lines.map(function (l) { return l.idProduct; });
        if (ids.length !== new Set(ids).size) { showToast('No repita productos en la misma transferencia.', false); return; }
        for (var i = 0; i < lines.length; i++) {
            var stock = getProductStock(lines[i].idProduct);
            if (lines[i].quantity > stock) { showToast('Cantidad mayor al stock disponible en origen.', false); return; }
        }
        postAction(urls().create, {
            idWarehouseOrigin: origin,
            idWarehouseDestination: dest,
            idEmployee: qs('ltrfEmployee').value,
            fecTransfer: qs('ltrfDate').value,
            detailsJson: JSON.stringify(lines)
        }).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('ltrfFormModal'); loadList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.id },
                { label: 'Fecha', value: d.fecTransfer },
                { label: 'Origen', value: d.warehouseOriginName },
                { label: 'Destino', value: d.warehouseDestinationName },
                { label: 'Estado', value: d.statusTransferName, badge: true },
                { label: 'Empleado', value: d.employeeName + ' (' + d.employeeUsername + ')' },
                { label: 'Creado', value: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            var html = rows.map(function (r) {
                var val = r.badge ? statusBadgeHtml(r.value) : escapeHtml(r.value);
                return '<div class="ltrf-detail__row"><span class="ltrf-detail__label">' + escapeHtml(r.label) + '</span><span class="ltrf-detail__value">' + val + '</span></div>';
            }).join('');
            html += '<h4 class="ltrf-detail__subtitle">Productos transferidos</h4><table class="ltrf__table ltrf__table--lines"><thead><tr><th>PRODUCTO</th><th>CANTIDAD</th></tr></thead><tbody>';
            html += (d.lines || []).map(function (l) { return '<tr><td>' + escapeHtml(l.productName) + '</td><td>' + escapeHtml(l.quantity) + '</td></tr>'; }).join('');
            html += '</tbody></table>';
            qs('ltrfDetailBody').innerHTML = html;
            openModal('ltrfDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('ltrfConfirmTitle').textContent = title;
        qs('ltrfConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('ltrfConfirmModal');
    }

    function handleCancel(id) {
        confirmAction('Cancelar transferencia', 'Se revertira el stock en los almacenes correspondientes. Desea continuar?', function () {
            postAction(urls().cancel, { id: id }).then(function (res) {
                showToast(res.message, res.success);
                if (res.success) {
                    loadList();
                    document.dispatchEvent(new CustomEvent('transfer:cancelled'));
                }
            });
        });
    }

    function bindEvents() {
        var searchInput = qs('ltrfSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadList(); }, 350);
        });
        qs('ltrfClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadList(); });
        qs('ltrfFilterOrigin')?.addEventListener('change', function (e) { state.idWarehouseOrigin = e.target.value; state.page = 1; loadList(); });
        qs('ltrfFilterDestination')?.addEventListener('change', function (e) { state.idWarehouseDestination = e.target.value; state.page = 1; loadList(); });
        qs('ltrfFilterStatus')?.addEventListener('change', function (e) { state.idStatusTransfer = e.target.value; state.page = 1; loadList(); });
        qs('ltrfFilterEmployee')?.addEventListener('change', function (e) { state.idEmployee = e.target.value; state.page = 1; loadList(); });
        qs('ltrfPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('ltrfPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('ltrfNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        qs('ltrfCreateBtn')?.addEventListener('click', openCreateModal);
        qs('ltrfFormSaveBtn')?.addEventListener('click', saveForm);
        qs('ltrfAddLineBtn')?.addEventListener('click', function () { addLineRow(); });
        qs('ltrfOrigin')?.addEventListener('change', function () {
            refreshDestinationOptions();
            resetLines();
            loadProductsForOrigin().then(function (products) {
                if (products.length === 0) {
                    showToast('El almacen de origen no tiene productos con stock disponible.', false);
                    return;
                }
                addLineRow();
            }).catch(function (err) { showToast(err.message || 'Error al cargar productos.', false); });
        });
        qs('ltrfLinesBody')?.addEventListener('change', function (e) {
            if (e.target.classList.contains('ltrf-line-product')) updateLineStock(e.target.closest('tr'));
        });
        qs('ltrfLinesBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('.ltrf-line-remove');
            if (!btn) return;
            var row = btn.closest('tr');
            if (row) row.remove();
        });
        qs('ltrfConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('ltrfConfirmModal'); });
        document.querySelectorAll('#ltrfRoot ~ .ltrf-modal [data-dismiss="modal"], .ltrf-modal [data-dismiss="modal"]').forEach(function (btn) {
            btn.addEventListener('click', function () { var modal = btn.closest('.ltrf-modal'); if (modal) closeModal(modal.id); });
        });
        document.querySelectorAll('.ltrf-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', function () { var modal = backdrop.closest('.ltrf-modal'); if (modal) closeModal(modal.id); });
        });
        qs('ltrfActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id);
            else if (action === 'cancel') handleCancel(id);
        });
    }

    function init() {
        var root = document.getElementById('ltrfRoot');
        if (!root || root.dataset.initialized === 'true' || !window.ltrfUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions().then(loadList).catch(function () { loadList(); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
