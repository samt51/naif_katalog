(function () {
    'use strict';
    const root = document.getElementById('performanceDiagnostics');
    if (!root) return;
    const panel = document.getElementById('perfDiagnosticsPanel');
    const result = document.getElementById('perfDiagnosticsResult');
    const round = value => Math.max(0, Math.round(Number(value) || 0));
    const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

    function analyze() {
        const navigation = performance.getEntriesByType('navigation')[0];
        const resources = performance.getEntriesByType('resource');
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        const ttfb = navigation ? navigation.responseStart - navigation.requestStart : 0;
        const download = navigation ? navigation.responseEnd - navigation.responseStart : 0;
        const render = navigation ? navigation.loadEventEnd - navigation.responseEnd : 0;
        const apiRequests = resources.filter(x => /\/api\/|api\.|apib2b/i.test(x.name));
        const images = resources.filter(x => x.initiatorType === 'img');
        const avgApi = apiRequests.length ? apiRequests.reduce((sum,x) => sum + x.duration, 0) / apiRequests.length : 0;
        const slowest = resources.slice().sort((a,b) => b.duration - a.duration).slice(0,3);
        const imageTransfer = images.reduce((sum,x) => sum + (x.transferSize || 0), 0);

        let level = 'good';
        let diagnosis = 'Sayfa normal hızda çalışıyor.';
        if (ttfb > 1500 || avgApi > 1800) {
            level = 'slow'; diagnosis = 'Muhtemel neden: API / veritabanı sunucu beklemesi.';
        } else if (download > 1200 || imageTransfer > 8 * 1024 * 1024 || (connection && connection.downlink && connection.downlink < 3)) {
            level = 'slow'; diagnosis = 'Muhtemel neden: internet bağlantısı veya büyük görseller.';
        } else if (render > 1200) {
            level = 'medium'; diagnosis = 'Muhtemel neden: tarayıcı render/JavaScript işlemleri.';
        } else if (ttfb > 700 || avgApi > 800 || download > 600) {
            level = 'medium'; diagnosis = 'Orta seviyede gecikme ölçüldü; aşağıdaki süreleri kontrol edin.';
        }
        root.dataset.level = level;
        const connectionText = connection ? `${connection.effectiveType || '?'}${connection.downlink ? ` / ~${connection.downlink} Mbps` : ''}` : 'Tarayıcı bildirmiyor';
        result.innerHTML = `<div class="perf-diagnostic-summary ${level}">${escapeHtml(diagnosis)}</div>
            <div class="perf-metric"><span>Sayfa sunucu bekleme (TTFB)</span><strong>${round(ttfb)} ms</strong></div>
            <div class="perf-metric"><span>Sayfa indirme</span><strong>${round(download)} ms</strong></div>
            <div class="perf-metric"><span>Tarayıcı/render</span><strong>${round(render)} ms</strong></div>
            <div class="perf-metric"><span>API ortalaması (${apiRequests.length})</span><strong>${round(avgApi)} ms</strong></div>
            <div class="perf-metric"><span>Görsel trafiği (${images.length})</span><strong>${(imageTransfer / 1048576).toFixed(2)} MB</strong></div>
            <div class="perf-metric"><span>Bağlantı tahmini</span><strong>${escapeHtml(connectionText)}</strong></div>
            <div class="perf-metric"><span>Uygulama sürümü</span><strong>${escapeHtml(document.getElementById('app-version')?.dataset.version || '-')}</strong></div>
            <div class="perf-slowest"><strong>En yavaş istekler</strong>${slowest.length ? slowest.map(x => `<div>${round(x.duration)} ms — ${escapeHtml(new URL(x.name, location.href).pathname)}</div>`).join('') : '<div>Henüz kaynak isteği ölçülmedi.</div>'}</div>`;
        window.appPerformanceDiagnostics = { level, diagnosis, ttfb: round(ttfb), download: round(download), render: round(render), averageApi: round(avgApi), imageMegabytes: Number((imageTransfer / 1048576).toFixed(2)), connection: connectionText };
    }

    document.getElementById('perfDiagnosticsToggle').addEventListener('click', () => { analyze(); panel.hidden = false; });
    document.getElementById('perfDiagnosticsClose').addEventListener('click', () => panel.hidden = true);
    document.getElementById('perfDiagnosticsRefresh').addEventListener('click', analyze);
    window.addEventListener('load', () => setTimeout(analyze, 250), { once: true });
})();
