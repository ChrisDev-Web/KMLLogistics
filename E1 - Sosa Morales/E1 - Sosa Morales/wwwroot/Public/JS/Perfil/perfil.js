(function () {
    'use strict';

    var photoInput = document.getElementById('perfPhotoInput');
    var photoForm = document.getElementById('perfPhotoForm');
    var preview = document.getElementById('perfPhotoPreview');
    var accountForm = document.getElementById('perfAccountForm');
    var removePhotoBtn = document.getElementById('perfRemovePhotoBtn');
    var removePhotoForm = document.getElementById('perfRemovePhotoForm');
    var confirmModal = document.getElementById('perfConfirmModal');
    var confirmBtn = document.getElementById('perfConfirmBtn');

    function openModal(id) {
        var modal = document.getElementById(id);
        if (!modal) return;
        modal.classList.add('is-open');
        modal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
    }

    function closeModal(id) {
        var modal = document.getElementById(id);
        if (!modal) return;
        modal.classList.remove('is-open');
        modal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
    }

    function closeAllModals() {
        document.querySelectorAll('.perf-modal.is-open').forEach(function (modal) {
            modal.classList.remove('is-open');
            modal.setAttribute('aria-hidden', 'true');
        });
        document.body.style.overflow = '';
    }

    if (removePhotoBtn && removePhotoForm) {
        removePhotoBtn.addEventListener('click', function () {
            openModal('perfConfirmModal');
        });
    }

    if (confirmBtn && removePhotoForm) {
        confirmBtn.addEventListener('click', function () {
            closeAllModals();
            removePhotoForm.submit();
        });
    }

    document.querySelectorAll('[data-dismiss="perf-modal"]').forEach(function (el) {
        el.addEventListener('click', closeAllModals);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeAllModals();
    });

    if (photoInput && photoForm) {
        photoInput.addEventListener('change', function () {
            var file = photoInput.files && photoInput.files[0];
            if (!file) return;

            if (file.size > 2 * 1024 * 1024) {
                alert('La imagen no puede superar 2 MB.');
                photoInput.value = '';
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                if (!preview) return;
                preview.innerHTML = '<img src="' + e.target.result + '" alt="Vista previa" class="perf-photo__img" />';
            };
            reader.readAsDataURL(file);

            photoForm.submit();
        });
    }

    if (accountForm) {
        accountForm.addEventListener('submit', function (e) {
            var pwd = document.getElementById('perfNewPassword');
            var confirm = document.getElementById('perfConfirmPassword');
            if (!pwd || !confirm) return;

            var pwdVal = pwd.value.trim();
            var confirmVal = confirm.value.trim();

            if (pwdVal && pwdVal.length < 6) {
                e.preventDefault();
                alert('La contraseña debe tener al menos 6 caracteres.');
                return;
            }

            if (pwdVal !== confirmVal) {
                e.preventDefault();
                alert('Las contraseñas no coinciden.');
            }
        });
    }
})();
