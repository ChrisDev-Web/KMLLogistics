(function () {
    'use strict';

    function urls() { return window.veUrls || {}; }
    function qs(id) { return document.getElementById(id); }

    function fillSelect(select, items, valueKey, textKey, placeholder) {
        if (!select) return;
        select.innerHTML = '<option value="">' + escapeHtml(placeholder || 'Seleccione...') + '</option>'
            + (items || []).map(function (item) {
                return '<option value="' + item[valueKey] + '">' + escapeHtml(item[textKey]) + '</option>';
            }).join('');
    }

    function escapeHtml(t) {
        var d = document.createElement('div');
        d.textContent = t == null ? '' : String(t);
        return d.innerHTML;
    }

    function setHint(text) {
        var hint = qs('veSaleHint');
        if (hint) hint.textContent = text || '';
    }

    function ensureHint() {
        if (qs('veSaleHint') || !qs('formFields')) return;
        var p = document.createElement('p');
        p.id = 'veSaleHint';
        p.className = 'crud-field-hint';
        p.style.cssText = 'grid-column:1/-1;font-size:.8rem;color:#64748b;margin:0;';
        qs('formFields').appendChild(p);
    }

    function reloadSales(shipmentId) {
        var saleSelect = qs('field_saleId');
        if (!saleSelect) return;
        ensureHint();

        if (!shipmentId) {
            fillSelect(saleSelect, [], 'idSale', 'name', 'Seleccione envio primero...');
            setHint('Primero elija el envio. Solo aparecen ventas empaquetadas y no asignadas a otro envio.');
            return;
        }

        var base = urls().available || '/VentasEnvio/AvailableSales';
        fetch(base + '?shipmentId=' + encodeURIComponent(shipmentId), {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                var items = data.items || [];
                fillSelect(saleSelect, items, 'idSale', 'name', items.length ? 'Seleccione venta...' : 'Sin ventas disponibles');
                if (!items.length) {
                    setHint('No hay ventas disponibles: deben estar empaquetadas en Detalle de caja y no estar ya en otro envio.');
                } else {
                    setHint('Ventas empaquetadas listas para asociar a este envio (calcula ruta y llegada).');
                }
            })
            .catch(function () {
                fillSelect(saleSelect, [], 'idSale', 'name', 'Error al cargar');
                setHint('Error al cargar ventas disponibles.');
            });
    }

    function bindShipmentSelect() {
        var shipmentSelect = qs('field_shipmentId');
        if (!shipmentSelect || shipmentSelect.dataset.veBound === 'true') return;
        shipmentSelect.dataset.veBound = 'true';
        shipmentSelect.addEventListener('change', function () { reloadSales(this.value); });
    }

    function bindCreateBtn() {
        var btn = qs('btnCreate');
        if (!btn || btn.dataset.veBound === 'true') return;
        btn.dataset.veBound = 'true';
        btn.addEventListener('click', function () {
            setTimeout(function () {
                ensureHint();
                bindShipmentSelect();
                var shipmentSelect = qs('field_shipmentId');
                reloadSales(shipmentSelect ? shipmentSelect.value : null);
            }, 150);
        });
    }

    function init() {
        bindCreateBtn();
        bindShipmentSelect();
        var form = qs('formFields');
        if (form) {
            new MutationObserver(function () {
                bindShipmentSelect();
                ensureHint();
            }).observe(form, { childList: true, subtree: true });
        }
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
