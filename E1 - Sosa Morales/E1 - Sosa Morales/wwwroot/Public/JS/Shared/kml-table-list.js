(function (global) {
    'use strict';

    function hasDataRows(tbody) {
        if (!tbody) return false;
        var rows = tbody.querySelectorAll('tr');
        for (var i = 0; i < rows.length; i++) {
            var cls = rows[i].className || '';
            if (cls.indexOf('__loading-row') === -1 && cls.indexOf('__empty-row') === -1) return true;
        }
        return false;
    }

    global.KmlTableList = {
        /** Inicia carga AJAX: overlay suave si ya hay filas; spinner solo en carga inicial. */
        begin: function (scrollEl, tbody, loadingHtml) {
            if (scrollEl) scrollEl.classList.add('is-loading');
            if (tbody && !hasDataRows(tbody) && loadingHtml) tbody.innerHTML = loadingHtml;
        },
        end: function (scrollEl) {
            if (scrollEl) scrollEl.classList.remove('is-loading');
        },
        lockPagination: function (prevBtn, nextBtn) {
            if (prevBtn) prevBtn.disabled = true;
            if (nextBtn) nextBtn.disabled = true;
        }
    };
})(window);
