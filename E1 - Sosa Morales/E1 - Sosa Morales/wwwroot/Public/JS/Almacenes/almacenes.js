(function () {
    'use strict';
    var state = { page: 1, pageSize: 10, search: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null, districtsLoaded: false };
    var colCount = 4;
    function urls() { return window.almUrls || {}; }
    function getToken() { var i = document.querySelector('input[name="__RequestVerificationToken"]'); return i ? i.value : ''; }
    function qs(id) { return document.getElementById(id); }
    function showToast(msg, ok) { var t = qs('almToast'); if (!t) return; t.textContent = msg; t.className = 'alm__toast is-visible ' + (ok ? 'alm__toast--success' : 'alm__toast--error'); setTimeout(function () { t.classList.remove('is-visible'); }, 3200); }
    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeAllModals() { document.querySelectorAll('.alm-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }
    function buildQuery(u, p) { var url = new URL(u, window.location.origin); Object.keys(p).forEach(function (k) { if (p[k] !== undefined && p[k] !== null && p[k] !== '') url.searchParams.set(k, p[k]); }); return url.toString(); }
    function fetchJson(url, options) {
        options = options || {}; options.headers = options.headers || {}; options.headers['X-Requested-With'] = 'XMLHttpRequest'; options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) { return r.json().then(function (d) { if (!r.ok) throw new Error(d.message || 'Error de red'); return d; }); });
    }
    function postAction(url, data) {
        var body = new URLSearchParams(); body.append('__RequestVerificationToken', getToken());
        Object.keys(data).forEach(function (k) { if (data[k] !== undefined && data[k] !== null) body.append(k, data[k]); });
        return fetchJson(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getToken() }, body: body.toString() });
    }
    function escapeHtml(t) { var d = document.createElement('div'); d.textContent = t == null ? '' : String(t); return d.innerHTML; }
    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        if (!items || !items.length) { tbody.innerHTML = '<tr class="alm__empty-row"><td colspan="' + (colCount + 1) + '">No se encontraron registros.</td></tr>'; return; }
        tbody.innerHTML = items.map(function (row) {
            var actions = mode === 'active'
                ? '<button type="button" class="alm__icon-btn alm__icon-btn--view" data-action="view" data-id="' + row.id + '"><i class="bi bi-eye"></i></button><button type="button" class="alm__icon-btn alm__icon-btn--edit" data-action="edit" data-id="' + row.id + '"><i class="bi bi-pencil"></i></button><button type="button" class="alm__icon-btn alm__icon-btn--delete" data-action="delete" data-id="' + row.id + '"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="alm__icon-btn alm__icon-btn--restore" data-action="restore" data-id="' + row.id + '"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="alm__icon-btn alm__icon-btn--purge" data-action="purge" data-id="' + row.id + '"><i class="bi bi-trash-fill"></i></button>';
            return '<tr><td>' + escapeHtml(row.id) + '</td><td>' + escapeHtml(row.name) + '</td><td>' + escapeHtml(row.address) + '</td><td>' + escapeHtml(row.districtName) + '</td><td class="alm__td-actions"><div class="alm__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }
    function updatePagination(info, prev, next, page, totalPages) {
        if (info) info.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prev) prev.disabled = page <= 1;
        if (next) next.disabled = page >= totalPages || totalPages === 0;
    }
    function loadActiveList() {
        var tbody = qs('almActiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="alm__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().list, { search: state.search, page: state.page, pageSize: state.pageSize })).then(function (data) {
            renderRows(tbody, data.items, 'active');
            updatePagination(qs('almPageInfo'), qs('almPrevBtn'), qs('almNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (e) { showToast(e.message, false); }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }
    function loadInactiveList() {
        var tbody = qs('almInactiveBody');
        var scrollEl = tbody && tbody.closest('[class*="__table-scroll"]');
        var loadHtml = '<tr class="alm__loading-row"><td colspan="' + (colCount + 1) + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml); else if (tbody) tbody.innerHTML = loadHtml;
        fetchJson(buildQuery(urls().listInactive, { search: state.inactiveSearch, page: state.inactivePage, pageSize: state.inactivePageSize })).then(function (data) {
            renderRows(tbody, data.items, 'inactive');
            updatePagination(qs('almInactivePageInfo'), qs('almInactivePrevBtn'), qs('almInactiveNextBtn'), data.page, data.totalPages || 1);
        }).catch(function () { }).finally(function () { if (window.KmlTableList) KmlTableList.end(scrollEl); });
    }
    function loadDistricts() {
        return fetchJson(urls().districtOptions).then(function (res) {
            var sel = qs('almDistrict'); if (!sel) return;
            var cur = sel.value;
            sel.innerHTML = '<option value="">Sin distrito</option>';
            (res.items || []).forEach(function (o) { var opt = document.createElement('option'); opt.value = o.id; opt.textContent = o.name; sel.appendChild(opt); });
            if (cur) sel.value = cur;
            state.districtsLoaded = true;
        });
    }
    function resetForm() { qs('almFormId').value = ''; qs('almName').value = ''; qs('almAddress').value = ''; if (qs('almDistrict')) qs('almDistrict').value = ''; }
    function openCreateModal() { resetForm(); qs('almFormModalTitle').textContent = 'Crear'; openModal('almFormModal'); }
    function openEditModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            resetForm(); qs('almFormModalTitle').textContent = 'Editar';
            qs('almFormId').value = res.data.id; qs('almName').value = res.data.name; qs('almAddress').value = res.data.address;
            if (qs('almDistrict')) qs('almDistrict').value = res.data.idDistrict || '';
            openModal('almFormModal');
        });
    }
    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (res) {
            if (!res.success) { showToast(res.message, false); return; }
            var d = res.data;
            var rows = [
                { l: 'ID', v: d.id }, { l: 'Nombre', v: d.name }, { l: 'Dirección', v: d.address },
                { l: 'País', v: d.countryName || '—' }, { l: 'Región', v: d.regionName || '—' },
                { l: 'Provincia', v: d.provinceName || '—' }, { l: 'Distrito', v: d.districtName || '—' },
                { l: 'Estado', v: d.status === 1 ? 'Activo' : 'Inactivo' }, { l: 'Creado', v: d.createdAt }
            ];
            if (d.updatedAt) rows.push({ l: 'Actualizado', v: d.updatedAt });
            qs('almDetailBody').innerHTML = rows.map(function (r) {
                return '<div class="alm-detail__row"><span class="alm-detail__label">' + escapeHtml(r.l) + '</span><span class="alm-detail__value">' + escapeHtml(r.v) + '</span></div>';
            }).join('');
            openModal('almDetailModal');
        });
    }
    function saveForm() {
        var form = qs('almForm'); if (!form.checkValidity()) { form.reportValidity(); return; }
        var id = qs('almFormId').value;
        var data = { name: qs('almName').value.trim(), address: qs('almAddress').value.trim(), idDistrict: qs('almDistrict').value || '' };
        postAction(id ? urls().update + '?id=' + id : urls().create, data).then(function (res) {
            showToast(res.message, res.success); if (res.success) { closeAllModals(); loadActiveList(); }
        });
    }
    function confirmAction(title, msg, cb) { qs('almConfirmTitle').textContent = title; qs('almConfirmMessage').textContent = msg; state.confirmCallback = cb; openModal('almConfirmModal'); }
    function bindEvents() {
        var si = qs('almSearchInput'), st;
        if (si) si.addEventListener('input', function () { clearTimeout(st); st = setTimeout(function () { state.search = si.value.trim(); state.page = 1; loadActiveList(); }, 350); });
        qs('almClearSearchBtn')?.addEventListener('click', function () { state.search = ''; si.value = ''; state.page = 1; loadActiveList(); });
        qs('almPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('almPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('almNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('almCreateBtn')?.addEventListener('click', openCreateModal);
        qs('almFormSaveBtn')?.addEventListener('click', saveForm);
        qs('almInactiveBtn')?.addEventListener('click', function () { state.inactivePage = 1; loadInactiveList(); openModal('almInactiveModal'); });
        qs('almConfirmBtn')?.addEventListener('click', function () { if (state.confirmCallback) state.confirmCallback(); state.confirmCallback = null; closeAllModals(); });
        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (b) { b.addEventListener('click', closeAllModals); });
        document.querySelectorAll('.alm-modal__backdrop').forEach(function (b) { b.addEventListener('click', closeAllModals); });
        qs('almActiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10), a = btn.getAttribute('data-action');
            if (a === 'view') openDetailModal(id); else if (a === 'edit') openEditModal(id);
            else if (a === 'delete') confirmAction('Desactivar', '¿Desea desactivar este almacén?', function () { postAction(urls().deleteLogic, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) loadActiveList(); }); });
        });
        qs('almInactiveBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]'); if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10), a = btn.getAttribute('data-action');
            if (a === 'restore') confirmAction('Restaurar', '¿Desea restaurar este almacén?', function () { postAction(urls().restore, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) { loadInactiveList(); loadActiveList(); } }); });
            else if (a === 'purge') confirmAction('Eliminar', '¿Eliminar permanentemente?', function () { postAction(urls().deletePhysical, { id: id }).then(function (r) { showToast(r.message, r.success); if (r.success) loadInactiveList(); }); });
        });
    }
    function init() {
        var root = qs('almRoot'); if (!root || root.dataset.initialized === 'true' || !window.almUrls) return;
        root.dataset.initialized = 'true'; bindEvents();
        loadDistricts().then(loadActiveList).catch(loadActiveList);
    }
    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
