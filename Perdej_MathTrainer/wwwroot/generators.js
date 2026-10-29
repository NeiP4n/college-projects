// generators.jsx — генераторы вопросов для всех тем

const _r = (a, b) => Math.floor(Math.random() * (b - a)) + a;
const _sh = arr => arr.map(v => [Math.random(), v]).sort((a, b) => a[0] - b[0]).map(x => x[1]);

// Normalize number to string: 40.0→"40", 40.50→"40.5", 3.14→"3.14"
const _fmt = v => {
  if (typeof v === 'number') {
    if (Number.isInteger(v)) return String(v);
    const s = parseFloat(v.toPrecision(6)).toString();
    return s;
  }
  return String(v);
};

// Find correct index: tries exact string match first, then numeric proximity
const _fi = (opts, val) => {
  const sv = _fmt(val);
  let i = opts.indexOf(sv);
  if (i >= 0) return i;
  const nv = parseFloat(sv);
  if (!isNaN(nv)) {
    i = opts.findIndex(o => Math.abs(parseFloat(o) - nv) < 0.01);
    if (i >= 0) return i;
  }
  return 0;
};
const _fd = (opts, val) => {
  for (let i = 0; i < opts.length; i++) if (Math.abs(opts[i] - val) < 0.001) return i;
  return 0;
};

// _q: строит вопрос, все опции приводятся через _fmt
const _q = (text, opts, val, expl) => {
  const norm = opts.map(o => _fmt(o));
  const s = _sh(norm);
  return {
    text,
    options: s,
    correctIndex: _fi(s, val),
    expl
  };
};

// _qd: для дробных ответов с фиксированными знаками (устаревший, оставлен для совместимости)
const _qd = (text, opts, val, expl) => {
  const s = _sh(opts);
  return {
    text,
    options: s.map(x => x.toFixed ? x.toFixed(2) : String(x)),
    correctIndex: _fd(s, val),
    expl
  };
};

// ── Уровень I ─────────────────────────────────────────────────────

function gen_num_theory(n) {
  const qs = [];
  const modes = ['gcd', 'lcm', 'prime', 'even'];
  for (let i = 0; i < n; i++) {
    const m = modes[i % 4];
    if (m === 'gcd') {
      const g = _r(2, 8),
        a = g * _r(2, 7),
        b = g * _r(2, 7);
      function gcd(x, y) {
        while (y) {
          [x, y] = [y, x % y];
        }
        return x;
      }
      const ans = gcd(a, b);
      qs.push(_q(`НОД(${a}, ${b}) = ?`, [String(ans), String(ans + g), String(g), String(a)], ans, `Алгоритм Евклида: НОД(${a},${b})=${ans}`));
    } else if (m === 'lcm') {
      const a = _r(2, 10),
        b = _r(2, 10);
      function gcd2(x, y) {
        while (y) {
          [x, y] = [y, x % y];
        }
        return x;
      }
      const ans = a * b / gcd2(a, b);
      qs.push(_q(`НОК(${a}, ${b}) = ?`, [String(ans), String(a * b), String(ans + 2), String(ans - 2)], ans, `НОК = ${a}×${b}/НОД(${a},${b}) = ${ans}`));
    } else if (m === 'prime') {
      const primes = [2, 3, 5, 7, 11, 13, 17, 19, 23];
      const notPrimes = [4, 6, 8, 9, 10, 12, 14, 15, 16];
      const isPrime = Math.random() > 0.5;
      const num = isPrime ? primes[_r(0, primes.length)] : notPrimes[_r(0, notPrimes.length)];
      const ans = isPrime ? 'Да' : 'Нет';
      qs.push(_q(`${num} — простое число?`, ['Да', 'Нет', 'Зависит от условия', 'Не определено'], ans, isPrime ? `${num} делится только на 1 и ${num}` : `${num} имеет делители кроме 1 и себя`));
    } else {
      const a = _r(10, 99),
        b = _r(10, 99),
        sum = a + b;
      qs.push(_q(`${a} + ${b}. Чётна ли сумма?`, [sum % 2 === 0 ? 'Да' : 'Нет', sum % 2 === 0 ? 'Нет' : 'Да', 'Всегда чётна', 'Нельзя определить'], sum % 2 === 0 ? 'Да' : 'Нет', `${a}+${b}=${sum}, ${sum % 2 === 0 ? 'чётное' : 'нечётное'}`));
    }
  }
  return qs;
}
function gen_arith_struct(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const a = _r(2, 9),
        b = _r(2, 9);
      qs.push(_q(`Переместительный закон: ${a}×${b} = ?×${a}`, [String(b), String(a), String(a + b), String(a * b)], b, `a×b=b×a — переместительный закон умножения`));
    } else if (i % 3 === 1) {
      const a = _r(2, 5),
        b = _r(2, 5),
        c = _r(2, 5);
      const l = a * (b + c),
        r = a * b + a * c;
      qs.push(_q(`${a}×(${b}+${c}) = ?`, [String(l), String(l + 1), String(l - 1), String(a * b)], l, `Распределительный закон: ${a}×(${b}+${c})=${a}×${b}+${a}×${c}=${r}`));
    } else {
      const bases = [2, 8, 16];
      const base = bases[_r(0, 3)];
      const num = _r(3, 15);
      if (base === 2) {
        const ans = num.toString(2);
        qs.push(_q(`${num} в двоичной системе?`, [ans, String(num + 1), String(num - 1), (num + 2).toString(2)], ans, `${num} = ${ans}₂`));
      } else if (base === 8) {
        const ans = num.toString(8);
        qs.push(_q(`${num} в восьмеричной системе?`, [ans, (num + 1).toString(8), (num + 8).toString(8), (num - 1).toString(8)], ans, `${num} = ${ans}₈`));
      } else {
        const ans = num.toString(16).toUpperCase();
        qs.push(_q(`${num} в шестнадцатеричной?`, [ans, (num + 1).toString(16).toUpperCase(), (num + 16).toString(16).toUpperCase(), (num - 1).toString(16).toUpperCase()], ans, `${num} = ${ans}₁₆`));
      }
    }
  }
  return qs;
}
function gen_rational(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const a = _r(1, 6),
        b = _r(2, 8),
        c = _r(1, 6),
        d = _r(2, 8);
      const num = a * d + c * b,
        den = b * d;
      function gcd(x, y) {
        while (y) {
          [x, y] = [y, x % y];
        }
        return x;
      }
      const g = gcd(Math.abs(num), den);
      const rn = num / g,
        rd = den / g;
      const ans = `${rn}/${rd}`;
      qs.push(_q(`${a}/${b} + ${c}/${d} = ?`, [ans, `${a + c}/${b + d}`, `${a * c}/${b * d}`, `${rn + 1}/${rd}`], ans, `Общий знаменатель ${b * d}: (${a * d}+${c * b})/${b * d}=${num}/${den}=${ans}`));
    } else if (i % 3 === 1) {
      const a = _r(1, 9),
        b = _r(2, 9),
        c = _r(1, 9),
        d = _r(2, 9);
      const cmp = a / b > c / d;
      qs.push(_q(`${a}/${b} □ ${c}/${d} (>, <, =)?`, [cmp ? '>' : '<', cmp ? '<' : '>', '=', 'Нельзя сравнить'], cmp ? '>' : '<', `${a}/${b}=${(a / b).toFixed(3)}, ${c}/${d}=${(c / d).toFixed(3)}`));
    } else {
      const a = _r(1, 8),
        b = _r(2, 7),
        c = _r(1, 6),
        d = _r(2, 7);
      const num = a * d,
        den = b * c;
      function gcd3(x, y) {
        while (y) {
          [x, y] = [y, x % y];
        }
        return x;
      }
      const g = gcd3(num, den);
      const rn = num / g,
        rd = den / g;
      const ans = rd === 1 ? String(rn) : `${rn}/${rd}`;
      qs.push(_q(`${a}/${b} ÷ ${c}/${d} = ?`, [ans, `${a * c}/${b * d}`, `${a}/${b * c}`, `${rn}/${rd + 1}`], ans, `Деление дробей: ${a}/${b} × ${d}/${c} = ${num}/${den} = ${ans}`));
    }
  }
  return qs;
}
function gen_power_trans(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 4 === 0) {
      const b = _r(2, 6),
        e1 = _r(1, 4),
        e2 = _r(1, 4),
        ans = e1 + e2;
      qs.push(_q(`${b}^${e1} × ${b}^${e2} = ${b}^?`, [String(ans), String(e1 * e2), String(ans + 1), String(ans - 1)], ans, `При умножении степеней с одним основанием показатели складываются: ${e1}+${e2}=${ans}`));
    } else if (i % 4 === 1) {
      const b = _r(2, 5),
        e1 = _r(3, 6),
        e2 = _r(1, 3),
        ans = e1 - e2;
      qs.push(_q(`${b}^${e1} ÷ ${b}^${e2} = ${b}^?`, [String(ans), String(e1 + e2), String(e1 * e2), String(ans + 1)], ans, `При делении степеней показатели вычитаются: ${e1}−${e2}=${ans}`));
    } else if (i % 4 === 2) {
      const b = _r(2, 5),
        e = _r(2, 4),
        ans = Math.pow(b, e);
      qs.push(_q(`(${b}^2)^${e / 2 | 0} ≠ но ${b}^${e} = ?`, [String(ans), String(b * e), String(ans + 1), String(b + e)], ans, `${b}^${e} = ${ans}`));
    } else {
      const bases = [4, 8, 9, 16, 25, 27];
      const b = bases[_r(0, bases.length)];
      const sr = Math.round(Math.sqrt(b));
      const ans = sr * sr === b ? sr : Math.pow(b, 1 / 3) | 0;
      qs.push(_q(`√${b} = ?`, [String(sr), String(sr + 1), String(sr - 1), String(sr * 2)], sr, `√${b} = ${sr}, т.к. ${sr}² = ${sr * sr}`));
    }
  }
  return qs;
}
function gen_percent_models(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const P = _r(1000, 5000),
        r = _r(5, 20),
        t = _r(1, 4);
      const ans = Math.round(P * Math.pow(1 + r / 100, t));
      qs.push(_q(`Вклад ${P}₽ под ${r}% годовых на ${t} лет (сложный %): итог?`, [String(ans), String(P + P * r * t / 100), String(ans + P), String(ans - P * r / 100 | 0)], ans, `${P}×(1+${r}/100)^${t} = ${ans}₽`));
    } else if (i % 3 === 1) {
      const a = _r(20, 200),
        b = _r(20, 200);
      const ans = Math.round(a * 100 / b);
      qs.push(_q(`${a} — это сколько % от ${b}?`, [String(ans) + '%', String(ans + 10) + '%', String(ans - 5) + '%', String(ans * 2) + '%'], String(ans) + '%', `${a}/${b}×100 = ${ans}%`));
    } else {
      const P = _r(100, 500),
        r1 = _r(10, 30),
        r2 = _r(5, 20);
      const ans = Math.round(P * (1 + r1 / 100) * (1 - r2 / 100));
      qs.push(_q(`Цена ${P}₽, +${r1}%, потом −${r2}%. Итог?`, [String(ans), String(P), String(Math.round(P * (1 + (r1 - r2) / 100))), String(ans + 10)], ans, `${P}×1.${r1 < 10 ? '0' + r1 : r1}×0.${100 - r2} = ${ans}₽`));
    }
  }
  return qs;
}
function gen_num_patterns(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const fibs = [1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89];
      const start = _r(0, 5);
      const ans = fibs[start + 4];
      qs.push(_q(`Числа Фибоначчи: ${fibs[start]}, ${fibs[start + 1]}, ${fibs[start + 2]}, ${fibs[start + 3]}, ?`, [String(ans), String(ans + 1), String(ans - 1), String(fibs[start + 3] * 2)], ans, `Каждый член = сумма двух предыдущих: ${fibs[start + 3]}+${fibs[start + 2]}=${ans}`));
    } else if (i % 3 === 1) {
      const d = _r(2, 7),
        a = _r(1, 10),
        arr = [a, a + d, a + 2 * d, a + 3 * d];
      const ans = a + 4 * d;
      qs.push(_q(`${arr.join(', ')}, ?`, [String(ans), String(ans + 1), String(ans - d), String(ans + d)], ans, `Разность d=${d}, следующий: ${a + 3 * d}+${d}=${ans}`));
    } else {
      const q = _r(2, 4),
        a = _r(1, 5),
        arr = [a, a * q, a * q * q, a * q * q * q];
      const ans = a * q * q * q * q;
      qs.push(_q(`Геом. прогрессия: ${arr.join(', ')}, ?`, [String(ans), String(ans + q), String(arr[3] + q), String(ans * q)], ans, `Знаменатель q=${q}: ${arr[3]}×${q}=${ans}`));
    }
  }
  return qs;
}

// ── Уровень II ────────────────────────────────────────────────────

function gen_lin_eq2(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    let a, b, c, diff, x;
    do {
      a = _r(1, 8);
      b = _r(-15, 15);
      c = _r(-10, 20);
      diff = c - b;
    } while (diff % a !== 0);
    x = diff / a;
    const sign = b >= 0 ? '+' : '−';
    qs.push(_q(`${a}x ${sign} ${Math.abs(b)} = ${c}`, [String(x), String(x + 1), String(x - 1), String(-x)], x, `${a}x = ${c}−(${b}) = ${diff}, x = ${diff}/${a} = ${x}`));
  }
  return qs;
}
function gen_lin_ineq2(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    let a = _r(1, 5),
      b = _r(-10, 10),
      c = _r(-10, 10),
      rhs = c - b;
    if (rhs % a !== 0) a = 1;
    const x = rhs / a,
      sign = b >= 0 ? '+' : '−';
    const gt = Math.random() > 0.5;
    const op = gt ? '>' : '<';
    const correctOp = gt ? `x > ${x}` : `x < ${x}`;
    qs.push(_q(`${a}x ${sign} ${Math.abs(b)} ${op} ${c}`, [correctOp, gt ? `x < ${x}` : `x > ${x}`, `x = ${x}`, `x ≥ ${x}`], correctOp, `${a}x ${op} ${c}−(${b}) = ${rhs}, x ${op} ${x}`));
  }
  return qs;
}
function gen_seq_prog(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const isGP = i % 2 === 1;
    if (!isGP) {
      const a1 = _r(1, 10),
        d = _r(1, 6),
        ni = _r(4, 9);
      const an = a1 + (ni - 1) * d;
      if (i % 4 === 0) {
        qs.push(_q(`АП: a₁=${a1}, d=${d}. Найдите a${ni}`, [String(an), String(an + d), String(an - d), String(a1 * ni)], an, `a${ni}=${a1}+(${ni}-1)×${d}=${an}`));
      } else {
        const sn = ni * (a1 + an) / 2;
        qs.push(_q(`АП: a₁=${a1}, a${ni}=${an}. Сумма S${ni}=?`, [String(sn), String(sn + ni), String(sn - ni), String(sn * 2)], sn, `S${ni}=${ni}×(${a1}+${an})/2=${sn}`));
      }
    } else {
      const b1 = _r(1, 4),
        q = _r(2, 4),
        ni = _r(3, 6);
      const bn = b1 * Math.pow(q, ni - 1);
      qs.push(_q(`ГП: b₁=${b1}, q=${q}. Найдите b${ni}`, [String(bn), String(bn * q), String(bn / q | 0), String(b1 * ni)], bn, `b${ni}=${b1}×${q}^${ni - 1}=${bn}`));
    }
  }
  return qs;
}
function gen_polynomials(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const a = _r(1, 5),
        b = _r(1, 8),
        x = _r(1, 4);
      const val = a * x * x + b * x;
      qs.push(_q(`P(x)=${a}x²+${b}x при x=${x}: P(${x})=?`, [String(val), String(a * x + b), String(val + b), String(val - a)], val, `${a}×${x}²+${b}×${x}=${a * x * x}+${b * x}=${val}`));
    } else if (i % 3 === 1) {
      const a = _r(2, 6),
        b = _r(2, 6);
      qs.push(_q(`Степень произведения (x²+1)×(x³+x)?`, '5 4 3 6'.split(' '), '5', `Степени складываются: 2+3=5`));
    } else {
      const a = _r(1, 5),
        b = _r(1, 8),
        c = _r(1, 6),
        d = _r(1, 8);
      const A = a + c,
        B = b + d;
      qs.push(_q(`(${a}x+${b})+(${c}x+${d}) = ?`, [`${A}x+${B}`, `${a + c}x+${b * d}`, `${A + 1}x+${B}`, `${A}x+${B + 1}`], `${A}x+${B}`, `Складываем подобные: (${a}+${c})x+(${b}+${d})=${A}x+${B}`));
    }
  }
  return qs;
}
function gen_alg_trans(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const a = _r(2, 8),
        b = _r(1, 6);
      const ans = a * a + 2 * a * b + b * b;
      qs.push(_q(`(${a}+${b})² = ?`, [`${a}²+2×${a}×${b}+${b}²`, `${a}²+${b}²`, `${ans}`, `2×${a}+2×${b}`], `${a}²+2×${a}×${b}+${b}²`, `Формула: (a+b)²=a²+2ab+b²`));
    } else if (i % 3 === 1) {
      const a = _r(3, 9),
        b = _r(1, 5);
      qs.push(_q(`(${a}+${b})(${a}-${b}) = ?`, [`${a}²-${b}²`, `${a}²-2${a}${b}+${b}²`, `${a}²+${b}²`, `(${a}-${b})²`], `${a}²-${b}²`, `Разность квадратов: (a+b)(a-b)=a²-b²`));
    } else {
      const a = _r(2, 6);
      qs.push(_q(`(x+${a})(x-${a}) = ?`, [`x²-${a * a}`, `x²+${a * a}`, `x²-${a}`, `x²+${a}`], `x²-${a * a}`, `Разность квадратов: x²-${a}²=x²-${a * a}`));
    }
  }
  return qs;
}
function gen_factorization(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const a = _r(2, 6),
        b = _r(1, 8);
      qs.push(_q(`Разложите: x²+${2 * b}x+${b * b} = ?`, [`(x+${b})²`, `(x-${b})²`, `(x+${b})(x-${b})`, `x(x+${2 * b})`], `(x+${b})²`, `Полный квадрат: x²+2×${b}×x+${b}²=(x+${b})²`));
    } else if (i % 3 === 1) {
      const a = _r(2, 7);
      qs.push(_q(`Разложите: x²-${a * a} = ?`, [`(x+${a})(x-${a})`, `(x-${a})²`, `(x+${a})²`, `x(x-${a})`], `(x+${a})(x-${a})`, `Разность квадратов: x²-${a}²=(x+${a})(x-${a})`));
    } else {
      const k = _r(2, 6),
        a = _r(1, 5),
        b = _r(1, 5);
      qs.push(_q(`Вынесите общий множитель: ${k * a}x+${k * b} = ?`, [`${k}(${a}x+${b})`, `${a}(${k}x+${b})`, `${k * a}(x+${b})`, `${k}x(${a}+${b})`], `${k}(${a}x+${b})`, `НОД(${k * a},${k * b})=${k}: ${k}(${a}x+${b})`));
    }
  }
  return qs;
}

// ── Уровень III ───────────────────────────────────────────────────

function gen_planimetry(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 4 === 0) {
      const a = _r(40, 80),
        b = _r(20, 50);
      const c = 180 - a - b;
      qs.push(_q(`Углы треугольника: ${a}° и ${b}°. Третий угол?`, [String(c) + '°', String(c + 10) + '°', String(c - 10) + '°', String(180 - a) + '°'], String(c) + '°', `Сумма углов треугольника = 180°: 180-${a}-${b}=${c}°`));
    } else if (i % 4 === 1) {
      const a = _r(3, 12),
        b = _r(3, 12);
      const c2 = a * a + b * b;
      qs.push(_q(`Катеты прямоугольного треугольника: ${a} и ${b}. Гипотенуза²=?`, [String(c2), String(c2 + a), String(c2 - b), String(a * b * 2)], c2, `Теорема Пифагора: c²=${a}²+${b}²=${a * a}+${b * b}=${c2}`));
    } else if (i % 4 === 2) {
      const n4 = _r(3, 8);
      const sum = (n4 - 2) * 180;
      qs.push(_q(`Сумма внутренних углов ${n4}-угольника?`, [String(sum) + '°', String(n4 * 90) + '°', String((n4 - 1) * 180) + '°', String(sum - 180) + '°'], String(sum) + '°', `(n-2)×180°=(${n4}-2)×180=${sum}°`));
    } else {
      const r = _r(3, 10);
      const circ = Math.round(2 * Math.PI * r * 10) / 10;
      qs.push(_q(`Длина окружности радиуса ${r} (π≈3.14)?`, [circ.toFixed(1), String(Math.PI * r * r | 0), (2 * r).toString(), String(circ + r)], circ.toFixed(1), `C=2πr=2×3.14×${r}=${circ.toFixed(1)}`));
    }
  }
  return qs;
}
function gen_geo_areas(n) {
  return gen_areas ? gen_areas(n) : gen_planimetry(n);
}
function gen_similarity(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const k = _r(2, 5);
    if (i % 3 === 0) {
      const a = _r(3, 10),
        b = a * k;
      qs.push(_q(`Стороны подобных треугольников: ${a} и ${b}. Коэффициент подобия?`, [String(k), String(k + 1), String(k - 1), String(a + b)], k, `k=${b}/${a}=${k}`));
    } else if (i % 3 === 1) {
      const s1 = _r(4, 12);
      const s2 = s1 * k;
      const area1 = s1 * s1;
      const area2 = s2 * s2;
      qs.push(_q(`Стороны подобных фигур ${s1} и ${s2}. Отношение площадей?`, [`${k}²=${k * k}:1`, `${k}:1`, `1:${k}`, `${k * k * k}:1`], `${k}²=${k * k}:1`, `Площади подобных фигур относятся как k²=${k}²=${k * k}`));
    } else {
      const a = _r(4, 12),
        b = _r(3, 10),
        c = a * k;
      const d = Math.round(b * k);
      qs.push(_q(`Подобные треугольники: стороны ${a},${b} и ${c},x. Найдите x`, [String(d), String(d + 1), String(b + k), String(b * k + 1)], d, `${c}/${a}=${k}, x=${b}×${k}=${d}`));
    }
  }
  return qs;
}
function gen_circles_arcs(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const r = _r(2, 8),
        deg = _r(30, 270);
      const arc = Math.round(2 * Math.PI * r * deg / 360 * 10) / 10;
      qs.push(_q(`Длина дуги ${deg}° окружности r=${r} (π≈3.14)?`, [arc.toFixed(1), (Math.PI * r * r * deg / 360).toFixed(1), (2 * Math.PI * r).toFixed(1), (arc * 2).toFixed(1)], arc.toFixed(1), `l=2πr×α/360=2×3.14×${r}×${deg}/360=${arc.toFixed(1)}`));
    } else if (i % 3 === 1) {
      const r = _r(3, 8),
        deg = [60, 90, 120, 180][_r(0, 4)];
      const area = Math.round(Math.PI * r * r * deg / 360 * 10) / 10;
      qs.push(_q(`Площадь сектора r=${r}, угол ${deg}°?`, [area.toFixed(1), (Math.PI * r * r).toFixed(1), (area * 2).toFixed(1), (Math.PI * r).toFixed(1)], area.toFixed(1), `S=πr²×${deg}/360=${area.toFixed(1)}`));
    } else {
      const d = _r(6, 20);
      const r = d / 2;
      qs.push(_q(`Диаметр окружности ${d}. Радиус = ?`, [String(r), String(d * 2), String(d + 2), String(r + 1)], r, `r=d/2=${d}/2=${r}`));
    }
  }
  return qs;
}
function gen_coord_geo(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const x1 = _r(-5, 5),
        y1 = _r(-5, 5),
        x2 = _r(-5, 5),
        y2 = _r(-5, 5);
      const d2 = (x2 - x1) ** 2 + (y2 - y1) ** 2;
      const d = Math.round(Math.sqrt(d2) * 100) / 100;
      qs.push(_q(`Расстояние между (${x1};${y1}) и (${x2};${y2})²?`, [String(d2), String(d2 + 4), String((Math.abs(x2 - x1) + Math.abs(y2 - y1)) ** 2), String(d2 - 2)], d2, `d²=(${x2}-${x1})²+(${y2}-${y1})²=${(x2 - x1) ** 2}+${(y2 - y1) ** 2}=${d2}`));
    } else if (i % 3 === 1) {
      const x1 = _r(-4, 4),
        y1 = _r(-4, 4),
        x2 = _r(-4, 4),
        y2 = _r(-4, 4);
      const mx = (x1 + x2) / 2,
        my = (y1 + y2) / 2;
      qs.push(_q(`Середина отрезка (${x1};${y1})—(${x2};${y2})?`, [`(${mx};${my})`, `(${x2 - x1};${y2 - y1})`, `(${mx + 1};${my})`, `(${x1};${y2})`], `(${mx};${my})`, `Середина: ((${x1}+${x2})/2; (${y1}+${y2})/2)=(${mx};${my})`));
    } else {
      const k = _r(-3, 4) || 1,
        b = _r(-5, 5);
      const x = _r(1, 4);
      const y = k * x + b;
      qs.push(_q(`y=${k}x+${b}, при x=${x}: y=?`, [String(y), String(y + k), String(y - k), String(k * x)], y, `y=${k}×${x}+${b}=${k * x}+${b}=${y}`));
    }
  }
  return qs;
}
function gen_vectors(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const ax = _r(-4, 4) || 1,
      ay = _r(-4, 4) || 1;
    const bx = _r(-4, 4) || 1,
      by = _r(-4, 4) || 1;
    if (i % 3 === 0) {
      qs.push(_q(`|a⃗| при a⃗=(${ax};${ay}): |a⃗|²=?`, [String(ax * ax + ay * ay), String(ax * ax + ay * ay + 1), String(Math.abs(ax) + Math.abs(ay)), String((ax + ay) ** 2)], ax * ax + ay * ay, `|a⃗|²=${ax}²+${ay}²=${ax * ax}+${ay * ay}=${ax * ax + ay * ay}`));
    } else if (i % 3 === 1) {
      const cx = ax + bx,
        cy = ay + by;
      qs.push(_q(`a⃗=(${ax};${ay}), b⃗=(${bx};${by}). a⃗+b⃗=?`, [`(${cx};${cy})`, `(${ax - bx};${ay - by})`, `(${cx + 1};${cy})`, `(${ax * bx};${ay * by})`], `(${cx};${cy})`, `Складываем покоординатно: (${ax}+${bx};${ay}+${by})=(${cx};${cy})`));
    } else {
      const dot = ax * bx + ay * by;
      qs.push(_q(`Скалярное произведение (${ax};${ay})·(${bx};${by})?`, [String(dot), String(dot + 2), String(ax * bx), String(dot - ay * by)], dot, `a⃗·b⃗=${ax}×${bx}+${ay}×${by}=${ax * bx}+${ay * by}=${dot}`));
    }
  }
  return qs;
}

// ── Уровень IV ────────────────────────────────────────────────────

function gen_quadratic(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 6 === 0) {
      const p = _r(-5, 5),
        q = _r(-5, 5);
      const b = -(p + q),
        c = p * q;
      const sign = b >= 0 ? '+' : '';
      qs.push(_q(`x²${sign}${b}x${c >= 0 ? '+' : ''}${c}=0. Корни?`, [`${p} и ${q}`, `${p} и ${-q}`, `${-p} и ${-q}`, `${Math.abs(p)} и ${Math.abs(q)}`], `${p} и ${q}`, `По теореме Виета: x₁+x₂=${p + q}=−(${b}), x₁×x₂=${p * q}=${c}`));
    } else if (i % 6 === 1) {
      const a = 1,
        b = _r(-6, 6),
        c = _r(-8, 8);
      const D = b * b - 4 * a * c;
      qs.push(_q(`D=b²-4ac при a=1, b=${b}, c=${c}?`, [String(D), String(D + 4), String(b * b), String(D - 4 * c)], D, `D=${b}²-4×1×${c}=${b * b}-${4 * c}=${D}`));
    } else if (i % 6 === 2) {
      const a = _r(1, 4),
        p = _r(-3, 3);
      const vx = -p / (2 * a),
        vy = _r(-5, 0);
      qs.push(_q(`f(x)=${a}(x${p >= 0 ? '-' : '+'}${Math.abs(p)})²${vy >= 0 ? '+' : ''}${vy}. Минимум?`, [String(vy), String(vy - 1), String(vy + 1), String(0)], vy, `Вершина параболы при x=${vx}, f(${vx})=${vy}`));
    } else if (i % 6 === 3) {
      const q = _q('По графику y=x²-4 определите нули функции?', ['x=2 и x=-2', 'x=4', 'x=0', 'x=1 и x=-1'], 'x=2 и x=-2', 'Нули там, где f(x)=0: x²-4=0 → x=±2');
      q.graph = {
        fns: ['x*x - 4'],
        labels: ['y=x²-4'],
        xMin: -3,
        xMax: 3,
        yMin: -1,
        yMax: 8
      };
      qs.push(q);
    } else if (i % 6 === 4) {
      const q = _q('По графику y=x²+2x-3 найдите абсциссу вершины параболы?', ['x=-1', 'x=0', 'x=1', 'x=2'], 'x=-1', 'Вершина x=-b/(2a)=-(2)/(2)=-1');
      q.graph = {
        fns: ['x*x + 2*x - 3'],
        labels: ['y=x²+2x-3'],
        xMin: -4,
        xMax: 2,
        yMin: -4,
        yMax: 4
      };
      qs.push(q);
    } else {
      const q = _q('Найдите дискриминант x²+5x+6=0?', ['D=1', 'D=0', 'D=12', 'D=-1'], 'D=1', 'D=b²-4ac=25-24=1');
      q.formula = 'D = b^2 - 4ac';
      qs.push(q);
    }
  }
  return qs;
}
function gen_func_graphs(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 7 === 0) {
      qs.push(_q('y=x² — чётная или нечётная функция?', ['Чётная', 'Нечётная', 'Ни та, ни другая', 'Обе'], 'Чётная', 'f(−x)=(−x)²=x²=f(x) — чётная'));
    } else if (i % 7 === 1) {
      qs.push(_q('Область определения f(x)=√x?', ['x≥0', 'x>0', 'Все вещественные', 'x≠0'], 'x≥0', 'Подкоренное выражение должно быть ≥0'));
    } else if (i % 7 === 2) {
      const k = _r(1, 5) || 1;
      qs.push(_q(`y=${k}x — монотонно...`, ['возрастает на всей прямой', 'убывает', 'возрастает только при x>0', 'не монотонна'], k > 0 ? 'возрастает на всей прямой' : 'убывает', k > 0 ? `Коэффициент k=${k}>0 — функция возрастает` : 'k<0 — убывает'));
    } else if (i % 7 === 3) {
      qs.push(_q('Нули функции y=x²-4?', ['x=±2', 'x=±4', 'x=4', 'x=±√2'], 'x=±2', 'x²=4, x=±2'));
    } else if (i % 7 === 4) {
      const q = _q('Найдите нуль функции f(x)=2x+1?', ['x=-0.5', 'x=0.5', 'x=0', 'x=1'], 'x=-0.5', 'Нуль там где f(x)=0: 2x+1=0 → x=-0.5');
      q.graph = {
        fns: ['2*x + 1'],
        labels: ['y=2x+1'],
        xMin: -2,
        xMax: 2,
        yMin: -3,
        yMax: 5
      };
      qs.push(q);
    } else if (i % 7 === 5) {
      const q = _q('Функция y=x³ обладает каким свойством?', ['Нечётная', 'Чётная', 'Ни та, ни другая', 'Возрастающая везде'], 'Нечётная', 'f(−x)=(−x)³=−x³=−f(x) — нечётная');
      q.graph = {
        fns: ['x*x*x'],
        labels: ['y=x³'],
        xMin: -2,
        xMax: 2,
        yMin: -8,
        yMax: 8
      };
      qs.push(q);
    } else {
      const q = _q('Найдите минимум функции y=|x|?', ['x=0', 'x=1', 'x=-1', 'Нет минимума'], 'x=0', 'Абсолютное значение достигает минимума в 0');
      q.graph = {
        fns: ['Math.abs(x)'],
        labels: ['y=|x|'],
        xMin: -3,
        xMax: 3,
        yMin: -1,
        yMax: 4
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_systems_eq(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const x = _r(-4, 5) || 1,
      y = _r(-4, 5) || 1;
    const a1 = _r(1, 4),
      a2 = _r(1, 4);
    const b1 = a1 * x + a2 * y;
    const c1 = _r(1, 3),
      c2 = _r(1, 3);
    const b2 = c1 * x + c2 * y;
    qs.push(_q(`Система: ${a1}x+${a2}y=${b1}, ${c1}x+${c2}y=${b2}. x+y=?`, [String(x + y), String(x + y + 1), String(x * y), String(x + y - 1)], x + y, `Решение: x=${x}, y=${y}, сумма=${x + y}`));
  }
  return qs;
}
function gen_parametric(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) {
      const a = _r(1, 5);
      qs.push(_q(`|ax+1|=a при a>0. Количество решений?`, ['2', '1', '0', 'Бесконечно'], '2', `|u|=a>0 всегда даёт 2 решения: u=a или u=-a`));
    } else {
      qs.push(_q(`При каком a уравнение ax=a не имеет решений?`, ['a=0', 'a=1', 'a=-1', 'Всегда имеет'], 'a=0', 'При a=0: 0=0, уравнение имеет бесконечно много решений, но нет конкретного. При a≠0: x=1'));
    }
  }
  return qs;
}
function gen_modular(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const a = _r(1, 8);
    if (i % 2 === 0) {
      qs.push(_q(`|x|=${a}. Решения?`, [`x=${a} или x=−${a}`, `x=${a}`, `x=−${a}`, `x=±${a + 1}`], `x=${a} или x=−${a}`, `|x|=a даёт x=a или x=-a`));
    } else {
      const b = _r(1, 5);
      qs.push(_q(`|x-${a}|>${b}. Ответ?`, [`x<${a - b} или x>${a + b}`, `${a - b}<x<${a + b}`, `x>${a + b}`, `x<${a - b}`], `x<${a - b} или x>${a + b}`, `|x-${a}|>${b}: x-${a}<-${b} или x-${a}>${b}`));
    }
  }
  return qs;
}
function gen_extremal(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const a = [-1, 1][i % 2] || 1,
      p = _r(-4, 4),
      q = _r(-6, 0);
    const vx = -p / (2 * a);
    const vy = q;
    qs.push(_q(`f(x)=${a === 1 ? '' : a}x²${p >= 0 ? '+' : ''}${p}x${vy >= 0 ? '+' : ''}${vy}. ${a > 0 ? 'Минимум' : 'Максимум'}?`, [String(vy), String(vy + 1), String(vy - 1), String(0)], vy, `Вершина при x=${vx.toFixed(1)}, значение=${vy}`));
  }
  return qs;
}

// ── Уровень V ─────────────────────────────────────────────────────

function gen_trig_ratios(n) {
  const vals = {
    0: {
      sin: 0,
      cos: 1,
      tg: 0
    },
    30: {
      sin: '1/2',
      cos: '√3/2',
      tg: '1/√3'
    },
    45: {
      sin: '√2/2',
      cos: '√2/2',
      tg: 1
    },
    60: {
      sin: '√3/2',
      cos: '1/2',
      tg: '√3'
    },
    90: {
      sin: 1,
      cos: 0,
      tg: 'не опр.'
    }
  };
  const angles = [0, 30, 45, 60, 90];
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 7 === 0 || i % 7 === 1 || i % 7 === 2 || i % 7 === 3) {
      const deg = angles[i % 5];
      const fns = ['sin', 'cos', 'tg'];
      const fn = fns[i % 3];
      const val = vals[deg][fn];
      const wrong = {
        0: '1',
        1: '1/2',
        '1/2': '√2/2',
        '√3/2': '1',
        '√2/2': '1/2',
        '1/√3': '1',
        '√3': '1/√3',
        'не опр.': 0
      }[String(val)] || '0';
      qs.push(_q(`${fn} ${deg}° = ?`, [String(val), wrong, '2', '-1'], String(val), `Табличное значение: ${fn} ${deg}° = ${val}`));
    } else if (i % 7 === 4) {
      const q = _q('Какой период у функции sin(x)?', ['2π', 'π', '4π', 'π/2'], '2π', 'Синус повторяется каждые 2π радиан');
      q.graph = {
        fns: ['Math.sin(x)'],
        labels: ['sin(x)'],
        xMin: -8,
        xMax: 8,
        yMin: -1.5,
        yMax: 1.5
      };
      qs.push(q);
    } else if (i % 7 === 5) {
      const q = _q('Чему равен cos(0)?', ['1', '0', '-1', 'π'], '1', 'cos(0) — табличное значение равно 1');
      q.graph = {
        fns: ['Math.cos(x)'],
        labels: ['cos(x)'],
        xMin: -8,
        xMax: 8,
        yMin: -1.5,
        yMax: 1.5
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_trig_formulas(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 4 === 0) qs.push(_q('sin²x + cos²x = ?', ['1', '0', '2', 'sin 2x'], '1', 'Основное тригонометрическое тождество'));else if (i % 4 === 1) qs.push(_q('sin 2x = ?', ['2 sin x cos x', 'sin²x - cos²x', '1 - 2sin²x', 'cos²x - sin²x'], '2 sin x cos x', 'Формула двойного угла для синуса'));else if (i % 4 === 2) qs.push(_q('cos 2x = ?', ['cos²x - sin²x', '2 sin x cos x', 'sin²x + cos²x', '1 - cos²x'], 'cos²x - sin²x', 'Формула двойного угла для косинуса'));else qs.push(_q('tg x = ?', ['sin x / cos x', 'cos x / sin x', '1 / sin x', '1 / cos x'], 'sin x / cos x', 'Определение тангенса'));
  }
  return qs;
}
function gen_trig_eq(n) {
  const qs = [];
  const vals = [0, 'π/6', 'π/4', 'π/3', 'π/2', '-π/6', '-π/4'];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) qs.push(_q('Общее решение sin x = 0?', ['πn, n∈ℤ', 'π/2+2πn', '2πn', 'π+2πn'], 'πn, n∈ℤ', 'sin x=0 при x=πn'));else if (i % 3 === 1) qs.push(_q('Общее решение cos x = 1?', ['2πn, n∈ℤ', 'πn', 'π/2+πn', '±π+2πn'], '2πn, n∈ℤ', 'cos x=1 при x=2πn'));else qs.push(_q('Простейшее уравнение tg x = 1: x = ?', ['π/4 + πn', 'π/4 + 2πn', 'π/3 + πn', 'π/6 + πn'], 'π/4 + πn', 'tg(π/4)=1, период π'));
  }
  return qs;
}
function gen_trig_ineq(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) qs.push(_q('sin x > 0 на промежутке?', ['(0; π)', '(π; 2π)', '(-π/2; π/2)', '(π/2; 3π/2)'], '(0; π)', 'sin x > 0 в I и II четвертях, т.е. на (0; π)'));else qs.push(_q('cos x < 0 на промежутке?', ['(π/2; 3π/2)', '(0; π)', '(-π; 0)', '(0; π/2)'], '(π/2; 3π/2)', 'cos x < 0 во II и III четвертях'));
  }
  return qs;
}
function gen_periodic(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 5 === 0) qs.push(_q('Период функции y = sin x?', ['2π', 'π', '4π', 'π/2'], '2π', 'sin повторяется каждые 2π'));else if (i % 5 === 1) qs.push(_q('Период y = cos 2x?', ['π', '2π', 'π/2', '4π'], 'π', 'Период cos kx = 2π/k = 2π/2 = π'));else if (i % 5 === 2) {
      const k = _r(2, 5);
      qs.push(_q(`Амплитуда y=${k}sin x?`, [String(k), '1', String(k * 2), String(k / 2)], k, `Амплитуда = коэффициент перед sin = ${k}`));
    } else if (i % 5 === 3) {
      const q = _q('По графику y=sin(2x) найдите период?', ['π', '2π', 'π/2', '4π'], 'π', 'Период sin(kx) = 2π/k = 2π/2 = π');
      q.graph = {
        fns: ['Math.sin(2*x)'],
        labels: ['sin(2x)'],
        xMin: -5,
        xMax: 5,
        yMin: -2,
        yMax: 2
      };
      qs.push(q);
    } else {
      const q = _q('По графику y=2sin(x) найдите амплитуду?', ['2', '1', '4', 'π'], '2', 'Амплитуда = коэффициент перед синусом = 2');
      q.graph = {
        fns: ['2*Math.sin(x)'],
        labels: ['2sin(x)'],
        xMin: -8,
        xMax: 8,
        yMin: -3,
        yMax: 3
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_harmonic(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) qs.push(_q('Ряд Фурье разлагает функцию в...?', ['Сумму синусов и косинусов', 'Сумму степеней', 'Произведение тригонометрических функций', 'Сумму экспонент'], 'Сумму синусов и косинусов', 'Ряд Фурье: a₀+Σ(aₙcos nx+bₙsin nx)'));else if (i % 3 === 1) qs.push(_q('Какие функции раскладываются в ряд Фурье?', ['Периодические', 'Только чётные', 'Только нечётные', 'Монотонные'], 'Периодические', 'Теорема Дирихле: периодические кусочно-монотонные функции'));else qs.push(_q('Коэффициент Фурье a₀ — это...?', ['Среднее значение функции за период', 'Амплитуда основной гармоники', 'Фаза колебания', 'Частота'], 'Среднее значение функции за период', 'a₀=(1/T)∫f(x)dx — среднее'));
  }
  return qs;
}

// ── Уровень VI ────────────────────────────────────────────────────

function gen_seq_limits(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 4 === 0) qs.push(_q('lim(n→∞) 1/n = ?', ['0', '1', '∞', '−1'], '0', '1/n стремится к 0 при n→∞'));else if (i % 4 === 1) qs.push(_q('lim(n→∞) (1+1/n)^n = ?', ['e ≈ 2.718', '1', '∞', '2'], 'e ≈ 2.718', 'Второй замечательный предел'));else if (i % 4 === 2) {
      const a = _r(2, 6);
      qs.push(_q(`lim(n→∞) ${a}/n² = ?`, ['0', String(a), '∞', '1'], '0', `${a}/n²→0, т.к. n²→∞`));
    } else qs.push(_q('lim(n→∞) n/(n+1) = ?', ['1', '0', '∞', '1/2'], '1', 'n/(n+1)=1/(1+1/n)→1'));
  }
  return qs;
}
function gen_func_limits(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 5 === 0) qs.push(_q('lim(x→0) sin x/x = ?', ['1', '0', '∞', 'sin 0'], '1', 'Первый замечательный предел'));else if (i % 5 === 1) {
      const a = _r(2, 7);
      qs.push(_q(`lim(x→${a}) (x²-${a * a})/(x-${a}) = ?`, [String(2 * a), String(a), String(a * a), '0'], String(2 * a), `(x-${a})(x+${a})/(x-${a})→${a}+${a}=${2 * a}`));
    } else if (i % 5 === 2) qs.push(_q('lim(x→0) (1-cos x)/x² = ?', ['1/2', '1', '0', '2'], '1/2', 'Эквивалентность: 1-cos x ~ x²/2'));else if (i % 5 === 3) qs.push(_q('lim(x→∞) (3x²+1)/(x²-5) = ?', ['3', '1', '0', '∞'], '3', 'Делим числитель и знаменатель на x²: (3+1/x²)/(1-5/x²)→3'));else {
      const q = _q('По графику y=sin(x)/x найдите предел при x→0?', ['1', '0', 'π', '∞'], '1', 'Первый замечательный предел: lim sin(x)/x = 1 при x→0');
      q.graph = {
        fns: ['Math.sin(x)/x'],
        labels: ['sin(x)/x'],
        xMin: -10,
        xMax: 10,
        yMin: -0.5,
        yMax: 1.5
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_derivative(n) {
  const qs = [];
  const rules = [{
    f: 'x^n',
    d: 'n·x^(n-1)',
    q: "(x³)' = ?",
    a: '3x²',
    wrong: ['3x³', 'x²', '3']
  }, {
    f: 'sin x',
    d: 'cos x',
    q: "(sin x)' = ?",
    a: 'cos x',
    wrong: ['−sin x', '−cos x', 'tg x']
  }, {
    f: 'cos x',
    d: '−sin x',
    q: "(cos x)' = ?",
    a: '−sin x',
    wrong: ['sin x', 'cos x', '−cos x']
  }, {
    f: 'e^x',
    d: 'e^x',
    q: "(eˣ)' = ?",
    a: 'eˣ',
    wrong: ['x·eˣ', 'eˣ⁻¹', '1/eˣ']
  }, {
    f: 'ln x',
    d: '1/x',
    q: "(ln x)' = ?",
    a: '1/x',
    wrong: ['ln x', 'x', '1/ln x']
  }];
  for (let i = 0; i < n; i++) {
    if (i < 3 || i % 7 < 4) {
      const r = rules[i % rules.length];
      qs.push(_q(r.q, [r.a, ...r.wrong], r.a, `Производная ${r.f} = ${r.d}`));
    } else if (i % 7 === 4) {
      const q = _q('По графику f(x)=x² найдите производную f\'(x)?', ['2x', 'x²', 'x', '2'], '2x', 'Производная f(x)=x² есть f\'(x)=2x');
      q.graph = {
        fns: ['x*x'],
        labels: ['f(x)=x²'],
        xMin: -3,
        xMax: 3,
        yMin: -1,
        yMax: 10
      };
      q.formula = 'f(x) = x^2';
      qs.push(q);
    } else if (i % 7 === 5) {
      const q = _q('По графику найдите где функция f(x)=x³/3-x убывает?', ['(−1, 1)', '(−∞, −1)', '(1, +∞)', 'везде'], '(−1, 1)', 'f\'(x)=x²-1=0 при x=±1, f убывает между корнями');
      q.graph = {
        fns: ['x*x*x/3 - x'],
        labels: ['y=x³/3−x'],
        xMin: -3,
        xMax: 3,
        yMin: -3,
        yMax: 3
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_func_analysis(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) qs.push(_q("f(x)=x³-3x. Критические точки?", ['x=±1', 'x=0', 'x=±3', 'x=1'], 'x=±1', "f'=3x²-3=0 → x²=1 → x=±1"));else if (i % 3 === 1) qs.push(_q("f''(x)<0 означает, что функция...", ['Выпуклая', 'Вогнутая', 'Возрастающая', 'Убывающая'], 'Выпуклая', "f''<0 — функция выпуклая (направлена вниз)"));else qs.push(_q("y=1/x: горизонтальная асимптота?", ['y=0', 'y=1', 'x=0', 'Нет асимптоты'], 'y=0', "При x→±∞, 1/x→0"));
  }
  return qs;
}
function gen_integral(n) {
  const qs = [];
  const rules = [{
    q: '∫x dx = ?',
    a: 'x²/2 + C',
    wrong: ['x²+C', '2x+C', 'x/2+C']
  }, {
    q: '∫cos x dx = ?',
    a: 'sin x + C',
    wrong: ['−sin x+C', 'cos x+C', '−cos x+C']
  }, {
    q: '∫eˣ dx = ?',
    a: 'eˣ + C',
    wrong: ['eˣ⁺¹+C', 'x·eˣ+C', '1/eˣ+C']
  }, {
    q: '∫1/x dx = ?',
    a: 'ln|x| + C',
    wrong: ['1/x²+C', 'x ln x+C', '−1/x²+C']
  }, {
    q: '∫sin x dx = ?',
    a: '−cos x + C',
    wrong: ['cos x+C', 'sin x+C', '−sin x+C']
  }];
  for (let i = 0; i < n; i++) {
    if (i < 3 || i % 7 < 4) {
      const r = rules[i % rules.length];
      qs.push(_q(r.q, [r.a, ...r.wrong], r.a, `Формула интегрирования`));
    } else if (i % 7 === 4) {
      const q = _q('Вычислите определённый интеграл ∫₀² x dx?', ['2', '4', '1', '8'], '2', '∫₀² x dx = [x²/2]₀² = 2²/2 - 0 = 2');
      q.formula = '\\int_0^2 x\\,dx';
      qs.push(q);
    } else if (i % 7 === 5) {
      const q = _q('По графику y=x² найдите площадь под кривой от 0 до 3?', ['9', '3', '27', '18'], '9', 'S=∫₀³ x² dx = [x³/3]₀³ = 27/3 = 9');
      q.graph = {
        fns: ['x*x'],
        labels: ['y=x²'],
        xMin: -1,
        xMax: 4,
        yMin: -1,
        yMax: 10
      };
      qs.push(q);
    }
  }
  return qs;
}
function gen_optimization(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const L = _r(20, 60);
      const side = L / 4;
      qs.push(_q(`Периметр квадрата ${L}. Максимальная площадь прямоугольника с тем же периметром?`, [String(side * side), String(L * L / 16 + L), String(L * L / 4), String(side)], side * side, `Квадрат даёт максимум: сторона ${L}/4=${side}, S=${side}²=${side * side}`));
    } else if (i % 3 === 1) {
      qs.push(_q('Как найти минимум функции на отрезке?', ['Сравнить критические точки и концы', 'Только критические точки', 'Только концы отрезка', 'Взять f(0)'], 'Сравнить критические точки и концы', 'Нужно проверить все критические точки внутри и значения на концах'));
    } else {
      const q = _q('По графику f(x)=x²-4x+3 найдите точку минимума?', ['x=2', 'x=0', 'x=1', 'x=3'], 'x=2', 'f\'(x)=2x-4=0 при x=2 — минимум');
      q.graph = {
        fns: ['x*x - 4*x + 3'],
        labels: ['y=x²−4x+3'],
        xMin: -1,
        xMax: 5,
        yMin: -2,
        yMax: 6
      };
      qs.push(q);
    }
  }
  return qs;
}

// ── Уровень VII ───────────────────────────────────────────────────

function gen_combinatorics(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const n2 = _r(4, 7),
        k = _r(1, 3);
      let c = 1;
      for (let j = 0; j < k; j++) c *= n2 - j;
      for (let j = 1; j <= k; j++) c /= j;
      qs.push(_q(`C(${n2},${k}) = ?`, [String(c), String(c + k), String(c - 1), String(n2 * k)], c, `C(${n2},${k})=${n2}!/(${k}!(${n2}-${k})!)=${c}`));
    } else if (i % 3 === 1) {
      const n2 = _r(3, 6);
      const p = n2 * (n2 - 1) * (n2 - 2);
      qs.push(_q(`A(${n2},3) — размещения из ${n2} по 3?`, [String(p), String(p / 6), String(n2 * 3), String(p + n2)], p, `A(${n2},3)=${n2}×${n2 - 1}×${n2 - 2}=${p}`));
    } else {
      const n2 = _r(3, 6);
      let f = 1;
      for (let j = 1; j <= n2; j++) f *= j;
      qs.push(_q(`${n2}! = ?`, [String(f), String(f + n2), String(f - 1), String(n2 * n2)], f, `${n2}!=${Array.from({
        length: n2
      }, (v, k) => k + 1).join('×')}=${f}`));
    }
  }
  return qs;
}
function gen_graph_theory(n) {
  const qs = [];
  const facts = [{
    q: 'Полный граф K₄ имеет рёбер?',
    a: '6',
    wrong: ['4', '8', '12'],
    e: 'K_n имеет n(n-1)/2 рёбер: 4×3/2=6'
  }, {
    q: 'Дерево с n вершинами имеет рёбер?',
    a: 'n-1',
    wrong: ['n', 'n+1', '2n'],
    e: 'Дерево — связный граф без циклов, всегда n-1 рёбер'
  }, {
    q: 'Эйлеров путь существует если...?',
    a: 'Ровно 0 или 2 вершины нечётной степени',
    wrong: ['Все вершины чётной степени', 'Любой граф', 'Граф связный'],
    e: 'Теорема Эйлера о существовании эйлерова пути'
  }, {
    q: 'Степень вершины в графе — это...?',
    a: 'Число рёбер, инцидентных вершине',
    wrong: ['Число вершин', 'Длина пути', 'Номер вершины'],
    e: 'Степень deg(v) = количество рёбер, выходящих из v'
  }, {
    q: 'Двудольный граф — это граф, где...?',
    a: 'Вершины делятся на 2 множества без рёбер внутри',
    wrong: ['Нет циклов', 'Все вершины степени 2', 'Граф полный'],
    e: 'Биграф: V=V₁∪V₂, рёбра только между V₁ и V₂'
  }];
  for (let i = 0; i < n; i++) {
    const f = facts[i % facts.length];
    qs.push(_q(f.q, [f.a, ...f.wrong], f.a, f.e));
  }
  return qs;
}
function gen_logic_systems(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 4 === 0) qs.push(_q('A И B = 1 когда?', ['Оба равны 1', 'Хотя бы один равен 1', 'Хотя бы один равен 0', 'A=1, B=0'], 'Оба равны 1', 'Конъюнкция истинна только когда оба операнда истинны'));else if (i % 4 === 1) qs.push(_q('НЕ(A ИЛИ B) = ?', ['НЕA И НЕB', 'НЕA ИЛИ НЕB', 'A И B', 'НЕA ИЛИ B'], 'НЕA И НЕB', 'Закон де Моргана: ¬(A∨B)=¬A∧¬B'));else if (i % 4 === 2) {
      const a = Math.random() > 0.5,
        b = Math.random() > 0.5;
      const r = a && b;
      qs.push(_q(`A=${a ? 1 : 0}, B=${b ? 1 : 0}. A И B = ?`, [String(r ? 1 : 0), String(r ? 0 : 1), '2', '0.5'], String(r ? 1 : 0), `${a ? 1 : 0} AND ${b ? 1 : 0} = ${r ? 1 : 0}`));
    } else qs.push(_q('Тавтология — это высказывание, которое...?', ['Истинно при любых значениях переменных', 'Всегда ложно', 'Иногда истинно', 'Зависит от переменных'], 'Истинно при любых значениях переменных', 'Тавтология: A∨¬A всегда истинна'));
  }
  return qs;
}
function gen_recurrence(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) {
      const a0 = _r(1, 5),
        a1 = _r(1, 5);
      const a2 = a0 + a1,
        a3 = a1 + a2,
        a4 = a2 + a3;
      qs.push(_q(`aₙ=aₙ₋₁+aₙ₋₂, a₀=${a0}, a₁=${a1}. a₄=?`, [String(a4), String(a4 + 1), String(a3), String(a2 + a0)], a4, `a₂=${a2}, a₃=${a3}, a₄=${a3}+${a2}=${a4}`));
    } else {
      const a = _r(2, 5),
        n2 = _r(3, 6);
      const val = a * Math.pow(2, n2);
      qs.push(_q(`aₙ=2aₙ₋₁, a₀=${a}. a${n2}=?`, [String(val), String(val * 2), String(a * n2), String(val - a)], val, `a${n2}=${a}×2^${n2}=${val}`));
    }
  }
  return qs;
}
function gen_boolean_alg(n) {
  const qs = [];
  const facts = [{
    q: 'A И 1 = ?',
    a: 'A',
    wrong: ['1', '0', 'НЕ A'],
    e: 'Нейтральный элемент конъюнкции: A∧1=A'
  }, {
    q: 'A ИЛИ 0 = ?',
    a: 'A',
    wrong: ['0', '1', 'НЕ A'],
    e: 'Нейтральный элемент дизъюнкции: A∨0=A'
  }, {
    q: 'A И НЕ A = ?',
    a: '0',
    wrong: ['A', '1', 'НЕ A'],
    e: 'Закон противоречия: A∧¬A=0'
  }, {
    q: 'A ИЛИ НЕ A = ?',
    a: '1',
    wrong: ['A', '0', 'НЕ A'],
    e: 'Закон исключённого третьего: A∨¬A=1'
  }, {
    q: 'A XOR A = ?',
    a: '0',
    wrong: ['A', '1', '2A'],
    e: 'Исключающее ИЛИ: A⊕A=0'
  }];
  for (let i = 0; i < n; i++) {
    const f = facts[i % facts.length];
    qs.push(_q(f.q, [f.a, ...f.wrong], f.a, f.e));
  }
  return qs;
}
function gen_algo_thinking(n) {
  const qs = [];
  const facts = [{
    q: 'O(n²) против O(n log n): что быстрее при большом n?',
    a: 'O(n log n)',
    wrong: ['O(n²)', 'Одинаково', 'Зависит от n'],
    e: 'n log n растёт медленнее n²'
  }, {
    q: 'Сортировка пузырьком: сложность?',
    a: 'O(n²)',
    wrong: ['O(n)', 'O(n log n)', 'O(1)'],
    e: 'В худшем случае n×(n-1)/2 сравнений → O(n²)'
  }, {
    q: 'Бинарный поиск в отсортированном массиве: сложность?',
    a: 'O(log n)',
    wrong: ['O(n)', 'O(n²)', 'O(1)'],
    e: 'Каждый шаг делит массив пополам'
  }, {
    q: 'Алгоритм с O(1) означает...?',
    a: 'Константное время, не зависит от n',
    wrong: ['Работает за 1 шаг', 'Самый быстрый', 'Линейное время'],
    e: 'O(1) — время не зависит от размера входных данных'
  }, {
    q: 'Задача NP-полная означает...?',
    a: 'Нет известного полиномиального алгоритма',
    wrong: ['Нерешаемая', 'Решается за O(n)', 'Решается за O(n²)'],
    e: 'NP-полные задачи — возможно, не решаются за полиномиальное время'
  }];
  for (let i = 0; i < n; i++) {
    const f = facts[i % facts.length];
    qs.push(_q(f.q, [f.a, ...f.wrong], f.a, f.e));
  }
  return qs;
}

// ── Уровень VIII ──────────────────────────────────────────────────

function gen_classic_prob(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const m = _r(1, 5),
        n2 = _r(6, 10);
      const ans = `${m}/${n2}`;
      qs.push(_q(`В урне ${n2} шаров, ${m} красных. P(красный)?`, [ans, `${m}/${n2 - m}`, `${n2 - m}/${n2}`, `${m}/${n2 + 1}`], ans, `P=m/n=${m}/${n2}`));
    } else if (i % 3 === 1) {
      const p1 = _r(1, 4),
        p2 = _r(1, 4),
        n2 = 10;
      const ans = p1 + p2;
      qs.push(_q(`P(A)=${p1}/${n2}, P(B)=${p2}/${n2}, несовместные. P(A+B)?`, [`${ans}/${n2}`, `${p1 * p2}/${n2 * n2}`, `${ans + 1}/${n2}`, `${n2}/${ans}`], `${ans}/${n2}`, `P(A∪B)=P(A)+P(B)=${p1}/${n2}+${p2}/${n2}=${ans}/${n2}`));
    } else {
      const d = _r(1, 5);
      const p = d / 6;
      qs.push(_q(`Игральная кость. P(выпадет ${d})?`, ['1/6', '1/3', '1/2', '1'], '1/6', `Все 6 граней равновероятны: P=1/6`));
    }
  }
  return qs;
}
function gen_comb_prob(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) {
      const n2 = _r(5, 8),
        k = 2;
      let c = n2 * (n2 - 1) / 2;
      const favN = _r(2, 4);
      let cf = favN * (favN - 1) / 2;
      const p = `${cf}/${c}`;
      qs.push(_q(`Из ${n2} шаров ${favN} красных. P(оба красных из 2)?`, [p, `${favN}/${n2}`, `${cf}/${c + 1}`, `${favN * (favN - 1)}/${n2 * (n2 - 1)}`], p, `C(${favN},2)/C(${n2},2)=${cf}/${c}`));
    } else {
      qs.push(_q('5 монет. P(ровно 3 орла)?', ['10/32', '1/32', '5/32', '16/32'], '10/32', 'C(5,3)/2⁵=10/32'));
    }
  }
  return qs;
}
function gen_random_var(n) {
  const qs = [];
  const facts = [{
    q: 'Дискретная случайная величина — это...?',
    a: 'Принимает счётное число значений',
    wrong: ['Любые вещественные значения', 'Только 0 и 1', 'Равномерно распределённая'],
    e: 'Дискретная СВ: X∈{x₁,x₂,...}'
  }, {
    q: 'Нормальное распределение задаётся...?',
    a: 'Средним и дисперсией',
    wrong: ['Только средним', 'Медианой', 'Максимальным значением'],
    e: 'N(μ,σ²) — параметры: математическое ожидание μ и дисперсия σ²'
  }, {
    q: 'P(X=k) для биномиального распределения B(n,p)?',
    a: 'C(n,k)·pᵏ·(1-p)^(n-k)',
    wrong: ['p^k', 'n!·p^k', 'C(n,k)/pᵏ'],
    e: 'Формула Бернулли'
  }, {
    q: 'Функция распределения F(x) = ?',
    a: 'P(X < x)',
    wrong: ['P(X = x)', 'P(X > x)', '1/P(X=x)'],
    e: 'F(x) = P(X < x) — вероятность того, что X меньше x'
  }, {
    q: 'Геометрическое распределение описывает...?',
    a: 'Число испытаний до первого успеха',
    wrong: ['Сумму успехов', 'Число неудач', 'Среднее число успехов'],
    e: 'P(X=k)=(1-p)^(k-1)·p'
  }];
  for (let i = 0; i < n; i++) {
    const f = facts[i % facts.length];
    qs.push(_q(f.q, [f.a, ...f.wrong], f.a, f.e));
  }
  return qs;
}
function gen_expectation(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const vals = [_r(1, 5), _r(6, 10)],
        probs = [0.3, 0.7];
      const e = vals[0] * probs[0] + vals[1] * probs[1];
      qs.push(_q(`X: ${vals[0]} с вер. 0.3, ${vals[1]} с вер. 0.7. E[X]=?`, [e.toFixed(1), String(vals[0] + vals[1]), String(vals[0] * vals[1]), ((vals[0] + vals[1]) / 2).toFixed(1)], e.toFixed(1), `E[X]=${vals[0]}×0.3+${vals[1]}×0.7=${e.toFixed(1)}`));
    } else if (i % 3 === 1) {
      const n2 = _r(3, 8),
        p = 0.5;
      const e = n2 * p;
      qs.push(_q(`E[X] для B(${n2}, 0.5)?`, [String(e), String(n2), String(e + 1), String(e - 1)], e, `E=np=${n2}×0.5=${e}`));
    } else {
      qs.push(_q('D[X] = E[X²] − ... ?', ['(E[X])²', 'E[X]', '2E[X]', 'E[X²]'], '(E[X])²', 'Дисперсия: D[X]=E[X²]−(E[X])²'));
    }
  }
  return qs;
}
function gen_statistics(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 3 === 0) {
      const arr = Array.from({
        length: 5
      }, () => _r(1, 10));
      const sum = arr.reduce((a, b) => a + b, 0);
      const mean = sum / 5;
      qs.push(_q(`Данные: ${arr.join(', ')}. Среднее?`, [mean.toFixed(1), (mean + 1).toFixed(1), (mean - 1).toFixed(1), sum.toFixed(1)], mean.toFixed(1), `Среднее=(${arr.join('+')})/5=${sum}/5=${mean.toFixed(1)}`));
    } else if (i % 3 === 1) {
      const arr = [3, 5, 7, 7, 9].sort();
      qs.push(_q(`Данные: ${arr.join(', ')}. Медиана?`, ['7', '5', '9', '6'], '7', 'Медиана — средний элемент отсортированного ряда'));
    } else {
      qs.push(_q('Корреляция r=0.9 означает...?', ['Сильная положительная связь', 'Слабая связь', 'Отрицательная связь', 'Нет связи'], 'Сильная положительная связь', '|r| близко к 1 — сильная связь; r>0 — положительная'));
    }
  }
  return qs;
}
function gen_decision_theory(n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    if (i % 2 === 0) {
      const p = _r(3, 7) / 10,
        prize = _r(10, 50),
        cost = Math.round(prize * p * 0.8);
      const ev = Math.round(prize * p * 10) / 10;
      qs.push(_q(`Лотерея: выиграть ${prize}₽ с вер. ${p}. Ожидаемый выигрыш?`, [`${ev}₽`, `${prize * p + 1}₽`, `${prize}₽`, `${p * 100}₽`], `${ev}₽`, `E=${prize}×${p}=${ev}₽`));
    } else {
      qs.push(_q('Критерий максимина — это...?', ['Максимизация минимального выигрыша', 'Максимизация ожидаемого', 'Минимизация максимального проигрыша', 'Выбор наибольшего шанса'], 'Максимизация минимального выигрыша', 'Пессимистичная стратегия: выбираем максимум из минимальных гарантированных результатов'));
    }
  }
  return qs;
}

// ── Уровни IX–XVI — знаниевые вопросы ────────────────────────────

function knowledgeQuiz(facts, n) {
  const qs = [];
  for (let i = 0; i < n; i++) {
    const f = facts[i % facts.length];
    qs.push(_q(f.q, [f.a, ...f.wrong], f.a, f.e));
  }
  return qs;
}
const FACTS_IX = [{
  q: 'Принцип Дирихле: n+1 предмет в n ящиках → ?',
  a: 'В каком-то ящике ≥2 предметов',
  wrong: ['Все ящики полные', 'Один ящик пустой', 'Равное распределение'],
  e: 'Принцип Дирихле (pigeonhole principle)'
}, {
  q: 'Инвариант задачи — это...?',
  a: 'Величина, не меняющаяся при допустимых операциях',
  wrong: ['Ответ задачи', 'Начальное условие', 'Метод доказательства'],
  e: 'Инвариант сохраняется — мощный метод олимпиадных задач'
}, {
  q: 'Метод математической индукции: база + шаг → ?',
  a: 'Утверждение истинно для всех натуральных n',
  wrong: ['Для конечного числа n', 'Только для чётных n', 'Не доказывает ничего'],
  e: 'База: P(1). Шаг: P(k)→P(k+1). Вывод: P(n) для всех n'
}, {
  q: 'Экстремальный принцип: рассматриваем...?',
  a: 'Наибольший или наименьший элемент',
  wrong: ['Любой элемент', 'Средний элемент', 'Случайный элемент'],
  e: 'Рассмотрение экстремального элемента часто даёт противоречие или доказательство'
}, {
  q: 'Олимпиадная задача на чётность: если сумма n чисел нечётна, то...?',
  a: 'Нечётное количество нечётных слагаемых',
  wrong: ['Все слагаемые нечётные', 'Хотя бы одно чётное', 'Ровно одно нечётное'],
  e: 'Чётность суммы определяется чётностью количества нечётных слагаемых'
}, {
  q: 'Принцип крайнего: в задачах о покрытии часто берём...?',
  a: 'Самый удалённый непокрытый элемент',
  wrong: ['Ближайший элемент', 'Любой элемент', 'Центральный элемент'],
  e: 'Рассмотрение крайнего/граничного случая'
}];
const FACTS_X = [{
  q: 'Определитель матрицы A: если det(A)=0, то...?',
  a: 'Матрица вырождена (нет обратной)',
  wrong: ['Матрица единичная', 'Система несовместна', 'Матрица симметрична'],
  e: 'det(A)=0 ↔ матрица необратима ↔ СЛАУ либо несовместна, либо неопределена'
}, {
  q: 'Малая теорема Ферма: aᵖ⁻¹ ≡ ? (mod p), где p простое, НОД(a,p)=1',
  a: '1',
  wrong: ['0', 'p-1', 'a'],
  e: 'Малая теорема Ферма: если p простое и p∤a, то aᵖ⁻¹≡1(mod p)'
}, {
  q: 'Группа — это множество с операцией, где выполняются...?',
  a: 'Ассоциативность, нейтральный элемент, обратные элементы',
  wrong: ['Только коммутативность', 'Только замкнутость', 'Дистрибутивность'],
  e: 'Аксиомы группы: замкнутость, ассоциативность, нейтральный, обратный'
}, {
  q: 'Формула Эйлера: e^(iπ) + ? = 0',
  a: '1',
  wrong: ['−1', 'i', 'e'],
  e: 'e^(iπ)+1=0 — самая красивая формула математики'
}, {
  q: 'ОДУ первого порядка с разделяющимися переменными: dy/dx = f(x)g(y) решается...?',
  a: 'Разделением: dy/g(y) = f(x)dx, затем интегрированием',
  wrong: ['Только числено', 'Методом Гаусса', 'Характеристическим уравнением'],
  e: 'Разделение переменных — основной метод'
}, {
  q: 'Собственные значения матрицы A — это корни...?',
  a: 'Характеристического многочлена det(A-λI)=0',
  wrong: ['det(A)=0', 'следа матрицы', 'det(A+λI)=0'],
  e: 'Av=λv → (A-λI)v=0 → det(A-λI)=0'
}];
const FACTS_XI = [{
  q: 'Теорема Гёделя о неполноте говорит...?',
  a: 'Любая достаточно богатая формальная система содержит истинные, но недоказуемые утверждения',
  wrong: ['Математика непротиворечива', 'Все истины доказуемы', 'Математика неполна в смысле ошибок'],
  e: 'Первая теорема Гёделя (1931): в любой непротиворечивой системе, содержащей арифметику, есть истинные недоказуемые утверждения'
}, {
  q: 'Энтропия Шеннона H = ?',
  a: '−Σ pᵢ log pᵢ',
  wrong: ['Σ pᵢ log pᵢ', 'Σ pᵢ²', 'log(n)'],
  e: 'H(X)=−Σ P(xᵢ)log P(xᵢ) — мера неопределённости'
}, {
  q: 'Цепь Маркова — это...?',
  a: 'Процесс, где будущее зависит только от настоящего',
  wrong: ['Произвольный случайный процесс', 'Детерминированный процесс', 'Функция времени'],
  e: 'Марковское свойство: P(Xₙ₊₁|X₁,...,Xₙ)=P(Xₙ₊₁|Xₙ)'
}, {
  q: 'Линейное программирование: оптимум достигается...?',
  a: 'В вершине многогранника допустимых решений',
  wrong: ['В центре области', 'На ребре', 'В любой точке'],
  e: 'Симплекс-метод перебирает вершины многогранника'
}, {
  q: 'Теория категорий изучает...?',
  a: 'Математические структуры и отображения между ними',
  wrong: ['Только теорию множеств', 'Только топологию', 'Алгебраические уравнения'],
  e: 'Категории: объекты + морфизмы + законы композиции'
}, {
  q: 'Стохастический процесс с непрерывным временем, марковский: пример?',
  a: 'Броуновское движение',
  wrong: ['Случайная выборка', 'Ряд Фурье', 'Арифметическая прогрессия'],
  e: 'Броуновское движение (процесс Винера) — базовый пример'
}];
const FACTS_XII = [{
  q: 'Трилистник — это...?',
  a: 'Простейший нетривиальный узел',
  wrong: ['Многоугольник', 'Граф', 'Функция'],
  e: 'Трилистник (trefoil knot) — простейший нетривиальный топологический узел'
}, {
  q: 'Эллиптическая кривая над ℝ: уравнение?',
  a: 'y² = x³ + ax + b',
  wrong: ['y = ax² + bx + c', 'x² + y² = r²', 'xy = a'],
  e: 'Уравнение Вейерштрасса для эллиптических кривых'
}, {
  q: 'Сколько задач тысячелетия решено на сегодня?',
  a: '1 (гипотеза Пуанкаре)',
  wrong: ['0', '2', '3'],
  e: 'Григорий Перельман решил гипотезу Пуанкаре в 2003 г., отказавшись от премии'
}, {
  q: 'Программа Ленглендса связывает...?',
  a: 'Теорию чисел и теорию представлений',
  wrong: ['Топологию и алгебру', 'Анализ и геометрию', 'Вероятность и статистику'],
  e: 'Грандиозная программа, объединяющая разные области математики'
}, {
  q: 'Теорема Вейля о точках на многообразии над конечными полями использует...?',
  a: 'Аналог гипотезы Римана для конечных полей',
  wrong: ['Теорему Пифагора', 'Интеграл Лебега', 'Метод Монте-Карло'],
  e: 'Делиньи доказал аналог ГР для конечных полей (1974, Медаль Филдса)'
}, {
  q: 'Теория мотивов предложена...?',
  a: 'Гротендиком в 1960-х',
  wrong: ['Эйлером', 'Гауссом', 'Нетер'],
  e: 'Александр Гротендик предложил идею "универсальной когомологической теории"'
}];
const FACTS_XIII = [{
  q: 'Гипотеза Римана: нули ζ(s) лежат на прямой Re(s) = ?',
  a: '1/2',
  wrong: ['1', '0', '2'],
  e: 'Все нетривиальные нули ζ(s) лежат на критической прямой Re(s)=1/2'
}, {
  q: 'P vs NP: если P=NP, это означает...?',
  a: 'Все задачи, решение которых легко проверить, можно легко найти',
  wrong: ['Все задачи неразрешимы', 'Компьютеры станут бесполезны', 'Криптография станет надёжнее'],
  e: 'P=NP разрушило бы криптографию — все NP-задачи решались бы эффективно'
}, {
  q: 'Уравнения Навье-Стокса описывают...?',
  a: 'Движение вязкой жидкости',
  wrong: ['Движение твёрдого тела', 'Квантовые эффекты', 'Гравитацию'],
  e: 'НС-уравнения — основа гидродинамики, но существование гладких решений не доказано'
}, {
  q: 'Гипотеза BSD связана с...?',
  a: 'Эллиптическими кривыми и L-функциями',
  wrong: ['Простыми числами', 'Матрицами', 'Интегралами'],
  e: 'Берч и Свиннертон-Дайер (1965): ранг группы рациональных точек ↔ порядок нуля L(E,s) в s=1'
}, {
  q: 'Yang-Mills Mass Gap: доказать что...?',
  a: 'Квантовая теория Янга-Миллса имеет ненулевую массу щели',
  wrong: ['Частицы безмассовые', 'Теория не существует', 'Все поля классические'],
  e: 'Математическое обоснование квантовой теории поля — $ 1 000 000 от Clay Math Institute'
}, {
  q: 'Сколько проблем тысячелетия предложил Clay Institute?',
  a: '7',
  wrong: ['5', '6', '10'],
  e: 'Clay Mathematics Institute в 2000 г. предложил 7 проблем по 1 млн долларов каждая'
}];
const FACTS_XIV = [{
  q: 'Гипотеза Гольдбаха: каждое чётное число ≥4 есть...?',
  a: 'Сумма двух простых',
  wrong: ['Произведение двух простых', 'Разность двух простых', 'Степень простого'],
  e: 'Сформулирована в 1742, до сих пор не доказана для всех чисел'
}, {
  q: 'NP-полная задача: первой доказанной была...?',
  a: 'SAT (задача выполнимости)',
  wrong: ['Задача коммивояжёра', 'Задача о рюкзаке', 'Разложение на простые'],
  e: 'Теорема Кука-Левина (1971): SAT — первая NP-полная задача'
}, {
  q: 'Перельман доказал гипотезу Пуанкаре используя...?',
  a: 'Поток Риччи',
  wrong: ['Теорему Пифагора', 'Спектральную теорию', 'Метод конечных элементов'],
  e: 'Перельман использовал поток Риччи Гамильтона с хирургией (2003)'
}, {
  q: 'Теорема Гаусса-Бонне связывает геометрию и...?',
  a: 'Топологию (Эйлерову характеристику)',
  wrong: ['Алгебру', 'Теорию вероятностей', 'Анализ'],
  e: '∫K dA = 2πχ(M) — интеграл кривизны = топологический инвариант'
}, {
  q: 'Гипотеза Риманна доказана для первых 10^13 нулей?',
  a: 'Да, численно проверена, но не доказана в общем',
  wrong: ['Полностью доказана', 'Опровергнута', 'Никогда не проверялась'],
  e: 'Численные вычисления подтверждают ГР, но общее доказательство отсутствует'
}, {
  q: 'Простых чисел бесконечно много. Это доказал...?',
  a: 'Евклид (~300 до н.э.)',
  wrong: ['Гаусс', 'Риман', 'Эйлер'],
  e: 'Евклид: предположим конечное множество, умножим все и +1 — противоречие'
}];
const FACTS_XV = [{
  q: 'Что изучает теория категорий?',
  a: 'Математические структуры и морфизмы между ними',
  wrong: ['Только алгебру', 'Только топологию', 'Теорию вероятностей'],
  e: 'Теория категорий — абстрактный язык для всей математики'
}, {
  q: 'Гипотеза континуума: |ℕ| < |X| < |ℝ| — такое X...?',
  a: 'Не существует (независимо от ZFC)',
  wrong: ['Существует', 'Это комплексные числа', 'Это рациональные числа'],
  e: 'Коэн (1963) доказал независимость ГК от стандартных аксиом ZFC'
}, {
  q: 'Аксиома выбора — это...?',
  a: 'Аксиома, допускающая выбор из бесконечного числа множеств',
  wrong: ['Теорема', 'Определение', 'Следствие ZFC'],
  e: 'Аксиома выбора независима от ZF и приводит к парадоксу Банаха-Тарского'
}, {
  q: 'Что такое "неизмеримое множество"?',
  a: 'Множество, которому нельзя присвоить меру Лебега',
  wrong: ['Очень большое множество', 'Конечное множество', 'Множество без элементов'],
  e: 'Существование неизмеримых множеств следует из аксиомы выбора'
}, {
  q: 'За пределами ZFC: пример независимого утверждения?',
  a: 'Гипотеза континуума',
  wrong: ['Теорема Пифагора', 'Закон исключённого третьего', '1+1=2'],
  e: 'ГК нельзя ни доказать, ни опровергнуть в рамках ZFC'
}, {
  q: 'Математика будущего: какое направление активно развивается?',
  a: 'Гомотопическая теория типов (HoTT)',
  wrong: ['Геометрия Евклида', 'Арифметика натуральных чисел', 'Школьная алгебра'],
  e: 'HoTT объединяет теорию типов, топологию и логику'
}];
const FACTS_XVI = [{
  q: 'Гипотеза Римана: статус в 2024 году?',
  a: 'Открытая проблема с 1859 года',
  wrong: ['Доказана в 2022', 'Опровергнута', 'Решена Перельманом'],
  e: 'Одна из 7 задач тысячелетия Clay Institute, $ 1 000 000'
}, {
  q: 'P ≠ NP: большинство математиков считают...?',
  a: 'P ≠ NP, но доказательства нет',
  wrong: ['P = NP', 'Проблема неразрешима', 'Зависит от компьютера'],
  e: 'Опросы показывают: ~83% специалистов уверены в P≠NP, но доказать не могут'
}, {
  q: 'Yang-Mills Mass Gap: почему это сложно?',
  a: 'Нет строгого математического фундамента для квантовой теории поля',
  wrong: ['Задача слишком лёгкая', 'Уже решена', 'Не имеет физического смысла'],
  e: 'Проблема требует построения математически строгой квантовой теории поля'
}, {
  q: 'BSD Conjecture: что означает "ранг эллиптической кривой"?',
  a: 'Размерность группы рациональных точек',
  wrong: ['Степень уравнения', 'Количество целых решений', 'Порядок симметрии'],
  e: 'E(ℚ)≅ℤʳ⊕T, r — ранг, T — конечная кручение-группа'
}, {
  q: 'Сколько задач тысячелетия ещё не решено?',
  a: '6',
  wrong: ['7', '5', '4'],
  e: 'Перельман решил гипотезу Пуанкаре в 2003. Остальные 6 открыты'
}, {
  q: 'Если ты докажешь гипотезу Римана, получишь...?',
  a: '$ 1 000 000 и Медаль Филдса (возможно)',
  wrong: ['Только признание', 'Ничего', 'Диплом'],
  e: 'Clay Math Institute предлагает 1 млн. Медаль Филдса — если претендент до 40 лет'
}];
const GENERATORS_ALL = {
  // I
  num_theory: gen_num_theory,
  arith_struct: gen_arith_struct,
  rational: gen_rational,
  power_trans: gen_power_trans,
  percent_models: gen_percent_models,
  num_patterns: gen_num_patterns,
  // II
  lin_eq2: gen_lin_eq2,
  lin_ineq2: gen_lin_ineq2,
  seq_prog: gen_seq_prog,
  polynomials: gen_polynomials,
  alg_trans: gen_alg_trans,
  factorization: gen_factorization,
  // III
  planimetry: gen_planimetry,
  geo_areas: n => areas ? areas(n) : gen_planimetry(n),
  similarity: gen_similarity,
  circles_arcs: gen_circles_arcs,
  coord_geo: gen_coord_geo,
  vectors: gen_vectors,
  // IV
  quadratic: gen_quadratic,
  func_graphs: gen_func_graphs,
  systems_eq: gen_systems_eq,
  parametric: gen_parametric,
  modular: gen_modular,
  extremal: gen_extremal,
  // V
  trig_ratios: gen_trig_ratios,
  trig_formulas: gen_trig_formulas,
  trig_eq: gen_trig_eq,
  trig_ineq: gen_trig_ineq,
  periodic: gen_periodic,
  harmonic: gen_harmonic,
  // VI
  seq_limits: gen_seq_limits,
  func_limits: gen_func_limits,
  derivative: gen_derivative,
  func_analysis: gen_func_analysis,
  integral: gen_integral,
  optimization: gen_optimization,
  // VII
  combinatorics: gen_combinatorics,
  graph_theory: gen_graph_theory,
  logic_systems: gen_logic_systems,
  recurrence: gen_recurrence,
  boolean_alg: gen_boolean_alg,
  algo_thinking: gen_algo_thinking,
  // VIII
  classic_prob: gen_classic_prob,
  comb_prob: gen_comb_prob,
  random_var: gen_random_var,
  expectation: gen_expectation,
  statistics: gen_statistics,
  decision_theory: gen_decision_theory,
  // IX–XVI (knowledge)
  invariants: n => knowledgeQuiz(FACTS_IX, n),
  pigeonhole: n => knowledgeQuiz(FACTS_IX, n),
  estimates: n => knowledgeQuiz(FACTS_IX, n),
  constructive: n => knowledgeQuiz(FACTS_IX, n),
  extremal_princ: n => knowledgeQuiz(FACTS_IX, n),
  nonstandard: n => knowledgeQuiz(FACTS_IX, n),
  lin_algebra: n => knowledgeQuiz(FACTS_X, n),
  analytic_geo: n => knowledgeQuiz(FACTS_X, n),
  num_theory_adv: n => knowledgeQuiz(FACTS_X, n),
  abstract_alg: n => knowledgeQuiz(FACTS_X, n),
  diff_eq: n => knowledgeQuiz(FACTS_X, n),
  complex_anal: n => knowledgeQuiz(FACTS_X, n),
  proof_theory: n => knowledgeQuiz(FACTS_XI, n),
  math_modeling: n => knowledgeQuiz(FACTS_XI, n),
  sys_optim: n => knowledgeQuiz(FACTS_XI, n),
  info_theory: n => knowledgeQuiz(FACTS_XI, n),
  stochastic: n => knowledgeQuiz(FACTS_XI, n),
  modern_math: n => knowledgeQuiz(FACTS_XI, n),
  knot_theory: n => knowledgeQuiz(FACTS_XII, n),
  elliptic_curves: n => knowledgeQuiz(FACTS_XII, n),
  automorphic: n => knowledgeQuiz(FACTS_XII, n),
  arith_geo: n => knowledgeQuiz(FACTS_XII, n),
  alg_varieties: n => knowledgeQuiz(FACTS_XII, n),
  motive_theory: n => knowledgeQuiz(FACTS_XII, n),
  riemann: n => knowledgeQuiz(FACTS_XIII, n),
  p_vs_np: n => knowledgeQuiz(FACTS_XIII, n),
  hodge: n => knowledgeQuiz(FACTS_XIII, n),
  navier_stokes: n => knowledgeQuiz(FACTS_XIII, n),
  yang_mills: n => knowledgeQuiz(FACTS_XIII, n),
  bsd: n => knowledgeQuiz(FACTS_XIII, n),
  prime_dist: n => knowledgeQuiz(FACTS_XIV, n),
  comp_complex: n => knowledgeQuiz(FACTS_XIV, n),
  quantum_field: n => knowledgeQuiz(FACTS_XIV, n),
  manifolds: n => knowledgeQuiz(FACTS_XIV, n),
  analytic_inv: n => knowledgeQuiz(FACTS_XIV, n),
  spacetime: n => knowledgeQuiz(FACTS_XIV, n),
  unknown_struct: n => knowledgeQuiz(FACTS_XV, n),
  beyond_axioms: n => knowledgeQuiz(FACTS_XV, n),
  new_objects: n => knowledgeQuiz(FACTS_XV, n),
  undiscovered: n => knowledgeQuiz(FACTS_XV, n),
  unknown_formal: n => knowledgeQuiz(FACTS_XV, n),
  math_boundary: n => knowledgeQuiz(FACTS_XV, n),
  sing_riemann: n => knowledgeQuiz(FACTS_XVI, n),
  sing_pnp: n => knowledgeQuiz(FACTS_XVI, n),
  sing_hodge2: n => knowledgeQuiz(FACTS_XVI, n),
  sing_ns: n => knowledgeQuiz(FACTS_XVI, n),
  sing_ym: n => knowledgeQuiz(FACTS_XVI, n),
  sing_bsd2: n => knowledgeQuiz(FACTS_XVI, n)
};
function generateQuestions(topicId, count = 5) {
  // old topics compatibility
  const OLD = {
    counting: gen_num_theory,
    arithmetic: gen_arith_struct,
    powers: gen_power_trans,
    fractions: gen_rational,
    percentages: gen_percent_models,
    linear_eq: gen_lin_eq2,
    inequalities: gen_lin_ineq2,
    progression: gen_seq_prog,
    areas: n => knowledgeQuiz([{
      q: 'S прямоугольника 5×8?',
      a: '40',
      wrong: ['13', '26', '42'],
      e: 'S=5×8=40'
    }, {
      q: 'S треугольника осн=6, h=4?',
      a: '12',
      wrong: ['24', '10', '6'],
      e: 'S=½×6×4=12'
    }, {
      q: 'S круга r=3 (π≈3.14)?',
      a: '28.3',
      wrong: ['18.8', '9.4', '56.5'],
      e: 'S=3.14×9=28.3'
    }, {
      q: 'S квадрата стороной 7?',
      a: '49',
      wrong: ['28', '14', '48'],
      e: 'S=7²=49'
    }, {
      q: 'S параллелограмма осн=5, h=6?',
      a: '30',
      wrong: ['11', '15', '60'],
      e: 'S=5×6=30'
    }], n)
  };
  const gen = GENERATORS_ALL[topicId] || OLD[topicId] || gen_num_theory;
  return gen(count);
}
function gradeFor(correct, total) {
  const r = correct / total;
  if (r >= 0.9) return 5;
  if (r >= 0.7) return 4;
  if (r >= 0.5) return 3;
  return 2;
}
Object.assign(window, {
  generateQuestions,
  gradeFor,
  GENERATORS_ALL
});
