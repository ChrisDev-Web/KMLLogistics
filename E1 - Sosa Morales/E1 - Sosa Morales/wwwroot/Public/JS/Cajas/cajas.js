(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 6;

    function urls() { return window.cajUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('cajToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'caj__toast is-visible ' + (isSuccess ? 'caj__toast--success' : 'caj__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.caj-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="caj__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.idBox) + '</td>'
                + '<td>' + fmtNum(row.weight) + '</td>'
                + '<td>' + fmtNum(row.height) + '</td>'
                + '<td>' + fmtNum(row.width) + '</td>'
                + '<td>' + fmtNum(row.length) + '</td>'
                + '<td>' + fmtNum(row.volume) + '</td>';
            var actions = mode === 'active'
                ? '<button type="button" class="caj__icon-btn caj__icon-btn--view" data-action="view" data-id="' + row.idBox + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="caj__icon-btn caj__icon-btn--edit" data-action="edit" data-id="' + row.idBox + '" title="Editar"><i class="bi bi-pencil"></i></button><button type="button" class="caj__icon-btn caj__icon-btn--delete" data-action="delete" data-id="' + row.idBox + '" title="Eliminar"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="caj__icon-btn caj__icon-btn--restore" data-action="restore" data-id="' + row.idBox + '" title="Restaurar"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="caj__icon-btn caj__icon-btn--purge" data-action="purge" data-id="' + row.idBox + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.idBox + '">' + cells + '<td class="caj__td-actions"><div class="caj__row-actions">' + actions + '</div></td></tr>';
        }).join('');
        tbody.innerHTML = html;
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function updateTableScroll(scrollEl, count) {
        if (!scrollEl) return;
        scrollEl.classList.toggle('is-scrollable', count > 10);
    }

    function loadActiveList() {
        var u = urls();
        var tbody = qs('cajActiveBody');
        var scrollEl = qs('cajTableScroll');
        var loadHtml = '<tr class="caj__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('cajPageInfo'), qs('cajPrevBtn'), qs('cajNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(scrollEl, data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="caj__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('cajInactiveBody');
        var scrollEl = qs('cajInactiveTableScroll');
        var loadHtml = '<tr class="caj__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('cajInactivePageInfo'), qs('cajInactivePrevBtn'), qs('cajInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(scrollEl, data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="caj__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function resetForm() { var form = qs('cajForm'); if (form) form.reset(); qs('cajFormId').value = ''; }

    function openCreateModal() { resetForm(); qs('cajFormModalTitle').textContent = 'Crear caja'; openModal('cajFormModal'); }

    function openEditModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            qs('cajFormModalTitle').textContent = 'Editar caja #' + id;
            qs('cajFormId').value = res.data.idBox;
            qs('cajWeight').value = res.data.weight != null ? res.data.weight : '';
            qs('cajHeight').value = res.data.height != null ? res.data.height : '';
            qs('cajWidth').value = res.data.width != null ? res.data.width : '';
            qs('cajLength').value = res.data.length != null ? res.data.length : '';
            openModal('cajFormModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.idBox },
                { label: 'Peso', value: fmtNum(d.weight) },
                { label: 'Alto', value: fmtNum(d.height) },
                { label: 'Ancho', value: fmtNum(d.width) },
                { label: 'Largo', value: fmtNum(d.length) },
                { label: 'Volumen', value: fmtNum(d.volume) },
                { label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' }
            ];
            if (d.createdAt) rows.push({ label: 'Creado', value: d.createdAt });
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('cajDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="caj-detail__row"><span class="caj-detail__label">' + escapeHtml(r.label) + '</span><span class="caj-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('cajDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var id = qs('cajFormId').value;
        var data = {
            weight: qs('cajWeight').value,
            height: qs('cajHeight').value,
            width: qs('cajWidth').value,
            length: qs('cajLength').value
        };
        var url = id ? urls().update + '?id=' + encodeURIComponent(id) : urls().create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('cajFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('cajConfirmTitle').textContent = title;
        qs('cajConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('cajConfirmModal');
    }

    function handleDeleteLogic(id) {
        confirmAction('Desactivar caja', 'Desea desactivar esta caja? Aparecera en Ver inactivos.', function () {
            postAction(urls().deleteLogic, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadActiveList(); });
        });
    }

    function handleRestore(id) {
        confirmAction('Restaurar caja', 'Desea restaurar esta caja?', function () {
            postAction(urls().restore, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) { loadInactiveList(); loadActiveList(); } });
        });
    }

    function handlePurge(id) {
        confirmAction('Eliminar permanentemente', 'Esta accion no se puede deshacer. Eliminar de la base de datos?', function () {
            postAction(urls().deletePhysical, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadInactiveList(); });
        });
    }

    function bindEvents() {
        var searchInput = qs('cajSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('cajClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('cajPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('cajPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('cajNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('cajCreateBtn')?.addEventListener('click', openCreateModal);
        qs('cajFormSaveBtn')?.addEventListener('click', saveForm);
        qs('cajInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1; state.inactiveSearch = '';
            var inactiveSearch = qs('cajInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            loadInactiveList(); openModal('cajInactiveModal');
        });
        var inactiveSearchInput = qs('cajInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('cajInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('cajInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('cajInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('cajInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('cajConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('cajConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.caj-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('cajActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('cajInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('cajRoot');
        if (!root || root.dataset.initialized === 'true' || !window.cajUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
