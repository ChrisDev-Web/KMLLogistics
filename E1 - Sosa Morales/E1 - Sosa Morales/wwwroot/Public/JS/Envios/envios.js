(function () {
    'use strict';

    var currentPage = 1;
    var totalPages = 1;
    var pageSize = 10;
    var confirmCallback = null;
    var optionData = {};
    var currentShipment = null;

    var LOCKED_STATUSES = ['En Transito', 'Entregado', 'Cancelado'];

    function cfg() { return window.kmlCrudConfig; }
    function boxUrls() { return window.envBoxUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function token() { return document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''; }

    function init() {
        var card = qs('envRoot');
        var config = cfg();
        if (!card || !config || card.dataset.crudInitialized === 'true') return;
        card.dataset.crudInitialized = 'true';
        currentPage = 1;
        optionData = {};
        loadOptions().then(function () {
            buildTableHead();
            buildForm();
            bindEvents();
            loadData(1);
        });
    }

    function bindEvents() {
        qs('txtSearch')?.addEventListener('keyup', function (e) { if (e.key === 'Enter') loadData(1); });
        qs('btnBuscar')?.addEventListener('click', function () { loadData(1); });
        qs('btnCreate')?.addEventListener('click', openCreateModal);
        qs('btnPrev')?.addEventListener('click', function () { changePage(-1); });
        qs('btnNext')?.addEventListener('click', function () { changePage(1); });
        qs('btnSave')?.addEventListener('click', saveItem);
        qs('btnCancel')?.addEventListener('click', closeModal);
        qs('btnCloseView')?.addEventListener('click', closeViewModal);
        qs('btnCloseMessage')?.addEventListener('click', closeMessageModal);
        qs('btnCancelConfirm')?.addEventListener('click', closeConfirmModal);
        qs('btnConfirm')?.addEventListener('click', executeConfirmAction);
        qs('btnAddBox')?.addEventListener('click', addBoxToShipment);
    }

    function canModifyBoxes(item) {
        if (!item) return false;
        return LOCKED_STATUSES.indexOf(item.shipmentStatusName) < 0;
    }

    function loadOptions() {
        var config = cfg();
        if (!config?.optionsUrl) return Promise.resolve();
        return fetch(config.optionsUrl, { credentials: 'same-origin' })
            .then(function (r) { return r.ok ? r.json() : {}; })
            .then(function (data) { optionData = data || {}; })
            .catch(function () { optionData = {}; });
    }

    function buildTableHead() {
        var config = cfg();
        qs('tableHead').innerHTML = '<tr>' + config.columns.map(function (c) {
            return '<th>' + c.label + '</th>';
        }).join('') + '<th>Acciones</th></tr>';
    }

    function buildForm() {
        var config = cfg();
        qs('formFields').innerHTML = config.fields.map(function (field) {
            if (field.type === 'select') {
                return '<label>' + field.label + '</label><select id="field_' + field.name + '">' + optionsHtml(field) + '</select>';
            }
            return '<label>' + field.label + '</label><input id="field_' + field.name + '" type="' + (field.type || 'text') + '" step="0.01" />';
        }).join('');
    }

    function optionsHtml(field) {
        var list = optionData[field.optionsKey] || [];
        return '<option value="">Seleccione...</option>' + list.map(function (o) {
            return '<option value="' + o[field.valueKey] + '">' + escapeHtml(o[field.textKey]) + '</option>';
        }).join('');
    }

    function loadData(page) {
        var config = cfg();
        currentPage = page;
        var params = new URLSearchParams({ search: qs('txtSearch')?.value || '', page: page, pageSize: pageSize });
        fetch(config.listUrl + '?' + params.toString(), { credentials: 'same-origin' })
            .then(function (r) {
                if (!r.ok) throw new Error('Error al cargar');
                return r.json();
            })
            .then(function (data) {
                totalPages = data.totalPages || 1;
                renderTable(data.items || []);
                qs('pageInfo').innerText = 'Pagina ' + (data.page || 1) + ' de ' + totalPages;
            })
            .catch(function () { showMessageModal('Error', 'Ocurrio un error al cargar los registros.'); });
    }

    function renderTable(items) {
        var config = cfg();
        var tbody = qs('tableBody');
        if (!items.length) {
            tbody.innerHTML = '<tr><td colspan="' + (config.columns.length + 1) + '" class="empty">No hay registros.</td></tr>';
            return;
        }
        tbody.innerHTML = items.map(function (item) {
            return '<tr>' + config.columns.map(function (c) {
                return '<td>' + formatValue(item[c.key], c) + '</td>';
            }).join('') + '<td class="actions">' + actionsHtml(item) + '</td></tr>';
        }).join('');
        tbody.querySelectorAll('[data-action]').forEach(function (btn) {
            btn.addEventListener('click', function () { handleAction(btn.dataset.action, btn.dataset.id); });
        });
    }

    function actionsHtml(item) {
        var config = cfg();
        var id = item[config.idKey];
        var html = config.getUrl ? '<button data-action="view" data-id="' + id + '" type="button">Ver</button>' : '';
        if (config.updateUrl) html += '<button data-action="edit" data-id="' + id + '" type="button">Editar</button>';
        if (config.deleteUrl) html += '<button data-action="delete" data-id="' + id + '" class="btn-danger" type="button">Eliminar</button>';
        return html;
    }

    function handleAction(action, id) {
        var config = cfg();
        if (action === 'view') return viewItem(id);
        if (action === 'edit') return openEditModal(id);
        if (action === 'delete') {
            showConfirmModal('Confirmar accion', 'Deseas eliminar este envio?', function () {
                postAction(config.deleteUrl, id);
            });
        }
    }

    function fetchItem(id) {
        var config = cfg();
        return fetch(config.getUrl + '?id=' + id, { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (json) { return json.success ? json.data : null; });
    }

    function toInputDate(value) {
        if (!value) return '';
        var d = new Date(value);
        if (isNaN(d.getTime())) return '';
        var pad = function (n) { return String(n).padStart(2, '0'); };
        return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate())
            + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes());
    }

    function viewItem(id) {
        var config = cfg();
        fetchItem(id).then(function (item) {
            if (!item) return showMessageModal('Error', 'No se pudo obtener el registro.');
            currentShipment = item;
            qs('detailBody').innerHTML = config.detailColumns.map(function (c) {
                return '<div class="detail-item"><span>' + c.label + '</span><strong>' + formatValue(item[c.key], c) + '</strong></div>';
            }).join('');
            loadViewBoxes(id);
            qs('modalView').style.display = 'flex';
        });
    }

    function openEditModal(id) {
        var config = cfg();
        fetchItem(id).then(function (item) {
            if (!item) return showMessageModal('Error', 'No se pudo obtener el registro.');
            currentShipment = item;
            qs('modalTitle').innerText = config.editTitle || 'Editar';
            qs('txtId').value = id;
            config.fields.forEach(function (f) {
                var el = qs('field_' + f.name);
                if (!el) return;
                var val = item[f.dataKey || f.name];
                el.value = f.type === 'datetime-local' ? toInputDate(val) : (val ?? '');
            });
            qs('envBoxesPanel').classList.remove('env-boxes--hidden');
            loadEditBoxes(item);
            qs('modalForm').style.display = 'flex';
        });
    }

    function openCreateModal() {
        var config = cfg();
        currentShipment = null;
        qs('modalTitle').innerText = config.createTitle || 'Nuevo';
        qs('txtId').value = '';
        config.fields.forEach(function (f) { var el = qs('field_' + f.name); if (el) el.value = ''; });
        qs('envBoxesPanel').classList.add('env-boxes--hidden');
        qs('modalForm').style.display = 'flex';
    }

    function saveItem() {
        var config = cfg();
        var id = qs('txtId').value;
        var form = new FormData();
        form.append('__RequestVerificationToken', token());
        if (id) form.append('id', id);
        config.fields.forEach(function (f) { form.append(f.name, qs('field_' + f.name).value); });
        var url = id ? config.updateUrl : config.createUrl;
        fetch(url, { method: 'POST', body: form, credentials: 'same-origin' })
            .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, json: j }; }); })
            .then(function (res) {
                if (!res.ok || !res.json.success) {
                    showMessageModal('Error', res.json.message || 'No se pudo completar la operacion.');
                    return;
                }
                if (id) {
                    showMessageModal('Operacion exitosa', res.json.message || 'Envio actualizado.');
                    fetchItem(id).then(function (item) {
                        if (item) { currentShipment = item; loadEditBoxes(item); }
                    });
                    loadData(currentPage);
                } else {
                    closeModal();
                    showMessageModal('Operacion exitosa', res.json.message || 'Envio creado. Editalo para asignar cajas.');
                    loadData(1);
                }
            })
            .catch(function () { showMessageModal('Error', 'No se pudo guardar.'); });
    }

    function postAction(url, id) {
        var form = new FormData();
        form.append('__RequestVerificationToken', token());
        form.append('id', id);
        fetch(url, { method: 'POST', body: form, credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (json) {
                showMessageModal(json.success ? 'Operacion exitosa' : 'Error', json.message || '');
                loadData(1);
            });
    }

    function updateCapacityBars(item) {
        var usedW = Number(item.usedWeight) || 0;
        var maxW = Number(item.maximumWeight) || 0;
        var usedV = Number(item.usedVolume) || 0;
        var maxV = Number(item.maximumVolume) || 0;
        var wPct = maxW > 0 ? Math.min(100, (usedW / maxW) * 100) : 0;
        var vPct = maxV > 0 ? Math.min(100, (usedV / maxV) * 100) : 0;

        var wBar = qs('envWeightBar');
        var vBar = qs('envVolumeBar');
        if (wBar) { wBar.style.width = wPct + '%'; wBar.classList.toggle('is-over', maxW > 0 && usedW > maxW); }
        if (vBar) { vBar.style.width = vPct + '%'; vBar.classList.toggle('is-over', maxV > 0 && usedV > maxV); }
        if (qs('envWeightLabel')) qs('envWeightLabel').textContent = fmtNum(usedW) + ' / ' + fmtNum(maxW) + ' kg';
        if (qs('envVolumeLabel')) qs('envVolumeLabel').textContent = fmtNum(usedV) + ' / ' + fmtNum(maxV) + ' m³';
        if (qs('envCapacityText')) qs('envCapacityText').textContent = (item.vehiclePlate || '') + ' · ' + (item.vehicleTypeName || '');
    }

    function renderBoxList(containerId, items, editable) {
        var wrap = qs(containerId);
        if (!wrap) return;
        if (!items || !items.length) {
            wrap.innerHTML = '<div class="env-boxes__empty">Sin cajas asignadas.</div>';
            return;
        }
        wrap.innerHTML = items.map(function (row) {
            var removeBtn = editable
                ? '<button type="button" class="btn-danger" data-remove-box="' + row.idShipmentBox + '">Quitar</button>'
                : '';
            return '<div class="env-box-item">'
                + '<div><strong>Caja #' + escapeHtml(row.idBox) + '</strong>'
                + '<div class="env-box-item__meta">' + fmtNum(row.weight) + ' kg · ' + fmtNum(row.volume) + ' m³</div></div>'
                + '<div class="env-box-item__actions">' + removeBtn + '</div></div>';
        }).join('');

        if (editable) {
            wrap.querySelectorAll('[data-remove-box]').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    removeBoxFromShipment(btn.getAttribute('data-remove-box'));
                });
            });
        }
    }

    function loadShipmentBoxes(shipmentId) {
        var params = new URLSearchParams({ shipmentId: shipmentId, page: 1, pageSize: 100 });
        return fetch(boxUrls().list + '?' + params.toString(), { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (data) { return data.items || []; });
    }

    function loadAvailableBoxes(shipmentId) {
        return fetch(boxUrls().available + '?shipmentId=' + shipmentId, { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (data) { return data; });
    }

    function loadEditBoxes(item) {
        if (!item) return;
        updateCapacityBars(item);
        var editable = canModifyBoxes(item);
        var addRow = qs('envAddBoxRow');
        var hint = qs('envBoxesHint');
        if (addRow) addRow.style.display = editable ? 'flex' : 'none';
        if (hint) {
            hint.textContent = editable
                ? 'Solo cajas con productos empaquetados y libres de otros envios.'
                : 'Este envio no permite agregar o quitar cajas en su estado actual.';
        }

        loadShipmentBoxes(item.idShipment).then(function (boxes) {
            renderBoxList('envBoxesList', boxes, editable);
        });

        if (!editable) {
            if (qs('envBoxSelect')) qs('envBoxSelect').innerHTML = '<option value="">No disponible</option>';
            return;
        }

        loadAvailableBoxes(item.idShipment).then(function (data) {
            var sel = qs('envBoxSelect');
            if (!sel) return;
            if (data.success === false) {
                sel.innerHTML = '<option value="">No disponible</option>';
                if (hint) hint.textContent = data.message || 'No se pueden modificar cajas.';
                return;
            }
            var items = data.items || [];
            sel.innerHTML = '<option value="">Seleccione caja disponible...</option>'
                + items.map(function (b) {
                    return '<option value="' + b.idBox + '">' + escapeHtml(b.name) + '</option>';
                }).join('');
            if (hint) {
                hint.textContent = data.message || (items.length
                    ? 'Seleccione una caja empaquetada libre para agregar.'
                    : 'No hay cajas libres para agregar.');
            }
        });
    }

    function loadViewBoxes(shipmentId) {
        loadShipmentBoxes(shipmentId).then(function (boxes) {
            renderBoxList('envViewBoxesList', boxes, false);
        });
    }

    function refreshCurrentShipment() {
        if (!currentShipment) return Promise.resolve();
        return fetchItem(currentShipment.idShipment).then(function (item) {
            if (item) {
                currentShipment = item;
                updateCapacityBars(item);
            }
            return item;
        });
    }

    function addBoxToShipment() {
        if (!currentShipment) return;
        var boxId = qs('envBoxSelect')?.value;
        if (!boxId) return showMessageModal('Aviso', 'Seleccione una caja.');
        var form = new FormData();
        form.append('__RequestVerificationToken', token());
        form.append('shipmentId', currentShipment.idShipment);
        form.append('boxId', boxId);
        fetch(boxUrls().add, { method: 'POST', body: form, credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (json) {
                if (!json.success) return showMessageModal('Error', json.message || 'No se pudo agregar la caja.');
                refreshCurrentShipment().then(function () {
                    loadEditBoxes(currentShipment);
                    loadData(currentPage);
                });
            })
            .catch(function () { showMessageModal('Error', 'No se pudo agregar la caja.'); });
    }

    function removeBoxFromShipment(shipmentBoxId) {
        if (!currentShipment) return;
        showConfirmModal('Quitar caja', 'Deseas quitar esta caja del envio?', function () {
            var form = new FormData();
            form.append('__RequestVerificationToken', token());
            form.append('id', shipmentBoxId);
            fetch(boxUrls().remove, { method: 'POST', body: form, credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (json) {
                    if (!json.success) return showMessageModal('Error', json.message || 'No se pudo quitar la caja.');
                    refreshCurrentShipment().then(function () {
                        loadEditBoxes(currentShipment);
                        loadData(currentPage);
                    });
                });
        });
    }

    function changePage(step) {
        var next = currentPage + step;
        if (next >= 1 && next <= totalPages) loadData(next);
    }

    function closeModal() { qs('modalForm').style.display = 'none'; currentShipment = null; }
    function closeViewModal() { qs('modalView').style.display = 'none'; currentShipment = null; }
    function showMessageModal(title, message) {
        qs('messageTitle').innerText = title;
        qs('messageText').innerText = message;
        qs('modalMessage').style.display = 'flex';
    }
    function closeMessageModal() { qs('modalMessage').style.display = 'none'; }
    function showConfirmModal(title, message, callback) {
        qs('confirmTitle').innerText = title;
        qs('confirmText').innerText = message;
        confirmCallback = callback;
        qs('modalConfirm').style.display = 'flex';
    }
    function closeConfirmModal() { qs('modalConfirm').style.display = 'none'; confirmCallback = null; }
    function executeConfirmAction() { if (confirmCallback) confirmCallback(); closeConfirmModal(); }

    function escapeHtml(value) {
        return String(value ?? '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;').replaceAll("'", '&#039;');
    }
    function fmtNum(v) { return v == null || v === '' ? '0' : Number(v).toLocaleString('es-PE', { maximumFractionDigits: 2 }); }
    function formatValue(value, col) {
        if (col.type === 'number') return value === null || value === undefined ? '-' : Number(value).toLocaleString('es-PE', { maximumFractionDigits: 2 });
        if (col.type === 'date') return value ? new Date(value).toLocaleString('es-PE') : '-';
        return escapeHtml(value ?? '-');
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
