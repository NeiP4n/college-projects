// data.jsx — Topics + question generator, ported faithfully from the C# MathTrainer codebase.
// Exposes window.TOPICS, window.generateQuestions(topicId, count), window.gradeFor(correct, total).

const rnd = (min, max) => Math.floor(Math.random() * (max - min)) + min; // [min, max)
const shuffle = (arr) => arr.map(v => [Math.random(), v]).sort((a, b) => a[0] - b[0]).map(x => x[1]);
const idxOf = (opts, val) => { const i = opts.indexOf(val); return i >= 0 ? i : 0; };
const idxOfDbl = (opts, val) => { for (let i = 0; i < opts.length; i++) if (Math.abs(opts[i] - val) < 0.001) return i; return 0; };

// ── Topics (from Topic.cs) ───────────────────────────────────────
const TOPICS = [
  { id: "counting",     name: "Счёт",                      level: "elementary", desc: "Сложение, вычитание, сравнение чисел" },
  { id: "arithmetic",   name: "Арифметические действия",   level: "elementary", desc: "+, −, ×, ÷, порядок действий" },
  { id: "powers",       name: "Степени и корни",           level: "elementary", desc: "Возведение в степень, извлечение корня" },
  { id: "fractions",    name: "Дроби",                     level: "elementary", desc: "Обыкновенные и десятичные дроби" },
  { id: "percentages",  name: "Проценты",                  level: "elementary", desc: "Нахождение процента, увеличение / уменьшение" },
  { id: "linear_eq",    name: "Линейные уравнения",        level: "middle",     desc: "Решение ax + b = c" },
  { id: "inequalities", name: "Неравенства",               level: "middle",     desc: "Линейные неравенства" },
  { id: "progression",  name: "Арифметическая прогрессия", level: "middle",     desc: "n-й член, сумма прогрессии" },
  { id: "areas",        name: "Площади фигур",             level: "middle",     desc: "Прямоугольник, треугольник, круг" },
];

// ── Generators (1:1 with QuestionGen.cs) ─────────────────────────
function counting(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    let a = rnd(10, 100), b = rnd(10, 100);
    if (i % 2 === 0) {
      const sum = a + b;
      qs.push({ text: `${a} + ${b} = ?`, options: shuffle([`${sum - 1}`, `${sum}`, `${sum + 1}`, `${sum + 2}`]),
        _val: `${sum}`, expl: `${a} + ${b} = ${sum}` });
    } else {
      if (a < b) [a, b] = [b, a];
      const diff = a - b;
      qs.push({ text: `${a} − ${b} = ?`, options: shuffle([`${diff}`, `${diff + 1}`, `${diff - 1}`, `${diff + 2}`]),
        _val: `${diff}`, expl: `${a} − ${b} = ${diff}` });
    }
  }
  return qs.map(finalize);
}

function arithmetic(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const a = rnd(2, 20), b = rnd(2, 15); let result;
    if (i % 3 === 0) {
      result = a * b;
      qs.push({ text: `${a} × ${b} = ?`, options: shuffle([`${result}`, `${result + 1}`, `${result - 1}`, `${result + b}`]),
        _val: `${result}`, expl: `${a} × ${b} = ${result}` });
    } else if (i % 3 === 1) {
      const add = rnd(1, 10); result = a * b + add;
      qs.push({ text: `${a} × ${b} + ${add} = ?`, options: shuffle([`${result + 1}`, `${result}`, `${result - 1}`, `${result + 2}`]),
        _val: `${result}`, expl: `${a} × ${b} + ${add} = ${result}` });
    } else {
      const c = a * b; result = c / a;
      qs.push({ text: `${c} ÷ ${a} = ?`, options: shuffle([`${result - 1}`, `${result + 1}`, `${result}`, `${result + 2}`]),
        _val: `${result}`, expl: `${c} ÷ ${a} = ${result}` });
    }
  }
  return qs.map(finalize);
}

function powers(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) {
      const b = rnd(2, 10), e = rnd(2, 4), r = Math.pow(b, e);
      qs.push({ text: `${b}^${e} = ?`, options: shuffle([`${r}`, `${r + 1}`, `${r - 1}`, `${b * e}`]),
        _val: `${r}`, expl: `${b}^${e} = ${r}` });
    } else {
      const s = rnd(2, 10), sq = s * s;
      qs.push({ text: `√${sq} = ?`, options: shuffle([`${s - 1}`, `${s + 1}`, `${s}`, `${s * 2}`]),
        _val: `${s}`, expl: `√${sq} = ${s}, т.к. ${s} × ${s} = ${sq}` });
    }
  }
  return qs.map(finalize);
}

function fractions(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const num = rnd(1, 8), den = rnd(2, 10);
    if (i % 2 === 0) {
      const val = Math.round((num / den) * 100) / 100;
      const shuf = shuffle([val, val + 0.01, val - 0.01, val + 0.02]);
      qs.push({ text: `Запишите ${num}/${den} в виде десятичной дроби`, options: shuf.map(x => x.toFixed(2)),
        _idx: idxOfDbl(shuf, val), expl: `${num}/${den} = ${val.toFixed(2)}` });
    } else {
      const n2 = rnd(1, 6), d2 = rnd(2, 6);
      const sum = Math.round((num / den + n2 / d2) * 100) / 100;
      const shuf = shuffle([sum, sum + 0.1, sum - 0.1, sum + 0.2]);
      qs.push({ text: `${num}/${den} + ${n2}/${d2} = ?  (десятичной дробью)`, options: shuf.map(x => x.toFixed(2)),
        _idx: idxOfDbl(shuf, sum), expl: `${num}/${den} + ${n2}/${d2} = ${sum.toFixed(2)}` });
    }
  }
  return qs.map(finalize);
}

function percentages(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const total = rnd(5, 30) * 10, pct = rnd(10, 51), result = Math.floor(total * pct / 100);
    qs.push({ text: `Найдите ${pct}% от ${total}`, options: shuffle([`${result}`, `${result + 1}`, `${result - 1}`, `${result * 2}`]),
      _val: `${result}`, expl: `${pct}% от ${total} = ${total} × ${pct} / 100 = ${result}` });
  }
  return qs.map(finalize);
}

function linearEq(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    let a, b, c, diff;
    do { a = rnd(1, 10); b = rnd(-15, 16); c = rnd(-10, 21); diff = c - b; } while (((diff % a) + a) % a !== 0);
    const x = diff / a, sign = b >= 0 ? "+" : "−";
    qs.push({ text: `${a}x ${sign} ${Math.abs(b)} = ${c}`, options: shuffle([`${x}`, `${x + 1}`, `${x - 1}`, `${-x}`]),
      _val: `${x}`, expl: `${a}x = ${c} − (${b}) = ${diff},  x = ${diff} / ${a} = ${x}` });
  }
  return qs.map(finalize);
}

function inequalities(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    let a = rnd(1, 5), b = rnd(-10, 11), c = rnd(-10, 11);
    const rhs = c - b;
    if (((rhs % a) + a) % a !== 0) a = 1;
    const x = (c - b) / a, sign = b >= 0 ? "+" : "−";
    qs.push({ text: `${a}x ${sign} ${Math.abs(b)} > ${c}`, options: shuffle([`x > ${x}`, `x < ${x}`, `x = ${x}`, `x ≥ ${x}`]),
      _val: `x > ${x}`, expl: `${a}x > ${rhs},  x > ${x}` });
  }
  return qs.map(finalize);
}

function progression(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const a1 = rnd(1, 10), d = rnd(1, 6), ni = rnd(4, 8);
    const an = a1 + (ni - 1) * d, sn = Math.floor(ni * (a1 + an) / 2);
    if (i % 2 === 0) {
      qs.push({ text: `Прогрессия: a₁ = ${a1}, d = ${d}. Найдите a${sub(ni)}`,
        options: shuffle([`${an}`, `${an + d}`, `${an - d}`, `${a1 + ni * d}`]),
        _val: `${an}`, expl: `a${sub(ni)} = ${a1} + (${ni} − 1) × ${d} = ${an}` });
    } else {
      qs.push({ text: `Прогрессия: a₁ = ${a1}, a${sub(ni)} = ${an}. Найдите сумму S${sub(ni)}`,
        options: shuffle([`${sn - 1}`, `${sn + 1}`, `${sn}`, `${sn * 2}`]),
        _val: `${sn}`, expl: `S${sub(ni)} = ${ni} × (${a1} + ${an}) / 2 = ${sn}` });
    }
  }
  return qs.map(finalize);
}

function areas(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const w = rnd(3, 12), h = rnd(3, 12);
      qs.push({ text: `Площадь прямоугольника ${w} × ${h}`, options: shuffle([`${w * h}`, `${w + h}`, `${(w + h) * 2}`, `${w * h + 1}`]),
        _val: `${w * h}`, expl: `S = ${w} × ${h} = ${w * h}` });
    } else if (i % 3 === 1) {
      const ba = rnd(4, 14), h = rnd(3, 12), s = ba * h / 2;
      qs.push({ text: `Площадь треугольника: основание ${ba}, высота ${h}`, options: shuffle([s.toFixed(1), `${ba * h}`, `${ba + h}`, (s * 2).toFixed(1)]),
        _val: s.toFixed(1), expl: `S = ½ × ${ba} × ${h} = ${s.toFixed(1)}` });
    } else {
      const r = rnd(2, 7), area = Math.round(Math.PI * r * r * 10) / 10;
      const opts = shuffle([area, Math.round(2 * Math.PI * r * 10) / 10, Math.round(Math.PI * r * 10) / 10, Math.round(area / 2 * 10) / 10]);
      qs.push({ text: `Площадь круга: радиус ${r}  (π ≈ 3.14)`, options: opts.map(x => x.toFixed(1)),
        _idx: idxOfDbl(opts, area), expl: `S = 3.14 × ${r}² = ${area.toFixed(1)}` });
    }
  }
  return qs.map(finalize);
}

// subscript digits for nicer "aₙ" rendering
function sub(n) { const m = { "0": "₀", "1": "₁", "2": "₂", "3": "₃", "4": "₄", "5": "₅", "6": "₆", "7": "₇", "8": "₈", "9": "₉" }; return String(n).split("").map(c => m[c] || c).join(""); }

function finalize(q) {
  if (q.correctIndex == null) q.correctIndex = q._idx != null ? q._idx : idxOf(q.options, q._val);
  delete q._val; delete q._idx;
  return q;
}

const GENERATORS = {
  counting, arithmetic, powers, fractions, percentages,
  linear_eq: linearEq, inequalities, progression, areas,
};

function generateQuestions(topicId, count = 5) {
  const gen = GENERATORS[topicId] || counting;
  return gen(count);
}

// Grade 2–5 from correct count out of total (school-style).
function gradeFor(correct, total) {
  const r = correct / total;
  if (r >= 0.9) return 5;
  if (r >= 0.7) return 4;
  if (r >= 0.5) return 3;
  return 2;
}

Object.assign(window, { TOPICS, generateQuestions, gradeFor });
