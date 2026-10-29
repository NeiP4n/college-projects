import React, { useState, useCallback, useRef, useEffect } from 'react';
const Calculator = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [expression, setExpression] = useState('');
  const [result, setResult] = useState('0');
  const [isDegrees, setIsDegrees] = useState(true);
  const [memory, setMemory] = useState(0);
  const [history, setHistory] = useState([]);
  const displayRef = useRef(null);

  // Evaluate expression safely
  const evaluateExpression = useCallback(expr => {
    try {
      if (!expr) return '0';

      // Replace display symbols with math symbols
      let mathExpr = expr.replace(/÷/g, '/').replace(/×/g, '*').replace(/−/g, '-').replace(/π/g, Math.PI.toString()).replace(/e(?![\d\.])/g, Math.E.toString());

      // Handle degree to radian conversion for trig functions
      if (isDegrees) {
        const degToRad = angle => angle * Math.PI / 180;
        mathExpr = mathExpr.replace(/sin\(/g, `sin(${degToRad}(`).replace(/cos\(/g, `cos(${degToRad}(`).replace(/tan\(/g, `tan(${degToRad}(`);
      }

      // Safe evaluation
      const func = new Function('Math', 'return ' + mathExpr);
      const res = func(Math);

      // Format result
      if (typeof res === 'number') {
        if (isNaN(res) || !isFinite(res)) return 'Error';
        return res.toString().length > 15 ? res.toExponential(8) : res.toString();
      }
      return 'Error';
    } catch (e) {
      return 'Error';
    }
  }, [isDegrees]);

  // Live calculation
  useEffect(() => {
    if (expression) {
      const res = evaluateExpression(expression);
      setResult(res);
    } else {
      setResult('0');
    }
  }, [expression, evaluateExpression]);

  // Scroll display to the right
  useEffect(() => {
    if (displayRef.current) {
      displayRef.current.scrollLeft = displayRef.current.scrollWidth;
    }
  }, [expression, result]);
  const handleNumber = num => {
    setExpression(prev => prev + num);
  };
  const handleOperator = op => {
    if (!expression) return;
    setExpression(prev => {
      const lastChar = prev[prev.length - 1];
      if (['+', '−', '×', '÷', '^', '.'].includes(lastChar)) {
        return prev.slice(0, -1) + op;
      }
      return prev + op;
    });
  };
  const handleEquals = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const numRes = parseFloat(res);
      if (!isNaN(numRes)) {
        setHistory(prev => [{
          expr: expression,
          result: res
        }, ...prev].slice(0, 5));
      }
      setExpression(res);
      setResult(res);
    }
  };
  const handleClear = () => {
    setExpression('');
    setResult('0');
  };
  const handleBackspace = () => {
    setExpression(prev => prev.slice(0, -1));
  };
  const handleTrig = func => {
    setExpression(prev => prev + `${func}(`);
  };
  const handlePower = () => {
    setExpression(prev => prev + '^');
  };
  const handleSquareRoot = () => {
    setExpression(prev => {
      const val = parseFloat(prev) || 0;
      return Math.sqrt(val).toString();
    });
  };
  const handleSquare = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setExpression((num * num).toString());
      }
    }
  };
  const handleReciprocal = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (num !== 0) {
        setExpression((1 / num).toString());
      }
    }
  };
  const handleAbsolute = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setExpression(Math.abs(num).toString());
      }
    }
  };
  const handleToggleSign = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setExpression((-num).toString());
      }
    }
  };
  const handleExponent = () => {
    setExpression(prev => prev + 'e+');
  };
  const handleMemoryClear = () => setMemory(0);
  const handleMemoryRecall = () => {
    setExpression(prev => prev + memory.toString());
  };
  const handleMemoryAdd = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setMemory(prev => prev + num);
        setExpression('');
      }
    }
  };
  const handleMemorySubtract = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setMemory(prev => prev - num);
        setExpression('');
      }
    }
  };
  const handlePercent = () => {
    if (!expression) return;
    const res = evaluateExpression(expression);
    if (res !== 'Error') {
      const num = parseFloat(res);
      if (!isNaN(num)) {
        setExpression((num / 100).toString());
      }
    }
  };

  // Button styling
  const btnBase = 'px-3 py-2 rounded text-sm font-medium transition-all duration-200';
  const btnNumber = `${btnBase} bg-slate-700 hover:bg-slate-600 text-white border border-slate-600`;
  const btnOperator = `${btnBase} bg-orange-600 hover:bg-orange-500 text-white border border-orange-700`;
  const btnScientific = `${btnBase} bg-slate-600 hover:bg-slate-500 text-white border border-slate-700`;
  const btnFunction = `${btnBase} bg-purple-700 hover:bg-purple-600 text-white border border-purple-800`;
  const btnMemory = `${btnBase} bg-cyan-700 hover:bg-cyan-600 text-white border border-cyan-800`;
  const Button = ({
    onClick,
    children,
    className
  }) => /*#__PURE__*/React.createElement("button", {
    onClick: onClick,
    className: className,
    style: {
      boxShadow: '0 0 20px rgba(0,0,0,0.3), inset 0 1px 0 rgba(255,255,255,0.1)',
      transition: 'all 200ms ease'
    },
    onMouseEnter: e => {
      e.currentTarget.style.boxShadow = '0 0 25px rgba(255,255,255,0.1), inset 0 1px 0 rgba(255,255,255,0.2)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.boxShadow = '0 0 20px rgba(0,0,0,0.3), inset 0 1px 0 rgba(255,255,255,0.1)';
    }
  }, children);
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("button", {
    onClick: () => setIsOpen(!isOpen),
    style: {
      position: 'fixed',
      bottom: '24px',
      right: '24px',
      width: '56px',
      height: '56px',
      borderRadius: '50%',
      backgroundColor: 'rgba(255, 158, 107, 0.9)',
      border: '1px solid rgba(255,255,255,0.2)',
      zIndex: 999,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      cursor: 'pointer',
      boxShadow: '0 8px 32px rgba(255, 158, 107, 0.4)',
      backdropFilter: 'blur(20px)',
      color: '#fff',
      fontSize: '24px',
      fontWeight: 'bold',
      transition: 'all 300ms ease'
    },
    onMouseEnter: e => {
      e.currentTarget.style.boxShadow = '0 12px 40px rgba(255, 158, 107, 0.6)';
      e.currentTarget.style.transform = 'scale(1.1)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.boxShadow = '0 8px 32px rgba(255, 158, 107, 0.4)';
      e.currentTarget.style.transform = 'scale(1)';
    }
  }, "\u2211"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'fixed',
      right: 0,
      top: 0,
      height: '100vh',
      width: '320px',
      backgroundColor: 'rgba(15, 20, 35, 0.92)',
      borderLeft: '1px solid rgba(255,255,255,0.1)',
      backdropFilter: 'blur(20px)',
      zIndex: 1000,
      transform: isOpen ? 'translateX(0)' : 'translateX(100%)',
      transition: 'transform 300ms ease',
      display: 'flex',
      flexDirection: 'column',
      boxShadow: isOpen ? '-8px 0 32px rgba(0,0,0,0.5)' : 'none',
      overflowY: 'auto',
      fontFamily: 'system-ui, -apple-system, sans-serif'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '16px',
      borderBottom: '1px solid rgba(255,255,255,0.1)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    ref: displayRef,
    style: {
      backgroundColor: 'rgba(0,0,0,0.3)',
      border: '1px solid rgba(255,255,255,0.1)',
      borderRadius: '8px',
      padding: '8px 12px',
      marginBottom: '8px',
      minHeight: '24px',
      fontSize: '11px',
      color: 'rgba(255,255,255,0.6)',
      fontFamily: 'Consolas, monospace',
      overflowX: 'auto',
      overflowY: 'hidden',
      wordBreak: 'break-all',
      whiteSpace: 'nowrap'
    }
  }, expression || '∅'), /*#__PURE__*/React.createElement("div", {
    style: {
      backgroundColor: 'rgba(0,0,0,0.2)',
      border: '1px solid rgba(255, 158, 107, 0.2)',
      borderRadius: '8px',
      padding: '12px',
      minHeight: '32px',
      fontSize: '20px',
      fontWeight: 'bold',
      color: '#fff',
      fontFamily: 'Consolas, monospace',
      textAlign: 'right',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'flex-end'
    }
  }, result), memory !== 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: '10px',
      color: 'rgba(255,255,255,0.4)',
      marginTop: '4px',
      textAlign: 'right'
    }
  }, "M: ", memory.toString().substring(0, 10))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      padding: '12px',
      overflowY: 'auto'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => setIsDegrees(!isDegrees),
    className: btnFunction
  }, isDegrees ? 'DEG' : 'RAD'), /*#__PURE__*/React.createElement(Button, {
    onClick: handleMemoryClear,
    className: btnMemory
  }, "MC"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleMemoryRecall,
    className: btnMemory
  }, "MR"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleMemoryAdd,
    className: btnMemory
  }, "M+"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleMemorySubtract,
    className: btnMemory
  }, "M\u2212")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('sin'),
    className: btnScientific
  }, "sin"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('cos'),
    className: btnScientific
  }, "cos"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('tan'),
    className: btnScientific
  }, "tan"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('ln'),
    className: btnScientific
  }, "ln"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('log'),
    className: btnScientific
  }, "log")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('asin'),
    className: btnScientific
  }, "asin"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('acos'),
    className: btnScientific
  }, "acos"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('atan'),
    className: btnScientific
  }, "atan"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleSquareRoot,
    className: btnFunction
  }, "\u221A"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleSquare,
    className: btnFunction
  }, "x\xB2")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: handlePower,
    className: btnFunction
  }, "x\u02B8"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('exp'),
    className: btnFunction
  }, "e\u02E3"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleTrig('pow(10'),
    className: btnFunction
  }, "10\u02E3"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleAbsolute,
    className: btnFunction
  }, "|x|"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleReciprocal,
    className: btnFunction
  }, "1/x")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => setExpression(prev => prev + 'π'),
    className: btnFunction
  }, "\u03C0"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => setExpression(prev => prev + 'e'),
    className: btnFunction
  }, "e"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('('),
    className: btnScientific
  }, "("), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber(')'),
    className: btnScientific
  }, ")"), /*#__PURE__*/React.createElement(Button, {
    onClick: handlePercent,
    className: btnScientific
  }, "%")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('7'),
    className: btnNumber
  }, "7"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('8'),
    className: btnNumber
  }, "8"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('9'),
    className: btnNumber
  }, "9"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleOperator('÷'),
    className: btnOperator
  }, "\xF7"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleClear,
    className: btnOperator
  }, "C")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('4'),
    className: btnNumber
  }, "4"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('5'),
    className: btnNumber
  }, "5"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('6'),
    className: btnNumber
  }, "6"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleOperator('×'),
    className: btnOperator
  }, "\xD7"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleBackspace,
    className: btnOperator
  }, "\u232B")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '6px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('1'),
    className: btnNumber
  }, "1"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('2'),
    className: btnNumber
  }, "2"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('3'),
    className: btnNumber
  }, "3"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleOperator('−'),
    className: btnOperator
  }, "\u2212"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleToggleSign,
    className: btnOperator
  }, "\xB1")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(5, 1fr)',
      gap: '6px',
      marginBottom: '12px'
    }
  }, /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('0'),
    className: btnNumber
  }, "0"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleNumber('.'),
    className: btnNumber
  }, "."), /*#__PURE__*/React.createElement(Button, {
    onClick: handleExponent,
    className: btnScientific
  }, "EXP"), /*#__PURE__*/React.createElement(Button, {
    onClick: () => handleOperator('+'),
    className: btnOperator
  }, "+"), /*#__PURE__*/React.createElement(Button, {
    onClick: handleEquals,
    className: `${btnOperator} font-bold`
  }, "="))), history.length > 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      borderTop: '1px solid rgba(255,255,255,0.1)',
      padding: '12px',
      fontSize: '11px'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      color: 'rgba(255,255,255,0.5)',
      marginBottom: '8px',
      fontWeight: 'bold'
    }
  }, "History"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: '6px'
    }
  }, history.map((item, idx) => /*#__PURE__*/React.createElement("div", {
    key: idx,
    onClick: () => setExpression(item.result),
    style: {
      backgroundColor: 'rgba(255,255,255,0.05)',
      padding: '6px 8px',
      borderRadius: '4px',
      cursor: 'pointer',
      color: 'rgba(255,255,255,0.7)',
      fontSize: '10px',
      fontFamily: 'Consolas, monospace',
      border: '1px solid rgba(255,255,255,0.05)',
      transition: 'all 200ms ease'
    },
    onMouseEnter: e => {
      e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.1)';
      e.currentTarget.style.borderColor = 'rgba(255,255,255,0.1)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.05)';
      e.currentTarget.style.borderColor = 'rgba(255,255,255,0.05)';
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      whiteSpace: 'nowrap',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      marginBottom: '2px'
    }
  }, item.expr.substring(0, 30)), /*#__PURE__*/React.createElement("div", {
    style: {
      color: 'rgba(255, 158, 107, 0.9)',
      fontWeight: 'bold'
    }
  }, "= ", item.result.substring(0, 20))))))));
};

// Export for global access
window.CalcPanel = Calculator;
