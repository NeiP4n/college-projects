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
  const evaluateExpression = useCallback((expr) => {
    try {
      if (!expr) return '0';

      // Replace display symbols with math symbols
      let mathExpr = expr
        .replace(/÷/g, '/')
        .replace(/×/g, '*')
        .replace(/−/g, '-')
        .replace(/π/g, Math.PI.toString())
        .replace(/e(?![\d\.])/g, Math.E.toString());

      // Handle degree to radian conversion for trig functions
      if (isDegrees) {
        const degToRad = (angle) => (angle * Math.PI) / 180;
        mathExpr = mathExpr
          .replace(/sin\(/g, `sin(${degToRad}(`)
          .replace(/cos\(/g, `cos(${degToRad}(`)
          .replace(/tan\(/g, `tan(${degToRad}(`);
      }

      // Safe evaluation
      const func = new Function('Math', 'return ' + mathExpr);
      const res = func(Math);

      // Format result
      if (typeof res === 'number') {
        if (isNaN(res) || !isFinite(res)) return 'Error';
        return res.toString().length > 15
          ? res.toExponential(8)
          : res.toString();
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

  const handleNumber = (num) => {
    setExpression(prev => prev + num);
  };

  const handleOperator = (op) => {
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
        setHistory(prev => [{ expr: expression, result: res }, ...prev].slice(0, 5));
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

  const handleTrig = (func) => {
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

  const Button = ({ onClick, children, className }) => (
    <button
      onClick={onClick}
      className={className}
      style={{
        boxShadow: '0 0 20px rgba(0,0,0,0.3), inset 0 1px 0 rgba(255,255,255,0.1)',
        transition: 'all 200ms ease'
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.boxShadow = '0 0 25px rgba(255,255,255,0.1), inset 0 1px 0 rgba(255,255,255,0.2)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.boxShadow = '0 0 20px rgba(0,0,0,0.3), inset 0 1px 0 rgba(255,255,255,0.1)';
      }}
    >
      {children}
    </button>
  );

  return (
    <>
      {/* Toggle Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        style={{
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
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.boxShadow = '0 12px 40px rgba(255, 158, 107, 0.6)';
          e.currentTarget.style.transform = 'scale(1.1)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.boxShadow = '0 8px 32px rgba(255, 158, 107, 0.4)';
          e.currentTarget.style.transform = 'scale(1)';
        }}
      >
        ∑
      </button>

      {/* Slide-in Panel */}
      <div
        style={{
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
        }}
      >
        {/* Display */}
        <div style={{ padding: '16px', borderBottom: '1px solid rgba(255,255,255,0.1)' }}>
          <div
            ref={displayRef}
            style={{
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
            }}
          >
            {expression || '∅'}
          </div>
          <div
            style={{
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
            }}
          >
            {result}
          </div>
          {memory !== 0 && (
            <div style={{ fontSize: '10px', color: 'rgba(255,255,255,0.4)', marginTop: '4px', textAlign: 'right' }}>
              M: {memory.toString().substring(0, 10)}
            </div>
          )}
        </div>

        {/* Keypad */}
        <div style={{ flex: 1, padding: '12px', overflowY: 'auto' }}>
          {/* Row 0: Mode/Memory */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => setIsDegrees(!isDegrees)} className={btnFunction}>
              {isDegrees ? 'DEG' : 'RAD'}
            </Button>
            <Button onClick={handleMemoryClear} className={btnMemory}>MC</Button>
            <Button onClick={handleMemoryRecall} className={btnMemory}>MR</Button>
            <Button onClick={handleMemoryAdd} className={btnMemory}>M+</Button>
            <Button onClick={handleMemorySubtract} className={btnMemory}>M−</Button>
          </div>

          {/* Row 1: Trig */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => handleTrig('sin')} className={btnScientific}>sin</Button>
            <Button onClick={() => handleTrig('cos')} className={btnScientific}>cos</Button>
            <Button onClick={() => handleTrig('tan')} className={btnScientific}>tan</Button>
            <Button onClick={() => handleTrig('ln')} className={btnScientific}>ln</Button>
            <Button onClick={() => handleTrig('log')} className={btnScientific}>log</Button>
          </div>

          {/* Row 2: Inverse Trig */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => handleTrig('asin')} className={btnScientific}>asin</Button>
            <Button onClick={() => handleTrig('acos')} className={btnScientific}>acos</Button>
            <Button onClick={() => handleTrig('atan')} className={btnScientific}>atan</Button>
            <Button onClick={handleSquareRoot} className={btnFunction}>√</Button>
            <Button onClick={handleSquare} className={btnFunction}>x²</Button>
          </div>

          {/* Row 3: Advanced */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={handlePower} className={btnFunction}>xʸ</Button>
            <Button onClick={() => handleTrig('exp')} className={btnFunction}>eˣ</Button>
            <Button onClick={() => handleTrig('pow(10')} className={btnFunction}>10ˣ</Button>
            <Button onClick={handleAbsolute} className={btnFunction}>|x|</Button>
            <Button onClick={handleReciprocal} className={btnFunction}>1/x</Button>
          </div>

          {/* Row 4: Constants */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => setExpression(prev => prev + 'π')} className={btnFunction}>π</Button>
            <Button onClick={() => setExpression(prev => prev + 'e')} className={btnFunction}>e</Button>
            <Button onClick={() => handleNumber('(')} className={btnScientific}>(</Button>
            <Button onClick={() => handleNumber(')')} className={btnScientific}>)</Button>
            <Button onClick={handlePercent} className={btnScientific}>%</Button>
          </div>

          {/* Row 5: 7-9, ÷, C */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => handleNumber('7')} className={btnNumber}>7</Button>
            <Button onClick={() => handleNumber('8')} className={btnNumber}>8</Button>
            <Button onClick={() => handleNumber('9')} className={btnNumber}>9</Button>
            <Button onClick={() => handleOperator('÷')} className={btnOperator}>÷</Button>
            <Button onClick={handleClear} className={btnOperator}>C</Button>
          </div>

          {/* Row 6: 4-6, ×, ⌫ */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => handleNumber('4')} className={btnNumber}>4</Button>
            <Button onClick={() => handleNumber('5')} className={btnNumber}>5</Button>
            <Button onClick={() => handleNumber('6')} className={btnNumber}>6</Button>
            <Button onClick={() => handleOperator('×')} className={btnOperator}>×</Button>
            <Button onClick={handleBackspace} className={btnOperator}>⌫</Button>
          </div>

          {/* Row 7: 1-3, −, ± */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '6px' }}>
            <Button onClick={() => handleNumber('1')} className={btnNumber}>1</Button>
            <Button onClick={() => handleNumber('2')} className={btnNumber}>2</Button>
            <Button onClick={() => handleNumber('3')} className={btnNumber}>3</Button>
            <Button onClick={() => handleOperator('−')} className={btnOperator}>−</Button>
            <Button onClick={handleToggleSign} className={btnOperator}>±</Button>
          </div>

          {/* Row 8: 0, ., EXP, +, = */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '6px', marginBottom: '12px' }}>
            <Button onClick={() => handleNumber('0')} className={btnNumber}>0</Button>
            <Button onClick={() => handleNumber('.')} className={btnNumber}>.</Button>
            <Button onClick={handleExponent} className={btnScientific}>EXP</Button>
            <Button onClick={() => handleOperator('+')} className={btnOperator}>+</Button>
            <Button onClick={handleEquals} className={`${btnOperator} font-bold`}>=</Button>
          </div>
        </div>

        {/* History */}
        {history.length > 0 && (
          <div style={{ borderTop: '1px solid rgba(255,255,255,0.1)', padding: '12px', fontSize: '11px' }}>
            <div style={{ color: 'rgba(255,255,255,0.5)', marginBottom: '8px', fontWeight: 'bold' }}>History</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
              {history.map((item, idx) => (
                <div
                  key={idx}
                  onClick={() => setExpression(item.result)}
                  style={{
                    backgroundColor: 'rgba(255,255,255,0.05)',
                    padding: '6px 8px',
                    borderRadius: '4px',
                    cursor: 'pointer',
                    color: 'rgba(255,255,255,0.7)',
                    fontSize: '10px',
                    fontFamily: 'Consolas, monospace',
                    border: '1px solid rgba(255,255,255,0.05)',
                    transition: 'all 200ms ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.1)';
                    e.currentTarget.style.borderColor = 'rgba(255,255,255,0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.05)';
                    e.currentTarget.style.borderColor = 'rgba(255,255,255,0.05)';
                  }}
                >
                  <div style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', marginBottom: '2px' }}>
                    {item.expr.substring(0, 30)}
                  </div>
                  <div style={{ color: 'rgba(255, 158, 107, 0.9)', fontWeight: 'bold' }}>
                    = {item.result.substring(0, 20)}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </>
  );
};

// Export for global access
window.CalcPanel = Calculator;

