(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', idBrand: '', idSupplier: '', deleteSupplierId: null, deleteBrandId: null };

    function urls() { return window.sbrUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function getToken() {
        var input = document.querySelector('[name=__RequestVerificationToken]');
        return input ? input.value : '';
    }
    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text == null ? '' : String(text);
        return div.innerHTML;
    }
    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (key) {
            if (params[key] !== undefined && params[key] !== null && params[key] !== '') url.searchParams.set(key, params[key]);
        });
        return url.toString();
    }

    function showToast(message, isSuccess) {
        var toast = qs('sbrToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'sbr__toast is-visible ' + (isSuccess ? 'sbr__toast--success' : 'sbr__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
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
        if (urls().brandFilters) {
            fetch(urls().brandFilters, { credentials: 'same-origin' })
                .then(function (response) { return response.json(); })
                .then(function (data) { fillSelect('sbrBrandFilter', data.items, 'Todas las marcas'); })
                .catch(function () { });
        }
        if (urls().supplierFilters) {
            fetch(urls().supplierFilters, { credentials: 'same-origin' })
                .then(function (response) { return response.json(); })
                .then(function (data) { fillSelect('sbrSupplierFilter', data.items, 'Todos los proveedores'); })
                .catch(function () { });
        }
    }

    function openModal(id) {
        var modal = qs(id);
        if (modal) {
            modal.classList.add('is-open');
            modal.setAttribute('aria-hidden', 'false');
        }
    }

    function closeModal(id) {
        var modal = qs(id);
        if (modal) {
            modal.classList.remove('is-open');
            modal.setAttribute('aria-hidden', 'true');
        }
    }

    function closeAllModals() {
        document.querySelectorAll('.sbr-modal.is-open').forEach(function (modal) {
            modal.classList.remove('is-open');
            modal.setAttribute('aria-hidden', 'true');
        });
    }

    function postAction(url, data) {
        data.append('__RequestVerificationToken', getToken());
        return fetch(url, { method: 'POST', body: data, credentials: 'same-origin' })
            .then(function (response) { return response.json(); });
    }

    function updatePagination(data) {
        var page = data.page || state.page || 1;
        var totalPages = data.totalPages || 1;
        var info = qs('sbrPageInfo');
        if (info) info.textContent = 'Pagina ' + page + ' de ' + totalPages;
        if (qs('sbrPrevBtn')) qs('sbrPrevBtn').disabled = page <= 1;
        if (qs('sbrNextBtn')) qs('sbrNextBtn').disabled = page >= totalPages;
    }

    function renderRows(items) {
        var tbody = qs('sbrBody');
        if (!tbody) return;

        if (!items || items.length === 0) {
            tbody.innerHTML = '<tr class="sbr__empty-row"><td colspan="3">No hay asignaciones.</td></tr>';
            return;
        }

        tbody.innerHTML = items.map(function (item) {
            return '<tr>' +
                '<td>' + escapeHtml(item.supplierName) + '</td>' +
                '<td><span class="sbr-tag">' + escapeHtml(item.brandName) + '</span></td>' +
                '<td class="sbr__td-actions"><div class="sbr__row-actions">' +
                '<button type="button" class="sbr__icon-btn sbr__icon-btn--delete" data-action="delete" data-supplier-id="' + item.idSupplier + '" data-brand-id="' + item.idBrand + '" title="Eliminar"><i class="bi bi-trash"></i></button>' +
                '</div></td>' +
                '</tr>';
        }).join('');
    }

    function loadList() {
        var tbody = qs('sbrBody');
        if (tbody) tbody.innerHTML = '<tr class="sbr__loading-row"><td colspan="3">Cargando registros...</td></tr>';

        fetch(buildQuery(urls().list, { search: state.search, idBrand: state.idBrand, idSupplier: state.idSupplier, page: state.page, pageSize: state.pageSize }), { credentials: 'same-origin' })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                renderRows(Array.isArray(data.items) ? data.items : []);
                updatePagination(data);
            })
            .catch(function () {
                if (tbody) tbody.innerHTML = '<tr class="sbr__empty-row"><td colspan="3">Error al cargar registros.</td></tr>';
                showToast('Error al cargar registros.', false);
            });
    }

    function resetForm() {
        var form = qs('sbrForm');
        if (form) form.reset();
    }

    function saveForm() {
        var form = qs('sbrForm');
        if (!form || !form.checkValidity()) {
            if (form) form.reportValidity();
            return;
        }

        var data = new URLSearchParams();
        data.append('idSupplier', qs('sId').value);
        data.append('idBrand', qs('bId').value);

        postAction(urls().save, data)
            .then(function (result) {
                showToast(result.message || (result.success ? 'Asignacion guardada.' : 'No se pudo guardar.'), result.success);
                if (result.success) {
                    closeModal('sbrModal');
                    loadList();
                }
            })
            .catch(function () { showToast('Error al guardar.', false); });
    }

    function confirmDelete(idSupplier, idBrand) {
        state.deleteSupplierId = idSupplier;
        state.deleteBrandId = idBrand;
        openModal('sbrConfirmModal');
    }

    function deleteSelected() {
        if (!state.deleteSupplierId || !state.deleteBrandId) return;

        var data = new URLSearchParams();
        data.append('idSupplier', state.deleteSupplierId);
        data.append('idBrand', state.deleteBrandId);

        postAction(urls().delete, data)
            .then(function (result) {
                showToast(result.message || (result.success ? 'Asignacion eliminada.' : 'No se pudo eliminar.'), result.success !== false);
                state.deleteSupplierId = null;
                state.deleteBrandId = null;
                closeModal('sbrConfirmModal');
                loadList();
            })
            .catch(function () { showToast('Error al eliminar.', false); });
    }

    function bindEvents() {
        var searchInput = qs('sbrSearchInput');
        var timer = null;
        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearTimeout(timer);
                timer = setTimeout(function () {
                    state.search = searchInput.value.trim();
                    state.page = 1;
                    loadList();
                }, 350);
            });
        }

        if (qs('sbrClearSearchBtn')) qs('sbrClearSearchBtn').addEventListener('click', function () {
            state.search = '';
            state.idBrand = '';
            state.idSupplier = '';
            state.page = 1;
            if (searchInput) searchInput.value = '';
            if (qs('sbrBrandFilter')) qs('sbrBrandFilter').value = '';
            if (qs('sbrSupplierFilter')) qs('sbrSupplierFilter').value = '';
            loadList();
        });

        if (qs('sbrBrandFilter')) qs('sbrBrandFilter').addEventListener('change', function (e) {
            state.idBrand = e.target.value;
            state.page = 1;
            loadList();
        });

        if (qs('sbrSupplierFilter')) qs('sbrSupplierFilter').addEventListener('change', function (e) {
            state.idSupplier = e.target.value;
            state.page = 1;
            loadList();
        });

        if (qs('sbrPageSize')) qs('sbrPageSize').addEventListener('change', function (event) {
            state.pageSize = parseInt(event.target.value, 10);
            state.page = 1;
            loadList();
        });

        if (qs('sbrPrevBtn')) qs('sbrPrevBtn').addEventListener('click', function () {
            if (state.page > 1) {
                state.page--;
                loadList();
            }
        });

        if (qs('sbrNextBtn')) qs('sbrNextBtn').addEventListener('click', function () {
            state.page++;
            loadList();
        });

        if (qs('sbrCreateBtn')) qs('sbrCreateBtn').addEventListener('click', function () {
            resetForm();
            openModal('sbrModal');
        });

        if (qs('sbrSaveBtn')) qs('sbrSaveBtn').addEventListener('click', saveForm);
        if (qs('sbrConfirmBtn')) qs('sbrConfirmBtn').addEventListener('click', deleteSelected);

        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (button) {
            button.addEventListener('click', closeAllModals);
        });

        document.querySelectorAll('.sbr-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', closeAllModals);
        });

        var tbody = qs('sbrBody');
        if (tbody) {
            tbody.addEventListener('click', function (event) {
                var button = event.target.closest('[data-action="delete"]');
                if (!button) return;
                confirmDelete(parseInt(button.getAttribute('data-supplier-id'), 10), parseInt(button.getAttribute('data-brand-id'), 10));
            });
        }
    }

    function init() {
        var root = qs('sbrRoot');
        if (!root || root.dataset.initialized === 'true' || !window.sbrUrls) return;
        root.dataset.initialized = 'true';

        bindEvents();
        loadFilters();
        loadList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
