(function () {
    'use strict';

    // 1. URLs y Tokens
    const getToken = () => document.querySelector('[name=__RequestVerificationToken]').value;

    // 2. Funciones globales para que el HTML pueda verlas
    window.closeModal = (id) => document.getElementById(id).classList.remove('is-open');
    window.openModal = (id) => document.getElementById(id).classList.add('is-open');

    // 3. Cargar tabla
    window.load = function () {
        fetch(prpUrls.list).then(r => r.json()).then(d => {
            const tbody = document.getElementById('prpBody');
            if (!tbody) return;
            tbody.innerHTML = d.items.map(i => `
                <tr>
                    <td>${i.productName}</td>
                    <td>${i.supplierName}</td>
                    <td>S/ ${i.supplierCost}</td>
                    <td>${i.isMainSupplier ? 'Sí' : 'No'}</td>
                    <td><button class="prp__btn prp__btn--danger" onclick="del(${i.idProductSupplier})">Eliminar</button></td>
                </tr>
            `).join('');
        });
    };

    // 4. Guardar
    window.save = function () {
        const f = document.getElementById('prpForm');
        const formData = new FormData(f);
        const data = new URLSearchParams();

        data.append('idProduct', formData.get('idProduct'));
        data.append('idSupplier', formData.get('idSupplier'));
        data.append('cost', formData.get('cost'));
        data.append('isMain', formData.get('isMain') === 'on');
        data.append('__RequestVerificationToken', getToken());

        fetch(prpUrls.save, { method: 'POST', body: data }).then(r => r.json()).then(r => {
            if (r.success) { alert(r.message); window.load(); window.closeModal('prpModal'); }
            else { alert('Error: ' + r.message); }
        });
    };

    // 5. Eliminar
    window.del = function (id) {
        if (!confirm('¿Eliminar esta asignación?')) return;
        const data = new URLSearchParams();
        data.append('id', id);
        data.append('__RequestVerificationToken', getToken());

        fetch(prpUrls.delete, { method: 'POST', body: data }).then(window.load);
    };

    // 6. Inicialización única
    function init() {
        const createBtn = document.getElementById('prpCreateBtn');
        if (createBtn) createBtn.onclick = () => window.openModal('prpModal');

        const saveBtn = document.getElementById('prpSaveBtn');
        if (saveBtn) saveBtn.onclick = window.save;

        window.load();
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();