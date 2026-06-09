(function () {
    'use strict';
    var state = { page: 1, pageSize: 10, search: '', idWarehouse: '', productPage: 1, productPageSize: 10, productSearch: '', modalWarehouseId: null };
    var summaryCols = 7;
    var productCols = 8;

    function urls() { return window.dtalmUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function showToast(msg, ok) { var t = qs('dtalmToast'); if (!t) return; t.textContent = msg; t.className = 'dtalm__toast is-visible ' + (ok ? 'dtalm__toast--success' : 'dtalm__toast--error'); setTimeout(function () { t.classList.remove('is-visible'); }, 3200); }
    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function buildQuery(u, p) { var url = new URL(u, window.location.origin); Object.keys(p).forEach(function (k) { if (p[k] !== undefined && p[k] !== null && p[k] !== '') url.searchParams.set(k, p[k]); }); return url.toString(); }
    function fetchJson(url) { return fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' }).then(function (r) { return r.json().then(function (d) { if (!r.ok) throw new Error(d.message || 'Error'); return d; }); }); }
    function escapeHtml(t) { var d = document.createElement('div'); d.textContent = t == null ? '' : String(t); return d.innerHTML; }
    function money(v) { return 'S/ ' + (v || '0.00'); }

    function loadMetrics() {
        fetchJson(buildQuery(urls().metrics, { idWarehouse: state.idWarehouse })).then(function (res) {
            if (!res.success) return;
            var d = res.data;
            qs('dtalmMetricWarehouses').textContent = d.warehouseCount;
            qs('dtalmMetricProducts').textContent = d.productCount;
            qs('dtalmMetricStock').textContent = d.totalStock;
            qs('dtalmMetricCost').textContent = money(d.totalCostValue);
            qs('dtalmMetricSale').textContent = money(d.totalSaleValue);
        });
    }

    function renderSummaryRows(tbody, items) {
        if (!items || !items.length) { tbody.innerHTML = '<tr class="dtalm__empty-row"><td colspan="' + summaryCols + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            return '<tr><td class="dtalm__col-num">' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.warehouseName) + '</td><td class="dtalm__col-num">' + escapeHtml(row.productCount) + '</td><td class="dtalm__col-num">' + escapeHtml(row.totalStock) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.totalCostValue) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.totalSaleValue) + '</td><td class="dtalm__td-actions"><button type="button" class="dtalm__icon-btn dtalm__icon-btn--view" data-action="warehouse" data-id="' + row.id + '"><i class="bi bi-eye"></i></button></td></tr>';
        }).join('');
    }

    function renderProductRows(tbody, items) {
        if (!items || !items.length) { tbody.innerHTML = '<tr class="dtalm__empty-row"><td colspan="' + productCols + '">No hay productos en este almacén.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            return '<tr><td>' + escapeHtml(row.productName) + '</td><td>' + escapeHtml(row.brandName) + '</td><td>' + escapeHtml(row.categoryName) + '</td><td class="dtalm__col-num">' + escapeHtml(row.stock) + '</td><td>' + escapeHtml(row.location) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.cost) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.salePrice) + '</td><td class="dtalm__td-actions"><button type="button" class="dtalm__icon-btn dtalm__icon-btn--view" data-action="product" data-id="' + row.id + '"><i class="bi bi-eye"></i></button></td></tr>';
        }).join('');
    }

    function updatePagination(info, prev, next, page, totalPages) {
        if (info) info.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prev) prev.disabled = page <= 1;
        if (next) next.disabled = page >= totalPages || totalPages === 0;
    }

    function loadList() {
        var tbody = qs('dtalmActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        if (state.idWarehouse) {
            var loadHtmlP = '<tr class="dtalm__loading-row"><td colspan="8">Cargando productos...</td></tr>';
            if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtmlP); else if (tbody) tbody.innerHTML = loadHtmlP;
            fetchJson(buildQuery(urls().listProducts, { idWarehouse: state.idWarehouse, search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
                qs('dtalmTableHead').innerHTML = '<tr><th>PRODUCTO</th><th>MARCA</th><th>CATEGORÍA</th><th class="dtalm__col-num">STOCK</th><th>UBICACIÓN</th><th class="dtalm__col-money">COSTO</th><th class="dtalm__col-money">P. VENTA</th><th class="dtalm__th-actions">ACCIONES</th></tr>';
                if (!data.items || !data.items.length) { tbody.innerHTML = '<tr class="dtalm__empty-row"><td colspan="8">No hay productos.</td></tr>'; }
                else {
                    tbody.innerHTML = data.items.map(function (row) {
                        return '<tr><td>' + escapeHtml(row.productName) + '</td><td>' + escapeHtml(row.brandName) + '</td><td>' + escapeHtml(row.categoryName) + '</td><td class="dtalm__col-num">' + escapeHtml(row.stock) + '</td><td>' + escapeHtml(row.location) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.cost) + '</td><td class="dtalm__col-money">S/ ' + escapeHtml(row.salePrice) + '</td><td class="dtalm__td-actions"><button type="button" class="dtalm__icon-btn dtalm__icon-btn--view" data-action="product" data-id="' + row.id + '"><i class="bi bi-eye"></i></button></td></tr>';
                    }).join('');
                }
                updatePagination(qs('dtalmPageInfo'), qs('dtalmPrevBtn'), qs('dtalmNextBtn'), data.page, data.totalPages || 1);
            }).catch(function (e) { showToast(e.message, false); }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
            return;
        }
        qs('dtalmTableHead').innerHTML = '<tr><th class="dtalm__col-num">ID</th><th>ALMACÉN</th><th class="dtalm__col-num">PRODUCTOS</th><th class="dtalm__col-num">STOCK</th><th class="dtalm__col-money">VALOR COSTO</th><th class="dtalm__col-money">VALOR VENTA</th><th class="dtalm__th-actions">ACCIONES</th></tr>';
        var loadHtml = '<tr class="dtalm__loading-row"><td colspan="' + summaryCols + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderSummaryRows(tbody, data.items);
            updatePagination(qs('dtalmPageInfo'), qs('dtalmPrevBtn'), qs('dtalmNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (e) { showToast(e.message, false); }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function loadProductModalList() {
        if (!state.modalWarehouseId) return;
        var tbody = qs('dtalmProductBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="dtalm__loading-row"><td colspan="' + productCols + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().listProducts, { idWarehouse: state.modalWarehouseId, search: state.productSearch, page: state.productPage, pageSize: state.productPageSize })).then(function (data) {
            renderProductRows(tbody, data.items);
            updatePagination(qs('dtalmProductPageInfo'), qs('dtalmProductPrevBtn'), qs('dtalmProductNextBtn'), data.page, data.totalPages || 1);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function openWarehouseModal(id) {
        state.modalWarehouseId = id;
        state.productPage = 1; state.productSearch = '';
        if (qs('dtalmProductSearchInput')) qs('dtalmProductSearchInput').value = '';
        fetchJson(buildQuery(urls().getWarehouse, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            var d = res.data;
            qs('dtalmWarehouseModalTitle').textContent = 'Almacén: ' + d.warehouseName;
            qs('dtalmWarehouseInfo').innerHTML = '<p><strong>Dirección:</strong> ' + escapeHtml(d.address) + '</p><p><strong>Distrito:</strong> ' + escapeHtml(d.districtName || '—') + '</p>';
            loadProductModalList();
            openModal('dtalmWarehouseModal');
        });
    }

    function openProductDetail(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            var d = res.data;
            var rows = [
                { l: 'Almacén', v: d.warehouseName }, { l: 'Producto', v: d.productName },
                { l: 'Marca', v: d.brandName }, { l: 'Categoría', v: d.categoryName },
                { l: 'Stock', v: d.stock }, { l: 'Ubicación', v: d.location || '—' },
                { l: 'Costo unit.', v: 'S/ ' + d.cost }, { l: 'Precio venta', v: 'S/ ' + d.salePrice },
                { l: 'Valor costo', v: 'S/ ' + d.lineCostValue }, { l: 'Valor venta', v: 'S/ ' + d.lineSaleValue }
            ];
            qs('dtalmDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="dtalm-detail__row"><span class="dtalm-detail__label">' + escapeHtml(r.l) + '</span><span class="dtalm-detail__value">' + escapeHtml(r.v) + '</span></div>';
            }).join('');
            openModal('dtalmDetailModal');
        });
    }

    function loadWarehouseOptions() {
        return fetchJson(urls().warehouseOptions).then(function (res) {
            var sel = qs('dtalmFilterWarehouse'); if (!sel) return;
            var cur = sel.value;
            sel.innerHTML = '<option value="">Todos los almacenes</option>';
            (res.items || []).forEach(function (o) { var opt = document.createElement('option'); opt.value = o.id; opt.textContent = o.name; sel.appendChild(opt); });
            if (cur) sel.value = cur;
        });
    }

    function bindEvents() {
        var si = qs('dtalmSearchInput'), st;
        if (si) si.addEventListener('input', function () { clearTimeout(st); st = setTimeout(function () { state.search = si.value.trim(); state.page = 1; loadList(); }, 350); });
        qs('dtalmClearSearchBtn')?.addEventListener('click', function () { state.search = ''; si.value = ''; state.page = 1; loadList(); });
        qs('dtalmFilterWarehouse')?.addEventListener('change', function (e) { state.idWarehouse = e.target.value; state.page = 1; loadMetrics(); loadList(); });
        qs('dtalmPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('dtalmPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('dtalmNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        qs('dtalmActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10);
            if (btn.getAttribute('data-action') === 'warehouse') openWarehouseModal(id);
            else if (btn.getAttribute('data-action') === 'product') openProductDetail(id);
        });
        var psi = qs('dtalmProductSearchInput'), pst;
        if (psi) psi.addEventListener('input', function () { clearTimeout(pst); pst = setTimeout(function () { state.productSearch = psi.value.trim(); state.productPage = 1; loadProductModalList(); }, 350); });
        qs('dtalmProductClearBtn')?.addEventListener('click', function () { state.productSearch = ''; psi.value = ''; state.productPage = 1; loadProductModalList(); });
        qs('dtalmProductPageSize')?.addEventListener('change', function (e) { state.productPageSize = parseInt(e.target.value, 10); state.productPage = 1; loadProductModalList(); });
        qs('dtalmProductPrevBtn')?.addEventListener('click', function () { if (state.productPage > 1) { state.productPage--; loadProductModalList(); } });
        qs('dtalmProductNextBtn')?.addEventListener('click', function () { state.productPage++; loadProductModalList(); });
        qs('dtalmProductBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action="product"]'); if (!btn) return;
            openProductDetail(parseInt(btn.getAttribute('data-id'), 10));
        });
        document.querySelectorAll('.dtalm-modal [data-dismiss="modal"]').forEach(function (b) {
            b.addEventListener('click', function () { var m = b.closest('.dtalm-modal'); if (m) closeModal(m.id); });
        });
        document.querySelectorAll('.dtalm-modal__backdrop').forEach(function (b) {
            b.addEventListener('click', function () { var m = b.closest('.dtalm-modal'); if (m) closeModal(m.id); });
        });
    }

    function init() {
        var root = qs('dtalmRoot'); if (!root || root.dataset.initialized === 'true' || !window.dtalmUrls) return;
        root.dataset.initialized = 'true'; bindEvents();
        loadWarehouseOptions().then(function () { loadMetrics(); loadList(); }).catch(function () { loadMetrics(); loadList(); });
    }
    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
