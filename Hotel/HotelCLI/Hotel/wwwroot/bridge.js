function postToHost(type, payload) {
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(JSON.stringify({ type: type, payload: payload || {} }));
  }
}

function escapeHtml(value) {
  if (value === null || value === undefined) return '';
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}
