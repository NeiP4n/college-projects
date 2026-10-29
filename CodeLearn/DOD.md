# Definition of Done — CodeLearn

<!-- updated: 2026-05-23 -->
<!-- stack: ASP.NET Core 9 MVC, EF Core, PostgreSQL, Razor, Vanilla JS, Ace Editor -->

Каждое изменение считается завершённым только если выполнены **все** пункты ниже.
У каждого пункта есть команда проверки — выполни её перед отметкой ✅.

---

## Код

- [x] Проект компилируется без ошибок
  > `dotnet build --no-restore -v q 2>&1 | tail -5`
  > Ожидание: `Ошибок: 0`

- [x] Нет отладочных логов (`Console.WriteLine` / `Debug.WriteLine`)
  > `grep -rn "Console\.WriteLine\|Debug\.WriteLine" CodeLearn --include="*.cs"`
  > Исключение: ApiRunController — там это генерируемый тест-раннер для студента

- [ ] Нет закомментированного кода без объяснения
  > `grep -rn "//.*=\|// TODO\|//TODO" CodeLearn --include="*.cs" | grep -v "// Test runner\|// Исключение"`

- [ ] Нет `TODO` без описания причины и срока
  > `grep -rn "TODO\|FIXME\|HACK" CodeLearn --include="*.cs"`

- [ ] Нет dead code
  > Проверить вручную: методы/классы без вызовов в изменённых файлах

- [x] Секреты только в `appsettings.json` или env
  > `grep -rn "sk-\|password\s*=\|api_key\s*=" CodeLearn --include="*.cs" -i`
  > Ожидание: нет совпадений

- [x] API ключи не логируются
  > `grep -rn "_logger\|ILogger" CodeLearn/Controllers/AiTutorController.cs`
  > Убедиться что ключ OpenAI не попадает в log message

---

## Архитектура

- [x] Builder API защищён `LocalOnlyAttribute`
  > `grep -n "LocalOnly" CodeLearn/Controllers/BuilderApiController.cs`
  > Ожидание: атрибут есть на классе или на каждом методе

- [x] Нет прямых запросов к БД из Views/JS
  > `grep -rn "AppDbContext\|DbContext" CodeLearn/Views --include="*.cshtml"`
  > Ожидание: нет совпадений

- [x] `AppDbContext` — единственная точка доступа к PostgreSQL
  > `grep -rn "NpgsqlConnection\|SqlConnection" CodeLearn --include="*.cs"`
  > Ожидание: нет raw connection вне AppDbContext

- [ ] Нет циклических зависимостей
  > Вручную проверить `deps.yaml`: A→B→A недопустимо
  > Особенно: Services не должны зависеть друг от друга по кругу

- [x] Новые типы блоков реализованы в обоих местах
  > 1. Найти новый тип в `Views/Lesson/Index.cshtml`: `grep -n "lb-<тип>" Index.cshtml`
  > 2. Найти в `wwwroot/js/builder.js`: `grep -n "<тип>" builder.js`
  > Ожидание: тип есть в обоих файлах

---

## Безопасность

- [x] `LocalOnlyAttribute` не снят с Builder API
  > `grep -n "\[LocalOnly\]" CodeLearn/Controllers/BuilderApiController.cs`
  > Ожидание: строка присутствует

- [ ] Пользовательский ввод проходит через `InputSanitizer`
  > `grep -n "InputSanitizer\|Sanitize(" CodeLearn/Controllers/LessonController.cs`
  > Ожидание: вызов перед любым SaveChanges с user input

- [ ] Sandbox для кода студента (known tech debt)
  > Текущее состояние: не реализован
  > Проверка при реализации: запустить `while(true){}` — должен получить timeout, не зависнуть

- [ ] `appsettings.json` не в git (production)
  > `cat .gitignore | grep appsettings`
  > Ожидание: `appsettings.Production.json` в .gitignore

- [x] AI-тьютор лимит работает
  > Вручную: отправить запросы к /api/ai-tutor сверх лимита
  > Ожидание: 429 или сообщение об исчерпании лимита

---

## Тесты (браузер — ручная проверка)

- [x] Логика блоков проверена в браузере
  > Чеклист для нового блока:
  > - [ ] начальное состояние корректно (disabled, overlay, etc.)
  > - [ ] верный ответ → completeBlock срабатывает, блок закрывается
  > - [ ] неверный ответ → ошибка без блокировки (для timed_test — таймер продолжается)
  > - [ ] таймаут (timed_test) → поле блокируется, кнопка disabled

- [x] Builder CRUD через MCP
  > ```bash
  > # Добавить тестовый блок
  > # (через Claude Desktop MCP codelearn → add_block)
  > # Проверить в браузере что блок отображается
  > # Удалить тестовый блок
  > ```

- [x] Компилятор `/api/run` работает
  > ```bash
  > curl -X POST http://localhost:5003/api/run \
  >   -H "Content-Type: application/json" \
  >   -d '{"code":"Console.WriteLine(\"ok\");"}'
  > # Ожидание: {"ok":true,"output":"ok\n"}
  >
  > curl -X POST http://localhost:5003/api/run \
  >   -H "Content-Type: application/json" \
  >   -d '{"code":"int x = \"broken\";"}'
  > # Ожидание: {"ok":false,"error":"..."}
  > ```

- [x] Quiz shuffle работает
  > 1. Открыть урок с quiz блоком
  > 2. Запомнить порядок вариантов
  > 3. F5 → порядок должен измениться
  > 4. Выбрать правильный ответ → ✅ (не зависит от позиции)

---

## ContentBlock специфика

- [x] `Content` — корректный JSON string
  > ```bash
  > curl http://localhost:5003/api/builder/lesson/{id}/blocks
  > # Проверить: content — это строка, не объект
  > # Правильно: "content": "{\"text\":\"...\"}"
  > # Неправильно: "content": {"text":"..."}
  > ```

- [x] Порядок блоков не нарушен
  > После добавления/удаления блока:
  > `curl http://localhost:5003/api/builder/lesson/{id}/blocks | python -m json.tool | grep order`
  > Ожидание: порядок непрерывный, без дублей

- [x] Новый тип в Builder palette
  > Открыть `http://localhost:5003/Builder` → в списке типов блоков должен быть новый тип
  > Добавить блок этого типа → он появляется в уроке

---

## Документация

- [x] `_Context/` стабы обновлены
  > Для каждого изменённого файла `path/to/File.cs`:
  > `ls _Context/path/to/File.md` — файл должен существовать
  > `grep "updated:" _Context/path/to/File.md` — дата должна совпадать с сегодня

- [x] `deps.yaml` актуален
  > Открыть `deps.yaml`, найти секцию изменённого файла
  > Проверить что новые import/using/injection добавлены в список зависимостей

- [x] `TOP_DOWN_DESIGN.md` актуален
  > Если добавлен новый модуль/сервис/тип блока — он должен быть в соответствующем разделе TDD

- [x] `<!-- updated: YYYY-MM-DD -->` проставлен
  > `grep -rn "updated:" _Context/ | grep -v "2026-05-23"`
  > Ожидание: нет старых дат в только что изменённых стабах

---

## Review

- [x] Изменения только в `allowed_paths`
  > `git diff --name-only`
  > Сравнить с `allowed_paths` текущей задачи — лишних файлов быть не должно

- [ ] Нет дублирования логики
  > Поиск похожих функций: если новый метод делает то что уже есть — рефакторинг
  > ⚠️ Known: CSharpSeeder + CSharpContentSeeder — кандидаты на слияние

- [x] Имена понятны без контекста
  > Вручную прочитать новые методы/переменные — можно ли понять назначение без комментария?

- [x] CSS конвенции соблюдены
  > `grep -n "class=\"[^\"]*\"" Index.cshtml | grep -v "lb-\|px-\|ace_\|hljs"`
  > Новые классы должны начинаться с `lb-` (блоки) или `px-` (panel expand)

---

## Production (перед деплоем)

- [ ] `appsettings.Production.json` без тестовых ключей
  > `cat CodeLearn/appsettings.Production.json | grep -i "key\|secret\|password\|connstr"`
  > Убедиться что нет значений по умолчанию/тестовых

- [ ] SQL логи на уровне Warning
  > В `appsettings.json` должно быть:
  > `"Microsoft.EntityFrameworkCore.Database.Command": "Warning"`
  > Проверка: `grep "Database.Command" CodeLearn/appsettings.json`

- [ ] Статика кеширована
  > В `_Layout.cshtml`: `<link href="~/css/lesson.css?v=@DateTime.Now.Ticks">`
  > Или через asp-append-version: `asp-append-version="true"`

- [ ] Builder недоступен снаружи
  > С внешнего IP или через curl с другого хоста:
  > `curl http://<server-ip>:5003/api/builder/overview`
  > Ожидание: 403 Forbidden или connection refused
