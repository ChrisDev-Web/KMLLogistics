(function () {
    'use strict';

    function urls() { return window.ceUrls || {}; }
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
        var hint = qs('ceBoxHint');
        if (hint) hint.textContent = text || '';
    }

    function ensureHint() {
        if (qs('ceBoxHint') || !qs('formFields')) return;
        var p = document.createElement('p');
        p.id = 'ceBoxHint';
        p.className = 'crud-field-hint';
        p.style.cssText = 'grid-column:1/-1;font-size:.8rem;color:#64748b;margin:0;';
        qs('formFields').appendChild(p);
    }

    function reloadBoxes(shipmentId) {
        var boxSelect = qs('field_boxId');
        if (!boxSelect) return;
        ensureHint();

        if (!shipmentId) {
            fillSelect(boxSelect, [], 'idBox', 'name', 'Seleccione envio primero...');
            setHint('Primero elija el envio; luego se cargan las cajas empaquetadas disponibles.');
            return;
        }

        var availableUrl = (urls().available || '/CajasEnvio/AvailableBoxes') + '?shipmentId=' + encodeURIComponent(shipmentId);
        fetch(availableUrl, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success === false) {
                    fillSelect(boxSelect, [], 'idBox', 'name', 'No disponible');
                    setHint(data.message || 'No se pueden modificar cajas en este envio.');
                    return;
                }
                var items = data.items || [];
                fillSelect(boxSelect, items, 'idBox', 'name', items.length ? 'Seleccione caja...' : 'Sin cajas libres');
                setHint(data.message || '');
            })
            .catch(function () {
                fillSelect(boxSelect, [], 'idBox', 'name', 'Error al cargar');
                setHint('Error al cargar cajas disponibles.');
            });
    }

    function bindShipmentSelect() {
        var shipmentSelect = qs('field_shipmentId');
        if (!shipmentSelect || shipmentSelect.dataset.ceBound === 'true') return;
        shipmentSelect.dataset.ceBound = 'true';
        shipmentSelect.addEventListener('change', function () { reloadBoxes(this.value); });
    }

    function bindCreateBtn() {
        var btn = qs('btnCreate');
        if (!btn || btn.dataset.ceBound === 'true') return;
        btn.dataset.ceBound = 'true';
        btn.addEventListener('click', function () {
            setTimeout(function () {
                ensureHint();
                bindShipmentSelect();
                var shipmentSelect = qs('field_shipmentId');
                reloadBoxes(shipmentSelect ? shipmentSelect.value : null);
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
