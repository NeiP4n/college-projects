const trees = [
    { x: 500, y: 600, type: 1 },
    { x: 530, y: 400, type: 2 },
    { x: 830, y: 560, type: 1 }

];

spawnObjects(trees, {
    container: "objects",
    path: "/img/main",
    name: "tree",
    className: "tree",
    format: "svg"
});