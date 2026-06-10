(function () {
    const cfg = window.kmlCrudConfig;
    if (!cfg) return;

    let currentPage = 1;
    let totalPages = 1;
    const pageSize = 10;
    let confirmCallback = null;
    let optionData = {};

    document.addEventListener("DOMContentLoaded", init);

    async function init() {
        await loadOptions();
        buildTableHead();
        buildForm();
        bindEvents();
        loadData(1);
    }

    function bindEvents() {
        qs("txtSearch")?.addEventListener("keyup", e => { if (e.key === "Enter") loadData(1); });
        qs("selectStatus")?.addEventListener("change", () => loadData(1));
        qs("btnBuscar")?.addEventListener("click", () => loadData(1));
        qs("btnCreate")?.addEventListener("click", openCreateModal);
        qs("btnPrev")?.addEventListener("click", () => changePage(-1));
        qs("btnNext")?.addEventListener("click", () => changePage(1));
        qs("btnSave")?.addEventListener("click", saveItem);
        qs("btnCancel")?.addEventListener("click", closeModal);
        qs("btnCloseView")?.addEventListener("click", closeViewModal);
        qs("btnCloseMessage")?.addEventListener("click", closeMessageModal);
        qs("btnCancelConfirm")?.addEventListener("click", closeConfirmModal);
        qs("btnConfirm")?.addEventListener("click", executeConfirmAction);
    }

    async function loadOptions() {
        if (!cfg.optionsUrl) return;
        try {
            const res = await fetch(cfg.optionsUrl);
            if (res.ok) optionData = await res.json();
        } catch { optionData = {}; }
    }

    function buildTableHead() {
        qs("tableHead").innerHTML = `<tr>${cfg.columns.map(c => `<th>${c.label}</th>`).join("")}<th>Acciones</th></tr>`;
    }

    function buildForm() {
        qs("formFields").innerHTML = cfg.fields.map(field => {
            if (field.type === "select") {
                return `<label>${field.label}</label><select id="field_${field.name}">${optionsHtml(field)}</select>`;
            }
            return `<label>${field.label}</label><input id="field_${field.name}" type="${field.type || "text"}" step="0.01" />`;
        }).join("");
    }

    function optionsHtml(field) {
        const list = optionData[field.optionsKey] || [];
        return `<option value="">Seleccione...</option>` + list.map(o => `<option value="${o[field.valueKey]}">${escapeHtml(o[field.textKey])}</option>`).join("");
    }

    async function loadData(page) {
        currentPage = page;
        const params = new URLSearchParams({ search: qs("txtSearch").value, page, pageSize });
        const status = qs("selectStatus")?.value || "active";
        const url = status === "inactive" && cfg.listInactiveUrl ? cfg.listInactiveUrl : cfg.listUrl;

        try {
            const res = await fetch(`${url}?${params.toString()}`);
            if (!res.ok) return showMessageModal("Error", "No se pudo cargar la informacion.");
            const data = await res.json();
            totalPages = data.totalPages || 1;
            renderTable(data.items || []);
            qs("pageInfo").innerText = `Pagina ${data.page || 1} de ${totalPages}`;
        } catch {
            showMessageModal("Error", "Ocurrio un error al cargar los registros.");
        }
    }

    function renderTable(items) {
        const tbody = qs("tableBody");
        if (!items.length) {
            tbody.innerHTML = `<tr><td colspan="${cfg.columns.length + 1}" class="empty">No hay registros.</td></tr>`;
            return;
        }

        tbody.innerHTML = items.map(item => `
            <tr>
                ${cfg.columns.map(c => `<td>${formatValue(item[c.key], c)}</td>`).join("")}
                <td class="actions">${actionsHtml(item)}</td>
            </tr>
        `).join("");

        tbody.querySelectorAll("[data-action]").forEach(btn => {
            btn.addEventListener("click", () => handleAction(btn.dataset.action, btn.dataset.id));
        });
    }

    function actionsHtml(item) {
        const id = item[cfg.idKey];
        const status = qs("selectStatus")?.value || "active";
        let html = cfg.getUrl ? `<button data-action="view" data-id="${id}" type="button">Ver</button>` : "";
        if (cfg.allowEdit !== false && cfg.updateUrl) html += `<button data-action="edit" data-id="${id}" type="button">Editar</button>`;
        if (cfg.deleteUrl) html += `<button data-action="delete" data-id="${id}" class="btn-danger" type="button">Eliminar</button>`;
        if (cfg.deleteLogicUrl && status === "active") html += `<button data-action="deleteLogic" data-id="${id}" class="btn-danger" type="button">Desactivar</button>`;
        if (cfg.restoreUrl && status === "inactive") html += `<button data-action="restore" data-id="${id}" type="button">Restaurar</button>`;
        if (cfg.deletePhysicalUrl && status === "inactive") html += `<button data-action="deletePhysical" data-id="${id}" class="btn-danger" type="button">Eliminar</button>`;
        return html;
    }

    async function handleAction(action, id) {
        if (action === "view") return viewItem(id);
        if (action === "edit") return openEditModal(id);
        const map = { delete: cfg.deleteUrl, deleteLogic: cfg.deleteLogicUrl, restore: cfg.restoreUrl, deletePhysical: cfg.deletePhysicalUrl };
        showConfirmModal("Confirmar accion", "Deseas continuar?", () => postAction(map[action], id));
    }

    async function fetchItem(id) {
        if (!cfg.getUrl) return null;
        const res = await fetch(`${cfg.getUrl}?id=${id}`);
        if (!res.ok) return null;
        const json = await res.json();
        return json.success ? json.data : null;
    }

    async function viewItem(id) {
        const item = await fetchItem(id);
        if (!item) return showMessageModal("Error", "No se pudo obtener el registro.");
        qs("detailBody").innerHTML = cfg.detailColumns.map(c => `<div class="detail-item"><span>${c.label}</span><strong>${formatValue(item[c.key], c)}</strong></div>`).join("");
        qs("modalView").style.display = "flex";
    }

    async function openEditModal(id) {
        const item = await fetchItem(id);
        if (!item) return showMessageModal("Error", "No se pudo obtener el registro.");
        qs("modalTitle").innerText = cfg.editTitle || "Editar";
        qs("txtId").value = id;
        cfg.fields.forEach(f => qs(`field_${f.name}`).value = item[f.dataKey || f.name] ?? "");
        qs("modalForm").style.display = "flex";
    }

    function openCreateModal() {
        qs("modalTitle").innerText = cfg.createTitle || "Nuevo";
        qs("txtId").value = "";
        cfg.fields.forEach(f => qs(`field_${f.name}`).value = "");
        qs("modalForm").style.display = "flex";
    }

    async function saveItem() {
        const id = qs("txtId").value;
        const form = new FormData();
        form.append("__RequestVerificationToken", token());
        if (id) form.append("id", id);
        cfg.fields.forEach(f => form.append(f.name, qs(`field_${f.name}`).value));
        const url = id ? cfg.updateUrl : cfg.createUrl;
        const res = await fetch(url, { method: "POST", body: form });
        if (!res.ok) return showMessageModal("Error", await readErrorMessage(res));
        const json = await res.json();
        if (json.success) { closeModal(); showMessageModal("Operacion exitosa", json.message || "Operacion completada."); loadData(currentPage); }
        else showMessageModal("Error", json.message || "No se pudo completar la operacion.");
    }

    async function postAction(url, id) {
        const form = new FormData();
        form.append("__RequestVerificationToken", token());
        form.append("id", id);
        const res = await fetch(url, { method: "POST", body: form });
        if (!res.ok) return showMessageModal("Error", await readErrorMessage(res));
        const json = await res.json();
        showMessageModal(json.success ? "Operacion exitosa" : "Error", json.message || "");
        loadData(1);
    }

    function changePage(step) { const next = currentPage + step; if (next >= 1 && next <= totalPages) loadData(next); }
    function closeModal() { qs("modalForm").style.display = "none"; }
    function closeViewModal() { qs("modalView").style.display = "none"; }
    function showMessageModal(title, message) { qs("messageTitle").innerText = title; qs("messageText").innerText = message; qs("modalMessage").style.display = "flex"; }
    function closeMessageModal() { qs("modalMessage").style.display = "none"; }
    function showConfirmModal(title, message, callback) { qs("confirmTitle").innerText = title; qs("confirmText").innerText = message; confirmCallback = callback; qs("modalConfirm").style.display = "flex"; }
    function closeConfirmModal() { qs("modalConfirm").style.display = "none"; confirmCallback = null; }
    function executeConfirmAction() { if (confirmCallback) confirmCallback(); closeConfirmModal(); }
    function token() { return document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""; }
    function qs(id) { return document.getElementById(id); }
    function escapeHtml(value) { return String(value ?? "").replaceAll("&","&amp;").replaceAll("<","&lt;").replaceAll(">","&gt;").replaceAll('"',"&quot;").replaceAll("'","&#039;"); }
    function formatValue(value, col) {
        if (col.type === "money") return Number(value || 0).toLocaleString("es-PE", { style:"currency", currency:"PEN" });
        if (col.type === "number") return value === null || value === undefined ? "-" : Number(value).toLocaleString("es-PE", { maximumFractionDigits: 2 });
        if (col.type === "date") return value ? new Date(value).toLocaleString("es-PE") : "-";
        if (col.type === "status") return `<span class="status-badge ${Number(value) === 1 ? "status-active" : "status-inactive"}">${Number(value) === 1 ? "Activo" : "Inactivo"}</span>`;
        return escapeHtml(value ?? "-");
    }
    async function readErrorMessage(res) {
        try {
            const text = await res.text();
            const json = JSON.parse(text);
            return json.message || "No se pudo procesar la solicitud.";
        } catch {
            return "No se pudo procesar la solicitud.";
        }
    }
})();
