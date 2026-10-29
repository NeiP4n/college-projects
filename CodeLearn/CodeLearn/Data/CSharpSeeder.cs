using CodeLearn.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeLearn.Data
{
    /// <summary>
    /// C# курс по структуре roadmap.sh.
    /// Сидер создаёт только модули и уроки (пустые).
    /// Контент-блоки добавляются отдельным скриптом через Builder API.
    /// </summary>
    public static class CSharpSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == "csharp");
            if (course == null) return;

            // Если уроки уже есть — не трогаем (блоки могут быть заполнены)
            bool hasLessons = await db.Lessons
                .AnyAsync(l => db.Modules
                    .Where(m => m.CourseId == course.Id)
                    .Any(m => m.Id == l.ModuleId));
            if (hasLessons) return;

            // Удаляем старые модули (cascade → уроки → блоки)
            db.Modules.RemoveRange(db.Modules.Where(m => m.CourseId == course.Id));
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════
            // Строка 1: Основы языка (y = 18)
            // ══════════════════════════════════════════════════
            var m1  = Mod(course.Id, "Введение в C# и .NET",      0,   6, 18, "#4488ff");
            var m2  = Mod(course.Id, "Переменные и типы данных",   1,  22, 18, "#7c6fff");
            var m3  = Mod(course.Id, "Операторы и выражения",      2,  38, 18, "#7cffb0");
            var m4  = Mod(course.Id, "Управляющие конструкции",    3,  54, 18, "#ffb07c");
            var m5  = Mod(course.Id, "Методы и функции",           4,  70, 18, "#ff7cae");
            var m6  = Mod(course.Id, "ООП: Основы",                5,  86, 18, "#7cfff0");

            // ══════════════════════════════════════════════════
            // Строка 2: ООП + Коллекции (y = 48)
            // ══════════════════════════════════════════════════
            var m7  = Mod(course.Id, "ООП: Наследование",          6,   6, 48, "#ff6ba8");
            var m8  = Mod(course.Id, "Коллекции и Generics",       7,  24, 48, "#6bbaff");
            var m9  = Mod(course.Id, "LINQ",                        8,  42, 48, "#6bffba");
            var m10 = Mod(course.Id, "Делегаты и события",         9,  60, 48, "#ffdb6b");
            var m11 = Mod(course.Id, "Исключения",                 10,  78, 48, "#ff6b6b");

            // ══════════════════════════════════════════════════
            // Строка 3: Async + Экосистема + Испытания (y = 78)
            // ══════════════════════════════════════════════════
            var m12 = Mod(course.Id, "Async / Await",              11,   6, 78, "#a0e080");
            var m13 = Mod(course.Id, ".NET Экосистема",            12,  26, 78, "#e0a080");

            var c1  = ChalMod(course.Id, "⚡ Алгоритмы",           13,  46, 78, "#ffd700");
            var c2  = ChalMod(course.Id, "⚡ ООП Дизайн",          14,  62, 78, "#ffd700");
            var c3  = ChalMod(course.Id, "⚡ LINQ Мастер",         15,  78, 78, "#ffd700");
            var c4  = ChalMod(course.Id, "⚡ Async Марафон",       16,  94, 78, "#ffd700");

            db.Modules.AddRange(m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12, m13, c1, c2, c3, c4);
            await db.SaveChangesAsync();

            // ── Уроки ──────────────────────────────────────────────────────────────

            // М1: Введение (3 урока)
            AddLessons(db, m1.Id,
                "Что такое C# и .NET",
                "dotnet CLI и первый проект",
                "Синтаксис и структура программы");

            // М2: Переменные и типы (7 уроков)
            AddLessons(db, m2.Id,
                "Целые числа: int, long, short, byte",
                "Дробные числа: float, double, decimal",
                "Логический тип bool и char",
                "Строки: string и интерполяция",
                "Приведение типов",
                "Nullable типы: ?, ??, ??=",
                "var, константы и readonly");

            // М3: Операторы (5 уроков)
            AddLessons(db, m3.Id,
                "Арифметические операторы",
                "Операторы сравнения и логики",
                "Битовые операторы",
                "Тернарный и null-операторы",
                "Приоритет операторов");

            // М4: Управляющие конструкции (6 уроков)
            AddLessons(db, m4.Id,
                "if / else if / else",
                "switch и switch expression",
                "Pattern Matching",
                "for и while",
                "foreach и do-while",
                "break, continue, return");

            // М5: Методы (7 уроков)
            AddLessons(db, m5.Id,
                "Объявление и вызов методов",
                "Перегрузка методов",
                "ref, out, in параметры",
                "params и необязательные параметры",
                "Рекурсия",
                "Expression-bodied члены",
                "Локальные функции");

            // М6: ООП Основы (8 уроков)
            AddLessons(db, m6.Id,
                "Классы и объекты",
                "Поля, свойства, автосвойства",
                "Конструкторы",
                "Модификаторы доступа",
                "Статические члены и Singleton",
                "Перегрузка операторов",
                "Record-типы",
                "Struct vs Class");

            // М7: ООП Наследование (7 уроков)
            AddLessons(db, m7.Id,
                "Наследование и base",
                "virtual, override, abstract",
                "Абстрактные классы",
                "Интерфейсы",
                "Полиморфизм",
                "sealed и new",
                "Паттерн-матчинг с типами");

            // М8: Коллекции и Generics (8 уроков)
            AddLessons(db, m8.Id,
                "Массивы: одномерные и многомерные",
                "List<T>",
                "Dictionary<K,V>",
                "HashSet<T>, Queue<T>, Stack<T>",
                "IEnumerable и foreach",
                "Generics: обобщённые типы",
                "Generics: ограничения where",
                "Сортировка: IComparable и IComparer");

            // М9: LINQ (7 уроков)
            AddLessons(db, m9.Id,
                "Введение в LINQ",
                "Where, Select, OrderBy",
                "GroupBy и Join",
                "Агрегация: Sum, Count, Min, Max, Average",
                "First, Single, Any, All, Take, Skip",
                "Цепочки запросов",
                "Query syntax vs Method syntax");

            // М10: Делегаты и события (6 уроков)
            AddLessons(db, m10.Id,
                "Делегаты",
                "Func<T> и Action<T>",
                "Лямбда-выражения",
                "Замыкания",
                "Событие event",
                "Анонимные типы");

            // М11: Исключения (5 уроков)
            AddLessons(db, m11.Id,
                "try / catch / finally",
                "Иерархия исключений",
                "Кастомные исключения",
                "throw, rethrow и when",
                "Best practices обработки ошибок");

            // М12: Async/Await (7 уроков)
            AddLessons(db, m12.Id,
                "Синхронность vs асинхронность",
                "Task и Task<T>",
                "async / await основы",
                "ConfigureAwait и контекст синхронизации",
                "Task.WhenAll и Task.WhenAny",
                "CancellationToken",
                "Deadlocks и best practices");

            // М13: .NET Экосистема (5 уроков)
            AddLessons(db, m13.Id,
                "NuGet и управление зависимостями",
                "Конфигурация и appsettings.json",
                "Логирование: ILogger и Serilog",
                "Dependency Injection",
                "Unit-тестирование с xUnit");

            // Испытания (по 3 урока каждое)
            AddLessons(db, c1.Id,
                "Испытание: числовые алгоритмы",
                "Испытание: строковые алгоритмы",
                "Испытание: сортировка и поиск");

            AddLessons(db, c2.Id,
                "Испытание: банковская система",
                "Испытание: геометрические фигуры",
                "Испытание: инвентарь RPG");

            AddLessons(db, c3.Id,
                "Испытание: анализ продаж",
                "Испытание: обработка текста",
                "Испытание: pipeline данных");

            AddLessons(db, c4.Id,
                "Испытание: параллельные задачи",
                "Испытание: таймаут и отмена",
                "Испытание: async pipeline");

            await db.SaveChangesAsync();
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        static LearningModule Mod(int courseId, string title, int order,
            double x, double y, string color) => new()
        {
            CourseId      = courseId,
            Title         = title,
            Order         = order,
            MapX          = x,
            MapY          = y,
            Color         = color,
            LabelFontSize = 10,
            IsChallenge   = false,
            IsLocked      = false
        };

        static LearningModule ChalMod(int courseId, string title, int order,
            double x, double y, string color) => new()
        {
            CourseId      = courseId,
            Title         = title,
            Order         = order,
            MapX          = x,
            MapY          = y,
            Color         = color,
            LabelFontSize = 9,
            IsChallenge   = true,
            IsLocked      = false
        };

        static void AddLessons(AppDbContext db, int moduleId, params string[] titles)
        {
            for (int i = 0; i < titles.Length; i++)
                db.Lessons.Add(new Lesson
                {
                    ModuleId = moduleId,
                    Title    = titles[i],
                    Order    = i,
                    IsLocked = false
                });
        }

        // ── Дополнительные уроки (не удаляет существующие) ────────────────────
        public static async Task SeedExtraLessonsAsync(AppDbContext db)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == "csharp");
            if (course == null) return;

            var modules = await db.Modules
                .Where(m => m.CourseId == course.Id && !m.IsChallenge)
                .OrderBy(m => m.Order)
                .ToListAsync();

            if (modules.Count < 13) return;

            // Словарь: заголовок модуля → доп. уроки
            var extras = new Dictionary<string, string[]>
            {
                ["Введение в C# и .NET"]        = ["CLR, JIT и IL-код", "Отладка в VS / VS Code"],
                ["Переменные и типы данных"]     = ["Enum: перечисления", "Кортежи (ValueTuple)"],
                ["Операторы и выражения"]        = ["checked / unchecked", "Оператор is и as"],
                ["Управляющие конструкции"]      = ["goto и когда его избегать", "yield return: ленивые последовательности"],
                ["Методы и функции"]             = ["Extension-методы", "Named аргументы и default-значения"],
                ["ООП: Основы"]                  = ["IDisposable и using", "Индексаторы"],
                ["ООП: Наследование"]            = ["Явная реализация интерфейса", "Ковариантность и контравариантность"],
                ["Коллекции и Generics"]         = ["Span<T> и Memory<T>", "ImmutableCollections"],
                ["LINQ"]                         = ["PLINQ и AsParallel()", "ToLookup и группировки"],
                ["Делегаты и события"]           = ["Предикаты и Comparison<T>", "Weak-события и утечки памяти"],
                ["Исключения"]                   = ["AggregateException", "ExceptionDispatchInfo"],
                ["Async / Await"]                = ["ValueTask<T>", "IAsyncEnumerable<T> и async streams"],
                [".NET Экосистема"]              = ["Reflection и атрибуты", "Source Generators"],
            };

            foreach (var mod in modules)
            {
                if (!extras.TryGetValue(mod.Title, out var newTitles)) continue;

                var existingTitles = await db.Lessons
                    .Where(l => l.ModuleId == mod.Id)
                    .Select(l => l.Title)
                    .ToListAsync();

                var maxOrder = await db.Lessons
                    .Where(l => l.ModuleId == mod.Id)
                    .Select(l => (int?)l.Order)
                    .MaxAsync() ?? -1;

                var order = maxOrder + 1;
                foreach (var title in newTitles)
                {
                    if (existingTitles.Contains(title)) continue;
                    db.Lessons.Add(new Lesson
                    {
                        ModuleId = mod.Id,
                        Title    = title,
                        Order    = order++,
                        IsLocked = false
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
