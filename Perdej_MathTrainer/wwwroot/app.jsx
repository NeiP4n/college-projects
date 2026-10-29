// app.jsx — MathTrainer: Alto-style UI + C# bridge
const { useState, useEffect, useRef } = React;

// ── C# Bridge ─────────────────────────────────────────────────────
const send = (action, data = {}) =>
  window.chrome.webview.postMessage(JSON.stringify({ action, ...data }));

// ── Local storage (streak only) ────────────────────────────────────
const store = {
  get(k, d) { try { return JSON.parse(localStorage.getItem('tm_' + k)) ?? d; } catch { return d; } },
  set(k, v) { try { localStorage.setItem('tm_' + k, JSON.stringify(v)); } catch {} },
};

const GRADE_LABEL = { 5: 'Отлично', 4: 'Хорошо', 3: 'Удовлетворительно', 2: 'Стоит повторить' };
const LEVELS = typeof LEVELS_ALL !== 'undefined' ? LEVELS_ALL : [
  { key: 'elementary', title: 'Начальная школа', sub: 'Базовый лагерь' },
  { key: 'middle',     title: 'Средняя школа',   sub: 'Высокогорье' },
];

// ── Login / Register ───────────────────────────────────────────────
function LoginScreen({ onLogin }) {
  const [reg,     setReg]     = useState(false);
  const [u,       setU]       = useState('');
  const [p,       setP]       = useState('');
  const [c,       setC]       = useState('');
  const [err,     setErr]     = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const h = e => {
      const { type, data } = JSON.parse(e.data);
      if (type === 'loginSuccess') onLogin(data.user);
      if (type === 'loginError')   { setErr(data.message); setLoading(false); }
    };
    window.chrome.webview.addEventListener('message', h);
    return () => window.chrome.webview.removeEventListener('message', h);
  }, [onLogin]);

  const submit = () => {
    if (!u.trim() || !p)        { setErr('Заполните все поля');   return; }
    if (reg && p !== c)         { setErr('Пароли не совпадают');  return; }
    setErr(''); setLoading(true);
    send(reg ? 'register' : 'login', { username: u.trim(), password: p });
  };
  const onKey = e => { if (e.key === 'Enter') submit(); };

  return (
    <div className="login-wrap">
      <div className="login-card reveal">
        <div className="login-logo">
          <span className="mark"><UIcon name="summit" size={24} stroke={1.8} /></span>
          <div><h1>Вершина</h1><p>тренажёр по математике</p></div>
        </div>

        <div className="lfield">
          <label>Логин</label>
          <input value={u} onChange={e => setU(e.target.value)} onKeyDown={onKey}
                 placeholder="Имя пользователя" autoComplete="off" />
        </div>
        <div className="lfield">
          <label>Пароль</label>
          <input type="password" value={p} onChange={e => setP(e.target.value)} onKeyDown={onKey}
                 placeholder="Пароль" />
        </div>
        {reg && (
          <div className="lfield">
            <label>Подтвердите пароль</label>
            <input type="password" value={c} onChange={e => setC(e.target.value)} onKeyDown={onKey}
                   placeholder="Повторите пароль" />
          </div>
        )}

        <div className="lerr">{err && '⚠ ' + err}</div>

        <button className="lbtn" onClick={submit} disabled={loading}>
          {loading ? '...' : reg ? 'Создать аккаунт' : 'Войти'}
        </button>
        <button className="ltoggle" onClick={() => { setReg(!reg); setErr(''); }}>
          {reg ? 'Уже есть аккаунт? Войти' : 'Нет аккаунта? Зарегистрироваться'}
        </button>
      </div>
    </div>
  );
}

// ── Topic card ─────────────────────────────────────────────────────
function TopicCard({ topic, best, onStart, onTheory, delay }) {
  return (
    <div className="card reveal" style={{ animationDelay: delay + 'ms' }}>
      <div className="card-ridge" aria-hidden="true" />
      <div className="card-top">
        <span className="card-icon"><TopicIcon id={topic.id} size={28} /></span>
        {best ? <span className={'grade-chip g' + best}>{best}</span>
               : <span className="new-chip">новый</span>}
      </div>
      <div className="card-name">{topic.name}</div>
      <div className="card-desc">{topic.desc}</div>
      <div className="card-actions">
        <button className="card-theory-btn" onClick={() => onTheory(topic)}>
          <UIcon name="bulb" size={13} stroke={2} /><span>Теория</span>
        </button>
        <button className="card-go" style={{background:'none',border:'none',cursor:'pointer',padding:0}}
                onClick={() => onStart(topic)}>
          <span>Начать</span><UIcon name="arrow" size={16} stroke={2} />
        </button>
      </div>
    </div>
  );
}

// ── Dashboard ──────────────────────────────────────────────────────
function Dashboard({ user, bests, streak, onStart, onTheory, onStats }) {
  const done = Object.keys(bests).length;
  const avg  = done ? Object.values(bests).reduce((a, b) => a + b, 0) / done : 0;
  const hour = new Date().getHours();
  const greet = hour < 5 ? 'Доброй ночи' : hour < 12 ? 'Доброе утро' : hour < 18 ? 'Добрый день' : 'Добрый вечер';
  const initials = user?.username?.[0]?.toUpperCase() ?? 'У';

  return (
    <div className="dash">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark"><UIcon name="summit" size={22} stroke={1.9} /></span>
          <div className="brand-text">
            <strong>Вершина</strong>
            <span>тренажёр по математике</span>
          </div>
        </div>
        <div className="topbar-right">
          <div className="streak" title="Дней подряд">
            <UIcon name="flame" size={16} stroke={1.9} /><span>{streak}</span>
          </div>
          {onStats && <button className="icon-btn" onClick={onStats} title="Статистика"
            style={{width:36,height:36,fontSize:18}}>📊</button>}
          <div className="avatar" title={user?.username}>{initials}</div>
        </div>
      </header>

      <section className="hero reveal">
        <p className="hero-eyebrow">{greet}, {user?.username ?? 'восходитель'}</p>
        <h1 className="hero-title">Выбери маршрут к&nbsp;вершине</h1>
        <div className="hero-stats">
          <div className="stat"><b>{done}<span>/9</span></b><label>маршрутов пройдено</label></div>
          <div className="divider" />
          <div className="stat"><b>{avg ? avg.toFixed(1) : '—'}</b><label>средний балл</label></div>
          <div className="divider" />
          <div className="stat"><b>{streak}</b><label>дней подряд</label></div>
        </div>
      </section>

      {LEVELS.map(lv => {
        const topics = TOPICS.filter(t => t.level === lv.key);
        return (
          <section className="level" key={lv.key}>
            <div className="level-head"><h2>{lv.title}</h2><span>{lv.sub}</span></div>
            <div className="grid">
              {topics.map((t, i) => (
                <TopicCard key={t.id} topic={t} best={bests[t.id]}
                           onStart={onStart} onTheory={onTheory} delay={i * 70} />
              ))}
            </div>
          </section>
        );
      })}

    </div>
  );
}

// ── Quiz ───────────────────────────────────────────────────────────
function QuizHost({ topic, onFinish, onExit }) {
  const [questions] = useState(() => generateQuestions(topic.id, 5));
  return <QuizInner topic={topic} questions={questions} onFinish={onFinish} onExit={onExit} />;
}

function QuizInner({ topic, questions, onFinish, onExit }) {
  const [idx,      setIdx]      = useState(0);
  const [picked,   setPicked]   = useState(null);
  const [answers,  setAnswers]  = useState([]);
  const [showExpl, setShowExpl] = useState(false);
  const q      = questions[idx];
  const locked = picked !== null;

  const choose = (i) => {
    if (locked) return;
    setPicked(i);
    setAnswers(a => [...a, { picked: i, correct: i === q.correctIndex }]);
  };

  const next = () => {
    const finalAnswers = answers; // already updated by choose
    if (idx + 1 >= questions.length) {
      onFinish(questions, finalAnswers);
    } else {
      setIdx(idx + 1); setPicked(null); setShowExpl(false);
    }
  };

  return (
    <div className="quiz">
      <div className="quiz-bar">
        <button className="icon-btn" onClick={onExit} aria-label="Выйти">
          <UIcon name="close" size={18} />
        </button>
        <div className="quiz-topic"><TopicIcon id={topic.id} size={18} /><span>{topic.name}</span></div>
        <div className="dots">
          {questions.map((_, i) => (
            <span key={i} className={'dot' + (i < idx ? ' past' : i === idx ? ' now' : '')} />
          ))}
        </div>
      </div>

      <div className="quiz-card reveal" key={idx}>
        <div className="q-count">Задача {idx + 1} из {questions.length}</div>
        <div className="q-text">{q.text}</div>
        {q.graph && window.Graph && <div className="q-graph">
          <Graph fns={q.graph.fns||[]} labels={q.graph.labels||[]}
            xMin={q.graph.xMin??-6} xMax={q.graph.xMax??6}
            yMin={q.graph.yMin??-4} yMax={q.graph.yMax??4}
            width={320} height={200} />
        </div>}
        {q.formula && window.KatexInline && <div className="q-formula">
          <KatexInline expr={q.formula} display={true} />
        </div>}
        <div className="opts">
          {q.options.map((opt, i) => {
            let cls = 'opt';
            if (locked) {
              if (i === q.correctIndex) cls += ' correct';
              else if (i === picked)   cls += ' wrong';
              else                     cls += ' dim';
            }
            return (
              <button key={i} className={cls} onClick={() => choose(i)} disabled={locked}>
                <span className="opt-key">{['А','Б','В','Г'][i]}</span>
                <span className="opt-val">{opt}</span>
                {locked && i === q.correctIndex && <span className="opt-mark ok"><UIcon name="check" size={16} stroke={2.4} /></span>}
                {locked && i === picked && i !== q.correctIndex && <span className="opt-mark no"><UIcon name="cross" size={15} stroke={2.4} /></span>}
              </button>
            );
          })}
        </div>
        {locked && (
          <div className="expl-zone reveal">
            <button className="expl-toggle" onClick={() => setShowExpl(s => !s)}>
              <UIcon name="bulb" size={16} />
              <span>{showExpl ? 'Скрыть решение' : 'Показать решение'}</span>
            </button>
            {showExpl && <div className="expl">{q.expl}</div>}
            <button className="next-btn" onClick={next}>
              <span>{idx + 1 >= questions.length ? 'К вершине' : 'Далее'}</span>
              <UIcon name="arrow" size={18} stroke={2.2} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ── Results ────────────────────────────────────────────────────────
function Results({ topic, questions, answers, grade, onRetry, onExit }) {
  const correct  = answers.filter(a => a.correct).length;
  return (
    <div className="quiz">
      <div className="quiz-bar">
        <button className="icon-btn" onClick={onExit} aria-label="К маршрутам">
          <UIcon name="back" size={18} />
        </button>
        <div className="quiz-topic"><TopicIcon id={topic.id} size={18} /><span>{topic.name}</span></div>
        <div style={{ width: 36 }} />
      </div>

      <div className="result-card reveal">
        <div className="summit-line"><UIcon name="summit" size={26} /></div>
        <div className="result-eyebrow">{grade >= 3 ? 'Вершина достигнута' : 'Спуск к лагерю'}</div>
        <div className={'big-grade g' + grade}>{grade}</div>
        <div className="grade-word">{GRADE_LABEL[grade]}</div>
        <div className="score-line">{correct} из {questions.length} верно</div>

        <div className="recap">
          {questions.map((q, i) => {
            const a = answers[i];
            return (
              <div className={'recap-row ' + (a.correct ? 'ok' : 'no')} key={i}>
                <span className="recap-ic">
                  {a.correct
                    ? <UIcon name="check" size={14} stroke={2.6} />
                    : <UIcon name="cross" size={13} stroke={2.6} />}
                </span>
                <span className="recap-q">{q.text}</span>
                <span className="recap-ans">
                  {a.correct
                    ? q.options[q.correctIndex]
                    : <><s>{q.options[a.picked]}</s> → {q.options[q.correctIndex]}</>}
                </span>
              </div>
            );
          })}
        </div>

        <div className="result-actions">
          <button className="btn-ghost" onClick={onExit}>
            <UIcon name="back" size={16} /><span>К маршрутам</span>
          </button>
          <button className="btn-solid" onClick={onRetry}>
            <UIcon name="replay" size={16} /><span>Ещё раз</span>
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Root ───────────────────────────────────────────────────────────
function MathApp({ tweaks }) {
  const [screen,  setScreen]  = useState('login');
  const [user,    setUser]    = useState(null);
  const [bests,   setBests]   = useState({});
  const [topic,   setTopic]   = useState(null);
  const [session, setSession] = useState(null);
  const [theoryTopic, setTheoryTopic] = useState(null);

  const [streak] = useState(() => {
    const today = new Date().toDateString();
    const last  = store.get('lastDay', null);
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
      const { type, data } = JSON.parse(e.data);
      if (type === 'bests') setBests(data.bests ?? {});
    };
    window.chrome.webview.addEventListener('message', h);
    return () => window.chrome.webview.removeEventListener('message', h);
  }, []);

  const onLogin = (u) => {
    setUser(u);
    setScreen('dashboard');
    send('getBests');
  };

  const start = (t) => {
    setTopic(t); setSession(null); setScreen('quiz');
  };

  const openTheory = (t) => {
    setTheoryTopic(t); setScreen('theory');
  };

  const finish = (questions, answers) => {
    const correct = answers.filter(a => a.correct).length;
    const grade   = gradeFor(correct, questions.length);
    setSession({ questions, answers, grade });

    // Save to C# DB
    send('saveResult', {
      topicId:      topic.id,
      topicName:    topic.name,
      correctCount: correct,
      totalCount:   questions.length,
      answers: answers.map((a, i) => ({
        questionText:  questions[i].text,
        userAnswer:    questions[i].options[a.picked],
        correctAnswer: questions[i].options[questions[i].correctIndex],
        isCorrect:     a.correct,
        explanation:   questions[i].expl,
      })),
    });

    // Update local bests immediately
    setBests(prev => {
      const nb = { ...prev };
      if (nb[topic.id] == null || grade > nb[topic.id]) nb[topic.id] = grade;
      return nb;
    });

    setScreen('results');
  };

  const timeMode = tweaks.timeMode ?? 'auto';

  return (
    <div className="app" style={{ '--accent': tweaks.accent, '--accent-2': tweaks.accent2 }}>
      <Scene timeMode={timeMode} cycleSeconds={tweaks.cycleSeconds} motion={tweaks.motion} snow={tweaks.snow} />
      <div className="scene-scrim" />
      <main className="content">
        {screen === 'login'     && <LoginScreen onLogin={onLogin} />}
        {screen === 'dashboard' && <Dashboard user={user} bests={bests} streak={streak}
                                    onStart={start} onTheory={openTheory}
                                    onStats={() => setScreen('stats')} />}
        {screen === 'theory'    && theoryTopic &&
          <TheoryScreen topic={theoryTopic}
            onStartQuiz={() => start(theoryTopic)}
            onBack={() => setScreen('dashboard')} />}
        {screen === 'quiz'      && topic &&
          <QuizHost topic={topic} onFinish={finish} onExit={() => setScreen('dashboard')} />}
        {screen === 'results'   && session &&
          <Results topic={topic} questions={session.questions} answers={session.answers}
            grade={session.grade}
            onRetry={() => start(topic)} onExit={() => setScreen('dashboard')} />}
        {screen === 'stats' && window.StatsScreen &&
          <StatsScreen onBack={() => setScreen('dashboard')} />}
      </main>
    </div>
  );
}

window.MathApp = MathApp;
