function spawnObjects(list = [], options = {}) {

    const {
        container: containerId = "objects",
        path = "",
        name = "",
        className = "object",
        format: defaultFormat = "svg"
    } = options;

    const container = document.getElementById(containerId);

    if (!container) {
        console.warn(`spawnObjects: container "${containerId}" not found`);
        return;
    }

    if (!Array.isArray(list) || list.length === 0) {
        console.warn("spawnObjects: list is empty");
        return;
    }

    const fragment = document.createDocumentFragment();

    for (const item of list) {

        if (!item) continue;

        const x = Number(item.x) || 0;
        const y = Number(item.y) || 0;

        const format = item.format || defaultFormat;
        const typePart = item.type ? `-${item.type}` : "";

        const fileName = `${name}${typePart}.${format}`;
        const src = `${path}/${fileName}`;

        const img = document.createElement("img");

        img.src = src;
        img.className = className;
        img.loading = "lazy";

        img.style.transform = `translate(${x}px, ${y}px)`;

        fragment.appendChild(img);
    }

    container.appendChild(fragment);
}