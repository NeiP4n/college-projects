// stats.jsx — Statistics screen

function StatsScreen({ onBack }) {
  const { useState, useEffect } = React;
  const [data, setData] = useState(null);
  const [tab,  setTab]  = useState('overview');

  useEffect(() => {
    const handler = (e) => {
      try {
        const msg = JSON.parse(e.data);
        if (msg.type === 'stats') setData(msg.data);
      } catch {}
    };
    window.addEventListener('message', handler);
    if (window.chrome && window.chrome.webview)
      window.chrome.webview.postMessage(JSON.stringify({ action: 'getStats' }));
    return () => window.removeEventListener('message', handler);
  }, []);

  const tabs = [
    { key: 'overview', label: 'Обзор'   },
    { key: 'topics',   label: 'Темы'    },
    { key: 'history',  label: 'История' },
  ];

  return (
    <div className="quiz stats-screen">
      <div className="quiz-bar">
        <button className="icon-btn" onClick={onBack} aria-label="Назад">
          <UIcon name="back" size={18} />
        </button>
        <div className="quiz-topic">
          <span>📊</span>
          <span>Статистика</span>
        </div>
        <div style={{ width: 36 }} />
      </div>

      <div className="stats-tabs">
        {tabs.map(t => (
          <button key={t.key}
            className={'stats-tab' + (tab === t.key ? ' active' : '')}
            onClick={() => setTab(t.key)}>{t.label}</button>
        ))}
      </div>

      <div className="stats-body">
        {!data ? (
          <div className="stats-loading">Загрузка…</div>
        ) : tab === 'overview' ? (
          <OverviewTab data={data} />
        ) : tab === 'topics' ? (
          <TopicsTab data={data} />
        ) : (
          <HistoryTab data={data} />
        )}
      </div>
    </div>
  );
}

function OverviewTab({ data }) {
  const cards = [
    { label: 'Тестов пройдено', value: data.totalTests,        icon: '📝' },
    { label: 'Средний балл',    value: (data.avgPct || 0) + '%', icon: '⭐' },
    { label: 'Тем изучено',     value: data.topicsCount,       icon: '📚' },
  ];
  const recent10 = (data.recent || []).slice(0, 10).reverse();

  return (
    <div className="stats-overview">
      <div className="stats-cards">
        {cards.map((c, i) => (
          <div className="stats-card" key={i}>
            <span className="stats-card-icon">{c.icon}</span>
            <span className="stats-card-val">{c.value}</span>
            <span className="stats-card-lbl">{c.label}</span>
          </div>
        ))}
      </div>

      {recent10.length > 0 ? (
        <div className="stats-chart-wrap">
          <div className="stats-chart-title">Последние {recent10.length} тестов</div>
          <div className="stats-bar-chart">
            {recent10.map((r, i) => (
              <div key={i} className="stats-bar-col" title={r.topicName + ': ' + r.pct + '%'}>
                <div className="stats-bar-val">{r.pct}%</div>
                <div className="stats-bar" style={{
                  height: Math.max(4, r.pct * 1.2) + 'px',
                  background: r.pct >= 90 ? '#34d399' : r.pct >= 70 ? '#a78bfa' : r.pct >= 50 ? '#fb923c' : '#f87171'
                }} />
                <div className="stats-bar-lbl">{r.topicName.substring(0, 6)}</div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="stats-empty">Пройди первый тест, чтобы увидеть статистику</div>
      )}
    </div>
  );
}

function TopicsTab({ data }) {
  const topics = data.byTopic || [];
  if (!topics.length) return <div className="stats-empty">Нет данных</div>;

  return (
    <div className="stats-topics">
      {topics.map((t, i) => (
        <div key={i} className="stats-topic-row">
          <div className="stats-topic-name">{t.name}</div>
          <div className="stats-topic-bars">
            <div className="stats-topic-bar-wrap" title={'Средний: ' + t.avg + '%'}>
              <div className="stats-topic-bar avg" style={{ width: t.avg + '%' }} />
            </div>
            <div className="stats-topic-bar-wrap" title={'Лучший: ' + t.best + '%'}>
              <div className="stats-topic-bar best" style={{ width: t.best + '%' }} />
            </div>
          </div>
          <div className="stats-topic-nums">
            <span className="stats-avg">{t.avg}%</span>
            <span className="stats-best">▲{t.best}%</span>
            <span className="stats-count">{t.count}×</span>
          </div>
        </div>
      ))}
    </div>
  );
}

function HistoryTab({ data }) {
  const history = data.recent || [];
  if (!history.length) return <div className="stats-empty">История пуста</div>;

  return (
    <div className="stats-history">
      {history.map((r, i) => {
        const grade = r.pct >= 90 ? 5 : r.pct >= 70 ? 4 : r.pct >= 50 ? 3 : 2;
        const color = grade === 5 ? '#34d399' : grade === 4 ? '#a78bfa' : grade === 3 ? '#fb923c' : '#f87171';
        return (
          <div key={i} className="stats-hist-row">
            <div className="stats-hist-grade" style={{ color }}>{grade}</div>
            <div className="stats-hist-info">
              <div className="stats-hist-topic">{r.topicName}</div>
              <div className="stats-hist-sub">{r.correct}/{r.total} верно · {r.date}</div>
            </div>
            <div className="stats-hist-pct" style={{ color }}>{r.pct}%</div>
          </div>
        );
      })}
    </div>
  );
}

Object.assign(window, { StatsScreen });
