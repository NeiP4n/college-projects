noise.seed(Math.random());

const bg = document.querySelector(".levels-bg");

let t = 0;

/* ===============================
   LEVELS
=============================== */

const levels = [];

document.querySelectorAll(".level").forEach(el => {

    levels.push({
        el: el,
        seedX: Math.random() * 1000,
        seedY: Math.random() * 1000,
        amplitude: 4 + Math.random() * 8,
        speed: 0.2 + Math.random() * 0.6
    });

});


/* ===============================
   ANIMATION
=============================== */

function animate() {

    t += 0.03; // быстрее время


    /* ===============================
       ЖИВОЙ ФОН КАРТЫ
    =============================== */

    if (bg) {

        const bgX1 = noise.perlin2(t * 0.4, 0) * 10;
        const bgY1 = noise.perlin2(0, t * 0.4) * 8;

        const bgX2 = noise.perlin2(t * 0.2 + 50, 0) * 8;
        const bgY2 = noise.perlin2(0, t * 0.2 + 50) * 6;

        const finalX = bgX1 + bgX2;
        const finalY = bgY1 + bgY2;

        bg.style.transform =
            `translate(calc(-50% + ${finalX}px), calc(-50% + ${finalY}px))`;

    }


    /* ===============================
       ДВИЖЕНИЕ УРОВНЕЙ
    =============================== */

    levels.forEach(level => {

        const x =
            noise.perlin2(t * level.speed + level.seedX, 0) * level.amplitude;

        const y =
            noise.perlin2(0, t * level.speed + level.seedY) * level.amplitude;

        level.el.style.transform = `translate(${x}px, ${y}px)`;

    });

    requestAnimationFrame(animate);
}

animate();