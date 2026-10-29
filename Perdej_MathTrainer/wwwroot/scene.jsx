// scene.jsx — Layered, animated Alto-style landscape with a continuous day↔night cycle.
// Imperatively driven via refs + rAF so the React tree never churns.
// Exposes window.Scene. Props: { timeMode, cycleSeconds, motion, snow }.

// ── color helpers ────────────────────────────────────────────────
const _h2r = (h) => { h = h.replace('#', ''); return [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)]; };
const _r2h = (a) => '#' + a.map(v => Math.max(0, Math.min(255, Math.round(v))).toString(16).padStart(2, '0')).join('');
const _lerp = (a, b, t) => a + (b - a) * t;
const _mix = (c1, c2, t) => { const a = _h2r(c1), b = _h2r(c2); return _r2h([_lerp(a[0], b[0], t), _lerp(a[1], b[1], t), _lerp(a[2], b[2], t)]); };

// keyframes around the cycle (p in [0,1)) — dawn → noon → sunset → night
const KF = [
  { p: 0.00, top: '#241a4d', mid: '#6b4a86', hor: '#f0a060', star: 0.15, far: '#4a3f7a', m2: '#3a3168', near: '#2a2452', front: '#1d1840' }, // dawn
  { p: 0.14, top: '#3f74ad', mid: '#86b6dd', hor: '#ffe0aa', star: 0.00, far: '#7d9fc4', m2: '#5f83ad', near: '#456891', front: '#324f73' }, // morning
  { p: 0.27, top: '#2f7fd0', mid: '#7cc0f0', hor: '#d6efff', star: 0.00, far: '#8fb6db', m2: '#6f9bc7', near: '#4f7aa8', front: '#3b5f88' }, // noon
  { p: 0.44, top: '#4a72a8', mid: '#9cc0dd', hor: '#ffd9a0', star: 0.00, far: '#86a6c8', m2: '#6a86ac', near: '#4e6890', front: '#3a4f72' }, // afternoon
  { p: 0.52, top: '#473a72', mid: '#bd5f80', hor: '#ff9d56', star: 0.06, far: '#7a5580', m2: '#5e4470', near: '#46355c', front: '#33274a' }, // sunset
  { p: 0.63, top: '#2c2659', mid: '#7a4d80', hor: '#e07a7e', star: 0.35, far: '#52406f', m2: '#3f315c', near: '#2e264a', front: '#221c3c' }, // dusk
  { p: 0.78, top: '#0d1330', mid: '#21285a', hor: '#3f3f74', star: 1.00, far: '#27305f', m2: '#1d244c', near: '#161b3c', front: '#10142f' }, // night
  { p: 0.92, top: '#141a3c', mid: '#2a2c62', hor: '#4a3f78', star: 0.70, far: '#312f63', m2: '#242350', near: '#1a193f', front: '#131534' }, // late
];
const CH = ['top', 'mid', 'hor', 'far', 'm2', 'near', 'front'];

function samplePalette(p) {
  p = ((p % 1) + 1) % 1;
  let i = 0;
  for (let k = 0; k < KF.length; k++) if (p >= KF[k].p) i = k;
  const a = KF[i];
  const b = KF[(i + 1) % KF.length];
  const span = (b.p > a.p ? b.p : b.p + 1) - a.p;
  const t = span === 0 ? 0 : (p - a.p) / span;
  const out = {};
  for (const c of CH) out[c] = _mix(a[c], b[c], t);
  out.star = _lerp(a.star, b.star, t);
  return out;
}

const FIXED = { dawn: 0.05, day: 0.27, dusk: 0.56, night: 0.78 };

// ── presentational layers ────────────────────────────────────────
function MountainLayer({ refEl, path, opacity }) {
  return (
    <svg ref={refEl} className="mtn" viewBox="0 0 1440 700" preserveAspectRatio="xMidYMax slice"
         style={{ opacity }} aria-hidden="true">
      <path d={path} />
    </svg>
  );
}

const PATHS = {
  far:   'M0,455 L150,372 L300,428 L470,338 L640,412 L820,346 L1010,420 L1210,350 L1440,408 L1440,700 L0,700 Z',
  mid:   'M0,520 L210,418 L420,500 L640,392 L880,498 L1120,404 L1330,478 L1440,452 L1440,700 L0,700 Z',
  near:  'M0,598 L250,468 L500,576 L780,442 L1060,556 L1300,462 L1440,508 L1440,700 L0,700 Z',
  front: 'M0,700 L0,486 C260,524 560,486 820,548 C1080,610 1280,602 1440,632 L1440,700 Z',
};

function Scene({ timeMode = 'auto', cycleSeconds = 90, motion = true, snow = false }) {
  const skyRef = React.useRef(null);
  const sunRef = React.useRef(null);
  const moonRef = React.useRef(null);
  const sunGlowRef = React.useRef(null);
  const starsRef = React.useRef(null);
  const hazeRef = React.useRef(null);
  const farRef = React.useRef(null), midRef = React.useRef(null), nearRef = React.useRef(null), frontRef = React.useRef(null);
  const riderRef = React.useRef(null);
  const rootRef = React.useRef(null);

  // stable random stars + snow
  const stars = React.useMemo(() =>
    Array.from({ length: 90 }, () => ({
      x: Math.random() * 100, y: Math.random() * 58,
      r: Math.random() * 1.4 + 0.4, d: Math.random() * 4, dur: 2.4 + Math.random() * 3.6,
    })), []);
  const flakes = React.useMemo(() =>
    Array.from({ length: 40 }, () => ({
      x: Math.random() * 100, s: Math.random() * 3 + 1.5,
      d: Math.random() * 12, dur: 9 + Math.random() * 11, drift: (Math.random() * 2 - 1) * 30,
    })), []);

  const startRef = React.useRef(performance.now());
  const parallax = React.useRef({ x: 0, y: 0, tx: 0, ty: 0 });

  React.useEffect(() => {
    let raf;
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const animate = motion && !reduce;

    const tick = (now) => {
      let p;
      if (timeMode === 'auto') {
        p = ((now - startRef.current) / 1000 / Math.max(8, cycleSeconds)) % 1;
      } else {
        p = FIXED[timeMode] ?? 0.27;
      }
      const pal = samplePalette(p);

      if (skyRef.current)
        skyRef.current.style.background =
          `linear-gradient(to bottom, ${pal.top} 0%, ${pal.mid} 46%, ${pal.hor} 100%)`;

      // celestial arcs
      const place = (el, glow, up, ph, warm) => {
        if (!el) return;
        const e = Math.sin(Math.max(0, Math.min(1, ph)) * Math.PI); // 0 at horizon, 1 at apex
        const x = 8 + Math.max(0, Math.min(1, ph)) * 84;
        const y = 80 - e * 66;
        const op = up ? Math.max(0, Math.min(1, e * 3.2)) : 0;
        el.style.left = x + '%';
        el.style.top = y + '%';
        el.style.opacity = op;
        if (warm) {
          const col = _mix('#ff7a3c', '#fff6e0', e);
          el.style.background = `radial-gradient(circle at 50% 50%, ${col} 0%, ${col} 58%, rgba(255,200,120,0) 72%)`;
          if (glow) { glow.style.left = x + '%'; glow.style.top = y + '%'; glow.style.opacity = op * 0.5; }
        }
      };
      // sun up first half, moon second half
      const sunPh = p < 0.5 ? p / 0.5 : -1;
      const moonPh = p >= 0.5 ? (p - 0.5) / 0.5 : -1;
      place(sunRef.current, sunGlowRef.current, p < 0.5, sunPh, true);
      place(moonRef.current, null, p >= 0.5, moonPh, false);

      if (starsRef.current) starsRef.current.style.opacity = pal.star;
      if (hazeRef.current) hazeRef.current.style.background =
        `radial-gradient(120% 70% at 50% 100%, ${pal.hor}aa 0%, ${pal.hor}00 60%)`;

      // mountain tints
      if (farRef.current) farRef.current.querySelector('path').setAttribute('fill', pal.far);
      if (midRef.current) midRef.current.querySelector('path').setAttribute('fill', pal.m2);
      if (nearRef.current) nearRef.current.querySelector('path').setAttribute('fill', pal.near);
      if (frontRef.current) frontRef.current.querySelector('path').setAttribute('fill', pal.front);

      // parallax easing
      if (animate) {
        const px = parallax.current;
        px.x += (px.tx - px.x) * 0.06;
        px.y += (px.ty - px.y) * 0.06;
        const set = (el, k) => { if (el) el.style.transform = `translate(${px.x * k}px, ${px.y * k * 0.4}px)`; };
        set(farRef.current, 4); set(midRef.current, 8); set(nearRef.current, 14); set(frontRef.current, 22);
      }

      raf = requestAnimationFrame(tick);
    };
    raf = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(raf);
  }, [timeMode, cycleSeconds, motion]);

  // mouse parallax
  React.useEffect(() => {
    if (!motion) return;
    const onMove = (e) => {
      const w = window.innerWidth, h = window.innerHeight;
      parallax.current.tx = (e.clientX / w - 0.5) * -1;
      parallax.current.ty = (e.clientY / h - 0.5) * -1;
    };
    window.addEventListener('pointermove', onMove);
    return () => window.removeEventListener('pointermove', onMove);
  }, [motion]);

  return (
    <div ref={rootRef} className="scene" aria-hidden="true">
      <div ref={skyRef} className="sky"></div>

      <div ref={starsRef} className="stars">
        {stars.map((s, i) => (
          <span key={i} style={{
            left: s.x + '%', top: s.y + '%', width: s.r * 2 + 'px', height: s.r * 2 + 'px',
            animationDelay: s.d + 's', animationDuration: s.dur + 's',
          }}></span>
        ))}
      </div>

      <div ref={sunGlowRef} className="sun-glow"></div>
      <div ref={sunRef} className="sun"></div>
      <div ref={moonRef} className="moon"><span className="crater c1"></span><span className="crater c2"></span><span className="crater c3"></span></div>

      <div className="clouds">
        <div className="cloud cloud-a"></div>
        <div className="cloud cloud-b"></div>
        <div className="cloud cloud-c"></div>
      </div>

      <div ref={hazeRef} className="haze"></div>

      <MountainLayer refEl={farRef} path={PATHS.far} opacity={0.9} />
      <MountainLayer refEl={midRef} path={PATHS.mid} opacity={0.95} />
      <MountainLayer refEl={nearRef} path={PATHS.near} opacity={1} />
      <MountainLayer refEl={frontRef} path={PATHS.front} opacity={1} />


      {snow && (
        <div className="snow">
          {flakes.map((f, i) => (
            <span key={i} style={{
              left: f.x + '%', width: f.s + 'px', height: f.s + 'px',
              animationDelay: f.d + 's', animationDuration: f.dur + 's',
              '--drift': f.drift + 'px',
            }}></span>
          ))}
        </div>
      )}
    </div>
  );
}

window.Scene = Scene;
