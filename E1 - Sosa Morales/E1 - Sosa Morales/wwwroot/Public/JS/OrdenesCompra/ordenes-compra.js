(function () {
    'use strict';

    var state = {
        page: 1, pageSize: 10, search: '',
        idPurchase: '', idSupplier: '', idEmployee: '', idPurchaseStatus: '',
        confirmCallback: null, filterOptionsLoaded: false,
        productOptions: [], lineCounter: 0, warehouses: []
    };
    var colCount = 8;

    function urls() { return window.ocmpUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('ocmpToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'ocmp__toast is-visible ' + (isSuccess ? 'ocmp__toast--success' : 'ocmp__toast--error');
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

    function formatMoney(value) {
        if (value == null || value === '') return 'S/ 0.00';
        var num = parseFloat(value);
        if (isNaN(num)) return 'S/ ' + escapeHtml(value);
        return 'S/ ' + num.toFixed(2);
    }

    function statusBadgeHtml(name, prefix) {
        prefix = prefix || 'ocmp';
        var label = escapeHtml(name || '');
        var n = (name || '').toLowerCase();
        var cls = prefix + '__status';
        if (n.indexOf('cancel') >= 0) cls += ' ' + prefix + '__status--cancel';
        else if (n.indexOf('complet') >= 0) cls += ' ' + prefix + '__status--done';
        else if (n.indexOf('pend') >= 0) cls += ' ' + prefix + '__status--pending';
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

    function buildWarehouseSelectOptions(selectedId) {
        return (state.warehouses || []).map(function (w) {
            return '<option value="' + w.id + '"' + (String(w.id) === String(selectedId) ? ' selected' : '') + '>' + escapeHtml(w.name) + '</option>';
        }).join('');
    }

    function loadFilterOptions() {
        return fetchJson(urls().filterOptions).then(function (data) {
            if (!data.success) throw new Error(data.message || 'Error al cargar opciones.');
            var suppliers = data.suppliers || [];
            var employees = data.employees || [];
            var statuses = data.statuses || [];
            state.warehouses = data.warehouses || [];
            populateSelect('ocmpFilterSupplier', suppliers, 'Todos');
            populateSelect('ocmpFilterEmployee', employees, 'Todos');
            populateSelect('ocmpFilterStatus', statuses, 'Todos');
            populateSelect('ocmpSupplier', suppliers, 'Seleccione...');
            populateSelect('ocmpEmployee', employees, 'Seleccione...');
            state.filterOptionsLoaded = true;
        });
    }

    function ensureFilterOptions() {
        if (state.filterOptionsLoaded) return Promise.resolve();
        return loadFilterOptions();
    }

    function loadProductsForSupplier() {
        var supplierId = qs('ocmpSupplier') ? qs('ocmpSupplier').value : '';
        if (!supplierId) { state.productOptions = []; return Promise.resolve([]); }
        return fetchJson(buildQuery(urls().productSupplierOptions, { idSupplier: supplierId })).then(function (data) {
            state.productOptions = data.products || [];
            return state.productOptions;
        });
    }

    function getProductCost(productSupplierId) {
        var p = (state.productOptions || []).find(function (x) { return String(x.id) === String(productSupplierId); });
        return p ? p.supplierCost : 0;
    }

    function buildProductSelectOptions(selectedId) {
        return (state.productOptions || []).map(function (p) {
            return '<option value="' + p.id + '"' + (String(p.id) === String(selectedId) ? ' selected' : '') + '>' + escapeHtml(p.name) + '</option>';
        }).join('');
    }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="ocmp__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            var actions = '<button type="button" class="ocmp__icon-btn ocmp__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button>';
            if (row.canComplete) actions += '<button type="button" class="ocmp__icon-btn ocmp__icon-btn--complete" data-action="complete" data-id="' + row.id + '" title="Completar"><i class="bi bi-check-circle"></i></button>';
            if (row.canCancel) actions += '<button type="button" class="ocmp__icon-btn ocmp__icon-btn--delete" data-action="cancel" data-id="' + row.id + '" data-status="' + escapeHtml(row.purchaseStatusName) + '" title="Cancelar"><i class="bi bi-x-circle"></i></button>';
            return '<tr data-id="' + row.id + '"><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.fecPurchase) + '</td><td>' + escapeHtml(row.supplierName) + '</td><td>' + escapeHtml(row.employeeName) + '</td><td>' + statusBadgeHtml(row.purchaseStatusName) + '</td><td class="ocmp__col-money">' + formatMoney(row.subtotal) + '</td><td class="ocmp__col-money">' + formatMoney(row.tax) + '</td><td class="ocmp__col-money">' + formatMoney(row.total) + '</td><td class="ocmp__td-actions"><div class="ocmp__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function updatePurchaseClearBtn() {
        var btn = qs('ocmpClearPurchaseBtn');
        var input = qs('ocmpFilterPurchase');
        if (!btn || !input) return;
        btn.classList.toggle('is-visible', !!input.value);
    }

    function loadList() {
        var u = urls();
        var tbody = qs('ocmpActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="ocmp__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.list, {
            search: state.search,
            idPurchase: state.idPurchase,
            idSupplier: state.idSupplier,
            idEmployee: state.idEmployee,
            idPurchaseStatus: state.idPurchaseStatus,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('ocmpPageInfo'), qs('ocmpPrevBtn'), qs('ocmpNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="ocmp__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar.</td></tr>';
            showToast(err.message || 'Error al cargar.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function addLineRow(productSupplierId, quantity, unitCost, warehouseId) {
        if (!qs('ocmpSupplier') || !qs('ocmpSupplier').value) {
            showToast('Seleccione primero el proveedor.', false);
            return Promise.reject(new Error('Proveedor requerido'));
        }
        if (!state.productOptions || state.productOptions.length === 0) {
            showToast('No hay productos disponibles para el proveedor seleccionado.', false);
            return Promise.reject(new Error('Sin productos'));
        }
        var tbody = qs('ocmpLinesBody');
        if (!tbody) return Promise.resolve();
        state.lineCounter++;
        var rowId = 'line-' + state.lineCounter;
        var defaultCost = unitCost != null ? unitCost : getProductCost(productSupplierId);
        var tr = document.createElement('tr');
        tr.setAttribute('data-line-id', rowId);
        tr.innerHTML = '<td><select class="ocmp-line-product" required><option value="">Seleccione...</option>' + buildProductSelectOptions(productSupplierId || '') + '</select></td>'
            + '<td><input type="number" class="ocmp-line-qty" min="1" value="' + (quantity || 1) + '" required /></td>'
            + '<td><input type="number" class="ocmp-line-cost" min="0.01" step="0.01" value="' + defaultCost + '" required /></td>'
            + '<td><select class="ocmp-line-warehouse" required><option value="">Seleccione...</option>' + buildWarehouseSelectOptions(warehouseId || '') + '</select></td>'
            + '<td><button type="button" class="ocmp__icon-btn ocmp__icon-btn--delete ocmp-line-remove" title="Quitar"><i class="bi bi-trash"></i></button></td>';
        tbody.appendChild(tr);
        var select = tr.querySelector('.ocmp-line-product');
        if (productSupplierId) select.value = productSupplierId;
        if (warehouseId) tr.querySelector('.ocmp-line-warehouse').value = warehouseId;
        return Promise.resolve();
    }

    function resetLines() {
        state.lineCounter = 0;
        var tbody = qs('ocmpLinesBody');
        if (tbody) tbody.innerHTML = '';
    }

    function ensureProductsLoaded() {
        var supplierId = qs('ocmpSupplier') ? qs('ocmpSupplier').value : '';
        if (!supplierId) {
            state.productOptions = [];
            return Promise.reject(new Error('Proveedor requerido'));
        }
        if (state.productOptions && state.productOptions.length > 0) {
            return Promise.resolve(state.productOptions);
        }
        return loadProductsForSupplier().then(function (products) {
            if (products.length === 0) {
                showToast('El proveedor no tiene productos asociados en Producto proveedores.', false);
                return Promise.reject(new Error('Sin productos'));
            }
            return products;
        });
    }

    function addProductLine() {
        return ensureProductsLoaded().then(function () { return addLineRow(); });
    }

    function openCreateModal() {
        ensureFilterOptions().then(function () {
            qs('ocmpForm').reset();
            resetLines();
            var now = new Date();
            now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
            qs('ocmpDate').value = now.toISOString().slice(0, 16);
            state.productOptions = [];
            openModal('ocmpFormModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar opciones.', false); });
    }

    function collectLines() {
        var lines = [];
        var rows = qs('ocmpLinesBody') ? qs('ocmpLinesBody').querySelectorAll('tr') : [];
        rows.forEach(function (row) {
            var productSupplierId = row.querySelector('.ocmp-line-product') ? row.querySelector('.ocmp-line-product').value : '';
            var qty = row.querySelector('.ocmp-line-qty') ? parseInt(row.querySelector('.ocmp-line-qty').value, 10) : 0;
            var unitCost = row.querySelector('.ocmp-line-cost') ? parseFloat(row.querySelector('.ocmp-line-cost').value) : 0;
            var warehouseId = row.querySelector('.ocmp-line-warehouse') ? row.querySelector('.ocmp-line-warehouse').value : '';
            if (productSupplierId && qty > 0 && unitCost > 0 && warehouseId) {
                lines.push({
                    idProductSupplier: parseInt(productSupplierId, 10),
                    quantity: qty,
                    unitCost: unitCost,
                    idWarehouse: parseInt(warehouseId, 10)
                });
            }
        });
        return lines;
    }

    function saveForm() {
        var form = qs('ocmpForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var lines = collectLines();
        if (lines.length === 0) { showToast('Agregue al menos un producto.', false); return; }
        var keys = lines.map(function (l) { return l.idProductSupplier + '-' + l.idWarehouse; });
        if (keys.length !== new Set(keys).size) { showToast('No repita el mismo producto en el mismo almacén.', false); return; }
        postAction(urls().create, {
            idSupplier: qs('ocmpSupplier').value,
            idEmployee: qs('ocmpEmployee').value,
            fecPurchase: qs('ocmpDate').value,
            detailsJson: JSON.stringify(lines)
        }).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('ocmpFormModal'); loadList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.id },
                { label: 'Fecha', value: d.fecPurchase },
                { label: 'Proveedor', value: d.supplierName },
                { label: 'Estado', value: d.statusPurchaseName, badge: true },
                { label: 'Empleado', value: d.employeeName + ' (' + d.employeeUsername + ')' },
                { label: 'Subtotal', value: formatMoney(d.subtotal) },
                { label: 'IGV', value: formatMoney(d.tax) },
                { label: 'Total', value: formatMoney(d.total) },
                { label: 'Creado', value: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            var html = rows.map(function (r) {
                var val = r.badge ? statusBadgeHtml(r.value) : escapeHtml(r.value);
                return '<div class="ocmp-detail__row"><span class="ocmp-detail__label">' + escapeHtml(r.label) + '</span><span class="ocmp-detail__value">' + val + '</span></div>';
            }).join('');
            html += '<h4 class="ocmp-detail__subtitle">Productos comprados</h4><table class="ocmp__table ocmp__table--lines"><thead><tr><th>PRODUCTO</th><th>CANT.</th><th>COSTO UNIT.</th><th>SUBTOTAL</th></tr></thead><tbody>';
            html += (d.lines || []).map(function (l) {
                return '<tr><td>' + escapeHtml(l.productName) + '</td><td>' + escapeHtml(l.quantity) + '</td><td class="ocmp__col-money">' + formatMoney(l.unitCost) + '</td><td class="ocmp__col-money">' + formatMoney(l.subtotal) + '</td></tr>';
            }).join('');
            html += '</tbody></table>';
            if (d.warehouseLines && d.warehouseLines.length > 0) {
                var whTitle = (d.statusPurchaseName || '').toLowerCase().indexOf('complet') >= 0
                    ? 'Distribucion por almacen'
                    : 'Distribucion planificada (pendiente de completar)';
                html += '<h4 class="ocmp-detail__subtitle">' + whTitle + '</h4><table class="ocmp__table ocmp__table--lines"><thead><tr><th>PRODUCTO</th><th>ALMACEN</th><th>CANT.</th></tr></thead><tbody>';
                html += d.warehouseLines.map(function (w) {
                    return '<tr><td>' + escapeHtml(w.productName) + '</td><td>' + escapeHtml(w.warehouseName) + '</td><td>' + escapeHtml(w.quantity) + '</td></tr>';
                }).join('');
                html += '</tbody></table>';
            }
            qs('ocmpDetailBody').innerHTML = html;
            openModal('ocmpDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function confirmAction(title, message, callback, confirmClass) {
        qs('ocmpConfirmTitle').textContent = title;
        qs('ocmpConfirmMessage').textContent = message;
        qs('ocmpConfirmBtn').className = 'ocmp__btn ' + (confirmClass || 'ocmp__btn--danger');
        state.confirmCallback = callback;
        openModal('ocmpConfirmModal');
    }

    function handleComplete(id) {
        confirmAction('Completar compra', 'Se distribuira el stock a los almacenes indicados. Desea continuar?', function () {
            postAction(urls().complete, { id: id }).then(function (res) {
                showToast(res.message, res.success);
                if (res.success) {
                    loadList();
                    document.dispatchEvent(new CustomEvent('purchase:completed'));
                }
            });
        }, 'ocmp__btn--primary');
    }

    function handleCancel(id, statusName) {
        var isCompleted = (statusName || '').toLowerCase().indexOf('complet') >= 0;
        var msg = isCompleted
            ? 'Se revertira el stock en los almacenes correspondientes. Desea continuar?'
            : 'Se cancelara la compra sin afectar el inventario. Desea continuar?';
        confirmAction('Cancelar compra', msg, function () {
            postAction(urls().cancel, { id: id }).then(function (res) {
                showToast(res.message, res.success);
                if (res.success) {
                    loadList();
                    document.dispatchEvent(new CustomEvent('purchase:cancelled'));
                }
            });
        });
    }

    function bindEvents() {
        var searchInput = qs('ocmpSearchInput');
        var purchaseInput = qs('ocmpFilterPurchase');
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
        qs('ocmpClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadList(); });
        qs('ocmpClearPurchaseBtn')?.addEventListener('click', function () {
            state.idPurchase = '';
            if (purchaseInput) purchaseInput.value = '';
            updatePurchaseClearBtn();
            state.page = 1;
            loadList();
        });
        qs('ocmpFilterSupplier')?.addEventListener('change', function (e) { state.idSupplier = e.target.value; state.page = 1; loadList(); });
        qs('ocmpFilterEmployee')?.addEventListener('change', function (e) { state.idEmployee = e.target.value; state.page = 1; loadList(); });
        qs('ocmpFilterStatus')?.addEventListener('change', function (e) { state.idPurchaseStatus = e.target.value; state.page = 1; loadList(); });
        qs('ocmpPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('ocmpPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('ocmpNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        qs('ocmpCreateBtn')?.addEventListener('click', openCreateModal);
        qs('ocmpFormSaveBtn')?.addEventListener('click', saveForm);
        qs('ocmpAddLineBtn')?.addEventListener('click', function () {
            addProductLine().catch(function (err) {
                if (err && err.message !== 'Proveedor requerido' && err.message !== 'Sin productos') {
                    showToast(err.message || 'Error al cargar productos.', false);
                }
            });
        });
        qs('ocmpSupplier')?.addEventListener('change', function () {
            resetLines();
            state.productOptions = [];
            addProductLine().catch(function (err) {
                if (err && err.message !== 'Proveedor requerido' && err.message !== 'Sin productos') {
                    showToast(err.message || 'Error al cargar productos.', false);
                }
            });
        });
        qs('ocmpLinesBody')?.addEventListener('change', function (e) {
            if (e.target.classList.contains('ocmp-line-product')) {
                var row = e.target.closest('tr');
                var costInput = row.querySelector('.ocmp-line-cost');
                if (costInput) costInput.value = getProductCost(e.target.value);
            }
        });
        qs('ocmpLinesBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('.ocmp-line-remove');
            if (!btn) return;
            var row = btn.closest('tr');
            if (row) row.remove();
        });
        qs('ocmpConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('ocmpConfirmModal'); });
        document.querySelectorAll('.ocmp-modal [data-dismiss="modal"]').forEach(function (btn) {
            btn.addEventListener('click', function () { var modal = btn.closest('.ocmp-modal'); if (modal) closeModal(modal.id); });
        });
        document.querySelectorAll('.ocmp-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', function () { var modal = backdrop.closest('.ocmp-modal'); if (modal) closeModal(modal.id); });
        });
        qs('ocmpActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id);
            else if (action === 'complete') handleComplete(id);
            else if (action === 'cancel') handleCancel(id, btn.getAttribute('data-status'));
        });
    }

    function init() {
        var root = document.getElementById('ocmpRoot');
        if (!root || root.dataset.initialized === 'true' || !window.ocmpUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions().then(loadList).catch(function () { loadList(); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
