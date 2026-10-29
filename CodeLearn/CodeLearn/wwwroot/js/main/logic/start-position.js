window.addEventListener("DOMContentLoaded", () => {

    const screenMain = document.querySelector(".screen-main");

    const BASE_WIDTH = 1920;
    const BASE_HEIGHT = 1080;

    const PHONE_BREAKPOINT = 900;

    const MIN_SCALE = 0.7;
    const MAX_SCALE = 2;

    let resizeTimeout = null;

    if (!screenMain) return;

    /* ===============================
       VIEWPORT
    =============================== */

    function getViewport() {

        if (window.visualViewport) {
            return {
                width: window.visualViewport.width,
                height: window.visualViewport.height
            };
        }

        return {
            width: window.innerWidth,
            height: window.innerHeight
        };

    }

    /* ===============================
       SCALE
    =============================== */

    function calculateScale(width, height) {

        const scaleX = width / BASE_WIDTH;
        const scaleY = height / BASE_HEIGHT;

        const rawScale = Math.min(scaleX, scaleY);

        return Math.max(MIN_SCALE, Math.min(rawScale, MAX_SCALE));

    }

    /* ===============================
       APPLY TRANSFORM
    =============================== */

    function applyTransform(scale, width) {

        const isPhone = width <= PHONE_BREAKPOINT;

        screenMain.style.position = "absolute";
        screenMain.style.bottom = "0";

        if (isPhone) {

            /* телефон — начинаем сцену с левого края */

            screenMain.style.left = "0";
            screenMain.style.transformOrigin = "left bottom";
            screenMain.style.transform = `scale(${scale})`;

        } else {

            /* ПК — центрируем */

            screenMain.style.left = "50%";
            screenMain.style.transformOrigin = "center bottom";
            screenMain.style.transform = `translateX(-50%) scale(${scale})`;

        }

    }

    /* ===============================
       SCALE SCENE
    =============================== */

    function scaleScene() {

        const viewport = getViewport();

        const scale =
            calculateScale(viewport.width, viewport.height);

        applyTransform(scale, viewport.width);

    }

    /* ===============================
       RESIZE
    =============================== */

    function onResize() {

        clearTimeout(resizeTimeout);

        resizeTimeout = setTimeout(() => {
            scaleScene();
        }, 60);

    }

    /* ===============================
       START
    =============================== */

    scaleScene();

    // Стартуем внизу — Lesson 1 виден сразу, скроллим вверх
    window.scrollTo({ top: document.body.scrollHeight, behavior: "instant" });

    window.addEventListener("resize", onResize);

});