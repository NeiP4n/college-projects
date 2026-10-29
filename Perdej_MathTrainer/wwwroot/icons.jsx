// icons.jsx — minimal stroke icons (topics + UI). Exposes window.TopicIcon, window.UIcon.
// Strokes inherit currentColor; no fills unless noted.

function TopicIcon({ id, size = 30 }) {
  const p = { width: size, height: size, viewBox: '0 0 32 32', fill: 'none',
    stroke: 'currentColor', strokeWidth: 1.7, strokeLinecap: 'round', strokeLinejoin: 'round' };
  switch (id) {
    case 'counting': return (
      <svg {...p}><circle cx="9" cy="9" r="2.2" /><circle cx="16" cy="9" r="2.2" /><circle cx="23" cy="9" r="2.2" />
        <circle cx="9" cy="16" r="2.2" /><circle cx="16" cy="16" r="2.2" /><circle cx="9" cy="23" r="2.2" /></svg>);
    case 'arithmetic': return (
      <svg {...p}><path d="M7 9h6M10 6v6" /><path d="M19 9h6" /><path d="M8 22l5 5M13 22l-5 5" /><path d="M19 24.5h6" /></svg>);
    case 'powers': return (
      <svg {...p}><path d="M5 24l4-12 3.5 8" /><path d="M7.5 18h4" /><path d="M20 11l2.5-3 2.5 3" /><text x="20" y="25" fontSize="11" stroke="none" fill="currentColor" fontFamily="Quicksand, sans-serif" fontWeight="600">x²</text></svg>);
    case 'fractions': return (
      <svg {...p}><path d="M22 7L10 25" /><circle cx="11" cy="10" r="2" /><circle cx="21" cy="22" r="2" /></svg>);
    case 'percentages': return (
      <svg {...p}><path d="M8 24L24 8" /><circle cx="10.5" cy="10.5" r="3" /><circle cx="21.5" cy="21.5" r="3" /></svg>);
    case 'linear_eq': return (
      <svg {...p}><path d="M6 26V6M6 26h20" /><path d="M9 23L25 9" /><circle cx="17" cy="15" r="1.4" fill="currentColor" stroke="none" /></svg>);
    case 'inequalities': return (
      <svg {...p}><path d="M19 8l-9 5 9 5" /><path d="M14 24h11" /><path d="M14 27.5h11" /></svg>);
    case 'progression': return (
      <svg {...p}><path d="M6 25h20" /><circle cx="9" cy="22" r="1.5" /><circle cx="15" cy="17" r="1.5" /><circle cx="21" cy="12" r="1.5" /><circle cx="26" cy="8" r="1.5" /><path d="M10 21l4-3.5M16 16l4-3M22 11l3-2" /></svg>);
    case 'areas': return (
      <svg {...p}><path d="M5 11h8v8H5z" /><path d="M19 7l5 8h-10z" opacity="0" /><path d="M22 6l5 9H17z" /><circle cx="11" cy="24" r="4.5" /></svg>);
    default: return (<svg {...p}><circle cx="16" cy="16" r="10" /></svg>);
  }
}

function UIcon({ name, size = 20, stroke = 1.8 }) {
  const p = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none',
    stroke: 'currentColor', strokeWidth: stroke, strokeLinecap: 'round', strokeLinejoin: 'round' };
  switch (name) {
    case 'summit': return (<svg {...p}><path d="M3 20l6-13 4 7 2-3 6 9z" /><path d="M7.5 13l1.5-3 2 3.5" /></svg>);
    case 'flame': return (<svg {...p}><path d="M12 3c1 3-2 4-2 7a3 3 0 1 0 6 0c0-1-.5-2-1-2.5C16 9 18 12 18 15a6 6 0 1 1-12 0c0-4 4-6 6-12z" /></svg>);
    case 'close': return (<svg {...p}><path d="M6 6l12 12M18 6L6 18" /></svg>);
    case 'check': return (<svg {...p}><path d="M5 13l4 4L19 7" /></svg>);
    case 'cross': return (<svg {...p}><path d="M6 6l12 12M18 6L6 18" /></svg>);
    case 'arrow': return (<svg {...p}><path d="M5 12h14M13 6l6 6-6 6" /></svg>);
    case 'back': return (<svg {...p}><path d="M19 12H5M11 6l-6 6 6 6" /></svg>);
    case 'replay': return (<svg {...p}><path d="M3 12a9 9 0 1 0 3-6.7M3 4v4h4" /></svg>);
    case 'bulb': return (<svg {...p}><path d="M9 18h6M10 21h4" /><path d="M12 3a6 6 0 0 0-4 10.5c.6.6 1 1.3 1 2.5h6c0-1.2.4-1.9 1-2.5A6 6 0 0 0 12 3z" /></svg>);
    default: return null;
  }
}

Object.assign(window, { TopicIcon, UIcon });
