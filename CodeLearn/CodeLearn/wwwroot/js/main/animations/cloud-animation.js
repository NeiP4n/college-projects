function animateClouds() {

    tCloud += 0.01;

    cloudObjects.forEach(cloud => {

        const x = noise.perlin2(tCloud * cloud.speed + cloud.seedX, 0) * cloud.amplitude;
        const y = noise.perlin2(0, tCloud * cloud.speed + cloud.seedY) * cloud.amplitude;

        cloud.el.style.transform = `translate(${x}px, ${y}px)`;

    });

    requestAnimationFrame(animateClouds);
}

animateClouds();