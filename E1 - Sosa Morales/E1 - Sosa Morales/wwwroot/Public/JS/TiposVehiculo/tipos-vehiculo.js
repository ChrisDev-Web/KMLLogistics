let currentPage = 1;
let totalPages = 1;
const pageSize = 10;

let confirmCallback = null;
let currentDetailVehicleTypeId = null;

document.addEventListener("DOMContentLoaded", () => {
    loadData(1);

    document.getElementById("txtSearch").addEventListener("keyup", e => {
        if (e.key === "Enter") loadData(1);
    });

    document.getElementById("selectStatus").addEventListener("change", () => {
        currentPage = 1;
        loadData(1);
    });

    const btnBuscar = document.getElementById("btnBuscar");
    if (btnBuscar) {
        btnBuscar.addEventListener("click", () => loadData(1));
    }
});

function token() {
    return document.querySelector('input[name="__RequestVerificationToken"]').value;
}

async function loadData(page) {
    currentPage = page;

    const search = document.getElementById("txtSearch").value;
    const status = document.getElementById("selectStatus").value;
    const url = status === "active" ? "/TiposVehiculo/List" : "/TiposVehiculo/ListInactive";

    try {
        const res = await fetch(`${url}?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`);

        if (!res.ok) {
            showMessageModal("Error", "No se pudo cargar la información.");
            return;
        }

        const data = await res.json();

        totalPages = data.totalPages || 1;
        renderTable(data.items || [], status);

        document.getElementById("pageInfo").innerText = `Página ${data.page || 1} de ${totalPages}`;
    } catch {
        showMessageModal("Error", "Ocurrió un error al cargar los registros.");
    }
}

function renderTable(items, status) {
    const tbody = document.getElementById("tableBody");
    tbody.innerHTML = "";

    if (items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="empty">No hay registros.</td></tr>`;
        return;
    }

    items.forEach(item => {
        const isActive = status === "active";
        const statusLabel = isActive ? "Activo" : "Inactivo";
        const statusClass = isActive ? "status-active" : "status-inactive";
        const actions = status === "active"
            ? `
                <button type="button" onclick="viewItem(${item.id})">Ver</button>
                <button type="button" onclick="goToVehicles(${item.id})">Ver vehículos</button>
                <button type="button" onclick="openEditModal(${item.id})">Editar</button>
                <button type="button" class="btn-danger" onclick="deleteLogic(${item.id})">Desactivar</button>
              `
            : `
                <button type="button" onclick="viewItem(${item.id})">Ver</button>
                <button type="button" onclick="goToVehicles(${item.id})">Ver vehículos</button>
                <button type="button" onclick="restoreItem(${item.id})">Restaurar</button>
                <button type="button" class="btn-danger" onclick="deletePhysical(${item.id})">Eliminar</button>
              `;

        tbody.innerHTML += `
            <tr>
                <td>${item.id}</td>
                <td>${escapeHtml(item.name)}</td>
                <td>${escapeHtml(item.description || "")}</td>
                <td><span class="count-pill">${formatVehicleCount(item.vehicleCount)}</span></td>
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
    document.getElementById("modalTitle").innerText = "Nuevo tipo de vehículo";
    document.getElementById("txtId").value = "";
    document.getElementById("txtName").value = "";
    document.getElementById("txtDescription").value = "";
    document.getElementById("modalForm").style.display = "flex";
}

async function openEditModal(id) {
    try {
        const res = await fetch(`/TiposVehiculo/Get?id=${id}`);

        if (!res.ok) {
            showMessageModal("Error", "No se pudo obtener el registro.");
            return;
        }

        const json = await res.json();

        if (!json.success) {
            showMessageModal("Error", json.message);
            return;
        }

        document.getElementById("modalTitle").innerText = "Editar tipo de vehículo";
        document.getElementById("txtId").value = json.data.id;
        document.getElementById("txtName").value = json.data.name;
        document.getElementById("txtDescription").value = json.data.description || "";
        document.getElementById("modalForm").style.display = "flex";
    } catch {
        showMessageModal("Error", "Ocurrió un error al obtener el registro.");
    }
}

async function viewItem(id) {
    try {
        const res = await fetch(`/TiposVehiculo/Get?id=${id}`);

        if (!res.ok) {
            showMessageModal("Error", "No se pudo obtener la información.");
            return;
        }

        const json = await res.json();

        if (!json.success) {
            showMessageModal("Error", json.message);
            return;
        }

        const data = json.data;
        const isActive = data.status == 1;
        const statusBadge = document.getElementById("detailStatus");

        document.getElementById("detailId").innerText = data.id;
        currentDetailVehicleTypeId = data.id;
        document.getElementById("detailName").innerText = data.name;
        document.getElementById("detailDescription").innerText = data.description || "Sin descripción";
        document.getElementById("detailVehicleCount").innerText = formatVehicleCount(data.vehicleCount);
        statusBadge.innerText = isActive ? "Activo" : "Inactivo";
        statusBadge.className = `status-badge ${isActive ? "status-active" : "status-inactive"}`;
        document.getElementById("detailCreatedAt").innerText = data.createdAt || "-";
        document.getElementById("detailUpdatedAt").innerText = data.updatedAt || "-";

        document.getElementById("modalView").style.display = "flex";
    } catch {
        showMessageModal("Error", "Ocurrió un error al obtener el registro.");
    }
}

function goToVehicles(id) {
    window.location.href = `/Vehiculos?vehicleTypeId=${encodeURIComponent(id)}`;
}

function goToVehiclesFromDetail() {
    if (!currentDetailVehicleTypeId) return;
    goToVehicles(currentDetailVehicleTypeId);
}

function closeModal() {
    document.getElementById("modalForm").style.display = "none";
}

function closeViewModal() {
    document.getElementById("modalView").style.display = "none";
}

async function saveItem() {
    const id = document.getElementById("txtId").value;
    const name = document.getElementById("txtName").value.trim();
    const description = document.getElementById("txtDescription").value.trim();

    if (!name) {
        showMessageModal("Validación", "Ingrese el nombre del tipo de vehículo.");
        return;
    }

    const url = id ? "/TiposVehiculo/Update" : "/TiposVehiculo/Create";

    const form = new FormData();
    form.append("__RequestVerificationToken", token());

    if (id) form.append("id", id);

    form.append("name", name);
    form.append("description", description);

    try {
        const res = await fetch(url, {
            method: "POST",
            body: form
        });

        if (!res.ok) {
            showMessageModal("Error", "No se pudo procesar la solicitud.");
            return;
        }

        const json = await res.json();

        if (json.success) {
            closeModal();
            showMessageModal("Operación exitosa", json.message || "El tipo de vehículo se ha guardado correctamente.");
            loadData(currentPage);
        } else {
            showMessageModal("Error", json.message || "No se pudo guardar el registro.");
        }
    } catch {
        showMessageModal("Error", "Ocurrió un error al guardar el registro.");
    }
}

async function postAction(url, id, successDefaultMessage) {
    const form = new FormData();
    form.append("__RequestVerificationToken", token());
    form.append("id", id);

    try {
        const res = await fetch(url, {
            method: "POST",
            body: form
        });

        if (!res.ok) {
            showMessageModal("Error", "No se pudo procesar la solicitud.");
            return;
        }

        const json = await res.json();

        if (json.success) {
            showMessageModal("Operación exitosa", json.message || successDefaultMessage);
            loadData(1);
        } else {
            showMessageModal("Error", json.message || "No se pudo completar la operación.");
        }
    } catch {
        showMessageModal("Error", "Ocurrió un error al procesar la operación.");
    }
}

function deleteLogic(id) {
    showConfirmModal(
        "Desactivar tipo de vehículo",
        "¿Deseas desactivar este tipo de vehículo?",
        () => postAction("/TiposVehiculo/DeleteLogic", id, "El tipo de vehículo se ha desactivado correctamente.")
    );
}

function restoreItem(id) {
    showConfirmModal(
        "Restaurar tipo de vehículo",
        "¿Deseas restaurar este tipo de vehículo?",
        () => postAction("/TiposVehiculo/Restore", id, "El tipo de vehículo se ha restaurado correctamente.")
    );
}

function deletePhysical(id) {
    showConfirmModal(
        "Eliminar permanentemente",
        "¿Deseas eliminar permanentemente este tipo de vehículo? Esta acción no se puede deshacer.",
        () => postAction("/TiposVehiculo/DeletePhysical", id, "El tipo de vehículo se ha eliminado permanentemente.")
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

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatVehicleCount(value) {
    const count = Number(value || 0);
    return `${count} ${count === 1 ? "vehículo" : "vehículos"}`;
}
