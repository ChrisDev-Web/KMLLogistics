(function () {
    'use strict';

    var state = {
        search: '',
        idCategory: '',
        idBrand: '',
        cart: [],
        clients: [],
        paymentMethods: [],
        employee: null,
        checkoutBusy: false
    };

    function urls() { return window.lvntUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function getToken() { var i = document.querySelector('#lvntRoot input[name="__RequestVerificationToken"]'); return i ? i.value : ''; }

    function showToast(msg, ok) {
        var t = qs('lvntToast');
        if (!t) return;
        t.textContent = msg;
        t.className = 'lvnt__toast is-visible ' + (ok ? 'lvnt__toast--success' : 'lvnt__toast--error');
        setTimeout(function () { t.classList.remove('is-visible'); }, 3200);
    }

    function buildQuery(baseUrl, params) {
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (k) {
            if (params[k] !== undefined && params[k] !== null && params[k] !== '') url.searchParams.set(k, params[k]);
        });
        return url.toString();
    }

    function fetchJson(url, options) {
        options = options || {};
        options.headers = options.headers || {};
        options.headers['X-Requested-With'] = 'XMLHttpRequest';
        options.credentials = 'same-origin';
        return fetch(url, options).then(function (r) {
            return r.json().then(function (d) {
                if (!r.ok) throw new Error(d.message || 'Error de red');
                return d;
            });
        });
    }

    function postCheckout(data) {
        var token = getToken();
        var body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        Object.keys(data).forEach(function (k) {
            if (data[k] !== undefined && data[k] !== null) body.append(k, data[k]);
        });
        return fetchJson(urls().checkout, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
            body: body.toString()
        });
    }

    function escapeHtml(text) {
        var d = document.createElement('div');
        d.textContent = text == null ? '' : String(text);
        return d.innerHTML;
    }

    function money(v) {
        var n = parseFloat(v);
        return 'S/ ' + (isNaN(n) ? '0.00' : n.toFixed(2));
    }

    function config() { return window.lvntConfig || {}; }

    function resolvePhotoUrl(photo) {
        if (!photo) return '';
        var value = String(photo).trim();
        if (!value) return '';
        if (/^(https?:)?\/\//i.test(value) || /^data:/i.test(value)) return value;
        if (value.charAt(0) === '/') return value;
        if (/^Public\//i.test(value)) return '/' + value;
        var base = config().productImageBase || '/Public/Images/Products/';
        return base + value.replace(/^\/+/, '');
    }

    function handlePhotoError(img) {
        var wrap = img.parentElement;
        if (!wrap) return;
        wrap.classList.add('lvnt-product__photo--empty');
        img.remove();
        var span = wrap.querySelector('span');
        if (span) span.style.display = 'flex';
    }

    function renderProductPhoto(photo, name) {
        var url = resolvePhotoUrl(photo);
        var initial = (name || '?').trim().charAt(0).toUpperCase() || '?';
        if (!url) {
            return '<div class="lvnt-product__photo lvnt-product__photo--empty"><span>' + escapeHtml(initial) + '</span></div>';
        }
        return '<div class="lvnt-product__photo">' +
            '<img src="' + escapeHtml(url) + '" alt="' + escapeHtml(name) + '" loading="lazy" onerror="handlePhotoError(this)" />' +
            '<span>' + escapeHtml(initial) + '</span></div>';
    }

    window.handlePhotoError = handlePhotoError;

    function populateSelect(selectId, options, placeholder) {
        var sel = qs(selectId);
        if (!sel) return;
        var cur = sel.value;
        sel.innerHTML = '<option value="">' + (placeholder || 'Seleccione...') + '</option>';
        (options || []).forEach(function (o) {
            var opt = document.createElement('option');
            opt.value = o.id;
            opt.textContent = o.name;
            sel.appendChild(opt);
        });
        if (cur) sel.value = cur;
    }

    function getCartTotals() {
        var subtotal = state.cart.reduce(function (sum, line) {
            return sum + (line.salePrice * line.quantity);
        }, 0);
        var tax = Math.round(subtotal * 0.18 * 100) / 100;
        var total = Math.round((subtotal + tax) * 100) / 100;
        return { subtotal: subtotal, tax: tax, total: total };
    }

    function findCartLine(idProduct) {
        for (var i = 0; i < state.cart.length; i++) {
            if (state.cart[i].idProduct === idProduct) return state.cart[i];
        }
        return null;
    }

    function updateTotalsUI() {
        var t = getCartTotals();
        if (qs('lvntSubtotal')) qs('lvntSubtotal').textContent = money(t.subtotal);
        if (qs('lvntTax')) qs('lvntTax').textContent = money(t.tax);
        if (qs('lvntTotal')) qs('lvntTotal').textContent = money(t.total);
        updateChangeField();
        updateCheckoutButton();
    }

    function isCashPayment() {
        var sel = qs('lvntPayment');
        if (!sel || !sel.value) return false;
        var pm = state.paymentMethods.find(function (p) { return String(p.id) === sel.value; });
        return pm && pm.name.toLowerCase().indexOf('efectivo') >= 0;
    }

    function updateChangeField() {
        var cashFields = qs('lvntCashFields');
        var amountPaidEl = qs('lvntAmountPaid');
        var changeEl = qs('lvntChange');
        var cash = isCashPayment();
        if (cashFields) cashFields.hidden = !cash;
        if (!cash) {
            if (changeEl) changeEl.value = '';
            return;
        }
        var total = getCartTotals().total;
        var paid = parseFloat(amountPaidEl && amountPaidEl.value ? amountPaidEl.value : '0');
        if (changeEl) changeEl.value = (!isNaN(paid) && paid >= total) ? (paid - total).toFixed(2) : '';
    }

    function updateCheckoutButton() {
        var btn = qs('lvntCheckoutBtn');
        if (!btn) return;
        var hasCart = state.cart.length > 0;
        var hasClient = qs('lvntClient') && qs('lvntClient').value;
        var hasPayment = qs('lvntPayment') && qs('lvntPayment').value;
        var validCash = true;
        if (isCashPayment()) {
            var total = getCartTotals().total;
            var paid = parseFloat(qs('lvntAmountPaid') && qs('lvntAmountPaid').value ? qs('lvntAmountPaid').value : '0');
            validCash = !isNaN(paid) && paid >= total;
        }
        btn.disabled = state.checkoutBusy || !hasCart || !hasClient || !hasPayment || !validCash || !state.employee;
    }

    function updateClientDocInfo() {
        var sel = qs('lvntClient');
        var info = qs('lvntDocInfo');
        if (!sel || !info) return;
        var client = state.clients.find(function (c) { return String(c.id) === sel.value; });
        if (!client) {
            info.textContent = 'Tipo comprobante: —';
            return;
        }
        var receipt = client.receiptType === 'FACTURA' ? 'Factura' : 'Boleta';
        info.innerHTML = '<strong>' + escapeHtml(client.documentTypeName) + ':</strong> ' + escapeHtml(client.documentNumber) +
            ' · <strong>Comprobante:</strong> ' + escapeHtml(receipt);
        updateCheckoutButton();
    }

    function renderCart() {
        var body = qs('lvntCartBody');
        if (!body) return;
        if (!state.cart.length) {
            body.innerHTML = '<p class="lvnt__cart-empty">Sin productos en el carrito.</p>';
            updateTotalsUI();
            return;
        }
        body.innerHTML = state.cart.map(function (line) {
            return '<div class="lvnt-cart-item" data-id="' + line.idProduct + '">' +
                '<div class="lvnt-cart-item__head">' +
                '<span class="lvnt-cart-item__id">#' + escapeHtml(line.idProduct) + '</span>' +
                '<button type="button" class="lvnt-cart-item__remove" data-action="remove" data-id="' + line.idProduct + '" title="Eliminar"><i class="bi bi-trash"></i></button>' +
                '</div>' +
                '<div class="lvnt-cart-item__name">' + escapeHtml(line.name) + '</div>' +
                '<div class="lvnt-cart-item__price">' + money(line.salePrice) + '</div>' +
                '<div class="lvnt-cart-item__qty">' +
                '<button type="button" class="lvnt-qty-btn" data-action="dec" data-id="' + line.idProduct + '">−</button>' +
                '<span>' + escapeHtml(line.quantity) + '</span>' +
                '<button type="button" class="lvnt-qty-btn" data-action="inc" data-id="' + line.idProduct + '">+</button>' +
                '</div></div>';
        }).join('');
        updateTotalsUI();
    }

    function addToCart(product) {
        if (!product || product.stock <= 0) {
            showToast('Producto sin stock disponible.', false);
            return;
        }
        var line = findCartLine(product.idProduct);
        if (line) {
            if (line.quantity >= product.stock) {
                showToast('Stock insuficiente.', false);
                return;
            }
            line.quantity += 1;
        } else {
            state.cart.push({
                idProduct: product.idProduct,
                idWarehouse: product.idWarehouse,
                name: product.name,
                salePrice: product.salePrice,
                quantity: 1,
                stock: product.stock
            });
        }
        renderCart();
    }

    function changeQty(idProduct, delta) {
        var line = findCartLine(idProduct);
        if (!line) return;
        var next = line.quantity + delta;
        if (next <= 0) {
            state.cart = state.cart.filter(function (l) { return l.idProduct !== idProduct; });
        } else if (next > line.stock) {
            showToast('Stock insuficiente.', false);
            return;
        } else {
            line.quantity = next;
        }
        renderCart();
    }

    function renderProducts(items) {
        var container = qs('lvntProducts');
        if (!container) return;
        if (!items || !items.length) {
            container.innerHTML = '<div class="lvnt__empty">No se encontraron productos con stock.</div>';
            return;
        }
        container.innerHTML = items.map(function (p) {
            var disabled = p.stock <= 0 ? ' disabled' : '';
            return '<article class="lvnt-product' + (p.stock <= 0 ? ' lvnt-product--out' : '') + '">' +
                renderProductPhoto(p.photo, p.name) +
                '<div class="lvnt-product__body">' +
                '<span class="lvnt-product__id">#' + escapeHtml(p.idProduct) + '</span>' +
                '<h4 class="lvnt-product__name">' + escapeHtml(p.name) + '</h4>' +
                '<div class="lvnt-product__meta">' +
                '<span class="lvnt-product__price">' + money(p.salePrice) + '</span>' +
                '<span class="lvnt-product__stock">Stock: ' + escapeHtml(p.stock) + '</span>' +
                '</div>' +
                '<button type="button" class="lvnt__btn lvnt__btn--primary lvnt__btn--sm" data-action="add" data-id="' + p.idProduct + '"' + disabled + '>' +
                '<i class="bi bi-plus-lg"></i> Agregar</button>' +
                '</div></article>';
        }).join('');
        container._products = items;
    }

    function loadProducts() {
        var container = qs('lvntProducts');
        if (container) container.innerHTML = '<div class="lvnt__loading">Cargando productos...</div>';
        fetchJson(buildQuery(urls().products, {
            search: state.search,
            idCategory: state.idCategory,
            idBrand: state.idBrand
        })).then(function (data) {
            renderProducts(data.items || []);
        }).catch(function (err) {
            if (container) container.innerHTML = '<div class="lvnt__empty">Error al cargar productos.</div>';
            showToast(err.message || 'Error al cargar productos.', false);
        });
    }

    function loadFilterOptions() {
        return Promise.all([
            fetchJson(urls().categoryFilters).then(function (d) {
                var sel = qs('lvntCategoryFilter');
                if (!sel) return;
                var cur = sel.value;
                sel.innerHTML = '<option value="">Todas las categorías</option>';
                (d.items || []).forEach(function (o) {
                    var opt = document.createElement('option');
                    opt.value = o.id;
                    opt.textContent = o.name;
                    sel.appendChild(opt);
                });
                if (cur) sel.value = cur;
            }),
            fetchJson(urls().brandFilters).then(function (d) {
                var sel = qs('lvntBrandFilter');
                if (!sel) return;
                var cur = sel.value;
                sel.innerHTML = '<option value="">Todas las marcas</option>';
                (d.items || []).forEach(function (o) {
                    var opt = document.createElement('option');
                    opt.value = o.id;
                    opt.textContent = o.name;
                    sel.appendChild(opt);
                });
                if (cur) sel.value = cur;
            })
        ]);
    }

    function loadInitData() {
        return fetchJson(urls().initData).then(function (data) {
            if (!data.success) throw new Error(data.message || 'No se pudo inicializar el POS.');
            state.employee = data.employee;
            state.clients = data.clients || [];
            state.paymentMethods = data.paymentMethods || [];
            populateSelect('lvntClient', state.clients.map(function (c) {
                return { id: c.id, name: c.name + ' (' + c.documentNumber + ')' };
            }), 'Seleccione cliente...');
            populateSelect('lvntPayment', state.paymentMethods, 'Seleccione...');
            updateCheckoutButton();
        });
    }

    function renderVoucherInWindow(printWin, voucherUrl) {
        if (!printWin || printWin.closed) {
            showToast('Permita ventanas emergentes para imprimir el comprobante.', false);
            return Promise.resolve(false);
        }
        return fetch(voucherUrl, {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (r) {
            if (!r.ok) throw new Error('No se pudo cargar el comprobante.');
            return r.text();
        }).then(function (html) {
            printWin.document.open();
            printWin.document.write(html);
            printWin.document.close();
            printWin.focus();
            return true;
        }).catch(function (err) {
            if (!printWin.closed) printWin.close();
            showToast(err.message || 'Error al abrir el comprobante.', false);
            return false;
        });
    }

    function checkout() {
        if (state.checkoutBusy) return;
        var clientId = qs('lvntClient') && qs('lvntClient').value;
        var paymentId = qs('lvntPayment') && qs('lvntPayment').value;
        if (!clientId || !paymentId || !state.cart.length) return;

        var totals = getCartTotals();
        var amountPaid = null;
        var changeAmount = null;
        if (isCashPayment()) {
            amountPaid = parseFloat(qs('lvntAmountPaid').value);
            if (isNaN(amountPaid) || amountPaid < totals.total) {
                showToast('El pago en efectivo no puede ser menor al total.', false);
                return;
            }
            changeAmount = Math.round((amountPaid - totals.total) * 100) / 100;
        }

        var details = state.cart.map(function (l) {
            return {
                idProduct: l.idProduct,
                idWarehouse: l.idWarehouse,
                quantity: l.quantity,
                unitPrice: l.salePrice
            };
        });

        state.checkoutBusy = true;
        updateCheckoutButton();
        var btn = qs('lvntCheckoutBtn');
        if (btn) btn.textContent = 'Procesando...';

        var printWin = window.open('', '_blank');

        postCheckout({
            idClient: clientId,
            idPaymentMethod: paymentId,
            detailsJson: JSON.stringify(details),
            amountPaid: amountPaid,
            changeAmount: changeAmount
        }).then(function (res) {
            if (!res.success) {
                if (printWin && !printWin.closed) printWin.close();
                showToast(res.message || 'No se pudo registrar la venta.', false);
                return;
            }
            showToast(res.message || 'Venta registrada correctamente.', true);
            state.cart = [];
            if (qs('lvntAmountPaid')) qs('lvntAmountPaid').value = '';
            renderCart();
            loadProducts();
            if (res.voucherUrl) {
                renderVoucherInWindow(printWin, res.voucherUrl);
            } else if (printWin && !printWin.closed) {
                printWin.close();
            }
        }).catch(function (err) {
            if (printWin && !printWin.closed) printWin.close();
            showToast(err.message || 'Error al confirmar la venta.', false);
        }).finally(function () {
            state.checkoutBusy = false;
            if (btn) btn.innerHTML = '<i class="bi bi-receipt"></i> Confirmar venta';
            updateCheckoutButton();
        });
    }

    function bindEvents() {
        var searchInput = qs('lvntSearchInput');
        var searchTimer;
        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearTimeout(searchTimer);
                searchTimer = setTimeout(function () {
                    state.search = searchInput.value.trim();
                    loadProducts();
                }, 350);
            });
        }
        qs('lvntClearSearchBtn')?.addEventListener('click', function () {
            state.search = '';
            if (searchInput) searchInput.value = '';
            loadProducts();
        });
        qs('lvntCategoryFilter')?.addEventListener('change', function (e) {
            state.idCategory = e.target.value;
            loadProducts();
        });
        qs('lvntBrandFilter')?.addEventListener('change', function (e) {
            state.idBrand = e.target.value;
            loadProducts();
        });
        qs('lvntProducts')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action="add"]');
            if (!btn || btn.disabled) return;
            var id = parseInt(btn.getAttribute('data-id'), 10);
            var products = qs('lvntProducts')._products || [];
            var product = products.find(function (p) { return p.idProduct === id; });
            if (product) addToCart(product);
        });
        qs('lvntCartBody')?.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-action]');
            if (!btn) return;
            var id = parseInt(btn.getAttribute('data-id'), 10);
            var action = btn.getAttribute('data-action');
            if (action === 'remove') {
                state.cart = state.cart.filter(function (l) { return l.idProduct !== id; });
                renderCart();
            } else if (action === 'inc') changeQty(id, 1);
            else if (action === 'dec') changeQty(id, -1);
        });
        qs('lvntClient')?.addEventListener('change', updateClientDocInfo);
        qs('lvntPayment')?.addEventListener('change', function () {
            updateChangeField();
            updateCheckoutButton();
        });
        qs('lvntAmountPaid')?.addEventListener('input', function () {
            updateChangeField();
            updateCheckoutButton();
        });
        qs('lvntCheckoutBtn')?.addEventListener('click', checkout);
    }

    function init() {
        var root = qs('lvntRoot');
        if (!root || root.dataset.initialized === 'true' || !window.lvntUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        loadFilterOptions()
            .then(loadInitData)
            .then(loadProducts)
            .catch(function (err) { showToast(err.message || 'Error al iniciar ventas.', false); });
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
