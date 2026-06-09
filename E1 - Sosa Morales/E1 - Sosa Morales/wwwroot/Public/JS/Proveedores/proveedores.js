(function () {
    'use strict';

    var state = {
        page: 1,
        pageSize: 10,
        search: '',
        idDocumentType: '',
        idDistrict: '',
        inactivePage: 1,
        inactivePageSize: 10,
        inactiveSearch: '',
        inactiveIdDocumentType: '',
        inactiveIdDistrict: '',
        confirmCallback: null,
        filterOptionsLoaded: false
    };
    var colCount = 7;

    function urls() { return window.supUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('supToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'sup__toast is-visible ' + (isSuccess ? 'sup__toast--success' : 'sup__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.sup-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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

    function populateFilterSelect(selectId, options) {
        var select = qs(selectId);
        if (!select) return;
        var current = select.value;
        select.innerHTML = '<option value="">Todos</option>';
        (options || []).forEach(function (opt) {
            var option = document.createElement('option');
            option.value = opt.id;
            option.textContent = opt.name;
            select.appendChild(option);
        });
        if (current) select.value = current;
    }

    function populateFormSelect(selectId, options, placeholder) {
        var select = qs(selectId);
        if (!select) return;
        var current = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>';
        (options || []).forEach(function (opt) {
            var option = document.createElement('option');
            option.value = opt.id;
            option.textContent = opt.name;
            select.appendChild(option);
        });
        if (current) select.value = current;
    }

    function loadFilterOptions() {
        return fetchJson(urls().filterOptions).then(function (data) {
            if (!data.success) throw new Error(data.message || 'Error al cargar opciones.');
            var docTypes = data.documentTypes || [];
            var districts = data.districts || [];
            populateFilterSelect('supFilterDocType', docTypes);
            populateFilterSelect('supFilterDistrict', districts);
            populateFilterSelect('supInactiveFilterDocType', docTypes);
            populateFilterSelect('supInactiveFilterDistrict', districts);
            populateFormSelect('supDocType', docTypes, 'Seleccione...');
            populateFormSelect('supDistrict', districts, 'Sin distrito');
            state.filterOptionsLoaded = true;
        });
    }

    function ensureFilterOptions() {
        if (state.filterOptionsLoaded) return Promise.resolve();
        return loadFilterOptions();
    }

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="sup__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.id) + '</td>'
                + '<td>' + escapeHtml(row.documentTypeName || '') + '</td>'
                + '<td>' + escapeHtml(row.documentNumber || '') + '</td>'
                + '<td>' + escapeHtml(row.name || '') + '</td>'
                + '<td>' + escapeHtml(row.phone || '') + '</td>'
                + '<td>' + escapeHtml(row.email || '') + '</td>'
                + '<td>' + escapeHtml(row.districtName || '') + '</td>';
            var actions = mode === 'active'
                ? '<button type="button" class="sup__icon-btn sup__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="sup__icon-btn sup__icon-btn--edit" data-action="edit" data-id="' + row.id + '" title="Editar"><i class="bi bi-pencil"></i></button><button type="button" class="sup__icon-btn sup__icon-btn--delete" data-action="delete" data-id="' + row.id + '" title="Eliminar"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="sup__icon-btn sup__icon-btn--restore" data-action="restore" data-id="' + row.id + '" title="Restaurar"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="sup__icon-btn sup__icon-btn--purge" data-action="purge" data-id="' + row.id + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.id + '">' + cells + '<td class="sup__td-actions"><div class="sup__row-actions">' + actions + '</div></td></tr>';
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
        var tbody = qs('supActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="sup__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.list, {
            search: state.search,
            idDocumentType: state.idDocumentType,
            idDistrict: state.idDistrict,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('supPageInfo'), qs('supPrevBtn'), qs('supNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('supTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="sup__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('supInactiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="sup__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.listInactive, {
            search: state.inactiveSearch,
            idDocumentType: state.inactiveIdDocumentType,
            idDistrict: state.inactiveIdDistrict,
            page: state.inactivePage,
            pageSize: state.inactivePageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('supInactivePageInfo'), qs('supInactivePrevBtn'), qs('supInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('supInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="sup__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function resetForm() { var form = qs('supForm'); if (form) form.reset(); qs('supFormId').value = ''; }

    function openCreateModal() {
        resetForm();
        qs('supFormModalTitle').textContent = 'Crear';
        ensureFilterOptions().then(function () { openModal('supFormModal'); }).catch(function (err) {
            showToast(err.message || 'Error al cargar opciones.', false);
            openModal('supFormModal');
        });
    }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            ensureFilterOptions().then(function () {
                qs('supFormModalTitle').textContent = 'Editar';
                qs('supFormId').value = res.data.id;
                qs('supDocType').value = res.data.idDocumentType || '';
                qs('supDocNumber').value = res.data.documentNumber || '';
                qs('supName').value = res.data.name || '';
                qs('supPhone').value = res.data.phone || '';
                qs('supEmail').value = res.data.email || '';
                qs('supAddress').value = res.data.address || '';
                qs('supDistrict').value = res.data.idDistrict != null ? res.data.idDistrict : '';
                openModal('supFormModal');
            });
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.id },
                { label: 'Tipo documento', value: d.documentTypeName },
                { label: 'N° documento', value: d.documentNumber },
                { label: 'Razón social', value: d.name },
                { label: 'Teléfono', value: d.phone || '—' },
                { label: 'Email', value: d.email || '—' },
                { label: 'Dirección', value: d.address || '—' },
                { label: 'País', value: d.countryName || '—' },
                { label: 'Región', value: d.regionName || '—' },
                { label: 'Provincia', value: d.provinceName || '—' },
                { label: 'Distrito', value: d.districtName || '—' },
                { label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' },
                { label: 'Creado', value: d.createdAt },
                { label: 'Actualizado', value: d.updatedAt || '—' }
            ];
            qs('supDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="sup-detail__row"><span class="sup-detail__label">' + escapeHtml(r.label) + '</span><span class="sup-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('supDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('supForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('supFormId').value;
        var data = {
            idDocumentType: qs('supDocType').value,
            documentNumber: qs('supDocNumber').value.trim(),
            name: qs('supName').value.trim(),
            phone: qs('supPhone').value.trim(),
            email: qs('supEmail').value.trim(),
            address: qs('supAddress').value.trim()
        };
        var districtVal = qs('supDistrict').value;
        if (districtVal) data.idDistrict = districtVal;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('supFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('supConfirmTitle').textContent = title;
        qs('supConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('supConfirmModal');
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
        var searchInput = qs('supSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('supClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('supFilterDocType')?.addEventListener('change', function (e) { state.idDocumentType = e.target.value; state.page = 1; loadActiveList(); });
        qs('supFilterDistrict')?.addEventListener('change', function (e) { state.idDistrict = e.target.value; state.page = 1; loadActiveList(); });
        qs('supPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('supPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('supNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('supCreateBtn')?.addEventListener('click', openCreateModal);
        qs('supFormSaveBtn')?.addEventListener('click', saveForm);
        qs('supInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1;
            state.inactiveSearch = '';
            state.inactiveIdDocumentType = '';
            state.inactiveIdDistrict = '';
            var inactiveSearch = qs('supInactiveSearchInput');
            if (inactiveSearch) inactiveSearch.value = '';
            if (qs('supInactiveFilterDocType')) qs('supInactiveFilterDocType').value = '';
            if (qs('supInactiveFilterDistrict')) qs('supInactiveFilterDistrict').value = '';
            loadInactiveList();
            openModal('supInactiveModal');
        });
        var inactiveSearchInput = qs('supInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('supInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('supInactiveFilterDocType')?.addEventListener('change', function (e) { state.inactiveIdDocumentType = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('supInactiveFilterDistrict')?.addEventListener('change', function (e) { state.inactiveIdDistrict = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('supInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('supInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('supInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('supConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('supConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.sup-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('supActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('supInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('supRoot');
        if (!root || root.dataset.initialized === 'true' || !window.supUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1;
        state.pageSize = 10;
        state.search = '';
        state.idDocumentType = '';
        state.idDistrict = '';
        state.inactivePage = 1;
        state.inactivePageSize = 10;
        state.inactiveSearch = '';
        state.inactiveIdDocumentType = '';
        state.inactiveIdDistrict = '';
        state.filterOptionsLoaded = false;
        bindEvents();
        loadFilterOptions().then(function () { loadActiveList(); }).catch(function (err) {
            showToast(err.message || 'Error al cargar filtros.', false);
            loadActiveList();
        });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
