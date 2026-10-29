import json, urllib.request, sys
sys.stdout.reconfigure(encoding='utf-8')

BASE = "http://localhost:5003/api/builder/lessons"

def post(lid, btype, content_obj):
    url = f"{BASE}/{lid}/blocks"
    body = json.dumps({"type": btype, "content": json.dumps(content_obj, ensure_ascii=False)}).encode("utf-8")
    req = urllib.request.Request(url, data=body, headers={"Content-Type": "application/json"}, method="POST")
    try:
        with urllib.request.urlopen(req) as r:
            st = r.status
    except urllib.error.HTTPError as e:
        st = e.code
        print(f"  ERR {e.code}: {e.read()[:100]}")
    print(f"  [{st}] {btype}")

# ============================================================
# Lesson 800: "Что такое C# и .NET"
# ============================================================
print("\n=== Lesson 800: Что такое C# и .NET ===")

post(800, "text", {"markdown": """## Что такое C# и .NET

**C#** (читается «Си-шарп») — современный объектно-ориентированный язык программирования, разработанный компанией Microsoft. Он входит в экосистему **.NET** и позволяет создавать:

- веб-приложения (ASP.NET Core)
- мобильные приложения (MAUI)
- игры (Unity)
- десктопные приложения и микросервисы

**.NET** — это платформа, включающая:

| Компонент | Назначение |
|-----------|-----------|
| CLR (Common Language Runtime) | Среда выполнения кода |
| BCL (Base Class Library) | Стандартная библиотека классов |
| SDK | Инструменты разработки и сборки |

C# компилируется в **IL (Intermediate Language)**, который затем JIT-компилируется CLR в машинный код — это даёт кроссплатформенность и высокую производительность."""})

post(800, "code", {"language": "csharp", "title": "Первая программа на C#", "code": """// Пространство имён группирует классы
namespace HelloWorld
{
    // Класс — основная единица кода в C#
    class Program
    {
        // Точка входа в программу
        static void Main(string[] args)
        {
            Console.WriteLine("Привет, мир!");
            Console.WriteLine($".NET версия: {Environment.Version}");
        }
    }
}"""})

post(800, "quiz", {"question": "Во что компилируется C#-код перед выполнением?", "options": ["В машинный код напрямую", "В IL (Intermediate Language)", "В байт-код Java", "В JavaScript"], "correct": 1, "explanation": "C#-код компилируется в IL (Intermediate Language), который затем JIT-компилятором CLR преобразуется в нативный машинный код при запуске. Это обеспечивает кроссплатформенность."})

post(800, "text", {"markdown": """## Версии .NET и C#

История платформы прошла долгий путь:

- **.NET Framework** (2002–2019) — только Windows
- **.NET Core** (2016–2020) — кроссплатформенный
- **.NET 5+** (2020–сейчас) — объединённая платформа

Каждая версия .NET связана с версией C#:

| .NET | C# | Ключевые фичи |
|------|----|---------------|
| .NET 6 | C# 10 | Global usings, file-scoped namespaces |
| .NET 7 | C# 11 | Raw strings, required members |
| .NET 8 | C# 12 | Primary constructors, collection expressions |
| .NET 9 | C# 13 | Params collections, новые LINQ методы |"""})

post(800, "knowledge_check", {"topic": "Экосистема .NET", "question": "Какой компонент .NET отвечает за выполнение управляемого кода и управление памятью?", "options": ["BCL (Base Class Library)", "SDK (Software Development Kit)", "CLR (Common Language Runtime)", "MSBuild"], "correct": 2, "explanation": "CLR (Common Language Runtime) — это среда выполнения .NET. Она управляет памятью через сборщик мусора (GC), обеспечивает безопасность типов и JIT-компиляцию IL-кода в машинный."})

post(800, "transfer_test", {"question": "Разработчик хочет создать кроссплатформенное веб-приложение на C#. Какую версию платформы ему следует использовать?", "options": [".NET Framework 4.8, так как это самая стабильная версия", ".NET 8 (или новее), так как это современная кроссплатформенная версия", "Mono, так как только он работает на Linux", ".NET Core 2.0, так как он специально для Linux"], "correct": 1, "explanation": ".NET 5 и выше — это объединённая кроссплатформенная платформа, заменившая .NET Core. .NET Framework работает только на Windows. Mono — сторонняя реализация, не рекомендуется для новых проектов."})

post(800, "checkpoint", {"title": "Итоги урока: Что такое C# и .NET", "description": "Проверьте, что вы усвоили ключевые концепции", "tasks": ["C# — объектно-ориентированный язык от Microsoft, входящий в экосистему .NET", "Код C# компилируется в IL, а не напрямую в машинный код", "CLR выполняет IL-код через JIT-компиляцию и управляет памятью", ".NET 5+ — единая кроссплатформенная платформа для всех типов приложений", "C# и .NET развиваются вместе: каждая версия .NET имеет соответствующую версию C#"]})

# ============================================================
# Lesson 801: "dotnet CLI и первый проект"
# ============================================================
print("\n=== Lesson 801: dotnet CLI и первый проект ===")

post(801, "text", {"markdown": """## dotnet CLI — командная строка .NET

**dotnet CLI** — это инструмент командной строки для создания, сборки и запуска .NET-проектов. Он устанавливается вместе с .NET SDK.

### Основные команды

```bash
# Проверить установку
dotnet --version

# Создать новый проект
dotnet new console -n MyApp

# Перейти в папку проекта
cd MyApp

# Запустить проект
dotnet run

# Собрать проект
dotnet build

# Опубликовать проект
dotnet publish -c Release
```

### Структура нового проекта

После `dotnet new console -n MyApp` создаётся:

```
MyApp/
├── MyApp.csproj   ← файл проекта (XML)
├── Program.cs     ← точка входа
└── obj/           ← временные файлы сборки
```""" })

post(801, "code", {"language": "csharp", "title": "Файл проекта MyApp.csproj", "code": """<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Тип выходного файла -->
    <OutputType>Exe</OutputType>

    <!-- Целевая версия .NET -->
    <TargetFramework>net8.0</TargetFramework>

    <!-- Nullable reference types включены -->
    <Nullable>enable</Nullable>

    <!-- Implicit using directives -->
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>"""})

post(801, "prediction", {"language": "csharp", "code": """// Program.cs в стиле .NET 6+ (top-level statements)
Console.WriteLine("Старт");

for (int i = 1; i <= 3; i++)
{
    Console.Write(i + " ");
}

Console.WriteLine();
Console.WriteLine("Финиш");""", "expectedOutput": "Старт\n1 2 3 \nФиниш", "hint": "Обратите внимание на Console.Write (без переноса) и Console.WriteLine (с переносом). Цикл выводит числа через пробел."})

post(801, "text", {"markdown": """## Top-level statements (C# 9+)

Начиная с .NET 6, консольные приложения используют **top-level statements** — код пишется без явного класса `Program` и метода `Main`:

```csharp
// Старый стиль (до C# 9)
namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Привет!");
        }
    }
}

// Новый стиль (C# 9+, .NET 6+)
Console.WriteLine("Привет!");
```

Компилятор автоматически генерирует класс и метод Main. Аргументы командной строки доступны через переменную `args`.

### Полезные команды dotnet

```bash
dotnet new --list          # список шаблонов
dotnet add package Newtonsoft.Json  # добавить NuGet пакет
dotnet test               # запустить тесты
dotnet --info             # информация об установке
```""" })

post(801, "fill_blank", {"template": """// Команды dotnet CLI:
// Создать консольный проект с именем Calculator:
dotnet ___ console -n Calculator

// Запустить проект:
dotnet ___

// Собрать без запуска:
dotnet ___

// Добавить пакет:
dotnet ___ package Newtonsoft.Json""", "blanks": ["new", "run", "build", "add"], "hint": "Четыре основные команды dotnet CLI: new (создать), run (запустить), build (собрать), add (добавить зависимость)."})

post(801, "quiz", {"question": "Что делает команда `dotnet publish -c Release`?", "options": ["Запускает приложение в режиме отладки", "Публикует приложение в оптимизированном виде для развёртывания", "Обновляет NuGet пакеты до последней версии", "Создаёт новый проект по шаблону Release"], "correct": 1, "explanation": "`dotnet publish` подготавливает приложение к развёртыванию. Флаг `-c Release` выбирает конфигурацию Release (с оптимизациями), в отличие от Debug (с отладочной информацией)."})

# ============================================================
# Lesson 802: "Синтаксис и структура программы"
# ============================================================
print("\n=== Lesson 802: Синтаксис и структура программы ===")

post(802, "text", {"markdown": """## Синтаксис и структура программы C#

Программа на C# состоит из нескольких уровней:

```
Пространство имён (namespace)
  └── Класс (class)
        └── Метод (method)
              └── Операторы (statements)
```

### Ключевые элементы синтаксиса

- **Каждый оператор** заканчивается точкой с запятой `;`
- **Блоки кода** ограничены фигурными скобками `{ }`
- **Комментарии**: однострочные `//`, многострочные `/* */`, XML `///`
- **Регистрозависимость**: `myVar` и `MyVar` — разные переменные
- **Соглашения об именовании**: PascalCase для классов/методов, camelCase для переменных"""})

post(802, "code", {"language": "csharp", "title": "Полная структура программы с комментариями", "code": """// Директивы using — подключение пространств имён
using System;
using System.Collections.Generic;

// Пространство имён — группировка кода
namespace MyApplication
{
    /// <summary>
    /// XML-документация для класса
    /// </summary>
    class Calculator
    {
        // Поле класса
        private string _name;

        // Конструктор
        public Calculator(string name)
        {
            _name = name;
        }

        // Метод класса
        public int Add(int a, int b)
        {
            /* Многострочный
               комментарий */
            return a + b; // возврат результата
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var calc = new Calculator("Базовый");
            int result = calc.Add(10, 25);
            Console.WriteLine($"Результат: {result}");
        }
    }
}"""})

post(802, "prediction", {"language": "csharp", "code": """int x = 5;
int y = 3;

Console.WriteLine(x + y);
Console.WriteLine(x - y);
Console.WriteLine(x * y);
Console.WriteLine(x / y);
Console.WriteLine(x % y);""", "expectedOutput": "8\n2\n15\n1\n2", "hint": "Обратите внимание на целочисленное деление: 5 / 3 = 1 (дробная часть отбрасывается). Операция % — это остаток от деления: 5 % 3 = 2."})

post(802, "text", {"markdown": """## Операторы и выражения

### Арифметические операторы

| Оператор | Описание | Пример |
|----------|----------|--------|
| `+` | Сложение | `5 + 3 = 8` |
| `-` | Вычитание | `5 - 3 = 2` |
| `*` | Умножение | `5 * 3 = 15` |
| `/` | Деление (целочисленное для int) | `5 / 3 = 1` |
| `%` | Остаток от деления | `5 % 3 = 2` |

### Операторы присваивания

```csharp
int n = 10;
n += 5;  // n = n + 5 → 15
n -= 3;  // n = n - 3 → 12
n *= 2;  // n = n * 2 → 24
n /= 4;  // n = n / 4 → 6
n++;     // n = n + 1 → 7
n--;     // n = n - 1 → 6
```

### Порядок выполнения операций

Как в математике: `*` и `/` раньше `+` и `-`. Скобки меняют порядок:
```csharp
int a = 2 + 3 * 4;    // 2 + 12 = 14
int b = (2 + 3) * 4;  // 5 * 4 = 20
```"""})

post(802, "fix_bug", {"language": "csharp", "buggyCode": """// Вычислить площадь прямоугольника и периметр
int width = 7
int height = 4;

int area = width * height
int perimeter = 2 * width + height;

Console.WriteLine("Площадь: " + area);
Console.Writeline("Периметр: " + perimeter);""", "expectedOutput": "Площадь: 28\nПериметр: 22", "hint": "Найдите три ошибки: пропущенные точки с запятой и опечатку в имени метода. Периметр прямоугольника = 2*(ширина+высота)."})

post(802, "drag_order", {"pieces": ["static void Main(string[] args)", "namespace MyApp", "Console.WriteLine(\"Hello\");", "class Program", "{", "{", "{", "}", "}", "}"], "solution": [1, 4, 3, 5, 0, 6, 2, 7, 8, 9], "hint": "Структура: namespace → class → Main метод → операторы внутри. Каждый блок открывается { и закрывается }."})

# ============================================================
# Lesson 803: "Целые числа: int, long, short, byte"
# ============================================================
print("\n=== Lesson 803: Целые числа: int, long, short, byte ===")

post(803, "text", {"markdown": """## Целочисленные типы в C#

C# предлагает несколько типов для хранения целых чисел. Выбор типа зависит от диапазона значений и требований к памяти:

| Тип | Размер | Минимум | Максимум | Псевдоним CLR |
|-----|--------|---------|---------|--------------|
| `byte` | 1 байт | 0 | 255 | System.Byte |
| `sbyte` | 1 байт | -128 | 127 | System.SByte |
| `short` | 2 байта | -32 768 | 32 767 | System.Int16 |
| `ushort` | 2 байта | 0 | 65 535 | System.UInt16 |
| `int` | 4 байта | -2 147 483 648 | 2 147 483 647 | System.Int32 |
| `uint` | 4 байта | 0 | ~4 млрд | System.UInt32 |
| `long` | 8 байт | ~-9.2 × 10¹⁸ | ~9.2 × 10¹⁸ | System.Int64 |
| `ulong` | 8 байт | 0 | ~18.4 × 10¹⁸ | System.UInt64 |

**Совет**: По умолчанию используйте `int`. Переходите к `long` когда числа превышают ~2 млрд."""})

post(803, "code", {"language": "csharp", "title": "Работа с целочисленными типами", "code": """// Диапазоны значений
Console.WriteLine($"int:   {int.MinValue} до {int.MaxValue}");
Console.WriteLine($"long:  {long.MinValue} до {long.MaxValue}");
Console.WriteLine($"byte:  {byte.MinValue} до {byte.MaxValue}");
Console.WriteLine($"short: {short.MinValue} до {short.MaxValue}");

// Объявление переменных
byte b = 200;
short s = 30_000;       // _ — разделитель для читаемости
int i = 2_000_000;
long l = 9_000_000_000L; // L — суффикс для long литерала

// Арифметика
int sum = i + 500_000;
long bigSum = l * 2;

Console.WriteLine($"sum = {sum}");
Console.WriteLine($"bigSum = {bigSum}");

// Переполнение (опасно!)
byte overflow = 255;
overflow++; // = 0 (переполнение без исключения!)
Console.WriteLine($"overflow: {overflow}");

// Безопасная проверка
checked
{
    // int x = int.MaxValue + 1; // OverflowException!
}"""})

post(803, "prediction", {"language": "csharp", "code": """int a = 1_000_000;
int b = 3_000;
long result = (long)a * b;

Console.WriteLine(a * 1000);   // int * int
Console.WriteLine(result);      // long
Console.WriteLine(10 / 3);
Console.WriteLine(-10 / 3);""", "expectedOutput": "1000000000\n3000000000\n3\n-3", "hint": "a * 1000 = 10^9 — это ещё вмещается в int (макс ~2.1×10^9). (long)a * b приводит a к long перед умножением. Целочисленное деление: -10/3 = -3 (округление к нулю)."})

post(803, "fill_blank", {"template": """// Объявите переменные подходящего типа:

// Возраст человека (0-150):
___ age = 25;

// Население страны (до 1.5 млрд):
___ population = 1_400_000_000;

// Номер порта сети (0-65535):
___ port = 8080;

// Количество байт в файле (до петабайт):
___ fileSize = 5_000_000_000_000L;

// Значение пикселя в градациях серого (0-255):
___ pixelValue = 128;""", "blanks": ["byte", "long", "ushort", "long", "byte"], "hint": "Выбирайте наименьший тип, который вмещает все возможные значения. Возраст: byte (0-255). Население: long (>int.MaxValue). Порт: ushort (0-65535). Байты файла: long. Пиксель: byte (0-255)."})

post(803, "mini_task", {"language": "csharp", "title": "Перевод единиц хранения данных", "description": "Дан размер файла в байтах. Выведите его размер в килобайтах, мегабайтах и гигабайтах (целочисленное деление).", "starterCode": """long bytes = 1_073_741_824; // 1 ГБ в байтах
long kilobytes = __;         // bytes / 1024
long megabytes = __;         // kilobytes / 1024
long gigabytes = __;         // megabytes / 1024

Console.WriteLine($"Байт: {bytes}");
Console.WriteLine($"Килобайт: {kilobytes}");
Console.WriteLine($"Мегабайт: {megabytes}");
Console.WriteLine($"Гигабайт: {gigabytes}");""", "expectedOutput": "Байт: 1073741824\nКилобайт: 1048576\nМегабайт: 1024\nГигабайт: 1"})

post(803, "quiz", {"question": "Что произойдёт при выполнении кода: `byte b = 255; b++;` в обычном (unchecked) контексте?", "options": ["Выбросится исключение OverflowException", "Переменная b станет равна 256", "Переменная b станет равна 0 (переполнение)", "Компилятор выдаст ошибку"], "correct": 2, "explanation": "В контексте unchecked (используется по умолчанию) переполнение типа byte происходит без исключения: 255 + 1 = 0 (значение «оборачивается»). Для получения исключения нужен контекст checked."})

# ============================================================
# Lesson 804: "Дробные числа: float, double, decimal"
# ============================================================
print("\n=== Lesson 804: Дробные числа: float, double, decimal ===")

post(804, "text", {"markdown": """## Типы для дробных чисел в C#

| Тип | Размер | Точность | Суффикс | Применение |
|-----|--------|----------|---------|-----------|
| `float` | 4 байта | ~7 знаков | `f` или `F` | Графика, физика, когда точность некритична |
| `double` | 8 байт | ~15-16 знаков | `d` (необяз.) | Научные вычисления, по умолчанию |
| `decimal` | 16 байт | 28-29 знаков | `m` или `M` | Финансы, деньги, точные расчёты |

### Важные особенности

- `float` и `double` — числа с плавающей точкой по стандарту IEEE 754
- Они **не могут точно** представить многие десятичные дроби
- `decimal` хранит числа в десятичной форме → точное представление
- Без суффикса литерал `3.14` имеет тип `double`"""})

post(804, "code", {"language": "csharp", "title": "Сравнение float, double и decimal", "code": """// Точность типов
float f = 1.0f / 3.0f;
double d = 1.0 / 3.0;
decimal m = 1.0m / 3.0m;

Console.WriteLine($"float:   {f}");
Console.WriteLine($"double:  {d}");
Console.WriteLine($"decimal: {m}");

// Проблема точности с плавающей точкой
double a = 0.1 + 0.2;
Console.WriteLine($"0.1 + 0.2 = {a}");        // не равно 0.3!
Console.WriteLine($"== 0.3? {a == 0.3}");      // False!

decimal da = 0.1m + 0.2m;
Console.WriteLine($"decimal: {da}");            // точно 0.3
Console.WriteLine($"== 0.3m? {da == 0.3m}");   // True

// Специальные значения double
Console.WriteLine(double.IsNaN(0.0 / 0.0));        // True
Console.WriteLine(double.IsInfinity(1.0 / 0.0));   // True
Console.WriteLine(double.PositiveInfinity);"""})

post(804, "prediction", {"language": "csharp", "code": """decimal price = 19.99m;
decimal tax = 0.20m;
decimal total = price + price * tax;

Console.WriteLine(Math.Round(total, 2));
Console.WriteLine(Math.Floor(total));
Console.WriteLine(Math.Ceiling(total));
Console.WriteLine((int)total);""", "expectedOutput": "23.99\n23\n24\n23", "hint": "19.99 * 0.20 = 3.998, итого 23.988. Round(23.988, 2) = 23.99. Floor = 23 (вниз). Ceiling = 24 (вверх). (int) = 23 (обрезание, как Floor для положительных)."})

post(804, "quiz", {"question": "Какой тип следует использовать для хранения суммы денег в банковском приложении?", "options": ["float — достаточно точности для большинства операций", "double — стандартный тип для вещественных чисел", "decimal — обеспечивает точное представление десятичных чисел", "long — целочисленный тип без ошибок округления"], "correct": 2, "explanation": "decimal использует десятичное (не двоичное) представление чисел, что позволяет точно хранить значения вроде 0.1 и 0.01. float и double имеют ошибки округления, критичные для финансовых расчётов."})

post(804, "fix_bug", {"language": "csharp", "buggyCode": """// Расчёт скидки
decimal originalPrice = 1500m;
decimal discountPercent = 15;   // 15%
float discountAmount = originalPrice * (discountPercent / 100);
decimal finalPrice = originalPrice - discountAmount;

Console.WriteLine($"Скидка: {discountAmount}");
Console.WriteLine($"Итого: {finalPrice}");""", "expectedOutput": "Скидка: 225\nИтого: 1275", "hint": "Ошибка в типах: нельзя умножить decimal на float и неявно привести результат. Используйте decimal для всех переменных. discountPercent тоже должен быть decimal (или добавьте суффикс m)."})

post(804, "compare_variants", {"question": "Что лучше использовать для хранения результата физического моделирования (позиции объектов в игре)?", "variants": [{"title": "float", "code": """// Координаты объекта в 3D пространстве
float posX = 123.456f;
float posY = 78.9f;
float posZ = -45.123f;

// Обновление позиции
float speed = 9.81f;
float deltaTime = 0.016f;
posY -= speed * deltaTime;

Console.WriteLine($"Y: {posY:F3}");"""}, {"title": "decimal", "code": """// Координаты объекта в 3D пространстве
decimal posX = 123.456m;
decimal posY = 78.9m;
decimal posZ = -45.123m;

// Обновление позиции
decimal speed = 9.81m;
decimal deltaTime = 0.016m;
posY -= speed * deltaTime;

Console.WriteLine($"Y: {posY:F3}");"""}], "correct": 0, "explanation": "Для физики игр float предпочтительнее: он в 4 раза меньше decimal по памяти, работает быстрее (GPU нативно поддерживает float32), а небольшая погрешность некритична для визуализации. decimal медленнее и избыточен для графики."})

post(804, "fill_blank", {"template": """// Выберите правильные суффиксы:

float pi = 3.14___;           // суффикс float
double gravity = 9.81___;     // суффикс double (можно не указывать)
decimal price = 29.99___;     // суффикс decimal

// Исправьте ошибки:
float radius = 5.0f;
double area = Math.PI * radius * radius;  // Math.PI это double
Console.WriteLine($"Площадь: {area:___}"); // формат 2 знака после запятой""", "blanks": ["f", "", "m", "F2"], "hint": "float: суффикс f/F. double: суффикс d/D (необязателен, литерал без суффикса = double). decimal: суффикс m/M. Формат F2 = Fixed 2 decimal places."})

# ============================================================
# Lesson 805: "Логический тип bool и char"
# ============================================================
print("\n=== Lesson 805: Логический тип bool и char ===")

post(805, "text", {"markdown": """## Тип bool — логические значения

`bool` хранит одно из двух значений: `true` или `false`. Это основа всей логики программы.

### Логические операторы

| Оператор | Название | Пример | Результат |
|----------|----------|--------|-----------|
| `&&` | И (AND) | `true && false` | `false` |
| `||` | ИЛИ (OR) | `true || false` | `true` |
| `!` | НЕ (NOT) | `!true` | `false` |
| `^` | Исключающее ИЛИ (XOR) | `true ^ true` | `false` |

### Короткое замыкание (Short-circuit evaluation)

```csharp
// && останавливается, если первый операнд false
// || останавливается, если первый операнд true
int x = 0;
bool result = (x != 0) && (10 / x > 1); // безопасно, деление не выполнится
```

## Тип char — символьный тип

`char` хранит **один символ** Unicode (16-бит, 0-65535). Литералы в одинарных кавычках: `'A'`, `'я'`, `'3'`.

```csharp
char letter = 'A';
char digit  = '7';
char space  = ' ';
char emoji  = '☺';
```"""})

post(805, "code", {"language": "csharp", "title": "Операции с bool и char", "code": """// bool — логические операции
bool isAdult = true;
bool hasTicket = false;

Console.WriteLine($"Войдёт: {isAdult && hasTicket}");
Console.WriteLine($"Проверяем: {isAdult || hasTicket}");
Console.WriteLine($"Не взрослый: {!isAdult}");

// Операторы сравнения возвращают bool
int age = 20;
bool canVote = age >= 18;
bool isTeenager = age >= 13 && age <= 19;
Console.WriteLine($"Может голосовать: {canVote}");
Console.WriteLine($"Подросток: {isTeenager}");

// char — символьные операции
char ch = 'A';
Console.WriteLine($"Символ: {ch}");
Console.WriteLine($"Код: {(int)ch}");   // ASCII/Unicode код

// Арифметика с char (работает через int)
char next = (char)(ch + 1);
Console.WriteLine($"Следующий: {next}");

// Проверка типа символа
char testChar = '5';
Console.WriteLine($"Это цифра: {char.IsDigit(testChar)}");
Console.WriteLine($"Это буква: {char.IsLetter(testChar)}");
Console.WriteLine($"Верхний регистр: {char.ToUpper('a')}");"""})

post(805, "prediction", {"language": "csharp", "code": """char c = 'e';
Console.WriteLine((int)c);
Console.WriteLine(char.ToUpper(c));
Console.WriteLine(char.IsLetter(c));
Console.WriteLine((char)((int)c - 32));""", "expectedOutput": "101\nE\nTrue\nE", "hint": "Код 'e' = 101 (ASCII). ToUpper('e') = 'E'. char.IsLetter('e') = True. 101 - 32 = 69, это символ 'E'."})

post(805, "quiz", {"question": "Какое значение вернёт выражение: `(5 > 3) && (10 < 5) || !(2 == 2)`?", "options": ["true", "false", "Ошибка компиляции", "null"], "correct": 1, "explanation": "(5>3)=true, (10<5)=false. true && false = false. !(2==2) = !true = false. false || false = false. Приоритет: ! выше &&, && выше ||."})

post(805, "fill_blank", {"template": """// Определить, является ли символ буквой верхнего регистра
char ch = 'G';

bool isLetter = char.___(ch);
bool isUpper = char.___(ch);
bool isDigit = char.___(ch);

// Преобразовать в нижний регистр
char lower = char.___(ch);

Console.WriteLine($"{ch}: буква={isLetter}, верхний={isUpper}, цифра={isDigit}, нижний={lower}");""", "blanks": ["IsLetter", "IsUpper", "IsDigit", "ToLower"], "hint": "Класс char предоставляет статические методы: IsLetter, IsUpper, IsLower, IsDigit, ToUpper, ToLower. Все принимают char и возвращают bool (кроме ToUpper/ToLower)."})

post(805, "mini_task", {"language": "csharp", "title": "Анализ символов строки", "description": "Подсчитайте количество букв, цифр и пробелов в заданной строке.", "starterCode": """string text = "Hello World 123";
int letters = 0;
int digits = 0;
int spaces = 0;

foreach (char c in text)
{
    if (char.___(c)) letters++;
    else if (char.___(c)) digits++;
    else if (c == ' ') spaces++;
}

Console.WriteLine($"Букв: {letters}");
Console.WriteLine($"Цифр: {digits}");
Console.WriteLine($"Пробелов: {spaces}");""", "expectedOutput": "Букв: 10\nЦифр: 3\nПробелов: 2"})

# ============================================================
# Lesson 806: "Строки: string и интерполяция"
# ============================================================
print("\n=== Lesson 806: Строки: string и интерполяция ===")

post(806, "text", {"markdown": """## Строки в C#

`string` — это неизменяемая последовательность символов Unicode. Тип `string` является псевдонимом для `System.String`.

### Способы создания строк

```csharp
string s1 = "Обычная строка";
string s2 = @"Verbatim строка (обратный слэш не нужно экранировать)";
string s3 = $"Интерполированная: {1 + 2}";
string s4 = $@"Verbatim + интерполяция: {s1}";
// Raw string literals (C# 11+) пишутся с тройными кавычками
// и позволяют использовать "" внутри без экранирования
```

### Эскейп-последовательности

| Последовательность | Значение |
|-------------------|----------|
| `\\n` | Новая строка |
| `\\t` | Табуляция |
| `\\\\"` | Кавычка |
| `\\\\` | Обратный слэш |
| `\\u0041` | Unicode символ (A) |"""})

post(806, "code", {"language": "csharp", "title": "Основные методы работы со строками", "code": """string text = "  Hello, World!  ";

// Основные методы
Console.WriteLine(text.Length);              // 18
Console.WriteLine(text.Trim());              // "Hello, World!"
Console.WriteLine(text.ToUpper());
Console.WriteLine(text.ToLower());
Console.WriteLine(text.Contains("World"));   // True
Console.WriteLine(text.Replace("World", "C#"));

// Разделение и объединение
string csv = "один,два,три,четыре";
string[] parts = csv.Split(',');
Console.WriteLine(parts.Length);             // 4
Console.WriteLine(parts[0]);                 // один

string joined = string.Join(" | ", parts);
Console.WriteLine(joined);

// Поиск и извлечение
string sentence = "Программирование на C#";
Console.WriteLine(sentence.IndexOf("C#"));  // 18
Console.WriteLine(sentence.Substring(0, 15));
Console.WriteLine(sentence.StartsWith("Прогр")); // True

// Проверка
Console.WriteLine(string.IsNullOrEmpty(""));      // True
Console.WriteLine(string.IsNullOrWhiteSpace("  ")); // True"""})

post(806, "code", {"language": "csharp", "title": "Строковая интерполяция и форматирование", "code": """string name = "Алексей";
int age = 28;
double salary = 85000.5;
DateTime date = new DateTime(2024, 3, 15);

// Базовая интерполяция
Console.WriteLine($"Имя: {name}, возраст: {age}");

// Форматирование чисел
Console.WriteLine($"Зарплата: {salary:N2}");      // 85 000,50
Console.WriteLine($"Зарплата: {salary:C}");        // валюта
Console.WriteLine($"Процент: {0.235:P1}");          // 23,5%
Console.WriteLine($"Число: {12345:D8}");            // 00012345

// Форматирование дат
Console.WriteLine($"Дата: {date:dd.MM.yyyy}");      // 15.03.2024
Console.WriteLine($"Дата: {date:D}");               // длинный формат

// Выравнивание (ширина поля)
Console.WriteLine($"{'Товар',-15}{'Цена',8}");
Console.WriteLine($"{"Яблоки",-15}{12.5,8:F2}");
Console.WriteLine($"{"Бананы",-15}{8.99,8:F2}");

// StringBuilder для многократной конкатенации
var sb = new System.Text.StringBuilder();
for (int i = 1; i <= 5; i++)
{
    sb.Append($"Элемент {i} ");
}
Console.WriteLine(sb.ToString());"""})

post(806, "prediction", {"language": "csharp", "code": """string a = "Hello";
string b = "World";

Console.WriteLine(a + " " + b);
Console.WriteLine($"{a.ToLower()}_{b.ToUpper()}");
Console.WriteLine(a.Length + b.Length);
Console.WriteLine(a[1]);
Console.WriteLine(string.Concat(a, b).Length);""", "expectedOutput": "Hello World\nhello_WORLD\n10\ne\n10", "hint": "a+\" \"+b = конкатенация. ToLower/ToUpper меняют регистр. Длины: 5+5=10. a[1] = второй символ 'e' (индексация с 0). Concat без пробела: 5+5=10."})

post(806, "fix_bug", {"language": "csharp", "buggyCode": """string firstName = "Иван";
string lastName = "Петров";
int year = 1995;

// Форматирование профиля
string profile = "Имя: " + firstname + " " + lastName;
string birth = $"Год рождения: {year}";
string age_info = $"Возраст: " + (2024 - year);

// Извлечь первую букву
char initial = lastName[1];  // первая буква фамилии

Console.WriteLine(profile);
Console.WriteLine(birth);
Console.WriteLine(age_info);
Console.WriteLine($"Инициал: {initial}.");""", "expectedOutput": "Имя: Иван Петров\nГод рождения: 1995\nВозраст: 29\nИнициал: П.", "hint": "Найдите две ошибки: опечатка в имени переменной (firstname вместо firstName) и неверный индекс (первый элемент массива — это индекс 0, а не 1)."})

post(806, "mini_task", {"language": "csharp", "title": "Форматирование таблицы товаров", "description": "Выведите таблицу товаров с выравниванием. Используйте интерполяцию строк с форматированием ширины поля.", "starterCode": """string[] names = { "Ноутбук", "Мышь", "Клавиатура" };
double[] prices = { 75000.0, 1200.50, 3500.0 };
int[] quantities = { 2, 5, 3 };

Console.WriteLine($"{"Товар",-15}{"Цена",10}{"Кол-во",8}{"Сумма",12}");
Console.WriteLine(new string('-', 45));

for (int i = 0; i < names.Length; i++)
{
    double total = ___ * ___;
    Console.WriteLine($"{names[i],-15}{prices[i],10:F2}{quantities[i],8}{total,12:F2}");
}""", "expectedOutput": "Товар               Цена  Кол-во       Сумма\n---------------------------------------------\nНоутбук           75000,00       2   150000,00\nМышь               1200,50       5     6002,50\nКлавиатура         3500,00       3    10500,00"})

# ============================================================
# Lesson 807: "Приведение типов"
# ============================================================
print("\n=== Lesson 807: Приведение типов ===")

post(807, "text", {"markdown": """## Приведение типов в C#

Приведение типов — это преобразование значения одного типа в другой.

### Неявное приведение (Implicit conversion)

Происходит автоматически, когда нет потери данных:

```
byte → short → int → long → float → double
```

```csharp
int i = 42;
long l = i;      // неявное: int → long (безопасно)
double d = i;    // неявное: int → double (безопасно)
```

### Явное приведение (Explicit cast)

Требует оператора `(тип)`, возможна потеря данных:

```csharp
double d = 9.99;
int i = (int)d;  // = 9 (дробная часть отбрасывается!)
```

### Методы Convert и Parse

```csharp
string s = "42";
int i = int.Parse(s);           // исключение если не число
int j = Convert.ToInt32(s);     // исключение если null или не число
bool ok = int.TryParse(s, out int k);  // безопасно, без исключения
```""" })

post(807, "code", {"language": "csharp", "title": "Все способы приведения типов", "code": """// 1. Неявное приведение
int intVal = 100;
long longVal = intVal;      // int → long
double doubleVal = intVal;  // int → double
Console.WriteLine($"long: {longVal}, double: {doubleVal}");

// 2. Явное приведение (cast)
double pi = 3.14159;
int truncated = (int)pi;    // 3 (обрезание, не округление!)
Console.WriteLine($"(int)3.14159 = {truncated}");

long bigNumber = 3_000_000_000L;
int overflow = (int)bigNumber;  // потеря данных!
Console.WriteLine($"Переполнение: {overflow}");

// 3. Методы Convert
string numStr = "255";
int parsed = Convert.ToInt32(numStr);
byte b = Convert.ToByte(numStr);
Console.WriteLine($"Convert: {parsed}, byte: {b}");

// 4. Parse
double price = double.Parse("19.99");
Console.WriteLine($"Parse: {price}");

// 5. TryParse — безопасный вариант
string input = "abc123";
if (int.TryParse(input, out int result))
{
    Console.WriteLine($"Успех: {result}");
}
else
{
    Console.WriteLine($"Не число: {input}");
}

// 6. as и is операторы (для ссылочных типов)
object obj = "Строка";
string str = obj as string;  // null если не string
Console.WriteLine(obj is string);  // True""" })

post(807, "prediction", {"language": "csharp", "code": """double d = 7.9;
int a = (int)d;
int b = (int)Math.Round(d);
int c = (int)Math.Floor(d);
int e = (int)Math.Ceiling(d);

Console.WriteLine(a);
Console.WriteLine(b);
Console.WriteLine(c);
Console.WriteLine(e);""", "expectedOutput": "7\n8\n7\n8", "hint": "(int) просто обрезает дробную часть = 7. Round(7.9) = 8 (округление к ближайшему). Floor(7.9) = 7 (вниз). Ceiling(7.9) = 8 (вверх)."})

post(807, "fix_bug", {"language": "csharp", "buggyCode": """// Считаем среднюю оценку студента
int[] grades = { 85, 92, 78, 96, 88 };
int sum = 0;

foreach (int grade in grades)
{
    sum += grade;
}

// Ошибка: целочисленное деление даёт неточный результат
int average = sum / grades.Length;
Console.WriteLine($"Сумма: {sum}");
Console.WriteLine($"Среднее: {average}");
Console.WriteLine($"Среднее точное: {average:F2}");""", "expectedOutput": "Сумма: 439\nСреднее: 87.80\nСреднее точное: 87.80", "hint": "Проблема в том, что деление int / int даёт int. Нужно явно привести один из операндов к double: (double)sum / grades.Length. Также тип переменной average должен быть double."})

post(807, "quiz", {"question": "Что выведет код: `Console.WriteLine((int)(-7.9));`?", "options": ["-8 (округление к ближайшему)", "-7 (усечение к нулю)", "8 (по абсолютному значению)", "Исключение OverflowException"], "correct": 1, "explanation": "Явное приведение (int) всегда усекает дробную часть в сторону нуля. Для -7.9 это -7 (не -8). Если нужно банковское округление, используйте Math.Round()."})

post(807, "fill_blank", {"template": """string userInput = Console.ReadLine() ?? "0";

// Безопасное преобразование с обработкой ошибок
if (int.___(userInput, out int number))
{
    // Успешно распознали число
    double squared = Math.___(number, 2);  // возведение в степень
    Console.WriteLine($"{number}^2 = {(___)squared}");
}
else
{
    Console.WriteLine("Введите целое число!");
}""", "blanks": ["TryParse", "Pow", "int"], "hint": "TryParse возвращает bool и заполняет out-параметр. Math.Pow возвращает double. Для вывода целого числа нужно явное приведение (int)."})

# ============================================================
# Lesson 808: "Nullable типы: ?, ??, ??="
# ============================================================
print("\n=== Lesson 808: Nullable типы: ?, ??, ??= ===")

post(808, "text", {"markdown": """## Nullable типы в C#

По умолчанию типы значений (int, double, bool и т.д.) **не могут быть null**. Nullable типы решают эту проблему.

### Nullable value types (T?)

```csharp
int? age = null;     // int? = Nullable<int>
double? price = null;
bool? isActive = null;
```

### Проверка и получение значения

```csharp
int? x = 42;

bool hasValue = x.HasValue;     // true
int value = x.Value;            // 42 (исключение если null!)
int safe = x.GetValueOrDefault(); // 42 или 0
int withDefault = x.GetValueOrDefault(-1); // 42 или -1
```

### Операторы null

| Оператор | Название | Поведение |
|----------|----------|-----------|
| `??` | Null-coalescing | Возвращает правый операнд если левый null |
| `??=` | Null-coalescing assignment | Присваивает если переменная null |
| `?.` | Null-conditional | Безопасный вызов члена, возвращает null вместо исключения |
| `!` | Null-forgiving | Говорит компилятору «это точно не null» |"""})

post(808, "code", {"language": "csharp", "title": "Работа с Nullable типами и null-операторами", "code": """// Nullable value types
int? score = null;
Console.WriteLine(score.HasValue);           // False
Console.WriteLine(score.GetValueOrDefault()); // 0

score = 85;
Console.WriteLine(score.HasValue);           // True
Console.WriteLine(score.Value);              // 85

// ?? оператор
int? nullableAge = null;
int age = nullableAge ?? 18;  // если null — используй 18
Console.WriteLine($"Возраст: {age}");  // 18

string? name = null;
string displayName = name ?? "Аноним";
Console.WriteLine(displayName);  // Аноним

// ??= оператор (присваивание если null)
string? config = null;
config ??= "default_value";
Console.WriteLine(config);  // default_value

config ??= "другое значение"; // НЕ перезапишет, уже не null
Console.WriteLine(config);  // default_value

// ?. оператор (null-conditional)
string? text = null;
int? length = text?.Length;  // не исключение, а null
Console.WriteLine(length.HasValue);  // False

text = "Hello";
Console.WriteLine(text?.ToUpper());  // HELLO
Console.WriteLine(text?.Length ?? 0); // 5

// Цепочка
string[]? lines = null;
int? firstLen = lines?[0]?.Length;
Console.WriteLine(firstLen);  // (пусто, null)"""})

post(808, "prediction", {"language": "csharp", "code": """int? a = null;
int? b = 10;
int? c = 20;

Console.WriteLine(a ?? b ?? c);
Console.WriteLine(b ?? c);
Console.WriteLine((a ?? 0) + (b ?? 0));

a ??= 5;
Console.WriteLine(a);

a ??= 100;
Console.WriteLine(a);""", "expectedOutput": "10\n10\n10\n5\n5", "hint": "a??b??c: a=null→пробуем b=10→возвращаем 10. b??c: b=10, не null→10. (null??0)+(10??0)=0+10=10. a??=5: a=null→присваиваем 5. a??=100: a=5 не null→не меняем."})

post(808, "quiz", {"question": "Какое исключение возникнет при обращении к `.Value` nullable-переменной, если она равна null?", "options": ["NullReferenceException", "InvalidOperationException", "ArgumentNullException", "OverflowException"], "correct": 1, "explanation": "При обращении к .Value у Nullable<T> когда HasValue == false выбрасывается InvalidOperationException с сообщением 'Nullable object must have a value'. Используйте GetValueOrDefault() или ?? для безопасного доступа."})

post(808, "fill_blank", {"template": """// База данных может вернуть null для некоторых полей
string? userName = GetUserFromDb();  // может вернуть null
int? userAge = GetUserAge();         // может вернуть null

// Безопасный вывод
Console.WriteLine(userName ___ "Неизвестный пользователь");

// Безопасный вызов метода
Console.WriteLine(userName___ToUpper());

// Убедиться что userAge не null, иначе установить 0
int age = userAge ___ 0;

// Присвоить значение только если null
userName ___= "Гость";""", "blanks": ["??", "?.", "??", "??"], "hint": "?? — оператор «если null то». ?. — безопасный вызов метода (возвращает null вместо исключения). ??= — присвоить только если переменная null."})

post(808, "mini_task", {"language": "csharp", "title": "Обработка данных с пропусками", "description": "Вычислите среднее значение массива, в котором некоторые элементы могут быть null. Пропускайте null-значения.", "starterCode": """int?[] measurements = { 10, null, 30, null, 50, 20, null, 40 };
int sum = 0;
int count = 0;

foreach (int? m in measurements)
{
    if (m ___ null)  // проверка на null
    {
        sum += m ___ 0;  // безопасное получение значения
        count++;
    }
}

double average = count > 0 ? (double)sum / count : 0;
Console.WriteLine($"Значений: {count}");
Console.WriteLine($"Сумма: {sum}");
Console.WriteLine($"Среднее: {average:F1}");""", "expectedOutput": "Значений: 5\nСумма: 150\nСреднее: 30,0"})

# ============================================================
# Lesson 809: "var, константы и readonly"
# ============================================================
print("\n=== Lesson 809: var, константы и readonly ===")

post(809, "text", {"markdown": """## var — неявная типизация

`var` позволяет компилятору **вывести тип** переменной автоматически из выражения справа. Тип определяется **на этапе компиляции**, не во время выполнения (в отличие от `dynamic`).

```csharp
var i = 42;           // int
var d = 3.14;         // double
var s = "hello";      // string
var list = new List<int>(); // List<int>
```

### Когда использовать var

**Рекомендуется**: когда тип очевиден из контекста
```csharp
var customer = new Customer();  // очевидно Customer
var items = GetItems();         // зависит от контекста
```

**Не рекомендуется**: когда тип неочевиден
```csharp
var result = Process();  // какой тип? неясно
```

## const — константы

`const` объявляет константу — значение, которое **не может изменяться** и задаётся во время компиляции.

```csharp
const double Pi = 3.14159265358979;
const int MaxAttempts = 3;
const string Version = "1.0.0";
```

## readonly — поля только для чтения

`readonly` — значение задаётся при объявлении **или в конструкторе**, после чего не может изменяться.

```csharp
class Config
{
    public readonly string ConnectionString;
    public readonly DateTime StartTime = DateTime.Now;

    public Config(string conn)
    {
        ConnectionString = conn; // разрешено в конструкторе
    }
}
```"""})

post(809, "code", {"language": "csharp", "title": "var, const и readonly на практике", "code": """// var — вывод типа
var greeting = "Привет";  // string
var count = 0;             // int
var ratio = 1.5;           // double

Console.WriteLine(greeting.GetType().Name); // String
Console.WriteLine(count.GetType().Name);    // Int32

// const — константы
const double GravityMs2 = 9.80665;
const int SecondsInHour = 3600;
const string AppName = "MyApp";

// Расчёт с константами
double time = 3.0;
double velocity = GravityMs2 * time;
Console.WriteLine($"Скорость падения за {time}с: {velocity:F2} м/с");

// readonly в классе
var circle = new Circle(5.0);
Console.WriteLine($"Радиус: {circle.Radius}");
Console.WriteLine($"Площадь: {circle.Area:F2}");

class Circle
{
    // readonly — задаётся в конструкторе
    public readonly double Radius;

    // const — общая для всех экземпляров
    public const double Pi = Math.PI;

    public Circle(double radius)
    {
        Radius = radius;
        // Pi = 3; // ошибка! const нельзя изменить
    }

    public double Area => Pi * Radius * Radius;
}"""})

post(809, "prediction", {"language": "csharp", "code": """const int Base = 10;
var values = new int[] { 1, 2, 3, 4, 5 };
var sum = 0;

foreach (var v in values)
{
    sum += v * Base;
}

var average = sum / values.Length;
Console.WriteLine($"Сумма: {sum}");
Console.WriteLine($"Среднее: {average}");
Console.WriteLine(sum.GetType().Name);""", "expectedOutput": "Сумма: 150\nСреднее: 30\nInt32", "hint": "Каждый элемент умножается на Base=10: 10+20+30+40+50=150. average=150/5=30 (целочисленное деление). sum — это int (var вывел тип из 0), GetType().Name = Int32."})

post(809, "compare_variants", {"question": "Какой подход лучше использовать для хранения магических чисел в коде?", "variants": [{"title": "Именованные константы", "code": """// Хорошая практика
const int MaxRetryAttempts = 3;
const double TimeoutSeconds = 30.0;
const string DefaultCurrency = "RUB";

for (int i = 0; i < MaxRetryAttempts; i++)
{
    // Попытка соединения
    if (Connect(TimeoutSeconds))
        break;
}"""}, {"title": "Магические числа", "code": """// Плохая практика
for (int i = 0; i < 3; i++)  // что значит 3?
{
    // Попытка соединения
    if (Connect(30.0))  // что значит 30?
        break;
}
// Если нужно изменить — ищем по всему коду!"""}], "correct": 0, "explanation": "Именованные константы делают код самодокументируемым, упрощают изменение значений в одном месте и предотвращают ошибки. Магические числа ухудшают читаемость и сопровождаемость кода."})

post(809, "fill_blank", {"template": """// Заполните правильные ключевые слова:

// Значение числа пи (не изменяется, известно при компиляции)
___ double Pi = 3.14159;

// Дата запуска приложения (задаётся один раз при старте)
___ DateTime LaunchDate = DateTime.Now;

// Переменная для подсчёта (тип выводится автоматически)
___ counter = 0;

// Список строк (тип выводится из выражения справа)
___ names = new List<string>();

// Размер буфера (константа класса)
___ int BufferSize = 4096;""", "blanks": ["const", "readonly", "var", "var", "const"], "hint": "const: значение известно при компиляции, не меняется никогда. readonly: задаётся при объявлении или в конструкторе. var: компилятор выводит тип из правой части выражения."})

post(809, "knowledge_check", {"topic": "var, const, readonly", "question": "Чем отличается `const` от `readonly`?", "options": ["const можно изменить в конструкторе, readonly — нет", "const вычисляется при компиляции и не требует памяти в объекте, readonly — при выполнении (в конструкторе)", "readonly работает только с числовыми типами", "Они полностью идентичны, только разный синтаксис"], "correct": 1, "explanation": "const — это константа времени компиляции: значение встраивается в IL-код напрямую, поле не занимает память в объекте. readonly — значение задаётся при выполнении (при объявлении или в конструкторе) и хранится в памяти объекта. const может быть только примитивного типа или string."})

post(809, "mini_task", {"language": "csharp", "title": "Калькулятор геометрических фигур", "description": "Используя константу Pi и var, вычислите площадь и периметр окружности и прямоугольника.", "starterCode": """const double Pi = Math.PI;

// Окружность с радиусом 7
var radius = 7.0;
var circleArea = Pi * ___ * ___;
var circumference = 2 * Pi * ___;

// Прямоугольник 8 x 5
var width = 8.0;
var height = 5.0;
var rectArea = ___ * ___;
var perimeter = 2 * (___ + ___);

Console.WriteLine($"Площадь круга: {circleArea:F2}");
Console.WriteLine($"Длина окружности: {circumference:F2}");
Console.WriteLine($"Площадь прямоугольника: {rectArea:F0}");
Console.WriteLine($"Периметр прямоугольника: {perimeter:F0}");""", "expectedOutput": "Площадь круга: 153,94\nДлина окружности: 43,98\nПлощадь прямоугольника: 40\nПериметр прямоугольника: 26"})

print("\n=== Готово! Все уроки 800-809 заполнены ===")
