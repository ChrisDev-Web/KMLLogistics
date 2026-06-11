(function () {
    'use strict';

    var config = window.statsConfig || {};
    var state = { preset: 'today', dateFrom: '', dateTo: '' };
    var charts = {};
    var initialized = false;

    var palette = {
        sales: '#10b981',
        salesLight: 'rgba(16, 185, 129, 0.15)',
        purchases: '#f59e0b',
        purchasesLight: 'rgba(245, 158, 11, 0.15)',
        profit: '#8b5cf6',
        profitLight: 'rgba(139, 92, 246, 0.2)',
        donut: ['#6366f1', '#10b981', '#f59e0b', '#ec4899', '#0ea5e9', '#84cc16']
    };

    function qs(id) { return document.getElementById(id); }

    function formatMoney(value) {
        var num = Number(value) || 0;
        return 'S/ ' + num.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function formatShortDate(iso) {
        var d = new Date(iso);
        if (Number.isNaN(d.getTime())) return '';
        return d.toLocaleDateString('es-PE', { day: '2-digit', month: 'short' });
    }

    function buildUrl() {
        var url = new URL(config.dataUrl, window.location.origin);
        url.searchParams.set('preset', state.preset);
        if (state.preset === 'custom') {
            if (state.dateFrom) url.searchParams.set('dateFrom', state.dateFrom);
            if (state.dateTo) url.searchParams.set('dateTo', state.dateTo);
        }
        return url.toString();
    }

    function setRefreshing(show) {
        var charts = qs('statsCharts');
        var kpis = qs('statsKpis');
        if (charts) charts.classList.toggle('is-refreshing', show);
        if (kpis) kpis.classList.toggle('is-refreshing', show);
    }

    function destroyChart(key) {
        if (charts[key]) {
            charts[key].destroy();
            charts[key] = null;
        }
    }

    function baseChartOptions() {
        return {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 16,
                        font: { size: 12, weight: '600' }
                    }
                },
                tooltip: {
                    backgroundColor: '#0f172a',
                    padding: 12,
                    cornerRadius: 8,
                    titleFont: { size: 13, weight: '600' },
                    bodyFont: { size: 12 },
                    callbacks: {
                        label: function (ctx) {
                            var label = ctx.dataset.label || '';
                            if (label) label += ': ';
                            if (ctx.parsed.y !== undefined && ctx.parsed.y !== null) {
                                label += formatMoney(ctx.parsed.y);
                            } else if (ctx.parsed !== undefined) {
                                label += formatMoney(ctx.parsed);
                            }
                            return label;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 11 }, color: '#94a3b8' }
                },
                y: {
                    grid: { color: '#f1f5f9' },
                    ticks: {
                        font: { size: 11 },
                        color: '#94a3b8',
                        callback: function (v) { return 'S/ ' + Number(v).toLocaleString('es-PE'); }
                    }
                }
            }
        };
    }

    function updateKpis(summary) {
        var s = summary || {};
        if (qs('kpiTotalSales')) qs('kpiTotalSales').textContent = formatMoney(s.totalSales);
        if (qs('kpiTotalPurchases')) qs('kpiTotalPurchases').textContent = formatMoney(s.totalPurchases);
        if (qs('kpiNetProfit')) qs('kpiNetProfit').textContent = formatMoney(s.netProfit);
        if (qs('kpiNetBalance')) {
            var balance = (Number(s.totalSales) || 0) - (Number(s.totalPurchases) || 0);
            qs('kpiNetBalance').textContent = formatMoney(balance);
        }
        if (qs('kpiSalesCount')) {
            qs('kpiSalesCount').textContent = (s.salesCount || 0) + ' venta' + ((s.salesCount || 0) === 1 ? '' : 's');
        }
        if (qs('kpiPurchasesCount')) {
            qs('kpiPurchasesCount').textContent = (s.purchasesCount || 0) + ' compra' + ((s.purchasesCount || 0) === 1 ? '' : 's');
        }
    }

    function renderTrendLine(trend) {
        var canvas = qs('chartTrendLine');
        if (!canvas || typeof Chart === 'undefined') return;

        var labels = (trend || []).map(function (r) { return formatShortDate(r.periodDate); });
        var sales = (trend || []).map(function (r) { return Number(r.salesAmount) || 0; });
        var purchases = (trend || []).map(function (r) { return Number(r.purchasesAmount) || 0; });

        destroyChart('trendLine');
        charts.trendLine = new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ventas',
                        data: sales,
                        borderColor: palette.sales,
                        backgroundColor: palette.salesLight,
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2.5,
                        pointRadius: 3,
                        pointHoverRadius: 6
                    },
                    {
                        label: 'Compras',
                        data: purchases,
                        borderColor: palette.purchases,
                        backgroundColor: palette.purchasesLight,
                        fill: true,
                        tension: 0.4,
                        borderWidth: 2.5,
                        pointRadius: 3,
                        pointHoverRadius: 6
                    }
                ]
            },
            options: baseChartOptions()
        });
    }

    function renderSalesBar(trend) {
        var canvas = qs('chartSalesBar');
        if (!canvas || typeof Chart === 'undefined') return;

        var labels = (trend || []).map(function (r) { return formatShortDate(r.periodDate); });
        var sales = (trend || []).map(function (r) { return Number(r.salesAmount) || 0; });

        destroyChart('salesBar');
        charts.salesBar = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ventas',
                    data: sales,
                    backgroundColor: 'rgba(99, 102, 241, 0.75)',
                    hoverBackgroundColor: '#6366f1',
                    borderRadius: 6,
                    borderSkipped: false
                }]
            },
            options: baseChartOptions()
        });
    }

    function renderProfitArea(trend) {
        var canvas = qs('chartProfitArea');
        if (!canvas || typeof Chart === 'undefined') return;

        var labels = (trend || []).map(function (r) { return formatShortDate(r.periodDate); });
        var profit = (trend || []).map(function (r) { return Number(r.netProfit) || 0; });

        destroyChart('profitArea');
        charts.profitArea = new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ganancia neta',
                    data: profit,
                    borderColor: palette.profit,
                    backgroundColor: palette.profitLight,
                    fill: true,
                    tension: 0.35,
                    borderWidth: 2.5,
                    pointRadius: 2,
                    pointHoverRadius: 5
                }]
            },
            options: baseChartOptions()
        });
    }

    function renderPayments(payments) {
        var canvas = qs('chartPayments');
        var legend = qs('paymentLegend');
        if (!canvas || typeof Chart === 'undefined') return;

        var items = payments || [];
        var labels = items.map(function (p) { return p.paymentMethod; });
        var amounts = items.map(function (p) { return Number(p.totalAmount) || 0; });
        var colors = items.map(function (_, i) { return palette.donut[i % palette.donut.length]; });

        destroyChart('payments');
        charts.payments = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: amounts,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#fff',
                    hoverOffset: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        padding: 12,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx) {
                                var total = amounts.reduce(function (a, b) { return a + b; }, 0);
                                var val = ctx.parsed || 0;
                                var pct = total > 0 ? ((val / total) * 100).toFixed(1) : '0';
                                return ctx.label + ': ' + formatMoney(val) + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });

        if (legend) {
            if (!items.length) {
                legend.innerHTML = '<li style="grid-column:1/-1;color:#94a3b8;">Sin ventas en el período</li>';
                return;
            }
            var total = amounts.reduce(function (a, b) { return a + b; }, 0);
            legend.innerHTML = items.map(function (p, i) {
                var amt = Number(p.totalAmount) || 0;
                var pct = total > 0 ? ((amt / total) * 100).toFixed(0) : 0;
                return '<li>' +
                    '<span class="stats-legend__dot" style="background:' + colors[i] + '"></span>' +
                    '<span>' + p.paymentMethod + ' (' + pct + '%)</span>' +
                    '<span class="stats-legend__amount">' + formatMoney(amt) + '</span>' +
                    '</li>';
            }).join('');
        }
    }

    function renderTopProducts(products) {
        var canvas = qs('chartTopProducts');
        if (!canvas || typeof Chart === 'undefined') return;

        var items = (products || []).slice().reverse();
        var labels = items.map(function (p) {
            var name = p.productName || '';
            return name.length > 28 ? name.slice(0, 26) + '…' : name;
        });
        var revenue = items.map(function (p) { return Number(p.revenue) || 0; });

        destroyChart('topProducts');
        charts.topProducts = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Ingresos',
                    data: revenue,
                    backgroundColor: 'rgba(14, 165, 233, 0.75)',
                    hoverBackgroundColor: '#0ea5e9',
                    borderRadius: 6,
                    borderSkipped: false
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        padding: 12,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (ctx) { return formatMoney(ctx.parsed.x); }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { color: '#f1f5f9' },
                        ticks: {
                            font: { size: 11 },
                            color: '#94a3b8',
                            callback: function (v) { return 'S/ ' + Number(v).toLocaleString('es-PE'); }
                        }
                    },
                    y: {
                        grid: { display: false },
                        ticks: { font: { size: 11 }, color: '#64748b' }
                    }
                }
            }
        });
    }

    function renderDashboard(data) {
        var period = data.period || {};
        if (qs('statsPeriodBadge')) {
            qs('statsPeriodBadge').textContent = period.label || 'Período seleccionado';
        }

        updateKpis(data.summary);
        renderTrendLine(data.trend);
        renderSalesBar(data.trend);
        renderProfitArea(data.trend);
        renderPayments(data.payments);
        renderTopProducts(data.topProducts);
    }

    function loadData() {
        if (!config.dataUrl) return;

        setRefreshing(true);

        fetch(buildUrl(), {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (r) {
                if (!r.ok) throw new Error('Error');
                return r.json();
            })
            .then(renderDashboard)
            .catch(function () {
                if (qs('statsPeriodBadge')) qs('statsPeriodBadge').textContent = 'Error al cargar datos';
            })
            .finally(function () {
                setRefreshing(false);
            });
    }

    function setPreset(preset) {
        state.preset = preset;
        document.querySelectorAll('.stats-preset').forEach(function (btn) {
            btn.classList.toggle('active', btn.getAttribute('data-preset') === preset);
        });

        var customRange = qs('statsCustomRange');
        if (customRange) customRange.hidden = preset !== 'custom';

        if (preset !== 'custom') {
            state.page = 1;
            loadData();
        }
    }

    function bindEvents() {
        document.querySelectorAll('.stats-preset').forEach(function (btn) {
            btn.addEventListener('click', function () {
                setPreset(btn.getAttribute('data-preset') || 'today');
            });
        });

        var applyBtn = qs('statsApplyRange');
        if (applyBtn) {
            applyBtn.addEventListener('click', function () {
                var from = qs('statsDateFrom');
                var to = qs('statsDateTo');
                state.dateFrom = from ? from.value : '';
                state.dateTo = to ? to.value : '';
                if (!state.dateFrom || !state.dateTo) return;
                loadData();
            });
        }

        var today = new Date();
        var todayStr = today.toISOString().slice(0, 10);

        if (qs('statsDateFrom')) qs('statsDateFrom').value = todayStr;
        if (qs('statsDateTo')) qs('statsDateTo').value = todayStr;
    }

    function init() {
        var root = qs('statsRoot');
        if (!root || !config.dataUrl) return;
        if (initialized && root.dataset.statsInit === 'true') return;

        root.dataset.statsInit = 'true';
        initialized = true;

        bindEvents();

        if (config.initialData) {
            renderDashboard(config.initialData);
        } else {
            loadData();
        }
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
