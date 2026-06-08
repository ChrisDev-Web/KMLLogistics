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

    function urls() { return window.cltUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('cltToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'clt__toast is-visible ' + (isSuccess ? 'clt__toast--success' : 'clt__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.clt-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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
            populateFilterSelect('cltFilterDocType', docTypes);
            populateFilterSelect('cltFilterDistrict', districts);
            populateFilterSelect('cltInactiveFilterDocType', docTypes);
            populateFilterSelect('cltInactiveFilterDistrict', districts);
            populateFormSelect('cltDocType', docTypes, 'Seleccione...');
            populateFormSelect('cltDistrict', districts, 'Sin distrito');
            state.filterOptionsLoaded = true;
        });
    }

    function ensureFilterOptions() {
        if (state.filterOptionsLoaded) return Promise.resolve();
        return loadFilterOptions();
    }

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="clt__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.id) + '</td>'
                + '<td>' + escapeHtml(row.documentTypeName || '') + '</td>'
                + '<td>' + escapeHtml(row.documentNumber || '') + '</td>'
                + '<td>' + escapeHtml(row.fullName || '') + '</td>'
                + '<td>' + escapeHtml(row.phone || '') + '</td>'
                + '<td>' + escapeHtml(row.email || '') + '</td>'
                + '<td>' + escapeHtml(row.districtName || '') + '</td>';
            var actions = mode === 'active'
                ? '<button type="button" class="clt__icon-btn clt__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button><button type="button" class="clt__icon-btn clt__icon-btn--edit" data-action="edit" data-id="' + row.id + '" title="Editar"><i class="bi bi-pencil"></i></button><button type="button" class="clt__icon-btn clt__icon-btn--delete" data-action="delete" data-id="' + row.id + '" title="Eliminar"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="clt__icon-btn clt__icon-btn--restore" data-action="restore" data-id="' + row.id + '" title="Restaurar"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="clt__icon-btn clt__icon-btn--purge" data-action="purge" data-id="' + row.id + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.id + '">' + cells + '<td class="clt__td-actions"><div class="clt__row-actions">' + actions + '</div></td></tr>';
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
        var tbody = qs('cltActiveBody');
        if (tbody) tbody.innerHTML = '<tr class="clt__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, {
            search: state.search,
            idDocumentType: state.idDocumentType,
            idDistrict: state.idDistrict,
            page: state.page,
            pageSize: state.pageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('cltPageInfo'), qs('cltPrevBtn'), qs('cltNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('cltTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="clt__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        });
    }

    function loadInactiveList() {
        var u = urls();
        var tbody = qs('cltInactiveBody');
        if (tbody) tbody.innerHTML = '<tr class="clt__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        fetchJson(buildQuery(u.listInactive, {
            search: state.inactiveSearch,
            idDocumentType: state.inactiveIdDocumentType,
            idDistrict: state.inactiveIdDistrict,
            page: state.inactivePage,
            pageSize: state.inactivePageSize
        })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('cltInactivePageInfo'), qs('cltInactivePrevBtn'), qs('cltInactiveNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('cltInactiveTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="clt__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar inactivos.</td></tr>';
            showToast(err.message || 'Error al cargar inactivos.', false);
        });
    }

    function resetForm() { var form = qs('cltForm'); if (form) form.reset(); qs('cltFormId').value = ''; }

    function openCreateModal() {
        resetForm();
        qs('cltFormModalTitle').textContent = 'Crear';
        ensureFilterOptions().then(function () { openModal('cltFormModal'); }).catch(function (err) {
            showToast(err.message || 'Error al cargar opciones.', false);
            openModal('cltFormModal');
        });
    }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            ensureFilterOptions().then(function () {
                var d = res.data;
                qs('cltFormModalTitle').textContent = 'Editar';
                qs('cltFormId').value = d.id;
                qs('cltDocType').value = d.idDocumentType || '';
                qs('cltDocNumber').value = d.documentNumber || '';
                qs('cltName').value = d.name || '';
                qs('cltLastNamePaternal').value = d.lastNamePaternal || '';
                qs('cltLastNameMaternal').value = d.lastNameMaternal || '';
                qs('cltPhone').value = d.phone || '';
                qs('cltEmail').value = d.email || '';
                qs('cltAddress').value = d.address || '';
                qs('cltDistrict').value = d.idDistrict != null ? String(d.idDistrict) : '';
                openModal('cltFormModal');
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
                { label: 'Tipo documento', value: d.documentTypeName },
                { label: 'N° documento', value: d.documentNumber },
                { label: 'Nombres', value: d.name },
                { label: 'Apellido paterno', value: d.lastNamePaternal },
                { label: 'Apellido materno', value: d.lastNameMaternal || '' },
                { label: 'Teléfono', value: d.phone || '' },
                { label: 'Email', value: d.email || '' },
                { label: 'Dirección', value: d.address || '' },
                { label: 'País', value: d.countryName || '—' },
                { label: 'Región', value: d.regionName || '—' },
                { label: 'Provincia', value: d.provinceName || '—' },
                { label: 'Distrito', value: d.districtName || '—' },
                { label: 'Estado', value: d.status === 1 ? 'Activo' : 'Inactivo' },
                { label: 'Creado', value: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('cltDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="clt-detail__row"><span class="clt-detail__label">' + escapeHtml(r.label) + '</span><span class="clt-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('cltDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('cltForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('cltFormId').value;
        var data = {
            idDocumentType: qs('cltDocType').value,
            documentNumber: qs('cltDocNumber').value.trim(),
            name: qs('cltName').value.trim(),
            lastNamePaternal: qs('cltLastNamePaternal').value.trim(),
            lastNameMaternal: qs('cltLastNameMaternal').value.trim(),
            phone: qs('cltPhone').value.trim(),
            email: qs('cltEmail').value.trim(),
            address: qs('cltAddress').value.trim()
        };
        var districtVal = qs('cltDistrict').value;
        if (districtVal) data.idDistrict = districtVal;
        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('cltFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('cltConfirmTitle').textContent = title;
        qs('cltConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('cltConfirmModal');
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
        var searchInput = qs('cltSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('cltClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('cltFilterDocType')?.addEventListener('change', function (e) { state.idDocumentType = e.target.value; state.page = 1; loadActiveList(); });
        qs('cltFilterDistrict')?.addEventListener('change', function (e) { state.idDistrict = e.target.value; state.page = 1; loadActiveList(); });
        qs('cltPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('cltPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('cltNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('cltCreateBtn')?.addEventListener('click', openCreateModal);
        qs('cltFormSaveBtn')?.addEventListener('click', saveForm);
        qs('cltInactiveBtn')?.addEventListener('click', function () {
            state.inactivePage = 1;
            state.inactiveSearch = '';
            state.inactiveIdDocumentType = '';
            state.inactiveIdDistrict = '';
            var inactiveSearch = qs('cltInactiveSearchInput'); if (inactiveSearch) inactiveSearch.value = '';
            var inactiveDocType = qs('cltInactiveFilterDocType'); if (inactiveDocType) inactiveDocType.value = '';
            var inactiveDistrict = qs('cltInactiveFilterDistrict'); if (inactiveDistrict) inactiveDistrict.value = '';
            loadInactiveList();
            openModal('cltInactiveModal');
        });
        var inactiveSearchInput = qs('cltInactiveSearchInput');
        var inactiveTimer = null;
        if (inactiveSearchInput) inactiveSearchInput.addEventListener('input', function () {
            clearTimeout(inactiveTimer);
            inactiveTimer = setTimeout(function () { state.inactiveSearch = inactiveSearchInput.value.trim(); state.inactivePage = 1; loadInactiveList(); }, 350);
        });
        qs('cltInactiveClearBtn')?.addEventListener('click', function () { state.inactiveSearch = ''; if (inactiveSearchInput) inactiveSearchInput.value = ''; state.inactivePage = 1; loadInactiveList(); });
        qs('cltInactiveFilterDocType')?.addEventListener('change', function (e) { state.inactiveIdDocumentType = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('cltInactiveFilterDistrict')?.addEventListener('change', function (e) { state.inactiveIdDistrict = e.target.value; state.inactivePage = 1; loadInactiveList(); });
        qs('cltInactivePageSize')?.addEventListener('change', function (e) { state.inactivePageSize = parseInt(e.target.value, 10); state.inactivePage = 1; loadInactiveList(); });
        qs('cltInactivePrevBtn')?.addEventListener('click', function () { if (state.inactivePage > 1) { state.inactivePage--; loadInactiveList(); } });
        qs('cltInactiveNextBtn')?.addEventListener('click', function () { state.inactivePage++; loadInactiveList(); });
        qs('cltConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('cltConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.clt-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('cltActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id); else if (action === 'edit') openEditModal(id); else if (action === 'delete') handleDeleteLogic(id);
        });
        qs('cltInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'restore') handleRestore(id); else if (action === 'purge') handlePurge(id);
        });
    }

    function init() {
        var root = document.getElementById('cltRoot');
        if (!root || root.dataset.initialized === 'true' || !window.cltUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        state.idDocumentType = ''; state.idDistrict = '';
        state.inactivePage = 1; state.inactivePageSize = 10; state.inactiveSearch = '';
        state.inactiveIdDocumentType = ''; state.inactiveIdDistrict = '';
        bindEvents();
        loadFilterOptions().then(function () { loadActiveList(); }).catch(function (err) {
            showToast(err.message || 'Error al cargar opciones de filtro.', false);
            loadActiveList();
        });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
