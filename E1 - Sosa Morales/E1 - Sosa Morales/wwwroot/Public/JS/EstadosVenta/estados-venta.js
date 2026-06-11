(function () {
    'use strict';
    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 3;

    function urls() { return window.evntUrls || {}; }
    function getToken() { var i = document.querySelector('input[name="__RequestVerificationToken"]'); return i ? i.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(msg, ok) {
        var t = qs('evntToast'); if (!t) return;
        t.textContent = msg; t.className = 'evnt__toast is-visible ' + (ok ? 'evnt__toast--success' : 'evnt__toast--error');
        setTimeout(function () { t.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.evnt-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (k) { if (params[k] !== undefined && params[k] !== null && params[k] !== '') url.searchParams.set(k, params[k]); });
        return url.toString();
    }

    function fetchJson(url, options) {
        options = options || {}; options.headers = options.headers || {};
        options.headers['X-Requested-With'] = 'XMLHttpRequest'; options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) {
            return r.json().then(function (d) { if (!r.ok) throw new Error(d.message || 'Error'); return d; });
        });
    }

    function postAction(url, data) {
        var token = getToken(); var body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        Object.keys(data).forEach(function (k) { if (data[k] !== undefined && data[k] !== null) body.append(k, data[k]); });
        return fetchJson(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token }, body: body.toString() });
    }

    function escapeHtml(t) { var d = document.createElement('div'); d.textContent = t == null ? '' : String(t); return d.innerHTML; }

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || !items.length) { tbody.innerHTML = '<tr class="evnt__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.name) + '</td><td>' + escapeHtml(row.description || '—') + '</td>';
            var actions = mode === 'active'
                ? '<button type="button" class="evnt__icon-btn evnt__icon-btn--view" data-action="view" data-id="' + row.id + '"><i class="bi bi-eye"></i></button><button type="button" class="evnt__icon-btn evnt__icon-btn--edit" data-action="edit" data-id="' + row.id + '"><i class="bi bi-pencil"></i></button><button type="button" class="evnt__icon-btn evnt__icon-btn--delete" data-action="delete" data-id="' + row.id + '"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="evnt__icon-btn evnt__icon-btn--restore" data-action="restore" data-id="' + row.id + '"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="evnt__icon-btn evnt__icon-btn--purge" data-action="purge" data-id="' + row.id + '"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.id + '">' + cells + '<td class="evnt__td-actions"><div class="evnt__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function loadActiveList() {
        var tbody = qs('evntActiveBody');
        if (tbody) tbody.innerHTML = '<tr class="evnt__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        fetchJson(buildQuery(urls().list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('evntPageInfo'), qs('evntPrevBtn'), qs('evntNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) { showToast(err.message, false); });
    }

    function loadInactiveList() {
        var tbody = qs('evntInactiveBody');
        if (tbody) tbody.innerHTML = '<tr class="evnt__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        fetchJson(buildQuery(urls().listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('evntInactivePageInfo'), qs('evntInactivePrevBtn'), qs('evntInactiveNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) { showToast(err.message, false); });
    }

    function resetForm() { qs('evntFormId').value = ''; qs('evntName').value = ''; qs('evntDescription').value = ''; }

    function openEditModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            resetForm(); qs('evntFormModalTitle').textContent = 'Editar';
            qs('evntFormId').value = res.data.id;
            qs('evntName').value = res.data.name || '';
            qs('evntDescription').value = res.data.description || '';
            openModal('evntFormModal');
        });
    }

    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.id }, { label: 'Nombre', value: d.name },
                { label: 'Descripción', value: d.description || '—' },
                { label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' },
                { label: 'Creado', value: d.createdAt }, { label: 'Actualizado', value: d.updatedAt || '—' }
            ];
            qs('evntDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="evnt-detail__row"><span class="evnt-detail__label">' + escapeHtml(r.label) + '</span><span class="evnt-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('evntDetailModal');
        });
    }

    function saveForm() {
        var id = qs('evntFormId').value;
        var data = { name: qs('evntName').value.trim(), description: qs('evntDescription').value.trim() };
        if (!data.name) { showToast('El nombre es obligatorio.', false); return; }
        var url = id ? urls().update + '?id=' + encodeURIComponent(id) : urls().create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('evntFormModal'); loadActiveList(); }
        });
    }

    function confirmAction(title, message, cb) {
        qs('evntConfirmTitle').textContent = title;
        qs('evntConfirmMessage').textContent = message;
        state.confirmCallback = cb;
        openModal('evntConfirmModal');
    }

    function bindEvents() {
        var searchInput = qs('evntSearchInput'), timer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(timer); timer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('evntClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('evntPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('evntPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('evntNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('evntCreateBtn')?.addEventListener('click', function () { resetForm(); qs('evntFormModalTitle').textContent = 'Crear'; openModal('evntFormModal'); });
        qs('evntFormSaveBtn')?.addEventListener('click', saveForm);
        qs('evntInactiveBtn')?.addEventListener('click', function () { state.inactivePage = 1; state.inactiveSearch = ''; var i = qs('evntInactiveSearchInput'); if (i) i.value = ''; loadInactiveList(); openModal('evntInactiveModal'); });
        var inactiveSearch = qs('evntInactiveSearchInput'), itimer = null;
        if (inactiveSearch) inactiveSearch.addEventListener('input', function () {
            clearTimeout(itimer); itimer = setTimeout(function () { state.inactiveSearch = inactiveSearch.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('evntInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearch) inactiveSearch.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('evntInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('evntInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('evntInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('evntConfirmBtn')?.addEventListener('click', function () { if (state.confirmCallback) state.confirmCallback(); state.confirmCallback = null; closeModal('evntConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (b) { b.addEventListener('click', closeAllModals); });
        qs('evntActiveBody')?.addEventListener('click', function (e) {
            var b = e.target.closest('[data-action]'); if (!b) return;
            var id = parseInt(b.getAttribute('data-id'), 10), act = b.getAttribute('data-action');
            if (act === 'view') openDetailModal(id);
            if (act === 'edit') openEditModal(id);
            if (act === 'delete') confirmAction('Desactivar', '¿Desactivar este estado?', function () {
                postAction(urls().deleteLogic, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) loadActiveList(); });
            });
        });
        qs('evntInactiveBody')?.addEventListener('click', function (e) {
            var b = e.target.closest('[data-action]'); if (!b) return;
            var id = parseInt(b.getAttribute('data-id'), 10), act = b.getAttribute('data-action');
            if (act === 'restore') confirmAction('Restaurar', '¿Restaurar este estado?', function () {
                postAction(urls().restore, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) { loadInactiveList(); loadActiveList(); } });
            });
            if (act === 'purge') confirmAction('Eliminar', '¿Eliminar permanentemente?', function () {
                postAction(urls().deletePhysical, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) loadInactiveList(); });
            });
        });
    }

    function init() {
        var root = qs('evntRoot');
        if (!root || root.dataset.initialized === 'true' || !window.evntUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
