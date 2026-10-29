// math.jsx â€” KaTeX renderer + SVG function plotter

// â”€â”€ KaTeX helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function renderKatex(expr, display) {
  if (!window.katex || !expr) return '';
  try {
    return katex.renderToString(String(expr), {
      displayMode: !!display,
      throwOnError: false,
      strict: false
    });
  } catch (e) {
    return String(expr);
  }
}
function KatexInline({
  expr,
  display
}) {
  const ref = React.useRef(null);
  React.useEffect(() => {
    if (!ref.current || !window.katex) return;
    try {
      katex.render(String(expr), ref.current, {
        displayMode: !!display,
        throwOnError: false,
        strict: false
      });
    } catch (e) {
      ref.current.textContent = String(expr);
    }
  }, [expr, display]);
  return React.createElement('span', {
    ref,
    className: display ? 'math-block' : 'math-inline'
  });
}
function KatexBlock({
  expr
}) {
  return React.createElement(KatexInline, {
    expr,
    display: true
  });
}

// â”€â”€ SVG Function Plotter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const COLORS = ['#a78bfa', '#34d399', '#fb923c', '#60a5fa', '#f472b6'];
function Graph({
  fns = [],
  xMin = -6,
  xMax = 6,
  yMin = -4,
  yMax = 4,
  labels = [],
  width = 340,
  height = 220
}) {
  const W = width,
    H = height,
    PAD = 32;
  const iW = W - PAD * 2,
    iH = H - PAD * 2;
  const sx = x => PAD + (x - xMin) / (xMax - xMin) * iW;
  const sy = y => H - PAD - (y - yMin) / (yMax - yMin) * iH;

  // Grid
  const gridLines = [];
  const step = Math.ceil((xMax - xMin) / 10);
  for (let x = Math.ceil(xMin); x <= xMax; x += step) {
    gridLines.push(/*#__PURE__*/React.createElement("line", {
      key: 'gx' + x,
      x1: sx(x),
      y1: PAD,
      x2: sx(x),
      y2: H - PAD,
      stroke: "rgba(255,255,255,.08)",
      strokeWidth: "1"
    }));
  }
  for (let y = Math.ceil(yMin); y <= yMax; y += step) {
    gridLines.push(/*#__PURE__*/React.createElement("line", {
      key: 'gy' + y,
      x1: PAD,
      y1: sy(y),
      x2: W - PAD,
      y2: sy(y),
      stroke: "rgba(255,255,255,.08)",
      strokeWidth: "1"
    }));
  }

  // Axes
  const ax0 = sx(0),
    ay0 = sy(0);
  const axInRange = ax0 >= PAD && ax0 <= W - PAD;
  const ayInRange = ay0 >= PAD && ay0 <= H - PAD;

  // Axis labels
  const axLabels = [];
  for (let x = Math.ceil(xMin); x <= xMax; x += step) {
    if (x === 0) continue;
    axLabels.push(/*#__PURE__*/React.createElement("text", {
      key: 'lx' + x,
      x: sx(x),
      y: ayInRange ? ay0 + 14 : H - PAD + 14,
      textAnchor: "middle",
      fontSize: "10",
      fill: "rgba(255,255,255,.4)"
    }, x));
  }
  for (let y = Math.ceil(yMin); y <= yMax; y += step) {
    if (y === 0) continue;
    axLabels.push(/*#__PURE__*/React.createElement("text", {
      key: 'ly' + y,
      x: axInRange ? ax0 - 10 : PAD - 10,
      y: sy(y) + 4,
      textAnchor: "end",
      fontSize: "10",
      fill: "rgba(255,255,255,.4)"
    }, y));
  }

  // Function paths
  const paths = fns.map((fnStr, fi) => {
    let fn;
    try {
      fn = new Function('x', `
        const sin=Math.sin,cos=Math.cos,tan=Math.tan,
              sqrt=Math.sqrt,abs=Math.abs,log=Math.log,
              exp=Math.exp,PI=Math.PI,E=Math.E,
              pow=Math.pow,sign=Math.sign,floor=Math.floor,ceil=Math.ceil;
        return ${fnStr};
      `);
    } catch (e) {
      return null;
    }
    const STEPS = 400;
    let d = '',
      prev = null;
    for (let i = 0; i <= STEPS; i++) {
      const x = xMin + (xMax - xMin) * i / STEPS;
      let y;
      try {
        y = fn(x);
      } catch (e) {
        prev = null;
        continue;
      }
      if (!isFinite(y) || isNaN(y)) {
        prev = null;
        continue;
      }
      const px = sx(x),
        py = sy(y);
      if (prev && Math.abs(py - prev.py) < iH * 3) {
        d += `L${px.toFixed(1)},${py.toFixed(1)}`;
      } else {
        d += `M${px.toFixed(1)},${py.toFixed(1)}`;
      }
      prev = {
        px,
        py
      };
    }
    return /*#__PURE__*/React.createElement("path", {
      key: fi,
      d: d,
      fill: "none",
      stroke: COLORS[fi % COLORS.length],
      strokeWidth: "2",
      strokeLinejoin: "round",
      strokeLinecap: "round"
    });
  });

  // Legend
  const legend = labels.map((lbl, i) => /*#__PURE__*/React.createElement("g", {
    key: i,
    transform: `translate(${W - PAD - 120},${PAD + i * 18})`
  }, /*#__PURE__*/React.createElement("line", {
    x1: "0",
    y1: "7",
    x2: "18",
    y2: "7",
    stroke: COLORS[i % COLORS.length],
    strokeWidth: "2"
  }), /*#__PURE__*/React.createElement("text", {
    x: "22",
    y: "11",
    fontSize: "11",
    fill: "rgba(255,255,255,.7)"
  }, lbl)));
  return /*#__PURE__*/React.createElement("svg", {
    width: W,
    height: H,
    className: "math-graph",
    style: {
      display: 'block',
      borderRadius: 10,
      background: 'rgba(0,0,0,.25)',
      margin: '12px 0'
    }
  }, gridLines, axInRange && /*#__PURE__*/React.createElement("line", {
    x1: ax0,
    y1: PAD,
    x2: ax0,
    y2: H - PAD,
    stroke: "rgba(255,255,255,.35)",
    strokeWidth: "1.5"
  }), ayInRange && /*#__PURE__*/React.createElement("line", {
    x1: PAD,
    y1: ay0,
    x2: W - PAD,
    y2: ay0,
    stroke: "rgba(255,255,255,.35)",
    strokeWidth: "1.5"
  }), axInRange && /*#__PURE__*/React.createElement("polygon", {
    points: `${ax0},${PAD - 6} ${ax0 - 4},${PAD + 4} ${ax0 + 4},${PAD + 4}`,
    fill: "rgba(255,255,255,.35)"
  }), ayInRange && /*#__PURE__*/React.createElement("polygon", {
    points: `${W - PAD + 6},${ay0} ${W - PAD - 4},${ay0 - 4} ${W - PAD - 4},${ay0 + 4}`,
    fill: "rgba(255,255,255,.35)"
  }), axLabels, paths, legend);
}
Object.assign(window, {
  KatexInline,
  MathBlock: KatexBlock,
  Graph
});
