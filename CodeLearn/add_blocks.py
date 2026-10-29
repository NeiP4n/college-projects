#!/usr/bin/env python3
"""Add content blocks to all C# course lessons."""
import json, urllib.request, urllib.error, sys

BASE = "http://localhost:5003/api/builder/lessons"

def post(lesson_id, block_type, content_obj):
    url = f"{BASE}/{lesson_id}/blocks"
    data = json.dumps({"type": block_type, "content": json.dumps(content_obj, ensure_ascii=False)}).encode("utf-8")
    req = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"}, method="POST")
    try:
        with urllib.request.urlopen(req) as r:
            return r.status
    except urllib.error.HTTPError as e:
        print(f"  ERROR {e.code} on lesson {lesson_id} type {block_type}: {e.read().decode()[:200]}")
        return e.code

def add(lesson_id, blocks):
    print(f"\n=== Lesson {lesson_id} ===")
    for t, c in blocks:
        st = post(lesson_id, t, c)
        print(f"  [{st}] {t}")

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 206 – Что такое C# и .NET
# ─────────────────────────────────────────────────────────────────────────────
add(206, [
("text", {"markdown": """## Что такое C#?

**C#** (произносится «си шарп») — объектно-ориентированный язык программирования, созданный Microsoft в 2000 году.

Ключевые факты:
- Строго типизированный
- Компилируется в **IL-байткод** (Intermediate Language)
- Запускается на виртуальной машине **CLR** (Common Language Runtime)
- Является частью платформы **.NET**

### Экосистема .NET

| Компонент | Роль |
|-----------|------|
| CLR | Выполнение IL-кода, сборка мусора |
| BCL | Базовая библиотека классов |
| SDK | Компилятор, инструменты сборки |
| NuGet | Менеджер пакетов |

> 💡 Современная версия — **.NET 8+** — кросс-платформенная: Windows, Linux, macOS."""}),

("code", {"language": "csharp", "title": "Первая программа на C#", "code": """// Самая простая программа на C#
Console.WriteLine("Hello, World!");

// C# 10+ поддерживает top-level statements — не нужно писать class и Main
// Компилятор сам создаёт точку входа

int year = 2024;
Console.WriteLine($"Добро пожаловать в C# {year}!");"""}),

("quiz", {"question": "Что такое CLR в контексте .NET?", "options": ["Common Language Runtime — виртуальная машина, выполняющая IL-код", "C Language Runtime — среда выполнения языка C", "Core Library Repository — хранилище библиотек", "Compiled Logic Runner — компилятор байткода"], "correct": 0, "explanation": "CLR (Common Language Runtime) — это виртуальная машина .NET, которая запускает IL-код, управляет памятью (сборка мусора) и обеспечивает безопасность типов."}),

("prediction", {"language": "csharp", "code": """string name = "C#";
int version = 12;
Console.WriteLine($"{name} v{version}");""", "expectedOutput": "C# v12", "hint": "$ перед строкой — это интерполяция, {выражение} подставляется в строку"}),

("code_to_intent", {"code": """Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Привет, мир!");""", "options": ["Устанавливает кодировку UTF-8 и выводит приветствие на русском", "Открывает файл и записывает в него текст", "Загружает внешнюю библиотеку и вызывает функцию"], "correctIntent": 0}),

("checkpoint", {"title": "Ты знаешь основы .NET", "description": "Убедись, что понимаешь следующее:", "tasks": ["C# компилируется в IL-байткод, а не в машинный код напрямую", "CLR — виртуальная машина, запускающая IL", "Платформа .NET включает CLR, BCL и SDK", "$ перед строкой включает интерполяцию переменных"]}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 207 – dotnet CLI и NuGet
# ─────────────────────────────────────────────────────────────────────────────
add(207, [
("text", {"markdown": """## dotnet CLI — инструмент командной строки

`dotnet` — главная команда для работы с проектами .NET.

### Часто используемые команды

```
dotnet new console -n MyApp   # создать консольный проект
dotnet build                  # скомпилировать
dotnet run                    # запустить
dotnet test                   # запустить тесты
dotnet add package Newtonsoft.Json  # добавить NuGet-пакет
```

### NuGet — менеджер пакетов .NET

NuGet — аналог npm (JS) или pip (Python). Хранит тысячи готовых библиотек.

**Файл проекта** `MyApp.csproj` хранит зависимости:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```"""}),

("code", {"language": "csharp", "title": "Структура минимального проекта", "code": """// Program.cs — точка входа (top-level statements)
Console.WriteLine("Мой проект запущен!");

// После dotnet new console -n HelloApp создаётся:
// HelloApp/
// ├── HelloApp.csproj   ← описание проекта
// ├── Program.cs        ← твой код
// └── obj/              ← временные файлы сборки"""}),

("drag_order", {"pieces": ["dotnet run", "dotnet new console -n HelloApp", "cd HelloApp", "dotnet build"], "solution": [1, 2, 3, 0], "hint": "Сначала создай проект, войди в папку, собери, затем запусти"}),

("quiz", {"question": "Какая команда добавляет NuGet-пакет в проект?", "options": ["dotnet add package <Имя>", "dotnet install <Имя>", "nuget get <Имя>", "dotnet import <Имя>"], "correct": 0, "explanation": "dotnet add package <НазваниеПакета> — стандартная команда CLI для добавления NuGet-зависимости в .csproj"}),

("fill_blank", {"template": "dotnet ___ console -n ___ && cd ___ && dotnet ___", "blanks": ["new", "MyApp", "MyApp", "run"], "hint": "Команды: new создаёт проект, run — запускает"}),

("mini_task", {"language": "csharp", "title": "Hello, CLI!", "description": "Напиши программу, которая выводит строку: dotnet is awesome!", "starterCode": "", "expectedOutput": "dotnet is awesome!"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 208 – Переменные и примитивные типы
# ─────────────────────────────────────────────────────────────────────────────
add(208, [
("text", {"markdown": """## Переменные и типы данных

В C# каждая переменная имеет **строгий тип**.

### Основные типы

| Тип | Размер | Диапазон / Пример |
|-----|--------|-------------------|
| `int` | 32 бит | −2 147 483 648 ... 2 147 483 647 |
| `long` | 64 бит | ±9.2 × 10¹⁸ |
| `double` | 64 бит | ≈ 15 знаков точности |
| `decimal` | 128 бит | финансовые расчёты |
| `bool` | 1 бит | true / false |
| `char` | 16 бит | один символ Unicode |
| `string` | — | строка символов |

### Объявление переменных

```csharp
int age = 25;
double price = 9.99;
bool isActive = true;
var name = "Алиса";   // var — вывод типа компилятором
```

> ⚠️ `var` не делает переменную динамической — тип определяется при объявлении и фиксируется навсегда."""}),

("code", {"language": "csharp", "title": "Примеры переменных и типов", "code": """int count = 10;
long bigNumber = 10_000_000_000L;   // _ для читаемости
double pi = 3.14159;
decimal price = 19.99m;             // суффикс m для decimal
bool isDone = false;
char grade = 'A';
string greeting = "Привет!";

// var — компилятор выводит тип автоматически
var year = 2024;          // int
var ratio = 0.5;          // double
var msg = "Сообщение";    // string

Console.WriteLine($"Год: {year}, Pi ≈ {pi:F2}");
Console.WriteLine($"Цена: {price:C}");"""}),

("quiz", {"question": "Какой суффикс нужно добавить к числовому литералу, чтобы создать переменную типа decimal?", "options": ["m или M", "d или D", "f или F", "l или L"], "correct": 0, "explanation": "Суффикс m (или M) превращает числовой литерал в decimal: decimal x = 9.99m; Без суффикса 9.99 — это double."}),

("fill_blank", {"template": "___ name = \"Алиса\";\n___ age = 20;\n___ isStudent = ___;\nConsole.WriteLine($\"{name} ({age}): {___}\");", "blanks": ["string", "int", "bool", "true", "isStudent"], "hint": "Первые три — типы переменных, последнее — имя булевой переменной"}),

("prediction", {"language": "csharp", "code": """int x = 10;
int y = 3;
Console.WriteLine(x / y);
Console.WriteLine((double)x / y);""", "expectedOutput": "3\n3.3333333333333335", "hint": "Деление двух int даёт int (без остатка). Приведение к double меняет поведение."}),

("fix_bug", {"language": "csharp", "buggyCode": """decimal balance = 1500.50;
Console.WriteLine($"Баланс: {balance}");""", "expectedOutput": "Баланс: 1500,50", "hint": "Литерал decimal требует суффикс m"}),

("transfer_test", {"question": "В банковском приложении нужно хранить сумму денег с точностью до копейки. Какой тип выбрать?", "options": ["decimal — точное представление, нет ошибок округления с плавающей точкой", "double — достаточная точность и быстрее в вычислениях", "int — хранить в копейках как целое число (тоже вариант)"], "correct": 0, "explanation": "decimal специально создан для финансовых расчётов: он не имеет ошибок двоичного представления. double и float могут давать копеечные ошибки."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 209 – Строки и интерполяция
# ─────────────────────────────────────────────────────────────────────────────
add(209, [
("text", {"markdown": """## Строки в C#

`string` — ссылочный тип, но ведёт себя как значимый (неизменяемый).

### Создание строк

```csharp
string s1 = "Привет";
string s2 = @"C:\\Users\\vasya";     // verbatim — без экранирования
string s3 = \"\"\"                    // raw string literals (C# 11)
    {
      "name": "Vasya"
    }
    \"\"\";
```

### Строковая интерполяция

```csharp
string name = "Маша";
int age = 22;
string result = $"Меня зовут {name}, мне {age} лет.";
// Можно форматировать:
double pi = Math.PI;
Console.WriteLine($"Pi = {pi:F3}");  // Pi = 3.142
```

### Полезные методы String

| Метод | Пример | Результат |
|-------|--------|-----------|
| `.ToUpper()` | `"hello".ToUpper()` | `"HELLO"` |
| `.Trim()` | `"  hi  ".Trim()` | `"hi"` |
| `.Contains()` | `"hello".Contains("ell")` | `true` |
| `.Replace()` | `"abc".Replace("b","X")` | `"aXc"` |
| `.Split()` | `"a,b,c".Split(',')` | `["a","b","c"]` |
| `.Length` | `"hello".Length` | `5` |"""}),

("code", {"language": "csharp", "title": "Строковые операции", "code": """string firstName = "Иван";
string lastName  = "Иванов";

// Конкатенация
string full1 = firstName + " " + lastName;

// Интерполяция (предпочтительно)
string full2 = $"{firstName} {lastName}";

// String.Format (старый стиль)
string full3 = string.Format("{0} {1}", firstName, lastName);

Console.WriteLine(full2.ToUpper());              // ИВАН ИВАНОВ
Console.WriteLine(full2.Length);                 // 11
Console.WriteLine(full2.Replace("а", "A"));      // ИвAн ИвAнов
Console.WriteLine(full2.StartsWith("Иван"));     // True

// Verbatim string
string path = @"C:\\Users\\ivan\\documents";
Console.WriteLine(path);"""}),

("quiz", {"question": "Какой из следующих способов является строковой интерполяцией в C#?", "options": ['$"Привет, {name}!"', '"Привет, " + name + "!"', 'string.Format("Привет, {0}!", name)', '@"Привет, {name}!"'], "correct": 0, "explanation": "$ перед кавычками включает интерполяцию. Выражения в {} вычисляются и подставляются в строку. @ — это verbatim строка (буквальная), в ней {} не интерполируются."}),

("fill_blank", {"template": "string city = \"Москва\";\nint pop = 12_500_000;\nConsole.WriteLine(___\"Город: {___}, население: {___:N0}\");", "blanks": ["$", "city", "pop"], "hint": "$ включает интерполяцию, :N0 форматирует число с разделителями"}),

("prediction", {"language": "csharp", "code": """string s = "  Hello World  ";
Console.WriteLine(s.Trim().ToLower().Replace("world", "C#"));""", "expectedOutput": "hello C#", "hint": "Методы вызываются по цепочке слева направо"}),

("mini_task", {"language": "csharp", "title": "Форматирование имени", "description": "Создай переменные firstName = \"анна\" и lastName = \"смирнова\". Выведи полное имя с заглавных букв через пробел.", "starterCode": "", "expectedOutput": "Анна Смирнова"}),

("compare_variants", {"question": "Какой способ формирования строки предпочтителен в современном C#?", "variants": [{"title": "Конкатенация через +", "code": "string r = \"Цена: \" + price + \" руб.\""}, {"title": "Интерполяция $", "code": "string r = $\"Цена: {price} руб.\";"}, {"title": "String.Format", "code": "string r = string.Format(\"Цена: {0} руб.\", price);"}], "correct": 1, "explanation": "Интерполяция ($\"\") наиболее читаема и современна. string.Format используется когда шаблон хранится в переменной. + медленнее при множественной конкатенации."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 210 – Операторы и выражения
# ─────────────────────────────────────────────────────────────────────────────
add(210, [
("text", {"markdown": """## Операторы в C#

### Арифметические операторы

| Оператор | Действие | Пример |
|----------|----------|--------|
| `+` | Сложение | `5 + 3 = 8` |
| `-` | Вычитание | `5 - 3 = 2` |
| `*` | Умножение | `5 * 3 = 15` |
| `/` | Деление | `7 / 2 = 3` (int!), `7.0 / 2 = 3.5` |
| `%` | Остаток | `7 % 3 = 1` |
| `++` | Инкремент | `x++ → x = x + 1` |
| `--` | Декремент | `x-- → x = x - 1` |

### Логические операторы

```csharp
bool a = true, b = false;
a && b   // И   → false
a || b   // ИЛИ → true
!a       // НЕ  → false
```

### Операторы сравнения

`==`, `!=`, `<`, `>`, `<=`, `>=`

### Тернарный оператор

```csharp
int abs = x >= 0 ? x : -x;   // если x>=0 → x, иначе -x
```

### Оператор null-coalescing

```csharp
string name = input ?? "Гость";  // если input == null → "Гость"
string name2 = input ??= "Гость"; // присвоить, если null
```"""}),

("code", {"language": "csharp", "title": "Операторы на практике", "code": """int a = 17, b = 5;
Console.WriteLine(a + b);    // 22
Console.WriteLine(a - b);    // 12
Console.WriteLine(a * b);    // 85
Console.WriteLine(a / b);    // 3   (целочисленное деление!)
Console.WriteLine(a % b);    // 2   (остаток)

// Порядок операций: как в математике
int result = 2 + 3 * 4;     // 14, а не 20
int result2 = (2 + 3) * 4;  // 20

// Тернарный оператор
int x = -5;
string sign = x > 0 ? "положительное" : x < 0 ? "отрицательное" : "ноль";
Console.WriteLine(sign);  // отрицательное

// Null-coalescing
string? input = null;
string displayName = input ?? "Гость";
Console.WriteLine(displayName);  // Гость"""}),

("prediction", {"language": "csharp", "code": """int x = 10;
x += 5;
x *= 2;
Console.WriteLine(x);""", "expectedOutput": "30", "hint": "x += 5 → x=15, затем x *= 2 → x=30"}),

("quiz", {"question": "Что выведет код: Console.WriteLine(10 / 3);", "options": ["3", "3.333...", "3.33", "4"], "correct": 0, "explanation": "Оба операнда — int, поэтому выполняется целочисленное деление. 10 / 3 = 3 (остаток 1 отбрасывается). Для получения дроби нужно (double)10 / 3."}),

("timed_test", {"question": "Что выведет: Console.WriteLine(2 + 3 * 4 - 1);", "timeLimit": 20, "expectedOutput": "13", "hint": "Умножение выполняется первым: 3*4=12, затем 2+12-1=13"}),

("fix_bug", {"language": "csharp", "buggyCode": """double half = 7 / 2;
Console.WriteLine(half);""", "expectedOutput": "3.5", "hint": "7/2 — целочисленное деление. Добавь .0 к одному из чисел или используй приведение типа"}),

("counterfactual", {"code": """string? s = null;
string result = s ?? "default";
Console.WriteLine(result);""", "removeLine": 2, "question": "Что будет, если убрать строку 2 (использование ??) и просто написать: string result = s;", "answer": "Ошибка компиляции: нельзя присвоить string? к string без явного разыменования"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 211 – if, else if, switch
# ─────────────────────────────────────────────────────────────────────────────
add(211, [
("text", {"markdown": """## Условные операторы

### if / else if / else

```csharp
int score = 75;

if (score >= 90)
    Console.WriteLine("Отлично");
else if (score >= 70)
    Console.WriteLine("Хорошо");
else if (score >= 50)
    Console.WriteLine("Удовлетворительно");
else
    Console.WriteLine("Провал");
```

### switch (классический)

```csharp
switch (day) {
    case "Monday":
        Console.WriteLine("Понедельник");
        break;
    case "Friday":
        Console.WriteLine("Пятница");
        break;
    default:
        Console.WriteLine("Другой день");
        break;
}
```

### switch expression (C# 8+)

```csharp
string result = score switch {
    >= 90 => "Отлично",
    >= 70 => "Хорошо",
    >= 50 => "Удовлетворительно",
    _     => "Провал"  // _ — default
};
```

> 💡 **switch expression** возвращает значение — это выражение, а не оператор."""}),

("code", {"language": "csharp", "title": "Пример: классификация чисел", "code": """int number = 42;

// Классический if/else
if (number > 0)
    Console.WriteLine("Положительное");
else if (number < 0)
    Console.WriteLine("Отрицательное");
else
    Console.WriteLine("Ноль");

// switch expression — элегантнее
string category = number switch {
    > 100   => "большое",
    > 10    => "среднее",
    > 0     => "маленькое",
    0       => "ноль",
    _       => "отрицательное"
};
Console.WriteLine($"{number} — {category}");"""}),

("quiz", {"question": "В switch expression что означает символ _ в ветке _ => \"...\";", "options": ["Wildcard-паттерн, соответствует любому значению (аналог default)", "Переменная с именем _", "Пустое значение (null)", "Синтаксическая ошибка — так нельзя писать"], "correct": 0, "explanation": "В switch expression символ _ — это discard pattern, он означает «любое другое значение». Это аналог default в классическом switch."}),

("prediction", {"language": "csharp", "code": """int x = 15;
string result = x switch {
    > 20 => "big",
    > 10 => "medium",
    _    => "small"
};
Console.WriteLine(result);""", "expectedOutput": "medium", "hint": "15 > 20? Нет. 15 > 10? Да → medium"}),

("drag_order", {"pieces": ["else", "{", "if (age >= 18)", "Console.WriteLine(\"Взрослый\");", "Console.WriteLine(\"Несовершеннолетний\");", "}"], "solution": [2, 1, 3, 0, 4, 5], "hint": "if идёт первым, затем открывающая скобка, тело if, else, тело else, закрывающая скобка"}),

("fix_bug", {"language": "csharp", "buggyCode": """int temp = 25;
if (temp > 30)
    Console.WriteLine("Жарко");
else if (temp > 20)
    Console.WriteLine("Тепло");
else if (temp > 10)
    Console.WriteLine("Прохладно");
else ;
    Console.WriteLine("Холодно");""", "expectedOutput": "Тепло", "hint": "Лишняя точка с запятой после else делает ветку пустой — следующая строка выполнится всегда"}),

("two_path", {"question": "Когда использовать if/else, а когда switch expression?", "pathA": {"title": "if / else if / else", "code": """if (status == \"active\") action = \"run\";
else if (status == \"paused\") action = \"wait\";
else if (status == \"stopped\") action = \"quit\";
else action = \"unknown\";"""}, "pathB": {"title": "switch expression", "code": """action = status switch {
    \"active\"  => \"run\",
    \"paused\"  => \"wait\",
    \"stopped\" => \"quit\",
    _          => \"unknown\"
};"""}, "explanation": "switch expression лаконичнее когда проверяется одна переменная на много значений. if/else if гибче — можно проверять разные переменные и сложные условия."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 212 – Циклы: for, while, foreach
# ─────────────────────────────────────────────────────────────────────────────
add(212, [
("text", {"markdown": """## Циклы в C#

### for

```csharp
for (int i = 0; i < 5; i++) {
    Console.WriteLine(i);  // 0, 1, 2, 3, 4
}
```

### while

```csharp
int n = 1;
while (n <= 5) {
    Console.WriteLine(n);
    n++;
}
```

### do-while (выполняется хотя бы раз)

```csharp
int x = 10;
do {
    Console.WriteLine(x);
    x--;
} while (x > 8);
// выведет: 10, 9
```

### foreach

```csharp
int[] nums = { 1, 2, 3, 4, 5 };
foreach (int n in nums) {
    Console.WriteLine(n);
}
```

### break и continue

```csharp
for (int i = 0; i < 10; i++) {
    if (i == 3) continue;  // пропустить 3
    if (i == 7) break;     // выйти на 7
    Console.WriteLine(i);  // 0, 1, 2, 4, 5, 6
}
```"""}),

("code", {"language": "csharp", "title": "Циклы на практике", "code": """// for: итерация с индексом
int sum = 0;
for (int i = 1; i <= 10; i++)
    sum += i;
Console.WriteLine($"Сумма 1..10 = {sum}");  // 55

// foreach: обход коллекции
string[] fruits = { "Яблоко", "Банан", "Вишня" };
foreach (string fruit in fruits)
    Console.WriteLine($"  {fruit}");

// while: условие неизвестно заранее
int value = 1;
while (value < 100)
    value *= 2;
Console.WriteLine($"Первая степень 2 > 100: {value}");  // 128"""}),

("prediction", {"language": "csharp", "code": """int result = 0;
for (int i = 1; i <= 5; i++) {
    if (i % 2 == 0) continue;
    result += i;
}
Console.WriteLine(result);""", "expectedOutput": "9", "hint": "continue пропускает чётные числа. Нечётные: 1+3+5=9"}),

("quiz", {"question": "Какой цикл гарантированно выполняется хотя бы один раз?", "options": ["do-while", "while", "for", "foreach"], "correct": 0, "explanation": "В do-while тело цикла выполняется, а потом проверяется условие. Поэтому тело выполняется минимум один раз, даже если условие сразу false."}),

("drag_order", {"pieces": ["Console.WriteLine(i);", "for (int i = 0; i < 3; i++)", "{", "}"], "solution": [1, 2, 0, 3], "hint": "Заголовок цикла, открывающая скобка, тело, закрывающая скобка"}),

("mini_task", {"language": "csharp", "title": "Сумма чётных", "description": "Найди сумму всех чётных чисел от 1 до 20 включительно и выведи результат.", "starterCode": "", "expectedOutput": "110"}),

("mutation", {"language": "csharp", "baseCode": """for (int i = 1; i <= 5; i++) {
    Console.WriteLine(i);
}""", "mutation": "Измени цикл так, чтобы числа выводились в обратном порядке: от 5 до 1", "expectedOutput": "5\n4\n3\n2\n1", "hint": "Начни с i = 5, уменьшай на 1 (i--), пока i >= 1"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 213 – Pattern Matching
# ─────────────────────────────────────────────────────────────────────────────
add(213, [
("text", {"markdown": """## Pattern Matching в C#

Pattern Matching — мощный механизм для проверки формы данных.

### Типы паттернов

```csharp
object obj = "Привет";

// Type pattern
if (obj is string s) Console.WriteLine(s.Length);

// Constant pattern
if (obj is "Привет") Console.WriteLine("Поздоровался!");

// Relational pattern (C# 9)
int x = 42;
bool big = x is > 100;

// Logical patterns
bool inRange = x is >= 10 and <= 100;

// Property pattern
var point = new { X = 5, Y = 0 };
if (point is { X: > 0, Y: 0 }) Console.WriteLine("На положительной оси X");
```

### switch + patterns

```csharp
object shape = new Circle(5.0);

double area = shape switch {
    Circle c        => Math.PI * c.Radius * c.Radius,
    Rectangle r     => r.Width * r.Height,
    null            => 0,
    _               => throw new ArgumentException()
};
```"""}),

("code", {"language": "csharp", "title": "Pattern Matching на практике", "code": """// Type pattern в if
object[] items = { 42, "hello", 3.14, true, null! };

foreach (object item in items) {
    string desc = item switch {
        int n      => $"Целое число: {n}",
        string s   => $"Строка длиной {s.Length}",
        double d   => $"Дробное: {d:F2}",
        bool b     => $"Булево: {b}",
        null       => "Пустое значение",
        _          => "Неизвестный тип"
    };
    Console.WriteLine(desc);
}"""}),

("quiz", {"question": "Что делает паттерн: if (obj is string s)", "options": ["Проверяет, что obj — это string, и если да, помещает его в переменную s", "Объявляет строковую переменную s и сравнивает её с obj", "Бросает исключение если obj не строка", "Всегда возвращает true"], "correct": 0, "explanation": "is string s — это declaration pattern. Он проверяет тип И одновременно присваивает результат в новую переменную s. Это удобнее чем (obj is string) && (string s = (string)obj)."}),

("code_to_intent", {"code": """int? n = GetValue();
string result = n switch {
    null    => "нет данных",
    < 0     => "отрицательное",
    0       => "ноль",
    <= 100  => "в диапазоне",
    _       => "большое"
};""", "options": ["Классифицирует nullable int по значению с помощью паттернов", "Сортирует массив целых чисел по возрастанию", "Конвертирует строку в число и проверяет формат"], "correctIntent": 0}),

("fill_blank", {"template": "int score = 85;\nstring grade = score ___\n{\n    ___ 90 => \"A\",\n    ___ 70 => \"B\",\n    ___    => \"C\"\n};", "blanks": ["switch", ">= ", ">= ", "_"], "hint": "switch выражение, паттерны >= и wildcard _"}),

("compare_variants", {"question": "Какой вариант использует паттерн-матчинг корректно?", "variants": [{"title": "Устаревший стиль", "code": "if (obj is string)\n{\n  string s = (string)obj;\n  Console.Write(s.Length);\n}"}, {"title": "Современный pattern", "code": "if (obj is string s)\n  Console.Write(s.Length);"}, {"title": "Неправильно", "code": "if (obj == string)\n  Console.Write(obj.Length);"}], "correct": 1, "explanation": "is string s — declaration pattern. Он безопасно проверяет тип и создаёт переменную s в одном выражении. Старый стиль работает, но многословен. obj == string — синтаксическая ошибка."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 214 – Методы и параметры
# ─────────────────────────────────────────────────────────────────────────────
add(214, [
("text", {"markdown": """## Методы в C#

Метод — именованный блок кода, принимающий параметры и возвращающий значение.

### Синтаксис

```csharp
возвращаемый_тип НазваниеМетода(тип параметр1, тип параметр2) {
    // тело
    return результат;
}
```

### Параметры

```csharp
// Обязательные
int Add(int a, int b) => a + b;

// Необязательные (по умолчанию)
void Greet(string name, string greeting = "Привет") =>
    Console.WriteLine($"{greeting}, {name}!");

// Именованные аргументы
Greet(greeting: "Здравствуй", name: "Иван");

// ref и out
void Swap(ref int a, ref int b) { int t = a; a = b; b = t; }

// params — переменное число аргументов
int Sum(params int[] nums) => nums.Sum();
int total = Sum(1, 2, 3, 4, 5);  // 15
```

### Expression body (=>)

```csharp
int Square(int x) => x * x;  // краткая форма для одного выражения
```"""}),

("code", {"language": "csharp", "title": "Методы на практике", "code": """// Простой метод
int Multiply(int a, int b) => a * b;

// Метод с параметрами по умолчанию
string FormatName(string first, string last, string title = "") =>
    string.IsNullOrEmpty(title)
        ? $"{first} {last}"
        : $"{title} {first} {last}";

// Метод с out-параметром
bool TryDivide(int a, int b, out double result) {
    if (b == 0) { result = 0; return false; }
    result = (double)a / b;
    return true;
}

Console.WriteLine(Multiply(6, 7));  // 42
Console.WriteLine(FormatName("Иван", "Петров", "Dr."));  // Dr. Иван Петров

if (TryDivide(10, 3, out double r))
    Console.WriteLine($"10 / 3 = {r:F2}");  // 10 / 3 = 3.33"""}),

("quiz", {"question": "Что означает ключевое слово params в параметре метода?", "options": ["Метод принимает массив аргументов, которые можно передавать через запятую", "Параметр является необязательным", "Параметр передаётся по ссылке", "Метод не возвращает значение"], "correct": 0, "explanation": "params позволяет передавать переменное количество аргументов: Sum(1,2,3) или Sum(new int[]{1,2,3}). Внутри метода это обычный массив."}),

("fill_blank", {"template": "___ double Average(___ double[] nums)\n{\n    if (nums.___ == 0) return 0;\n    return nums.Sum() ___ nums.Length;\n}", "blanks": ["static", "params", "Length", "/"], "hint": "static метод, params массив, .Length для длины, / для деления"}),

("fix_bug", {"language": "csharp", "buggyCode": """int Max(int a, int b) {
    if (a > b)
        return a;
}

Console.WriteLine(Max(5, 3));""", "expectedOutput": "5", "hint": "Компилятор требует возврата значения по всем путям выполнения. Добавь return b; в ветку else"}),

("mini_task", {"language": "csharp", "title": "Метод Factorial", "description": "Напиши метод int Factorial(int n), вычисляющий n! итерационно. Вызови Factorial(5) и выведи результат.", "starterCode": "", "expectedOutput": "120"}),

("intent_to_code", {"language": "csharp", "intent": "Напиши метод IsPalindrome(string s), который возвращает true если строка читается одинаково в обоих направлениях. Вызови с аргументом \"level\" и выведи результат.", "expectedOutput": "True", "hint": "Сравни строку с её реверсом: new string(s.Reverse().ToArray())"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 215 – Func, Action, Lambda
# ─────────────────────────────────────────────────────────────────────────────
add(215, [
("text", {"markdown": """## Делегаты, Func, Action и лямбды

### Делегаты

Делегат — тип, который представляет метод.

```csharp
delegate int Transformer(int x);
Transformer square = x => x * x;
Console.WriteLine(square(5));  // 25
```

### Func и Action (встроенные делегаты)

```csharp
// Func<T1, T2, ..., TResult> — принимает аргументы, возвращает TResult
Func<int, int>         square  = x => x * x;
Func<int, int, int>    add     = (a, b) => a + b;
Func<string, bool>     isEmpty = s => s.Length == 0;

// Action<T1, T2, ...> — принимает аргументы, ничего не возвращает
Action<string>         print   = msg => Console.WriteLine(msg);
Action                 hello   = () => Console.WriteLine("Hello!");
```

### Лямбда-выражения

```csharp
// Короткая форма
Func<int, int> f = x => x * 2;

// Блочная форма
Func<int, int> g = x => {
    int result = x * 2;
    return result + 1;
};
```

### Передача лямбд в методы (LINQ)

```csharp
var nums = new[] { 1, 2, 3, 4, 5 };
var evens = nums.Where(x => x % 2 == 0);  // { 2, 4 }
```"""}),

("code", {"language": "csharp", "title": "Func, Action, Lambda", "code": """// Func
Func<int, int, int> multiply = (a, b) => a * b;
Console.WriteLine(multiply(6, 7));  // 42

// Action
Action<string> log = msg => Console.WriteLine($"[LOG] {msg}");
log("Приложение запущено");

// Передача функции как аргумента
int[] numbers = { 5, 2, 8, 1, 9, 3 };
int[] sorted = numbers.OrderBy(x => x).ToArray();
Console.WriteLine(string.Join(", ", sorted));  // 1, 2, 3, 5, 8, 9

// Хранение нескольких трансформаций
var transforms = new List<Func<int, int>> {
    x => x + 1,
    x => x * 2,
    x => x - 3
};

int value = 5;
foreach (var f in transforms)
    value = f(value);
Console.WriteLine(value);  // ((5+1)*2)-3 = 9"""}),

("quiz", {"question": "Чем Func<int, bool> отличается от Action<int>?", "options": ["Func возвращает bool, Action ничего не возвращает (void)", "Func быстрее Action", "Func принимает int, Action — bool", "Нет разницы — это синонимы"], "correct": 0, "explanation": "Func<T, TResult> всегда имеет возвращаемое значение (последний типовой параметр — тип возврата). Action<T> ничего не возвращает (аналог void)."}),

("prediction", {"language": "csharp", "code": """Func<int, int> double_ = x => x * 2;
Func<int, int> addTen  = x => x + 10;

int result = addTen(double_(5));
Console.WriteLine(result);""", "expectedOutput": "20", "hint": "Сначала double_(5) = 10, затем addTen(10) = 20"}),

("compare_variants", {"question": "Какой вариант является правильной лямбдой для Func<int, int, bool>?", "variants": [{"title": "Вариант A", "code": "(a, b) => a > b"}, {"title": "Вариант B", "code": "a => a > 0"}, {"title": "Вариант C", "code": "() => true"}], "correct": 0, "explanation": "Func<int, int, bool> принимает два int и возвращает bool. Лямбда (a, b) => a > b принимает два параметра и возвращает bool. Вариант B — Func<int,bool>, Вариант C — Func<bool>."}),

("drag_order", {"pieces": ["Func<int, int> square = x => x * x;", "int[] nums = { 1, 2, 3, 4, 5 };", "int[] squares = nums.Select(square).ToArray();", "Console.WriteLine(string.Join(\", \", squares));"], "solution": [1, 0, 2, 3], "hint": "Сначала массив, потом делегат, потом применение, потом вывод"}),

("mini_task", {"language": "csharp", "title": "Фильтр с лямбдой", "description": "Создай список чисел от 1 до 10. С помощью Func<int, bool> и LINQ Where выбери только те, которые делятся на 3. Выведи результат через пробел.", "starterCode": "", "expectedOutput": "3 6 9"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 216 – Классы, свойства, конструкторы
# ─────────────────────────────────────────────────────────────────────────────
add(216, [
("text", {"markdown": """## Классы в C#

Класс — шаблон для создания объектов.

### Структура класса

```csharp
public class Person {
    // Поля (private по умолчанию)
    private string _name;

    // Автосвойства
    public int Age { get; set; }

    // Свойство с логикой
    public string Name {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException();
    }

    // Конструктор
    public Person(string name, int age) {
        Name = name;
        Age = age;
    }

    // Метод
    public string Greet() => $"Привет! Я {Name}, мне {Age} лет.";
}
```

### Primary Constructor (C# 12)

```csharp
public class Point(double x, double y) {
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Distance => Math.Sqrt(X*X + Y*Y);
}
```

### Модификаторы доступа

| Модификатор | Видимость |
|------------|-----------|
| `public` | Все |
| `private` | Только этот класс |
| `protected` | Класс и наследники |
| `internal` | Внутри сборки |"""}),

("code", {"language": "csharp", "title": "Класс Car", "code": """public class Car {
    public string Brand  { get; init; }   // init — только при создании
    public string Model  { get; init; }
    public int    Year   { get; set; }
    private int   _mileage = 0;

    public Car(string brand, string model, int year) {
        Brand = brand;
        Model = model;
        Year  = year;
    }

    public void Drive(int km) {
        if (km <= 0) throw new ArgumentException("Должно быть > 0");
        _mileage += km;
    }

    public string Info => $"{Year} {Brand} {Model} — {_mileage} км";
}

var car = new Car("Toyota", "Camry", 2022);
car.Drive(150);
car.Drive(300);
Console.WriteLine(car.Info);  // 2022 Toyota Camry — 450 км"""}),

("quiz", {"question": "Что такое автосвойство в C#?", "options": ["Свойство вида public int Age { get; set; } — компилятор сам создаёт backing field", "Свойство, которое устанавливается автоматически из конструктора", "Статическое свойство, не требующее объекта", "Свойство с автоматической валидацией данных"], "correct": 0, "explanation": "Автосвойство public int Age { get; set; } — синтаксический сахар. Компилятор создаёт скрытое поле и простые get/set. Это сокращение от: private int _age; public int Age { get => _age; set => _age = value; }"}),

("fill_blank", {"template": "public ___ Animal {\n    public ___ Name { get; ___ ; }\n    public Animal(___ name) { Name = ___; }\n    public ___ Speak() => Console.WriteLine($\"{Name} говорит!\");\n}", "blanks": ["class", "string", "set", "string", "name", "void"], "hint": "class, тип свойства, модификатор set, тип параметра, присваивание, возвращаемый тип"}),

("fix_bug", {"language": "csharp", "buggyCode": """public class Rectangle {
    public double Width;
    public double Height;

    public Rectangle(Width, Height) {
        this.Width = Width;
        this.Height = Height;
    }

    public double Area() => Width * Height;
}

var r = new Rectangle(5, 3);
Console.WriteLine(r.Area());""", "expectedOutput": "15", "hint": "Параметры конструктора должны иметь тип: double Width, double Height"}),

("mini_task", {"language": "csharp", "title": "Класс Circle", "description": "Создай класс Circle с полем Radius (double). Добавь свойство Area (только get), вычисляющее площадь. Создай Circle с Radius=5 и выведи площадь с точностью до 2 знаков.", "starterCode": "", "expectedOutput": "78.54"}),

("checkpoint", {"title": "ООП — базовые концепции", "description": "Убедись, что понимаешь:", "tasks": ["Класс — шаблон; объект — экземпляр класса", "Поля хранят состояние; свойства добавляют логику get/set", "Конструктор инициализирует объект при создании (new)", "private ограничивает видимость внутри класса", "this ссылается на текущий объект"]}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 217 – Наследование и полиморфизм
# ─────────────────────────────────────────────────────────────────────────────
add(217, [
("text", {"markdown": """## Наследование в C#

Наследование позволяет создать новый класс на основе существующего.

```csharp
public class Animal {
    public string Name { get; }
    public Animal(string name) => Name = name;

    public virtual string Speak() => "...";  // virtual — можно переопределить
}

public class Dog : Animal {        // Dog наследует Animal
    public Dog(string name) : base(name) {}  // вызов конструктора базового класса

    public override string Speak() => "Гав!";  // переопределение
}

public class Cat : Animal {
    public Cat(string name) : base(name) {}
    public override string Speak() => "Мяу!";
}
```

### Полиморфизм

```csharp
Animal[] animals = { new Dog("Шарик"), new Cat("Мурка"), new Dog("Рекс") };
foreach (Animal a in animals)
    Console.WriteLine($"{a.Name}: {a.Speak()}");
// Шарик: Гав!, Мурка: Мяу!, Рекс: Гав!
```

### abstract и sealed

```csharp
public abstract class Shape {        // нельзя создать экземпляр
    public abstract double Area();   // нет тела — обязательно переопределить
}

public sealed class FinalClass { }  // нельзя наследовать
```

### Интерфейсы

```csharp
public interface IDrawable {
    void Draw();   // только контракт, нет реализации (в стандартном случае)
}

public class Circle : Shape, IDrawable {
    public double Radius { get; }
    public Circle(double r) => Radius = r;
    public override double Area() => Math.PI * Radius * Radius;
    public void Draw() => Console.WriteLine($"O (r={Radius})");
}
```"""}),

("code", {"language": "csharp", "title": "Иерархия фигур", "code": """abstract class Shape {
    public abstract double Area();
    public virtual string Describe() => $"Фигура с площадью {Area():F2}";
}

class Circle : Shape {
    double R;
    public Circle(double r) => R = r;
    public override double Area() => Math.PI * R * R;
}

class Rectangle : Shape {
    double W, H;
    public Rectangle(double w, double h) { W = w; H = h; }
    public override double Area() => W * H;
    public override string Describe() => $"Прямоугольник {W}×{H} = {Area():F2}";
}

Shape[] shapes = { new Circle(5), new Rectangle(4, 6), new Circle(2) };
foreach (var s in shapes)
    Console.WriteLine(s.Describe());"""}),

("quiz", {"question": "Что означает ключевое слово override в методе?", "options": ["Метод переопределяет виртуальный или абстрактный метод базового класса", "Метод скрывает метод базового класса (без полиморфизма)", "Метод нельзя переопределить в дочерних классах", "Метод вызывается перед конструктором"], "correct": 0, "explanation": "override говорит компилятору, что метод переопределяет virtual или abstract метод родителя. Это обеспечивает динамический полиморфизм — при вызове через ссылку базового класса вызывается реализация дочернего."}),

("prediction", {"language": "csharp", "code": """class Base {
    public virtual void Print() => Console.WriteLine("Base");
}
class Child : Base {
    public override void Print() => Console.WriteLine("Child");
}

Base obj = new Child();
obj.Print();""", "expectedOutput": "Child", "hint": "Полиморфизм: хотя тип переменной Base, объект является Child, поэтому вызывается Child.Print()"}),

("code_to_intent", {"code": """interface ILogger {
    void Log(string message);
}
class ConsoleLogger : ILogger {
    public void Log(string message) => Console.WriteLine($"[INFO] {message}");
}
class FileLogger : ILogger {
    public void Log(string message) { /* запись в файл */ }
}""", "options": ["Определяет интерфейс для логгирования с двумя реализациями (консоль и файл)", "Создаёт абстрактный класс с двумя наследниками для вывода текста", "Объявляет две перегрузки метода Log"], "correctIntent": 0}),

("two_path", {"question": "Когда использовать abstract class, а когда interface?", "pathA": {"title": "abstract class", "code": """// Когда есть общая реализация
abstract class Vehicle {
    public int Speed { get; protected set; }
    public virtual void Accelerate(int delta)
        => Speed += delta;         // общий код!
    public abstract void Brake();  // каждый реализует по-своему
}"""}, "pathB": {"title": "interface", "code": """// Когда нужен только контракт
interface IDriveable {
    void Drive();
    void Stop();
}
// Класс может реализовывать МНОГО интерфейсов
class Car : IDriveable, IChargeable {
    public void Drive() { ... }
    public void Stop()  { ... }
    public void Charge(){ ... }
}"""}, "explanation": "abstract class хорош когда нужно поделиться общим кодом между похожими классами. interface — чистый контракт для разнородных классов, которые должны уметь одно и то же."}),

("constraint", {"language": "csharp", "title": "Полиморфный вывод", "description": "Создай базовый класс Animal со строкой Name и виртуальным методом Sound(). Создай Dog (Гав) и Cat (Мяу). Создай массив из 3 животных разного типа и выведи каждое в формате 'Имя: Звук'.", "constraint": "Нельзя использовать if/switch — только полиморфизм", "starterCode": "", "expectedOutput": "Шарик: Гав\nМурка: Мяу\nРекс: Гав", "hint": "Используй virtual/override и массив Animal[]"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 218 – Array, List<T>, Dictionary
# ─────────────────────────────────────────────────────────────────────────────
add(218, [
("text", {"markdown": """## Коллекции в C#

### Array (массив)

```csharp
int[] nums = new int[5];          // фиксированный размер
int[] data = { 10, 20, 30 };     // инициализация
Console.WriteLine(data[1]);       // 20
Console.WriteLine(data.Length);   // 3
Array.Sort(data);                 // сортировка
```

### List<T> (динамический список)

```csharp
var list = new List<int> { 1, 2, 3 };
list.Add(4);
list.Remove(2);
list.Insert(0, 99);
bool has = list.Contains(3);
int count = list.Count;
```

### Dictionary<TKey, TValue>

```csharp
var dict = new Dictionary<string, int> {
    ["apple"]  = 5,
    ["banana"] = 3
};
dict["cherry"] = 7;

if (dict.TryGetValue("apple", out int qty))
    Console.WriteLine($"Яблок: {qty}");

foreach (var (key, value) in dict)
    Console.WriteLine($"{key}: {value}");
```

### HashSet<T> (уникальные значения)

```csharp
var set = new HashSet<int> { 1, 2, 3, 2, 1 };
// set содержит только { 1, 2, 3 }
```"""}),

("code", {"language": "csharp", "title": "Коллекции на практике", "code": """// List<T>
var fruits = new List<string> { "Яблоко", "Банан", "Вишня" };
fruits.Add("Груша");
fruits.Remove("Банан");
Console.WriteLine($"Фруктов: {fruits.Count}");
Console.WriteLine(string.Join(", ", fruits));

// Dictionary
var wordCount = new Dictionary<string, int>();
string[] words = { "привет", "мир", "привет", "c#", "мир", "привет" };

foreach (string word in words) {
    if (!wordCount.ContainsKey(word))
        wordCount[word] = 0;
    wordCount[word]++;
}

foreach (var (word, count) in wordCount)
    Console.WriteLine($"  {word}: {count}");"""}),

("quiz", {"question": "В чём главное отличие Array от List<T>?", "options": ["Array имеет фиксированный размер, List<T> динамически расширяется", "Array быстрее List<T> во всех операциях", "List<T> может хранить только ссылочные типы", "Array поддерживает LINQ, а List<T> — нет"], "correct": 0, "explanation": "Array создаётся с фиксированным размером. List<T> автоматически расширяет внутренний буфер при Add. Оба поддерживают LINQ, оба могут хранить любые типы."}),

("fill_blank", {"template": "var scores = new _____<string, int>();\nscores[\"Alice\"] = 95;\nscores[\"Bob\"]   = 87;\n\nif (scores._____(\"Alice\", out ___ s))\n    Console.WriteLine($\"Alice: {s}\");", "blanks": ["Dictionary", "TryGetValue", "int"], "hint": "Dictionary, метод для безопасного получения значения, тип out-параметра"}),

("drag_order", {"pieces": ["var list = new List<int>();", "Console.WriteLine(list.Count);", "list.Add(10);", "list.Add(20);", "list.Remove(10);"], "solution": [0, 2, 3, 4, 1], "hint": "Сначала создай список, добавь элементы, удали один, потом выведи Count"}),

("mini_task", {"language": "csharp", "title": "Максимум в списке", "description": "Создай List<int> с числами: 5, 12, 3, 88, 41, 7. Найди максимум с помощью метода Max() и выведи его.", "starterCode": "", "expectedOutput": "88"}),

("knowledge_check", {"topic": "Коллекции", "question": "Какую коллекцию лучше использовать для хранения уникальных слов?", "options": ["HashSet<string>", "List<string>", "string[]", "Dictionary<string, string>"], "correct": 0, "explanation": "HashSet<T> гарантирует уникальность элементов и выполняет Contains/Add за O(1). List допускает дубликаты. Dictionary нужен для пар ключ-значение."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 219 – LINQ: Where, Select, GroupBy
# ─────────────────────────────────────────────────────────────────────────────
add(219, [
("text", {"markdown": """## LINQ — Language Integrated Query

LINQ позволяет запрашивать коллекции прямо в C#.

### Основные методы

```csharp
var nums = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Where — фильтрация
var evens = nums.Where(x => x % 2 == 0);  // { 2, 4, 6, 8, 10 }

// Select — трансформация
var squares = nums.Select(x => x * x);     // { 1, 4, 9, 16, ... }

// OrderBy / OrderByDescending
var sorted = nums.OrderByDescending(x => x);

// First / FirstOrDefault
int first = nums.First(x => x > 5);       // 6
int? found = nums.FirstOrDefault(x => x > 100);  // null

// Count, Sum, Min, Max, Average
int total = nums.Sum();                    // 55

// Any, All
bool anyBig  = nums.Any(x => x > 9);      // true
bool allPos  = nums.All(x => x > 0);      // true
```

### GroupBy

```csharp
var grouped = words.GroupBy(w => w.Length);
foreach (var g in grouped)
    Console.WriteLine($"Длина {g.Key}: {string.Join(", ", g)}");
```

### Цепочка методов

```csharp
var result = nums
    .Where(x => x % 2 == 0)
    .Select(x => x * x)
    .OrderByDescending(x => x)
    .Take(3);
// { 100, 64, 36 }
```"""}),

("code", {"language": "csharp", "title": "LINQ в действии", "code": """var products = new[] {
    new { Name = "Яблоко",  Price = 30,  Category = "Фрукт" },
    new { Name = "Банан",   Price = 45,  Category = "Фрукт" },
    new { Name = "Морковь", Price = 15,  Category = "Овощ"  },
    new { Name = "Брокколи",Price = 80,  Category = "Овощ"  },
    new { Name = "Киви",    Price = 120, Category = "Фрукт" },
};

// Фрукты дороже 40 рублей
var expFruits = products
    .Where(p => p.Category == "Фрукт" && p.Price > 40)
    .Select(p => p.Name);
Console.WriteLine(string.Join(", ", expFruits));  // Банан, Киви

// Средняя цена по категориям
var avgByCategory = products
    .GroupBy(p => p.Category)
    .Select(g => new { Category = g.Key, Avg = g.Average(p => p.Price) });

foreach (var item in avgByCategory)
    Console.WriteLine($"{item.Category}: {item.Avg:F0} руб.");"""}),

("quiz", {"question": "Что возвращает метод FirstOrDefault(), если элемент не найден?", "options": ["Значение по умолчанию для типа (null для ссылочных, 0 для int)", "Бросает исключение InvalidOperationException", "Возвращает последний элемент коллекции", "Возвращает -1"], "correct": 0, "explanation": "FirstOrDefault() возвращает default(T): для string — null, для int — 0, для struct — структуру с нулевыми полями. Метод First() бросает исключение если элемент не найден."}),

("prediction", {"language": "csharp", "code": """int[] nums = { 3, 1, 4, 1, 5, 9, 2, 6 };
var result = nums
    .Where(x => x > 3)
    .OrderBy(x => x)
    .First();
Console.WriteLine(result);""", "expectedOutput": "4", "hint": "Числа > 3: {4, 5, 9, 6}. Сортируем: {4, 5, 6, 9}. First() → 4"}),

("fill_blank", {"template": "var names = new[] { \"Аня\", \"Борис\", \"Света\", \"Дима\", \"Анна\" };\n\n// Имена на А, отсортированные\nvar result = names\n    ._____(n => n.StartsWith(\"А\"))\n    ._____()\n    .ToList();", "blanks": ["Where", "OrderBy"], "hint": "Where для фильтрации, OrderBy для сортировки (без параметра — нужен x => x)"}),

("mini_task", {"language": "csharp", "title": "LINQ агрегация", "description": "Дан список чисел: 5, 12, 3, 88, 41, 7, 29, 55, 16, 34. Найди сумму всех чисел больше 20 и выведи её.", "starterCode": "", "expectedOutput": "247"}),

("transfer_test", {"question": "Тебе нужно выбрать из базы данных пользователей старше 18, отсортировать по имени и взять первые 10. Какой LINQ-запрос правильный?", "options": ["users.Where(u => u.Age > 18).OrderBy(u => u.Name).Take(10)", "users.Take(10).Where(u => u.Age > 18).OrderBy(u => u.Name)", "users.OrderBy(u => u.Name).Where(u => u.Age > 18).First(10)"], "correct": 0, "explanation": "Порядок важен: сначала фильтрация (Where), потом сортировка (OrderBy), потом ограничение (Take). Иначе Take(10) возьмёт первые 10 до фильтрации, а First(10) — не существует."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 220 – try / catch / finally
# ─────────────────────────────────────────────────────────────────────────────
add(220, [
("text", {"markdown": """## Обработка исключений

Исключение — объект, описывающий ошибку. В C# они наследуют `Exception`.

### try / catch / finally

```csharp
try {
    int result = 10 / 0;           // DivideByZeroException
} catch (DivideByZeroException ex) {
    Console.WriteLine($"Ошибка: {ex.Message}");
} catch (Exception ex) {           // ловит все остальные
    Console.WriteLine($"Общая ошибка: {ex.Message}");
} finally {
    Console.WriteLine("Это выполнится в любом случае");
}
```

### Иерархия исключений

```
Exception
├── SystemException
│   ├── InvalidOperationException
│   ├── ArgumentException
│   │   └── ArgumentNullException
│   ├── NullReferenceException
│   └── DivideByZeroException
└── ApplicationException (для своих)
```

### throw

```csharp
void SetAge(int age) {
    if (age < 0) throw new ArgumentException("Возраст не может быть отрицательным");
    // ...
}
```

### when — фильтр исключений

```csharp
catch (HttpException ex) when (ex.StatusCode == 404) {
    Console.WriteLine("Страница не найдена");
}
```"""}),

("code", {"language": "csharp", "title": "Обработка исключений", "code": """// Безопасное деление
static bool TryDivide(int a, int b, out double result) {
    try {
        result = (double)a / b;
        if (b == 0) throw new DivideByZeroException();
        return true;
    } catch (DivideByZeroException) {
        result = 0;
        return false;
    }
}

// Безопасный парсинг
static int ParseSafe(string input) {
    try {
        return int.Parse(input);
    } catch (FormatException) {
        Console.WriteLine($"Не число: '{input}'");
        return 0;
    } finally {
        Console.WriteLine($"ParseSafe завершён для '{input}'");
    }
}

Console.WriteLine(ParseSafe("42"));     // 42
Console.WriteLine(ParseSafe("abc"));    // 0 (после сообщения об ошибке)"""}),

("quiz", {"question": "Когда выполняется блок finally?", "options": ["Всегда: и когда исключение произошло, и когда нет", "Только если исключение было поймано", "Только если исключение НЕ произошло", "Только если блок catch вернул true"], "correct": 0, "explanation": "finally выполняется всегда — это гарантия освобождения ресурсов. Даже если исключение не поймано или метод вызвал return в try, блок finally всё равно выполнится."}),

("drag_order", {"pieces": ["Console.WriteLine(\"finally\");", "try {", "} catch (Exception ex) {", "} finally {", "int x = int.Parse(\"abc\");", "Console.WriteLine(ex.Message);", "}"], "solution": [1, 4, 2, 5, 3, 0, 6], "hint": "try { тело } catch { обработка } finally { гарантированный код }"}),

("prediction", {"language": "csharp", "code": """try {
    Console.WriteLine("try");
    throw new Exception("oops");
    Console.WriteLine("after throw");
} catch (Exception ex) {
    Console.WriteLine($"catch: {ex.Message}");
} finally {
    Console.WriteLine("finally");
}""", "expectedOutput": "try\ncatch: oops\nfinally", "hint": "После throw выполнение в try прерывается. catch ловит. finally выполняется всегда. Строка after throw недостижима."}),

("fix_bug", {"language": "csharp", "buggyCode": """string input = "not a number";
int value = int.Parse(input);
Console.WriteLine($"Значение: {value}");""", "expectedOutput": "Ошибка: неверный формат\nЗначение: 0", "hint": "Оберни int.Parse в try/catch FormatException, при ошибке выведи сообщение и используй value = 0"}),

("knowledge_check", {"topic": "Исключения", "question": "Ты пишешь API и хочешь вернуть 404 когда элемент не найден. Как правильно сигнализировать об этом в бизнес-слое?", "options": ["throw new KeyNotFoundException($\"Элемент {id} не найден\");", "return null;", "Console.WriteLine(\"Не найдено\"); return 0;", "throw new Exception();"], "correct": 0, "explanation": "KeyNotFoundException — семантически правильный тип для 'ключ не найден'. Возврат null требует проверки во всех местах использования (null-hell). Конкретный тип исключения позволяет коду выше его правильно обработать."}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 221 – Кастомные исключения
# ─────────────────────────────────────────────────────────────────────────────
add(221, [
("text", {"markdown": """## Кастомные исключения

Когда стандартных исключений недостаточно — создай своё.

### Создание кастомного исключения

```csharp
public class InsufficientFundsException : Exception {
    public decimal Amount   { get; }
    public decimal Balance  { get; }

    public InsufficientFundsException(decimal amount, decimal balance)
        : base($"Недостаточно средств: нужно {amount}, есть {balance}")
    {
        Amount  = amount;
        Balance = balance;
    }
}
```

### Использование

```csharp
public class BankAccount {
    private decimal _balance;

    public void Withdraw(decimal amount) {
        if (amount > _balance)
            throw new InsufficientFundsException(amount, _balance);
        _balance -= amount;
    }
}

try {
    account.Withdraw(1000);
} catch (InsufficientFundsException ex) {
    Console.WriteLine(ex.Message);
    Console.WriteLine($"Нехватка: {ex.Amount - ex.Balance}");
}
```

### Правила именования

- Имя оканчивается на `Exception`
- Наследует `Exception` или `ApplicationException`
- Реализует три стандартных конструктора (best practice)"""}),

("code", {"language": "csharp", "title": "Кастомные исключения в банке", "code": """public class ValidationException : Exception {
    public string FieldName { get; }

    public ValidationException(string fieldName, string message)
        : base($"Ошибка в поле '{fieldName}': {message}") {
        FieldName = fieldName;
    }
}

public class User {
    private string _email = "";

    public string Email {
        get => _email;
        set {
            if (string.IsNullOrEmpty(value))
                throw new ValidationException("Email", "не может быть пустым");
            if (!value.Contains("@"))
                throw new ValidationException("Email", "должен содержать @");
            _email = value;
        }
    }
}

try {
    var user = new User();
    user.Email = "not-an-email";
} catch (ValidationException ex) {
    Console.WriteLine($"Поле: {ex.FieldName}");
    Console.WriteLine(ex.Message);
}"""}),

("quiz", {"question": "От какого класса должно наследовать кастомное исключение?", "options": ["Exception (или любого его наследника)", "Object", "Error", "ApplicationError"], "correct": 0, "explanation": "Все исключения в C# наследуют Exception. Твоё кастомное исключение должно наследовать Exception или один из его наследников (например, ArgumentException для ошибок аргументов)."}),

("fill_blank", {"template": "public class AgeException : ___ {\n    public int Age { get; }\n\n    public AgeException(int age)\n        ___ ($\"Неверный возраст: {age}\")\n    {\n        Age = ___;\n    }\n}", "blanks": ["Exception", ": base", "age"], "hint": "Наследование Exception, вызов конструктора базового класса через : base(), сохранение в поле"}),

("mini_task", {"language": "csharp", "title": "TemperatureException", "description": "Создай исключение TemperatureException с полем Temperature. Создай метод CheckTemp(double t), бросающий исключение если t < -273.15. Вызови с t = -300 и поймай, выведи сообщение.", "starterCode": "", "expectedOutput": "Недопустимая температура: -300"}),

("semantic_diff", {"intendedCode": """public class MyException : Exception {
    public MyException(string msg) : base(msg) {}
}""", "actualCode": """public class MyException {
    public string Message { get; }
    public MyException(string msg) { Message = msg; }
}""", "explanation": "Правый вариант НЕ является исключением — он просто класс с полем Message. Его нельзя throw, нельзя catch, нельзя передать в блок catch. Обязательно наследовать Exception."}),

("checkpoint", {"title": "Исключения — лучшие практики", "description": "Запомни правила:", "tasks": ["Имя исключения оканчивается на Exception", "Наследуй от Exception или более конкретного класса", "Добавляй полезные свойства (FieldName, Amount, etc.)", "Передавай message в базовый конструктор через : base(msg)", "Используй кастомное исключение когда стандартного недостаточно"]}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 222 – async / await основы
# ─────────────────────────────────────────────────────────────────────────────
add(222, [
("text", {"markdown": """## async / await

Асинхронность позволяет не блокировать поток пока ожидается IO.

### Без async — блокирует поток

```csharp
// Плохо: поток заморожен на время запроса
string data = File.ReadAllText("big.txt");  // блокирует!
```

### С async / await — поток свободен

```csharp
// Хорошо: поток возвращается в пул пока ждёт IO
string data = await File.ReadAllTextAsync("big.txt");
```

### Правила

1. Метод с `await` должен быть помечен `async`
2. `async` метод возвращает `Task`, `Task<T>` или `void` (только для event handlers)
3. `await` разворачивает Task и возвращает результат

```csharp
// Синхронный
string GetData() => "данные";

// Асинхронный аналог
async Task<string> GetDataAsync() {
    await Task.Delay(100);  // имитация IO
    return "данные";
}

// Вызов
string result = await GetDataAsync();
```

### Типичный паттерн

```csharp
public async Task<User> GetUserAsync(int id) {
    var response = await _httpClient.GetAsync($"/users/{id}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<User>();
}
```"""}),

("code", {"language": "csharp", "title": "async/await на практике", "code": """using System.Net.Http;

async Task<string> FetchTitleAsync(string url) {
    using var client = new HttpClient();
    string html = await client.GetStringAsync(url);
    // Находим тег <title>
    int start = html.IndexOf("<title>") + 7;
    int end   = html.IndexOf("</title>", start);
    return start > 6 && end > start
        ? html[start..end]
        : "Без заголовка";
}

// Имитация без реального запроса
async Task<int> SumAsync(int a, int b) {
    await Task.Delay(10);  // имитация задержки IO
    return a + b;
}

int result = await SumAsync(20, 22);
Console.WriteLine(result);  // 42"""}),

("quiz", {"question": "Что происходит когда поток встречает await?", "options": ["Поток возвращается в пул потоков, метод продолжается позже когда результат готов", "Создаётся новый поток для выполнения операции", "Вся программа приостанавливается до завершения операции", "await немедленно возвращает null если операция не готова"], "correct": 0, "explanation": "await — это точка подвески (suspension point). Когда встречается await, текущий поток освобождается. Когда операция завершается, продолжение метода запланируется на освободившемся потоке."}),

("prediction", {"language": "csharp", "code": """async Task<int> ComputeAsync() {
    Console.WriteLine("start");
    await Task.Delay(0);
    Console.WriteLine("end");
    return 42;
}

var task = ComputeAsync();
Console.WriteLine("after call");
int r = await task;
Console.WriteLine(r);""", "expectedOutput": "start\nafter call\nend\n42", "hint": "Task.Delay(0) — это awaitable. 'after call' может выйти до 'end' из-за асинхронного планирования."}),

("compare_variants", {"question": "Какой вариант правильно объявляет асинхронный метод?", "variants": [{"title": "Вариант A (правильно)", "code": "async Task<string> GetNameAsync()\n{\n    await Task.Delay(10);\n    return \"Иван\";\n}"}, {"title": "Вариант B (ошибка)", "code": "string GetNameAsync()\n{\n    await Task.Delay(10);\n    return \"Иван\";\n}"}, {"title": "Вариант C (неидиоматично)", "code": "async void GetNameAsync()\n{\n    await Task.Delay(10);\n    return \"Иван\";\n}"}], "correct": 0, "explanation": "A — правильно: async Task<string> когда метод возвращает значение. B — ошибка: без async нельзя использовать await. C — async void нельзя await'ить и у него нет возвращаемого значения (только для event handlers)."}),

("mini_task", {"language": "csharp", "title": "Первый async метод", "description": "Напиши async метод GreetAsync(string name), который ждёт Task.Delay(0), затем возвращает строку $\"Привет, {name}!\". Вызови его с именем World и выведи результат.", "starterCode": "", "expectedOutput": "Привет, World!"}),

("timed_test", {"question": "Каков тип возврата async метода, который не возвращает значение?", "timeLimit": 15, "expectedOutput": "Task", "hint": "async метод без return значения возвращает Task (не void, если только не event handler)"}),
])

# ─────────────────────────────────────────────────────────────────────────────
# LESSON 223 – Task, CancellationToken, WhenAll
# ─────────────────────────────────────────────────────────────────────────────
add(223, [
("text", {"markdown": """## Продвинутые Task-паттерны

### Task.WhenAll — параллельное ожидание

```csharp
Task<int> t1 = FetchDataAsync(1);
Task<int> t2 = FetchDataAsync(2);
Task<int> t3 = FetchDataAsync(3);

int[] results = await Task.WhenAll(t1, t2, t3);
// Все три запроса идут параллельно!
```

### Task.WhenAny — ждать первого

```csharp
var tasks = new[] { SlowAsync(), FastAsync(), MediumAsync() };
Task<string> first = await Task.WhenAny(tasks);
Console.WriteLine(await first);
```

### CancellationToken

```csharp
async Task DownloadAsync(string url, CancellationToken ct) {
    ct.ThrowIfCancellationRequested();
    var response = await httpClient.GetAsync(url, ct);
    // ...
}

// Использование
using var cts = new CancellationTokenSource(timeout: TimeSpan.FromSeconds(5));
await DownloadAsync("https://example.com", cts.Token);
```

### Task.Run — CPU-работа в пуле потоков

```csharp
int heavy = await Task.Run(() => {
    // тяжёлые вычисления, не блокируют UI
    return Enumerable.Range(1, 1000000).Sum();
});
```"""}),

("code", {"language": "csharp", "title": "Task.WhenAll и CancellationToken", "code": """async Task<int> SimulateWorkAsync(int id, int delayMs, CancellationToken ct = default) {
    await Task.Delay(delayMs, ct);
    return id * 10;
}

// WhenAll — параллельно
var tasks = new[] {
    SimulateWorkAsync(1, 100),
    SimulateWorkAsync(2, 50),
    SimulateWorkAsync(3, 200)
};
int[] results = await Task.WhenAll(tasks);
Console.WriteLine(string.Join(", ", results));  // 10, 20, 30 (в порядке задач)

// Отмена по таймауту
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
try {
    await SimulateWorkAsync(99, 300, cts.Token);  // 300ms > 150ms → отмена
} catch (OperationCanceledException) {
    Console.WriteLine("Операция отменена по таймауту");
}"""}),

("quiz", {"question": "Что делает Task.WhenAll(t1, t2, t3)?", "options": ["Запускает все задачи параллельно и ждёт завершения всех", "Запускает задачи последовательно и ждёт каждую по очереди", "Запускает только первую завершившуюся задачу", "Отменяет все задачи кроме первой"], "correct": 0, "explanation": "Task.WhenAll ждёт завершения всех переданных задач. Задачи выполняются параллельно (если запущены до WhenAll). Это позволяет сделать несколько независимых запросов одновременно."}),

("prediction", {"language": "csharp", "code": """async Task<int> AddAsync(int a, int b) {
    await Task.Delay(0);
    return a + b;
}

var t1 = AddAsync(10, 20);
var t2 = AddAsync(30, 40);
int[] r = await Task.WhenAll(t1, t2);
Console.WriteLine(r[0] + r[1]);""", "expectedOutput": "100", "hint": "t1 → 30, t2 → 70. r[0]+r[1] = 100"}),

("fill_blank", {"template": "using var cts = new _____Source(TimeSpan.FromSeconds(10));\n\ntry {\n    await LongOperationAsync(cts._____);\n} catch (OperationCanceled___) {\n    Console.WriteLine(\"Отменено\");\n}", "blanks": ["CancellationToken", "Token", "Exception"], "hint": "CancellationTokenSource, его Token, тип исключения при отмене"}),

("mini_task", {"language": "csharp", "title": "Параллельные задачи", "description": "Создай три метода GetNumberAsync(int n), каждый возвращает n*n после Task.Delay(0). Запусти их параллельно через Task.WhenAll для n=3,4,5. Выведи сумму результатов.", "starterCode": "", "expectedOutput": "50"}),

("code_to_intent", {"code": """var cts = new CancellationTokenSource();
cts.CancelAfter(5000);

try {
    await ProcessDataAsync(data, cts.Token);
} catch (OperationCanceledException) {
    logger.Warn("Обработка прервана по таймауту");
} finally {
    cts.Dispose();
}""", "options": ["Запускает обработку с тайм-аутом 5 секунд, логирует если превышен", "Повторяет операцию 5000 раз при ошибке", "Создаёт 5000 параллельных задач"], "correctIntent": 0}),
])

# ─────────────────────────────────────────────────────────────────────────────
# CHALLENGE LESSONS 224-227
# ─────────────────────────────────────────────────────────────────────────────
add(224, [
("text", {"markdown": """## ⚡ Испытание: Калькулятор

Ты прошёл базовые темы C#. Время проверить знания в боевом задании!

**Задача:** Реализовать консольный калькулятор.

**Требования:**
- Принимает два числа и оператор (+, -, *, /)
- Обрабатывает деление на ноль
- Работает с дробными числами (double)

**Пример:**
```
// Вызов:
Calculate(10, 3, '/')  →  "3.33"
Calculate(5,  0, '/')  →  "Ошибка: деление на ноль"
```"""}),

("challenge_task", {"language": "csharp", "title": "Консольный калькулятор", "description": "Реализуй метод string Calculate(double a, double b, char op). Поддержи операции +, -, *, /. При делении на ноль верни строку 'Ошибка: деление на ноль'. Результат округляй до 2 знаков.", "requirements": ["Поддержка +, -, *, /", "Обработка деления на ноль", "Результат с 2 знаками после запятой", "Проверить: Calculate(10, 3, '/') == \"3.33\""], "starterCode": "", "expectedOutput": "3.33\nОшибка: деление на ноль"}),

("quiz", {"question": "После выполнения испытания — какой тип использовать для точных денежных вычислений?", "options": ["decimal", "double", "float", "int"], "correct": 0, "explanation": "decimal — для финансов. double имеет ошибки представления. Для калькулятора с дробными числами общего назначения double приемлем, но для денег — только decimal."}),
])

add(225, [
("text", {"markdown": """## ⚡ Испытание: FizzBuzz

Классическая задача программирования — проверяет базовые навыки.

**Правила:**
- Для каждого числа от 1 до N:
  - Если делится на 3 → выведи "Fizz"
  - Если делится на 5 → выведи "Buzz"
  - Если делится на 15 → выведи "FizzBuzz"
  - Иначе → выведи само число

**Пример для N=15:**
```
1 2 Fizz 4 Buzz Fizz 7 8 Fizz Buzz 11 Fizz 13 14 FizzBuzz
```"""}),

("challenge_task", {"language": "csharp", "title": "FizzBuzz", "description": "Выведи FizzBuzz для чисел от 1 до 20. Каждое значение на новой строке.", "requirements": ["Числа от 1 до 20", "Делится на 15 → FizzBuzz", "Делится на 3 → Fizz", "Делится на 5 → Buzz", "Иначе → число"], "starterCode": "", "expectedOutput": "1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz\n16\n17\nFizz\n19\nBuzz"}),

("quiz", {"question": "Почему проверку на делимость на 15 нужно ставить ДО проверок на 3 и 5?", "options": ["Если поставить сначала /3, число 15 выдаст 'Fizz' а не 'FizzBuzz'", "Это требование задачи, порядок не важен", "Проверка /15 быстрее чем две отдельные проверки", "В C# условия выполняются снизу вверх"], "correct": 0, "explanation": "15 делится и на 3, и на 5. Если первой стоит ветка /3, то 15 попадёт в неё и выведет 'Fizz'. Поэтому сначала нужно проверить наиболее специфичное условие — делимость на 15."}),
])

add(226, [
("text", {"markdown": """## ⚡ Испытание: LINQ мастер

Покажи владение LINQ на реальной задаче обработки данных.

**Задача:** Обработать список продаж и получить статистику.

```csharp
record Sale(string Product, string Category, double Amount);

var sales = new List<Sale> {
    new("Ноутбук",    "Электроника", 55000),
    new("Телефон",    "Электроника", 35000),
    new("Стол",       "Мебель",      12000),
    new("Кресло",     "Мебель",       8500),
    new("Наушники",   "Электроника", 15000),
    new("Шкаф",       "Мебель",      25000),
};
```

**Нужно получить:**
1. Суммарную выручку по категориям
2. Топ-2 самых дорогих товара по названию"""}),

("challenge_task", {"language": "csharp", "title": "LINQ: статистика продаж", "description": "Используя приведённые данные, выведи суммарную выручку по категориям (каждая на новой строке в формате 'Категория: сумма') и топ-2 дорогих товара (названия через запятую).", "requirements": ["GroupBy по Category", "Sum по Amount", "OrderBy по Amount descending", "Take(2)"], "starterCode": "", "expectedOutput": "Электроника: 105000\nМебель: 45500\nНоутбук, Телефон"}),

("quiz", {"question": "Какой LINQ-метод группирует элементы по ключу?", "options": ["GroupBy", "Group", "Partition", "Cluster"], "correct": 0, "explanation": "GroupBy(x => x.Key) — стандартный LINQ-метод, возвращающий IEnumerable<IGrouping<TKey, TElement>>. У каждой группы есть Key и перечисление элементов."}),
])

add(227, [
("text", {"markdown": """## ⚡ Испытание: Асинхронщик

Финальное испытание по async/await и параллелизму.

**Задача:** Имитировать параллельную загрузку данных.

```csharp
// Каждый "запрос" занимает delay миллисекунд
async Task<string> FetchAsync(string name, int delayMs) {
    await Task.Delay(delayMs);
    return $"[{name}]";
}
```

**Нужно:**
1. Запустить 3 задачи параллельно (не последовательно!)
2. Собрать результаты
3. Вывести их через запятую в порядке завершения или в исходном порядке"""}),

("challenge_task", {"language": "csharp", "title": "Параллельная загрузка", "description": "Реализуй метод, который параллельно вызывает FetchAsync для имён Alpha(100ms), Beta(50ms), Gamma(200ms). Используй Task.WhenAll, собери результаты и выведи через запятую в исходном порядке (Alpha, Beta, Gamma).", "requirements": ["Task.WhenAll для параллельности", "Не await каждую задачу по очереди", "Вывести результаты в исходном порядке"], "starterCode": "async Task<string> FetchAsync(string name, int delayMs) {\n    await Task.Delay(delayMs);\n    return $\"[{name}]\";\n}\n", "expectedOutput": "[Alpha], [Beta], [Gamma]"}),

("quiz", {"question": "Если запустить три async задачи через await одну за другой (не WhenAll) — как это повлияет на время выполнения?", "options": ["Время будет суммой всех задержек — они выполняются последовательно", "Время будет равно максимальной задержке — они всё равно параллельны", "Время будет равно минимальной задержке", "Разницы нет — await всегда параллелен"], "correct": 0, "explanation": "await task1; await task2; await task3; — это последовательное ожидание. 100+50+200 = 350мс. Task.WhenAll(t1,t2,t3) — параллельное, займёт max(100,50,200) = 200мс. Разница в 1.75 раза!"}),
])

print("\n✅ Все блоки добавлены!")
