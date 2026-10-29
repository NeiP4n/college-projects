(function () {
  'use strict';

  function byId(id) {
    return document.getElementById(id);
  }

  (function clock() {
    var el = byId('courseClock');
    if (!el) return;

    function tick() {
      var d = new Date();
      el.textContent = [d.getHours(), d.getMinutes(), d.getSeconds()]
        .map(function (n) { return String(n).padStart(2, '0'); })
        .join(':');
    }

    tick();
    window.setInterval(tick, 1000);
  })();

  (function rain() {
    var canvas = byId('courseBinaryRain');
    if (!canvas) return;

    var ctx = canvas.getContext('2d');
    var fontSize = 14;
    var cols = 0;
    var drops = [];

    function resize() {
      canvas.width = canvas.offsetWidth;
      canvas.height = canvas.offsetHeight;
      cols = Math.floor(canvas.width / fontSize);
      drops = Array(cols).fill(0).map(function () {
        return Math.random() * canvas.height / fontSize;
      });
    }

    function draw() {
      ctx.fillStyle = 'rgba(0,0,0,0.08)';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      ctx.fillStyle = '#fff';
      ctx.font = fontSize + 'px FiraCode, Consolas, monospace';

      for (var i = 0; i < cols; i++) {
        ctx.fillText(Math.random() > 0.5 ? '0' : '1', i * fontSize, drops[i] * fontSize);
        if (drops[i] * fontSize > canvas.height && Math.random() > 0.975) drops[i] = 0;
        drops[i]++;
      }
    }

    resize();
    window.addEventListener('resize', resize);
    window.setInterval(draw, 80);
  })();

  (function filters() {
    var buttons = document.querySelectorAll('.course-filters button');
    var cards = document.querySelectorAll('.course-card');

    buttons.forEach(function (button) {
      button.addEventListener('click', function () {
        buttons.forEach(function (item) { item.classList.remove('is-active'); });
        button.classList.add('is-active');

        var filter = button.dataset.filter;
        cards.forEach(function (card) {
          var cats = (card.dataset.cats || '').split(' ');
          var state = card.dataset.state;
          var show = true;

          if (filter === 'available') show = state === 'available';
          else if (filter === 'locked') show = state === 'locked';
          else if (filter !== 'all') show = cats.indexOf(filter) >= 0;

          card.style.display = show ? '' : 'none';
        });
      });
    });
  })();

  function biosLine(html, delay) {
    return new Promise(function (resolve) {
      var log = byId('courseBiosLog');
      var line = document.createElement('div');
      line.className = 'course-bios-line';
      line.innerHTML = html;
      log.appendChild(line);

      window.requestAnimationFrame(function () {
        line.classList.add('is-shown');
        window.setTimeout(resolve, delay);
      });
    });
  }

  function wait(ms) {
    return new Promise(function (resolve) {
      window.setTimeout(resolve, ms);
    });
  }

  async function startBios(courseName, target) {
    var picker = document.querySelector('.course-picker');
    var bios = byId('courseBios');
    var glitch = byId('courseGlitch');
    var load = byId('courseBiosLoad');
    var fill = byId('courseBiosFill');
    var pct = byId('courseBiosPct');

    glitch.classList.add('fire');
    picker.style.filter = 'invert(1) brightness(2)';
    await wait(280);

    picker.style.display = 'none';
    glitch.classList.remove('fire');
    glitch.style.display = 'none';
    bios.classList.add('is-on');
    byId('courseBiosLog').innerHTML = '';
    load.classList.remove('is-on');
    fill.style.width = '0';
    pct.textContent = '0';

    await biosLine('<span class="course-dim">[CodeLearn BIOS v1.4.2]</span> <span class="course-ok">POST OK</span>', 90);
    await biosLine('Memory test ............ <span class="course-ok">8192 KB / OK</span>', 110);
    await biosLine('Booting Pixel/OS ....... <span class="course-ok">DONE</span>', 100);
    await biosLine('Mounting <span class="course-warn">/courses</span> ........ <span class="course-ok">OK</span>', 100);
    await biosLine('Selecting course: <span class="course-warn">' + courseName + '</span>', 180);
    await biosLine('Initializing module map <span class="course-dim">............</span>', 180);

    byId('courseBiosName').textContent = courseName;
    load.classList.add('is-on');

    await new Promise(function (resolve) {
      var p = 0;
      var timer = window.setInterval(function () {
        p += [4, 7, 9, 12, 15][Math.floor(Math.random() * 5)];
        if (p >= 100) {
          p = 100;
          window.clearInterval(timer);
          resolve();
        }
        fill.style.width = p + '%';
        pct.textContent = p;
      }, 80);
    });

    await biosLine('Mount <span class="course-ok">success</span>. Launching <span class="course-warn">/Main/Index</span> ...', 260);
    document.body.style.transition = 'opacity .3s steps(4)';
    document.body.style.opacity = '0';
    await wait(320);
    window.location.href = target;
  }

  document.querySelectorAll('.course-card').forEach(function (card) {
    card.addEventListener('click', function (event) {
      var state = card.dataset.state;
      var bootMsg = byId('courseBootMsg');
      var name = card.querySelector('.course-name').textContent.trim();

      if (state !== 'available') {
        event.preventDefault();
        card.animate([
          { transform: 'translateX(0)' },
          { transform: 'translateX(-6px)' },
          { transform: 'translateX(6px)' },
          { transform: 'translateX(-3px)' },
          { transform: 'translateX(0)' }
        ], { duration: 280, easing: 'steps(5)' });
        bootMsg.textContent = 'access denied - ' + name.toLowerCase();
        window.setTimeout(function () { bootMsg.textContent = 'ready'; }, 1600);
        return;
      }

      event.preventDefault();
      startBios(name, card.getAttribute('href'));
    });
  });

  (function keyboardNav() {
    var idx = 0;

    function cards() {
      return Array.from(document.querySelectorAll('.course-card'))
        .filter(function (card) { return card.style.display !== 'none'; });
    }

    function focusCurrent() {
      var visible = cards();
      if (!visible.length) return;
      idx = (idx + visible.length) % visible.length;
      visible.forEach(function (card) { card.style.outline = ''; });
      visible[idx].style.outline = '2px solid #fff';
      visible[idx].style.outlineOffset = '4px';
      visible[idx].scrollIntoView({ block: 'nearest', inline: 'nearest' });
    }

    document.addEventListener('keydown', function (event) {
      var visible = cards();
      if (!visible.length) return;

      if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
        idx++;
        focusCurrent();
      } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
        idx--;
        focusCurrent();
      } else if (event.key === 'Enter') {
        visible[idx].click();
      }
    });
  })();
})();
