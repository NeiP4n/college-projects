# TOP_DOWN_DESIGN — CodeLearn

<!-- updated: 2026-05-23 -->

---

## Цель системы (WHY)

CodeLearn — интерактивная платформа обучения программированию на C#. Существует потому, что стандартные видеокурсы и статьи не дают немедленной обратной связи: студент читает → не практикует → забывает.

Решаемая проблема: дать обучающемуся блочный урок с задачами прямо в браузере, мгновенной проверкой кода через компилятор (.NET), и встроенным AI-тьютором — без установки среды разработки.

---

## Архитектура

```
┌─────────────────────────────────────────────────┐
│                   Browser                       │
│  Razor Views (cshtml) + Vanilla JS + Ace Editor │
└───────────────────┬─────────────────────────────┘
                    │ HTTP (MVC)
┌───────────────────▼─────────────────────────────┐
│              ASP.NET Core 9 MVC                 │
│  Controllers: Lesson, Course, Account, Builder  │
│               AiTutor, ApiRun, Profile          │
└──────┬──────────────┬──────────────┬────────────┘
       │              │              │
  ┌────▼───┐    ┌─────▼────┐   ┌────▼──────┐
  │Services│    │Builder   │   │ApiRun     │
  │Auth    │    │API       │   │(Roslyn/   │
  │Email   │    │(LocalOnly│   │ .NET exec)│
  │Avatar  │    │ CRUD)    │   └───────────┘
  └────┬───┘    └──────────┘
       │
┌──────▼────────────────────────────────────────┐
│  EF Core 9 → PostgreSQL                       │
│  AppDbContext: Users, Lessons, ContentBlocks, │
│  Modules, Courses, Progress, Completions...   │
└───────────────────────────────────────────────┘
```

**Слои:**
- **Presentation**: Razor Views + статика (CSS, JS, Ace Editor, highlight.js)
- **Application**: Controllers + Services (Auth, Email, Avatar, RegistrationFlow)
- **Domain**: Models (Lesson, ContentBlock, LearningModule, Course, AppUser, ...)
- **Infrastructure**: EF Core, AppDbContext, Seeders, OpenAI API (AiTutor)

**Security boundary**: Builder API доступен только с локальных IP (`LocalOnlyAttribute`). Внешние пользователи не могут управлять контентом.

---

## Основные модули и их ответственность

| Модуль | Файл | Ответственность |
|--------|------|-----------------|
| LessonController | Controllers/LessonController.cs | Отдаёт страницу урока, обрабатывает прогресс, ошибки |
| BuilderApiController | Controllers/BuilderApiController.cs | CRUD для уроков/блоков/модулей (только локальный IP) |
| AccountController | Controllers/AccountController.cs | Регистрация, вход, профиль |
| AiTutorController | Controllers/AiTutorController.cs | Проксирует запросы к OpenAI с лимитом (`UserAiUsage`) |
| ApiRunController | Controllers/ApiRunController.cs | Компилирует и запускает C# код студента |
| AppDbContext | Data/AppDbContext.cs | Единственная точка доступа к БД |
| CSharpSeeder | Data/CSharpSeeder.cs | Начальные данные: модули и уроки C# |
| AuthService | Services/AuthService.cs | Логин/регистрация, хеширование паролей |
| EmailService | Services/EmailService.cs | Отправка писем (подтверждение, восстановление) |
| Lesson/Index.cshtml | Views/Lesson/Index.cshtml | Рендерит все типы блоков, quiz, timed_test, two_path, etc. |
| Builder/Index.cshtml | Views/Builder/Index.cshtml | Drag-and-drop редактор контента уроков |

---

## Типы контент-блоков (ContentBlock.Type)

```
text            — markdown-текст с hljs подсветкой
code            — Ace editor, read-only viewer
quiz            — выбор одного варианта (shuffle при загрузке)
task            — textarea + компилятор
control_question— вопрос с раскрывающимся ответом
short_answer    — текстовый ввод, проверка по regex/substring
classify        — drag-and-drop по категориям
knowledge_check — быстрый вопрос без оценки
challenge_task  — повышенная сложность
transfer_test   — применение знания в новом контексте
timed_test      — ввод кода с таймером (SVG ring countdown)
compare_variants— Ace viewer × 2 (pros/cons/conclusion)
two_path        — два пути решения с Ace viewer
intent_to_code  — описание → написать код
code_to_intent  — код → объяснить намерение
counterexample  — пример неправильного кода + объяснение
adversarial_test— намеренно ошибочный код → найти баг
explain         — объяснение концепции
checkpoint      — промежуточная проверка понимания
control_task    — итоговое задание модуля
```

---

## Интерфейсы между модулями

- **Builder ↔ БД**: REST API `/api/builder/*` → EF Core → PostgreSQL
- **Lesson ↔ ApiRun**: JS fetch → `/api/run` → Roslyn/dotnet exec → JSON {ok, output, error}
- **Lesson ↔ AiTutor**: JS fetch → `/api/ai-tutor` → проверка лимита → OpenAI API → streaming response
- **Lesson ↔ Progress**: JS `completeBlock()` → POST `/Lesson/CompleteBlock` → `UserLessonProgress`
- **Account ↔ Email**: регистрация → EmailService → SMTP
- **MCP Server ↔ Builder API**: `codelearn_mcp.py` (stdio) → HTTP → BuilderApiController (localhost only)

---

## Data Flow

```
Студент открывает урок
  → LessonController.Index(id)
  → EF: Lesson + ContentBlocks (ordered)
  → Razor: рендер блоков по Type
  → Browser: Ace editors init, hljs, quiz shuffle

Студент запускает код
  → JS fetch POST /api/run { code }
  → ApiRunController → компиляция → {ok, output}
  → JS: показывает результат

Студент завершает блок
  → JS completeBlock() → POST /Lesson/CompleteBlock
  → EF: UserLessonProgress update
  → JS: следующий блок разблокируется

Редактор контента (Builder)
  → Builder/Index.cshtml (drag-drop)
  → JS → /api/builder/block (POST/PUT/DELETE)
  → BuilderApiController → EF → PostgreSQL
```

---

## Dependency Flow

```
Views ──────────────────────────→ Controllers
Controllers ────────────────────→ Services
Controllers ────────────────────→ AppDbContext
Services (Auth, Email, Avatar) ─→ AppDbContext
AppDbContext ────────────────────→ PostgreSQL

LocalOnlyAttribute ─────────────→ BuilderApiController (guard)
AppUserClaimsPrincipalFactory ──→ Identity pipeline
InputSanitizer ─────────────────→ Controllers (input validation)
```

---

## Ключевые архитектурные решения

1. **ContentBlock.Content — двойная сериализация** (JSON string внутри JSON). Упрощает хранение разнородных структур в одной таблице без отдельных таблиц на тип блока. Трейдофф: сложнее запрашивать по полям контента.

2. **LocalOnlyAttribute** на Builder API — контент редактируется только локально, API ключи хранятся только в `appsettings.json` и никогда не логируются/экспонируются.

3. **Ace Editor + highlight.js** — два разных инструмента подсветки: Ace для интерактивных редакторов, hljs для markdown `pre code` блоков. Ace в read-only режиме скрывает курсор через CSS + JS.

4. **Нет SPA-фреймворка** — Razor + Vanilla JS. Снижает сложность деплоя и количество зависимостей. Трейдофф: сложнее делать реактивные UI.

5. **MCP сервер (`codelearn_mcp.py`)** — Claude Desktop управляет контентом через stdio без прямого доступа к БД.

---

## Tradeoffs / Ограничения

- ContentBlock без версионирования — нет истории изменений контента
- Компилятор кода (`ApiRunController`) запускает произвольный код — нужна sandbox-изоляция (сейчас не реализована полностью)
- AI-тьютор лимит по пользователю (`UserAiUsage`) — нет rate limiting на уровне IP
- Builder доступен только локально — нельзя редактировать контент удалённо
- Нет unit-тестов для логики блоков

---

## Технический долг

- `CSharpSeeder.cs` и `CSharpContentSeeder.cs` — дублирование логики посева данных
- Нет абстракции над типами блоков (switch/if chains в Razor)
- `appsettings.json` содержит секреты — нужен secrets manager или env variables для production
- SQL запросы логируются в development (нужен фильтр для `Microsoft.EntityFrameworkCore.Database.Command`)

---

## Риски масштабирования

- Запуск C# кода студентов — CPU/memory intensive, нет очереди и изоляции контейнером
- Все блоки урока загружаются сразу — при большом количестве блоков возможна задержка
- OpenAI API вызовы не кешируются — при высокой нагрузке большие расходы
- PostgreSQL без read replicas — единая точка отказа для чтения
