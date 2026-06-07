(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 3;

    function urls() { return window.provUrls || {}; }
    function getToken() { var input = document.querySelector('input[name=\"__RequestVerificationToken\"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('provToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'prov__toast is-visible ' + (isSuccess ? 'prov__toast--success' : 'prov__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.prov-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class=\"prov__empty-row\"><td colspan=\"' + (colCount + 1) + '\">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '';
            cells = '<td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.regionName) + '</td><td>' + escapeHtml(row.name) + '</td>';
            var actions = mode === 'active'
                ? '<button type=\"button\" class=\"prov__icon-btn prov__icon-btn--view\" data-action=\"view\" data-id=\"' + row.id + '\" title=\"Ver detalle\"><i class=\"bi bi-eye\"></i></button><button type=\"button\" class=\"prov__icon-btn prov__icon-btn--edit\" data-action=\"edit\" data-id=\"' + row.id + '\" title=\"Editar\"><i class=\"bi bi-pencil\"></i></button><button type=\"button\" class=\"prov__icon-btn prov__icon-btn--delete\" data-action=\"delete\" data-id=\"' + row.id + '\" title=\"Eliminar\"><i class=\"bi bi-trash\"></i></button>'
                : '<button type=\"button\" class=\"prov__icon-btn prov__icon-btn--restore\" data-action=\"restore\" data-id=\"' + row.id + '\" title=\"Restaurar\"><i class=\"bi bi-arrow-counterclockwise\"></i></button><button type=\"button\" class=\"prov__icon-btn prov__icon-btn--purge\" data-action=\"purge\" data-id=\"' + row.id + '\" title=\"Eliminar permanentemente\"><i class=\"bi bi-trash-fill\"></i></button>';
            return '<tr data-id=\"' + row.id + '\">' + cells + '<td class=\"prov__td-actions\"><div class=\"prov__row-actions\">' + actions + '</div></td></tr>';
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
        var tbody = qs('provActiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"prov__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('provPageInfo'), qs('provPrevBtn'), qs('provNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('provTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"prov__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('provInactiveBody');
        if (tbody) tbody.innerHTML = '<tr class=\"prov__loading-row\"><td colspan=\"' + (colCount + 1) + '\">Cargando...</td></tr>';
        fetchJson(buildQuery(u.listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('provInactivePageInfo'), qs('provInactivePrevBtn'), qs('provInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('provInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class=\"prov__empty-row\"><td colspan=\"' + (colCount + 1) + '\">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        });
    }
    function loadFkOptions() {
        return fetchJson(urls().fkOptions).then(function (data) {
            if (!data.success) return;
            var select = qs('provFkSelect');
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

    function resetForm() { var form = qs('provForm'); if (form) form.reset(); qs('provFormId').value = ''; }

    function openCreateModal() { resetForm(); qs('provFormModalTitle').textContent = 'Crear'; loadFkOptions().then(function () { openModal('provFormModal'); }).catch(function (err) { showToast(err.message || 'Error al cargar regiones.', false); openModal('provFormModal'); }); }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            qs('provFormModalTitle').textContent = 'Editar';
            qs('provFormId').value = res.data.id;
            qs('provName').value = res.data.name || '';

            loadFkOptions().then(function () {
                if (res.data.idRegion !== undefined) qs('provFkSelect').value = res.data.idRegion;
                openModal('provFormModal');
            });
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [{ label: 'ID', value: d.id }, { label: 'Nombre', value: d.name }];
            
            if (d.regionName) rows.push({ label: 'Región', value: d.regionName });
            rows.push({ label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' });
            rows.push({ label: 'Creado', value: d.createdAt });
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('provDetailBody').innerHTML = rows.map(function (r) {
                return '<div class=\"prov-detail__row\"><span class=\"prov-detail__label\">' + escapeHtml(r.label) + '</span><span class=\"prov-detail__value\">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('provDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('provForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('provFormId').value;
        var data = { name: qs('provName').value.trim() };
        data.idRegion = qs('provFkSelect').value;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('provFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('provConfirmTitle').textContent = title;
        qs('provConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('provConfirmModal');
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
        var searchInput = qs('provSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('provClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('provPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('provPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('provNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('provCreateBtn')?.addEventListener('click', openCreateModal);
        qs('provFormSaveBtn')?.addEventListener('click', saveForm);
        qs('provInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1; state.inactiveSearch = '';
            var inactiveSearch = qs('provInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            loadInactiveList(); openModal('provInactiveModal');
        });
        var inactiveSearchInput = qs('provInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('provInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('provInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('provInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('provInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('provConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('provConfirmModal'); });
        document.querySelectorAll('[data-dismiss=\"modal\"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.prov-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('provActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('provInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('provRoot');
        if (!root || root.dataset.initialized === 'true' || !window.provUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
