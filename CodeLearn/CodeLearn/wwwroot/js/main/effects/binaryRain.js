document.addEventListener("DOMContentLoaded", () => {

    const clouds = [
        { x: 900, y: 160 }
    ];

    const container = document.getElementById("binaryClouds");

    if (!container) {
        console.warn("binaryClouds container not found");
        return;
    }

    const cloudObjects = [];

    clouds.forEach(cloud => {

        const wrapper = document.createElement("div");
        wrapper.className = "binary-cloud-wrapper";

        wrapper.style.left = cloud.x + "px";
        wrapper.style.top = cloud.y + "px";

        const img = document.createElement("img");
        img.src = "/img/main/binary-cloud.svg";

        const canvas = document.createElement("canvas");
        canvas.className = "binaryRain";

        wrapper.appendChild(img);
        wrapper.appendChild(canvas);

        container.appendChild(wrapper);

        startBinaryRain(canvas);

        cloudObjects.push({
            el: wrapper,
            seedX: Math.random() * 1000,
            seedY: Math.random() * 1000,
            amplitude: 10 + Math.random() * 15,
            speed: 0.15 + Math.random() * 0.25
        });

    });

    function startBinaryRain(canvas) {

        const ctx = canvas.getContext("2d");

        canvas.width = 220;
        canvas.height = 420;

        const digits = ["0", "1"];
        const particles = [];

        class Particle {

            constructor() {
                this.x = Math.random() * canvas.width;
                this.y = 0;
                this.speed = 1 + Math.random() * 2;
                this.opacity = 1;
                this.char = digits[Math.floor(Math.random() * 2)];
            }

            update() {
                this.y += this.speed;
                this.opacity -= 0.004;
            }

            draw() {

                ctx.save();

                ctx.globalAlpha = this.opacity;

                ctx.fillStyle = "#9aa0a6";
                ctx.font = "18px monospace";

                ctx.fillText(this.char, this.x, this.y);

                ctx.restore();

            }

        }

        function spawn() {

            if (Math.random() < 0.4) {
                particles.push(new Particle());
            }

        }

        function animate() {

            ctx.clearRect(0, 0, canvas.width, canvas.height);

            spawn();

            for (let i = particles.length - 1; i >= 0; i--) {

                const p = particles[i];

                p.update();
                p.draw();

                if (p.opacity <= 0) {
                    particles.splice(i, 1);
                }

            }

            requestAnimationFrame(animate);

        }

        animate();

    }

    /* ===============================
       ДВИЖЕНИЕ ОБЛАКОВ
    =============================== */

    noise.seed(Math.random());



});