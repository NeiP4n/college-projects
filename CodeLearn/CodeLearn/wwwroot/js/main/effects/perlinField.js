const flag = document.querySelectorAll(".flag");

const wind = {

    speed: 0.35,
    strength: 0.25,
    t: 0,

    noise(v) {
        return (
            Math.sin(v) +
            Math.sin(v * 0.7) +
            Math.sin(v * 0.33)
        ) / 3;
    },

    update() {

        this.t += this.speed;

        flag.forEach((flag, i) => {

            const n = this.noise(this.t + i * 10);

            const scaleX = 1 + n * this.strength;
            const skew = n * 8;

            flag.style.transform =
                `scaleX(${scaleX}) skewY(${skew}deg)`;

        });

        requestAnimationFrame(() => this.update());
    }

};

wind.update();