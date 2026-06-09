(function () {
    'use strict';
    var state = { page: 1, pageSize: 10, search: '', idWarehouse: '', idProduct: '', idMovementType: '', movementDirection: '', filterLoaded: false };
    var colCount = 9;

    function urls() { return window.movinvUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function showToast(msg, ok) { var t = qs('movinvToast'); if (!t) return; t.textContent = msg; t.className = 'movinv__toast is-visible ' + (ok ? 'movinv__toast--success' : 'movinv__toast--error'); setTimeout(function () { t.classList.remove('is-visible'); }, 3200); }
    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function buildQuery(u, p) { var url = new URL(u, window.location.origin); Object.keys(p).forEach(function (k) { if (p[k] !== undefined && p[k] !== null && p[k] !== '') url.searchParams.set(k, p[k]); }); return url.toString(); }
    function fetchJson(url) { return fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' }).then(function (r) { return r.json().then(function (d) { if (!r.ok) throw new Error(d.message || 'Error'); return d; }); }); }
    function escapeHtml(t) { var d = document.createElement('div'); d.textContent = t == null ? '' : String(t); return d.innerHTML; }

    function directionBadge(dir) {
        var cls = 'movinv__direction';
        if (dir === 'entrada') cls += ' movinv__direction--in';
        else cls += ' movinv__direction--out';
        var label = dir === 'entrada' ? 'Entrada' : 'Salida';
        return '<span class="' + cls + '">' + label + '</span>';
    }

    function renderRows(tbody, items) {
        if (!items || !items.length) { tbody.innerHTML = '<tr class="movinv__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            return '<tr><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.productName) + '</td><td>' + escapeHtml(row.warehouseName) + '</td><td>' + escapeHtml(row.movementTypeName) + '</td><td>' + directionBadge(row.movementDirection) + '</td><td>' + escapeHtml(row.quantity) + '</td><td>' + escapeHtml(row.reference) + '</td><td>' + escapeHtml(row.fecMovement) + '</td><td>' + escapeHtml(row.employeeName) + '</td><td class="movinv__td-actions"><button type="button" class="movinv__icon-btn movinv__icon-btn--view" data-action="view" data-id="' + row.id + '"><i class="bi bi-eye"></i></button></td></tr>';
        }).join('');
    }

    function updatePagination(info, prev, next, page, totalPages) {
        if (info) info.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prev) prev.disabled = page <= 1;
        if (next) next.disabled = page >= totalPages || totalPages === 0;
    }

    function populateSelect(id, options, placeholder) {
        var sel = qs(id); if (!sel) return;
        var cur = sel.value;
        sel.innerHTML = '<option value="">' + placeholder + '</option>';
        (options || []).forEach(function (o) { var opt = document.createElement('option'); opt.value = o.id; opt.textContent = o.name; sel.appendChild(opt); });
        if (cur) sel.value = cur;
    }

    function loadFilters() {
        return fetchJson(urls().filterOptions).then(function (data) {
            populateSelect('movinvFilterWarehouse', data.warehouses, 'Todos');
            populateSelect('movinvFilterProduct', data.products, 'Todos');
            populateSelect('movinvFilterType', data.movementTypes, 'Todos');
            state.filterLoaded = true;
        });
    }

    function loadList() {
        var tbody = qs('movinvActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="movinv__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, {
            search: state.search, idWarehouse: state.idWarehouse, idProduct: state.idProduct,
            idMovementType: state.idMovementType, movementDirection: state.movementDirection,
            page: state.page, pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('movinvPageInfo'), qs('movinvPrevBtn'), qs('movinvNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (e) { showToast(e.message, false); }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function openDetail(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            var d = res.data;
            var rows = [
                { l: 'ID', v: d.id }, { l: 'Producto', v: d.productName }, { l: 'Almacén', v: d.warehouseName },
                { l: 'Tipo movimiento', v: d.movementTypeName }, { l: 'Dirección', v: d.movementDirection === 'entrada' ? 'Entrada' : 'Salida', badge: true, dir: d.movementDirection },
                { l: 'Cantidad', v: d.quantity }, { l: 'Referencia', v: d.reference || '—' },
                { l: 'Fecha', v: d.fecMovement }, { l: 'Empleado', v: d.employeeName + ' (' + d.employeeUsername + ')' },
                { l: 'Registro', v: d.createdAt }
            ];
            qs('movinvDetailBody').innerHTML = rows.map(function (r) {
                var val = r.badge ? directionBadge(r.dir) : escapeHtml(r.v);
                return '<div class="movinv-detail__row"><span class="movinv-detail__label">' + escapeHtml(r.l) + '</span><span class="movinv-detail__value">' + val + '</span></div>';
            }).join('');
            openModal('movinvDetailModal');
        });
    }

    function bindEvents() {
        var si = qs('movinvSearchInput'), st;
        if (si) si.addEventListener('input', function () { clearTimeout(st); st = setTimeout(function () { state.search = si.value.trim(); state.page = 1; loadList(); }, 350); });
        qs('movinvClearSearchBtn')?.addEventListener('click', function () { state.search = ''; si.value = ''; state.page = 1; loadList(); });
        ['movinvFilterWarehouse', 'movinvFilterProduct', 'movinvFilterType', 'movinvFilterDirection'].forEach(function (id) {
            qs(id)?.addEventListener('change', function (e) {
                if (id === 'movinvFilterWarehouse') state.idWarehouse = e.target.value;
                else if (id === 'movinvFilterProduct') state.idProduct = e.target.value;
                else if (id === 'movinvFilterType') state.idMovementType = e.target.value;
                else state.movementDirection = e.target.value;
                state.page = 1; loadList();
            });
        });
        qs('movinvPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('movinvPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('movinvNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        qs('movinvActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action="view"]'); if (!btn) return;
            openDetail(parseInt(btn.getAttribute('data-id'), 10));
        });
        document.querySelectorAll('.movinv-modal [data-dismiss="modal"]').forEach(function (b) {
            b.addEventListener('click', function () { var m = b.closest('.movinv-modal'); if (m) closeModal(m.id); });
        });
        document.querySelectorAll('.movinv-modal__backdrop').forEach(function (b) {
            b.addEventListener('click', function () { var m = b.closest('.movinv-modal'); if (m) closeModal(m.id); });
        });
    }

    function init() {
        var root = qs('movinvRoot'); if (!root || root.dataset.initialized === 'true' || !window.movinvUrls) return;
        root.dataset.initialized = 'true'; bindEvents();
        loadFilters().then(loadList).catch(loadList);
    }
    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
