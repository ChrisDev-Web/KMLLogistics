(function () {
    'use strict';

    var state = { page: 1, pageSize: 10, search: '', idProduct: '', idSupplier: '', deleteId: null, itemsById: {} };

    function urls() { return window.prpUrls || {}; }
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
        var toast = qs('prpToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'prp__toast is-visible ' + (isSuccess ? 'prp__toast--success' : 'prp__toast--error');
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
        if (urls().productFilters) {
            fetch(urls().productFilters, { credentials: 'same-origin' })
                .then(function (response) { return response.json(); })
                .then(function (data) { fillSelect('prpProductFilter', data.items, 'Todos los productos'); })
                .catch(function () { });
        }
        if (urls().supplierFilters) {
            fetch(urls().supplierFilters, { credentials: 'same-origin' })
                .then(function (response) { return response.json(); })
                .then(function (data) { fillSelect('prpSupplierFilter', data.items, 'Todos los proveedores'); })
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
        document.querySelectorAll('.prp-modal.is-open').forEach(function (modal) {
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
        var info = qs('prpPageInfo');
        if (info) info.textContent = 'Pagina ' + page + ' de ' + totalPages;
        if (qs('prpPrevBtn')) qs('prpPrevBtn').disabled = page <= 1;
        if (qs('prpNextBtn')) qs('prpNextBtn').disabled = page >= totalPages;
    }

    function renderRows(items) {
        var tbody = qs('prpBody');
        if (!tbody) return;
        state.itemsById = {};

        if (!items || items.length === 0) {
            tbody.innerHTML = '<tr class="prp__empty-row"><td colspan="5">No hay asignaciones.</td></tr>';
            return;
        }

        tbody.innerHTML = items.map(function (item) {
            state.itemsById[item.idProductSupplier] = item;
            return '<tr>' +
                '<td>' + escapeHtml(item.productName) + '</td>' +
                '<td>' + escapeHtml(item.supplierName) + '</td>' +
                '<td>S/ ' + Number(item.supplierCost || 0).toFixed(2) + '</td>' +
                '<td><span class="prp-tag ' + (item.isMainSupplier ? 'prp-tag--ok' : 'prp-tag--muted') + '">' + (item.isMainSupplier ? 'Si' : 'No') + '</span></td>' +
                '<td class="prp__td-actions"><div class="prp__row-actions">' +
                '<button type="button" class="prp__icon-btn prp__icon-btn--view" data-action="view" data-id="' + item.idProductSupplier + '" title="Ver detalle"><i class="bi bi-eye"></i></button>' +
                '<button type="button" class="prp__icon-btn prp__icon-btn--delete" data-action="delete" data-id="' + item.idProductSupplier + '" title="Eliminar"><i class="bi bi-trash"></i></button>' +
                '</div></td>' +
                '</tr>';
        }).join('');
    }

    function loadList() {
        var tbody = qs('prpBody');
        if (tbody) tbody.innerHTML = '<tr class="prp__loading-row"><td colspan="5">Cargando registros...</td></tr>';

        fetch(buildQuery(urls().list, { search: state.search, idProduct: state.idProduct, idSupplier: state.idSupplier, page: state.page, pageSize: state.pageSize }), { credentials: 'same-origin' })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                renderRows(Array.isArray(data.items) ? data.items : []);
                updatePagination(data);
            })
            .catch(function () {
                if (tbody) tbody.innerHTML = '<tr class="prp__empty-row"><td colspan="5">Error al cargar registros.</td></tr>';
                showToast('Error al cargar registros.', false);
            });
    }

    function resetForm() {
        var form = qs('prpForm');
        if (form) form.reset();
    }

    function saveForm() {
        var form = qs('prpForm');
        if (!form || !form.checkValidity()) {
            if (form) form.reportValidity();
            return;
        }

        var formData = new FormData(form);
        var data = new URLSearchParams();
        data.append('idProduct', formData.get('idProduct'));
        data.append('idSupplier', formData.get('idSupplier'));
        data.append('cost', formData.get('cost'));
        data.append('isMain', formData.get('isMain') === 'on');

        postAction(urls().save, data)
            .then(function (result) {
                showToast(result.message || (result.success ? 'Asignacion guardada.' : 'No se pudo guardar.'), result.success);
                if (result.success) {
                    closeModal('prpModal');
                    loadList();
                }
            })
            .catch(function () { showToast('Error al guardar.', false); });
    }

    function confirmDelete(id) {
        state.deleteId = id;
        openModal('prpConfirmModal');
    }

    function openDetail(id) {
        var item = state.itemsById[id];
        if (!item || !qs('prpDetailBody')) return;

        var rows = [
            { label: 'Producto', value: item.productName },
            { label: 'Proveedor', value: item.supplierName },
            { label: 'Costo', value: 'S/ ' + Number(item.supplierCost || 0).toFixed(2) },
            { label: 'Proveedor principal', value: item.isMainSupplier ? 'Si' : 'No' }
        ];

        qs('prpDetailBody').innerHTML = rows.map(function (row) {
            return '<div class="prp-detail__row"><span class="prp-detail__label">' + escapeHtml(row.label) + '</span><span class="prp-detail__value">' + escapeHtml(row.value) + '</span></div>';
        }).join('');
        openModal('prpDetailModal');
    }

    function deleteSelected() {
        if (!state.deleteId) return;

        var data = new URLSearchParams();
        data.append('id', state.deleteId);

        postAction(urls().delete, data)
            .then(function (result) {
                showToast(result.message || (result.success ? 'Asignacion eliminada.' : 'No se pudo eliminar.'), result.success !== false);
                state.deleteId = null;
                closeModal('prpConfirmModal');
                loadList();
            })
            .catch(function () { showToast('Error al eliminar.', false); });
    }

    function bindEvents() {
        var searchInput = qs('prpSearchInput');
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

        if (qs('prpClearSearchBtn')) qs('prpClearSearchBtn').addEventListener('click', function () {
            state.search = '';
            state.idProduct = '';
            state.idSupplier = '';
            state.page = 1;
            if (searchInput) searchInput.value = '';
            if (qs('prpProductFilter')) qs('prpProductFilter').value = '';
            if (qs('prpSupplierFilter')) qs('prpSupplierFilter').value = '';
            loadList();
        });

        if (qs('prpProductFilter')) qs('prpProductFilter').addEventListener('change', function (e) {
            state.idProduct = e.target.value;
            state.page = 1;
            loadList();
        });

        if (qs('prpSupplierFilter')) qs('prpSupplierFilter').addEventListener('change', function (e) {
            state.idSupplier = e.target.value;
            state.page = 1;
            loadList();
        });

        if (qs('prpPageSize')) qs('prpPageSize').addEventListener('change', function (event) {
            state.pageSize = parseInt(event.target.value, 10);
            state.page = 1;
            loadList();
        });

        if (qs('prpPrevBtn')) qs('prpPrevBtn').addEventListener('click', function () {
            if (state.page > 1) {
                state.page--;
                loadList();
            }
        });

        if (qs('prpNextBtn')) qs('prpNextBtn').addEventListener('click', function () {
            state.page++;
            loadList();
        });

        if (qs('prpCreateBtn')) qs('prpCreateBtn').addEventListener('click', function () {
            resetForm();
            openModal('prpModal');
        });

        if (qs('prpSaveBtn')) qs('prpSaveBtn').addEventListener('click', saveForm);
        if (qs('prpConfirmBtn')) qs('prpConfirmBtn').addEventListener('click', deleteSelected);

        document.querySelectorAll('[data-dismiss="modal"]').forEach(function (button) {
            button.addEventListener('click', closeAllModals);
        });

        document.querySelectorAll('.prp-modal__backdrop').forEach(function (backdrop) {
            backdrop.addEventListener('click', closeAllModals);
        });

        var tbody = qs('prpBody');
        if (tbody) {
            tbody.addEventListener('click', function (event) {
                var button = event.target.closest('[data-action]');
                if (!button) return;
                var id = parseInt(button.getAttribute('data-id'), 10);
                if (button.getAttribute('data-action') === 'view') openDetail(id);
                if (button.getAttribute('data-action') === 'delete') confirmDelete(id);
            });
        }
    }

    function init() {
        var root = qs('prpRoot');
        if (!root || root.dataset.initialized === 'true' || !window.prpUrls) return;
        root.dataset.initialized = 'true';

        bindEvents();
        loadFilters();
        loadList();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
