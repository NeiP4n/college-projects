// app.jsx — MathTrainer: Alto-style UI + C# bridge
const {
  useState,
  useEffect,
  useRef
} = React;

// ── C# Bridge ─────────────────────────────────────────────────────
const send = (action, data = {}) => window.chrome.webview.postMessage(JSON.stringify({
  action,
  ...data
}));

// ── Local storage (streak only) ────────────────────────────────────
const store = {
  get(k, d) {
    try {
      return JSON.parse(localStorage.getItem('tm_' + k)) ?? d;
    } catch {
      return d;
    }
  },
  set(k, v) {
    try {
      localStorage.setItem('tm_' + k, JSON.stringify(v));
    } catch {}
  }
};
const GRADE_LABEL = {
  5: 'Отлично',
  4: 'Хорошо',
  3: 'Удовлетворительно',
  2: 'Стоит повторить'
};
const LEVELS = typeof LEVELS_ALL !== 'undefined' ? LEVELS_ALL : [{
  key: 'elementary',
  title: 'Начальная школа',
  sub: 'Базовый лагерь'
}, {
  key: 'middle',
  title: 'Средняя школа',
  sub: 'Высокогорье'
}];

// ── Login / Register ───────────────────────────────────────────────
function LoginScreen({
  onLogin
}) {
  const [reg, setReg] = useState(false);
  const [u, setU] = useState('');
  const [p, setP] = useState('');
  const [c, setC] = useState('');
  const [err, setErr] = useState('');
  const [loading, setLoading] = useState(false);
  useEffect(() => {
    const h = e => {
      const {
        type,
        data
      } = JSON.parse(e.data);
      if (type === 'loginSuccess') onLogin(data.user);
      if (type === 'loginError') {
        setErr(data.message);
        setLoading(false);
      }
    };
    window.chrome.webview.addEventListener('message', h);
    return () => window.chrome.webview.removeEventListener('message', h);
  }, [onLogin]);
  const submit = () => {
    if (!u.trim() || !p) {
      setErr('Заполните все поля');
      return;
    }
    if (reg && p !== c) {
      setErr('Пароли не совпадают');
      return;
    }
    setErr('');
    setLoading(true);
    send(reg ? 'register' : 'login', {
      username: u.trim(),
      password: p
    });
  };
  const onKey = e => {
    if (e.key === 'Enter') submit();
  };
  return /*#__PURE__*/React.createElement("div", {
    className: "login-wrap"
  }, /*#__PURE__*/React.createElement("div", {
    className: "login-card reveal"
  }, /*#__PURE__*/React.createElement("div", {
    className: "login-logo"
  }, /*#__PURE__*/React.createElement("span", {
    className: "mark"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "summit",
    size: 24,
    stroke: 1.8
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h1", null, "\u0412\u0435\u0440\u0448\u0438\u043D\u0430"), /*#__PURE__*/React.createElement("p", null, "\u0442\u0440\u0435\u043D\u0430\u0436\u0451\u0440 \u043F\u043E \u043C\u0430\u0442\u0435\u043C\u0430\u0442\u0438\u043A\u0435"))), /*#__PURE__*/React.createElement("div", {
    className: "lfield"
  }, /*#__PURE__*/React.createElement("label", null, "\u041B\u043E\u0433\u0438\u043D"), /*#__PURE__*/React.createElement("input", {
    value: u,
    onChange: e => setU(e.target.value),
    onKeyDown: onKey,
    placeholder: "\u0418\u043C\u044F \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u044F",
    autoComplete: "off"
  })), /*#__PURE__*/React.createElement("div", {
    className: "lfield"
  }, /*#__PURE__*/React.createElement("label", null, "\u041F\u0430\u0440\u043E\u043B\u044C"), /*#__PURE__*/React.createElement("input", {
    type: "password",
    value: p,
    onChange: e => setP(e.target.value),
    onKeyDown: onKey,
    placeholder: "\u041F\u0430\u0440\u043E\u043B\u044C"
  })), reg && /*#__PURE__*/React.createElement("div", {
    className: "lfield"
  }, /*#__PURE__*/React.createElement("label", null, "\u041F\u043E\u0434\u0442\u0432\u0435\u0440\u0434\u0438\u0442\u0435 \u043F\u0430\u0440\u043E\u043B\u044C"), /*#__PURE__*/React.createElement("input", {
    type: "password",
    value: c,
    onChange: e => setC(e.target.value),
    onKeyDown: onKey,
    placeholder: "\u041F\u043E\u0432\u0442\u043E\u0440\u0438\u0442\u0435 \u043F\u0430\u0440\u043E\u043B\u044C"
  })), /*#__PURE__*/React.createElement("div", {
    className: "lerr"
  }, err && '⚠ ' + err), /*#__PURE__*/React.createElement("button", {
    className: "lbtn",
    onClick: submit,
    disabled: loading
  }, loading ? '...' : reg ? 'Создать аккаунт' : 'Войти'), /*#__PURE__*/React.createElement("button", {
    className: "ltoggle",
    onClick: () => {
      setReg(!reg);
      setErr('');
    }
  }, reg ? 'Уже есть аккаунт? Войти' : 'Нет аккаунта? Зарегистрироваться')));
}

// ── Topic card ─────────────────────────────────────────────────────
function TopicCard({
  topic,
  best,
  onStart,
  onTheory,
  delay
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "card reveal",
    style: {
      animationDelay: delay + 'ms'
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "card-ridge",
    "aria-hidden": "true"
  }), /*#__PURE__*/React.createElement("div", {
    className: "card-top"
  }, /*#__PURE__*/React.createElement("span", {
    className: "card-icon"
  }, /*#__PURE__*/React.createElement(TopicIcon, {
    id: topic.id,
    size: 28
  })), best ? /*#__PURE__*/React.createElement("span", {
    className: 'grade-chip g' + best
  }, best) : /*#__PURE__*/React.createElement("span", {
    className: "new-chip"
  }, "\u043D\u043E\u0432\u044B\u0439")), /*#__PURE__*/React.createElement("div", {
    className: "card-name"
  }, topic.name), /*#__PURE__*/React.createElement("div", {
    className: "card-desc"
  }, topic.desc), /*#__PURE__*/React.createElement("div", {
    className: "card-actions"
  }, /*#__PURE__*/React.createElement("button", {
    className: "card-theory-btn",
    onClick: () => onTheory(topic)
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "bulb",
    size: 13,
    stroke: 2
  }), /*#__PURE__*/React.createElement("span", null, "\u0422\u0435\u043E\u0440\u0438\u044F")), /*#__PURE__*/React.createElement("button", {
    className: "card-go",
    style: {
      background: 'none',
      border: 'none',
      cursor: 'pointer',
      padding: 0
    },
    onClick: () => onStart(topic)
  }, /*#__PURE__*/React.createElement("span", null, "\u041D\u0430\u0447\u0430\u0442\u044C"), /*#__PURE__*/React.createElement(UIcon, {
    name: "arrow",
    size: 16,
    stroke: 2
  }))));
}

// ── Dashboard ──────────────────────────────────────────────────────
function Dashboard({
  user,
  bests,
  streak,
  onStart,
  onTheory,
  onStats
}) {
  const done = Object.keys(bests).length;
  const avg = done ? Object.values(bests).reduce((a, b) => a + b, 0) / done : 0;
  const hour = new Date().getHours();
  const greet = hour < 5 ? 'Доброй ночи' : hour < 12 ? 'Доброе утро' : hour < 18 ? 'Добрый день' : 'Добрый вечер';
  const initials = user?.username?.[0]?.toUpperCase() ?? 'У';
  return /*#__PURE__*/React.createElement("div", {
    className: "dash"
  }, /*#__PURE__*/React.createElement("header", {
    className: "topbar"
  }, /*#__PURE__*/React.createElement("div", {
    className: "brand"
  }, /*#__PURE__*/React.createElement("span", {
    className: "brand-mark"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "summit",
    size: 22,
    stroke: 1.9
  })), /*#__PURE__*/React.createElement("div", {
    className: "brand-text"
  }, /*#__PURE__*/React.createElement("strong", null, "\u0412\u0435\u0440\u0448\u0438\u043D\u0430"), /*#__PURE__*/React.createElement("span", null, "\u0442\u0440\u0435\u043D\u0430\u0436\u0451\u0440 \u043F\u043E \u043C\u0430\u0442\u0435\u043C\u0430\u0442\u0438\u043A\u0435"))), /*#__PURE__*/React.createElement("div", {
    className: "topbar-right"
  }, /*#__PURE__*/React.createElement("div", {
    className: "streak",
    title: "\u0414\u043D\u0435\u0439 \u043F\u043E\u0434\u0440\u044F\u0434"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "flame",
    size: 16,
    stroke: 1.9
  }), /*#__PURE__*/React.createElement("span", null, streak)), onStats && /*#__PURE__*/React.createElement("button", {
    className: "icon-btn",
    onClick: onStats,
    title: "\u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430",
    style: {
      width: 36,
      height: 36,
      fontSize: 18
    }
  }, "\uD83D\uDCCA"), /*#__PURE__*/React.createElement("div", {
    className: "avatar",
    title: user?.username
  }, initials))), /*#__PURE__*/React.createElement("section", {
    className: "hero reveal"
  }, /*#__PURE__*/React.createElement("p", {
    className: "hero-eyebrow"
  }, greet, ", ", user?.username ?? 'восходитель'), /*#__PURE__*/React.createElement("h1", {
    className: "hero-title"
  }, "\u0412\u044B\u0431\u0435\u0440\u0438 \u043C\u0430\u0440\u0448\u0440\u0443\u0442 \u043A\xA0\u0432\u0435\u0440\u0448\u0438\u043D\u0435"), /*#__PURE__*/React.createElement("div", {
    className: "hero-stats"
  }, /*#__PURE__*/React.createElement("div", {
    className: "stat"
  }, /*#__PURE__*/React.createElement("b", null, done, /*#__PURE__*/React.createElement("span", null, "/9")), /*#__PURE__*/React.createElement("label", null, "\u043C\u0430\u0440\u0448\u0440\u0443\u0442\u043E\u0432 \u043F\u0440\u043E\u0439\u0434\u0435\u043D\u043E")), /*#__PURE__*/React.createElement("div", {
    className: "divider"
  }), /*#__PURE__*/React.createElement("div", {
    className: "stat"
  }, /*#__PURE__*/React.createElement("b", null, avg ? avg.toFixed(1) : '—'), /*#__PURE__*/React.createElement("label", null, "\u0441\u0440\u0435\u0434\u043D\u0438\u0439 \u0431\u0430\u043B\u043B")), /*#__PURE__*/React.createElement("div", {
    className: "divider"
  }), /*#__PURE__*/React.createElement("div", {
    className: "stat"
  }, /*#__PURE__*/React.createElement("b", null, streak), /*#__PURE__*/React.createElement("label", null, "\u0434\u043D\u0435\u0439 \u043F\u043E\u0434\u0440\u044F\u0434")))), LEVELS.map(lv => {
    const topics = TOPICS.filter(t => t.level === lv.key);
    return /*#__PURE__*/React.createElement("section", {
      className: "level",
      key: lv.key
    }, /*#__PURE__*/React.createElement("div", {
      className: "level-head"
    }, /*#__PURE__*/React.createElement("h2", null, lv.title), /*#__PURE__*/React.createElement("span", null, lv.sub)), /*#__PURE__*/React.createElement("div", {
      className: "grid"
    }, topics.map((t, i) => /*#__PURE__*/React.createElement(TopicCard, {
      key: t.id,
      topic: t,
      best: bests[t.id],
      onStart: onStart,
      onTheory: onTheory,
      delay: i * 70
    }))));
  }));
}

// ── Quiz ───────────────────────────────────────────────────────────
function QuizHost({
  topic,
  onFinish,
  onExit
}) {
  const [questions] = useState(() => generateQuestions(topic.id, 5));
  return /*#__PURE__*/React.createElement(QuizInner, {
    topic: topic,
    questions: questions,
    onFinish: onFinish,
    onExit: onExit
  });
}
function QuizInner({
  topic,
  questions,
  onFinish,
  onExit
}) {
  const [idx, setIdx] = useState(0);
  const [picked, setPicked] = useState(null);
  const [answers, setAnswers] = useState([]);
  const [showExpl, setShowExpl] = useState(false);
  const q = questions[idx];
  const locked = picked !== null;
  const choose = i => {
    if (locked) return;
    setPicked(i);
    setAnswers(a => [...a, {
      picked: i,
      correct: i === q.correctIndex
    }]);
  };
  const next = () => {
    const finalAnswers = answers; // already updated by choose
    if (idx + 1 >= questions.length) {
      onFinish(questions, finalAnswers);
    } else {
      setIdx(idx + 1);
      setPicked(null);
      setShowExpl(false);
    }
  };
  return /*#__PURE__*/React.createElement("div", {
    className: "quiz"
  }, /*#__PURE__*/React.createElement("div", {
    className: "quiz-bar"
  }, /*#__PURE__*/React.createElement("button", {
    className: "icon-btn",
    onClick: onExit,
    "aria-label": "\u0412\u044B\u0439\u0442\u0438"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "close",
    size: 18
  })), /*#__PURE__*/React.createElement("div", {
    className: "quiz-topic"
  }, /*#__PURE__*/React.createElement(TopicIcon, {
    id: topic.id,
    size: 18
  }), /*#__PURE__*/React.createElement("span", null, topic.name)), /*#__PURE__*/React.createElement("div", {
    className: "dots"
  }, questions.map((_, i) => /*#__PURE__*/React.createElement("span", {
    key: i,
    className: 'dot' + (i < idx ? ' past' : i === idx ? ' now' : '')
  })))), /*#__PURE__*/React.createElement("div", {
    className: "quiz-card reveal",
    key: idx
  }, /*#__PURE__*/React.createElement("div", {
    className: "q-count"
  }, "\u0417\u0430\u0434\u0430\u0447\u0430 ", idx + 1, " \u0438\u0437 ", questions.length), /*#__PURE__*/React.createElement("div", {
    className: "q-text"
  }, q.text), q.graph && window.Graph && /*#__PURE__*/React.createElement("div", {
    className: "q-graph"
  }, /*#__PURE__*/React.createElement(Graph, {
    fns: q.graph.fns || [],
    labels: q.graph.labels || [],
    xMin: q.graph.xMin ?? -6,
    xMax: q.graph.xMax ?? 6,
    yMin: q.graph.yMin ?? -4,
    yMax: q.graph.yMax ?? 4,
    width: 320,
    height: 200
  })), q.formula && window.KatexInline && /*#__PURE__*/React.createElement("div", {
    className: "q-formula"
  }, /*#__PURE__*/React.createElement(KatexInline, {
    expr: q.formula,
    display: true
  })), /*#__PURE__*/React.createElement("div", {
    className: "opts"
  }, q.options.map((opt, i) => {
    let cls = 'opt';
    if (locked) {
      if (i === q.correctIndex) cls += ' correct';else if (i === picked) cls += ' wrong';else cls += ' dim';
    }
    return /*#__PURE__*/React.createElement("button", {
      key: i,
      className: cls,
      onClick: () => choose(i),
      disabled: locked
    }, /*#__PURE__*/React.createElement("span", {
      className: "opt-key"
    }, ['А', 'Б', 'В', 'Г'][i]), /*#__PURE__*/React.createElement("span", {
      className: "opt-val"
    }, opt), locked && i === q.correctIndex && /*#__PURE__*/React.createElement("span", {
      className: "opt-mark ok"
    }, /*#__PURE__*/React.createElement(UIcon, {
      name: "check",
      size: 16,
      stroke: 2.4
    })), locked && i === picked && i !== q.correctIndex && /*#__PURE__*/React.createElement("span", {
      className: "opt-mark no"
    }, /*#__PURE__*/React.createElement(UIcon, {
      name: "cross",
      size: 15,
      stroke: 2.4
    })));
  })), locked && /*#__PURE__*/React.createElement("div", {
    className: "expl-zone reveal"
  }, /*#__PURE__*/React.createElement("button", {
    className: "expl-toggle",
    onClick: () => setShowExpl(s => !s)
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "bulb",
    size: 16
  }), /*#__PURE__*/React.createElement("span", null, showExpl ? 'Скрыть решение' : 'Показать решение')), showExpl && /*#__PURE__*/React.createElement("div", {
    className: "expl"
  }, q.expl), /*#__PURE__*/React.createElement("button", {
    className: "next-btn",
    onClick: next
  }, /*#__PURE__*/React.createElement("span", null, idx + 1 >= questions.length ? 'К вершине' : 'Далее'), /*#__PURE__*/React.createElement(UIcon, {
    name: "arrow",
    size: 18,
    stroke: 2.2
  })))));
}

// ── Results ────────────────────────────────────────────────────────
function Results({
  topic,
  questions,
  answers,
  grade,
  onRetry,
  onExit
}) {
  const correct = answers.filter(a => a.correct).length;
  return /*#__PURE__*/React.createElement("div", {
    className: "quiz"
  }, /*#__PURE__*/React.createElement("div", {
    className: "quiz-bar"
  }, /*#__PURE__*/React.createElement("button", {
    className: "icon-btn",
    onClick: onExit,
    "aria-label": "\u041A \u043C\u0430\u0440\u0448\u0440\u0443\u0442\u0430\u043C"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "back",
    size: 18
  })), /*#__PURE__*/React.createElement("div", {
    className: "quiz-topic"
  }, /*#__PURE__*/React.createElement(TopicIcon, {
    id: topic.id,
    size: 18
  }), /*#__PURE__*/React.createElement("span", null, topic.name)), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 36
    }
  })), /*#__PURE__*/React.createElement("div", {
    className: "result-card reveal"
  }, /*#__PURE__*/React.createElement("div", {
    className: "summit-line"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "summit",
    size: 26
  })), /*#__PURE__*/React.createElement("div", {
    className: "result-eyebrow"
  }, grade >= 3 ? 'Вершина достигнута' : 'Спуск к лагерю'), /*#__PURE__*/React.createElement("div", {
    className: 'big-grade g' + grade
  }, grade), /*#__PURE__*/React.createElement("div", {
    className: "grade-word"
  }, GRADE_LABEL[grade]), /*#__PURE__*/React.createElement("div", {
    className: "score-line"
  }, correct, " \u0438\u0437 ", questions.length, " \u0432\u0435\u0440\u043D\u043E"), /*#__PURE__*/React.createElement("div", {
    className: "recap"
  }, questions.map((q, i) => {
    const a = answers[i];
    return /*#__PURE__*/React.createElement("div", {
      className: 'recap-row ' + (a.correct ? 'ok' : 'no'),
      key: i
    }, /*#__PURE__*/React.createElement("span", {
      className: "recap-ic"
    }, a.correct ? /*#__PURE__*/React.createElement(UIcon, {
      name: "check",
      size: 14,
      stroke: 2.6
    }) : /*#__PURE__*/React.createElement(UIcon, {
      name: "cross",
      size: 13,
      stroke: 2.6
    })), /*#__PURE__*/React.createElement("span", {
      className: "recap-q"
    }, q.text), /*#__PURE__*/React.createElement("span", {
      className: "recap-ans"
    }, a.correct ? q.options[q.correctIndex] : /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("s", null, q.options[a.picked]), " \u2192 ", q.options[q.correctIndex])));
  })), /*#__PURE__*/React.createElement("div", {
    className: "result-actions"
  }, /*#__PURE__*/React.createElement("button", {
    className: "btn-ghost",
    onClick: onExit
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "back",
    size: 16
  }), /*#__PURE__*/React.createElement("span", null, "\u041A \u043C\u0430\u0440\u0448\u0440\u0443\u0442\u0430\u043C")), /*#__PURE__*/React.createElement("button", {
    className: "btn-solid",
    onClick: onRetry
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "replay",
    size: 16
  }), /*#__PURE__*/React.createElement("span", null, "\u0415\u0449\u0451 \u0440\u0430\u0437")))));
}

// ── Root ───────────────────────────────────────────────────────────
function MathApp({
  tweaks
}) {
  const [screen, setScreen] = useState('login');
  const [user, setUser] = useState(null);
  const [bests, setBests] = useState({});
  const [topic, setTopic] = useState(null);
  const [session, setSession] = useState(null);
  const [theoryTopic, setTheoryTopic] = useState(null);
  const [streak] = useState(() => {
    const today = new Date().toDateString();
    const last = store.get('lastDay', null);
    let s = store.get('streak', 1);
    if (last !== today) {
      const yesterday = new Date(Date.now() - 864e5).toDateString();
      s = last === yesterday ? s + 1 : 1;
      store.set('streak', s);
      store.set('lastDay', today);
    }
    return s;
  });

  // Listen for bests response
  useEffect(() => {
    const h = e => {
      const {
        type,
        data
      } = JSON.parse(e.data);
      if (type === 'bests') setBests(data.bests ?? {});
    };
    window.chrome.webview.addEventListener('message', h);
    return () => window.chrome.webview.removeEventListener('message', h);
  }, []);
  const onLogin = u => {
    setUser(u);
    setScreen('dashboard');
    send('getBests');
  };
  const start = t => {
    setTopic(t);
    setSession(null);
    setScreen('quiz');
  };
  const openTheory = t => {
    setTheoryTopic(t);
    setScreen('theory');
  };
  const finish = (questions, answers) => {
    const correct = answers.filter(a => a.correct).length;
    const grade = gradeFor(correct, questions.length);
    setSession({
      questions,
      answers,
      grade
    });

    // Save to C# DB
    send('saveResult', {
      topicId: topic.id,
      topicName: topic.name,
      correctCount: correct,
      totalCount: questions.length,
      answers: answers.map((a, i) => ({
        questionText: questions[i].text,
        userAnswer: questions[i].options[a.picked],
        correctAnswer: questions[i].options[questions[i].correctIndex],
        isCorrect: a.correct,
        explanation: questions[i].expl
      }))
    });

    // Update local bests immediately
    setBests(prev => {
      const nb = {
        ...prev
      };
      if (nb[topic.id] == null || grade > nb[topic.id]) nb[topic.id] = grade;
      return nb;
    });
    setScreen('results');
  };
  const timeMode = tweaks.timeMode ?? 'auto';
  return /*#__PURE__*/React.createElement("div", {
    className: "app",
    style: {
      '--accent': tweaks.accent,
      '--accent-2': tweaks.accent2
    }
  }, /*#__PURE__*/React.createElement(Scene, {
    timeMode: timeMode,
    cycleSeconds: tweaks.cycleSeconds,
    motion: tweaks.motion,
    snow: tweaks.snow
  }), /*#__PURE__*/React.createElement("div", {
    className: "scene-scrim"
  }), /*#__PURE__*/React.createElement("main", {
    className: "content"
  }, screen === 'login' && /*#__PURE__*/React.createElement(LoginScreen, {
    onLogin: onLogin
  }), screen === 'dashboard' && /*#__PURE__*/React.createElement(Dashboard, {
    user: user,
    bests: bests,
    streak: streak,
    onStart: start,
    onTheory: openTheory,
    onStats: () => setScreen('stats')
  }), screen === 'theory' && theoryTopic && /*#__PURE__*/React.createElement(TheoryScreen, {
    topic: theoryTopic,
    onStartQuiz: () => start(theoryTopic),
    onBack: () => setScreen('dashboard')
  }), screen === 'quiz' && topic && /*#__PURE__*/React.createElement(QuizHost, {
    topic: topic,
    onFinish: finish,
    onExit: () => setScreen('dashboard')
  }), screen === 'results' && session && /*#__PURE__*/React.createElement(Results, {
    topic: topic,
    questions: session.questions,
    answers: session.answers,
    grade: session.grade,
    onRetry: () => start(topic),
    onExit: () => setScreen('dashboard')
  }), screen === 'stats' && window.StatsScreen && /*#__PURE__*/React.createElement(StatsScreen, {
    onBack: () => setScreen('dashboard')
  })));
}
window.MathApp = MathApp;
