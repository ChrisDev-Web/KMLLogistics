(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 3;

    function urls() { return window.regUrls || {}; }
    function getToken() { var input = document.querySelector('input[name=\"__RequestVerificationToken\"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('regToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'reg__toast is-visible ' + (isSuccess ? 'reg__toast--success' : 'reg__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.reg-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class=\"reg__empty-row\"><td colspan=\"' + (colCount + 1) + '\">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '';
            cells = '<td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.countryName) + '</td><td>' + escapeHtml(row.name) + '</td>';
            var actions = mode === 'active'
                ? '<button type=\"button\" class=\"reg__icon-btn reg__icon-btn--view\" data-action=\"view\" data-id=\"' + row.id + '\" title=\"Ver detalle\"><i class=\"bi bi-eye\"></i></button><button type=\"button\" class=\"reg__icon-btn reg__icon-btn--edit\" data-action=\"edit\" data-id=\"' + row.id + '\" title=\"Editar\"><i class=\"bi bi-pencil\"></i></button><button type=\"button\" class=\"reg__icon-btn reg__icon-btn--delete\" data-action=\"delete\" data-id=\"' + row.id + '\" title=\"Eliminar\"><i class=\"bi bi-trash\"></i></button>'
                : '<button type=\"button\" class=\"reg__icon-btn reg__icon-btn--restore\" data-action=\"restore\" data-id=\"' + row.id + '\" title=\"Restaurar\"><i class=\"bi bi-arrow-counterclockwise\"></i></button><button type=\"button\" class=\"reg__icon-btn reg__icon-btn--purge\" data-action=\"purge\" data-id=\"' + row.id + '\" title=\"Eliminar permanentemente\"><i class=\"bi bi-trash-fill\"></i></button>';
            return '<tr data-id=\"' + row.id + '\">' + cells + '<td class=\"reg__td-actions\"><div class=\"reg__row-actions\">' + actions + '</div></td></tr>';
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
        var tbody = qs('regActiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"reg__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('regPageInfo'), qs('regPrevBtn'), qs('regNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('regTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"reg__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('regInactiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"reg__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando...</td></tr>';
        fetchJson(buildQuery(u.listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('regInactivePageInfo'), qs('regInactivePrevBtn'), qs('regInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('regInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"reg__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        });
    }
    function loadFkOptions() {
        return fetchJson(urls().fkOptions).then(function (data) {
            if (!data.success) return;
            var select = qs('regFkSelect');
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

    function resetForm() { var form = qs('regForm'); if (form) form.reset(); qs('regFormId').value = ''; }

    function openCreateModal() { resetForm(); qs('regFormModalTitle').textContent = 'Crear'; loadFkOptions().then(function () { openModal('regFormModal'); }).catch(function (err) { showToast(err.message || 'Error al cargar países.', false); openModal('regFormModal'); }); }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            qs('regFormModalTitle').textContent = 'Editar';
            qs('regFormId').value = res.data.id;
            qs('regName').value = res.data.name || '';

            loadFkOptions().then(function () {
                if (res.data.idCountry !== undefined) qs('regFkSelect').value = res.data.idCountry;
                openModal('regFormModal');
            });
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [{ label: 'ID', value: d.id }, { label: 'Nombre', value: d.name }];
            
            if (d.countryName) rows.push({ label: 'País', value: d.countryName });
            rows.push({ label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' });
            rows.push({ label: 'Creado', value: d.createdAt });
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('regDetailBody').innerHTML = rows.map(function (r) {
                return '<div class=\"reg-detail__row\"><span class=\"reg-detail__label\">' + escapeHtml(r.label) + '</span><span class=\"reg-detail__value\">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('regDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('regForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('regFormId').value;
        var data = { name: qs('regName').value.trim() };
        data.idCountry = qs('regFkSelect').value;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('regFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('regConfirmTitle').textContent = title;
        qs('regConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('regConfirmModal');
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
        var searchInput = qs('regSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('regClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('regPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('regPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('regNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('regCreateBtn')?.addEventListener('click', openCreateModal);
        qs('regFormSaveBtn')?.addEventListener('click', saveForm);
        qs('regInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1; state.inactiveSearch = '';
            var inactiveSearch = qs('regInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            loadInactiveList(); openModal('regInactiveModal');
        });
        var inactiveSearchInput = qs('regInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('regInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('regInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('regInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('regInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('regConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('regConfirmModal'); });
        document.querySelectorAll('[data-dismiss=\"modal\"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.reg-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('regActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('regInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('regRoot');
        if (!root || root.dataset.initialized === 'true' || !window.regUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
