// wwwroot/js/main/map-objects.js
// Загружает и отображает MapObjects на игровой карте.

(function () {
    'use strict';

    if (window.__mapObjectsLoaded) return;
    window.__mapObjectsLoaded = true;

    const courseId = window.__mapCourseId ?? null;
    if (!courseId) return;

    const levelsMap = document.querySelector('.levels-map') ?? document.querySelector('.levels-main');
    if (!levelsMap) return;

    function clearOldObjects() {
        levelsMap.querySelectorAll('.map-obj').forEach(el => el.remove());
    }

    // ── Расширение карты вверх ────────────────────────────────────────────────
    // main-container изначально height:250vh (задано в CSS).
    // Нам нужно добавить к этому extPx * scale экранных пикселей.
    // start-position.js применяет scale в DOMContentLoaded — ждём его через window.load
    // или requestAnimationFrame после DOMContentLoaded.
    // Расширение карты вверх берём С СЕРВЕРА (одно для всех игроков), а не из localStorage.
    let topExtPx = 0;

    function applyTopExtension() {
        if (topExtPx <= 0) return;

        const container = document.querySelector('.main-container');
        const screenMain = document.querySelector('.screen-main');
        if (!container || !screenMain) return;

        // Читаем scale который уже применён start-position.js
        const matrix = new DOMMatrix(getComputedStyle(screenMain).transform);
        const scale = matrix.a || 1;  // matrix.a = scaleX

        // main-container высота из CSS = 250vh, берём реальную px
        container.style.height = '';
        const baseH = container.getBoundingClientRect().height;

        // Добавляем экранные пиксели сверху
        container.style.height = (baseH + topExtPx * scale) + 'px';
    }

    // Ждём rAF чтобы start-position.js успел выставить масштаб
    function scheduleApply() {
        requestAnimationFrame(() => requestAnimationFrame(applyTopExtension));
    }

    fetch('/api/builder/map-settings?courseId=' + courseId)
        .then(r => r.json())
        .then(s => { topExtPx = parseInt(s.topExtPx, 10) || 0; scheduleApply(); })
        .catch(() => { });

    // ── Рендер объектов ───────────────────────────────────────────────────────
    clearOldObjects();

    fetch('/api/builder/map-objects?courseId=' + courseId)
        .then(r => r.json())
        .then(objects => { objects.forEach(renderObject); })
        .catch(() => { });

    function renderObject(obj) {
        const wrap = document.createElement('div');
        wrap.className = 'map-obj';
        var hasH = obj.height && obj.height > 0;
        wrap.style.cssText = [
            'position:absolute',
            'left:' + obj.mapX + '%',
            'top:' + obj.mapY + '%',
            'width:' + obj.width + 'px',
            (hasH ? 'height:' + obj.height + 'px' : ''),
            (obj.invert ? 'filter:invert(1)' : ''),
            'transform:translate(-50%,-50%) rotate(' + (obj.rotation || 0) + 'deg)',
            'z-index:' + ((obj.zIndex || 0) + 1),
            'pointer-events:none',
            'user-select:none',
        ].filter(Boolean).join(';');

        if (obj.type === 'svg_inline') {
            wrap.innerHTML = obj.src;
            const svg = wrap.querySelector('svg');
            if (svg) {
                svg.style.width = '100%';
                svg.style.display = 'block';
                if (hasH) { svg.style.height = '100%'; svg.setAttribute('preserveAspectRatio', 'none'); }
                else      { svg.style.height = 'auto'; }
            }
        } else if (obj.type === 'image_url') {
            const img = document.createElement('img');
            img.src = obj.src;
            img.style.cssText = hasH ? 'width:100%;height:100%;display:block' : 'width:100%;height:auto;display:block';
            img.draggable = false;
            wrap.appendChild(img);
        } else if (obj.type === 'emoji') {
            wrap.style.fontSize = (obj.width * 0.8) + 'px';
            wrap.style.lineHeight = '1';
            wrap.style.display = 'flex';
            wrap.style.alignItems = 'center';
            wrap.style.justifyContent = 'center';
            wrap.textContent = obj.src;
        }

        levelsMap.appendChild(wrap);
    }
})();
