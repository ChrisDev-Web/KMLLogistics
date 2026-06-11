(function () {
    'use strict';

    const cfg = window.statsConfig || {};
    const COLORS = {
        sales: '#2563eb',
        purchases: '#ea580c',
        profit: '#16a34a',
        flow: '#7c3aed',
        payments: ['#2563eb', '#16a34a', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#ec4899']
    };

    let charts = {};
    let currentData = cfg.initialData || null;
    let activePreset = 'last30';
    let activeSeries = 'all';
    let currentTrendGranularity = 'daily';

    const $ = (sel) => document.querySelector(sel);
    const $$ = (sel) => document.querySelectorAll(sel);

    function fmtMoney(v) {
        return 'S/ ' + Number(v || 0).toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function fmtPct(v) {
        const n = Number(v || 0);
        const sign = n > 0 ? '+' : '';
        return sign + n.toFixed(1) + '%';
    }

    function parseDateLabel(raw) {
        if (!raw) return '';
        const s = String(raw);
        const m = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
        if (m) return new Date(+m[1], +m[2] - 1, +m[3]);
        const d = new Date(s);
        return isNaN(d) ? new Date() : d;
    }

    function formatDateShort(d) {
        return d.toLocaleDateString('es-PE', { day: '2-digit', month: 'short' });
    }

    function formatDateTime(d) {
        const dt = parseDateLabel(d);
        return dt.toLocaleString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function destroyChart(key) {
        if (charts[key]) {
            charts[key].destroy();
            charts[key] = null;
        }
    }

    function setBadge(el, metric) {
        if (!el || !metric) return;
        const pct = Number(metric.changePercent || 0);
        el.textContent = fmtPct(pct) + ' vs período anterior';
        el.classList.remove('up', 'down', 'flat');
        if (pct > 0) el.classList.add('up');
        else if (pct < 0) el.classList.add('down');
        else el.classList.add('flat');
    }

    function updateKpis(data) {
        const s = data.summary || {};
        const c = data.comparison || {};
        const netFlow = (s.totalSales || 0) - (s.totalPurchases || 0);

        const elSales = $('#kpiTotalSales');
        const elPurch = $('#kpiTotalPurchases');
        const elFlow = $('#kpiNetBalance');
        const elProfit = $('#kpiNetProfit');

        if (elSales) elSales.textContent = fmtMoney(s.totalSales);
        if (elPurch) elPurch.textContent = fmtMoney(s.totalPurchases);
        if (elFlow) elFlow.textContent = fmtMoney(netFlow);
        if (elProfit) elProfit.textContent = fmtMoney(s.netProfit);

        setBadge($('#kpiBadgeSales'), c.sales);
        setBadge($('#kpiBadgePurchases'), c.purchases);
        setBadge($('#kpiBadgeFlow'), c.netFlow);
        setBadge($('#kpiBadgeProfit'), c.profit);

        renderSparklines(data.trend || []);
    }

    function renderSparklines(trend) {
        const sales = trend.map(t => Number(t.salesAmount || 0));
        const purch = trend.map(t => Number(t.purchasesAmount || 0));
        const profit = trend.map(t => Number(t.netProfit || 0));
        const flow = trend.map(t => Number(t.salesAmount || 0) - Number(t.purchasesAmount || 0));

        makeSpark('sparkSales', sales, COLORS.sales);
        makeSpark('sparkPurchases', purch, COLORS.purchases);
        makeSpark('sparkFlow', flow, COLORS.flow);
        makeSpark('sparkProfit', profit, COLORS.profit);
    }

    function makeSpark(canvasId, values, color) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        destroyChart(canvasId);
        const data = values.length ? values : [0];
        charts[canvasId] = new Chart(canvas, {
            type: 'line',
            data: {
                labels: data.map((_, i) => i),
                datasets: [{
                    data,
                    borderColor: color,
                    backgroundColor: color + '22',
                    fill: true,
                    tension: 0.4,
                    pointRadius: 0,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false }, tooltip: { enabled: false } },
                scales: {
                    x: { display: false },
                    y: { display: false }
                },
                animation: { duration: 400 }
            }
        });
    }

    function formatHourSlot(slot) {
        const h = Number(slot) || 0;
        return String(h).padStart(2, '0') + ':00';
    }

    function getTrendLabels(trend, granularity) {
        const mode = granularity || currentTrendGranularity;
        if (mode === 'hourly') {
            return (trend || []).map(t => formatHourSlot(t.hourSlot));
        }
        return (trend || []).map(t => formatDateShort(parseDateLabel(t.periodDate)));
    }

    function updateTrendSubtitle(granularity) {
        const el = document.getElementById('trendChartSubtitle');
        if (!el) return;
        el.textContent = granularity === 'hourly'
            ? 'Desglose por hora del día (cada 2 h)'
            : 'Historial operativo del período';
    }

    function syncPresetUI(preset) {
        if (!preset) return;
        activePreset = preset;
        $$('#statsPresets .dash-pill').forEach(b => {
            b.classList.toggle('active', b.dataset.preset === preset);
        });
        const customRange = document.getElementById('statsCustomRange');
        if (customRange) customRange.hidden = preset !== 'custom';
    }

    function renderTrendChart(trend, granularity) {
        const canvas = document.getElementById('chartTrend');
        if (!canvas) return;
        destroyChart('chartTrend');

        const mode = granularity || currentTrendGranularity;
        const rows = trend || [];
        const labels = getTrendLabels(rows, mode);
        const sales = rows.map(t => Number(t.salesAmount || 0));
        const purch = rows.map(t => Number(t.purchasesAmount || 0));
        const profit = rows.map(t => Number(t.netProfit || 0));

        const lineStyle = (color, fill) => ({
            borderColor: color,
            backgroundColor: color + '22',
            tension: 0.35,
            fill: fill,
            borderWidth: 2.5,
            pointRadius: 0,
            pointHoverRadius: 5,
            pointBackgroundColor: color,
            pointBorderColor: '#fff',
            pointBorderWidth: 2,
            spanGaps: true
        });

        const datasets = [];
        if (activeSeries === 'all' || activeSeries === 'sales') {
            datasets.push({
                label: 'Ventas',
                data: sales,
                ...lineStyle(COLORS.sales, activeSeries === 'all')
            });
        }
        if (activeSeries === 'all' || activeSeries === 'purchases') {
            datasets.push({
                label: 'Compras',
                data: purch,
                ...lineStyle(COLORS.purchases, false)
            });
        }
        if (activeSeries === 'all' || activeSeries === 'profit') {
            datasets.push({
                label: 'Ganancia',
                data: profit,
                ...lineStyle(COLORS.profit, false)
            });
        }

        charts.chartTrend = new Chart(canvas, {
            type: 'line',
            data: { labels, datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { display: activeSeries === 'all', position: 'bottom' },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => ctx.dataset.label + ': ' + fmtMoney(ctx.parsed.y)
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: (v) => 'S/ ' + Number(v).toLocaleString('es-PE')
                        },
                        grid: { color: '#e2e8f0', drawBorder: false }
                    },
                    x: {
                        ticks: {
                            maxTicksLimit: mode === 'hourly' ? 12 : 12,
                            maxRotation: 0,
                            autoSkip: mode !== 'hourly'
                        },
                        grid: { color: '#f1f5f9', drawBorder: false }
                    }
                }
            }
        });
    }

    function renderDonut(canvasId, centerId, legendId, items, centerLabel) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        destroyChart(canvasId);

        const labels = items.map(i => i.label);
        const values = items.map(i => i.value);
        const colors = items.map((_, i) => COLORS.payments[i % COLORS.payments.length]);
        const total = values.reduce((a, b) => a + b, 0);

        const center = document.getElementById(centerId);
        if (center) center.textContent = centerLabel || fmtMoney(total);

        charts[canvasId] = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderWidth: 0,
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const pct = total ? ((ctx.parsed / total) * 100).toFixed(0) : 0;
                                return ctx.label + ': ' + fmtMoney(ctx.parsed) + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });

        const legend = document.getElementById(legendId);
        if (!legend) return;
        if (!items.length) {
            legend.innerHTML = '<li class="dash-empty">Sin datos en el período</li>';
            return;
        }
        legend.innerHTML = items.map((item, i) => {
            const pct = total ? ((item.value / total) * 100).toFixed(0) : 0;
            return `<li>
                <span class="dash-legend__label">
                    <span class="dash-legend__dot" style="background:${colors[i]}"></span>
                    <span>${item.label}</span>
                </span>
                <span class="dash-legend__val">${fmtMoney(item.value)} (${pct}%)</span>
            </li>`;
        }).join('');
    }

    function renderFlowChart(data) {
        const s = data.summary || {};
        const sales = Number(s.totalSales || 0);
        const purch = Number(s.totalPurchases || 0);
        const net = sales - purch;

        renderDonut('chartFlow', 'flowCenter', 'flowLegend', [
            { label: 'Ingresos (ventas)', value: sales },
            { label: 'Egresos (compras)', value: purch }
        ], fmtMoney(net));
    }

    function renderPaymentsChart(payments) {
        const items = (payments || []).map(p => ({
            label: p.paymentMethod || 'Otro',
            value: Number(p.totalAmount || 0)
        }));
        const total = items.reduce((a, b) => a + b.value, 0);
        renderDonut('chartPayments', 'paymentCenter', 'paymentLegend', items, fmtMoney(total));
    }

    function renderHourlyChart(hourly) {
        const canvas = document.getElementById('chartHourly');
        if (!canvas) return;
        destroyChart('chartHourly');

        const rows = hourly || [];
        const labels = rows.map(h => String(h.hourOfDay).padStart(2, '0') + ':00');
        const sales = rows.map(h => Number(h.salesAmount || 0));
        const purch = rows.map(h => Number(h.purchasesAmount || 0));

        charts.chartHourly = new Chart(canvas, {
            type: 'bar',
            data: {
                labels,
                datasets: [
                    { label: 'Ingresos', data: sales, backgroundColor: COLORS.sales + 'cc', borderRadius: 3 },
                    { label: 'Egresos', data: purch, backgroundColor: COLORS.purchases + 'cc', borderRadius: 3 }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        callbacks: { label: (ctx) => ctx.dataset.label + ': ' + fmtMoney(ctx.parsed.y) }
                    }
                },
                scales: {
                    y: {
                        ticks: { callback: (v) => 'S/ ' + Number(v).toLocaleString('es-PE') },
                        grid: { color: '#f1f5f9' }
                    },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    function renderResultChart(data) {
        const canvas = document.getElementById('chartResult');
        if (!canvas) return;
        destroyChart('chartResult');

        const s = data.summary || {};
        const sales = Number(s.totalSales || 0);
        const purch = Number(s.totalPurchases || 0);
        const profit = Number(s.netProfit || 0);
        const net = sales - purch;

        charts.chartResult = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: ['Ventas', 'Compras', 'Ganancia', 'Balance'],
                datasets: [{
                    data: [sales, -purch, profit, net],
                    backgroundColor: [COLORS.sales, COLORS.purchases, COLORS.profit, COLORS.flow],
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => fmtMoney(Math.abs(ctx.parsed.y))
                        }
                    }
                },
                scales: {
                    y: {
                        ticks: { callback: (v) => 'S/ ' + Number(v).toLocaleString('es-PE') },
                        grid: { color: '#f1f5f9' }
                    },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    function renderCategoryBars(categories) {
        const container = document.getElementById('categoryBars');
        if (!container) return;

        const rows = categories || [];
        if (!rows.length) {
            container.innerHTML = '<div class="dash-empty">Sin ventas por categoría</div>';
            return;
        }

        const total = rows.reduce((a, r) => a + Number(r.revenue || 0), 0);
        container.innerHTML = rows.map(r => {
            const rev = Number(r.revenue || 0);
            const pct = total ? ((rev / total) * 100).toFixed(0) : 0;
            return `<div class="dash-bar-row">
                <div class="dash-bar-row__head">
                    <span class="dash-bar-row__name">${r.categoryName || '—'}</span>
                    <span class="dash-bar-row__pct">${pct}% · ${fmtMoney(rev)}</span>
                </div>
                <div class="dash-bar-row__track">
                    <div class="dash-bar-row__fill" style="width:${pct}%"></div>
                </div>
            </div>`;
        }).join('');
    }

    function renderProductsChart(products) {
        const canvas = document.getElementById('chartProducts');
        if (!canvas) return;
        destroyChart('chartProducts');

        const rows = (products || []).slice(0, 8);
        const labels = rows.map(p => p.productName || '—');
        const values = rows.map(p => Number(p.revenue || 0));

        charts.chartProducts = new Chart(canvas, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Ingresos',
                    data: values,
                    backgroundColor: COLORS.flow + 'cc',
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: { label: (ctx) => fmtMoney(ctx.parsed.x) }
                    }
                },
                scales: {
                    x: {
                        ticks: { callback: (v) => 'S/ ' + Number(v).toLocaleString('es-PE') },
                        grid: { color: '#f1f5f9' }
                    },
                    y: { grid: { display: false } }
                }
            }
        });
    }

    function renderStockAlerts(alerts) {
        const tbody = document.getElementById('stockAlertsBody');
        if (!tbody) return;
        const rows = alerts || [];
        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="dash-empty">Sin alertas de stock activas</td></tr>';
            return;
        }
        tbody.innerHTML = rows.map(a => `<tr>
            <td>${a.productName || '—'}</td>
            <td>${a.warehouseName || '—'}</td>
            <td>${a.stock ?? 0} u.</td>
            <td><span class="dash-badge dash-badge--low">Bajo</span></td>
        </tr>`).join('');
    }

    function renderRecentSales(sales) {
        const tbody = document.getElementById('recentSalesBody');
        if (!tbody) return;
        const rows = sales || [];
        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="dash-empty">Sin ventas en el período</td></tr>';
            return;
        }
        tbody.innerHTML = rows.map(s => `<tr>
            <td>${formatDateTime(s.createdAt)}</td>
            <td>${s.clientName || '—'}</td>
            <td>${s.paymentMethod || '—'}</td>
            <td>${fmtMoney(s.total)}</td>
        </tr>`).join('');
    }

    function renderAll(data) {
        if (!data) return;
        currentData = data;

        const badge = document.getElementById('statsPeriodBadge');
        if (badge && data.period) badge.textContent = data.period.label || '';
        if (data.period?.preset) syncPresetUI(data.period.preset);

        currentTrendGranularity = data.trendGranularity || 'daily';
        updateTrendSubtitle(currentTrendGranularity);

        updateKpis(data);
        renderTrendChart(data.trend || [], currentTrendGranularity);
        renderFlowChart(data);
        renderPaymentsChart(data.payments || []);
        renderHourlyChart(data.hourly || []);
        renderResultChart(data);
        renderCategoryBars(data.topCategories || []);
        renderProductsChart(data.topProducts || []);
        renderStockAlerts(data.stockAlerts || []);
        renderRecentSales(data.recentSales || []);
    }

    async function loadData(preset, dateFrom, dateTo) {
        const params = new URLSearchParams();
        if (preset) params.set('preset', preset);
        if (dateFrom) params.set('dateFrom', dateFrom);
        if (dateTo) params.set('dateTo', dateTo);

        try {
            const res = await fetch(cfg.dataUrl + '?' + params.toString(), {
                headers: { Accept: 'application/json' }
            });
            if (!res.ok) throw new Error('Error al cargar datos');
            const data = await res.json();
            renderAll(data);
        } catch (err) {
            console.error(err);
        }
    }

    function bindPresets() {
        const presets = document.getElementById('statsPresets');
        const customRange = document.getElementById('statsCustomRange');
        if (!presets) return;

        presets.addEventListener('click', (e) => {
            const btn = e.target.closest('[data-preset]');
            if (!btn) return;

            $$('#statsPresets .dash-pill').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            const preset = btn.dataset.preset;
            activePreset = preset;

            if (preset === 'custom') {
                if (customRange) customRange.hidden = false;
                return;
            }
            if (customRange) customRange.hidden = true;
            loadData(preset);
        });

        const applyBtn = document.getElementById('statsApplyRange');
        if (applyBtn) {
            applyBtn.addEventListener('click', () => {
                const from = document.getElementById('statsDateFrom')?.value;
                const to = document.getElementById('statsDateTo')?.value;
                if (!from || !to) return;
                loadData('custom', from, to);
            });
        }
    }

    function bindSeriesToggles() {
        const toggles = document.getElementById('seriesToggles');
        if (!toggles) return;

        toggles.addEventListener('click', (e) => {
            const btn = e.target.closest('[data-series]');
            if (!btn) return;
            $$('#seriesToggles .dash-series__btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activeSeries = btn.dataset.series;
            if (currentData) renderTrendChart(currentData.trend || [], currentTrendGranularity);
        });
    }

    function initDates() {
        const today = new Date();
        const from = document.getElementById('statsDateFrom');
        const to = document.getElementById('statsDateTo');
        const iso = (d) => d.toISOString().slice(0, 10);
        if (from) from.value = iso(new Date(today.getFullYear(), today.getMonth(), 1));
        if (to) to.value = iso(today);
    }

    function init() {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no cargado');
            return;
        }
        initDates();
        bindPresets();
        bindSeriesToggles();
        if (currentData?.period?.preset) syncPresetUI(currentData.period.preset);
        if (currentData) renderAll(currentData);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
