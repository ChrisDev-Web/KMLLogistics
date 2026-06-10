(function () {
    'use strict';

    var pollTimer = null;
    var countTimer = null;
    var animHandle = null;
    var animTick = 0;
    var cachedItems = [];
    var selectedId = null;

    function urls() { return window.sgvUrls || {}; }
    function qs(id) { return document.getElementById(id); }
    function getToken() { var i = document.querySelector('input[name="__RequestVerificationToken"]'); return i ? i.value : ''; }

    function escapeHtml(t) { var d = document.createElement('div'); d.textContent = t == null ? '' : String(t); return d.innerHTML; }
    function fmtDate(v) { return v ? new Date(v).toLocaleString('es-PE') : '—'; }
    function pct(v) { return Math.round(Math.max(0, Math.min(1, Number(v) || 0)) * 100); }

    function pad2(n) { return String(n).padStart(2, '0'); }

    function formatRemaining(ms) {
        if (ms <= 0) return { text: '00:00:00', arrived: true };
        var totalSec = Math.floor(ms / 1000);
        var h = Math.floor(totalSec / 3600);
        var m = Math.floor((totalSec % 3600) / 60);
        var s = totalSec % 60;
        return { text: pad2(h) + ':' + pad2(m) + ':' + pad2(s), arrived: false };
    }

    function updateCountdownDisplay(item) {
        var bar = qs('sgvCountdownBar');
        var valueEl = qs('sgvCountdownValue');
        var subEl = qs('sgvCountdownSub');
        if (!bar || !valueEl || !subEl) return;

        if (!item || !item.arrivalDate) {
            bar.classList.add('sgv__countdown-bar--hidden');
            return;
        }

        bar.classList.remove('sgv__countdown-bar--hidden');
        var arrival = new Date(item.arrivalDate).getTime();
        var remaining = formatRemaining(arrival - Date.now());

        valueEl.textContent = remaining.arrived ? 'Llego al destino' : remaining.text;
        subEl.textContent = (item.vehiclePlate || '') + ' · ' + (item.clientName || 'Sin cliente')
            + ' — ETA ' + fmtDate(item.arrivalDate);
    }

    function stopCountdown() {
        if (countTimer) {
            clearInterval(countTimer);
            countTimer = null;
        }
    }

    function startCountdown() {
        stopCountdown();
        updateCountdownDisplay(getSelectedItem());
        countTimer = setInterval(function () {
            updateCountdownDisplay(getSelectedItem());
        }, 1000);
    }

    function getSelectedItem() {
        if (!selectedId) return null;
        return cachedItems.find(function (it) { return String(it.idShipment) === String(selectedId); }) || null;
    }

    function bezierPoint(p0, p1, p2, t) {
        var u = 1 - t;
        return {
            x: u * u * p0.x + 2 * u * t * p1.x + t * t * p2.x,
            y: u * u * p0.y + 2 * u * t * p1.y + t * t * p2.y
        };
    }

    function bezierTangent(p0, p1, p2, t) {
        var u = 1 - t;
        return {
            x: 2 * u * (p1.x - p0.x) + 2 * t * (p2.x - p1.x),
            y: 2 * u * (p1.y - p0.y) + 2 * t * (p2.y - p1.y)
        };
    }

    function isMoto(typeName) {
        return String(typeName || '').toLowerCase().indexOf('moto') >= 0;
    }

    function hasRoute(item) {
        if (!item) return false;
        var oLat = Number(item.originLatitude);
        var oLng = Number(item.originLongitude);
        var dLat = Number(item.destLatitude);
        var dLng = Number(item.destLongitude);
        if ([oLat, oLng, dLat, dLng].some(function (v) { return isNaN(v); })) return false;
        return !(oLat === dLat && oLng === dLng && !(Number(item.simulatedDistanceKm) > 0));
    }

    function drawHills(ctx, w, h) {
        ctx.fillStyle = '#4ade80';
        ctx.beginPath();
        ctx.moveTo(0, h * 0.72);
        ctx.quadraticCurveTo(w * 0.25, h * 0.58, w * 0.5, h * 0.7);
        ctx.quadraticCurveTo(w * 0.78, h * 0.82, w, h * 0.68);
        ctx.lineTo(w, h);
        ctx.lineTo(0, h);
        ctx.closePath();
        ctx.fill();

        ctx.fillStyle = '#22c55e';
        ctx.beginPath();
        ctx.moveTo(0, h * 0.78);
        ctx.quadraticCurveTo(w * 0.35, h * 0.66, w * 0.62, h * 0.76);
        ctx.quadraticCurveTo(w * 0.85, h * 0.84, w, h * 0.74);
        ctx.lineTo(w, h);
        ctx.lineTo(0, h);
        ctx.closePath();
        ctx.fill();
    }

    function drawRoadPath(ctx, points, width) {
        if (points.length < 2) return;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';

        ctx.strokeStyle = '#64748b';
        ctx.lineWidth = width + 10;
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        for (var i = 1; i < points.length; i++) ctx.lineTo(points[i].x, points[i].y);
        ctx.stroke();

        ctx.strokeStyle = '#334155';
        ctx.lineWidth = width;
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        for (var j = 1; j < points.length; j++) ctx.lineTo(points[j].x, points[j].y);
        ctx.stroke();

        ctx.strokeStyle = '#facc15';
        ctx.lineWidth = 2;
        ctx.setLineDash([14, 12]);
        ctx.lineDashOffset = -animTick * 0.6;
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        for (var k = 1; k < points.length; k++) ctx.lineTo(points[k].x, points[k].y);
        ctx.stroke();
        ctx.setLineDash([]);
    }

    function drawWarehouse(ctx, x, y) {
        ctx.fillStyle = '#16a34a';
        ctx.fillRect(x - 14, y - 18, 28, 22);
        ctx.fillStyle = '#14532d';
        ctx.beginPath();
        ctx.moveTo(x - 18, y - 18);
        ctx.lineTo(x, y - 30);
        ctx.lineTo(x + 18, y - 18);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = '#fff';
        ctx.fillRect(x - 6, y - 10, 12, 14);
        ctx.fillStyle = '#0f172a';
        ctx.font = 'bold 10px Segoe UI, sans-serif';
        ctx.fillText('Origen', x - 18, y + 14);
    }

    function drawDestination(ctx, x, y) {
        ctx.fillStyle = '#dc2626';
        ctx.beginPath();
        ctx.moveTo(x, y - 28);
        ctx.lineTo(x + 12, y - 8);
        ctx.lineTo(x - 12, y - 8);
        ctx.closePath();
        ctx.fill();
        ctx.fillStyle = '#fff';
        ctx.beginPath();
        ctx.arc(x, y - 14, 5, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillRect(x - 2, y - 8, 4, 16);
        ctx.fillStyle = '#0f172a';
        ctx.font = 'bold 10px Segoe UI, sans-serif';
        ctx.fillText('Destino', x - 20, y + 18);
    }

    function drawTruck(ctx, x, y, angle, moto, bounce) {
        ctx.save();
        ctx.translate(x, y + bounce);
        ctx.rotate(angle);

        if (moto) {
            ctx.fillStyle = '#2563eb';
            ctx.beginPath();
            ctx.ellipse(0, 2, 16, 6, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = '#1e40af';
            ctx.fillRect(-4, -8, 10, 8);
            ctx.fillStyle = '#0f172a';
            ctx.beginPath();
            ctx.arc(-8, 6, 4, 0, Math.PI * 2);
            ctx.arc(8, 6, 4, 0, Math.PI * 2);
            ctx.fill();
        } else {
            ctx.fillStyle = '#0f172a';
            ctx.beginPath();
            ctx.arc(-12, 10, 5, 0, Math.PI * 2);
            ctx.arc(14, 10, 5, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = '#2563eb';
            ctx.fillRect(-22, -6, 34, 14);
            ctx.fillStyle = '#1d4ed8';
            ctx.fillRect(8, -12, 16, 20);
            ctx.fillStyle = '#bae6fd';
            ctx.fillRect(11, -9, 10, 8);
            ctx.fillStyle = '#fbbf24';
            ctx.fillRect(22, -4, 4, 6);
        }

        ctx.restore();
    }

    function drawDust(ctx, x, y, tick) {
        ctx.fillStyle = 'rgba(148,163,184,0.35)';
        for (var i = 0; i < 3; i++) {
            var r = 3 + (tick % 10) * 0.2 + i;
            ctx.beginPath();
            ctx.arc(x - 18 - i * 6, y + 4 + Math.sin((tick + i) * 0.3) * 2, r, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    function drawRoadScene(canvas, item) {
        if (!canvas || !item) return;
        var ctx = canvas.getContext('2d');
        var w = canvas.width = canvas.clientWidth || 600;
        var h = canvas.height = canvas.clientHeight || 300;
        ctx.clearRect(0, 0, w, h);

        var sky = ctx.createLinearGradient(0, 0, 0, h);
        sky.addColorStop(0, '#38bdf8');
        sky.addColorStop(0.45, '#7dd3fc');
        sky.addColorStop(1, '#bbf7d0');
        ctx.fillStyle = sky;
        ctx.fillRect(0, 0, w, h);

        ctx.fillStyle = 'rgba(255,255,255,0.75)';
        ctx.beginPath();
        ctx.arc(w * 0.82, h * 0.18, 22, 0, Math.PI * 2);
        ctx.arc(w * 0.9, h * 0.18, 28, 0, Math.PI * 2);
        ctx.arc(w * 0.96, h * 0.2, 20, 0, Math.PI * 2);
        ctx.fill();

        drawHills(ctx, w, h);

        if (!hasRoute(item)) {
            ctx.fillStyle = 'rgba(255,255,255,0.85)';
            ctx.fillRect(w * 0.15, h * 0.38, w * 0.7, 48);
            ctx.fillStyle = '#475569';
            ctx.font = '14px Segoe UI, sans-serif';
            ctx.textAlign = 'center';
            ctx.fillText('Asocie una venta al envio para ver la ruta en el mapa.', w / 2, h * 0.44);
            ctx.textAlign = 'left';
            return;
        }

        var p0 = { x: 70, y: h * 0.72 };
        var p1 = { x: w * 0.5, y: h * 0.38 };
        var p2 = { x: w - 70, y: h * 0.62 };
        var steps = 40;
        var roadPts = [];
        for (var s = 0; s <= steps; s++) {
            roadPts.push(bezierPoint(p0, p1, p2, s / steps));
        }

        drawRoadPath(ctx, roadPts, 22);

        var progress = Math.max(0, Math.min(1, Number(item.routeProgress) || 0));
        var truckPos = bezierPoint(p0, p1, p2, progress);
        var tangent = bezierTangent(p0, p1, p2, progress);
        var angle = Math.atan2(tangent.y, tangent.x);
        var bounce = Math.sin(animTick * 0.12) * 1.5;

        drawWarehouse(ctx, p0.x, p0.y - 8);
        drawDestination(ctx, p2.x, p2.y - 6);

        if (progress > 0.02) drawDust(ctx, truckPos.x, truckPos.y, animTick);
        drawTruck(ctx, truckPos.x, truckPos.y - 6, angle, isMoto(item.vehicleTypeName), bounce);

        ctx.fillStyle = 'rgba(15,23,42,0.75)';
        ctx.font = 'bold 11px Segoe UI, sans-serif';
        ctx.fillText(item.vehiclePlate || 'Vehiculo', truckPos.x - 24, truckPos.y - 22);
    }

    function renderDetail(item) {
        var wrap = qs('sgvDetail');
        if (!wrap) return;

        if (!item) {
            wrap.innerHTML = '<div class="sgv__empty">No hay camiones en transito en este momento.</div>';
            stopAnimation();
            stopCountdown();
            updateCountdownDisplay(null);
            return;
        }

        var progress = pct(item.routeProgress);
        var eta = fmtDate(item.arrivalDate);

        wrap.innerHTML = '<article class="sgv__card" data-id="' + item.idShipment + '">'
            + '<div class="sgv__card-header"><h3>' + escapeHtml(item.vehiclePlate) + ' · ' + escapeHtml(item.vehicleTypeName) + '</h3>'
            + '<span class="sgv__badge">' + escapeHtml(item.shipmentStatusName) + '</span></div>'
            + '<div class="sgv__meta">'
            + '<div>Cliente: <strong>' + escapeHtml(item.clientName || '—') + '</strong></div>'
            + '<div>Destino: <strong>' + escapeHtml(item.deliveryAddress || 'Sin direccion') + '</strong></div>'
            + '<div>Distancia sim.: <strong>' + escapeHtml(item.simulatedDistanceKm) + ' km</strong></div>'
            + '<div>Salida: <strong>' + fmtDate(item.departureDate) + '</strong></div>'
            + '<div>Llegada est.: <strong>' + eta + '</strong></div>'
            + '</div>'
            + '<div class="sgv__legend">'
            + '<span><i class="sgv__dot--origin"></i> Almacen origen</span>'
            + '<span><i class="sgv__dot--dest"></i> Punto de entrega</span>'
            + '<span><i class="sgv__dot--truck"></i> Posicion actual (GPS simulado)</span>'
            + '</div>'
            + '<div class="sgv__scene-wrap"><canvas class="sgv__scene" id="sgvRoadCanvas"></canvas></div>'
            + '<div class="sgv__progress">'
            + '<div class="sgv__progress-bar"><div class="sgv__progress-fill" id="sgvProgressFill" style="width:' + progress + '%"></div></div>'
            + '<div class="sgv__progress-text">'
            + '<span>Progreso de ruta: <strong id="sgvProgressPct">' + progress + '%</strong></span>'
            + '<span>ETA: <strong id="sgvEta">' + escapeHtml(eta) + '</strong></span>'
            + '</div></div>'
            + '</article>';

        startAnimation();
        startCountdown();
    }

    function updateDetailMetrics(item) {
        if (!item) return;
        var progress = pct(item.routeProgress);
        var fill = qs('sgvProgressFill');
        var pctEl = qs('sgvProgressPct');
        var etaEl = qs('sgvEta');
        if (fill) fill.style.width = progress + '%';
        if (pctEl) pctEl.textContent = progress + '%';
        if (etaEl) etaEl.textContent = fmtDate(item.arrivalDate);
        updateCountdownDisplay(item);
    }

    function populateSelect(items) {
        var sel = qs('sgvVehicleSelect');
        if (!sel) return;

        var prev = selectedId || sel.value;
        sel.innerHTML = '<option value="">Seleccione un camion...</option>'
            + items.map(function (it) {
                return '<option value="' + it.idShipment + '">'
                    + escapeHtml(it.vehiclePlate) + ' · ' + escapeHtml(it.vehicleTypeName)
                    + ' — ' + escapeHtml(it.clientName || 'Sin cliente')
                    + '</option>';
            }).join('');

        if (prev && items.some(function (i) { return String(i.idShipment) === String(prev); })) {
            sel.value = String(prev);
            selectedId = prev;
        } else if (items.length) {
            selectedId = items[0].idShipment;
            sel.value = String(selectedId);
        } else {
            selectedId = null;
            sel.value = '';
        }
    }

    function applySelection(rebuild) {
        var item = getSelectedItem();
        if (rebuild || !qs('sgvRoadCanvas')) {
            renderDetail(item);
        } else {
            updateDetailMetrics(item);
        }
    }

    function stopAnimation() {
        if (animHandle) {
            cancelAnimationFrame(animHandle);
            animHandle = null;
        }
    }

    function startAnimation() {
        stopAnimation();
        function frame() {
            animTick++;
            var canvas = qs('sgvRoadCanvas');
            var item = getSelectedItem();
            if (canvas && item) drawRoadScene(canvas, item);
            animHandle = requestAnimationFrame(frame);
        }
        animHandle = requestAnimationFrame(frame);
    }

    function loadList() {
        fetch(urls().list + '?inTransitOnly=true', {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success === false) {
                    if (qs('sgvDetail')) qs('sgvDetail').innerHTML = '<div class="sgv__empty">' + escapeHtml(data.message || 'Error al cargar seguimiento.') + '</div>';
                    stopAnimation();
                    stopCountdown();
                    updateCountdownDisplay(null);
                    return;
                }
                cachedItems = data.items || [];
                populateSelect(cachedItems);
                applySelection(false);
            })
            .catch(function () {
                if (qs('sgvDetail')) qs('sgvDetail').innerHTML = '<div class="sgv__empty">Error al cargar seguimiento.</div>';
                stopAnimation();
                stopCountdown();
                updateCountdownDisplay(null);
            });
    }

    function syncAndLoad() {
        var token = getToken();
        var body = new URLSearchParams();
        body.append('__RequestVerificationToken', token);
        fetch(urls().sync, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body.toString(), credentials: 'same-origin' })
            .finally(loadList);
    }

    function bindEvents() {
        qs('sgvRefreshBtn')?.addEventListener('click', syncAndLoad);
        qs('sgvVehicleSelect')?.addEventListener('change', function () {
            selectedId = this.value || null;
            applySelection(true);
        });
        window.addEventListener('resize', function () {
            var canvas = qs('sgvRoadCanvas');
            var item = getSelectedItem();
            if (canvas && item) drawRoadScene(canvas, item);
        });
    }

    function init() {
        var root = qs('sgvRoot');
        if (!root || root.dataset.initialized === 'true' || !window.sgvUrls) return;
        root.dataset.initialized = 'true';
        bindEvents();
        syncAndLoad();
        if (pollTimer) clearInterval(pollTimer);
        pollTimer = setInterval(syncAndLoad, 15000);
    }

    document.addEventListener('DOMContentLoaded', init);
    document.addEventListener('dashboard:contentLoaded', init);
})();
