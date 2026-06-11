(function () {
    'use strict';
    var state = { page: 1, pageSize: 10, search: '', idCategory: '', idBrand: '', inactivePage: 1, inactivePageSize: 10, inactiveSearch: '', confirmCallback: null };
    var colCount = 7;

    function urls() { return window.proUrls || {}; }
    function getToken() { var i = document.querySelector('input[name="__RequestVerificationToken"]'); return i ? i.value : ''; }
    function qs(id) { return document.getElementById(id); }

    function showToast(msg, isOk) {
        var t = qs('proToast'); if (!t) return;
        t.textContent = msg; t.className = 'pro__toast is-visible ' + (isOk ? 'pro__toast--success' : 'pro__toast--error');
        setTimeout(function () { t.classList.remove('is-visible'); }, 3200);
    }

    function openModal(id) { var m = qs(id); if (m) { m.classList.add('is-open'); m.setAttribute('aria-hidden', 'false'); } }
    function closeModal(id) { var m = qs(id); if (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); } }
    function closeAllModals() { document.querySelectorAll('.pro-modal.is-open').forEach(function (m) { m.classList.remove('is-open'); m.setAttribute('aria-hidden', 'true'); }); }

    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (k) { if (params[k] !== undefined && params[k] !== null && params[k] !== '') url.searchParams.set(k, params[k]); });
        return url.toString();
    }

    function fetchJson(url, options) {
        options = options || {}; options.headers = options.headers || {};
        options.headers['X-Requested-With'] = 'XMLHttpRequest'; options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) { return r.json().then(function (d) { if (!r.ok) throw new Error(d.message || 'Error'); return d; }); });
    }

    function postAction(url, data) {
        var token = getToken(); var body = new URLSearchParams(); body.append('__RequestVerificationToken', token);
        Object.keys(data).forEach(function (k) { if (data[k] !== undefined && data[k] !== null && data[k] !== '') body.append(k, data[k]); });
        return fetchJson(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token }, body: body.toString() });
    }

    function escapeHtml(text) { var d = document.createElement('div'); d.textContent = text == null ? '' : String(text); return d.innerHTML; }

    function postForm(url, formData) {
        var token = getToken();
        formData.append('__RequestVerificationToken', token);
        return fetchJson(url, { method: 'POST', headers: { 'RequestVerificationToken': token }, body: formData });
    }

    function resolvePhotoUrl(photo) {
        if (!photo) return '';
        var value = String(photo).trim();
        if (!value) return '';
        if (/^(https?:)?\/\//i.test(value) || /^data:/i.test(value) || value.charAt(0) === '/') return value;
        return '/Public/Images/Products/' + value;
    }

    function renderPhoto(photo, name) {
        var url = resolvePhotoUrl(photo);
        var initial = (name || '?').trim().charAt(0).toUpperCase() || '?';
        if (!url) return '<span class="pro-photo pro-photo--empty">' + escapeHtml(initial) + '</span>';
        return '<span class="pro-photo"><img src="' + escapeHtml(url) + '" alt="' + escapeHtml(name || 'Producto') + '" onerror="this.parentElement.classList.add(\'pro-photo--empty\');this.remove();" /><span>' + escapeHtml(initial) + '</span></span>';
    }

    function setPhotoPreview(photo, name) {
        var preview = qs('proPhotoPreview');
        if (!preview) return;
        var url = resolvePhotoUrl(photo);
        var initial = (name || 'PR').trim().slice(0, 2).toUpperCase() || 'PR';
        if (!url) {
            preview.className = 'pro-photo-preview pro-photo-preview--empty';
            preview.innerHTML = '<span>' + escapeHtml(initial) + '</span>';
            return;
        }
        preview.className = 'pro-photo-preview';
        preview.innerHTML = '<img src="' + escapeHtml(url) + '" alt="' + escapeHtml(name || 'Producto') + '" onerror="this.parentElement.className=\'pro-photo-preview pro-photo-preview--empty\';this.parentElement.innerHTML=\'<span>' + escapeHtml(initial) + '</span>\';" />';
    }

    function fillSelect(selectId, items, emptyLabel) {
        var select = qs(selectId);
        if (!select) return;
        var html = '<option value="">' + escapeHtml(emptyLabel) + '</option>';
        html += (Array.isArray(items) ? items : []).map(function (item) {
            return '<option value="' + item.id + '">' + escapeHtml(item.name) + '</option>';
        }).join('');
        select.innerHTML = html;
    }

    function loadFilters() {
        if (urls().categoryFilters) {
            fetchJson(urls().categoryFilters).then(function (data) {
                fillSelect('proCategoryFilter', data.items, 'Todas las categorias');
            }).catch(function () { });
        }
        if (urls().brandFilters) {
            fetchJson(urls().brandFilters).then(function (data) {
                fillSelect('proBrandFilter', data.items, 'Todas las marcas');
            }).catch(function () { });
        }
    }

    function renderRows(tbody, items, mode) {
        if (!tbody) return;
        var arr = Array.isArray(items) ? items : (items && items.items ? items.items : []);
        if (arr.length === 0) { tbody.innerHTML = '<tr class="pro__empty-row"><td colspan="' + (colCount + 1) + '">No hay registros.</td></tr>'; return; }

        tbody.innerHTML = arr.map(function (r) {
            var actions = mode === 'active'
                ? '<button type="button" class="pro__icon-btn pro__icon-btn--view" data-action="view" data-id="'+r.idProduct+'"><i class="bi bi-eye"></i></button><button type="button" class="pro__icon-btn pro__icon-btn--edit" data-action="edit" data-id="'+r.idProduct+'"><i class="bi bi-pencil"></i></button><button type="button" class="pro__icon-btn pro__icon-btn--delete" data-action="delete" data-id="'+r.idProduct+'"><i class="bi bi-trash"></i></button>'
                : '<button type="button" class="pro__icon-btn pro__icon-btn--restore" data-action="restore" data-id="'+r.idProduct+'"><i class="bi bi-arrow-counterclockwise"></i></button><button type="button" class="pro__icon-btn pro__icon-btn--purge" data-action="purge" data-id="'+r.idProduct+'"><i class="bi bi-trash-fill"></i></button>';
            
            var cells = '<td>' + renderPhoto(r.photo, r.name) + '</td><td>' + r.idProduct + '</td><td>' + escapeHtml(r.name) + '</td><td>' + escapeHtml(r.categoryName) + '</td><td>' + escapeHtml(r.brandName) + '</td><td>S/ ' + Number(r.cost).toFixed(2) + '</td><td><strong>S/ ' + Number(r.salePrice).toFixed(2) + '</strong></td>';
            if(mode === 'inactive') cells = '<td>' + renderPhoto(r.photo, r.name) + '</td><td>' + r.idProduct + '</td><td>' + escapeHtml(r.name) + '</td><td>S/ ' + Number(r.salePrice).toFixed(2) + '</td>';
            return '<tr data-id="' + r.idProduct + '">' + cells + '<td class="pro__td-actions"><div class="pro__row-actions">' + actions + '</div></td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + (page || 1) + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = (page || 1) <= 1;
        if (nextBtn) nextBtn.disabled = (page || 1) >= (totalPages || 1) || (totalPages || 0) === 0;
    }

    function loadList(urlObj, tbodyId, searchVal, pageVal, pageSizeVal, mode) {
        fetchJson(buildQuery(urlObj, { search: searchVal, idCategory: state.idCategory, idBrand: state.idBrand, page: pageVal, pageSize: pageSizeVal })).then(function (d) {
            renderRows(qs(tbodyId), d.items, mode);
            if (mode === 'active') {
                updatePagination(qs('proPageInfo'), qs('proPrevBtn'), qs('proNextBtn'), d.page, d.totalPages || 1);
            }
        });
    }

    function loadActiveList() { loadList(urls().list, 'proActiveBody', state.search, state.page, state.pageSize, 'active'); }
    function loadInactiveList() { loadList(urls().listInactive, 'proInactiveBody', state.inactiveSearch, state.inactivePage, state.inactivePageSize, 'inactive'); }

    function resetForm() {
        var f = qs('proForm');
        if (f) f.reset();
        qs('proFormId').value = '0';
        if (qs('proRemovePhoto')) qs('proRemovePhoto').value = 'false';
        setPhotoPreview('', '');
    }

    function saveForm() {
        var f = qs('proForm'); if (!f.checkValidity()) { f.reportValidity(); return; }
        
        var data = new FormData();
        data.append('IdProduct', qs('proFormId').value);
        data.append('Name', qs('proName').value.trim());
        data.append('IdCategory', qs('proCategory').value);
        data.append('IdBrand', qs('proBrand').value);
        data.append('Cost', qs('proCost').value);
        data.append('ProfitPercentage', qs('proProfit').value);
        data.append('Weight', qs('proWeight').value);
        data.append('Height', qs('proHeight').value);
        data.append('Width', qs('proWidth').value);
        data.append('Length', qs('proLength').value);
        data.append('Description', qs('proDescription').value.trim());
        data.append('removePhoto', qs('proRemovePhoto') ? qs('proRemovePhoto').value : 'false');
        var photoInput = qs('proPhotoInput');
        if (photoInput && photoInput.files && photoInput.files[0]) data.append('photo', photoInput.files[0]);

        postForm(urls().save, data).then(function (r) {
            showToast(r.message, r.success); if (r.success) { closeModal('proFormModal'); loadActiveList(); }
        }).catch(function (e) { showToast('Error: ' + e.message, false); });
    }

    function openEditModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (r) {
            if (!r.success) { showToast(r.message, false); return; }
            resetForm(); var d = r.data;
            qs('proFormModalTitle').textContent = 'Editar';
            qs('proFormId').value = d.idProduct; qs('proName').value = d.name;
            qs('proCategory').value = d.idCategory; qs('proBrand').value = d.idBrand;
            qs('proCost').value = d.cost; qs('proProfit').value = d.profitPercentage;
            qs('proWeight').value = d.weight || ''; qs('proHeight').value = d.height || '';
            qs('proWidth').value = d.width || ''; qs('proLength').value = d.length || '';
            qs('proDescription').value = d.description || '';
            setPhotoPreview(d.photo, d.name);
            openModal('proFormModal');
        });
    }
    function openDetailModal(id) {
        fetchJson(buildQuery(urls().get, { id: id })).then(function (r) {
            if (!r.success) { showToast(r.message || 'Registro no encontrado.', false); return; }
            var d = r.data;

            // Armamos las filas del detalle
            var rows = [
                { label: 'ID', value: d.idProduct },
                { label: 'Nombre', value: d.name },
                { label: 'Costo', value: 'S/ ' + Number(d.cost).toFixed(2) },
                { label: '% Ganancia', value: d.profitPercentage + '%' },
                { label: 'Precio Venta', value: 'S/ ' + Number(d.salePrice).toFixed(2) }
            ];

            if (d.description) rows.push({ label: 'Descripción', value: d.description });
            if (d.weight) rows.push({ label: 'Peso', value: d.weight + ' kg' });
            if (d.length || d.width || d.height) rows.push({ label: 'Dimensiones', value: (d.length || 0) + 'x' + (d.width || 0) + 'x' + (d.height || 0) + ' cm' });

            var photoHtml = '<div class="pro-detail__photo">' + renderPhoto(d.photo, d.name) + '</div>';
            qs('proDetailBody').innerHTML = photoHtml + rows.map(function (row) {
                return '<div class="pro-detail__row"><span class="pro-detail__label">' + escapeHtml(row.label) + '</span><span class="pro-detail__value">' + escapeHtml(row.value) + '</span></div>';
            }).join('');

            openModal('proDetailModal');
        }).catch(function (err) { showToast(err.message || 'Error al cargar.', false); });
    }
    function bindEvents() {
        var searchTimer = null;
        qs('proSearchInput')?.addEventListener('input', function (e) {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () { state.search = e.target.value.trim(); state.page = 1; loadActiveList(); }, 350);
        });
        qs('proClearSearchBtn')?.addEventListener('click', function () {
            state.search = '';
            state.idCategory = '';
            state.idBrand = '';
            state.page = 1;
            if (qs('proSearchInput')) qs('proSearchInput').value = '';
            if (qs('proCategoryFilter')) qs('proCategoryFilter').value = '';
            if (qs('proBrandFilter')) qs('proBrandFilter').value = '';
            loadActiveList();
        });
        qs('proPageSize')?.addEventListener('change', function (e) { state.pageSize = parseInt(e.target.value, 10); state.page = 1; loadActiveList(); });
        qs('proPrevBtn')?.addEventListener('click', function () { if (state.page > 1) { state.page--; loadActiveList(); } });
        qs('proNextBtn')?.addEventListener('click', function () { state.page++; loadActiveList(); });
        qs('proCategoryFilter')?.addEventListener('change', function (e) { state.idCategory = e.target.value; state.page = 1; loadActiveList(); });
        qs('proBrandFilter')?.addEventListener('change', function (e) { state.idBrand = e.target.value; state.page = 1; loadActiveList(); });
        qs('proCreateBtn')?.addEventListener('click', function () { resetForm(); qs('proFormModalTitle').textContent = 'Crear Producto'; openModal('proFormModal'); });
        qs('proFormSaveBtn')?.addEventListener('click', saveForm);
        qs('proInactiveBtn')?.addEventListener('click', function () { loadInactiveList(); openModal('proInactiveModal'); });

        qs('proPhotoInput')?.addEventListener('change', function (e) {
            var file = e.target.files && e.target.files[0];
            if (!file) return;
            if (!/\.(jpe?g|png|webp)$/i.test(file.name)) {
                showToast('Formato no permitido. Use JPG, PNG o WEBP.', false);
                e.target.value = '';
                return;
            }
            if (file.size > 2 * 1024 * 1024) {
                showToast('La imagen no puede superar 2 MB.', false);
                e.target.value = '';
                return;
            }
            if (qs('proRemovePhoto')) qs('proRemovePhoto').value = 'false';
            var reader = new FileReader();
            reader.onload = function (ev) { setPhotoPreview(ev.target.result, qs('proName') ? qs('proName').value : 'Producto'); };
            reader.readAsDataURL(file);
        });

        qs('proPhotoRemoveBtn')?.addEventListener('click', function () {
            var photoInput = qs('proPhotoInput');
            if (photoInput) photoInput.value = '';
            if (qs('proRemovePhoto')) qs('proRemovePhoto').value = 'true';
            setPhotoPreview('', qs('proName') ? qs('proName').value : '');
        });

        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (b) { b.addEventListener('click', closeAllModals); });

        // Clics en la tabla ACTIVA
        qs('proActiveBody')?.addEventListener('click', function (e) {
            var b = e.target.closest('[data-action]'); if (!b) return;
            var id = parseInt(b.getAttribute('data-id'), 10), act = b.getAttribute('data-action');

            if (act === 'view') openDetailModal(id); // Faltaba esto
            if (act === 'edit') openEditModal(id);
            if (act === 'delete') {
                qs('proConfirmTitle').textContent = 'Desactivar';
                qs('proConfirmMessage').textContent = '¿Desactivar producto? Aparecerá en inactivos.';
                state.confirmCallback = function () { postAction(urls().deleteLogic, { id: id }).then(function (r) { showToast(r.message, r.success); loadActiveList(); }); };
                openModal('proConfirmModal');
            }
        });

        // Clics en la tabla INACTIVA (¡Faltaba todo este bloque!)
        qs('proInactiveBody')?.addEventListener('click', function (e) {
            var b = e.target.closest('[data-action]'); if (!b) return;
            var id = parseInt(b.getAttribute('data-id'), 10), act = b.getAttribute('data-action');

            if (act === 'restore') {
                qs('proConfirmTitle').textContent = 'Restaurar';
                qs('proConfirmMessage').textContent = '¿Desea restaurar este producto a la lista activa?';
                state.confirmCallback = function () { postAction(urls().restore, { id: id }).then(function (r) { showToast(r.message, r.success); loadInactiveList(); loadActiveList(); }); };
                openModal('proConfirmModal');
            }
            if (act === 'purge') {
                qs('proConfirmTitle').textContent = 'Eliminar Definitivo';
                qs('proConfirmMessage').textContent = 'Esta acción no se puede deshacer. ¿Eliminar de la base de datos?';
                state.confirmCallback = function () { postAction(urls().deletePhysical, { id: id }).then(function (r) { showToast(r.message, r.success); loadInactiveList(); loadActiveList(); }); };
                openModal('proConfirmModal');
            }
        });

        qs('proConfirmBtn')?.addEventListener('click', function () { if (state.confirmCallback) state.confirmCallback(); state.confirmCallback = null; closeModal('proConfirmModal'); });
    }

    function init() {
        var root = document.getElementById('proRoot');
        if (!root || root.dataset.initialized === 'true' || !window.proUrls) return;
        root.dataset.initialized = 'true';

        bindEvents();
        loadFilters();
        loadActiveList();
    }

    document.addEventListener('DOMContentLoaded', init);

    // ¡ESTA ES LA LÍNEA MÁGICA QUE FALTABA!
    document.addEventListener('dashboard:contentLoaded', init);
})();
