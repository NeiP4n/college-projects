const fog = document.querySelector(".fog");
const turbulence = document.querySelector("#fog-warp feTurbulence");

let tFog = 0;
let fogOffset = 0;

function animateFog() {

    if (turbulence) {

        tFog += 0.002;

        const freqX = 0.01 + Math.sin(tFog) * 0.004;
        const freqY = 0.02 + Math.cos(tFog) * 0.004;

        turbulence.setAttribute(
            "baseFrequency",
            `${freqX} ${freqY}`
        );
    }

    if (fog) {

        fogOffset += 0.3;

        fog.style.backgroundPosition = `0px ${fogOffset}px`;
    }

    requestAnimationFrame(animateFog);
}

animateFog();