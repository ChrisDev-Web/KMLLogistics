(function () {
    'use strict';

    var state = {
        page: 1,
        pageSize: 10,
        search: '',
        idSale: '',
        idProduct: '',
        idClient: '',
        filterOptionsLoaded: false
    };
    var colCount = 9;

    function urls() { return window.dvntUrls || {}; }
    function qs(id) { return document.getElementById(id); }

    function showToast(message, isSuccess) {
        var toast = qs('dvntToast');
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'dvnt__toast is-visible ' + (isSuccess ? 'dvnt__toast--success' : 'dvnt__toast--error');
        setTimeout(function () { toast.classList.remove('is-visible'); }, 3200);
    }

    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (key) {
            if (params[key] !== undefined && params[key] !== null && params[key] !== '') url.searchParams.set(key, params[key]);
        });
        return url.toString();
    }

    function fetchJson(url, options) {
        options = options || {};
        options.headers = options.headers || {};
        options.headers['X-Requested-With'] = 'XMLHttpRequest';
        options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) {
            return r.json().then(function (data) {
                if (!r.ok) throw new Error(data.message || 'Error de red');
                return data;
            });
        });
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.textContent = text == null ? '' : String(text);
        return div.innerHTML;
    }

    function formatMoney(value) {
        if (value == null || value === '') return 'S/ 0.00';
        var num = parseFloat(value);
        return isNaN(num) ? 'S/ 0.00' : 'S/ ' + num.toFixed(2);
    }

    function formatDate(value) {
        if (!value) return '—';
        var dt = new Date(value);
        if (isNaN(dt.getTime())) return escapeHtml(value);
        return dt.toLocaleString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function populateSelect(selectId, options) {
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

    function filterParams() {
        return {
            search: state.search,
            idSale: state.idSale,
            idProduct: state.idProduct,
            idClient: state.idClient
        };
    }

    function loadFilterOptions() {
        return fetchJson(urls().filterOptions).then(function (data) {
            if (!data.success) throw new Error('Error al cargar filtros.');
            populateSelect('dvntFilterProduct', data.products || []);
            populateSelect('dvntFilterClient', data.clients || []);
            state.filterOptionsLoaded = true;
        });
    }

    function loadMetrics() {
        fetchJson(buildQuery(urls().metrics, filterParams())).then(function (res) {
            if (!res.success || !res.data) return;
            var d = res.data;
            qs('dvntMetricSales').textContent = d.saleCount != null ? d.saleCount : '—';
            qs('dvntMetricProfit').textContent = formatMoney(d.netProfit);
            qs('dvntMetricSubtotal').textContent = formatMoney(d.totalSubtotal);
            qs('dvntMetricTax').textContent = formatMoney(d.totalTax);
            qs('dvntMetricTotal').textContent = formatMoney(d.totalAmount);
        }).catch(function () { /* métricas opcionales */ });
    }

    function renderRows(tbody, items) {
        if (!tbody) return;
        if (!items || !items.length) {
            tbody.innerHTML = '<tr class="dvnt__empty-row"><td colspan="' + colCount + '">No se encontraron registros.</td></tr>';
            return;
        }
        tbody.innerHTML = items.map(function (row) {
            return '<tr><td class="dvnt__col-num">' + escapeHtml(row.idSaleDetail) + '</td>' +
                '<td>' + escapeHtml(row.saleNumber || row.idSale) + '</td>' +
                '<td>' + escapeHtml(row.productName) + '</td>' +
                '<td>' + escapeHtml(row.clientName) + '</td>' +
                '<td>' + escapeHtml(row.warehouseName) + '</td>' +
                '<td class="dvnt__col-num">' + escapeHtml(row.quantity) + '</td>' +
                '<td class="dvnt__col-money">' + formatMoney(row.unitPrice) + '</td>' +
                '<td class="dvnt__col-money">' + formatMoney(row.subtotal) + '</td>' +
                '<td>' + formatDate(row.createdAt) + '</td></tr>';
        }).join('');
    }

    function updatePagination(infoEl, prevBtn, nextBtn, page, totalPages) {
        if (infoEl) infoEl.textContent = 'Pagina ' + page + ' de ' + (totalPages || 1);
        if (prevBtn) prevBtn.disabled = page <= 1;
        if (nextBtn) nextBtn.disabled = page >= totalPages || totalPages === 0;
    }

    function reloadAll() {
        loadMetrics();
        loadList();
    }

    function loadList() {
        var tbody = qs('dvntBody');
        var scrollEl = qs('dvntTableScroll');
        var loadHtml = '<tr class="dvnt__loading-row"><td colspan="' + colCount + '">Cargando...</td></tr>';
        if (window.KmlTableList) KmlTableList.begin(scrollEl, tbody, loadHtml);
        else if (tbody) tbody.innerHTML = loadHtml;

        fetchJson(buildQuery(urls().list, Object.assign({}, filterParams(), {
            page: state.page,
            pageSize: state.pageSize
        }))).then(function (data) {
            renderRows(tbody, data.items);
            updatePagination(qs('dvntPageInfo'), qs('dvntPrevBtn'), qs('dvntNextBtn'), data.page, data.totalPages || 1);
        }).catch(function (err) {
            if (tbody) tbody.innerHTML = '<tr class="dvnt__empty-row"><td colspan="' + colCount + '">Error al cargar.</td></tr>';
            showToast(err.message || 'Error al cargar.', false);
        }).finally(function () {
            if (window.KmlTableList) KmlTableList.end(scrollEl);
        });
    }

    function bindEvents() {
        var searchInput = qs('dvntSearchInput');
        var saleInput = qs('dvntFilterSale');
        var searchTimer;
        var saleTimer;

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearTimeout(searchTimer);
                searchTimer = setTimeout(function () {
                    state.search = searchInput.value.trim();
                    state.page = 1;
                    reloadAll();
                }, 350);
            });
        }

        if (saleInput) {
            saleInput.addEventListener('input', function () {
                clearTimeout(saleTimer);
                saleTimer = setTimeout(function () {
                    state.idSale = saleInput.value.trim();
                    state.page = 1;
                    reloadAll();
                }, 350);
            });
        }

        qs('dvntClearSearchBtn')?.addEventListener('click', function () {
            state.search = '';
            if (searchInput) searchInput.value = '';
            state.page = 1;
            reloadAll();
        });

        qs('dvntFilterProduct')?.addEventListener('change', function (e) {
            state.idProduct = e.target.value;
            state.page = 1;
            reloadAll();
        });

        qs('dvntFilterClient')?.addEventListener('change', function (e) {
            state.idClient = e.target.value;
            state.page = 1;
            reloadAll();
        });

        qs('dvntPageSize')?.addEventListener('change', function (e) {
            state.pageSize = parseInt(e.target.value, 10);
            state.page = 1;
            loadList();
        });

        qs('dvntPrevBtn')?.addEventListener('click', function () {
            if (state.page > 1) { state.page--; loadList(); }
        });

        qs('dvntNextBtn')?.addEventListener('click', function () {
            state.page++;
            loadList();
        });
    }

    function init() {
        var root = qs('dvntRoot');
        if (!root || root.dataset.initialized === 'true' || !window.dvntUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions()
            .then(reloadAll)
            .catch(function () { reloadAll(); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
