window.save = () => {
    const data = new URLSearchParams();
    data.append('idSupplier', document.getElementById('sId').value);
    data.append('idBrand', document.getElementById('bId').value);
    data.append('__RequestVerificationToken', document.querySelector('[name=__RequestVerificationToken]').value);

    fetch('@Url.Action("Save")', { method: 'POST', body: data })
        .then(r => r.json()).then(r => {
            if (r.success) { window.load(); window.closeModal('sbrModal'); }
            else alert(r.message);
        });
};

window.del = (sId, bId) => {
    if (!confirm('¿Eliminar relación?')) return;
    const data = new URLSearchParams();
    data.append('idSupplier', sId);
    data.append('idBrand', bId);
    data.append('__RequestVerificationToken', document.querySelector('[name=__RequestVerificationToken]').value);
    fetch('@Url.Action("Delete")', { method: 'POST', body: data }).then(window.load);
};