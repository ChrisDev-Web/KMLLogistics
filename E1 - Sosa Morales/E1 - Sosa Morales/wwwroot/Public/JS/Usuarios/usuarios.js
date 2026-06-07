(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', confirmCallback: null, isEdit: false };
    var colCount = 3;

    function urls() { return window.usrUrls || {}; }
    function getToken() { var input = document.querySelector('input[name="__RequestVerificationToken"]'); return input ? input.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('usrToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'usr__toast is-visible ' + (isSuccess ? 'usr__toast--success' : 'usr__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.usr-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

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

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || items.length === 0) { tbody.innerHTML = '<tr class="usr__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        var html = items.map(function (row) {
            var cells = '<td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.username) + '</td><td>' + escapeHtml(row.roleName || '') + '</td>';
            var actions = '<button type="button" class="usr__icon-btn usr__icon-btn--view" data-action="view" data-id="' + row.id + '" title="Ver detalle"><i class="bi bi-eye"></i></button>'
                + '<button type="button" class="usr__icon-btn usr__icon-btn--edit" data-action="edit" data-id="' + row.id + '" title="Editar"><i class="bi bi-pencil"></i></button>'
                + '<button type="button" class="usr__icon-btn usr__icon-btn--purge" data-action="delete" data-id="' + row.id + '" title="Eliminar permanentemente"><i class="bi bi-trash-fill"></i></button>';
            return '<tr data-id="' + row.id + '">' + cells + '<td class="usr__td-actions"><div class="usr__row-actions">' + actions + '</div></td></tr>';
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
        var tbody = qs('usrActiveBody');
        if (tbody) tbody.innerHTML = '<tr class="usr__loading-row"><td colspan="' + (colCount + 1) + '">Cargando registros...</td></tr>';
        fetchJson(buildQuery(u.list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('usrPageInfo'), qs('usrPrevBtn'), qs('usrNextBtn'), data.page, data.totalPages || 1);
            updateTableScroll(qs('usrTableScroll'), data.items ? data.items.length : 0);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="usr__empty-row"><td colspan="' + (colCount + 1) + '">Error al cargar los registros.</td></tr>';
            showToast(err.message || 'Error al cargar los registros.', false);
        });
    }

    function loadFkOptions() {
        return fetchJson(urls().fkOptions).then(function (data) {
            if (!data.success) return;
            var select = qs('usrRoleSelect');
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

    function setPasswordRequired(isRequired) {
        var pwd = qs('usrPassword');
        var hint = qs('usrPasswordHint');
        state.isEdit = !isRequired;
        if (pwd) pwd.required = isRequired;
        if (hint) hint.textContent = isRequired ? '*' : '(opcional)';
    }

    function resetForm() {
        var form = qs('usrForm');
        if (form) form.reset();
        qs('usrFormId').value = '';
        setPasswordRequired(true);
    }

    function openCreateModal() {
        resetForm();
        qs('usrFormModalTitle').textContent = 'Crear';
        loadFkOptions().then(function () { openModal('usrFormModal'); }).catch(function (err) {
            showToast(err.message || 'Error al cargar roles.', false);
            openModal('usrFormModal');
        });
    }

    function openEditModal(id) {
        var u = urls();
        fetchJson(buildQuery(u.get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message || 'Registro no encontrado.', false); return; }
            resetForm();
            setPasswordRequired(false);
            qs('usrFormModalTitle').textContent = 'Editar';
            qs('usrFormId').value = res.data.id;
            qs('usrUsername').value = res.data.username || '';
            loadFkOptions().then(function () {
                if (res.data.idRole !== undefined) qs('usrRoleSelect').value = res.data.idRole;
                openModal('usrFormModal');
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
                { label: 'Usuario', value: d.username },
                { label: 'Rol', value: d.roleName },
                { label: 'Creado', value: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ label: 'Actualizado', value: d.updatedAt });
            qs('usrDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="usr-detail__row"><span class="usr-detail__label">' + escapeHtml(r.label) + '</span><span class="usr-detail__value">' + escapeHtml(r.value) + '</span></div>';
            }).join('');
            openModal('usrDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }

    function saveForm() {
        var u = urls();
        var form = qs('usrForm');
        var id = qs('usrFormId').value;
        var password = qs('usrPassword').value;

        if (!id && !password.trim()) {
            showToast('La contraseña es obligatoria al crear.', false);
            return;
        }

        if (!form.checkValidity()) { form.reportValidity(); return; }

        var data = {
            username: qs('usrUsername').value.trim(),
            idRole: qs('usrRoleSelect').value
        };
        if (password.trim()) data.password = password;

        var url = id ? u.update + '?id=' + encodeURIComponent(id) : u.create;
        postAction(url, data).then(function (res) {
            showToast(res.message, res.success);
            if (res.success) { closeModal('usrFormModal'); loadActiveList(); }
        }).catch(function (err) { showToast(err.message || 'Error al guardar.', false); });
    }

    function confirmAction(title, message, callback) {
        qs('usrConfirmTitle').textContent = title;
        qs('usrConfirmMessage').textContent = message;
        state.confirmCallback = callback;
        openModal('usrConfirmModal');
    }

    function handleDeletePhysical(id) {
        confirmAction('Eliminar permanentemente', 'Esta acción no se puede deshacer. ¿Eliminar el usuario de la base de datos?', function () {
            postAction(urls().deletePhysical, { id: id }).then(function (res) {
                showToast(res.message, res.success);
                if (res.success) loadActiveList();
            });
        });
    }

    function bindEvents() {
        var searchInput = qs('usrSearchInput');
        var searchTimer = null;
        if (searchInput) searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = searchInput.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('usrClearSearchBtn')?.addEventListener('click', function () { state.search = ''; if (searchInput) searchInput.value = ''; state.page = 1; loadActiveList(); });
        qs('usrPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('usrPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('usrNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('usrCreateBtn')?.addEventListener('click', openCreateModal);
        qs('usrFormSaveBtn')?.addEventListener('click', saveForm);
        qs('usrConfirmBtn')?.addEventListener('click', function () { if (typeof state.confirmCallback === 'function') state.confirmCallback(); state.confirmCallback = null; closeModal('usrConfirmModal'); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (btn) { btn.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.usr-modal__backdrop').forEach(function (backdrop) { backdrop.addEventListener('click', closeAllModals); });
        qs('usrActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10); var action = btn.getAttribute('data-action');
            if (action === 'view') openDetailModal(id);
            else if (action === 'edit') openEditModal(id);
            else if (action === 'delete') handleDeletePhysical(id);
        });
    }

    function init() {
        var root = document.getElementById('usrRoot');
        if (!root || root.dataset.initialized === 'true' || !window.usrUrls) return;
        root.dataset.initialized = 'true';
        state.page = 1; state.pageSize = 10; state.search = '';
        bindEvents();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
