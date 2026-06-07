(function () {
    'use strict';

    var config = window.alertasStockConfig || {};
    var toast = document.getElementById('alertsToast');

    function showToast(message, type) {
        if (!toast) return;
        toast.textContent = message;
        toast.className = 'alerts-toast is-visible alerts-toast--' + (type || 'info');
        window.setTimeout(function () {
            toast.classList.remove('is-visible');
        }, 4000);
    }

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function initFilterToggle() {
        var btn = document.getElementById('toggleFiltersBtn');
        var panel = document.getElementById('alertsFilterForm');
        if (!btn || !panel) return;

        btn.addEventListener('click', function () {
            var isOpen = panel.classList.toggle('is-open');
            btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        });
    }

    function initDataTable() {
        if (!config.hasData || typeof $ === 'undefined' || !$.fn.DataTable) return;
        if (!document.getElementById('alertsTable')) return;

        if ($.fn.DataTable.isDataTable('#alertsTable')) {
            $('#alertsTable').DataTable().destroy();
        }

        $('#alertsTable').DataTable({
            order: [[3, 'desc']],
            pageLength: 10,
            searching: true,
            dom: '<"alerts-dt-top"lf>rt<"alerts-dt-bottom"ip>',
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            columnDefs: [
                { orderable: false, targets: 6 }
            ]
        });
    }

    function bindResendButtons() {
        document.querySelectorAll('.btn-resend-alert').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var alertId = btn.getAttribute('data-alert-id');
                var row = btn.closest('tr');
                if (!alertId || !config.resendUrl) return;

                btn.disabled = true;

                var body = new URLSearchParams();
                body.append('id', alertId);
                body.append('__RequestVerificationToken', getAntiForgeryToken());

                fetch(config.resendUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    credentials: 'same-origin',
                    body: body.toString()
                })
                    .then(function (response) { return response.json(); })
                    .then(function (data) {
                        showToast(data.message, data.success ? 'success' : 'error');

                        if (!data.success || !row) return;

                        if (data.message.indexOf('cerr') !== -1) {
                            if ($.fn.DataTable && $.fn.DataTable.isDataTable('#alertsTable')) {
                                $('#alertsTable').DataTable().row(row).remove().draw(false);
                            } else {
                                row.remove();
                            }
                            updateKpi(-1);
                        } else {
                            var countCell = row.querySelector('.cell-notification-count');
                            var lastNotifiedCell = row.querySelector('.cell-last-notified');
                            if (countCell) {
                                countCell.textContent = String(parseInt(countCell.textContent, 10) + 1);
                            }
                            if (lastNotifiedCell) {
                                var now = new Date();
                                lastNotifiedCell.innerHTML =
                                    '<span class="date-cell__date">' + now.toLocaleDateString('es-PE') + '</span>' +
                                    '<span class="date-cell__time">' + now.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' }) + '</span>';
                            }
                        }
                    })
                    .catch(function () {
                        showToast('No se pudo reenviar la alerta.', 'error');
                    })
                    .finally(function () {
                        btn.disabled = false;
                    });
            });
        });
    }

    function updateKpi(delta) {
        var el = document.getElementById('kpiActive');
        if (!el) return;
        var current = parseInt(el.textContent, 10) || 0;
        el.textContent = String(Math.max(0, current + delta));
    }

    function initAlertsPage() {
        if (!document.querySelector('.alerts-page')) return;
        initFilterToggle();
        initDataTable();
        bindResendButtons();
    }

    document.addEventListener('DOMContentLoaded', initAlertsPage);
    document.addEventListener('dashboard:contentLoaded', initAlertsPage);
})();
