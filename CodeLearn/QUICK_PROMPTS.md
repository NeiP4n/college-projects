## Новая большая фича
Прочитай ARCHITECTURE.md и PROJECT_RULES.md, используй CodeGraph, сначала создай plan + DOD + TEST STEPS, потом implementation, не нарушай MVC и не создавай дубли.

## Новый чат / новый агент
Сначала изучи структуру проекта через CodeGraph, прочитай ARCHITECTURE.md и PROJECT_RULES.md, работай как senior architect и не изменяй unrelated files.

## Refactor
Используй CodeGraph для поиска зависимостей, дублей и side effects, минимизируй breaking changes и не изменяй public API без необходимости.

## Исправление бага
Найди root cause через CodeGraph, не трогай unrelated files, добавь TEST STEPS и проверь edge cases перед завершением.

## Новый модуль
Сначала предложи architecture impact и зависимости, потом создай DOD + TEST STEPS и только после этого implementation.

## Если ИИ начинает делать хаос
Остановись, сделай architecture review, найди duplicated logic, dead code и potential side effects, потом предложи cleanup plan.

## Перед завершением задачи
Обнови DOD и CHANGELOG_AI.md, кратко опиши изменённые файлы, риски и как проверить результат.