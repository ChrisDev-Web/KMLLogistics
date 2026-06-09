(function () {
    'use strict';

    var state = {
        page: 1,
        pageSize: 10,
        search: '',
        idDocumentType: '',
        idDistrict: '',
        idJobPosition: '',
        inactivePage: 1,
        inactivePageSize: 10,
        inactiveSearch: '',
        inactiveIdDocumentType: '',
        inactiveIdDistrict: '',
        inactiveIdJobPosition: '',
        confirmCallback: null,
        filterOptionsLoaded: false
    };
    var colCount = 6;
    var inactiveColCount = 4;

    function urls() { return window.empUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('empToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'emp__toast is-visible ' + (isSuccess ? 'emp__toast--success' : 'emp__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }

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
            var jobPositions = data.jobPositions || [];
            populateFilterSelect('empFilterDocType', docTypes);
            populateFilterSelect('empFilterDistrict', districts);
            populateFilterSelect('empFilterJobPosition', jobPositions);
            populateFilterSelect('empInactiveFilterDocType', docTypes);
            populateFilterSelect('empInactiveFilterDistrict', districts);
            populateFilterSelect('empInactiveFilterJobPosition', jobPositions);
            populateFormSelect('empDocType', docTypes, 'Seleccione...');
            populateFormSelect('empDistrict', districts, 'Sin distrito');
            populateFormSelect('empJobPosition', jobPositions, 'Seleccione...');
            state.filterOptionsLoaded = true;
        });
    }

    function loadUserOptions(excludeEmployeeId) {
        var params = {};
        if (excludeEmployeeId) params.excludeEmployeeId = excludeEmployeeId;
        return fetchJson(buildQuery(urls().userOptions, params)).then(function (data) {
            if (!data.success) throw new Error(data.message || 'Error al cargar usuarios.');
            populateFormSelect('empUser', data.users || [], 'Seleccione...');
        });
    }

    function ensureFilterOptions() {
        if (state.filterOptionsLoaded) return Promise.resolve();
        return loadFilterOptions();
    }

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        var cols = mode === 'inactive' ? inactiveColCount : colCount;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="emp__empty-row"><td colspan="' + (cols + 1) + '">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.id) + '</td>'
                + '<td>' + escapeHtml(row.userName || '') + '</td>';
            if (mode === 'active') {
                cells += '<td>' + escapeHtml(row.documentTypeName || '') + '</td>';
            }
            cells += '<td>' + escapeHtml(row.fullName || '') + '</td>'
                + '<td>' + escapeHtml(row.jobPositionName || '') + '</td>';
            if (mode === 'active') {
                cells += '<td>' + escapeHtml(row.districtName || '') + '</td>';
            }
            var actions = mode === 'active'
                ? '<button type="button" class="emp__icon-btn emp__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="emp__icon-btn emp__icon-btn--edit" data-action="edit" data-id="' + row.id + '" title="Editar"><i class="bi bi-pencil"></i></button><button type="button" class="emp__icon-btn emp__icon-btn--delete" data-action="delete" data-id="' + row.id + '" title="Eliminar"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="emp__icon-btn emp__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="emp__icon-btn emp__icon-btn--restore" data-action="restore" data-id="' + row.id + '" title="Restaurar"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="emp__icon-btn emp__icon-btn--purge" data-action="purge" data-id="' + row.id + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.id + '">' + cells + '<td class="emp__td-actions"><div class="emp__row-actions">' + actions + '</div></td></tr>';
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
        var tbody = qs('empActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="emp__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.list, {
            search: state.search,
            idDocumentType: state.idDocumentType,
            idDistrict: state.idDistrict,
            idJobPosition: state.idJobPosition,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('empPageInfo'), qs('empPrevBtn'), qs('empNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('empTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="emp__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('empInactiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="emp__loading-row"><td colspan="' + (inactiveColCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(u.listInactive, {
            search: state.inactiveSearch,
            idDocumentType: state.inactiveIdDocumentType,
            idDistrict: state.inactiveIdDistrict,
            idJobPosition: state.inactiveIdJobPosition,
            page: state.inactivePage,
            pageSize: state.inactivePageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('empInactivePageInfo'), qs('empInactivePrevBtn'), qs('empInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('empInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="emp__empty-row"><td colspan="' + (inactiveColCount + 1) + '">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }

    function resetForm() { var form = qs('empForm'); if (form) form.reset(); qs('empFormId').value = ''; }

    function openCreateModal() {
        resetForm();
        qs('empFormModalTitle').textContent = 'Crear';
        Promise.all([ensureFilterOptions(), loadUserOptions(null)]).then(function () {
            openModal('empFormModal');
        }).catch(function (err) {
            showToast(err.message || 'Error al cargar opciones.', false);
            openModal('empFormModal');
        });
    }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            Promise.all([ensureFilterOptions(), loadUserOptions(id)]).then(function () {
                var d = res.data;
                qs('empFormModalTitle').textContent = 'Editar';
                qs('empFormId').value = d.id;
                qs('empUser').value = d.idUser || '';
                qs('empDocType').value = d.idDocumentType || '';
                qs('empDocNumber').value = d.documentNumber || '';
                qs('empName').value = d.name || '';
                qs('empLastNamePaternal').value = d.lastNamePaternal || '';
                qs('empLastNameMaternal').value = d.lastNameMaternal || '';
                qs('empJobPosition').value = d.idJobPosition || '';
                qs('empPhone').value = d.phone || '';
                qs('empEmail').value = d.email || '';
                qs('empDistrict').value = d.idDistrict != null ? String(d.idDistrict) : '';
                openModal('empFormModal');
            }).catch(function (err) { showToast(err.message || 'Error al cargar opciones.', false); });
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function openDetailModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            var d = res.data;
            var rows = [
                { label: 'ID', value: d.id },
                { label: 'Usuario', value: d.username },
                { label: 'Rol', value: d.roleName || '' },
                { label: 'Cargo', value: d.jobPositionName },
                { label: 'Tipo documento', value: d.documentTypeName },
                { label: 'N\u00b0 documento', value: d.documentNumber },
                { label: 'Nombres', value: d.name },
                { label: 'Apellido paterno', value: d.lastNamePaternal },
                { label: 'Apellido materno', value: d.lastNameMaternal || '' },
                { label: 'Tel\u00e9fono', value: d.phone || '' },
                { label: 'Email', value: d.email || '' },
                { label: 'Pa\u00eds', value: d.countryName || '-' },
                { label: 'Regi\u00f3n', value: d.regionName || '-' },
                { label: 'Provincia', value: d.provinceName || '-' },
                { label: 'Distrito', value: d.districtName || '-' },
                { label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' },
                { label: 'Creado', value: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('empDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="emp-detail__row"><span class="emp-detail__label">' + escapeHtml(r.label) + '</span><span class="emp-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('empDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('empForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('empFormId').value;
        var data = {
            idUser: qs('empUser').value,
            idJobPosition: qs('empJobPosition').value,
            idDocumentType: qs('empDocType').value,
            documentNumber: qs('empDocNumber').value.trim(),
            name: qs('empName').value.trim(),
            lastNamePaternal: qs('empLastNamePaternal').value.trim(),
            lastNameMaternal: qs('empLastNameMaternal').value.trim(),
            phone: qs('empPhone').value.trim(),
            email: qs('empEmail').value.trim()
        };
        var districtVal = qs('empDistrict').value;
        if (districtVal) data.idDistrict = districtVal;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('empFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('empConfirmTitle').textContent = title;
        qs('empConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('empConfirmModal');
    }

    function handleDeleteLogic(id) {
        confirmAction('Desactivar registro', '\u00bfDesea desactivar este registro? Aparecer\u00e1 en Ver inactivos.', function () {
            postAction(urls().deleteLogic, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadActiveList(); });
        });
    }

    function handleRestore(id) {
        confirmAction('Restaurar registro', '\u00bfDesea restaurar este registro?', function () {
            postAction(urls().restore, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) { loadInactiveList(); loadActiveList(); } });
        });
    }

    function handlePurge(id) {
        confirmAction('Eliminar permanentemente', 'Esta acci\u00f3n no se puede deshacer. \u00bfEliminar de la base de datos?', function () {
            postAction(urls().deletePhysical, { id: id }).then(function (res) { showToast(res.message, res.success); if (res.success) loadInactiveList(); });
        });
    }

    function bindEvents() {
        var searchInput = qs('empSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('empClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('empFilterDocType')?.addEventListener('change', function (e) { state.idDocumentType = e.target.value; state.page = 1; loadActiveList(); });
        qs('empFilterDistrict')?.addEventListener('change', function (e) { state.idDistrict = e.target.value; state.page = 1; loadActiveList(); });
        qs('empFilterJobPosition')?.addEventListener('change', function (e) { state.idJobPosition = e.target.value; state.page = 1; loadActiveList(); });
        qs('empPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('empPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('empNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('empCreateBtn')?.addEventListener('click', openCreateModal);
        qs('empFormSaveBtn')?.addEventListener('click', saveForm);
        qs('empInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1;
            state.inactiveSearch = '';
            state.inactiveIdDocumentType = '';
            state.inactiveIdDistrict = '';
            state.inactiveIdJobPosition = '';
            var inactiveSearch = qs('empInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            var inactiveDocType = qs('empInactiveFilterDocType'); if (inactiveDocType) inactiveDocType.value = '';
            var inactiveDistrict = qs('empInactiveFilterDistrict'); if (inactiveDistrict) inactiveDistrict.value = '';
            var inactiveJob = qs('empInactiveFilterJobPosition'); if (inactiveJob) inactiveJob.value = '';
            loadInactiveList();
            openModal('empInactiveModal');
        });
        var inactiveSearchInput = qs('empInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('empInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('empInactiveFilterDocType')?.addEventListener('change', function (e) { state.inactiveIdDocumentType = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('empInactiveFilterDistrict')?.addEventListener('change', function (e) { state.inactiveIdDistrict = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('empInactiveFilterJobPosition')?.addEventListener('change', function (e) { state.inactiveIdJobPosition = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('empInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('empInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('empInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('empConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('empConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var modal = btn.closest('.emp-modal');
                if (modal) closeModal(modal.id);
            });
        });
        document.querySelectorAll('.emp-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', function () {
                var modal = backdrop.closest('.emp-modal');
                if (modal) closeModal(modal.id);
            });
        });
        qs('empActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('empInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id);
            else if (action === 'restore') handleRestore(id);
            else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('empRoot');
        if (!root || root.dataset.initialized === 'true' || !window.empUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        state.idDocumentType = ''; state.idDistrict = ''; state.idJobPosition = '';
        state.inactivePage = 1; state.inactivePageSize = 10; state.inactiveSearch = '';
        state.inactiveIdDocumentType = ''; state.inactiveIdDistrict = ''; state.inactiveIdJobPosition = '';
        bindEvents();
        loadFilterOptions().then(function () { loadActiveList(); }).catch(function (err) {
            showToast(err.message || 'Error al cargar opciones de filtro.', false);
            loadActiveList();
        });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
