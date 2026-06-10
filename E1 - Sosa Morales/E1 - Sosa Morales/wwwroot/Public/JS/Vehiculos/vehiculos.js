let currentPage = 1;
let totalPages = 1;
const pageSize = 10;
let confirmCallback = null;
let typeOptions = [];

async function initVehiculosPage() {
    const card = document.querySelector(".crud-card");
    if (!card || card.dataset.initialized === "true") return;
    card.dataset.initialized = "true";

    await loadTypeOptions();
    loadData(1);

    document.getElementById("txtSearch").addEventListener("keyup", e => {
        if (e.key === "Enter") loadData(1);
    });

    document.getElementById("selectStatus").addEventListener("change", () => loadData(1));
    document.getElementById("selectType").addEventListener("change", () => loadData(1));
    document.getElementById("btnBuscar").addEventListener("click", () => loadData(1));
}

document.addEventListener("DOMContentLoaded", initVehiculosPage);
document.addEventListener("dashboard:contentLoaded", initVehiculosPage);

function token() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : "";
}

async function loadTypeOptions() {
    try {
        const res = await fetch("/Vehiculos/TypeOptions");
        if (!res.ok) return;

        typeOptions = await res.json();
        fillTypeSelect(document.getElementById("selectType"), "Todos los tipos");
        fillTypeSelect(document.getElementById("txtVehicleType"), "Seleccione...");
        applyInitialFiltersFromUrl();
    } catch {
        showMessageModal("Error", "No se pudieron cargar los tipos de vehiculo.");
    }
}

function applyInitialFiltersFromUrl() {
    const params = new URLSearchParams(window.location.search);
    const vehicleTypeId = params.get("vehicleTypeId");
    if (vehicleTypeId) {
        document.getElementById("selectType").value = vehicleTypeId;
    }
}

function fillTypeSelect(select, placeholder) {
    select.innerHTML = `<option value="">${placeholder}</option>`;
    typeOptions.forEach(type => {
        select.innerHTML += `<option value="${type.idVehicleType}">${escapeHtml(type.name)}</option>`;
    });
}

async function loadData(page) {
    currentPage = page;

    const search = document.getElementById("txtSearch").value;
    const status = document.getElementById("selectStatus").value;
    const vehicleTypeId = document.getElementById("selectType").value;
    const url = status === "active" ? "/Vehiculos/List" : "/Vehiculos/ListInactive";
    const params = new URLSearchParams({ search, page, pageSize });
    if (vehicleTypeId) params.append("vehicleTypeId", vehicleTypeId);

    try {
        const res = await fetch(`${url}?${params.toString()}`);
        if (!res.ok) {
            showMessageModal("Error", "No se pudo cargar la informacion.");
            return;
        }

        const data = await res.json();
        totalPages = data.totalPages || 1;
        renderTable(data.items || [], status);
        document.getElementById("pageInfo").innerText = `Pagina ${data.page || 1} de ${totalPages}`;
    } catch {
        showMessageModal("Error", "Ocurrio un error al cargar los registros.");
    }
}

function renderTable(items, status) {
    const tbody = document.getElementById("tableBody");
    tbody.innerHTML = "";

    if (items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="empty">No hay registros.</td></tr>`;
        return;
    }

    items.forEach(item => {
        const isActive = status === "active";
        const statusLabel = isActive ? "Activo" : "Inactivo";
        const statusClass = isActive ? "status-active" : "status-inactive";
        const actions = isActive
            ? `
                <button type="button" onclick="viewItem(${item.id})">Ver</button>
                <button type="button" onclick="openEditModal(${item.id})">Editar</button>
                <button type="button" class="btn-danger" onclick="deleteLogic(${item.id})">Desactivar</button>
              `
            : `
                <button type="button" onclick="viewItem(${item.id})">Ver</button>
                <button type="button" onclick="restoreItem(${item.id})">Restaurar</button>
                <button type="button" class="btn-danger" onclick="deletePhysical(${item.id})">Eliminar</button>
              `;

        tbody.innerHTML += `
            <tr>
                <td>${item.id}</td>
                <td><strong>${escapeHtml(item.plate)}</strong></td>
                <td>${escapeHtml(item.vehicleTypeName)}</td>
                <td>${formatNumber(item.maximumWeight)}</td>
                <td>${formatNumber(item.maximumVolume)}</td>
                <td><span class="status-badge ${statusClass}">${statusLabel}</span></td>
                <td class="actions">${actions}</td>
            </tr>
        `;
    });
}

function changePage(step) {
    const next = currentPage + step;
    if (next < 1 || next > totalPages) return;
    loadData(next);
}

function openCreateModal() {
    document.getElementById("modalTitle").innerText = "Nuevo vehiculo";
    document.getElementById("txtId").value = "";
    document.getElementById("txtVehicleType").value = "";
    document.getElementById("txtPlate").value = "";
    document.getElementById("txtMaximumWeight").value = "";
    document.getElementById("txtMaximumVolume").value = "";
    document.getElementById("modalForm").style.display = "flex";
}

async function openEditModal(id) {
    const data = await fetchItem(id);
    if (!data) return;

    document.getElementById("modalTitle").innerText = "Editar vehiculo";
    document.getElementById("txtId").value = data.id;
    document.getElementById("txtVehicleType").value = data.vehicleTypeId;
    document.getElementById("txtPlate").value = data.plate;
    document.getElementById("txtMaximumWeight").value = data.maximumWeight ?? "";
    document.getElementById("txtMaximumVolume").value = data.maximumVolume ?? "";
    document.getElementById("modalForm").style.display = "flex";
}

async function viewItem(id) {
    const data = await fetchItem(id);
    if (!data) return;

    const isActive = data.status == 1;
    const statusBadge = document.getElementById("detailStatus");

    document.getElementById("detailId").innerText = data.id;
    document.getElementById("detailPlate").innerText = data.plate;
    document.getElementById("detailVehicleType").innerText = data.vehicleTypeName;
    document.getElementById("detailMaximumWeight").innerText = formatNumber(data.maximumWeight);
    document.getElementById("detailMaximumVolume").innerText = formatNumber(data.maximumVolume);
    document.getElementById("detailCreatedAt").innerText = data.createdAt || "-";
    document.getElementById("detailUpdatedAt").innerText = data.updatedAt || "-";
    statusBadge.innerText = isActive ? "Activo" : "Inactivo";
    statusBadge.className = `status-badge ${isActive ? "status-active" : "status-inactive"}`;

    document.getElementById("modalView").style.display = "flex";
}

async function fetchItem(id) {
    try {
        const res = await fetch(`/Vehiculos/Get?id=${id}`);
        if (!res.ok) {
            showMessageModal("Error", "No se pudo obtener el registro.");
            return null;
        }

        const json = await res.json();
        if (!json.success) {
            showMessageModal("Error", json.message);
            return null;
        }

        return json.data;
    } catch {
        showMessageModal("Error", "Ocurrio un error al obtener el registro.");
        return null;
    }
}

function closeModal() {
    document.getElementById("modalForm").style.display = "none";
}

function closeViewModal() {
    document.getElementById("modalView").style.display = "none";
}

async function saveItem() {
    const id = document.getElementById("txtId").value;
    const vehicleTypeId = document.getElementById("txtVehicleType").value;
    const plate = document.getElementById("txtPlate").value.trim();
    const maximumWeight = document.getElementById("txtMaximumWeight").value;
    const maximumVolume = document.getElementById("txtMaximumVolume").value;

    if (!vehicleTypeId) {
        showMessageModal("Validacion", "Seleccione el tipo de vehiculo.");
        return;
    }

    if (!plate) {
        showMessageModal("Validacion", "Ingrese la placa del vehiculo.");
        return;
    }

    const url = id ? "/Vehiculos/Update" : "/Vehiculos/Create";
    const form = new FormData();
    form.append("__RequestVerificationToken", token());
    if (id) form.append("id", id);
    form.append("vehicleTypeId", vehicleTypeId);
    form.append("plate", plate);
    form.append("maximumWeight", maximumWeight);
    form.append("maximumVolume", maximumVolume);

    try {
        const res = await fetch(url, { method: "POST", body: form });
        if (!res.ok) {
            showMessageModal("Error", "No se pudo procesar la solicitud.");
            return;
        }

        const json = await res.json();
        if (json.success) {
            closeModal();
            showMessageModal("Operacion exitosa", json.message || "El vehiculo se ha guardado correctamente.");
            loadData(currentPage);
        } else {
            showMessageModal("Error", json.message || "No se pudo guardar el registro.");
        }
    } catch {
        showMessageModal("Error", "Ocurrio un error al guardar el registro.");
    }
}

async function postAction(url, id, successDefaultMessage) {
    const form = new FormData();
    form.append("__RequestVerificationToken", token());
    form.append("id", id);

    try {
        const res = await fetch(url, { method: "POST", body: form });
        if (!res.ok) {
            showMessageModal("Error", "No se pudo procesar la solicitud.");
            return;
        }

        const json = await res.json();
        if (json.success) {
            showMessageModal("Operacion exitosa", json.message || successDefaultMessage);
            loadData(1);
        } else {
            showMessageModal("Error", json.message || "No se pudo completar la operacion.");
        }
    } catch {
        showMessageModal("Error", "Ocurrio un error al procesar la operacion.");
    }
}

function deleteLogic(id) {
    showConfirmModal(
        "Desactivar vehiculo",
        "Deseas desactivar este vehiculo?",
        () => postAction("/Vehiculos/DeleteLogic", id, "El vehiculo se ha desactivado correctamente.")
    );
}

function restoreItem(id) {
    showConfirmModal(
        "Restaurar vehiculo",
        "Deseas restaurar este vehiculo?",
        () => postAction("/Vehiculos/Restore", id, "El vehiculo se ha restaurado correctamente.")
    );
}

function deletePhysical(id) {
    showConfirmModal(
        "Eliminar permanentemente",
        "Deseas eliminar permanentemente este vehiculo? Esta accion no se puede deshacer.",
        () => postAction("/Vehiculos/DeletePhysical", id, "El vehiculo se ha eliminado permanentemente.")
    );
}

function showMessageModal(title, message) {
    document.getElementById("messageTitle").innerText = title;
    document.getElementById("messageText").innerText = message;
    document.getElementById("modalMessage").style.display = "flex";
}

function closeMessageModal() {
    document.getElementById("modalMessage").style.display = "none";
}

function showConfirmModal(title, message, callback) {
    document.getElementById("confirmTitle").innerText = title;
    document.getElementById("confirmText").innerText = message;
    confirmCallback = callback;
    document.getElementById("modalConfirm").style.display = "flex";
}

function closeConfirmModal() {
    document.getElementById("modalConfirm").style.display = "none";
    confirmCallback = null;
}

function executeConfirmAction() {
    if (typeof confirmCallback === "function") {
        confirmCallback();
    }

    closeConfirmModal();
}

function formatNumber(value) {
    if (value === null || value === undefined || value === "") return "-";
    return Number(value).toLocaleString("es-PE", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
