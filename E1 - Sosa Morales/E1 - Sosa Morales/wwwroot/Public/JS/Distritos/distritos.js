(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 3;

    function urls() { return window.distUrls || {}; }
    function getToken() { var input = document.querySelector('input[name=\"__RequestVerificationToken\"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('distToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'dist__toast is-visible ' + (isSuccess ? 'dist__toast--success' : 'dist__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.dist-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class=\"dist__empty-row\"><td colspan=\"' + (colCount + 1) + '\">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '';
            cells = '<td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.provinceName) + '</td><td>' + escapeHtml(row.name) + '</td>';
            var actions = mode === 'active'
                ? '<button type=\"button\" class=\"dist__icon-btn dist__icon-btn--view\" data-action=\"view\" data-id=\"' + row.id + '\" title=\"Ver detalle\"><i class=\"bi bi-eye\"></i></button><button type=\"button\" class=\"dist__icon-btn dist__icon-btn--edit\" data-action=\"edit\" data-id=\"' + row.id + '\" title=\"Editar\"><i class=\"bi bi-pencil\"></i></button><button type=\"button\" class=\"dist__icon-btn dist__icon-btn--delete\" data-action=\"delete\" data-id=\"' + row.id + '\" title=\"Eliminar\"><i class=\"bi bi-trash\"></i></button>'
                : '<button type=\"button\" class=\"dist__icon-btn dist__icon-btn--restore\" data-action=\"restore\" data-id=\"' + row.id + '\" title=\"Restaurar\"><i class=\"bi bi-arrow-counterclockwise\"></i></button><button type=\"button\" class=\"dist__icon-btn dist__icon-btn--purge\" data-action=\"purge\" data-id=\"' + row.id + '\" title=\"Eliminar permanentemente\"><i class=\"bi bi-trash-fill\"></i></button>';
            return '<tr data-id=\"' + row.id + '\">' + cells + '<td class=\"dist__td-actions\"><div class=\"dist__row-actions\">' + actions + '</div></td></tr>';
        }).join('');
        tbody.innerHTML = html;
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Página ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function updateTableScroll(scrollEl, count) {
        if (!scrollEl) return;
        scrollEl.classList.toggle('is-scrollable', count > 10);
    }

    function loadActiveList() {
        var u = urls();
        var tbody = qs('distActiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"dist__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('distPageInfo'), qs('distPrevBtn'), qs('distNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('distTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"dist__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('distInactiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"dist__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando...</td></tr>';
        fetchJson(buildQuery(u.listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('distInactivePageInfo'), qs('distInactivePrevBtn'), qs('distInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('distInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"dist__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        });
    }
    function loadFkOptions() {
        return fetchJson(urls().fkOptions).then(function (data) {
            if (!data.success) return;
            var select = qs('distFkSelect');
            if (!select) return;
            var current = select.value;
            select.innerHTML = '<option value="">Seleccione...</option>';
            (data.items || []).forEach(function (opt) {
                var option = document.createElement('option');
                option.value = opt.id;
                option.textContent = opt.name;
                select.appendChild(option);
            });
            if (current) select.value = current;
        });
    }

    function resetForm() { var form = qs('distForm'); if (form) form.reset(); qs('distFormId').value = ''; }

    function openCreateModal() { resetForm(); qs('distFormModalTitle').textContent = 'Crear'; loadFkOptions().then(function () { openModal('distFormModal'); }).catch(function (err) { showToast(err.message || 'Error al cargar provincias.', false); openModal('distFormModal'); }); }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            qs('distFormModalTitle').textContent = 'Editar';
            qs('distFormId').value = res.data.id;
            qs('distName').value = res.data.name || '';

            loadFkOptions().then(function () {
                if (res.data.idProvince !== undefined) qs('distFkSelect').value = res.data.idProvince;
                openModal('distFormModal');
            });
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [{ label: 'ID', value: d.id }, { label: 'Nombre', value: d.name }];
            
            if (d.provinceName) rows.push({ label: 'Provincia', value: d.provinceName });
            rows.push({ label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' });
            rows.push({ label: 'Creado', value: d.createdAt });
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('distDetailBody').innerHTML = rows.map(function (r) {
                return '<div class=\"dist-detail__row\"><span class=\"dist-detail__label\">' + escapeHtml(r.label) + '</span><span class=\"dist-detail__value\">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('distDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('distForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('distFormId').value;
        var data = { name: qs('distName').value.trim() };
        data.idProvince = qs('distFkSelect').value;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('distFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('distConfirmTitle').textContent = title;
        qs('distConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('distConfirmModal');
    }

    function handleDeleteLogic(id) {
        confirmAction('Desactivar registro', '¿Desea desactivar este registro? Aparecerá en Ver inactivos.', function () {
            postAction(urls().deleteLogic, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadActiveList(); });
        });
    }

    function handleRestore(id) {
        confirmAction('Restaurar registro', '¿Desea restaurar este registro?', function () {
            postAction(urls().restore, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) { loadInactiveList(); loadActiveList(); } });
        });
    }

    function handlePurge(id) {
        confirmAction('Eliminar permanentemente', 'Esta acción no se puede deshacer. ¿Eliminar de la base de datos?', function () {
            postAction(urls().deletePhysical, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadInactiveList(); });
        });
    }

    function bindEvents() {
        var searchInput = qs('distSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('distClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('distPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('distPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('distNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('distCreateBtn')?.addEventListener('click', openCreateModal);
        qs('distFormSaveBtn')?.addEventListener('click', saveForm);
        qs('distInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1; state.inactiveSearch = '';
            var inactiveSearch = qs('distInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            loadInactiveList(); openModal('distInactiveModal');
        });
        var inactiveSearchInput = qs('distInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('distInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('distInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('distInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('distInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('distConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('distConfirmModal'); });
        document.querySelectorAll('[data-dismiss=\"modal\"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.dist-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('distActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('distInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('distRoot');
        if (!root || root.dataset.initialized === 'true' || !window.distUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
