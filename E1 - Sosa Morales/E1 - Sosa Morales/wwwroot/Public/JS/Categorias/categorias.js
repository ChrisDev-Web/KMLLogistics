(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 3;

    function urls() { return window.catUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('catToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'cat__toast is-visible ' + (isSuccess ? 'cat__toast--success' : 'cat__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.cat-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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
        // Ajustamos cómo se lee el arreglo. Si viene directo o envuelto en items.
        var dataArray = Array.isArray(items) ? items : (items && items.items ? items.items : items);

        if (!dataArray || dataArray.length === 0) { tbody.innerHTML = '<tr class="cat__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }

        var html = dataArray.map(function (row) {
            var cells = '';
            // Aseguramos leer idCategory si id no existe (por si tu C# manda idCategory)
            var id = row.id || row.idCategory;
            cells = '<td>' + escapeHtml(id) + '</td><td>' + escapeHtml(row.name) + '</td><td>' + escapeHtml(row.description || '') + '</td>';
            var actions = mode === 'active'
                ? '<button type="button" class="cat__icon-btn cat__icon-btn--view" data-action="view" data-id="' + id + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="cat__icon-btn cat__icon-btn--edit" data-action="edit" data-id="' + id + '" title="Editar"><i class="bi bi-pencil"></i></button><button type="button" class="cat__icon-btn cat__icon-btn--delete" data-action="delete" data-id="' + id + '" title="Eliminar"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="cat__icon-btn cat__icon-btn--restore" data-action="restore" data-id="' + id + '" title="Restaurar"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="cat__icon-btn cat__icon-btn--purge" data-action="purge" data-id="' + id + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + id + '">' + cells + '<td class="cat__td-actions"><div class="cat__row-actions">' + actions + '</div></td></tr>';
        }).join('');
        tbody.innerHTML = html;
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Página ' + (page || 1) + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = (page || 1) <= 1;
        if (nextBtn) nextBtn.disabled = (page || 1) >= (totalPages || 1) || (totalPages || 0) === 0;
    }

    function updateTableScroll(scrollEl, count) {
        if (!scrollEl) return;
        scrollEl.classList.toggle('is-scrollable', count > 10);
    }

    function loadActiveList() {
        var u = urls();
        var tbody = qs('catActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="cat__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';

        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;

        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {

            // Ahora lee directamente items porque el controlador ya lo manda así
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('catPageInfo'), qs('catPrevBtn'), qs('catNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('catTableScroll'), data.items ? data.items.length : 0);

        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="cat__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('catInactiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="cat__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;

        fetchJson(buildQuery(u.listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            var items = data.data || data.items || data;
            renderRows(tbody, items, 'inactive');
            updatePagination(qs('catInactivePageInfo'), qs('catInactivePrevBtn'), qs('catInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('catInactiveTableScroll'), items ? items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="cat__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function resetForm() { var form = qs('catForm'); if (form) form.reset(); qs('catFormId').value = ''; }

    function openCreateModal() { resetForm(); qs('catFormModalTitle').textContent = 'Crear'; openModal('catFormModal'); }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            qs('catFormModalTitle').textContent = 'Editar';
            qs('catFormId').value = res.data.id || res.data.idCategory;
            qs('catName').value = res.data.name || '';
            qs('catDescription').value = res.data.description || '';
            openModal('catFormModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var realId = d.id || d.idCategory;
            var rows = [{ label: 'ID', value: realId }, { label: 'Nombre', value: d.name }];
            if (d.description !== undefined && d.description !== '') rows.push({ label: 'Descripción', value: d.description });

            rows.push({ label: 'Estado', value: (d.status === 1 || d.status === undefined) ? 'Activo' : 'Inactivo' });
            if (d.createdAt) rows.push({ label: 'Creado', value: d.createdAt });
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });

            qs('catDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="cat-detail__row"><span class="cat-detail__label">' + escapeHtml(r.label) + '</span><span class="cat-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('catDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('catForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('catFormId').value;
        var data = { name: qs('catName').value.trim() };
        data.description = qs('catDescription').value.trim();

        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('catFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('catConfirmTitle').textContent = title;
        qs('catConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('catConfirmModal');
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
            postAction(urls().deletePhysical, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) { loadInactiveList(); loadActiveList(); } });
        });
    }

    function bindEvents() {
        var searchInput = qs('catSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });

        qs('catClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('catPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('catPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('catNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('catCreateBtn')?.addEventListener('click', openCreateModal);
        qs('catFormSaveBtn')?.addEventListener('click', saveForm);

        qs('catInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1; state.inactiveSearch = '';
            var inactiveSearch = qs('catInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            loadInactiveList(); openModal('catInactiveModal');
        });

        var inactiveSearchInput = qs('catInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });

        qs('catInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('catInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('catInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('catInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('catConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('catConfirmModal'); });

        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.cat-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });

        qs('catActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });

        qs('catInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('catRoot');
        if (!root || root.dataset.initialized === 'true' || !window.catUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();