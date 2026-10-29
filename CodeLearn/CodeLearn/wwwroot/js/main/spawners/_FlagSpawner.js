const flags = [
    { x: 1244, y: 813, type: 1 },
    { x: 1342, y: 760, type: 1 },
    { x: 1389, y: 677, type: 1 },
    { x: 1553, y: 600, type: 1 }
];

spawnObjects(flags, {
    container: "objects",
    path: "/img/main",
    name: "flag",
    className: "flag",
    format: "png"
});