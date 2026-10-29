const TWEAK_DEFAULTS = {
  "timeMode": "auto",
  "cycleSeconds": 30,
  "motion": true,
  "snow": true,
  "palette": ["#b39ddb", "#e0d2f5"]
};
const TIME_OPTS = [{
  value: "auto",
  label: "Цикл дня"
}, {
  value: "dawn",
  label: "Рассвет"
}, {
  value: "day",
  label: "День"
}, {
  value: "dusk",
  label: "Закат"
}, {
  value: "night",
  label: "Ночь"
}];
function Root() {
  const CalcPanel = window.CalcPanel;
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);
  const tweaks = {
    timeMode: t.timeMode,
    cycleSeconds: t.cycleSeconds,
    motion: t.motion,
    snow: t.snow,
    accent: t.palette && t.palette[0] || "#b39ddb",
    accent2: t.palette && t.palette[1] || "#e0d2f5"
  };
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(MathApp, {
    tweaks: tweaks
  }), window.CalcPanel && /*#__PURE__*/React.createElement(CalcPanel, null), /*#__PURE__*/React.createElement(TweaksPanel, {
    title: "\u041D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0438 \u0441\u0446\u0435\u043D\u044B"
  }, /*#__PURE__*/React.createElement(TweakSection, {
    label: "\u0410\u0442\u043C\u043E\u0441\u0444\u0435\u0440\u0430"
  }), /*#__PURE__*/React.createElement(TweakSelect, {
    label: "\u0412\u0440\u0435\u043C\u044F \u0441\u0443\u0442\u043E\u043A",
    value: t.timeMode,
    options: TIME_OPTS,
    onChange: v => setTweak('timeMode', v)
  }), /*#__PURE__*/React.createElement(TweakSlider, {
    label: "\u0414\u043B\u0438\u043D\u0430 \u0446\u0438\u043A\u043B\u0430",
    value: t.cycleSeconds,
    min: 20,
    max: 240,
    step: 5,
    unit: " \u0441",
    onChange: v => setTweak('cycleSeconds', v)
  }), /*#__PURE__*/React.createElement(TweakToggle, {
    label: "\u0414\u0432\u0438\u0436\u0435\u043D\u0438\u0435 \u0438 \u043F\u0430\u0440\u0430\u043B\u043B\u0430\u043A\u0441",
    value: t.motion,
    onChange: v => setTweak('motion', v)
  }), /*#__PURE__*/React.createElement(TweakToggle, {
    label: "\u0421\u043D\u0435\u0433",
    value: t.snow,
    onChange: v => setTweak('snow', v)
  }), /*#__PURE__*/React.createElement(TweakSection, {
    label: "\u0410\u043A\u0446\u0435\u043D\u0442"
  }), /*#__PURE__*/React.createElement(TweakColor, {
    label: "\u041F\u0430\u043B\u0438\u0442\u0440\u0430",
    value: t.palette,
    options: [["#ff9e6b", "#ffd29a"], ["#ff7a8a", "#ffc1b0"], ["#7fd1c0", "#bdeede"], ["#b39ddb", "#e0d2f5"], ["#7ec8ff", "#cfeaff"]],
    onChange: v => setTweak('palette', v)
  })));
}
ReactDOM.createRoot(document.getElementById('root')).render(/*#__PURE__*/React.createElement(Root, null));
