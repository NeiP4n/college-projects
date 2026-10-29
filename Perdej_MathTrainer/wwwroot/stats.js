// stats.jsx — Statistics screen

function StatsScreen({
  onBack
}) {
  const {
    useState,
    useEffect
  } = React;
  const [data, setData] = useState(null);
  const [tab, setTab] = useState('overview');
  useEffect(() => {
    const handler = e => {
      try {
        const msg = JSON.parse(e.data);
        if (msg.type === 'stats') setData(msg.data);
      } catch {}
    };
    window.addEventListener('message', handler);
    if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(JSON.stringify({
      action: 'getStats'
    }));
    return () => window.removeEventListener('message', handler);
  }, []);
  const tabs = [{
    key: 'overview',
    label: 'Обзор'
  }, {
    key: 'topics',
    label: 'Темы'
  }, {
    key: 'history',
    label: 'История'
  }];
  return /*#__PURE__*/React.createElement("div", {
    className: "quiz stats-screen"
  }, /*#__PURE__*/React.createElement("div", {
    className: "quiz-bar"
  }, /*#__PURE__*/React.createElement("button", {
    className: "icon-btn",
    onClick: onBack,
    "aria-label": "\u041D\u0430\u0437\u0430\u0434"
  }, /*#__PURE__*/React.createElement(UIcon, {
    name: "back",
    size: 18
  })), /*#__PURE__*/React.createElement("div", {
    className: "quiz-topic"
  }, /*#__PURE__*/React.createElement("span", null, "\uD83D\uDCCA"), /*#__PURE__*/React.createElement("span", null, "\u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430")), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 36
    }
  })), /*#__PURE__*/React.createElement("div", {
    className: "stats-tabs"
  }, tabs.map(t => /*#__PURE__*/React.createElement("button", {
    key: t.key,
    className: 'stats-tab' + (tab === t.key ? ' active' : ''),
    onClick: () => setTab(t.key)
  }, t.label))), /*#__PURE__*/React.createElement("div", {
    className: "stats-body"
  }, !data ? /*#__PURE__*/React.createElement("div", {
    className: "stats-loading"
  }, "\u0417\u0430\u0433\u0440\u0443\u0437\u043A\u0430\u2026") : tab === 'overview' ? /*#__PURE__*/React.createElement(OverviewTab, {
    data: data
  }) : tab === 'topics' ? /*#__PURE__*/React.createElement(TopicsTab, {
    data: data
  }) : /*#__PURE__*/React.createElement(HistoryTab, {
    data: data
  })));
}
function OverviewTab({
  data
}) {
  const cards = [{
    label: 'Тестов пройдено',
    value: data.totalTests,
    icon: '📝'
  }, {
    label: 'Средний балл',
    value: (data.avgPct || 0) + '%',
    icon: '⭐'
  }, {
    label: 'Тем изучено',
    value: data.topicsCount,
    icon: '📚'
  }];
  const recent10 = (data.recent || []).slice(0, 10).reverse();
  return /*#__PURE__*/React.createElement("div", {
    className: "stats-overview"
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-cards"
  }, cards.map((c, i) => /*#__PURE__*/React.createElement("div", {
    className: "stats-card",
    key: i
  }, /*#__PURE__*/React.createElement("span", {
    className: "stats-card-icon"
  }, c.icon), /*#__PURE__*/React.createElement("span", {
    className: "stats-card-val"
  }, c.value), /*#__PURE__*/React.createElement("span", {
    className: "stats-card-lbl"
  }, c.label)))), recent10.length > 0 ? /*#__PURE__*/React.createElement("div", {
    className: "stats-chart-wrap"
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-chart-title"
  }, "\u041F\u043E\u0441\u043B\u0435\u0434\u043D\u0438\u0435 ", recent10.length, " \u0442\u0435\u0441\u0442\u043E\u0432"), /*#__PURE__*/React.createElement("div", {
    className: "stats-bar-chart"
  }, recent10.map((r, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    className: "stats-bar-col",
    title: r.topicName + ': ' + r.pct + '%'
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-bar-val"
  }, r.pct, "%"), /*#__PURE__*/React.createElement("div", {
    className: "stats-bar",
    style: {
      height: Math.max(4, r.pct * 1.2) + 'px',
      background: r.pct >= 90 ? '#34d399' : r.pct >= 70 ? '#a78bfa' : r.pct >= 50 ? '#fb923c' : '#f87171'
    }
  }), /*#__PURE__*/React.createElement("div", {
    className: "stats-bar-lbl"
  }, r.topicName.substring(0, 6)))))) : /*#__PURE__*/React.createElement("div", {
    className: "stats-empty"
  }, "\u041F\u0440\u043E\u0439\u0434\u0438 \u043F\u0435\u0440\u0432\u044B\u0439 \u0442\u0435\u0441\u0442, \u0447\u0442\u043E\u0431\u044B \u0443\u0432\u0438\u0434\u0435\u0442\u044C \u0441\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0443"));
}
function TopicsTab({
  data
}) {
  const topics = data.byTopic || [];
  if (!topics.length) return /*#__PURE__*/React.createElement("div", {
    className: "stats-empty"
  }, "\u041D\u0435\u0442 \u0434\u0430\u043D\u043D\u044B\u0445");
  return /*#__PURE__*/React.createElement("div", {
    className: "stats-topics"
  }, topics.map((t, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    className: "stats-topic-row"
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-name"
  }, t.name), /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-bars"
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-bar-wrap",
    title: 'Средний: ' + t.avg + '%'
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-bar avg",
    style: {
      width: t.avg + '%'
    }
  })), /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-bar-wrap",
    title: 'Лучший: ' + t.best + '%'
  }, /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-bar best",
    style: {
      width: t.best + '%'
    }
  }))), /*#__PURE__*/React.createElement("div", {
    className: "stats-topic-nums"
  }, /*#__PURE__*/React.createElement("span", {
    className: "stats-avg"
  }, t.avg, "%"), /*#__PURE__*/React.createElement("span", {
    className: "stats-best"
  }, "\u25B2", t.best, "%"), /*#__PURE__*/React.createElement("span", {
    className: "stats-count"
  }, t.count, "\xD7")))));
}
function HistoryTab({
  data
}) {
  const history = data.recent || [];
  if (!history.length) return /*#__PURE__*/React.createElement("div", {
    className: "stats-empty"
  }, "\u0418\u0441\u0442\u043E\u0440\u0438\u044F \u043F\u0443\u0441\u0442\u0430");
  return /*#__PURE__*/React.createElement("div", {
    className: "stats-history"
  }, history.map((r, i) => {
    const grade = r.pct >= 90 ? 5 : r.pct >= 70 ? 4 : r.pct >= 50 ? 3 : 2;
    const color = grade === 5 ? '#34d399' : grade === 4 ? '#a78bfa' : grade === 3 ? '#fb923c' : '#f87171';
    return /*#__PURE__*/React.createElement("div", {
      key: i,
      className: "stats-hist-row"
    }, /*#__PURE__*/React.createElement("div", {
      className: "stats-hist-grade",
      style: {
        color
      }
    }, grade), /*#__PURE__*/React.createElement("div", {
      className: "stats-hist-info"
    }, /*#__PURE__*/React.createElement("div", {
      className: "stats-hist-topic"
    }, r.topicName), /*#__PURE__*/React.createElement("div", {
      className: "stats-hist-sub"
    }, r.correct, "/", r.total, " \u0432\u0435\u0440\u043D\u043E \xB7 ", r.date)), /*#__PURE__*/React.createElement("div", {
      className: "stats-hist-pct",
      style: {
        color
      }
    }, r.pct, "%"));
  }));
}
Object.assign(window, {
  StatsScreen
});
