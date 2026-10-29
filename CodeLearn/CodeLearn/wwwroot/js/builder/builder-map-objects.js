// wwwroot/js/builder-map-objects.js
// Управление произвольными визуальными объектами на карте курса.
// Подключается в Builder/Index.cshtml рядом с основным builder.js

'use strict';

window.MapObjectsManager = (function () {


    // Минимальный top (%) с учётом расширения карты вверх
    function _minTopPct() {
        const extPx = parseInt(localStorage.getItem('bld_map_top_ext_px'), 10) || 0;
        return -(extPx / 1280 * 100);
    }

    // ─── state ───────────────────────────────────────────────────────────────────
    let _courseId = null;
    let _mapEl = null;       // DOM-элемент карты (#map-canvas или аналог)
    let _objects = [];         // [{id, type, src, label, mapX, mapY, width, rotation, zIndex}]
    let _editMode = false;      // включён ли режим редактирования map-objects
    let _dragState = null;       // {objId, startX, startY, origX, origY}
    let _selectedId = null;

    // ─── public API ──────────────────────────────────────────────────────────────
    async function init(courseId, mapSelector) {
        _courseId = courseId;
        _mapEl = document.querySelector(mapSelector);
        if (!_mapEl) { console.warn('[MapObjects] map element not found:', mapSelector); return; }

        await loadAll();
        renderAll();
        _buildPanel();
    }

    async function loadAll() {
        try {
            const r = await fetch(`/api/builder/map-objects?courseId=${_courseId}`);
            _objects = await r.json();
        } catch (e) { console.error('[MapObjects] load error', e); }
    }

    function setEditMode(on) {
        _editMode = on;
        _mapEl.querySelectorAll('.mo-wrap').forEach(el => {
            el.classList.toggle('mo-editable', on);
        });
        if (!on) _selectObject(null);
    }

    // ─── render ──────────────────────────────────────────────────────────────────
    function renderAll() {
        // Удаляем старые элементы
        _mapEl.querySelectorAll('.mo-wrap').forEach(el => el.remove());
        _objects.forEach(obj => _renderOne(obj));
    }

    function _renderOne(obj) {
        const wrap = document.createElement('div');
        wrap.className = 'mo-wrap';
        wrap.dataset.id = obj.id;
        wrap.style.cssText = `
            position: absolute;
            left:      ${obj.mapX}%;
            top:       ${obj.mapY}%;
            width:     ${obj.width}px;
            transform: translate(-50%,-50%) rotate(${obj.rotation}deg);
            z-index:   ${obj.zIndex + 1};
            cursor:    ${_editMode ? 'grab' : 'default'};
            user-select: none;
        `;

        // Внутренний контент
        wrap.innerHTML = _buildContent(obj);

        // Edit controls (видны только в edit-mode)
        const ctrl = document.createElement('div');
        ctrl.className = 'mo-ctrl';
        ctrl.innerHTML = `
            <button class="mo-btn mo-del"   title="Удалить">✕</button>
            <button class="mo-btn mo-props" title="Свойства">⚙</button>
            <div    class="mo-resize-handle" title="Изменить размер"></div>
        `;
        wrap.appendChild(ctrl);

        // Events
        wrap.addEventListener('mousedown', e => _onDragStart(e, obj.id));
        wrap.querySelector('.mo-del').addEventListener('click', e => { e.stopPropagation(); _deleteObj(obj.id); });
        wrap.querySelector('.mo-props').addEventListener('click', e => { e.stopPropagation(); _openProps(obj.id); });
        wrap.querySelector('.mo-resize-handle').addEventListener('mousedown', e => { e.stopPropagation(); _onResizeStart(e, obj.id); });
        wrap.addEventListener('click', e => { if (_editMode) { e.stopPropagation(); _selectObject(obj.id); } });

        if (_editMode) wrap.classList.add('mo-editable');
        if (_selectedId === obj.id) wrap.classList.add('mo-selected');

        _mapEl.appendChild(wrap);
    }

    function _buildContent(obj) {
        if (obj.type === 'svg_inline') {
            return `<div class="mo-inner" style="width:100%;height:100%">${obj.src}</div>`;
        }
        if (obj.type === 'image_url') {
            return `<img class="mo-inner" src="${_esc(obj.src)}" style="width:100%;height:auto;display:block" draggable="false">`;
        }
        if (obj.type === 'emoji') {
            return `<div class="mo-inner mo-emoji">${_esc(obj.src)}</div>`;
        }
        return `<div class="mo-inner">${_esc(obj.src)}</div>`;
    }

    function _refreshOne(obj) {
        const old = _mapEl.querySelector(`.mo-wrap[data-id="${obj.id}"]`);
        if (old) old.remove();
        _renderOne(obj);
    }

    // ─── drag ────────────────────────────────────────────────────────────────────
    function _onDragStart(e, id) {
        if (!_editMode) return;
        if (e.target.closest('.mo-ctrl')) return;
        e.preventDefault();
        e.stopPropagation();

        const obj = _objects.find(o => o.id === id);
        const rect = _mapEl.getBoundingClientRect();
        _dragState = {
            id,
            startMouseX: e.clientX,
            startMouseY: e.clientY,
            origX: obj.mapX,
            origY: obj.mapY,
            mapW: rect.width,
            mapH: rect.height,
        };

        _selectObject(id);

        const onMove = ev => {
            if (!_dragState) return;
            const dx = (ev.clientX - _dragState.startMouseX) / _dragState.mapW * 100;
            const dy = (ev.clientY - _dragState.startMouseY) / _dragState.mapH * 100;

            const newX = Math.max(0, Math.min(100, _dragState.origX + dx));
            const newY = Math.max(_minTopPct(), Math.min(100, _dragState.origY + dy));

            obj.mapX = newX;
            obj.mapY = newY;

            const el = _mapEl.querySelector(`.mo-wrap[data-id="${id}"]`);
            if (el) { el.style.left = newX + '%'; el.style.top = newY + '%'; }
        };

        const onUp = async () => {
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
            if (_dragState) {
                await _savePosition(_dragState.id, obj.mapX, obj.mapY);
                _dragState = null;
            }
        };

        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    // ─── resize ──────────────────────────────────────────────────────────────────
    function _onResizeStart(e, id) {
        if (!_editMode) return;
        e.preventDefault();

        const obj = _objects.find(o => o.id === id);
        const startX = e.clientX;
        const origW = obj.width;

        const onMove = ev => {
            const delta = ev.clientX - startX;
            obj.width = Math.max(16, Math.min(600, origW + delta));
            const el = _mapEl.querySelector(`.mo-wrap[data-id="${id}"]`);
            if (el) el.style.width = obj.width + 'px';
        };

        const onUp = async () => {
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
            await _saveProps(id, { width: obj.width });
        };

        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }

    // ─── select ──────────────────────────────────────────────────────────────────
    function _selectObject(id) {
        _selectedId = id;
        _mapEl.querySelectorAll('.mo-wrap').forEach(el => {
            el.classList.toggle('mo-selected', el.dataset.id == id);
        });
        _updatePropsPanel(id);
    }

    // ─── CRUD helpers ────────────────────────────────────────────────────────────
    async function _savePosition(id, mapX, mapY) {
        await fetch(`/api/builder/map-objects/${id}/position`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mapX, mapY }),
        });
    }

    async function _saveProps(id, patch) {
        const obj = _objects.find(o => o.id === id);
        if (!obj) return;
        Object.assign(obj, patch);

        await fetch(`/api/builder/map-objects/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                courseId: obj.courseId,
                type: obj.type,
                src: obj.src,
                label: obj.label,
                mapX: obj.mapX,
                mapY: obj.mapY,
                width: obj.width,
                rotation: obj.rotation,
                zIndex: obj.zIndex,
            }),
        });
        _refreshOne(obj);
    }

    async function _deleteObj(id) {
        if (!confirm('Удалить объект с карты?')) return;
        await fetch(`/api/builder/map-objects/${id}`, { method: 'DELETE' });
        _objects = _objects.filter(o => o.id !== id);
        const el = _mapEl.querySelector(`.mo-wrap[data-id="${id}"]`);
        if (el) el.remove();
        if (_selectedId === id) _selectObject(null);
        _rebuildList();
    }

    async function _createObject(dto) {
        const r = await fetch('/api/builder/map-objects', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ courseId: _courseId, ...dto }),
        });
        const obj = await r.json();
        _objects.push(obj);
        _renderOne(obj);
        _rebuildList();
        return obj;
    }

    // ─── side panel ──────────────────────────────────────────────────────────────
    function _buildPanel() {
        // Если панель уже есть — не дублируем
        if (document.getElementById('mo-panel')) return;

        const panel = document.createElement('div');
        panel.id = 'mo-panel';
        panel.innerHTML = `
<div class="mo-panel-header">
    <span>🗺 Объекты карты</span>
    <div style="display:flex;align-items:center;gap:8px">
        <label class="mo-toggle-label">
            <input type="checkbox" id="mo-edit-toggle"> Режим правки
        </label>
        <button class="mo-btn" id="mo-collapse-btn" title="Свернуть" style="padding:1px 6px;font-size:13px">▼</button>
    </div>
</div>

<div class="mo-panel-body" id="mo-panel-body">
    <!-- Вкладки -->
    <div class="mo-tabs">
        <button class="mo-tab active" data-tab="add">+ Добавить</button>
        <button class="mo-tab"        data-tab="list">Список</button>
        <button class="mo-tab"        data-tab="props">Свойства</button>
    </div>

    <!-- Таб: Добавить -->
    <div class="mo-tab-content active" id="mo-tab-add">
        <div class="mo-section-label">Быстрые объекты</div>
        <div class="mo-presets" id="mo-presets"></div>

        <div class="mo-section-label" style="margin-top:10px">Загрузить PNG / SVG</div>
        <input type="file" id="mo-file-input" accept=".png,.jpg,.jpeg,.webp,.svg,.gif" style="display:none">
        <button class="mo-action-btn" id="mo-upload-btn">📂 Выбрать файл…</button>

        <div class="mo-section-label" style="margin-top:10px">SVG-код вручную</div>
        <textarea id="mo-svg-input" placeholder='<svg ...>...</svg>' rows="4"></textarea>
        <input type="text" id="mo-svg-label" placeholder="Название (необязательно)">
        <button class="mo-action-btn" id="mo-svg-add-btn">Добавить SVG</button>

        <div class="mo-section-label" style="margin-top:10px">Emoji / текст</div>
        <input type="text" id="mo-emoji-input" placeholder="⚡ или любой текст" maxlength="8">
        <button class="mo-action-btn" id="mo-emoji-add-btn">Добавить emoji</button>
    </div>

    <!-- Таб: Список -->
    <div class="mo-tab-content" id="mo-tab-list">
        <div id="mo-object-list" class="mo-obj-list"></div>
    </div>

    <!-- Таб: Свойства -->
    <div class="mo-tab-content" id="mo-tab-props">
        <div id="mo-no-selection" style="color:#888;font-size:12px;padding:8px">
            Кликните объект на карте для выбора
        </div>
        <div id="mo-props-form" style="display:none">
            <label>Ширина (px)
                <input type="number" id="mo-prop-width" min="16" max="600" step="4">
            </label>
            <label>Поворот (°)
                <input type="number" id="mo-prop-rotation" min="-180" max="360" step="5">
            </label>
            <label>Z-Index
                <input type="number" id="mo-prop-zindex" min="-10" max="100">
            </label>
            <label>Подпись
                <input type="text" id="mo-prop-label">
            </label>
            <button class="mo-action-btn mo-apply-btn" id="mo-prop-apply">Применить</button>
            <button class="mo-action-btn mo-del-btn"   id="mo-prop-delete">Удалить</button>
        </div>
    </div>
</div>`;

        // Монтируем в canvas-area (position:relative), а не в body —
        // чтобы панель не перекрывала другие части Builder'а
        const canvas = document.getElementById('bldCanvas') || document.body;
        canvas.appendChild(panel);
        _injectStyles();
        _bindPanelEvents(panel);
        _buildPresets();
        _rebuildList();
    }

    function _bindPanelEvents(panel) {
        // Вкладки
        panel.querySelectorAll('.mo-tab').forEach(btn => {
            btn.addEventListener('click', () => {
                panel.querySelectorAll('.mo-tab,.mo-tab-content').forEach(el => el.classList.remove('active'));
                btn.classList.add('active');
                panel.querySelector(`#mo-tab-${btn.dataset.tab}`).classList.add('active');
            });
        });

        // Edit toggle
        document.getElementById('mo-edit-toggle').addEventListener('change', e => {
            setEditMode(e.target.checked);
        });

        // Кнопка свернуть/развернуть
        document.getElementById('mo-collapse-btn').addEventListener('click', () => {
            const body = document.getElementById('mo-panel-body');
            const btn  = document.getElementById('mo-collapse-btn');
            const collapsed = body.style.display === 'none';
            body.style.display = collapsed ? '' : 'none';
            btn.textContent    = collapsed ? '▼' : '▲';
            btn.title          = collapsed ? 'Свернуть' : 'Развернуть';
        });

        // Upload image
        const fileInput = document.getElementById('mo-file-input');
        document.getElementById('mo-upload-btn').addEventListener('click', () => fileInput.click());
        fileInput.addEventListener('change', async () => {
            const file = fileInput.files[0];
            if (!file) return;
            const fd = new FormData();
            fd.append('file', file);
            const r = await fetch('/api/builder/map-objects/upload-image', { method: 'POST', body: fd });
            const data = await r.json();
            if (data.url) {
                await _createObject({ type: 'image_url', src: data.url, label: file.name, width: 80 });
                _toast('Изображение добавлено на карту');
            } else {
                _toast(data.error || 'Ошибка загрузки', true);
            }
            fileInput.value = '';
        });

        // Add SVG
        document.getElementById('mo-svg-add-btn').addEventListener('click', async () => {
            const src = document.getElementById('mo-svg-input').value.trim();
            const label = document.getElementById('mo-svg-label').value.trim();
            if (!src) { _toast('Введите SVG-код', true); return; }
            await _createObject({ type: 'svg_inline', src, label: label || 'SVG объект', width: 64 });
            document.getElementById('mo-svg-input').value = '';
            document.getElementById('mo-svg-label').value = '';
            _toast('SVG добавлен');
        });

        // Add emoji
        document.getElementById('mo-emoji-add-btn').addEventListener('click', async () => {
            const src = document.getElementById('mo-emoji-input').value.trim();
            if (!src) { _toast('Введите emoji', true); return; }
            await _createObject({ type: 'emoji', src, label: src, width: 48 });
            document.getElementById('mo-emoji-input').value = '';
            _toast('Emoji добавлен');
        });

        // Props apply
        document.getElementById('mo-prop-apply').addEventListener('click', async () => {
            if (!_selectedId) return;
            await _saveProps(_selectedId, {
                width: parseInt(document.getElementById('mo-prop-width').value) || 48,
                rotation: parseFloat(document.getElementById('mo-prop-rotation').value) || 0,
                zIndex: parseInt(document.getElementById('mo-prop-zindex').value) || 0,
                label: document.getElementById('mo-prop-label').value,
            });
            _toast('Сохранено');
        });

        // Props delete
        document.getElementById('mo-prop-delete').addEventListener('click', () => {
            if (_selectedId) _deleteObj(_selectedId);
        });
    }

    // Обновить форму свойств при выборе объекта
    function _updatePropsPanel(id) {
        const form = document.getElementById('mo-props-form');
        const nosel = document.getElementById('mo-no-selection');
        if (!form) return;

        const obj = _objects.find(o => o.id === id);
        if (!obj) {
            form.style.display = 'none';
            nosel.style.display = 'block';
            return;
        }
        form.style.display = 'block';
        nosel.style.display = 'none';
        document.getElementById('mo-prop-width').value = obj.width;
        document.getElementById('mo-prop-rotation').value = obj.rotation;
        document.getElementById('mo-prop-zindex').value = obj.zIndex;
        document.getElementById('mo-prop-label').value = obj.label;
    }

    function _rebuildList() {
        const list = document.getElementById('mo-obj-list');
        if (!list) return;
        if (_objects.length === 0) {
            list.innerHTML = '<div style="color:#888;font-size:12px;padding:8px">Объектов нет</div>';
            return;
        }
        list.innerHTML = _objects.map(o => `
            <div class="mo-list-item ${_selectedId === o.id ? 'mo-list-selected' : ''}"
                 data-id="${o.id}">
                <span class="mo-list-icon">${_typeIcon(o.type)}</span>
                <span class="mo-list-label">${_esc(o.label || o.type)}</span>
                <button class="mo-list-del" data-id="${o.id}">✕</button>
            </div>`
        ).join('');

        list.querySelectorAll('.mo-list-item').forEach(el => {
            el.addEventListener('click', () => {
                _selectObject(parseInt(el.dataset.id));
                // Переключить на вкладку свойств
                document.querySelector('.mo-tab[data-tab="props"]').click();
            });
        });
        list.querySelectorAll('.mo-list-del').forEach(btn => {
            btn.addEventListener('click', e => {
                e.stopPropagation();
                _deleteObj(parseInt(btn.dataset.id));
            });
        });
    }

    function _openProps(id) {
        _selectObject(id);
        const propsTab = document.querySelector('.mo-tab[data-tab="props"]');
        if (propsTab) propsTab.click();
    }

    // ─── presets (встроенные SVG для карты) ─────────────────────────────────────
    const PRESETS = [
        {
            label: '🌲 Дерево',
            type: 'svg_inline',
            width: 40,
            src: `<svg viewBox="0 0 32 48" xmlns="http://www.w3.org/2000/svg">
                      <polygon points="16,2 30,22 22,22 28,38 4,38 10,22 2,22" fill="#2d8a4e" stroke="#1a5c33" stroke-width="1.5"/>
                      <rect x="13" y="38" width="6" height="8" fill="#7b4f2e"/>
                    </svg>`,
        },
        {
            label: '🪨 Камень',
            type: 'svg_inline',
            width: 44,
            src: `<svg viewBox="0 0 48 32" xmlns="http://www.w3.org/2000/svg">
                      <ellipse cx="24" cy="20" rx="22" ry="11" fill="#888" stroke="#555" stroke-width="1.5"/>
                      <ellipse cx="22" cy="18" rx="16" ry="7"  fill="#aaa"/>
                    </svg>`,
        },
        {
            label: '💧 Вода',
            type: 'svg_inline',
            width: 56,
            src: `<svg viewBox="0 0 64 24" xmlns="http://www.w3.org/2000/svg">
                      <rect width="64" height="24" rx="4" fill="#1a7fa8" opacity=".7"/>
                      <path d="M0 10 Q16 4 32 10 Q48 16 64 10 L64 24 L0 24Z" fill="#2299cc" opacity=".5"/>
                    </svg>`,
        },
        {
            label: '⭐ Звезда',
            type: 'svg_inline',
            width: 40,
            src: `<svg viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                      <polygon points="16,2 20,12 31,12 22,19 25,30 16,23 7,30 10,19 1,12 12,12"
                               fill="#ffd700" stroke="#c8a000" stroke-width="1"/>
                    </svg>`,
        },
        {
            label: '🏔 Гора',
            type: 'svg_inline',
            width: 60,
            src: `<svg viewBox="0 0 64 48" xmlns="http://www.w3.org/2000/svg">
                      <polygon points="32,2 60,46 4,46" fill="#6b7280" stroke="#4b5563" stroke-width="1.5"/>
                      <polygon points="32,2 42,22 22,22" fill="#e5e7eb"/>
                    </svg>`,
        },
        {
            label: '🏰 Замок',
            type: 'svg_inline',
            width: 52,
            src: `<svg viewBox="0 0 52 52" xmlns="http://www.w3.org/2000/svg">
                      <rect x="6"  y="20" width="40" height="30" fill="#9ca3af" stroke="#6b7280" stroke-width="1.5"/>
                      <rect x="2"  y="14" width="10" height="16" fill="#9ca3af" stroke="#6b7280" stroke-width="1.5"/>
                      <rect x="40" y="14" width="10" height="16" fill="#9ca3af" stroke="#6b7280" stroke-width="1.5"/>
                      <rect x="2"  y="10" width="3" height="6" fill="#6b7280"/>
                      <rect x="7"  y="10" width="3" height="6" fill="#6b7280"/>
                      <rect x="40" y="10" width="3" height="6" fill="#6b7280"/>
                      <rect x="45" y="10" width="3" height="6" fill="#6b7280"/>
                      <rect x="21" y="35" width="10" height="15" fill="#4b5563"/>
                      <rect x="22" y="18" width="8"  height="8"  fill="#60a5fa" opacity=".6"/>
                    </svg>`,
        },
        {
            label: '🛤 Дорога',
            type: 'svg_inline',
            width: 80,
            src: `<svg viewBox="0 0 80 20" xmlns="http://www.w3.org/2000/svg">
                      <rect width="80" height="20" fill="#6b7280"/>
                      <rect x="0"  y="9" width="20" height="2" fill="#fbbf24"/>
                      <rect x="30" y="9" width="20" height="2" fill="#fbbf24"/>
                      <rect x="60" y="9" width="20" height="2" fill="#fbbf24"/>
                    </svg>`,
        },
        {
            label: '🌸 Цветок',
            type: 'svg_inline',
            width: 36,
            src: `<svg viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                      <circle cx="16" cy="16" r="5"  fill="#fbbf24"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#f472b6" transform="rotate(0   16 16)"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#f472b6" transform="rotate(60  16 16)"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#f472b6" transform="rotate(120 16 16)"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#ec4899" transform="rotate(180 16 16)"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#ec4899" transform="rotate(240 16 16)"/>
                      <ellipse cx="16" cy="6"  rx="4" ry="6" fill="#ec4899" transform="rotate(300 16 16)"/>
                    </svg>`,
        },
        {
            label: '⚡ Молния',
            type: 'svg_inline',
            width: 32,
            src: `<svg viewBox="0 0 24 40" xmlns="http://www.w3.org/2000/svg">
                      <polygon points="14,0 4,22 12,22 8,40 22,16 13,16 20,0"
                               fill="#fbbf24" stroke="#d97706" stroke-width="1"/>
                    </svg>`,
        },
        {
            label: '🔮 Кристалл',
            type: 'svg_inline',
            width: 36,
            src: `<svg viewBox="0 0 32 44" xmlns="http://www.w3.org/2000/svg">
                      <polygon points="16,2 28,14 16,42 4,14" fill="#a78bfa" stroke="#7c3aed" stroke-width="1.5"/>
                      <polygon points="16,2 28,14 16,20 4,14"  fill="#c4b5fd"/>
                    </svg>`,
        },
    ];

    function _buildPresets() {
        const container = document.getElementById('mo-presets');
        if (!container) return;
        container.innerHTML = PRESETS.map((p, i) => `
            <button class="mo-preset-btn" title="${_esc(p.label)}" data-idx="${i}">
                ${p.type === 'svg_inline' ? p.src : p.label}
            </button>`
        ).join('');

        container.querySelectorAll('.mo-preset-btn').forEach(btn => {
            btn.addEventListener('click', async () => {
                const p = PRESETS[parseInt(btn.dataset.idx)];
                await _createObject({ type: p.type, src: p.src, label: p.label, width: p.width });
                _toast(`${p.label} добавлен`);
            });
        });
    }

    // ─── styles ──────────────────────────────────────────────────────────────────
    function _injectStyles() {
        if (document.getElementById('mo-styles')) return;
        const style = document.createElement('style');
        style.id = 'mo-styles';
        style.textContent = `
/* ── Wrap ──────────────────────────────────────── */
.mo-wrap            { box-sizing: border-box; }
.mo-wrap .mo-ctrl   { display: none; }
.mo-wrap.mo-editable { outline: 1px dashed rgba(255,255,255,.3); }
.mo-wrap.mo-editable:hover { outline: 1px dashed #7c6fff; }
.mo-wrap.mo-editable .mo-ctrl { display: flex; position: absolute;
    top: -20px; right: 0; gap: 2px; align-items: center; }
.mo-wrap.mo-selected { outline: 2px solid #7c6fff !important; }

.mo-btn { background: #1a1a2e; border: 1px solid #7c6fff; color: #fff;
    font-size: 11px; padding: 1px 5px; border-radius: 3px; cursor: pointer; line-height:1.4; }
.mo-btn:hover { background: #7c6fff; }

.mo-resize-handle { width: 10px; height: 10px; background: #7c6fff;
    border-radius: 2px; cursor: ew-resize; margin-left: 2px; }

.mo-emoji .mo-inner { font-size: 200%; line-height: 1; }
.mo-emoji           { display: flex; align-items:center; justify-content:center; }

/* ── Panel ──────────────────────────────────────── */
#mo-panel {
    position: absolute; right: 8px; bottom: 8px;
    width: 240px; max-height: 480px;
    background: #12121f; border: 1px solid #2a2a4a;
    border-radius: 8px; color: #e0e0f0; font-size: 13px;
    box-shadow: 0 4px 24px rgba(0,0,0,.6);
    z-index: 100; display: flex; flex-direction: column;
    overflow: hidden;
}
.mo-panel-header {
    display: flex; justify-content: space-between; align-items: center;
    padding: 8px 10px; background: #1a1a2e; font-weight: 600;
    border-bottom: 1px solid #2a2a4a; font-size:12px;
    cursor: default;
}
.mo-toggle-label { font-size:11px; display:flex; gap:4px; align-items:center; cursor:pointer; }
.mo-panel-body   { overflow-y: auto; flex: 1; }

/* ── Tabs ──────────────────────────────────────── */
.mo-tabs         { display: flex; border-bottom: 1px solid #2a2a4a; }
.mo-tab          { flex:1; padding:5px 4px; background:none; border:none;
    color:#888; cursor:pointer; font-size:11px; }
.mo-tab.active   { color:#7c6fff; border-bottom:2px solid #7c6fff; }
.mo-tab-content  { display:none; padding:8px; }
.mo-tab-content.active { display:block; }

/* ── Controls ──────────────────────────────────── */
.mo-section-label { font-size:10px; color:#666; text-transform:uppercase;
    letter-spacing:.5px; margin-bottom:4px; }
.mo-action-btn  { width:100%; padding:5px 8px; background:#1e1e3a;
    border:1px solid #3a3a6a; color:#c0c0f0; border-radius:4px;
    cursor:pointer; font-size:12px; margin-top:3px; }
.mo-action-btn:hover { background:#2a2a5a; }
.mo-del-btn     { background:#3a1a1a !important; border-color:#7a2a2a !important; }
.mo-del-btn:hover { background:#5a2a2a !important; }

#mo-panel textarea, #mo-panel input[type=text], #mo-panel input[type=number] {
    width: 100%; box-sizing: border-box;
    background: #0d0d1a; border: 1px solid #2a2a4a;
    color: #e0e0f0; border-radius: 4px; padding: 4px 6px;
    font-size: 12px; margin-bottom: 3px;
}
#mo-panel label { display:block; font-size:11px; color:#888; margin-bottom:2px; }

/* ── Presets ──────────────────────────────────── */
.mo-presets { display: flex; flex-wrap: wrap; gap: 4px; }
.mo-preset-btn { width: 42px; height: 42px; background: #1a1a2e;
    border: 1px solid #2a2a4a; border-radius: 4px; cursor: pointer;
    display: flex; align-items: center; justify-content: center;
    padding: 3px; overflow: hidden; }
.mo-preset-btn svg { width: 100%; height: 100%; }
.mo-preset-btn:hover { border-color: #7c6fff; background: #2a2a4a; }

/* ── List ──────────────────────────────────────── */
.mo-obj-list   { display: flex; flex-direction: column; gap: 2px; }
.mo-list-item  { display: flex; align-items: center; gap: 6px; padding: 4px 6px;
    border-radius: 4px; cursor: pointer; }
.mo-list-item:hover         { background: #1a1a2e; }
.mo-list-selected           { background: #1e1a3a; outline: 1px solid #7c6fff; }
.mo-list-icon               { font-size: 14px; }
.mo-list-label              { flex: 1; font-size: 11px; overflow: hidden;
    text-overflow: ellipsis; white-space: nowrap; }
.mo-list-del   { background:none; border:none; color:#666; cursor:pointer; font-size:12px; }
.mo-list-del:hover          { color: #ff6b6b; }

/* ── Toast ─────────────────────────────────────── */
.mo-toast { position:fixed; bottom:70px; right:20px; background:#2a2a5a;
    color:#e0e0f0; padding:6px 14px; border-radius:6px; font-size:12px;
    z-index:10000; pointer-events:none; opacity:0;
    transition: opacity .2s; }
.mo-toast.show { opacity:1; }
        `;
        document.head.appendChild(style);
    }

    // ─── utils ───────────────────────────────────────────────────────────────────
    function _typeIcon(type) {
        return { svg_inline: '⬡', image_url: '🖼', emoji: '😀' }[type] ?? '◻';
    }

    function _esc(s) {
        return String(s ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    let _toastTimer = null;
    function _toast(msg, isError = false) {
        let el = document.querySelector('.mo-toast');
        if (!el) { el = document.createElement('div'); el.className = 'mo-toast'; document.body.appendChild(el); }
        el.textContent = msg;
        el.style.background = isError ? '#3a1a1a' : '#2a2a5a';
        el.classList.add('show');
        clearTimeout(_toastTimer);
        _toastTimer = setTimeout(() => el.classList.remove('show'), 2500);
    }

    // ─── public ──────────────────────────────────────────────────────────────────
    return { init, setEditMode, loadAll, renderAll };

})();
