const TWEAK_DEFAULTS = {
  "timeMode": "auto",
  "cycleSeconds": 30,
  "motion": true,
  "snow": true,
  "palette": ["#b39ddb", "#e0d2f5"]
};

const TIME_OPTS = [
  { value: "auto",  label: "Цикл дня" },
  { value: "dawn",  label: "Рассвет"  },
  { value: "day",   label: "День"     },
  { value: "dusk",  label: "Закат"    },
  { value: "night", label: "Ночь"     },
];

function Root() {
  const CalcPanel = window.CalcPanel;
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);
  const tweaks = {
    timeMode:     t.timeMode,
    cycleSeconds: t.cycleSeconds,
    motion:       t.motion,
    snow:         t.snow,
    accent:       (t.palette && t.palette[0]) || "#b39ddb",
    accent2:      (t.palette && t.palette[1]) || "#e0d2f5",
  };
  return (
    <React.Fragment>
      <MathApp tweaks={tweaks} />
      {window.CalcPanel && <CalcPanel />}
      <TweaksPanel title="Настройки сцены">
        <TweakSection label="Атмосфера" />
        <TweakSelect label="Время суток" value={t.timeMode}
          options={TIME_OPTS} onChange={v => setTweak('timeMode', v)} />
        <TweakSlider label="Длина цикла" value={t.cycleSeconds} min={20} max={240} step={5} unit=" с"
          onChange={v => setTweak('cycleSeconds', v)} />
        <TweakToggle label="Движение и параллакс" value={t.motion}
          onChange={v => setTweak('motion', v)} />
        <TweakToggle label="Снег" value={t.snow}
          onChange={v => setTweak('snow', v)} />
        <TweakSection label="Акцент" />
        <TweakColor label="Палитра" value={t.palette}
          options={[
            ["#ff9e6b","#ffd29a"],
            ["#ff7a8a","#ffc1b0"],
            ["#7fd1c0","#bdeede"],
            ["#b39ddb","#e0d2f5"],
            ["#7ec8ff","#cfeaff"],
          ]}
          onChange={v => setTweak('palette', v)} />
      </TweaksPanel>
    </React.Fragment>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<Root />);
