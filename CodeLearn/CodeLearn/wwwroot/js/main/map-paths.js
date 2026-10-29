(function () {
  'use strict';

  var dataNode = document.getElementById('map-modules-data');
  var mods = window.MAP_MODULES;

  if (!mods && dataNode) {
    try {
      mods = JSON.parse(dataNode.textContent || '[]');
    } catch (error) {
      mods = [];
    }
  }

  if (!mods || mods.length < 2) return;

  var map = document.querySelector('.levels-map');
  if (!map) return;

  var svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('class', 'map-paths-svg');
  map.insertBefore(svg, map.firstChild);

  var ICON_W = 100;
  var ICON_H = 84;
  var START_INSET = 58;
  var END_INSET = 64;
  var ARROW_STEP = 58;
  var REDRAW_DELAY = 80;
  var resizeTimer = 0;

  function center(mod) {
    return {
      x: (mod.x / 100) * map.offsetWidth + ICON_W / 2,
      y: (mod.y / 100) * map.offsetHeight + ICON_H / 2
    };
  }

  function svgEl(tag, attrs) {
    var element = document.createElementNS('http://www.w3.org/2000/svg', tag);
    Object.keys(attrs).forEach(function (key) {
      element.setAttribute(key, attrs[key]);
    });
    return element;
  }

  function lerp(a, b, t) {
    return a + (b - a) * t;
  }

  function clamp01(value) {
    return Math.max(0, Math.min(1, value));
  }

  function drawConnection(from, to) {
    var a = center(from);
    var b = center(to);
    var dx = b.x - a.x;
    var dy = b.y - a.y;
    var dist = Math.sqrt(dx * dx + dy * dy);
    if (dist < 4) return;

    var active = from.isUnlocked;
    var nextAvailable = active && !to.isUnlocked;
    var color = active ? (from.color || '#4caf50') : '#1c2028';

    var startInset = Math.min(START_INSET, dist * 0.35);
    var endInset = Math.min(END_INSET, dist * 0.35);
    if (startInset + endInset >= dist) {
      startInset = dist * 0.22;
      endInset = dist * 0.22;
    }

    var sx = lerp(a.x, b.x, clamp01(startInset / dist));
    var sy = lerp(a.y, b.y, clamp01(startInset / dist));
    var ex = lerp(a.x, b.x, clamp01(1 - endInset / dist));
    var ey = lerp(a.y, b.y, clamp01(1 - endInset / dist));
    var len = Math.max(0, dist - startInset - endInset);
    if (len < 8) return;

    var angle = Math.atan2(dy, dx) * 180 / Math.PI;
    var group = svgEl('g', {
      'class': active ? 'map-path is-active' : 'map-path is-locked'
    });

    var line = svgEl('line', {
      x1: sx,
      y1: sy,
      x2: ex,
      y2: ey,
      stroke: color,
      'stroke-width': active ? '2' : '1',
      'stroke-dasharray': '5 6',
      'stroke-linecap': 'square',
      'stroke-opacity': active ? '0.55' : '0.18',
      'class': active ? 'map-path-line' : ''
    });
    group.appendChild(line);

    var count = Math.max(1, Math.floor(len / ARROW_STEP));
    for (var j = 1; j <= count; j++) {
      var t = j / (count + 1);
      var cx = lerp(sx, ex, t);
      var cy = lerp(sy, ey, t);
      var size = active ? 5 : 4;

      var arrowGroup = svgEl('g', {
        transform: 'translate(' + cx + ',' + cy + ') rotate(' + angle + ')'
      });

      var arrow = svgEl('polyline', {
        points: (-size) + ',' + (-size) + ' 0,0 ' + (-size) + ',' + size,
        stroke: color,
        'stroke-width': active ? '2.5' : '1.5',
        fill: 'none',
        'stroke-linecap': 'square',
        'stroke-linejoin': 'miter',
        'stroke-opacity': active ? '1' : '0.22',
        'class': active ? 'map-path-arrow' : ''
      });

      if (active) arrow.style.animationDelay = (j * 220) + 'ms';

      arrowGroup.appendChild(arrow);
      group.appendChild(arrowGroup);
    }

    if (nextAvailable) {
      var dot = svgEl('circle', {
        cx: b.x,
        cy: b.y,
        r: '5',
        fill: to.color || '#ffffff',
        'fill-opacity': '0.18',
        stroke: to.color || '#ffffff',
        'stroke-width': '1',
        'stroke-opacity': '0.5',
        'class': 'map-path-dot'
      });
      group.appendChild(dot);
    }

    svg.appendChild(group);
  }

  function render() {
    if (!map.offsetWidth || !map.offsetHeight) return;
    while (svg.lastChild) svg.removeChild(svg.lastChild);

    // ── Основная цепочка: только обычные модули (не испытания) ──────────────
    var regular = mods.filter(function (m) { return !m.isChallenge; });
    for (var i = 0; i < regular.length - 1; i++) {
      drawConnection(regular[i], regular[i + 1]);
    }

    // ── Ответвления: каждое испытание ← предыдущий обычный модуль ───────────
    mods.forEach(function (m) {
      if (!m.isChallenge) return;
      // Найти ближайший обычный модуль с меньшим order
      var prev = null;
      for (var k = 0; k < regular.length; k++) {
        if (regular[k].order < m.order) prev = regular[k];
      }
      if (prev) drawConnection(prev, m);
    });
  }

  render();
  window.addEventListener('resize', function () {
    window.clearTimeout(resizeTimer);
    resizeTimer = window.setTimeout(render, REDRAW_DELAY);
  });

  // ── Публичный API: для редактора карты (динамическая перерисовка) ──────────
  window.MapPaths = {
    render: render,
    // Обновить позицию модуля по id и перерисовать стрелки
    setPosition: function (id, x, y) {
      for (var i = 0; i < mods.length; i++) {
        if (mods[i].id === id) {
          mods[i].x = x;
          mods[i].y = y;
          break;
        }
      }
      render();
    }
  };
})();
