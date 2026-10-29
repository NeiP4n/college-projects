/* ══════════════════════════════════════
   BUILDER.JS — визуальный конструктор
══════════════════════════════════════ */

const API = '/api/builder';

// ── Цветовые пресеты узлов карты ─────────────────────────────────────────────
const NODE_COLORS = [
    '#ffffff', '#ff9900', '#00ff88', '#ff4466',
    '#4488ff', '#ffff00', '#00ffff', '#aa44ff',
    '#ff6644', '#44ffcc',
];

// ── Состояние ─────────────────────────────────────────────────────────────────
const state = {
    courses: [],
    activeCourseId: null,
    modules: [],
    activeModuleId: null,
    activeLessonId: null,
    activeBlockId: null,
    activeEntity: null,   // 'module' | 'lesson' | 'block'
    openModuleIds: new Set(),
    statusTimer: null,
    activeTab: 'content', // 'content' | 'map'
    mapDrag: null,      // текущий drag на карте
    _mapHandlers: null,      // глобальные обработчики drag-and-drop карты
};

// ── API helpers ───────────────────────────────────────────────────────────────
async function api(method, url, body) {
    const res = await fetch(API + url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: body ? JSON.stringify(body) : undefined,
    });
    if (!res.ok) throw new Error(await res.text());
    if (res.status === 204) return null;
    return res.json();
}
const GET = (url) => api('GET', url);
const POST = (url, body) => api('POST', url, body);
const PUT = (url, body) => api('PUT', url, body);
const DELETE = (url) => api('DELETE', url);

// ── Утилиты ───────────────────────────────────────────────────────────────────
function setStatus(msg, cls = '') {
    const el = document.getElementById('bldStatus');
    if (!el) return;
    el.textContent = msg;
    el.className = 'bld-status ' + cls;
    clearTimeout(state.statusTimer);
    if (cls === 'saved') state.statusTimer = setTimeout(() => { el.textContent = ''; }, 3000);
}

function getCourse(id) { return state.courses.find(c => c.id === id); }
function getModule(id) { return state.modules.find(m => m.id === id); }
function getLesson(id) {
    for (const m of state.modules)
        for (const l of m.lessons) if (l.id === id) return l;
    return null;
}
function getBlock(id) {
    const lesson = getLesson(state.activeLessonId);
    return lesson?.blocks?.find(b => b.id === id) ?? null;
}

function previewContent(block) {
    try {
        const c = JSON.parse(block.content);
        switch (block.type) {
            case 'text': return c.markdown || '';
            case 'task': return (c.title || '') + (c.description ? '\n' + c.description : '');
            case 'code': return '[' + (c.language || 'code') + ']\n' + (c.code || '');
            case 'quiz': return c.question || '';
            case 'button': return '[ ' + (c.text || 'button') + ' ]';
            case 'prediction': return '▶ Предсказание\n' + (c.code || '');
            case 'modify': return '✏ Правка: ' + (c.modifyInstruction || '') + '\n' + (c.code || '');
            case 'fill_blank': return '__ Пропуски\n' + (c.template || '');
            case 'drag_order': return '⠿ Порядок\n' + (c.pieces || []).join(' | ');
            case 'fix_bug': return '🐛 Баг\n' + (c.buggyCode || '');
            case 'mini_task': return '▸ ' + (c.title || '') + '\n' + (c.description || '');
            case 'explain': return '💬 Объяснение: ' + (c.prompt || '');
            case 'control_question': return '? ' + (c.question || '');
            case 'control_task': return '✓ Контр. задача: ' + (c.title || '');
            case 'checkpoint': return '⬛ Чекпоинт: ' + (c.title || '');
            case 'knowledge_check': return '📋 Тема: ' + (c.topic || '') + '\n' + (c.question || '');
            case 'transfer_test': return '↗ Перенос: ' + (c.question || '');
            case 'timed_test': return '⏱ ' + (c.timeLimit || 60) + 'с: ' + (c.question || '');
            case 'compare_variants': return '⚖ Сравнение: ' + (c.question || '');
            case 'counterexample': return '✗ Контрпример: ' + (c.claim || '');
            case 'goal_construction': return '🎯 Цель: ' + (c.goal || '');
            case 'template_completion': return '▦ Шаблон\n' + (c.template || '');
            case 'constraint': return '⛔ Ограничение: ' + (c.constraint || '') + '\n' + (c.title || '');
            case 'rebuild_scratch': return '↺ С нуля: ' + (c.title || '');
            case 'counterfactual': return '? Что если убрать строку ' + (c.removeLine || '?') + '\n' + (c.code || '');
            case 'broken_reality': return '💀 Скрытый баг\n' + (c.code || '');
            case 'intent_to_code': return '→ ' + (c.intent || '');
            case 'code_to_intent': return '← Код→смысл\n' + (c.code || '');
            case 'semantic_diff': return '≠ Смысл-диф';
            case 'adversarial_test': return '⚔ Сломай: ' + (c.claim || '');
            case 'two_path': return '⇄ Два пути: ' + (c.question || '');
            case 'progressive_reveal': return '▶▶ По частям: ' + (c.title || '');
            case 'mutation': return '⟳ Мутация: ' + (c.mutation || '');
            case 'challenge_task': return '⚡ ИСПЫТАНИЕ: ' + (c.title || '') + (c.description ? '\n' + c.description : '');
            default: return block.content;
        }
    } catch { return block.content; }
}

function esc(s) {
    return String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

// ── Загрузка данных ───────────────────────────────────────────────────────────
async function load() {
    setStatus('загрузка...', 'saving');
    const courses = await GET('/courses');
    state.courses = courses;
    if (!state.activeCourseId && courses.length) state.activeCourseId = courses[0].id;
    if (state.activeCourseId && !courses.some(c => c.id === state.activeCourseId)) {
        state.activeCourseId = courses[0]?.id ?? null;
    }

    // Обновляем ссылку "Редактор карты" актуальным courseId
    const btnMapEditor = document.getElementById('btnMapEditor');
    if (btnMapEditor && state.activeCourseId)
        btnMapEditor.href = `/Builder/MapEditor?courseId=${state.activeCourseId}`;

    const activeCourse = getCourse(state.activeCourseId);
    const mods = activeCourse?.modules ?? [];
    for (const m of mods) {
        for (const l of m.lessons) l.blocks = [];
    }
    state.modules = mods;
    if (state.activeLessonId) await loadBlocks(state.activeLessonId);
    render();
    setStatus('', '');
}

async function loadBlocks(lessonId) {
    const lesson = getLesson(lessonId);
    if (!lesson) return;
    lesson.blocks = await GET('/lessons/' + lessonId + '/blocks');
}

// ── Переключение вкладок ──────────────────────────────────────────────────────
function switchTab(tab) {
    state.activeTab = tab;

    // Обновляем кнопки вкладок
    document.getElementById('tabContent')?.classList.toggle('active', tab === 'content');
    document.getElementById('tabMap')?.classList.toggle('active', tab === 'map');

    // Палитра видна только в режиме контента
    const palette = document.getElementById('bldPalette');
    if (palette) palette.style.display = tab === 'content' ? '' : 'none';

    // Убираем старые обработчики карты
    if (tab !== 'map') unmountMapHandlers();

    renderCanvas();
}

// ── Главный рендер ────────────────────────────────────────────────────────────
function render() {
    renderSidebar();
    renderCanvas();
    renderProps();
}

// ── Боковая панель ────────────────────────────────────────────────────────────
function renderCoursePicker() {
    if (!state.courses.length) {
        return '<div style="padding:0 0 12px"><button class="bld-btn sm accent" onclick="addCourse()">+ курс</button></div>';
    }

    const items = [...state.courses]
        .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
        .map(c => {
            const active = state.activeCourseId === c.id ? 'active' : '';
            return `<div class="bld-course-chip ${active}" data-cid="${c.id}">
        <span class="bld-module-dot" style="background:${esc(c.color || '#fff')}"></span>
        <span>${esc(c.title)}</span>
        <small>${esc(c.state || '')}</small>
      </div>`;
        }).join('');

    return `<div class="bld-course-picker">
    <div class="bld-course-picker-title">Курсы</div>
    ${items}
    <button class="bld-btn sm accent" onclick="addCourse()">+ курс</button>
  </div>`;
}

function bindCoursePicker(root) {
    root.querySelectorAll('.bld-course-chip').forEach(chip => {
        chip.addEventListener('click', () => selectCourse(+chip.dataset.cid));
    });
}

function renderSidebar() {
    const el = document.getElementById('bldSidebar');
    if (!el) return;

    if (!state.modules.length) {
        el.innerHTML = renderCoursePicker() + '<div class="bld-empty">Нет модулей в выбранном курсе.<br>Создайте первый.</div>';
        bindCoursePicker(el);
        return;
    }

    const sortedMods = [...state.modules].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

    el.innerHTML = renderCoursePicker() + sortedMods.map((m, idx) => {
        const isOpen = state.openModuleIds.has(m.id);
        const isActive = state.activeModuleId === m.id && state.activeEntity === 'module';
        const dot = `<span class="bld-module-dot" style="background:${esc(m.color || '#fff')}"></span>`;

        const lessons = (m.lessons || []).map((l, li) => {
            const la = state.activeLessonId === l.id;
            return `<div class="bld-lesson ${la ? 'active' : ''}" data-lid="${l.id}">
        <span class="bld-lesson-num">${li + 1}.</span>
        <span class="bld-lesson-name">${esc(l.title)}</span>
        <span class="bld-lesson-lock">${l.isLocked ? '🔒' : '🔓'}</span>
      </div>`;
        }).join('') +
            `<div style="padding:4px 0">
       <button class="bld-btn sm" onclick="addLesson(${m.id})">+ урок</button>
     </div>`;

        const challengeClass = m.isChallenge ? ' bld-module--challenge' : '';
        const challengePrefix = m.isChallenge ? '<span style="color:#ffd700;margin-right:3px">⚡</span>' : '';
        return `<div class="bld-module${challengeClass}">
      <div class="bld-module-head ${isOpen ? 'open' : ''} ${isActive ? 'active' : ''}" data-mid="${m.id}">
        <span class="bld-module-arrow">▶</span>
        ${dot}
        <span class="bld-module-num">${idx + 1}</span>
        <span class="bld-module-name">${challengePrefix}${esc(m.title)}</span>
        <span class="bld-module-lock">${m.isLocked ? '🔒' : '🔓'}</span>
      </div>
      <div class="bld-lessons">${lessons}</div>
    </div>`;
    }).join('');

    bindCoursePicker(el);

    el.querySelectorAll('.bld-module-head').forEach(h => {
        h.addEventListener('click', () => {
            const mid = +h.dataset.mid;
            if (state.openModuleIds.has(mid)) state.openModuleIds.delete(mid);
            else state.openModuleIds.add(mid);
            selectModule(mid);
        });
    });

    el.querySelectorAll('.bld-lesson').forEach(l => {
        l.addEventListener('click', e => {
            e.stopPropagation();
            selectLesson(+l.dataset.lid);
        });
    });
}

// ── Канвас (контент или карта) ────────────────────────────────────────────────
function renderCanvas() {
    if (state.activeTab === 'map') {
        renderMapEditor();
    } else {
        renderContentCanvas();
    }
}

// ── Контент урока ─────────────────────────────────────────────────────────────
function renderContentCanvas() {
    const el = document.getElementById('bldCanvas');
    if (!el) return;

    el.className = 'bld-canvas-area bld-scroll';

    if (!state.activeLessonId) {
        el.innerHTML = '<div class="bld-empty">Выберите урок<br>в боковой панели.</div>';
        return;
    }

    const lesson = getLesson(state.activeLessonId);
    const blocks = lesson?.blocks ?? [];

    el.innerHTML = blocks.length
        ? blocks.map(b => {
            const sel = state.activeBlockId === b.id ? 'selected' : '';
            return `<div class="bld-block ${sel}" draggable="true" data-bid="${b.id}">
          <div class="bld-block-head">
            <span class="bld-block-drag" title="перетащить">⠿</span>
            <span class="bld-block-type">${b.type}</span>
            <button class="bld-block-del" onclick="deleteBlock(${b.id})" title="удалить">✕</button>
          </div>
          <div class="bld-block-preview">${esc(previewContent(b))}</div>
        </div>`;
        }).join('')
        : '<div class="bld-empty">Добавьте блок из палитры.</div>';

    el.querySelectorAll('.bld-block').forEach(b => {
        b.addEventListener('click', e => {
            if (e.target.classList.contains('bld-block-del')) return;
            selectBlock(+b.dataset.bid);
        });
    });

    initDragBlocks(el, blocks);
}

// ── Редактор карты ────────────────────────────────────────────────────────────
// ── Расширение карты ВВЕРХ ───────────────────────────────────────────────────
// Храним высоту верхнего расширителя в px, независимо от основной карты.
const MAP_TOP_EXT_KEY = 'bld_map_top_ext_px';
const MAP_TOP_EXT_STEP = 200;   // px за один шаг
const MAP_TOP_EXT_MIN = 0;
const MAP_TOP_EXT_MAX = 4000;

function getMapTopExt() {
    const v = parseInt(localStorage.getItem(MAP_TOP_EXT_KEY), 10);
    return isNaN(v) ? 0 : Math.max(MAP_TOP_EXT_MIN, Math.min(MAP_TOP_EXT_MAX, v));
}

function applyMapTopExt(px) {
    px = Math.max(MAP_TOP_EXT_MIN, Math.min(MAP_TOP_EXT_MAX, px));
    localStorage.setItem(MAP_TOP_EXT_KEY, px);
    const ext = document.getElementById('bldMapTopExt');
    if (ext) ext.style.height = px + 'px';
    const label = document.getElementById('bldMapExtLabel');
    if (label) label.textContent = px === 0 ? '0' : '+' + px + 'px';
    // Держим скролл внизу — чтобы основная карта оставалась видна
    const wrap = document.getElementById('bldMapWrap');
    if (wrap) wrap.scrollTop = wrap.scrollHeight;
}

function mapExtUp() { applyMapTopExt(getMapTopExt() + MAP_TOP_EXT_STEP); }
function mapExtDown() { applyMapTopExt(getMapTopExt() - MAP_TOP_EXT_STEP); }
function mapExtReset() { applyMapTopExt(0); }


// Минимальный top (%) с учётом расширения карты вверх
function mapMinTopPct() {
    const extPx = getMapTopExt();
    return -(extPx / 1280 * 100);
}

function renderMapEditor() {
    const el = document.getElementById('bldCanvas');
    if (!el) return;

    el.className = 'bld-canvas-area bld-map-area';

    const sortedMapMods = [...state.modules].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
    const nodes = sortedMapMods.map((m, idx) => {
        const sel = (state.activeModuleId === m.id && state.activeEntity === 'module') ? 'selected' : '';
        const lck = m.isLocked ? 'locked' : '';
        const chl = m.isChallenge ? 'challenge' : '';
        const defaultColor = m.isChallenge ? '#ffd700' : '#fff';
        const c = m.color || defaultColor;

        const icon = m.isChallenge ? `
          <div class="bld-map-node-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48" shape-rendering="crispEdges" fill="none" width="44" height="44">
              <rect fill="${c}" x="18" y="42" width="12" height="4"/>
              <rect fill="${c}" x="14" y="40" width="20" height="2"/>
              <rect fill="${c}" opacity="0.80" x="10" y="16" width="28" height="22"/>
              <rect fill="#000" opacity="0.50" x="12" y="18" width="24" height="18"/>
              <rect fill="#fff" opacity="0.20" x="12" y="18" width="4" height="18"/>
              <rect fill="#fff" opacity="0.20" x="12" y="18" width="24" height="3"/>
              <rect fill="${c}" opacity="0.80" x="6"  y="18" width="4" height="12"/>
              <rect fill="${c}" opacity="0.80" x="38" y="18" width="4" height="12"/>
              <rect fill="#000" opacity="0.50" x="8"  y="20" width="2" height="8"/>
              <rect fill="${c}" x="8" y="14" width="32" height="2"/>
              <rect fill="#fff" opacity="0.90" x="22" y="22" width="4" height="4"/>
              <rect fill="#fff" opacity="0.90" x="20" y="24" width="8" height="2"/>
              <rect fill="#fff" opacity="0.90" x="18" y="26" width="12" height="2"/>
              <rect fill="#fff" opacity="0.90" x="20" y="28" width="8" height="2"/>
              <rect fill="#fff" opacity="0.90" x="22" y="30" width="4" height="2"/>
            </svg>
          </div>` : `
          <div class="bld-map-node-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 40" shape-rendering="crispEdges" fill="none" width="52" height="44">
              <rect fill="${c}" x="2"  y="2"  width="14" height="4"/>
              <rect fill="${c}" x="16" y="4"  width="2"  height="2"/>
              <rect fill="${c}" opacity="0.85" x="2" y="6" width="44" height="30"/>
              <rect fill="#000" opacity="0.55" x="4" y="8" width="40" height="26"/>
              <rect fill="${c}" opacity="0.18" x="4" y="8" width="40" height="26"/>
              <rect fill="#fff" opacity="0.15" x="4" y="8" width="40" height="3"/>
              <rect fill="#fff" opacity="0.15" x="4" y="11" width="2"  height="23"/>
              <rect fill="${c}" opacity="0.85" x="4"  y="8"  width="2" height="2"/>
              <rect fill="${c}" opacity="0.85" x="42" y="8"  width="2" height="2"/>
              <rect fill="${c}" opacity="0.85" x="4"  y="32" width="2" height="2"/>
              <rect fill="${c}" opacity="0.85" x="42" y="32" width="2" height="2"/>
              <rect fill="#000" opacity="0.35" x="4"  y="33" width="40" height="2"/>
              <rect fill="#000" opacity="0.35" x="43" y="9"  width="2"  height="24"/>
              <rect fill="${c}" opacity="0.85" x="22" y="20" width="4" height="2"/>
              <rect fill="${c}" opacity="0.85" x="21" y="22" width="6" height="3"/>
            </svg>
          </div>`;

        return `<div class="bld-map-node ${sel} ${lck} ${chl}"
                 data-mid="${m.id}"
                 style="left:${m.mapX ?? 50}%;top:${m.mapY ?? 50}%;--node-color:${esc(c)}">
              <div class="bld-map-node-badge">${m.isChallenge ? 'ИСПЫТАНИЕ' : idx + 1}</div>
              ${icon}
              <div class="bld-map-node-name">${esc(m.title)}</div>
            </div>`;
    }).join('');

    const emptyHint = state.modules.length === 0
        ? `<div class="bld-map-empty">Нет модулей.<br>Нажми кнопку ниже.</div>`
        : '';

    // Внешний враппер — скролл-контейнер
    // Внутренний bldMapInner — точно такие же пропорции как .levels-main (760×1280)
    // Узлы позиционируются внутри него — те же % что и в игре
    el.innerHTML = `
    <div class="bld-map-wrap" id="bldMapWrap">
      <div class="bld-map-top-ext" id="bldMapTopExt"></div>
      <div class="bld-map-inner" id="bldMapInner">

        <!-- Фон -->
        <div class="bld-map-bg"></div>

        <!-- Декор -->
        <img class="bld-map-decor bld-map-mountain-l" src="/img/main/decor/mountain.svg" draggable="false">
        <img class="bld-map-decor bld-map-mountain-r" src="/img/main/decor/mountain.svg" draggable="false">
        <img class="bld-map-decor bld-map-castle-l"   src="/img/main/decor/castle.svg"   draggable="false">
        <img class="bld-map-decor bld-map-castle-r"   src="/img/main/decor/castle.svg"   draggable="false">

        <!-- Дорожка уровней -->
        <img class="bld-map-levels-bg" src="/img/main/levels/levels-background.svg" draggable="false">

        <!-- Узлы модулей -->
        ${nodes}
        ${emptyHint}

        <!-- Подсказка и кнопка -->
        <div class="bld-map-hint">Перетащи узел · клик = выбрать</div>
        <button class="bld-map-add-btn" onclick="addModule()">+ Добавить модуль</button>
      </div>
    </div>`;

    // Кнопки управления расширением вверх
    const wrap = document.getElementById('bldMapWrap');
    if (wrap) {
        const bar = document.createElement('div');
        bar.className = 'bld-map-ext-bar';
        bar.id = 'bldMapExtBar';
        bar.innerHTML = `
                <button class="bld-btn sm" onclick="mapExtDown()" title="Убрать пространство сверху">−</button>
                <span class="bld-map-ext-label" id="bldMapExtLabel">0</span>
                <button class="bld-btn sm" onclick="mapExtUp()" title="Добавить пространство сверху карты">+ вверх</button>
                <button class="bld-btn sm" onclick="mapExtReset()" style="color:var(--text3);font-size:8px" title="Сбросить">сброс</button>
            `;
        // Вставить BAR перед bld-map-wrap (в bld-canvas-area)
        const canvas = document.getElementById('bldCanvas');
        canvas.insertBefore(bar, wrap);
    }

    applyMapTopExt(getMapTopExt());

    // После рендера прокручиваем враппер вниз —
    // основная карта видна сразу, пользователь скроллит вверх в доп. пространство
    const _wrap = document.getElementById('bldMapWrap');
    if (_wrap) setTimeout(() => { _wrap.scrollTop = _wrap.scrollHeight; }, 0);

    mountMapHandlers();

    if (window.MapObjectsManager && state.activeCourseId) {
        MapObjectsManager.init(state.activeCourseId, '#bldMapInner');
    }
}

// ── Обработчики drag на карте ─────────────────────────────────────────────────
function mountMapHandlers() {
    unmountMapHandlers(); // сначала снимаем старые

    const move = e => onMapMove(e);
    const up = e => onMapUp(e);
    document.addEventListener('mousemove', move);
    document.addEventListener('mouseup', up);
    state._mapHandlers = { move, up };

    // mousedown на узлах — позиция считается внутри bldMapInner (реальные пропорции карты)
    const inner = document.getElementById('bldMapInner');
    if (!inner) return;
    inner.querySelectorAll('.bld-map-node').forEach(node => {
        node.addEventListener('mousedown', e => {
            e.preventDefault();
            state.mapDrag = {
                node,
                wrap: inner,           // расчёт % относительно inner
                id: +node.dataset.mid,
                startX: e.clientX,
                startY: e.clientY,
                origLeft: parseFloat(node.style.left),
                origTop: parseFloat(node.style.top),
                hasMoved: false,
            };
            node.classList.add('dragging');
        });
    });
}

function unmountMapHandlers() {
    if (!state._mapHandlers) return;
    document.removeEventListener('mousemove', state._mapHandlers.move);
    document.removeEventListener('mouseup', state._mapHandlers.up);
    state._mapHandlers = null;
    state.mapDrag = null;
}

function onMapMove(e) {
    if (!state.mapDrag) return;
    const { node, wrap, startX, startY, origLeft, origTop } = state.mapDrag;
    const rect = wrap.getBoundingClientRect();
    const dx = (e.clientX - startX) / rect.width * 100;
    const dy = (e.clientY - startY) / rect.height * 100;
    if (Math.abs(dx) > 0.3 || Math.abs(dy) > 0.3) state.mapDrag.hasMoved = true;
    node.style.left = Math.max(2, Math.min(96, origLeft + dx)) + '%';
    node.style.top = Math.max(mapMinTopPct(), Math.min(96, origTop + dy)) + '%';
}

async function onMapUp() {
    if (!state.mapDrag) return;
    const { node, id, hasMoved } = state.mapDrag;
    node.classList.remove('dragging');

    if (!hasMoved) {
        // клик без движения → выбор модуля
        state.mapDrag = null;
        selectModule(id);
        return;
    }

    const newLeft = parseFloat(node.style.left);
    const newTop = parseFloat(node.style.top);
    state.mapDrag = null;

    // обновляем state
    const mod = getModule(id);
    if (mod) { mod.mapX = newLeft; mod.mapY = newTop; }

    // обновляем поля в props (если открыты)
    const pX = document.getElementById('pModX');
    const pY = document.getElementById('pModY');
    if (pX && state.activeModuleId === id) { pX.value = newLeft.toFixed(1); }
    if (pY && state.activeModuleId === id) { pY.value = newTop.toFixed(1); }

    try {
        setStatus('сохранение...', 'saving');
        await PUT('/modules/' + id + '/map', { mapX: newLeft, mapY: newTop });
        setStatus('сохранено', 'saved');
    } catch { setStatus('ошибка!', ''); }
}

// ── Панель свойств ────────────────────────────────────────────────────────────
function renderProps() {
    const el = document.getElementById('bldProps');
    if (!el) return;

    if (state.activeEntity === 'block' && state.activeBlockId) {
        const block = getBlock(state.activeBlockId);
        if (block) { el.innerHTML = renderBlockEditor(block); bindPropEvents(block); return; }
    }
    if (state.activeEntity === 'lesson' && state.activeLessonId) {
        const lesson = getLesson(state.activeLessonId);
        if (lesson) { el.innerHTML = renderLessonEditor(lesson); bindLessonEvents(lesson); return; }
    }
    if (state.activeEntity === 'module' && state.activeModuleId) {
        const mod = getModule(state.activeModuleId);
        if (mod) { el.innerHTML = renderModuleEditor(mod); bindModuleEvents(mod); return; }
    }
    if (state.activeEntity === 'course' && state.activeCourseId) {
        const course = getCourse(state.activeCourseId);
        if (course) { el.innerHTML = renderCourseEditor(course); bindCourseEvents(course); return; }
    }

    el.innerHTML = '<div class="bld-props-empty">Выберите элемент<br>для редактирования.</div>';
}

// ── Редакторы свойств ─────────────────────────────────────────────────────────
function renderCourseEditor(c) {
    return `<div class="bld-scroll">
    <div class="bld-form-group">
      <label class="bld-label">Название курса</label>
      <input class="bld-input" id="pCourseTitle" value="${esc(c.title)}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Slug для URL</label>
      <input class="bld-input" id="pCourseSlug" value="${esc(c.slug)}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Описание</label>
      <textarea class="bld-textarea" id="pCourseDesc">${esc(c.description || '')}</textarea>
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Трек</label>
      <input class="bld-input" id="pCourseTrack" value="${esc(c.track || 'BACKEND')}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Теги</label>
      <input class="bld-input" id="pCourseTag" value="${esc(c.tag || '')}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Категории фильтра</label>
      <input class="bld-input" id="pCourseCats" value="${esc(c.categories || 'backend')}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Состояние</label>
      <select class="bld-select" id="pCourseState">
        ${['available', 'soon', 'locked'].map(s => `<option value="${s}" ${c.state === s ? 'selected' : ''}>${s}</option>`).join('')}
      </select>
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Badge</label>
      <input class="bld-input" id="pCourseBadge" value="${esc(c.badge || 'START')}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Цвет</label>
      <input type="color" id="pCourseColor" value="${esc(c.color || '#ffffff')}" style="width:48px;height:32px;border:2px solid var(--border);background:none">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Оценка часов</label>
      <input class="bld-input" id="pCourseHours" type="number" min="1" value="${c.estimatedHours ?? 6}">
    </div>
    <div class="bld-form-group bld-checkbox-row">
      <input type="checkbox" id="pCoursePublished" ${c.isPublished ? 'checked' : ''}>
      <label for="pCoursePublished" class="bld-label" style="margin:0">Показывать на странице курсов</label>
    </div>
    <div class="bld-divider"></div>
    <button class="bld-btn accent" id="pCourseSave">Сохранить</button>
    &nbsp;
    <button class="bld-btn danger" id="pCourseDel">Удалить курс</button>
  </div>`;
}

function renderModuleEditor(m) {
    const colorSwatches = NODE_COLORS.map(c => {
        const active = (m.color || '#ffffff').toLowerCase() === c ? 'active' : '';
        return `<div class="bld-color-swatch ${active}" data-color="${c}"
                 style="background:${c}"
                 title="${c}"></div>`;
    }).join('');

    return `<div class="bld-scroll">
    <div class="bld-form-group">
      <label class="bld-label">Название модуля</label>
      <input class="bld-input" id="pModTitle" value="${esc(m.title)}">
    </div>
    <div class="bld-form-group">
      <label class="bld-label">Описание</label>
      <textarea class="bld-textarea" id="pModDesc">${esc(m.description)}</textarea>
    </div>
    <div class="bld-form-group bld-checkbox-row">
      <input type="checkbox" id="pModLock" ${m.isLocked ? 'checked' : ''}>
      <label for="pModLock" class="bld-label" style="margin:0">Заблокирован</label>
    </div>
    <div class="bld-form-group bld-checkbox-row">
      <input type="checkbox" id="pModChallenge" ${m.isChallenge ? 'checked' : ''}>
      <label for="pModChallenge" class="bld-label" style="margin:0;color:#ffd700">⚡ Контрольная задача (испытание)</label>
    </div>
    <div class="bld-divider"></div>
    <div class="bld-form-group">
      <label class="bld-label">Цвет узла на карте</label>
      <div style="display:flex;gap:8px;align-items:center;margin-bottom:8px">
        <input type="color" id="pModColor" value="${esc(m.color || '#ffffff')}"
               style="width:36px;height:28px;border:2px solid var(--border);background:none;cursor:pointer;padding:2px">
        <span class="bld-label" style="margin:0">${esc(m.color || '#ffffff')}</span>
      </div>
      <div class="bld-color-swatches" id="pModSwatches">${colorSwatches}</div>
    </div>
    <div class="bld-divider"></div>
    <div class="bld-form-group">
      <label class="bld-label">Размер шрифта метки (px)</label>
      <div style="display:flex;gap:8px;align-items:center">
        <input class="bld-input" id="pModLabelSize" type="number" min="6" max="32" step="1"
               value="${m.labelFontSize ?? 9}" style="width:80px">
        <span class="bld-label" style="margin:0;font-size:7px">пр: 9 — маленький, 14 — средний, 20 — крупный</span>
      </div>
    </div>
    <div class="bld-divider"></div>
    <div class="bld-form-group">
      <label class="bld-label">Позиция на карте</label>
      <div style="display:flex;gap:8px">
        <div style="flex:1">
          <label class="bld-label" style="font-size:6px">X (лево %)</label>
          <input class="bld-input" id="pModX" type="number" min="0" max="100" step="0.5"
                 value="${(m.mapX ?? 50).toFixed(1)}">
        </div>
        <div style="flex:1">
          <label class="bld-label" style="font-size:6px">Y (верх %)</label>
          <input class="bld-input" id="pModY" type="number" min="0" max="100" step="0.5"
                 value="${(m.mapY ?? 50).toFixed(1)}">
        </div>
      </div>
    </div>
    <div class="bld-divider"></div>
    <button class="bld-btn accent" id="pModSave">Сохранить</button>
    &nbsp;
    <button class="bld-btn danger" id="pModDel">Удалить модуль</button>
  </div>`;
}

function renderLessonEditor(l) {
    return `<div class="bld-scroll">
    <div class="bld-form-group">
      <label class="bld-label">Название урока</label>
      <input class="bld-input" id="pLesTitle" value="${esc(l.title)}">
    </div>
    <div class="bld-form-group bld-checkbox-row">
      <input type="checkbox" id="pLesLock" ${l.isLocked ? 'checked' : ''}>
      <label for="pLesLock" class="bld-label" style="margin:0">Заблокирован</label>
    </div>
    <div class="bld-divider"></div>
    <button class="bld-btn accent" id="pLesSave">Сохранить</button>
    &nbsp;
    <button class="bld-btn danger" id="pLesDel">Удалить урок</button>
  </div>`;
}

function renderBlockEditor(block) {
    let fields = '';
    try {
        const c = JSON.parse(block.content);
        switch (block.type) {
            case 'text':
                fields = `<div class="bld-form-group">
          <label class="bld-label">Текст (markdown)</label>
          <textarea class="bld-textarea" id="pBlkMd" style="min-height:160px">${esc(c.markdown || '')}</textarea>
        </div>`;
                break;
            case 'task':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Заголовок задания</label>
            <input class="bld-input" id="pBlkTaskTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание</label>
            <textarea class="bld-textarea" id="pBlkTaskDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkTaskLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Стартовый код</label>
            <textarea class="bld-textarea" id="pBlkTaskCode" style="font-family:monospace;min-height:120px">${esc(c.starterCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод (точное совпадение)</label>
            <input class="bld-input" id="pBlkTaskOut" value="${esc(c.expectedOutput || '')}" placeholder="5  или  Hello World">
          </div>`;
                break;
            case 'code':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык</label>
            <select class="bld-select" id="pBlkCodeLang">
              ${['csharp', 'javascript', 'python', 'sql', 'bash'].map(l =>
                    `<option value="${l}" ${c.language === l ? 'selected' : ''}>${l}</option>`).join('')}
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок</label>
            <input class="bld-input" id="pBlkCodeTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Код</label>
            <textarea class="bld-textarea" id="pBlkCodeBody" style="font-family:monospace;min-height:140px">${esc(c.code || '')}</textarea>
          </div>`;
                break;
            case 'quiz':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос</label>
            <textarea class="bld-textarea" id="pBlkQ">${esc(c.question || '')}</textarea>
          </div>
          ${[0, 1, 2, 3].map(i => `
          <div class="bld-form-group">
            <label class="bld-label">Вариант ${['A', 'B', 'C', 'D'][i]}</label>
            <input class="bld-input" id="pBlkOpt${i}" value="${esc((c.options || [])[i] || '')}">
          </div>`).join('')}
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ</label>
            <select class="bld-select" id="pBlkCorrect">
              ${['A', 'B', 'C', 'D'].map((l, i) => `<option value="${i + 1}" ${(c.correct ?? 0) === i ? 'selected' : ''}>${l} — Вариант ${i + 1}</option>`).join('')}
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение</label>
            <textarea class="bld-textarea" id="pBlkExpl">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;
            case 'button':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Текст кнопки</label>
            <input class="bld-input" id="pBlkBtnText" value="${esc(c.text || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ссылка (href)</label>
            <input class="bld-input" id="pBlkBtnHref" value="${esc(c.href || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Вариант</label>
            <select class="bld-select" id="pBlkBtnVariant">
              <option value="primary"   ${c.variant === 'primary' ? 'selected' : ''}>primary</option>
              <option value="secondary" ${c.variant === 'secondary' ? 'selected' : ''}>secondary</option>
            </select>
          </div>`;
                break;

            // ── ПРЕДСКАЗАНИЕ ──────────────────────────────────────────────────────
            case 'prediction':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Код (показывается студенту)</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:120px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка (опционально)</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'modify':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Исходный код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Инструкция (что изменить)</label>
            <textarea class="bld-textarea" id="pBlkInstruction">${esc(c.modifyInstruction || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'counterfactual':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Номер убираемой строки</label>
            <input class="bld-input" id="pBlkRemoveLine" type="number" min="1" value="${c.removeLine || 1}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Вопрос ("Что если убрать строку N?")</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ</label>
            <textarea class="bld-textarea" id="pBlkAnswer">${esc(c.answer || '')}</textarea>
          </div>`;
                break;

            case 'broken_reality':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Код (с багом на редком кейсе)</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:120px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Входные данные, где баг проявляется</label>
            <input class="bld-input" id="pBlkEdgeCase" value="${esc(c.edgeCaseInput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание бага (для преподавателя)</label>
            <textarea class="bld-textarea" id="pBlkBugDesc">${esc(c.bugDescription || '')}</textarea>
          </div>`;
                break;

            case 'semantic_diff':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Что хотел написать (намерение)</label>
            <textarea class="bld-textarea" id="pBlkIntended" style="font-family:monospace">${esc(c.intendedCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Что написал на самом деле</label>
            <textarea class="bld-textarea" id="pBlkActual" style="font-family:monospace">${esc(c.actualCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение расхождения</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            case 'mutation':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Базовый код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.baseCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Мутация (что изменилось)</label>
            <textarea class="bld-textarea" id="pBlkMutation">${esc(c.mutation || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод после адаптации</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            // ── КОНСТРУИРОВАНИЕ ───────────────────────────────────────────────────
            case 'fill_blank':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Шаблон (используй ___ для пропусков)</label>
            <textarea class="bld-textarea" id="pBlkTemplate" style="font-family:monospace;min-height:120px">${esc(c.template || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильные ответы (по одному на строку, в порядке пропусков)</label>
            <textarea class="bld-textarea" id="pBlkBlanks">${esc((c.blanks || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'drag_order':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Кусочки кода (каждый на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkPieces" style="font-family:monospace;min-height:120px">${esc((c.pieces || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный порядок (индексы через запятую, с 0)</label>
            <input class="bld-input" id="pBlkSolution" value="${esc((c.solution || []).join(','))}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'goal_construction':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Цель (что должна сделать программа)</label>
            <textarea class="bld-textarea" id="pBlkGoal">${esc(c.goal || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <textarea class="bld-textarea" id="pBlkExpected" style="font-family:monospace">${esc(c.expectedOutput || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'template_completion':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Шаблон-каркас (с ___ в местах пропусков)</label>
            <textarea class="bld-textarea" id="pBlkTemplate" style="font-family:monospace;min-height:120px">${esc(c.template || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильные вставки (по одной на строку)</label>
            <textarea class="bld-textarea" id="pBlkBlanks">${esc((c.blanks || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'fix_bug':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Код с ошибкой</label>
            <textarea class="bld-textarea" id="pBlkBuggyCode" style="font-family:monospace;min-height:120px">${esc(c.buggyCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод (правильный)</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'mini_task':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание (1 шаг)</label>
            <textarea class="bld-textarea" id="pBlkDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Стартовый код</label>
            <textarea class="bld-textarea" id="pBlkStarterCode" style="font-family:monospace">${esc(c.starterCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>`;
                break;

            case 'constraint':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок задачи</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание</label>
            <textarea class="bld-textarea" id="pBlkDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ограничение (например: без if, за один проход)</label>
            <input class="bld-input" id="pBlkConstraint" value="${esc(c.constraint || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Стартовый код</label>
            <textarea class="bld-textarea" id="pBlkStarterCode" style="font-family:monospace">${esc(c.starterCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'rebuild_scratch':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание задачи</label>
            <textarea class="bld-textarea" id="pBlkDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'progressive_reveal':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Заголовок</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказки (каждая на новой строке, раскрываются по одной)</label>
            <textarea class="bld-textarea" id="pBlkHints" style="min-height:100px">${esc((c.hints || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Финальный код (показывается после всех подсказок)</label>
            <textarea class="bld-textarea" id="pBlkFinalCode" style="font-family:monospace;min-height:100px">${esc(c.finalCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>`;
                break;

            // ── ОБЪЯСНЕНИЕ ─────────────────────────────────────────────────────────
            case 'explain':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос / запрос</label>
            <textarea class="bld-textarea" id="pBlkPrompt">${esc(c.prompt || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Код (который нужно объяснить)</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.codeSnippet || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Эталонное объяснение (для проверки)</label>
            <textarea class="bld-textarea" id="pBlkCorrect">${esc(c.correctExplanation || '')}</textarea>
          </div>`;
                break;

            case 'intent_to_code':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Намерение (на обычном языке)</label>
            <textarea class="bld-textarea" id="pBlkIntent">${esc(c.intent || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <textarea class="bld-textarea" id="pBlkExpected" style="font-family:monospace">${esc(c.expectedOutput || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'code_to_intent':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Варианты смысла (каждый на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkOptions">${esc((c.options || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный вариант</label>
            <select class="bld-select" id="pBlkCorrect">
              ${(c.options || ['', '', '']).map((o, i) => `<option value="${i + 1}" ${(c.correctIntent ?? 0) === i ? 'selected' : ''}>${i + 1}. ${esc(o || 'Вариант ' + (i + 1))}</option>`).join('')}
            </select>
          </div>`;
                break;

            // ── ИСПЫТАНИЕ ─────────────────────────────────────────────────────────
            case 'challenge_task':
                fields = `
          <div style="background:rgba(255,215,0,0.06);border:1px solid #ffd700;padding:8px 10px;margin-bottom:12px;font-size:7px;color:#ffd700;letter-spacing:1px">
            ⚡ Контрольная задача — студент пишет программу с нуля
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value="csharp" ${(!c.language || c.language === 'csharp') ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок испытания</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание (что требуется сделать)</label>
            <textarea class="bld-textarea" id="pBlkDesc" style="min-height:100px">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Требования (каждое на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkReqs" style="min-height:80px">${esc((c.requirements || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Стартовый код (опционально)</label>
            <textarea class="bld-textarea" id="pBlkStarterCode" style="font-family:monospace;min-height:80px">${esc(c.starterCode || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод (точное совпадение)</label>
            <textarea class="bld-textarea" id="pBlkExpected" style="font-family:monospace">${esc(c.expectedOutput || '')}</textarea>
          </div>`;
                break;

            // ── КОНТРОЛЬ ───────────────────────────────────────────────────────────
            case 'control_question':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос (без подсказок)</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ</label>
            <textarea class="bld-textarea" id="pBlkAnswer">${esc(c.answer || '')}</textarea>
          </div>`;
                break;

            case 'control_task':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Язык компилятора</label>
            <select class="bld-select" id="pBlkLang">
              <option value=""       ${!c.language ? 'selected' : ''}>— текстовый ответ —</option>
              <option value="csharp" ${c.language === 'csharp' ? 'selected' : ''}>C#</option>
              <option value="python" ${c.language === 'python' ? 'selected' : ''}>Python</option>
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Заголовок</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание задачи</label>
            <textarea class="bld-textarea" id="pBlkDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый вывод</label>
            <textarea class="bld-textarea" id="pBlkExpected" style="font-family:monospace">${esc(c.expectedOutput || '')}</textarea>
          </div>`;
                break;

            case 'checkpoint':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Название чекпоинта</label>
            <input class="bld-input" id="pBlkTitle" value="${esc(c.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Описание</label>
            <textarea class="bld-textarea" id="pBlkDesc">${esc(c.description || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Задания чекпоинта (каждое на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkTasks" style="min-height:100px">${esc((c.tasks || []).join('\n'))}</textarea>
          </div>`;
                break;

            case 'knowledge_check':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Тема</label>
            <input class="bld-input" id="pBlkTopic" value="${esc(c.topic || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Вопрос</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          ${[0, 1, 2, 3].map(i => `
          <div class="bld-form-group">
            <label class="bld-label">Вариант ${['A', 'B', 'C', 'D'][i]}</label>
            <input class="bld-input" id="pBlkOpt${i}" value="${esc((c.options || [])[i] || '')}">
          </div>`).join('')}
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ</label>
            <select class="bld-select" id="pBlkCorrect">
              ${['A', 'B', 'C', 'D'].map((l, i) => `<option value="${i + 1}" ${(c.correct ?? 0) === i ? 'selected' : ''}>${l} — Вариант ${i + 1}</option>`).join('')}
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            case 'transfer_test':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос (та же логика, другой контекст)</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          ${[0, 1, 2].map(i => `
          <div class="bld-form-group">
            <label class="bld-label">Вариант ${['A', 'B', 'C'][i]}</label>
            <input class="bld-input" id="pBlkOpt${i}" value="${esc((c.options || [])[i] || '')}">
          </div>`).join('')}
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ</label>
            <select class="bld-select" id="pBlkCorrect">
              ${['A', 'B', 'C'].map((l, i) => `<option value="${i + 1}" ${(c.correct ?? 0) === i ? 'selected' : ''}>${l} — Вариант ${i + 1}</option>`).join('')}
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            case 'timed_test':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Лимит времени (секунды)</label>
            <input class="bld-input" id="pBlkTimeLimit" type="number" min="5" max="300" value="${c.timeLimit || 30}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ожидаемый ответ</label>
            <input class="bld-input" id="pBlkExpected" value="${esc(c.expectedOutput || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка (показывается после истечения времени)</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            // ── АНАЛИЗ ─────────────────────────────────────────────────────────────
            case 'compare_variants':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          ${(c.variants || [{ title: '', code: '' }, { title: '', code: '' }]).map((v, i) => `
          <div class="bld-form-group">
            <label class="bld-label">Вариант ${String.fromCharCode(65 + i)} — заголовок</label>
            <input class="bld-input" id="pBlkVTitle${i}" value="${esc(v.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Вариант ${String.fromCharCode(65 + i)} — код</label>
            <textarea class="bld-textarea" id="pBlkVCode${i}" style="font-family:monospace">${esc(v.code || '')}</textarea>
          </div>`).join('')}
          <div class="bld-form-group">
            <label class="bld-label">Лучший вариант</label>
            <select class="bld-select" id="pBlkCorrect">
              ${(c.variants || [{ title: 'A' }, { title: 'B' }]).map((v, i) => `<option value="${i + 1}" ${(c.correct ?? 0) === i ? 'selected' : ''}>${String.fromCharCode(65 + i)} — ${esc(v.title || 'Вариант ' + (i + 1))}</option>`).join('')}
            </select>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            case 'counterexample':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Утверждение об алгоритме</label>
            <textarea class="bld-textarea" id="pBlkClaim">${esc(c.claim || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный вход, ломающий утверждение</label>
            <input class="bld-input" id="pBlkAnswer" value="${esc(c.answer || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            case 'adversarial_test':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Код</label>
            <textarea class="bld-textarea" id="pBlkCode" style="font-family:monospace;min-height:100px">${esc(c.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Утверждение о корректности</label>
            <textarea class="bld-textarea" id="pBlkClaim">${esc(c.claim || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'two_path':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос для сравнения</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Путь A — заголовок</label>
            <input class="bld-input" id="pBlkPathATitle" value="${esc(c.pathA?.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Путь A — код</label>
            <textarea class="bld-textarea" id="pBlkPathACode" style="font-family:monospace">${esc(c.pathA?.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Путь B — заголовок</label>
            <input class="bld-input" id="pBlkPathBTitle" value="${esc(c.pathB?.title || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Путь B — код</label>
            <textarea class="bld-textarea" id="pBlkPathBCode" style="font-family:monospace">${esc(c.pathB?.code || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Объяснение (итог сравнения)</label>
            <textarea class="bld-textarea" id="pBlkExplanation">${esc(c.explanation || '')}</textarea>
          </div>`;
                break;

            // ── ДОПОЛНИТЕЛЬНЫЕ ────────────────────────────────────────────────────────
            case 'short_answer':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Вопрос</label>
            <textarea class="bld-textarea" id="pBlkQuestion">${esc(c.question || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Правильный ответ (точный текст)</label>
            <input class="bld-input" id="pBlkAnswer" value="${esc(c.answer || '')}">
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Подсказка (необязательно)</label>
            <input class="bld-input" id="pBlkHint" value="${esc(c.hint || '')}">
          </div>`;
                break;

            case 'classify':
                fields = `
          <div class="bld-form-group">
            <label class="bld-label">Задание</label>
            <textarea class="bld-textarea" id="pBlkPrompt">${esc(c.prompt || '')}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Категории (каждая на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkCategories">${esc((c.categories || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Элементы для сортировки (каждый на новой строке)</label>
            <textarea class="bld-textarea" id="pBlkItems">${esc((c.items || []).join('\n'))}</textarea>
          </div>
          <div class="bld-form-group">
            <label class="bld-label">Ответ (JSON: {"Категория":["эл1","эл2"]})</label>
            <textarea class="bld-textarea" id="pBlkAnswer" style="font-family:monospace">${esc(typeof c.answer === 'string' ? c.answer : JSON.stringify(c.answer || {}))}</textarea>
          </div>`;
                break;
        }
    } catch {
        fields = `<div class="bld-form-group">
      <label class="bld-label">Content (JSON)</label>
      <textarea class="bld-textarea" id="pBlkRaw">${esc(block.content)}</textarea>
    </div>`;
    }

    return `<div class="bld-scroll">
    <div class="bld-panel-title" style="padding:10px 0 8px;border:none">
      ТИП: <span style="color:var(--text)">${block.type.toUpperCase()}</span>
    </div>
    ${fields}
    <div class="bld-divider"></div>
    <button class="bld-btn accent" id="pBlkSave">Сохранить</button>
  </div>`;
}

// ── Bind events ───────────────────────────────────────────────────────────────
function bindCourseEvents(c) {
    document.getElementById('pCourseSave')?.addEventListener('click', async () => {
        setStatus('сохранение...', 'saving');
        try {
            await PUT('/courses/' + c.id, {
                title: document.getElementById('pCourseTitle').value,
                slug: document.getElementById('pCourseSlug').value,
                description: document.getElementById('pCourseDesc').value,
                track: document.getElementById('pCourseTrack').value,
                tag: document.getElementById('pCourseTag').value,
                categories: document.getElementById('pCourseCats').value,
                state: document.getElementById('pCourseState').value,
                badge: document.getElementById('pCourseBadge').value,
                color: document.getElementById('pCourseColor').value,
                estimatedHours: parseInt(document.getElementById('pCourseHours').value) || 6,
                isPublished: document.getElementById('pCoursePublished').checked,
            });
            await load();
            state.activeEntity = 'course';
            renderProps();
            setStatus('сохранено', 'saved');
        } catch (e) {
            console.error(e);
            setStatus('ошибка!', '');
        }
    });

    document.getElementById('pCourseDel')?.addEventListener('click', async () => {
        if (!confirm('Удалить курс?\nМодули останутся в базе без привязки к курсу.')) return;
        await DELETE('/courses/' + c.id);
        state.activeCourseId = null;
        state.activeModuleId = null;
        state.activeLessonId = null;
        state.activeEntity = null;
        await load();
    });
}

function bindModuleEvents(m) {
    // Цветовые свотчи — кликать на пресет
    document.getElementById('pModSwatches')?.addEventListener('click', e => {
        const swatch = e.target.closest('.bld-color-swatch');
        if (!swatch) return;
        const color = swatch.dataset.color;
        document.getElementById('pModColor').value = color;
        document.querySelectorAll('.bld-color-swatch').forEach(s => s.classList.remove('active'));
        swatch.classList.add('active');
    });

    // Нативный color-picker → обновляем подпись
    document.getElementById('pModColor')?.addEventListener('input', e => {
        const colorLabel = e.target.nextElementSibling;
        if (colorLabel) colorLabel.textContent = e.target.value;
    });

    document.getElementById('pModSave')?.addEventListener('click', async () => {
        setStatus('сохранение...', 'saving');
        try {
            const color = document.getElementById('pModColor').value;
            const mapX = parseFloat(document.getElementById('pModX').value);
            const mapY = parseFloat(document.getElementById('pModY').value);

            const labelFontSize = parseInt(document.getElementById('pModLabelSize')?.value) || 9;
            await PUT('/modules/' + m.id, {
                courseId: state.activeCourseId,
                title: document.getElementById('pModTitle').value,
                description: document.getElementById('pModDesc').value,
                isLocked: document.getElementById('pModLock').checked,
                isChallenge: document.getElementById('pModChallenge')?.checked ?? m.isChallenge,
                color,
                mapX,
                mapY,
                labelFontSize,
            });
            await load();
            // Если карта открыта — перерисовать узлы
            if (state.activeTab === 'map') renderMapEditor();
            setStatus('сохранено', 'saved');
        } catch { setStatus('ошибка!', ''); }
    });

    document.getElementById('pModDel')?.addEventListener('click', async () => {
        if (!confirm('Удалить модуль?\nУроки и их контент останутся в базе (без привязки к модулю).')) return;
        await DELETE('/modules/' + m.id);
        state.activeModuleId = null;
        state.activeEntity = null;
        await load();
    });
}

function bindLessonEvents(l) {
    document.getElementById('pLesSave')?.addEventListener('click', async () => {
        setStatus('сохранение...', 'saving');
        try {
            await PUT('/lessons/' + l.id, {
                title: document.getElementById('pLesTitle').value,
                isLocked: document.getElementById('pLesLock').checked,
            });
            await load(); setStatus('сохранено', 'saved');
        } catch { setStatus('ошибка!', ''); }
    });
    document.getElementById('pLesDel')?.addEventListener('click', async () => {
        if (!confirm('Удалить урок?\nБлоки контента останутся в базе (без привязки к уроку).')) return;
        await DELETE('/lessons/' + l.id);
        state.activeLessonId = null;
        state.activeEntity = null;
        await load();
    });
}

function bindPropEvents(block) {
    document.getElementById('pBlkSave')?.addEventListener('click', async () => {
        setStatus('сохранение...', 'saving');
        try {
            const content = buildBlockContent(block.type);
            await PUT('/blocks/' + block.id, { type: block.type, content });
            await loadBlocks(state.activeLessonId);
            render(); setStatus('сохранено', 'saved');
        } catch (e) { setStatus('ошибка!', ''); console.error(e); }
    });
}

function buildBlockContent(type) {
    const g = id => document.getElementById(id)?.value ?? '';
    const gi = id => Math.max(0, parseInt(g(id)) - 1);
    const lines = id => g(id).split('\n').filter(s => s.trim());

    switch (type) {
        // ── Базовые ──────────────────────────────────────────────────────────────
        case 'text': return JSON.stringify({ markdown: g('pBlkMd') });
        case 'task': return JSON.stringify({ title: g('pBlkTaskTitle'), description: g('pBlkTaskDesc'), language: g('pBlkTaskLang'), starterCode: g('pBlkTaskCode'), expectedOutput: g('pBlkTaskOut') });
        case 'code': return JSON.stringify({ language: g('pBlkCodeLang'), title: g('pBlkCodeTitle'), code: g('pBlkCodeBody') });
        case 'quiz': return JSON.stringify({ question: g('pBlkQ'), options: [g('pBlkOpt0'), g('pBlkOpt1'), g('pBlkOpt2'), g('pBlkOpt3')], correct: gi('pBlkCorrect'), explanation: g('pBlkExpl') });
        case 'button': return JSON.stringify({ text: g('pBlkBtnText'), href: g('pBlkBtnHref'), variant: g('pBlkBtnVariant') });
        // ── Предсказание ──────────────────────────────────────────────────────────
        case 'prediction': return JSON.stringify({ language: g('pBlkLang'), code: g('pBlkCode'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'modify': return JSON.stringify({ language: g('pBlkLang'), code: g('pBlkCode'), modifyInstruction: g('pBlkInstruction'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'counterfactual': return JSON.stringify({ code: g('pBlkCode'), removeLine: parseInt(g('pBlkRemoveLine')) || 1, question: g('pBlkQuestion'), answer: g('pBlkAnswer') });
        case 'broken_reality': return JSON.stringify({ code: g('pBlkCode'), edgeCaseInput: g('pBlkEdgeCase'), bugDescription: g('pBlkBugDesc') });
        case 'semantic_diff': return JSON.stringify({ intendedCode: g('pBlkIntended'), actualCode: g('pBlkActual'), explanation: g('pBlkExplanation') });
        case 'mutation': return JSON.stringify({ language: g('pBlkLang'), baseCode: g('pBlkCode'), mutation: g('pBlkMutation'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        // ── Конструирование ───────────────────────────────────────────────────────
        case 'fill_blank': return JSON.stringify({ template: g('pBlkTemplate'), blanks: lines('pBlkBlanks'), hint: g('pBlkHint') });
        case 'drag_order': return JSON.stringify({ pieces: lines('pBlkPieces'), solution: g('pBlkSolution').split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n)), hint: g('pBlkHint') });
        case 'goal_construction': return JSON.stringify({ language: g('pBlkLang'), goal: g('pBlkGoal'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'template_completion': return JSON.stringify({ template: g('pBlkTemplate'), blanks: lines('pBlkBlanks'), hint: g('pBlkHint') });
        case 'fix_bug': return JSON.stringify({ language: g('pBlkLang'), buggyCode: g('pBlkBuggyCode'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'mini_task': return JSON.stringify({ language: g('pBlkLang'), title: g('pBlkTitle'), description: g('pBlkDesc'), starterCode: g('pBlkStarterCode'), expectedOutput: g('pBlkExpected') });
        case 'constraint': return JSON.stringify({ language: g('pBlkLang'), title: g('pBlkTitle'), description: g('pBlkDesc'), constraint: g('pBlkConstraint'), starterCode: g('pBlkStarterCode'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'rebuild_scratch': return JSON.stringify({ language: g('pBlkLang'), title: g('pBlkTitle'), description: g('pBlkDesc'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'progressive_reveal': return JSON.stringify({ title: g('pBlkTitle'), hints: lines('pBlkHints'), finalCode: g('pBlkFinalCode'), expectedOutput: g('pBlkExpected') });
        // ── Объяснение ────────────────────────────────────────────────────────────
        case 'explain': return JSON.stringify({ prompt: g('pBlkPrompt'), codeSnippet: g('pBlkCode'), correctExplanation: g('pBlkCorrect') });
        case 'intent_to_code': return JSON.stringify({ language: g('pBlkLang'), intent: g('pBlkIntent'), expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        case 'code_to_intent': return JSON.stringify({ code: g('pBlkCode'), options: lines('pBlkOptions'), correctIntent: gi('pBlkCorrect') });
        // ── Испытание ─────────────────────────────────────────────────────────────
        case 'challenge_task': return JSON.stringify({ language: g('pBlkLang'), title: g('pBlkTitle'), description: g('pBlkDesc'), requirements: lines('pBlkReqs'), starterCode: g('pBlkStarterCode'), expectedOutput: g('pBlkExpected') });
        // ── Контроль ─────────────────────────────────────────────────────────────
        case 'control_question': return JSON.stringify({ question: g('pBlkQuestion'), answer: g('pBlkAnswer') });
        case 'control_task': return JSON.stringify({ language: g('pBlkLang'), title: g('pBlkTitle'), description: g('pBlkDesc'), expectedOutput: g('pBlkExpected') });
        case 'checkpoint': return JSON.stringify({ title: g('pBlkTitle'), description: g('pBlkDesc'), tasks: lines('pBlkTasks') });
        case 'knowledge_check': return JSON.stringify({ topic: g('pBlkTopic'), question: g('pBlkQuestion'), options: [g('pBlkOpt0'), g('pBlkOpt1'), g('pBlkOpt2'), g('pBlkOpt3')].filter(Boolean), correct: gi('pBlkCorrect'), explanation: g('pBlkExplanation') });
        case 'transfer_test': return JSON.stringify({ question: g('pBlkQuestion'), options: [g('pBlkOpt0'), g('pBlkOpt1'), g('pBlkOpt2')].filter(Boolean), correct: gi('pBlkCorrect'), explanation: g('pBlkExplanation') });
        case 'timed_test': return JSON.stringify({ question: g('pBlkQuestion'), timeLimit: parseInt(g('pBlkTimeLimit')) || 30, expectedOutput: g('pBlkExpected'), hint: g('pBlkHint') });
        // ── Анализ ────────────────────────────────────────────────────────────────
        case 'compare_variants': {
            const variants = [];
            for (let i = 0; document.getElementById('pBlkVTitle' + i); i++) {
                variants.push({ title: g('pBlkVTitle' + i), code: g('pBlkVCode' + i) });
            }
            return JSON.stringify({ question: g('pBlkQuestion'), variants, correct: gi('pBlkCorrect'), explanation: g('pBlkExplanation') });
        }
        case 'counterexample': return JSON.stringify({ code: g('pBlkCode'), claim: g('pBlkClaim'), answer: g('pBlkAnswer'), explanation: g('pBlkExplanation') });
        case 'adversarial_test': return JSON.stringify({ code: g('pBlkCode'), claim: g('pBlkClaim'), hint: g('pBlkHint') });
        case 'two_path': return JSON.stringify({ question: g('pBlkQuestion'), pathA: { title: g('pBlkPathATitle'), code: g('pBlkPathACode') }, pathB: { title: g('pBlkPathBTitle'), code: g('pBlkPathBCode') }, explanation: g('pBlkExplanation') });
        // ── Дополнительные ───────────────────────────────────────────────────────
        case 'short_answer': return JSON.stringify({ question: g('pBlkQuestion'), answer: g('pBlkAnswer'), hint: g('pBlkHint') });
        case 'classify': return JSON.stringify({ prompt: g('pBlkPrompt'), categories: lines('pBlkCategories'), items: lines('pBlkItems'), answer: g('pBlkAnswer') });
        default: return g('pBlkRaw');
    }
}

// ── Drag-and-drop блоков (режим контента) ─────────────────────────────────────
function initDragBlocks(container, blocks) {
    let dragId = null;

    container.querySelectorAll('.bld-block').forEach(el => {
        el.addEventListener('dragstart', e => {
            dragId = +el.dataset.bid;
            el.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
        });
        el.addEventListener('dragend', () => {
            el.classList.remove('dragging');
            container.querySelectorAll('.bld-block').forEach(b => b.classList.remove('drag-over'));
        });
        el.addEventListener('dragover', e => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            el.classList.add('drag-over');
        });
        el.addEventListener('dragleave', () => el.classList.remove('drag-over'));
        el.addEventListener('drop', async e => {
            e.preventDefault();
            el.classList.remove('drag-over');
            const targetId = +el.dataset.bid;
            if (dragId === targetId) return;

            const lesson = getLesson(state.activeLessonId);
            if (!lesson?.blocks) return;

            const fromIdx = lesson.blocks.findIndex(b => b.id === dragId);
            const toIdx = lesson.blocks.findIndex(b => b.id === targetId);
            if (fromIdx < 0 || toIdx < 0) return;

            const [moved] = lesson.blocks.splice(fromIdx, 1);
            lesson.blocks.splice(toIdx, 0, moved);
            lesson.blocks.forEach((b, i) => b.order = i);
            renderContentCanvas();

            try {
                await POST('/reorder', {
                    entityType: 'block',
                    items: lesson.blocks.map(b => ({ id: b.id, order: b.order }))
                });
                setStatus('порядок сохранён', 'saved');
            } catch { setStatus('ошибка сохранения', ''); }
        });
    });
}

// ── Выбор элементов ───────────────────────────────────────────────────────────
function selectCourse(id) {
    state.activeCourseId = id;
    state.modules = getCourse(id)?.modules ?? [];
    state.activeModuleId = null;
    state.activeLessonId = null;
    state.activeBlockId = null;
    state.activeEntity = 'course';
    render();
}

function selectModule(id) {
    state.activeModuleId = id;
    state.activeEntity = 'module';
    renderSidebar();
    renderProps();
    // Обновляем выделение на карте (если открыта)
    if (state.activeTab === 'map') {
        document.querySelectorAll('.bld-map-node').forEach(n => {
            n.classList.toggle('selected', +n.dataset.mid === id);
        });
    }
}

async function selectLesson(id) {
    state.activeLessonId = id;
    state.activeEntity = 'lesson';
    const lesson = getLesson(id);
    if (lesson && !lesson.blocks.length) await loadBlocks(id);
    render();
}

function selectBlock(id) {
    state.activeBlockId = id;
    state.activeEntity = 'block';
    renderContentCanvas();
    renderProps();
}

// ── Добавить элементы ─────────────────────────────────────────────────────────
async function addCourse() {
    const title = prompt('Название курса:');
    if (!title) return;
    setStatus('создание...', 'saving');
    const course = await POST('/courses', {
        title,
        slug: title,
        track: 'BACKEND',
        tag: '',
        color: NODE_COLORS[state.courses.length % NODE_COLORS.length],
        state: 'available',
        badge: 'START',
        categories: 'backend',
        estimatedHours: 6,
        isPublished: true,
    });
    state.activeCourseId = course.id;
    await load();
    selectCourse(course.id);
}

async function unlockAll() {
    if (!confirm('Разблокировать ВСЕ модули и уроки?')) return;
    setStatus('разблокировка...', 'saving');
    try {
        await POST('/unlock-all', {});
        setStatus('все разблокировано!', 'saved');
        await load();
    } catch (e) {
        setStatus('ошибка!', '');
        console.error(e);
    }
}

async function lockAll() {
    if (!confirm('Включить блокировку по прохождению? (модули/уроки откроются по очереди)')) return;
    setStatus('включение...', 'saving');
    try {
        const r = await POST('/lock-all', {});
        setStatus(r?.message || 'блокировка включена', 'saved');
    } catch (e) {
        setStatus('ошибка!', '');
        console.error(e);
    }
}

async function completeAll() {
    if (!confirm('Отметить ВСЕ уроки пройденными для всех пользователей?')) return;
    setStatus('прохождение...', 'saving');
    try {
        const r = await POST('/complete-all', {});
        setStatus(r?.message || 'всё пройдено', 'saved');
    } catch (e) {
        setStatus('ошибка!', '');
        console.error(e);
    }
}

async function resetProgress() {
    if (!confirm('Сбросить прогресс ВСЕХ пользователей? (все прохождения уроков будут удалены)')) return;
    setStatus('сброс...', 'saving');
    try {
        const r = await POST('/reset-progress', {});
        setStatus(r?.message || 'прогресс сброшен', 'saved');
    } catch (e) {
        setStatus('ошибка!', '');
        console.error(e);
    }
}

async function seedCsharp() {
    if (!confirm('Это УДАЛИТ весь текущий C# курс и заменит его новым контентом. Продолжить?')) return;
    setStatus('сидинг...', 'saving');
    try {
        await POST('/seed-csharp', {});
        setStatus('готово!', 'saved');
        await load();
        render();
    } catch (e) {
        setStatus('ошибка!', '');
        console.error(e);
    }
}

async function addChallenge() {
    const targetCourseId = state.activeCourseId;
    if (!targetCourseId) { alert('Сначала создайте или выберите курс.'); return; }
    const title = prompt('Название контрольной задачи (испытания):');
    if (!title) return;
    setStatus('создание...', 'saving');
    const offset = state.modules.length * 4;
    const m = await POST('/modules', {
        courseId: targetCourseId,
        title,
        isChallenge: true,
        isLocked: true,
        mapX: Math.min(94, 50 + offset),
        mapY: Math.min(94, 50 + offset),
        color: '#ffd700',
    });
    state.openModuleIds.add(m.id);
    await load();
    selectModule(m.id);
}

async function addModule(courseId) {
    const targetCourseId = courseId || state.activeCourseId;
    if (!targetCourseId) { alert('Сначала создайте или выберите курс.'); return; }
    const title = prompt('Название модуля:');
    if (!title) return;
    setStatus('создание...', 'saving');
    // Ставим в центр, немного со смещением чтобы не накладывались
    const offset = state.modules.length * 4;
    const m = await POST('/modules', {
        courseId: targetCourseId,
        title,
        mapX: 50 + offset,
        mapY: 50 + offset,
        color: NODE_COLORS[state.modules.length % NODE_COLORS.length],
    });
    state.openModuleIds.add(m.id);
    await load();
    selectModule(m.id);
}

async function addLesson(moduleId) {
    const title = prompt('Название урока:');
    if (!title) return;
    setStatus('создание...', 'saving');
    const l = await POST('/modules/' + moduleId + '/lessons', { title });
    state.openModuleIds.add(moduleId);
    await load();
    await selectLesson(l.id);
}

async function addBlock(type) {
    if (!state.activeLessonId) { alert('Сначала выберите урок.'); return; }
    const defaults = {
        // ── Базовые ───────────────────────────────────────────────────────────────
        text: JSON.stringify({ markdown: 'Введите текст...' }),
        task: JSON.stringify({ title: 'Задание', description: '', starterCode: '', expectedOutput: '' }),
        code: JSON.stringify({ language: 'csharp', title: '', code: '' }),
        quiz: JSON.stringify({ question: 'Вопрос?', options: ['', '', '', ''], correct: 0, explanation: '' }),
        button: JSON.stringify({ text: 'Далее', href: '#', variant: 'primary' }),
        // ── Предсказание ──────────────────────────────────────────────────────────
        prediction: JSON.stringify({ code: 'x = 5\nprint(x)', expectedOutput: '5', hint: '' }),
        modify: JSON.stringify({ code: 'x = 5\nprint(x)', modifyInstruction: 'Измените одну строку, чтобы вывело 10', expectedOutput: '10', hint: '' }),
        counterfactual: JSON.stringify({ code: 'x = 5\ny = x + 1\nprint(y)', removeLine: 2, question: 'Что выведет код, если убрать строку 2?', answer: 'ошибка — y не определена' }),
        broken_reality: JSON.stringify({ code: 'def max_val(lst):\n    m = lst[0]\n    for x in lst:\n        if x > m: m = x\n    return m', edgeCaseInput: '[]', bugDescription: 'Падает на пустом списке' }),
        semantic_diff: JSON.stringify({ intendedCode: '# Хотел сделать:\nx = x + 1', actualCode: '# Написал:\nx == x + 1', explanation: 'Оператор сравнения вместо присваивания' }),
        mutation: JSON.stringify({ baseCode: 'for i in range(5):\n    print(i)', mutation: 'Измените range(5) на range(1, 6)', expectedOutput: '1\n2\n3\n4\n5', hint: '' }),
        // ── Конструирование ───────────────────────────────────────────────────────
        fill_blank: JSON.stringify({ template: 'x = ___\nif x ___ 0:\n    print("положительное")', blanks: ['5', '>'], hint: 'Заполни пропуски' }),
        drag_order: JSON.stringify({ pieces: ['print(result)', 'result = 0', 'for i in range(1,6):', '    result += i'], solution: [1, 2, 3, 0], hint: 'Расположи строки в правильном порядке' }),
        goal_construction: JSON.stringify({ language: 'python', goal: 'Вывести числа от 1 до 5, каждое на новой строке', expectedOutput: '1\n2\n3\n4\n5', hint: 'Используй цикл for и range()' }),
        template_completion: JSON.stringify({ language: 'python', template: 'total = 0\nfor i in ___:\n    total = ___\nprint(total)', blanks: ['range(1, 6)', 'total + i'], hint: 'Дополни шаблон суммирования' }),
        fix_bug: JSON.stringify({ language: 'python', buggyCode: 'total = 0\nfor i in range(5)\n    total += i\nprint(total)', expectedOutput: '10', hint: 'Обрати внимание на синтаксис цикла' }),
        mini_task: JSON.stringify({ language: 'python', title: 'Мини-задача', description: 'Выведи приветствие с именем пользователя', starterCode: 'name = "Алиса"\n# твой код', expectedOutput: 'Привет, Алиса!' }),
        constraint: JSON.stringify({ language: 'python', title: 'С ограничением', description: 'Найди максимум в списке', constraint: 'без функции max()', starterCode: 'nums = [3, 1, 4, 1, 5, 9]', expectedOutput: '9', hint: '' }),
        rebuild_scratch: JSON.stringify({ language: 'python', title: 'Воспроизведи', description: 'Напиши функцию, которая считает сумму чисел от 1 до n', expectedOutput: '15', hint: '' }),
        progressive_reveal: JSON.stringify({ language: 'python', title: 'Цикл с накоплением', hints: ['Заведи переменную total = 0', 'Пройдись по range(1, 6) в цикле for', 'Прибавляй i к total на каждом шаге', 'print(total) в конце'], finalCode: 'total = 0\nfor i in range(1, 6):\n    total += i\nprint(total)', expectedOutput: '15' }),
        // ── Объяснение ────────────────────────────────────────────────────────────
        explain: JSON.stringify({ prompt: 'Объясни, что делает этот код, в 1–2 предложениях', codeSnippet: 'x = 0\nfor i in range(5):\n    x += i', correctExplanation: 'Код суммирует числа от 0 до 4 включительно' }),
        intent_to_code: JSON.stringify({ intent: 'Выведи все чётные числа от 0 до 10', expectedOutput: '0\n2\n4\n6\n8\n10', hint: '' }),
        code_to_intent: JSON.stringify({ code: 'nums = [1,2,3,4,5]\nresult = [x*x for x in nums]\nprint(result)', options: ['Находит сумму чисел', 'Возводит каждый элемент в квадрат', 'Фильтрует чётные числа'], correctIntent: 1 }),
        // ── Испытание ─────────────────────────────────────────────────────────────
        challenge_task: JSON.stringify({ language: 'csharp', title: 'Финальное испытание', description: 'Напиши программу, которая...', requirements: ['Используй цикл', 'Выведи результат через Console.WriteLine'], starterCode: '// Напишите программу с нуля\nusing System;\n\nclass Program {\n    static void Main() {\n        \n    }\n}', expectedOutput: '' }),
        // ── Контроль ─────────────────────────────────────────────────────────────
        control_question: JSON.stringify({ question: 'Что делает оператор += ?', answer: 'Прибавляет значение справа к переменной слева и записывает результат обратно' }),
        control_task: JSON.stringify({ title: 'Самостоятельная задача', description: 'Напиши цикл, который выводит квадраты чисел от 1 до 5', expectedOutput: '1\n4\n9\n16\n25' }),
        checkpoint: JSON.stringify({ title: 'Чекпоинт: Циклы', description: '3 задания без подсказок', tasks: ['Выведи числа от 1 до 10', 'Посчитай сумму чисел от 1 до 100', 'Найди все числа, делящиеся на 3, от 1 до 30'] }),
        knowledge_check: JSON.stringify({ topic: 'Переменные', question: 'Что произойдёт с переменной x после выполнения x = x + 1?', options: ['Ничего', 'x увеличится на 1', 'Ошибка', 'x обнулится'], correct: 1, explanation: 'Присваивание обновляет значение переменной' }),
        transfer_test: JSON.stringify({ question: 'Где ещё используется та же логика накопления?', options: ['Подсчёт символов в строке', 'Сортировка массива', 'Вывод числа на экран'], correct: 0, explanation: 'Накопление — это паттерн "переменная-счётчик плюс цикл"' }),
        timed_test: JSON.stringify({ question: 'Что выведет print(2 ** 3)?', timeLimit: 30, expectedOutput: '8', hint: '' }),
        // ── Анализ ────────────────────────────────────────────────────────────────
        compare_variants: JSON.stringify({ question: 'Какое решение лучше и почему?', variants: [{ title: 'Вариант A', code: 'result = sum(range(1,6))' }, { title: 'Вариант B', code: 'result = 0\nfor i in range(1,6):\n    result += i' }], correct: 0, explanation: 'Вариант A лаконичнее — встроенная функция sum() делает то же самое в одну строку' }),
        counterexample: JSON.stringify({ code: 'def is_positive(n):\n    return n > 0', claim: 'Функция возвращает True для любого числа больше нуля', answer: '-1', explanation: 'Функция возвращает False для отрицательных чисел, что корректно, но для 0 возвращает False, а не True' }),
        adversarial_test: JSON.stringify({ code: 'def divide(a, b):\n    return a / b', claim: 'Функция всегда возвращает число', testInput: 'divide(5, 0)', hint: 'Подумай о крайних случаях' }),
        two_path: JSON.stringify({ question: 'Сравни два подхода. Что быстрее и почему?', pathA: { title: 'Цикл while', code: 'i = 0\nwhile i < n:\n    i += 1' }, pathB: { title: 'Цикл for', code: 'for i in range(n):\n    pass' }, explanation: 'Оба O(n), но for читабельнее и идиоматичнее' }),
        // ── Дополнительные ───────────────────────────────────────────────────────
        short_answer: JSON.stringify({ question: 'Какое ключевое слово используется для объявления переменной, значение которой нельзя изменить?', answer: 'const', hint: 'Это слово из 5 букв' }),
        classify: JSON.stringify({ prompt: 'Разнеси элементы по категориям', categories: ['Value Type', 'Reference Type'], items: ['int', 'string', 'double', 'class', 'struct', 'bool'], answer: '{"Value Type":["int","double","struct","bool"],"Reference Type":["string","class"]}' }),
    };
    setStatus('создание...', 'saving');
    const b = await POST('/lessons/' + state.activeLessonId + '/blocks', {
        type, content: defaults[type] || '{}'
    });
    await loadBlocks(state.activeLessonId);
    selectBlock(b.id);
    renderContentCanvas();
    renderProps();
    setStatus('создано', 'saved');
}

async function deleteBlock(id) {
    if (!confirm('Удалить блок?')) return;
    await DELETE('/blocks/' + id);
    if (state.activeBlockId === id) { state.activeBlockId = null; state.activeEntity = null; }
    await loadBlocks(state.activeLessonId);
    render();
}

// ── Init ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    load().then(() => {
        if (window.MapObjectsManager && state.activeCourseId) {
            MapObjectsManager.init(state.activeCourseId, '#bldMapInner');
        }
    });
});
// ══════════════════════════════════════════════════════════════════
//  НАСТРОЙКИ ПРОЕКТА
// ══════════════════════════════════════════════════════════════════

let _settingsData = null;   // кэш последнего GET /settings
let _usersData = [];     // все пользователи
let _activeStab = 'stats';

// ── Открыть/закрыть панель ────────────────────────────────────────
function showSettings() {
    document.getElementById('bldSettingsOverlay').style.display = '';
    showSTab(_activeStab);          // восстанавливаем последнюю вкладку
}

function hideSettings() {
    document.getElementById('bldSettingsOverlay').style.display = 'none';
}

// Закрыть по клику на оверлей (но не на саму панель)
document.getElementById('bldSettingsOverlay').addEventListener('click', function (e) {
    if (e.target === this) hideSettings();
});

// ── Переключение вкладок ──────────────────────────────────────────
const S_TABS = ['stats', 'ai', 'platform', 'email', 'users', 'system'];

function showSTab(tab) {
    _activeStab = tab;
    S_TABS.forEach(t => {
        document.getElementById('stab-' + t).classList.toggle('bld-stab--active', t === tab);
        document.getElementById('spanel-' + t).style.display = t === tab ? '' : 'none';
    });

    if (tab === 'stats') loadStats();
    if (tab === 'ai') loadAiSettings();
    if (tab === 'platform') loadPlatformSettings();
    if (tab === 'email') loadEmailSettings();
    if (tab === 'users') loadUsers();
    if (tab === 'system') loadSysInfo();
}

// ── Статистика ────────────────────────────────────────────────────
async function loadStats() {
    try {
        const d = await fetch('/api/builder/stats').then(r => r.json());
        setText('ss-users', d.totalUsers);
        setText('ss-lessons', d.totalLessons);
        setText('ss-modules', d.totalModules);
        setText('ss-completions', d.totalCompletions);
        setText('ss-errors', d.totalErrors);
        setText('ss-comments', d.totalComments);
        setText('ss-ai-today', d.aiToday);
        setText('ss-ai-total', d.aiTotal);
    } catch (e) {
        console.error('Stats error', e);
    }
}

// Модели по умолчанию для каждого провайдера
const PROVIDER_MODELS = {
    groq: 'meta-llama/llama-4-scout-17b-16e-instruct',
    openrouter: 'meta-llama/llama-4-scout:free',
    gemini: 'gemini-2.0-flash',
    gemini25: 'gemini-2.5-flash',
    cerebras: 'qwen-3-32b',
    mistral: 'mistral-medium-latest',
    sambanova: 'Meta-Llama-3.3-70B-Instruct',
    anthropic: 'claude-haiku-4-5',
};

// Инфо о лимитах и ключах для каждого провайдера
const PROVIDER_INFO = {
    groq: {
        limits: '30 RPM · 1 000 RPD · 30k TPM · 500k TPD',
        key: 'Бесплатный ключ: console.groq.com',
        color: 'var(--text2)'
    },
    gemini: {
        limits: '15 RPM · 1 500 RPD · 1 000 000 TPM  — лучший бесплатный лимит',
        key: 'Бесплатный ключ: aistudio.google.com/apikey',
        color: '#55cc88'
    },
    gemini25: {
        limits: '10 RPM · 500 RPD · 250 000 TPM (preview)',
        key: 'Бесплатный ключ: aistudio.google.com/apikey (тот же)',
        color: '#55cc88'
    },
    openrouter: {
        limits: '20 RPM бесплатно (19 моделей)',
        key: 'Бесплатный ключ: openrouter.ai/keys',
        color: 'var(--text2)'
    },
    cerebras: {
        limits: 'Очень быстрый (WSE чипы), лимиты по запросу',
        key: 'Бесплатный ключ: cloud.cerebras.ai',
        color: 'var(--text2)'
    },
    mistral: {
        limits: 'Бесплатный tier для личного использования',
        key: 'Бесплатный ключ: console.mistral.ai',
        color: 'var(--text2)'
    },
    sambanova: {
        limits: 'Быстрый, бесплатный для личного использования',
        key: 'Бесплатный ключ: cloud.sambanova.ai',
        color: 'var(--text2)'
    },
    anthropic: {
        limits: 'Платный ($), не бесплатный',
        key: 'Платный API: console.anthropic.com',
        color: '#ff5555'
    },
};

function onProviderChange() {
    const prov = document.getElementById('sf-ai-provider').value;
    const model = PROVIDER_MODELS[prov] || '';
    document.getElementById('sf-ai-model').value = model;
    document.getElementById('sf-ai-model-hint').textContent = 'модель по умолчанию для ' + prov;

    // Показываем инфо о лимитах
    const info = PROVIDER_INFO[prov];
    const limEl = document.getElementById('sf-ai-limits');
    if (limEl && info) {
        limEl.innerHTML =
            '<span style="color:' + info.color + '">' + esc(info.limits) + '</span>' +
            ' &nbsp;|&nbsp; ' + esc(info.key);
    } else if (limEl) {
        limEl.textContent = '';
    }
}

// ── Настройки AI ──────────────────────────────────────────────────
async function loadAiSettings() {
    try {
        _settingsData = await fetch('/api/builder/settings').then(r => r.json());
        const d = _settingsData;

        const provEl = document.getElementById('sf-ai-provider');
        if (provEl) provEl.value = d.aiProvider || 'groq';

        document.getElementById('sf-ai-model').value = d.aiModel || PROVIDER_MODELS[d.aiProvider] || '';
        document.getElementById('sf-ai-key').value = d.aiApiKey || '';
        document.getElementById('sf-ai-limit').value = d.aiDailyMessageLimit ?? 20;
        document.getElementById('sf-ai-tokens').value = d.aiMaxTokens ?? 768;
        document.getElementById('sf-ai-history').value = d.aiHistoryMessages ?? 8;

        // Показываем лимиты текущего провайдера
        onProviderChange();

        const status = document.getElementById('sf-ai-key-status');
        if (d.aiApiKeySet) {
            status.textContent = '+ ключ установлен';
            status.className = 'bld-shint ok';
        } else {
            status.textContent = '! ключ не задан — AI-наставник недоступен';
            status.className = 'bld-shint err';
        }
    } catch (e) { console.error('Settings load error', e); }
}

async function saveAiSettings() {
    try {
        const r = await fetch('/api/builder/settings', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                aiProvider: document.getElementById('sf-ai-provider').value,
                aiApiKey: document.getElementById('sf-ai-key').value.trim(),
                aiModel: document.getElementById('sf-ai-model').value.trim(),
                aiDailyMessageLimit: parseInt(document.getElementById('sf-ai-limit').value) || 15,
                aiMaxTokens: parseInt(document.getElementById('sf-ai-tokens').value) || 512,
                aiHistoryMessages: parseInt(document.getElementById('sf-ai-history').value) || 6,
            }),
        });
        if (!r.ok) throw new Error(await r.text());
        _settingsData = null;
        showBldStatus('AI-настройки сохранены');
        await loadAiSettings();
    } catch (e) {
        showBldStatus('Ошибка: ' + e.message, true);
    }
}

// ── Настройки Платформы ───────────────────────────────────────────
async function loadPlatformSettings() {
    if (!_settingsData) {
        _settingsData = await fetch('/api/builder/settings').then(r => r.json());
    }
    const d = _settingsData;
    setVal('sf-pl-sitename', d.platformSiteName ?? 'CodeLearn');
    setChk('sf-pl-reg', d.platformRegistrationOpen ?? true);
    setChk('sf-pl-ai', d.platformAllowAiChat ?? true);
    setChk('sf-pl-comments', d.platformAllowComments ?? true);
    setVal('sf-pl-cmaxlen', d.platformCommentMaxLength ?? 1000);
    setVal('sf-pl-hint', d.platformHintThreshold ?? 3);
    setVal('sf-pl-poll', d.platformChatPollInterval ?? 3);
    setVal('sf-pl-cpage', d.platformCommentsPerPage ?? 30);
    setChk('sf-pl-progression', d.platformProgressionLock ?? true);
}

async function savePlatformSettings() {
    try {
        const r = await fetch('/api/builder/settings', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                platformSiteName: document.getElementById('sf-pl-sitename').value.trim(),
                platformRegistrationOpen: document.getElementById('sf-pl-reg').checked,
                platformAllowAiChat: document.getElementById('sf-pl-ai').checked,
                platformAllowComments: document.getElementById('sf-pl-comments').checked,
                platformCommentMaxLength: parseInt(document.getElementById('sf-pl-cmaxlen').value) || 1000,
                platformHintThreshold: parseInt(document.getElementById('sf-pl-hint').value) || 3,
                platformChatPollIntervalSec: parseInt(document.getElementById('sf-pl-poll').value) || 3,
                platformCommentsPerPage: parseInt(document.getElementById('sf-pl-cpage').value) || 30,
                platformProgressionLock: document.getElementById('sf-pl-progression').checked,
            }),
        });
        if (!r.ok) throw new Error(await r.text());
        _settingsData = null;
        showBldStatus('Настройки платформы сохранены');
    } catch (e) {
        showBldStatus('Ошибка: ' + e.message, true);
    }
}

// ── Настройки Email ───────────────────────────────────────────────
async function loadEmailSettings() {
    if (!_settingsData) {
        _settingsData = await fetch('/api/builder/settings').then(r => r.json());
    }
    const d = _settingsData;
    document.getElementById('sf-em-host').value = d.emailSmtpHost || '';
    document.getElementById('sf-em-port').value = d.emailSmtpPort || 587;
    document.getElementById('sf-em-user').value = d.emailUser || '';
    document.getElementById('sf-em-pass').value = d.emailAppPassword || '';
    document.getElementById('sf-em-from').value = d.emailFrom || '';
}

async function saveEmailSettings() {
    const body = {
        emailSmtpHost: document.getElementById('sf-em-host').value.trim(),
        emailSmtpPort: parseInt(document.getElementById('sf-em-port').value) || 587,
        emailUser: document.getElementById('sf-em-user').value.trim(),
        emailAppPassword: document.getElementById('sf-em-pass').value.trim(),
        emailFrom: document.getElementById('sf-em-from').value.trim(),
    };
    try {
        const r = await fetch('/api/builder/settings', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body),
        });
        if (!r.ok) throw new Error(await r.text());
        _settingsData = null;          // сбросить кэш
        showBldStatus('Email-настройки сохранены ✓');
    } catch (e) {
        showBldStatus('Ошибка: ' + e.message, true);
    }
}

// ── Пользователи ──────────────────────────────────────────────────
async function loadUsers() {
    const listEl = document.getElementById('sf-users-list');
    listEl.innerHTML = '<div class="bld-sloading">Загрузка...</div>';
    try {
        _usersData = await fetch('/api/builder/users').then(r => r.json());
        document.getElementById('sf-users-count').textContent =
            _usersData.length + ' ' + pluralUsers(_usersData.length);
        renderUsers(_usersData);
    } catch (e) {
        listEl.innerHTML = '<div class="bld-sloading">Ошибка загрузки</div>';
    }
}

function filterUsers() {
    const q = (document.getElementById('sf-users-search').value || '').toLowerCase();
    const filtered = q
        ? _usersData.filter(u =>
            (u.nickName || '').toLowerCase().includes(q) ||
            (u.email || '').toLowerCase().includes(q))
        : _usersData;
    document.getElementById('sf-users-count').textContent =
        filtered.length + ' ' + pluralUsers(filtered.length);
    renderUsers(filtered);
}

function renderUsers(list) {
    const el = document.getElementById('sf-users-list');
    if (!list.length) {
        el.innerHTML = '<div class="bld-sloading">Не найдено</div>';
        return;
    }
    el.innerHTML = '';
    list.forEach(u => {
        const row = document.createElement('div');
        row.className = 'bld-suser';

        const avatar = document.createElement('img');
        avatar.className = 'bld-suser-avatar';
        avatar.src = u.avatarUrl || '/img/main/ui/default-avatar.png';
        avatar.alt = '';

        const info = document.createElement('div');
        info.className = 'bld-suser-info';
        info.innerHTML =
            '<div class="bld-suser-nick">' + esc(u.nickName || 'без ника') +
            (u.hasGoogle ? ' <span class="bld-suser-google">[G]</span>' : '') + '</div>' +
            '<div class="bld-suser-email">' + esc(u.email || '') + '</div>';

        const stat = document.createElement('div');
        stat.className = 'bld-suser-stat';
        stat.textContent = u.completions + ' / ' + u.errors;
        stat.title = 'прошёл / ошибок';

        const date = document.createElement('div');
        date.className = 'bld-suser-date';
        const d = new Date(u.createdAt);
        date.textContent = d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: '2-digit' });

        const delBtn = document.createElement('button');
        delBtn.className = 'bld-btn sm danger';
        delBtn.textContent = 'удалить';
        delBtn.title = 'Удалить пользователя';
        delBtn.onclick = () => deleteUser(u.id, u.nickName || u.email, row);

        row.append(avatar, info, stat, date, delBtn);
        el.appendChild(row);
    });
}

async function deleteUser(id, label, rowEl) {
    if (!confirm('Удалить пользователя «' + label + '»?\nВсе данные будут удалены.')) return;
    try {
        const r = await fetch('/api/builder/users/' + id, { method: 'DELETE' });
        if (!r.ok) throw new Error(await r.text());
        rowEl.remove();
        _usersData = _usersData.filter(u => u.id !== id);
        document.getElementById('sf-users-count').textContent =
            _usersData.length + ' ' + pluralUsers(_usersData.length);
        showBldStatus('Пользователь удалён');
    } catch (e) {
        showBldStatus('Ошибка: ' + e.message, true);
    }
}

// ── Система ───────────────────────────────────────────────────────
async function loadSysInfo() {
    try {
        const d = await fetch('/api/builder/sysinfo').then(r => r.json());

        const setText2 = (id, val) => { const e = document.getElementById(id); if (e) e.textContent = val ?? '–'; };
        setText2('sys-env', d.environment);
        setText2('sys-started', d.startedAt);
        setText2('sys-uptime', d.uptimeHuman);
        setText2('sys-os', d.os);
        setText2('sys-fw', d.framework);
        setText2('sys-arch', d.architecture);
        setText2('sys-machine', d.machineName);
        setText2('sys-cpu', d.processorCount + ' ядер');
        setText2('sys-mem', d.memoryMb + ' МБ');
        setText2('sys-mempeak', d.peakMemoryMb + ' МБ');

        const dbEl = document.getElementById('sys-db');
        if (dbEl) {
            dbEl.textContent = d.dbStatus;
            dbEl.style.color = d.dbStatus === 'OK' ? 'var(--text2)' : '#ff5555';
        }

        const logEl = document.getElementById('sys-log');
        if (logEl) {
            if (!d.logLines || !d.logLines.length) {
                logEl.textContent = '(лог пуст или недоступен)';
            } else {
                logEl.textContent = d.logLines.join('\n');
                logEl.scrollTop = logEl.scrollHeight;
            }
        }
    } catch (e) {
        const logEl = document.getElementById('sys-log');
        if (logEl) logEl.textContent = 'Ошибка загрузки: ' + e.message;
    }
}

// ── Утилиты ───────────────────────────────────────────────────────
function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val ?? '–';
}

function setVal(id, val) {
    const el = document.getElementById(id);
    if (el) el.value = val ?? '';
}

function setChk(id, val) {
    const el = document.getElementById(id);
    if (el) el.checked = !!val;
}

function esc(s) {
    return String(s)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function togglePwd(inputId, btn) {
    const inp = document.getElementById(inputId);
    if (inp.type === 'password') { inp.type = 'text'; btn.textContent = 'скрыть'; }
    else { inp.type = 'password'; btn.textContent = 'показать'; }
}

function pluralUsers(n) {
    const m = n % 10, h = n % 100;
    if (h >= 11 && h <= 19) return 'пользователей';
    if (m === 1) return 'пользователь';
    if (m >= 2 && m <= 4) return 'пользователя';
    return 'пользователей';
}

function showBldStatus(msg, isErr = false) {
    const el = document.getElementById('bldStatus');
    if (!el) return;
    el.textContent = msg;
    el.style.color = isErr ? '#ff5555' : 'var(--text2)';
    clearTimeout(state.statusTimer);
    state.statusTimer = setTimeout(() => { el.textContent = ''; }, 3000);
}
