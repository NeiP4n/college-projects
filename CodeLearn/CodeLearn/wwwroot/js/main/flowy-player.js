// Flowy-плеер: вставь ссылку YouTube / Spotify / SoundCloud.
// Все три управляются НАШИМИ кнопками (play/pause/next/seek/vol), iframe скрытый —
// компактный интерфейс не меняется. Состояние хранится → музыка переживает переходы.
(function () {
    'use strict';
    if (window.__flowyInit) return;
    window.__flowyInit = true;

    var ICON_PLAY  = '<svg viewBox="0 0 12 12" shape-rendering="crispEdges"><rect x="3" y="2" width="2" height="8"/><rect x="5" y="3" width="2" height="6"/><rect x="7" y="4" width="2" height="4"/></svg>';
    var ICON_PAUSE = '<svg viewBox="0 0 12 12" shape-rendering="crispEdges"><rect x="3" y="2" width="2" height="8"/><rect x="7" y="2" width="2" height="8"/></svg>';

    var $ = function (id) { return document.getElementById(id); };
    var flowyEl = $('flowy'); if (!flowyEl) return;
    var vibeEl = $('flowyVibe'), searchB = $('flowySearch'), collapseB = $('flowyCollapse'),
        prevB = $('flowyPrev'), playB = $('flowyPlay'), nextB = $('flowyNext'),
        volDn = $('flowyVolDown'), volUp = $('flowyVolUp'), iconEl = $('flowyIcon'),
        nowEl = $('flowyNow'), volEl = $('flowyVol'), barEl = $('flowyBar'),
        fillEl = $('flowyBarFill'), timeEl = $('flowyTime'), embedEl = $('flowyEmbed'),
        flowyDl = $('flowyDl');
    var curTrackUrl = '';   // ссылка на текущий трек (для кнопки скачивания)

    var SKEY = 'flowy_state';
    function loadSt() { try { return JSON.parse(localStorage.getItem(SKEY) || 'null'); } catch (e) { return null; } }
    function save() { try { localStorage.setItem(SKEY, JSON.stringify(st)); } catch (e) {} }
    var st = loadSt() || { prov: null };
    var vol = parseInt(localStorage.getItem('flowy_vol') || '50', 10);
    var active = null;
    showVol();
    if (st.vibe) vibeEl.value = st.vibe;

    // ── Утилиты UI ────────────────────────────────────────────────────────────
    function setPlayIcon(on) { playB.innerHTML = on ? ICON_PAUSE : ICON_PLAY; iconEl.classList.toggle('playing', on); }
    function setNow(t) { if (nowEl) nowEl.textContent = t || ''; }
    function fmt(s) { s = Math.floor(s || 0); var m = Math.floor(s / 60), ss = s % 60; return m + ':' + (ss < 10 ? '0' : '') + ss; }
    function setBar(cur, dur) {
        if (!isFinite(dur) || dur <= 0) { if (fillEl) fillEl.style.width = '100%'; if (timeEl) timeEl.textContent = 'LIVE'; }
        else { if (fillEl) fillEl.style.width = (cur / dur * 100).toFixed(1) + '%'; if (timeEl) timeEl.textContent = fmt(cur) + ' / ' + fmt(dur); }
    }
    function showVol() { if (volEl) volEl.textContent = vol + '%'; }

    // ── Распознавание ссылки ──────────────────────────────────────────────────
    function detect(raw) {
        var s = (raw || '').trim(); if (!s) return null;
        var sp = s.match(/open\.spotify\.com\/(?:intl-[\w-]+\/)?(track|playlist|album|episode|artist|show)\/(\w+)/)
              || s.match(/spotify:(track|playlist|album|episode|artist|show):(\w+)/);
        if (sp) return { prov: 'spotify', uri: 'spotify:' + sp[1] + ':' + sp[2] };
        if (/soundcloud\.com\//i.test(s)) return { prov: 'soundcloud', scUrl: s };
        var pl = s.match(/[?&]list=([\w-]+)/); if (pl) return { prov: 'youtube', type: 'playlist', id: pl[1] };
        var v = s.match(/(?:v=|youtu\.be\/|\/embed\/|\/shorts\/)([\w-]{11})/); if (v) return { prov: 'youtube', type: 'video', id: v[1] };
        if (/^[\w-]{11}$/.test(s)) return { prov: 'youtube', type: 'video', id: s };
        return null;
    }

    // тот же источник, что уже загружен сейчас?
    function sameSource(d) {
        if (!st || st.prov !== d.prov) return false;
        if (d.prov === 'youtube')    return st.type === d.type && st.id === d.id;
        if (d.prov === 'soundcloud') return st.scUrl === d.scUrl;
        if (d.prov === 'spotify')    return st.uri === d.uri;
        return false;
    }

    // реально ли что-то играет прямо сейчас?
    function isPlayingNow() {
        if (active === 'youtube')    return ytReady && ytPlayer.getPlayerState && ytPlayer.getPlayerState() === 1;
        if (active === 'soundcloud') return !!scPlaying;
        if (active === 'spotify')    return spIsPlaying;
        return false;
    }

    function loadFromInput() {
        var d = detect(vibeEl.value);
        if (!d) { setNow('нужна ссылка YouTube / Spotify / SoundCloud'); return; }
        // не перезапускаем ТОЛЬКО если этот же трек уже реально играет
        if (sameSource(d) && active === d.prov && isPlayingNow()) { vibeEl.blur(); return; }
        d.vibe = vibeEl.value;
        applyLoad(d, true); vibeEl.blur();
    }

    function stopOthers(keep) {
        if (keep !== 'youtube' && ytPlayer && ytPlayer.pauseVideo) try { ytPlayer.pauseVideo(); } catch (e) {}
        if (keep !== 'soundcloud' && scWidget) try { scWidget.pause(); } catch (e) {}
        if (keep !== 'spotify' && spCtrl) try { spCtrl.pause(); } catch (e) {}
    }

    function applyLoad(d, fresh) {
        stopOthers(d.prov);
        active = d.prov;
        setBar(0, 0); setNow('');
        if (d.prov === 'youtube') {
            clearEmbed();
            st = { prov: 'youtube', type: d.type, id: d.id, time: fresh ? 0 : (st.time || 0), playing: true, vibe: d.vibe || st.vibe || '', index: 0 };
            save();
            ensureYT(function () {
                if (d.type === 'playlist') ytPlayer.loadPlaylist({ list: d.id, listType: 'playlist', index: 0 });
                // одиночное видео → радио-микс RD<id>: ютуб продолжит похожими треками
                else ytPlayer.loadPlaylist({ list: 'RD' + d.id, listType: 'playlist', index: 0 });
                ytPlayer.setVolume(vol);
            });
        } else if (d.prov === 'soundcloud') {
            st = { prov: 'soundcloud', scUrl: d.scUrl, scCurUrl: d.scUrl, scPos: 0, vibe: d.vibe || st.vibe || '' }; save();
            loadSC(d.scUrl, /\/sets\//i.test(d.scUrl), 0);
        } else if (d.prov === 'spotify') {
            st = { prov: 'spotify', uri: d.uri, spPos: 0, vibe: d.vibe || st.vibe || '' }; save();
            loadSpotify(d.uri);
        }
    }
    function clearEmbed() { embedEl.innerHTML = ''; scWidget = null; spCtrl = null; }

    // ── YouTube ─────────────────────────────────────────────────────────────────
    var ytPlayer, ytReady = false, ytQueue = [], isLiveYT = false, ytNeedMix = false, ytMixSeed = null;
    function ensureYT(cb) {
        if (ytReady) { cb && cb(); return; }
        if (cb) ytQueue.push(cb);
        if (ytPlayer) return;
        if (!window.YT || !window.YT.Player) {
            if (!window.__flowyYTApi) { window.__flowyYTApi = true;
                var p = window.onYouTubeIframeAPIReady;
                window.onYouTubeIframeAPIReady = function () { if (typeof p === 'function') try { p(); } catch (e) {} createYT(); };
                var t = document.createElement('script'); t.src = 'https://www.youtube.com/iframe_api'; document.head.appendChild(t);
            }
        } else createYT();
    }
    function createYT() {
        if (ytPlayer) return;
        ytPlayer = new YT.Player('flowyYT', {
            height: '100%', width: '100%', playerVars: { autoplay: 0, controls: 0, disablekb: 1, playsinline: 1 },
            events: {
                onReady: function () { ytReady = true; ytPlayer.setVolume(vol); setInterval(ytTick, 1000); var q = ytQueue; ytQueue = []; q.forEach(function (f) { f(); }); },
                onStateChange: function (e) {
                    if (active !== 'youtube') return;
                    if (e.data === 1) { setPlayIcon(true); st.playing = true; save(); }
                    else if (e.data === 2) { setPlayIcon(false); st.playing = false; save(); }
                    else if (e.data === 0) {   // трек закончился
                        setPlayIcon(false);
                        // после возобновлённого одиночного трека → продолжаем радио-миксом
                        if (ytNeedMix && ytMixSeed) { ytNeedMix = false; try { ytPlayer.loadPlaylist({ list: 'RD' + ytMixSeed, listType: 'playlist' }); } catch (x) {} }
                    }
                    try {
                        var d = ytPlayer.getVideoData();
                        if (d) {
                            if (d.title) { st.label = d.title; setNow(d.title); }
                            if (d.video_id) { st.curId = d.video_id; curTrackUrl = 'https://www.youtube.com/watch?v=' + d.video_id; save(); }
                        }
                    } catch (x) {}
                },
                onError: function () {
                    if (active === 'youtube' && st.type === 'video' && st.id) {
                        try { ytPlayer.loadVideoById(st.id); } catch (x) {}
                    }
                }
            }
        });
    }
    function ytTick() {
        if (active !== 'youtube' || !ytReady || !ytPlayer.getCurrentTime) return;
        var cur = ytPlayer.getCurrentTime() || 0, dur = ytPlayer.getDuration() || 0;
        isLiveYT = !isFinite(dur) || dur <= 0; setBar(cur, dur);
        st.time = cur; try { st.index = ytPlayer.getPlaylistIndex(); } catch (e) {} save();
    }

    // ── SoundCloud (Widget API) ──────────────────────────────────────────────────
    var scWidget = null, scIsSet = false, scSeekTo = 0, scWatch = null, scPlaying = false;
    function loadStation(permalink) {
        if (!permalink) return;
        // станция трека: soundcloud.com/<path> → soundcloud.com/stations/track/<path>
        var station = permalink.replace(/^https?:\/\/(www\.)?soundcloud\.com\//i, 'https://soundcloud.com/stations/track/');
        scIsSet = true; scPlaying = false;
        scWidget.load(station, { auto_play: true, visual: false, show_comments: false });
        // watchdog: если станция не заиграла за 5с — не зависаем, возвращаем трек
        if (scWatch) clearTimeout(scWatch);
        scWatch = setTimeout(function () {
            if (active === 'soundcloud' && !scPlaying) {
                setNow('рекомендации недоступны для этого трека');
                loadSC(st.scCurUrl || st.scUrl, false, 0);
            }
        }, 5000);
    }
    function loadSC(url, isSet, seekMs) {
        scIsSet = !!isSet || /\/sets\//i.test(url) || /track-stations:/.test(url);
        scSeekTo = seekMs || 0;
        embedEl.innerHTML = '<iframe id="flowySC" allow="autoplay" src="https://w.soundcloud.com/player/?url=' +
            encodeURIComponent(url) + '&auto_play=true&visual=false&hide_related=true&show_comments=false&buying=false&sharing=false"></iframe>';
        ensureSCApi(function () {
            scWidget = SC.Widget(document.getElementById('flowySC'));
            scWidget.bind(SC.Widget.Events.READY, function () { scWidget.setVolume(vol); });
            scWidget.bind(SC.Widget.Events.PLAY, function () {
                if (active !== 'soundcloud') return;
                scPlaying = true; if (scWatch) { clearTimeout(scWatch); scWatch = null; }
                setPlayIcon(true);
                scWidget.getCurrentSound(function (s) {
                    if (s) {
                        if (s.title) setNow(s.title);
                        if (s.permalink_url) { curTrackUrl = s.permalink_url; st.scCurUrl = s.permalink_url; save(); }
                    }
                });
                if (scSeekTo > 0) { scWidget.seekTo(scSeekTo); scSeekTo = 0; }
            });
            scWidget.bind(SC.Widget.Events.PAUSE, function () { if (active === 'soundcloud') setPlayIcon(false); });
            scWidget.bind(SC.Widget.Events.PLAY_PROGRESS, function (e) {
                if (active !== 'soundcloud') return;
                st.scPos = e.currentPosition; save();
                scWidget.getDuration(function (d) { setBar(e.currentPosition / 1000, d / 1000); });
            });
            // Трек/сет закончился → станция (похожие треки)
            scWidget.bind(SC.Widget.Events.FINISH, function () {
                if (active !== 'soundcloud') return;
                if (scIsSet) { scWidget.next(); return; }   // в сете/станции — дальше сама
                scWidget.getCurrentSound(function (s) { if (s) loadStation(s.permalink_url); });
            });
        });
    }
    function ensureSCApi(cb) {
        if (window.SC && window.SC.Widget) { cb(); return; }
        if (!window.__flowySCApi) { window.__flowySCApi = true;
            var s = document.createElement('script'); s.src = 'https://w.soundcloud.com/player/api.js'; document.head.appendChild(s);
        }
        var tries = 0; (function wait() { if (window.SC && window.SC.Widget) cb(); else if (tries++ < 60) setTimeout(wait, 150); })();
    }

    // ── Spotify (IFrame API) ──────────────────────────────────────────────────────
    var spCtrl = null, spAPI = null, spPending = null, spIsPlaying = false;
    function loadSpotify(uri, seekMs) {
        // ссылка для кнопки скачивания: spotify:track:ID → open.spotify.com/track/ID
        var parts = uri.split(':'); if (parts.length === 3) curTrackUrl = 'https://open.spotify.com/' + parts[1] + '/' + parts[2];
        embedEl.innerHTML = '<div id="flowySP"></div>';
        ensureSpAPI(function () {
            spAPI.createController(document.getElementById('flowySP'), { uri: uri, width: '100%', height: 80 }, function (ctrl) {
                spCtrl = ctrl;
                ctrl.addListener('playback_update', function (e) {
                    if (active !== 'spotify') return;
                    spLastDur = e.data.duration || 0;
                    st.spPos = e.data.position || 0; save();
                    setBar((e.data.position || 0) / 1000, (e.data.duration || 0) / 1000);
                    spIsPlaying = !e.data.isPaused;
                    setPlayIcon(!e.data.isPaused);
                });
                ctrl.addListener('ready', function () {
                    setNow('Spotify');
                    try { ctrl.play(); } catch (x) {}
                    if (seekMs > 0) setTimeout(function () { try { ctrl.seek(seekMs / 1000); } catch (x) {} }, 700);
                });
            });
        });
    }
    function ensureSpAPI(cb) {
        if (spAPI) { cb(); return; }
        spPending = cb;
        if (!window.__flowySpApi) { window.__flowySpApi = true;
            window.onSpotifyIframeApiReady = function (API) { spAPI = API; if (spPending) { var f = spPending; spPending = null; f(); } };
            var s = document.createElement('script'); s.src = 'https://open.spotify.com/embed/iframe-api/v1'; s.async = true; document.head.appendChild(s);
        }
    }

    // ── Контролы (роутинг по активному провайдеру) ─────────────────────────────
    searchB.addEventListener('click', loadFromInput);
    vibeEl.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); loadFromInput(); } });

    playB.addEventListener('click', function () {
        if (active === 'youtube') { if (!ytReady) { loadFromInput(); return; } var s = ytPlayer.getPlayerState(); s === 1 ? ytPlayer.pauseVideo() : ytPlayer.playVideo(); }
        else if (active === 'soundcloud' && scWidget) scWidget.toggle();
        else if (active === 'spotify' && spCtrl) spCtrl.togglePlay();
        else loadFromInput();
    });
    nextB.addEventListener('click', function () {
        if (active === 'youtube' && ytReady) { try { if (ytPlayer.getPlaylist() && ytPlayer.getPlaylist().length) ytPlayer.nextVideo(); } catch (e) {} }
        else if (active === 'soundcloud' && scWidget) {
            // в сете/станции — следующий трек; для одиночного — грузим станцию (похожие)
            if (scIsSet) scWidget.next();
            else scWidget.getCurrentSound(function (s) { if (s) loadStation(s.permalink_url); });
        }
    });
    prevB.addEventListener('click', function () {
        if (active === 'youtube' && ytReady) { var h = false; try { h = ytPlayer.getPlaylist() && ytPlayer.getPlaylist().length > 1; } catch (e) {} h ? ytPlayer.previousVideo() : ytPlayer.seekTo(0, true); }
        else if (active === 'soundcloud' && scWidget) scWidget.prev();
        else if (active === 'spotify' && spCtrl) spCtrl.seek(0);
    });
    barEl.addEventListener('click', function (e) {
        var r = barEl.getBoundingClientRect(); var ratio = Math.min(1, Math.max(0, (e.clientX - r.left) / r.width));
        if (active === 'youtube' && ytReady && !isLiveYT) { var d = ytPlayer.getDuration() || 0; if (d > 0) ytPlayer.seekTo(ratio * d, true); }
        else if (active === 'soundcloud' && scWidget) scWidget.getDuration(function (d) { scWidget.seekTo(ratio * d); });
        else if (active === 'spotify' && spCtrl) { /* длительность из последнего playback_update */ if (spLastDur > 0) spCtrl.seek(ratio * spLastDur / 1000); }
    });
    var spLastDur = 0;
    volDn.addEventListener('click', function () { vol = Math.max(0, vol - 5); applyVol(); });
    volUp.addEventListener('click', function () { vol = Math.min(100, vol + 5); applyVol(); });
    function applyVol() {
        showVol(); localStorage.setItem('flowy_vol', String(vol));
        if (active === 'youtube' && ytReady) ytPlayer.setVolume(vol);
        else if (active === 'soundcloud' && scWidget) scWidget.setVolume(vol);
        // Spotify embed не даёт менять громкость через API
    }

    // Скачивание текущего трека в .mp3 — автоматически (fetch → файл), без ухода со страницы
    if (flowyDl) flowyDl.addEventListener('click', function () {
        if (!curTrackUrl) { setNow('сначала запусти трек'); return; }
        if (active === 'spotify') { setNow('Spotify защищён DRM — скачать нельзя'); return; }
        setNow('скачивание .mp3…');
        fetch('/api/flowy/download?url=' + encodeURIComponent(curTrackUrl))
            .then(function (r) { if (!r.ok) return r.text().then(function (t) { throw new Error(t || 'ошибка'); }); return r.blob(); })
            .then(function (b) {
                var a = document.createElement('a');
                a.href = URL.createObjectURL(b);
                a.download = (st.label || 'track').replace(/[\\/:*?"<>|]+/g, '_') + '.mp3';
                document.body.appendChild(a); a.click();
                setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 1000);
                setNow('скачано ✓');
            })
            .catch(function (e) { setNow('не удалось скачать: ' + (e.message || '').slice(0, 60)); });
    });

    // ── Возобновление при загрузке ─────────────────────────────────────────────
    if (st.prov === 'youtube' && st.id) {
        active = 'youtube'; setNow(st.label || '');
        ensureYT(function () {
            if (st.type === 'playlist') {
                var o = { list: st.id, listType: 'playlist', index: st.index || 0 };
                st.playing ? ytPlayer.loadPlaylist(o) : ytPlayer.cuePlaylist(o);
            } else {
                // возобновляем ТЕКУЩИЙ трек с сохранённой секунды; когда кончится → радио-микс
                var seed = st.curId || st.id;
                var o2 = { videoId: seed, startSeconds: Math.max(0, st.time || 0) };
                st.playing ? ytPlayer.loadVideoById(o2) : ytPlayer.cueVideoById(o2);
                ytMixSeed = seed; ytNeedMix = true;
            }
        });
    } else if (st.prov === 'soundcloud' && (st.scCurUrl || st.scUrl)) {
        active = 'soundcloud'; loadSC(st.scCurUrl || st.scUrl, false, st.scPos || 0);
    } else if (st.prov === 'spotify' && st.uri) {
        active = 'spotify'; loadSpotify(st.uri, st.spPos || 0);
    }

    window.addEventListener('beforeunload', function () {
        if (active === 'youtube' && ytReady && ytPlayer.getCurrentTime) { st.time = ytPlayer.getCurrentTime() || 0; save(); }
    });

    // ── Перетаскивание + сворачивание ─────────────────────────────────────────
    try { var pos = JSON.parse(localStorage.getItem('flowy_pos') || 'null'); if (pos && typeof pos.l === 'number') { flowyEl.style.left = pos.l + 'px'; flowyEl.style.top = pos.t + 'px'; flowyEl.style.right = 'auto'; } } catch (e) {}
    if (localStorage.getItem('flowy_collapsed') === '1') flowyEl.classList.add('collapsed');

    // ── Размер видео-окна (запоминаем то, что выставил пользователь) ──
    var syncPanelWidth = function () {};   // переопределится ниже, если есть видео-окно
    var screenEl = $('flowyScreen');
    if (screenEl) {
        var PANEL_PAD = 22; // горизонтальные паддинги панели (11+11)
        syncPanelWidth = function () {
            // в свёрнутом виде ширину не навязываем (панель = только иконка)
            if (flowyEl.classList.contains('collapsed')) { flowyEl.style.width = ''; return; }
            // панель обхватывает видео-окно по ширине (но не уже 360px)
            flowyEl.style.width = Math.max(360, screenEl.offsetWidth + PANEL_PAD) + 'px';
        };
        try {
            var sz = JSON.parse(localStorage.getItem('flowy_screen') || 'null');
            if (sz && sz.w) { screenEl.style.width = sz.w + 'px'; screenEl.style.height = sz.h + 'px'; }
        } catch (e) {}
        if (window.ResizeObserver) {
            var roTimer = null;
            new ResizeObserver(function () {
                syncPanelWidth();
                clearTimeout(roTimer);
                roTimer = setTimeout(function () {
                    localStorage.setItem('flowy_screen', JSON.stringify({
                        w: Math.round(screenEl.offsetWidth), h: Math.round(screenEl.offsetHeight)
                    }));
                }, 250);
            }).observe(screenEl);
        }
        syncPanelWidth();
    }
    function setCollapsed(on) { flowyEl.classList.toggle('collapsed', on); localStorage.setItem('flowy_collapsed', on ? '1' : '0'); syncPanelWidth(); }
    collapseB.addEventListener('click', function () { setCollapsed(!flowyEl.classList.contains('collapsed')); });

    var drag = null;
    iconEl.addEventListener('mousedown', function (e) { e.preventDefault(); var r = flowyEl.getBoundingClientRect(); drag = { sx: e.clientX, sy: e.clientY, ox: r.left, oy: r.top, moved: false }; });
    document.addEventListener('mousemove', function (e) {
        if (!drag) return; var dx = e.clientX - drag.sx, dy = e.clientY - drag.sy;
        if (!drag.moved && Math.abs(dx) + Math.abs(dy) < 4) return;
        drag.moved = true; flowyEl.classList.add('dragging');
        var w = flowyEl.offsetWidth, h = flowyEl.offsetHeight;
        flowyEl.style.left = Math.max(0, Math.min(window.innerWidth - w, drag.ox + dx)) + 'px';
        flowyEl.style.top = Math.max(0, Math.min(window.innerHeight - h, drag.oy + dy)) + 'px';
        flowyEl.style.right = 'auto';
    });
    document.addEventListener('mouseup', function () {
        if (!drag) return; flowyEl.classList.remove('dragging');
        if (drag.moved) { var r = flowyEl.getBoundingClientRect(); localStorage.setItem('flowy_pos', JSON.stringify({ l: Math.round(r.left), t: Math.round(r.top) })); }
        else if (flowyEl.classList.contains('collapsed')) setCollapsed(false);
        drag = null;
    });
})();
