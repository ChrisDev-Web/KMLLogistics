(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', confirmCallback: null, preview: null, saleOptions: [], boxOptions: [] };
    var colCount = 5;

    function urls() { return window.dcbUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('dcbToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'dcb__toast is-visible ' + (isSuccess ? 'dcb__toast--success' : 'dcb__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.dcb-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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
            return r.text().then(function (text) {
                var data = null;
                try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }
                if (!r.ok) {
                    var msg = (data && (data.message || data.title)) || (text && text.length < 300 ? text : 'Error de red');
                    throw new Error(msg);
                }
                if (data === null) throw new Error('Respuesta invalida del servidor.');
                return data;
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
    function fmtNum(v) { return v == null || v === '' ? '—' : Number(v).toLocaleString(undefined, { maximumFractionDigits: 2 }); }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="dcb__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            var actions = '<button type="button" class="dcb__icon-btn dcb__icon-btn--view" data-action="view" data-id="' + row.idBoxDetail + '" title="Ver detalle"><i class="bi bi-eye"></i></button>'
                + '<button type="button" class="dcb__icon-btn dcb__icon-btn--delete" data-action="delete" data-id="' + row.idBoxDetail + '" title="Eliminar"><i class="bi bi-trash"></i></button>';
            return '<tr data-id="' + row.idBoxDetail + '">'
                + '<td>' + escapeHtml(row.idBoxDetail) + '</td>'
                + '<td>Caja #' + escapeHtml(row.idBox) + '</td>'
                + '<td>#' + escapeHtml(row.idSale) + '</td>'
                + '<td>' + escapeHtml(row.productName) + '</td>'
                + '<td>' + escapeHtml(row.quantity) + '</td>'
                + '<td class="dcb__td-actions"><div class="dcb__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function loadList() {
        var tbody = qs('dcbActiveBody');
        var scrollEl = qs('dcbTableScroll');
        var loadHtml = '<tr class="dcb__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('dcbPageInfo'), qs('dcbPrevBtn'), qs('dcbNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="dcb__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function fillSelect(select, items, valueKey, textKey, placeholder) {
        if (!select) return;
        select.innerHTML = '<option value="">' + escapeHtml(placeholder) + '</option>'
            + (items || []).map(function (item) {
                return '<option value="' + escapeHtml(item[valueKey]) + '">' + escapeHtml(item[textKey]) + '</option>';
            }).join('');
    }

    function resetPreview() {
        state.preview = null;
        qs('dcbPreviewWrap').hidden = true;
        qs('dcbPreviewBody').innerHTML = '';
        qs('dcbPreviewSummary').innerHTML = '';
        qs('dcbFormSaveBtn').disabled = true;
    }

    function loadPreview(saleDetailId) {
        resetPreview();
        if (!saleDetailId) return;
        fetchJson(buildQuery(urls().preview, { saleDetailId: saleDetailId })).then(function (res) {
            if (!res.success) { showToast(res.message || 'No se pudo cargar la venta.', false); return; }
            state.preview = res;
            var pendingTotal = (res.lines || []).reduce(function (sum, line) { return sum + (line.pendingQuantity || 0); }, 0);
            if (pendingTotal <= 0) {
                showToast('Esta venta ya fue empaquetada por completo.', false);
                return;
            }
            qs('dcbPreviewSummary').innerHTML =
                '<span>Venta <strong>#' + escapeHtml(res.idSale) + '</strong></span>'
                + '<span>Peso total: <strong>' + fmtNum(res.totalWeight) + ' kg</strong></span>'
                + '<span>Volumen total: <strong>' + fmtNum(res.totalVolume) + '</strong></span>'
                + (res.suggestedIdBox ? '<span>Caja sugerida: <strong>#' + escapeHtml(res.suggestedIdBox) + '</strong></span>' : '<span>Sin caja sugerida (revise dimensiones)</span>');
            qs('dcbPreviewBody').innerHTML = (res.lines || []).map(function (line) {
                return '<tr><td>' + escapeHtml(line.productName) + '</td>'
                    + '<td>' + escapeHtml(line.soldQuantity) + '</td>'
                    + '<td>' + escapeHtml(line.packedQuantity) + '</td>'
                    + '<td>' + escapeHtml(line.pendingQuantity) + '</td>'
                    + '<td>' + fmtNum(line.unitWeight) + '</td>'
                    + '<td>' + fmtNum(line.unitVolume) + '</td></tr>';
            }).join('');
            qs('dcbPreviewWrap').hidden = false;
            if (res.suggestedIdBox) qs('dcbBox').value = String(res.suggestedIdBox);
            qs('dcbFormSaveBtn').disabled = !qs('dcbBox').value;
        }).catch(function (err) { showToast(err.message || 'Error al cargar preview.', false); });
    }

    function openCreateModal() {
        resetPreview();
        qs('dcbSaleDetail').value = '';
        qs('dcbBox').value = '';
        fetchJson(urls().options).then(function (data) {
            state.saleOptions = data.saleDetails || [];
            state.boxOptions = data.boxes || [];
            fillSelect(qs('dcbSaleDetail'), state.saleOptions, 'idSaleDetail', 'name', 'Seleccione una venta...');
            fillSelect(qs('dcbBox'), state.boxOptions, 'idBox', 'name', 'Seleccione una caja...');
            openModal('dcbFormModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar opciones.', false); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.idBoxDetail },
                { label: 'Caja', value: 'Caja #' + d.idBox },
                { label: 'Venta', value: '#' + d.idSale },
                { label: 'Producto', value: d.productName },
                { label: 'Cantidad', value: d.quantity }
            ];
            qs('dcbDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="dcb-detail__row"><span class="dcb-detail__label">' + escapeHtml(r.label) + '</span><span class="dcb-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('dcbDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        if (!state.preview || !state.preview.idSale) { showToast('Seleccione una venta valida.', false); return; }
        var boxId = parseInt(qs('dcbBox').value, 10);
        if (!boxId) { showToast('Seleccione una caja.', false); return; }
        postAction(urls().createBySale, { boxId: boxId, saleId: state.preview.idSale }).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('dcbFormModal'); loadList(); }
        }).catch(function (err) { showToast(err.message || 'Error al empaquetar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('dcbConfirmTitle').textContent = title;
        qs('dcbConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('dcbConfirmModal');
    }

    function handleDelete(id) {
        confirmAction('Eliminar detalle', 'Desea eliminar este detalle de caja?', function () {
            postAction(urls().delete, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadList(); });
        });
    }

    function bindEvents() {
        var searchInput = qs('dcbSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadList(); }, 350);
        });
        qs('dcbClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadList(); });
        qs('dcbPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadList(); });
        qs('dcbPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadList(); } });
        qs('dcbNextBtn')?.addEventListener('click', function () { state.page++; loadList(); });
        qs('dcbCreateBtn')?.addEventListener('click', openCreateModal);
        qs('dcbFormSaveBtn')?.addEventListener('click', saveForm);
        qs('dcbSaleDetail')?.addEventListener('change', function (e) { loadPreview(parseInt(e.target.value, 10) || null); });
        qs('dcbBox')?.addEventListener('change', function () { qs('dcbFormSaveBtn').disabled = !qs('dcbBox').value || !state.preview; });
        qs('dcbConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('dcbConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.dcb-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('dcbActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'delete') handleDelete(id);
        });
    }

    function init() {
        var root = document.getElementById('dcbRoot');
        if (!root || root.dataset.initialized === 'true' || !window.dcbUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
