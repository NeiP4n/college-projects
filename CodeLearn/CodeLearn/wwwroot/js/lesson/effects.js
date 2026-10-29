/* ══════════════════════════════════════
   EFFECTS.JS — фоновые canvas-эффекты
   Используется на страницах урока и курса
══════════════════════════════════════ */

// ── Сетка ────────────────────────────────────────────────────────────────
(function () {
  const c = document.getElementById('bg-canvas');
  if (!c) return;
  const ctx = c.getContext('2d');
  function resize() {
    c.width = window.innerWidth; c.height = window.innerHeight;
    ctx.fillStyle = '#000'; ctx.fillRect(0, 0, c.width, c.height);
    ctx.strokeStyle = 'rgba(255,255,255,0.018)'; ctx.lineWidth = 1;
    for (let x = 0; x < c.width; x += 48) { ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, c.height); ctx.stroke(); }
    for (let y = 0; y < c.height; y += 48) { ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(c.width, y); ctx.stroke(); }
    ctx.strokeStyle = 'rgba(255,255,255,0.007)'; ctx.lineWidth = 1;
    for (let x = 0; x < c.width; x += 12) { ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, c.height); ctx.stroke(); }
    for (let y = 0; y < c.height; y += 12) { ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(c.width, y); ctx.stroke(); }
    const g = ctx.createRadialGradient(c.width / 2, c.height / 2, 0, c.width / 2, c.height / 2, Math.max(c.width, c.height) * .75);
    g.addColorStop(0, 'rgba(0,0,0,0)'); g.addColorStop(1, 'rgba(0,0,0,0.65)');
    ctx.fillStyle = g; ctx.fillRect(0, 0, c.width, c.height);
  }
  window.addEventListener('resize', resize); resize();
})();

// ── Бинарный дождь ────────────────────────────────────────────────────────
(function () {
  const c = document.getElementById('rain-canvas');
  if (!c) return;
  const ctx = c.getContext('2d');
  const chars = '01アイウエオ{}[]<>=+!#'.split('');
  const SZ = 13; let cols, drops;
  function init() {
    c.width = window.innerWidth; c.height = window.innerHeight;
    cols = Math.floor(c.width / SZ);
    drops = Array.from({ length: cols }, () => Math.random() * -80);
  }
  function draw() {
    ctx.fillStyle = 'rgba(0,0,0,0.05)'; ctx.fillRect(0, 0, c.width, c.height);
    ctx.font = SZ + 'px monospace';
    for (let i = 0; i < cols; i++) {
      const ch = chars[Math.floor(Math.random() * chars.length)];
      const b = Math.random();
      ctx.fillStyle = b > .97 ? 'rgba(255,255,255,0.9)' : `rgba(255,255,255,${0.06 + b * .12})`;
      ctx.fillText(ch, i * SZ, drops[i] * SZ);
      if (drops[i] * SZ > c.height && Math.random() > .975) drops[i] = 0;
      drops[i] += 0.35 + Math.random() * .25;
    }
  }
  window.addEventListener('resize', init); init();
  setInterval(draw, 55);
})();

// ── Плавающие C# символы (30 fps) ────────────────────────────────────────
(function () {
  const c = document.getElementById('stars-canvas');
  if (!c) return;
  const ctx = c.getContext('2d');
  const csharp = ['C#', 'var', 'int', 'if', '{}', '[]', '=>', 'new', 'void', 'class', '//', 'bool', 'string', 'return'];
  let particles = [], frame = 0;
  function init() {
    c.width = window.innerWidth; c.height = window.innerHeight; particles = [];
    for (let i = 0; i < 22; i++) particles.push({   // 22 вместо 30
      x: Math.random() * c.width, y: Math.random() * c.height,
      isChar: Math.random() < .4, char: csharp[Math.floor(Math.random() * csharp.length)],
      size: Math.random() * 1.2 + .4, fontSize: Math.floor(Math.random() * 4 + 7),
      vy: -(Math.random() * .18 + .04), vx: (Math.random() - .5) * .06,
      opacity: Math.random() * .2 + .04
    });
  }
  function draw() {
    frame++;
    requestAnimationFrame(draw);
    if (frame % 2 !== 0) return;   // 30 fps вместо 60
    ctx.clearRect(0, 0, c.width, c.height);
    particles.forEach(p => {
      ctx.globalAlpha = p.opacity; ctx.fillStyle = '#fff';
      if (p.isChar) { ctx.font = `${p.fontSize}px 'Courier New',monospace`; ctx.fillText(p.char, p.x, p.y); }
      else ctx.fillRect(p.x, p.y, p.size, p.size);
      p.x += p.vx; p.y += p.vy;
      if (p.y < -20) { p.y = c.height + 10; p.x = Math.random() * c.width; }
    });
    ctx.globalAlpha = 1;
  }
  window.addEventListener('resize', init); init(); draw();
})();

// ── Туман (20 fps) ────────────────────────────────────────────────────────
(function () {
  const c = document.getElementById('fog-canvas');
  if (!c) return;
  const ctx = c.getContext('2d'); let t = 0, frame = 0;
  function init() { c.width = window.innerWidth; c.height = window.innerHeight; }
  function draw() {
    frame++;
    requestAnimationFrame(draw);
    if (frame % 3 !== 0) return;   // ~20 fps вместо 60
    ctx.clearRect(0, 0, c.width, c.height);
    const x = c.width * .3 + Math.sin(t * .0006) * c.width * .15;
    const y = c.height * .4 + Math.cos(t * .0004) * c.height * .1;
    const g = ctx.createRadialGradient(x, y, 0, x, y, c.width * .55);
    g.addColorStop(0, 'rgba(255,255,255,0.05)');
    g.addColorStop(.5, 'rgba(255,255,255,0.015)');
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g; ctx.fillRect(0, 0, c.width, c.height); t += 3;
  }
  window.addEventListener('resize', init); init(); draw();
})();

// ── Cursor trail (минимальный, почти незаметный) ─────────────────────────
document.addEventListener('mousemove', function (e) {
  if (Math.random() > 0.02) return;   // 2% — очень редко
  const s = document.createElement('div');
  s.style.cssText = `position:fixed;pointer-events:none;z-index:99997;width:1px;height:1px;background:rgba(255,255,255,0.10);left:${e.clientX}px;top:${e.clientY}px;animation:px-fade-up .4s ease forwards`;
  document.body.appendChild(s);
  setTimeout(function () { s.remove(); }, 400);
});
