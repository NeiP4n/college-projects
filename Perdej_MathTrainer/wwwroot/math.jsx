// math.jsx — KaTeX renderer + SVG function plotter

// ── KaTeX render (synchronous) ────────────────────────────────────
function katexHTML(expr, display) {
  if (!window.katex || !expr) return { __html: String(expr) };
  try {
    return {
      __html: katex.renderToString(String(expr), {
        displayMode:  !!display,
        throwOnError: false,
        strict:       false,
        trust:        true,
        macros: {
          "\\vec":   "\\overrightarrow",
          "\\d":     "\\mathrm{d}",
          "\\R":     "\\mathbb{R}",
          "\\N":     "\\mathbb{N}",
          "\\Z":     "\\mathbb{Z}",
          "\\inf":   "\\infty",
        },
      })
    };
  } catch(e) {
    return { __html: String(expr) };
  }
}

function KatexInline({ expr }) {
  return React.createElement('span', {
    className: 'math-inline katex-wrap',
    dangerouslySetInnerHTML: katexHTML(expr, false),
  });
}

function KatexBlock({ expr }) {
  return React.createElement('div', {
    className: 'math-block katex-wrap',
    dangerouslySetInnerHTML: katexHTML(expr, true),
  });
}

// ── SVG Function Plotter ──────────────────────────────────────────
const COLORS = ['#a78bfa', '#34d399', '#fb923c', '#60a5fa', '#f472b6', '#fbbf24'];

function Graph({ fns = [], xMin = -6, xMax = 6, yMin = -4, yMax = 4,
                 labels = [], width = 340, height = 220 }) {
  const W = width, H = height, PAD = 36;
  const iW = W - PAD * 2, iH = H - PAD * 2;

  const sx = x => PAD + (x - xMin) / (xMax - xMin) * iW;
  const sy = y => H - PAD - (y - yMin) / (yMax - yMin) * iH;

  // Grid
  const xRange = xMax - xMin, yRange = yMax - yMin;
  const gridStep = Math.pow(10, Math.floor(Math.log10(xRange / 5)));
  const gridLines = [];

  for (let x = Math.ceil(xMin / gridStep) * gridStep; x <= xMax; x += gridStep) {
    const px = sx(x);
    gridLines.push(
      <line key={'gx'+x} x1={px} y1={PAD} x2={px} y2={H-PAD}
            stroke="rgba(255,255,255,.07)" strokeWidth="1" />
    );
  }
  for (let y = Math.ceil(yMin / gridStep) * gridStep; y <= yMax; y += gridStep) {
    const py = sy(y);
    gridLines.push(
      <line key={'gy'+y} x1={PAD} y1={py} x2={W-PAD} y2={py}
            stroke="rgba(255,255,255,.07)" strokeWidth="1" />
    );
  }

  // Axes
  const ax0 = sx(0), ay0 = sy(0);
  const xAxisY = Math.max(PAD, Math.min(H - PAD, ay0));
  const yAxisX = Math.max(PAD, Math.min(W - PAD, ax0));

  // Axis tick labels
  const axLabels = [];
  for (let x = Math.ceil(xMin / gridStep) * gridStep; x <= xMax; x += gridStep) {
    if (Math.abs(x) < gridStep * 0.01) continue;
    const label = Math.abs(x) >= 1000 ? x.toExponential(0) :
                  Number.isInteger(x) ? x : +x.toFixed(2);
    axLabels.push(
      <text key={'lx'+x} x={sx(x)} y={xAxisY + 14}
            textAnchor="middle" fontSize="10" fill="rgba(255,255,255,.45)">{label}</text>
    );
  }
  for (let y = Math.ceil(yMin / gridStep) * gridStep; y <= yMax; y += gridStep) {
    if (Math.abs(y) < gridStep * 0.01) continue;
    const label = Number.isInteger(y) ? y : +y.toFixed(2);
    axLabels.push(
      <text key={'ly'+y} x={yAxisX - 6} y={sy(y) + 4}
            textAnchor="end" fontSize="10" fill="rgba(255,255,255,.45)">{label}</text>
    );
  }

  // Function paths
  const paths = fns.map((fnStr, fi) => {
    let fn;
    try {
      fn = new Function('x', `
        const sin=Math.sin,cos=Math.cos,tan=Math.tan,asin=Math.asin,
              acos=Math.acos,atan=Math.atan,atan2=Math.atan2,
              sqrt=Math.sqrt,abs=Math.abs,log=Math.log,log2=Math.log2,
              exp=Math.exp,PI=Math.PI,E=Math.E,
              pow=Math.pow,sign=Math.sign,floor=Math.floor,ceil=Math.ceil,
              round=Math.round,hypot=Math.hypot,cbrt=Math.cbrt,
              sinh=Math.sinh,cosh=Math.cosh,tanh=Math.tanh;
        return (${fnStr});
      `);
    } catch(e) { return null; }

    const STEPS = 600;
    let d = '', prev = null;
    for (let i = 0; i <= STEPS; i++) {
      const x = xMin + (xMax - xMin) * i / STEPS;
      let y;
      try { y = fn(x); } catch(e) { prev = null; continue; }
      if (!isFinite(y) || isNaN(y) || Math.abs(y) > (yRange * 20)) {
        prev = null; continue;
      }
      const px = sx(x), py = sy(y);
      // Break line on steep discontinuities (e.g. tan)
      const jump = prev && Math.abs(py - prev.py) > iH * 2;
      if (!prev || jump) {
        d += `M${px.toFixed(1)},${py.toFixed(1)}`;
      } else {
        d += `L${px.toFixed(1)},${py.toFixed(1)}`;
      }
      prev = { px, py };
    }
    return (
      <path key={fi} d={d} fill="none"
            stroke={COLORS[fi % COLORS.length]} strokeWidth="2.2"
            strokeLinejoin="round" strokeLinecap="round" />
    );
  });

  // Legend
  const legend = labels.map((lbl, i) => (
    <g key={i} transform={`translate(${W - PAD - 100},${PAD + 4 + i * 18})`}>
      <line x1="0" y1="7" x2="16" y2="7" stroke={COLORS[i % COLORS.length]} strokeWidth="2.2"/>
      <text x="20" y="11" fontSize="11" fill="rgba(255,255,255,.75)">{lbl}</text>
    </g>
  ));

  return (
    <svg width={W} height={H} className="math-graph"
         style={{ display:'block', borderRadius:12,
                  background:'rgba(0,0,0,.28)', margin:'10px 0' }}>
      {gridLines}

      {/* X axis */}
      <line x1={PAD} y1={xAxisY} x2={W-PAD+8} y2={xAxisY}
            stroke="rgba(255,255,255,.4)" strokeWidth="1.5"/>
      <polygon points={`${W-PAD+8},${xAxisY} ${W-PAD},${xAxisY-4} ${W-PAD},${xAxisY+4}`}
               fill="rgba(255,255,255,.4)"/>
      <text x={W-PAD+12} y={xAxisY+4} fontSize="11" fill="rgba(255,255,255,.5)">x</text>

      {/* Y axis */}
      <line x1={yAxisX} y1={H-PAD} x2={yAxisX} y2={PAD-8}
            stroke="rgba(255,255,255,.4)" strokeWidth="1.5"/>
      <polygon points={`${yAxisX},${PAD-8} ${yAxisX-4},${PAD} ${yAxisX+4},${PAD}`}
               fill="rgba(255,255,255,.4)"/>
      <text x={yAxisX+6} y={PAD-2} fontSize="11" fill="rgba(255,255,255,.5)">y</text>

      {axLabels}
      {paths}
      {legend}
    </svg>
  );
}

Object.assign(window, { KatexInline, MathBlock: KatexBlock, Graph });
