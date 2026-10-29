
add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCerts : ICertificatePolicy {
    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) { return true; }
}
"@ -ErrorAction SilentlyContinue
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCerts -ErrorAction SilentlyContinue
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$BASE = "https://localhost:7187"

function Invoke-Api($method, $url, $body) {
    if ($body) {
        $json  = $body | ConvertTo-Json -Depth 10 -Compress
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
        return Invoke-RestMethod -Uri "$BASE$url" -Method $method -Body $bytes -ContentType "application/json; charset=utf-8"
    }
    return Invoke-RestMethod -Uri "$BASE$url" -Method $method
}
function POST($url, $body) { return Invoke-Api "POST" $url $body }
function PUT($url, $body)  { return Invoke-Api "PUT"  $url $body }
function DEL($url)         { try { Invoke-Api "DELETE" $url $null | Out-Null } catch {} }
function bc($obj)          { return $obj | ConvertTo-Json -Depth 10 -Compress }

# ══════════════════════════════════════════════════════
# ИСПРАВЛЯЕМ НАЗВАНИЯ МОДУЛЕЙ
# ══════════════════════════════════════════════════════
PUT "/api/builder/modules/1" @{
    title="Введение"; description="Что такое C# и .NET, как настроить окружение и запустить первый проект"
    isLocked=$false; mapX=37.7; mapY=76.4; color="#4488ff"; labelFontSize=12
} | Out-Null
Write-Host "OK: модуль 1 Введение"

PUT "/api/builder/modules/4" @{
    title="Как работает .NET"; description="Компиляция, IL, CLR, JIT, AOT — что происходит с кодом до выполнения"
    isLocked=$false; mapX=48.0; mapY=65.0; color="#ff9900"; labelFontSize=11
} | Out-Null
Write-Host "OK: модуль 4 Как работает .NET"

PUT "/api/builder/modules/5" @{
    title="Основы C#"; description="Переменные, типы данных, операторы, строки — фундамент языка"
    isLocked=$false; mapX=58.0; mapY=54.0; color="#00ff88"; labelFontSize=11
} | Out-Null
Write-Host "OK: модуль 5 Основы C#"

# ══════════════════════════════════════════════════════
# УДАЛЯЕМ ВСЕ СТАРЫЕ БЛОКИ
# ══════════════════════════════════════════════════════
foreach ($modId in @(1, 4, 5)) {
    $lessons = Invoke-Api "GET" "/api/builder/modules/$modId/lessons" $null
    foreach ($lesson in $lessons) {
        $blocks = Invoke-Api "GET" "/api/builder/lessons/$($lesson.id)/blocks" $null
        foreach ($block in $blocks) { DEL "/api/builder/blocks/$($block.id)" }
    }
}
Write-Host "OK: все блоки очищены"

# ══════════════════════════════════════════════════════
# МОДУЛЬ 1 (Введение) — уроки 1,2,4,5
# ══════════════════════════════════════════════════════

# Урок 1: Что такое C# и .NET (lesson id=1)
POST "/api/builder/lessons/1/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Что такое .NET и C#?

**.NET** — платформа от Microsoft. Это не язык — это **экосистема**: runtime, библиотеки, инструменты.

**C#** — язык программирования, который работает поверх .NET.

```
[ Ваш C# код ]
       ↓
[ .NET BCL — коллекции, IO, сеть, строки ]
       ↓
[ .NET Runtime (CLR) — GC, JIT, исключения ]
       ↓
[ Операционная система ]
```

### История версий

| Версия | Год | Ключевое |
|--------|-----|----------|
| .NET Framework 1.0 | 2002 | Только Windows |
| .NET Core 1.0 | 2016 | Кроссплатформенность |
| .NET 5 | 2020 | Объединение Core и Framework |
| .NET 6 | 2021 | LTS, Minimal API |
| .NET 8 | 2023 | LTS, Native AOT — используем сегодня |

> **Важно:** Используем **.NET 8 LTS**. .NET Framework — legacy, не путай." })
} | Out-Null

POST "/api/builder/lessons/1/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Современный стиль (C# 9+, top-level statements)
Console.WriteLine("Hello, World!");

// Классический стиль
namespace MyFirstApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}' })
} | Out-Null
Write-Host "OK: урок 1 (Что такое C# и .NET)"

# Урок 2: Установка окружения (lesson id=2)
POST "/api/builder/lessons/2/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Установка окружения

### Шаг 1: Установить .NET SDK
Скачай с **https://dotnet.microsoft.com/download**. Выбирай **.NET 8 (LTS)**.

### Шаг 2: IDE

| IDE | Описание | Цена |
|-----|----------|------|
| **Visual Studio 2022** | Полная IDE для .NET (Windows) | Бесплатно (Community) |
| **JetBrains Rider** | Кроссплатформенная | Платная |
| **VS Code + C# Dev Kit** | Лёгкая, кроссплатформенная | Бесплатно |

### Структура проекта после создания

    MyFirstApp/
    ├── MyFirstApp.csproj    <- файл проекта (XML)
    ├── Program.cs           <- точка входа
    └── obj/                 <- временные файлы компиляции" })
} | Out-Null

POST "/api/builder/lessons/2/blocks" @{
    type="code"; order=1
    content=(bc @{ language="bash"; code='# Проверить версию .NET
dotnet --version

# Список установленных SDK
dotnet --list-sdks

# Создать первый проект
dotnet new console -n MyFirstApp
cd MyFirstApp
dotnet run' })
} | Out-Null
Write-Host "OK: урок 2 (Установка окружения)"

# Урок 3: Команды dotnet CLI (lesson id=4)
POST "/api/builder/lessons/4/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Команды dotnet CLI

**dotnet** — основной инструмент для работы с .NET проектами. Все операции делаются через него в терминале.

Запускается как: **dotnet [команда] [аргументы]**" })
} | Out-Null

POST "/api/builder/lessons/4/blocks" @{
    type="code"; order=1
    content=(bc @{ language="bash"; code='dotnet new console        # новый консольный проект
dotnet new webapi         # новый Web API проект
dotnet new classlib       # новая библиотека классов

dotnet build              # собрать проект
dotnet run                # запустить проект
dotnet test               # запустить тесты
dotnet publish            # опубликовать для деплоя

dotnet add package Newtonsoft.Json   # добавить NuGet пакет
dotnet remove package Newtonsoft.Json
dotnet list package                  # список пакетов

dotnet restore            # восстановить зависимости
dotnet clean              # очистить артефакты сборки' })
} | Out-Null
Write-Host "OK: урок 3 (Команды dotnet CLI)"

# Урок 4: NuGet (lesson id=5)
POST "/api/builder/lessons/5/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## NuGet — менеджер пакетов .NET

**NuGet** — аналог npm для Node.js или pip для Python, но для .NET. Пакеты публикуются на **nuget.org**.

### Популярные пакеты

| Пакет | Назначение |
|-------|-----------|
| Newtonsoft.Json | Работа с JSON |
| Serilog | Логирование |
| Dapper | Лёгкий ORM |
| Microsoft.EntityFrameworkCore | Полный ORM |
| FluentValidation | Валидация данных |
| AutoMapper | Маппинг объектов |" })
} | Out-Null

POST "/api/builder/lessons/5/blocks" @{
    type="code"; order=1
    content=(bc @{ language="bash"; code='# Добавить пакет
dotnet add package Serilog

# Конкретная версия
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0

# Удалить пакет
dotnet remove package Serilog' })
} | Out-Null

POST "/api/builder/lessons/5/blocks" @{
    type="code"; order=2
    content=(bc @{ language="xml"; code='<!-- В .csproj появится автоматически: -->
<ItemGroup>
  <PackageReference Include="Serilog" Version="3.1.1" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
</ItemGroup>' })
} | Out-Null
Write-Host "OK: урок 4 (NuGet)"

# ══════════════════════════════════════════════════════
# МОДУЛЬ 4 (Как работает .NET) — уроки 6,7,8,9
# ══════════════════════════════════════════════════════

# Урок: Компиляция и IL (lesson id=6)
POST "/api/builder/lessons/6/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Как компилируется C# код?

Компьютер понимает только машинный код. C# — высокоуровневый язык. Между ними несколько слоёв:

    C# Source Code (.cs)
           |  C# Compiler (Roslyn)
           v
    IL / CIL (Intermediate Language) - .dll или .exe
           |  JIT Compiler (при первом запуске)
           v
    Native Machine Code (x86/x64/ARM)

**IL (Intermediate Language)** — промежуточный язык, общий для всех .NET языков (C#, F#, VB.NET). Именно он хранится в .dll файле, а не машинный код." })
} | Out-Null

POST "/api/builder/lessons/6/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Наш C# код:
Console.WriteLine("Hello, World!");

// После компиляции Roslyn превращает это в IL:
// .method private hidebysig static void Main() cil managed
// {
//   .entrypoint
//   .maxstack 8
//   IL_0000: ldstr "Hello, World!"
//   IL_0005: call void [System.Console]System.Console::WriteLine(string)
//   IL_000a: ret
// }' })
} | Out-Null
Write-Host "OK: урок Компиляция и IL"

# Урок: CLR (lesson id=7)
POST "/api/builder/lessons/7/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## CLR — Common Language Runtime

**CLR** — это движок (виртуальная машина) .NET. Он управляет выполнением твоего кода.

### Что делает CLR

| Компонент | Ответственность |
|-----------|----------------|
| **JIT Compiler** | Компилирует IL в машинный код при запуске |
| **Garbage Collector** | Автоматически освобождает неиспользуемую память |
| **Exception Handling** | Механизм try/catch/finally |
| **Type System** | Единая система типов для всех .NET языков |
| **Thread Management** | Управление потоками |

### Managed vs Unmanaged Code

| Managed Code | Unmanaged Code |
|-------------|----------------|
| Под управлением CLR | Вне CLR (C, C++) |
| Автоматическая память | Ручная память |
| C#, F#, VB.NET | Win32 API, нативные библиотеки |" })
} | Out-Null
Write-Host "OK: урок CLR"

# Урок: JIT и AOT (lesson id=8)
POST "/api/builder/lessons/8/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## JIT vs AOT компиляция

### JIT (Just-In-Time) — по умолчанию
- IL компилируется в машинный код **во время выполнения**
- Первый запуск чуть медленнее (cold start)
- Оптимизирует под конкретное железо
- Tiered compilation: сначала быстро, потом переоптимизирует горячие методы

### AOT (Ahead-Of-Time) — .NET 7+
- Компилируется в машинный код **заранее**, до запуска
- Очень быстрый старт
- Меньший размер
- Используется для микросервисов, мобильных приложений" })
} | Out-Null

POST "/api/builder/lessons/8/blocks" @{
    type="code"; order=1
    content=(bc @{ language="bash"; code='# Обычная публикация (JIT)
dotnet publish -c Release

# Native AOT — компилируется полностью заранее, быстрый старт
dotnet publish -r linux-x64 -p:PublishAot=true

# Self-contained — включает runtime, не нужен .NET на целевой машине
dotnet publish -r win-x64 --self-contained true' })
} | Out-Null
Write-Host "OK: урок JIT и AOT"

# Урок: Сборки и namespace (lesson id=9)
POST "/api/builder/lessons/9/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Сборки (Assemblies) и пространства имён

**Assembly** — скомпилированный .NET код в файле .dll или .exe.

    MyApp/
    ├── MyApp.exe              <- исполняемая сборка
    ├── MyLibrary.dll          <- библиотечная сборка
    └── Newtonsoft.Json.dll    <- сторонняя зависимость

**Namespace** — логическая организация классов внутри сборок. Как папки для файлов.

Соглашение об именах: **Компания.Проект.Слой**" })
} | Out-Null

POST "/api/builder/lessons/9/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Объявление namespace
namespace MyCompany.MyApp.Services
{
    public class UserService
    {
        // логика сервиса
    }
}

// Использование
using MyCompany.MyApp.Services;
var service = new UserService();

// Global using (C# 10+) — в файле GlobalUsings.cs
global using System.Collections.Generic;
global using MyCompany.MyApp.Services;' })
} | Out-Null
Write-Host "OK: урок Сборки и namespace"

# ══════════════════════════════════════════════════════
# МОДУЛЬ 5 (Основы C#) — уроки 10..15
# ══════════════════════════════════════════════════════

# Урок: Переменные (id=10)
POST "/api/builder/lessons/10/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Переменные и константы

**Переменная** — именованная область памяти для хранения данных.

Синтаксис: **тип имя = значение;**

### var — вывод типа
Компилятор сам определяет тип по значению. Тип фиксируется при компиляции — это не динамическая типизация!

### Константы
- **const** — значение известно на этапе компиляции, никогда не меняется
- **readonly** — значение задаётся один раз (в конструкторе или при объявлении)" })
} | Out-Null

POST "/api/builder/lessons/10/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Объявление с явным типом
int age = 25;
string name = "Ivan";
bool isActive = true;

// var — компилятор определяет тип
var score = 100;        // int
var greeting = "Hi";   // string
var price = 9.99m;      // decimal

// Константы — значение задаётся один раз
const double PI = 3.14159265358979;
const string APP_NAME = "CodeLearn";

// Несколько переменных одного типа
int x = 1, y = 2, z = 3;' })
} | Out-Null
Write-Host "OK: урок Переменные"

# Урок: Типы данных (id=11)
POST "/api/builder/lessons/11/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Типы данных C#

### Value Types (хранятся в стеке)

| Тип | .NET тип | Размер | Диапазон / Применение |
|-----|----------|--------|-----------------------|
| bool | Boolean | 1 байт | true / false |
| int | Int32 | 4 байта | целые числа, ~2 млрд |
| long | Int64 | 8 байт | большие целые числа |
| double | Double | 8 байт | дробные числа, 15-16 знаков |
| decimal | Decimal | 16 байт | финансы — без ошибок округления! |
| char | Char | 2 байта | один символ Unicode |
| float | Single | 4 байта | дробные числа, 7 знаков |

### Reference Types (хранятся в куче)
- **string** — строка (неизменяемая)
- **object** — базовый тип всех типов в .NET
- массивы, классы, интерфейсы

> Для денег всегда **decimal** — float и double имеют ошибки округления!" })
} | Out-Null

POST "/api/builder/lessons/11/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Числовые литералы
int million  = 1_000_000;      // _ для читаемости
double pi    = 3.14;
float f      = 3.14f;          // суффикс f
decimal money = 99.99m;        // суффикс m — всегда для денег!
long big     = 9_000_000_000L; // суффикс L

// Шестнадцатеричные и бинарные литералы
int hex = 0xFF;           // = 255
int bin = 0b1111_1111;   // = 255

// char и string
char letter = (char)65;  // A
string text  = "Hello";' })
} | Out-Null
Write-Host "OK: урок Типы данных"

# Урок: Nullable (id=12)
POST "/api/builder/lessons/12/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Nullable типы

**Проблема:** value types (int, bool, etc.) не могут быть null. Но в базе данных поле может отсутствовать.

**Решение:** добавь ? к типу. int? — это сокращение для Nullable<int>.

### Операторы для работы с null

| Оператор | Название | Что делает |
|----------|----------|-----------|
| ?? | null-coalescing | если null — вернуть правую часть |
| ??= | null-coalescing assignment | присвоить только если null |
| ?. | null-conditional | вызвать только если не null |
| ! | null-forgiving | сказать компилятору 'точно не null' |" })
} | Out-Null

POST "/api/builder/lessons/12/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Nullable value types
int? age = null;
bool? isActive = null;

// Проверка наличия значения
if (age.HasValue)
    Console.WriteLine(age.Value);

// ?? — вернуть значение по умолчанию если null
int result = age ?? 0;

// ??= — присвоить только если null
age ??= 18;

// ?. — null-conditional, не выбросит исключение
string? name = null;
int? length = name?.Length;   // null, а не NullReferenceException

// Цепочка
string? city = user?.Address?.City;

// Nullable reference types (C# 8+, включить в .csproj)
// <Nullable>enable</Nullable>
string notNull  = "Ivan";    // компилятор предупредит если присвоишь null
string? canNull = null;      // явно разрешаем null' })
} | Out-Null
Write-Host "OK: урок Nullable"

# Урок: Операторы (id=13)
POST "/api/builder/lessons/13/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Операторы C#

### Арифметические
- Операторы: **+ - * / %** (остаток от деления)
- Целочисленное деление: 10 / 3 = 3 (не 3.33). Для дробного результата используй double или decimal.

### Инкремент и декремент
- **a++** — постфикс: сначала использовать значение, потом увеличить
- **++a** — префикс: сначала увеличить, потом использовать

### Составное присваивание
**a += 5** равно **a = a + 5**. Аналогично для -=, *=, /=, %=

### Тернарный оператор
**условие ? если_true : если_false** — короткая замена if/else для простых случаев." })
} | Out-Null

POST "/api/builder/lessons/13/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='int a = 10, b = 3;

// Арифметика
int sum  = a + b;   // 13
int diff = a - b;   // 7
int mul  = a * b;   // 30
int div  = a / b;   // 3  (целочисленное деление!)
int mod  = a % b;   // 1  (остаток)

// Инкремент / декремент
a++;          // постфикс
++a;          // префикс
a += 5;       // составное присваивание

// Сравнения
bool eq  = a == b;   // равно
bool neq = a != b;   // не равно
bool gt  = a > b;    // больше

// Логические
bool and = true && false;   // И (ленивое)
bool or  = true || false;   // ИЛИ (ленивое)
bool not = !true;            // НЕ

// Тернарный оператор
int age = 20;
string status = age >= 18 ? "Взрослый" : "Ребёнок";' })
} | Out-Null
Write-Host "OK: урок Операторы"

# Урок: Строки (id=14)
POST "/api/builder/lessons/14/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Строки в C#

**string** — неизменяемый ссылочный тип. Каждое изменение создаёт **новый** объект в памяти.

### Способы создания строк

| Синтаксис | Когда использовать |
|-----------|-------------------|
| Обычные кавычки | Всегда по умолчанию |
| Знак доллара + кавычки | Интерполяция — вставить значения переменных |
| @ + кавычки | Пути к файлам, многострочный текст без экранирования |

### StringBuilder
Когда собираешь строку из многих частей в цикле — используй **StringBuilder**. Иначе каждый + создаёт новый объект на куче." })
} | Out-Null

POST "/api/builder/lessons/14/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='string name = "World";

// String Interpolation — предпочтительный способ
string msg  = $"Hello, {name}!";
string calc = $"2 + 2 = {2 + 2}";
string fmt  = $"Pi = {Math.PI:F2}";   // форматирование внутри {}

// Verbatim @ — без экранирования обратных слешей
string path  = @"C:\Users\Ivan\Documents";
string multi = @"Строка 1
Строка 2";

// Методы строк
string s = "  Hello, World!  ";
s.ToUpper();                 // "  HELLO, WORLD!  "
s.Trim();                    // "Hello, World!"
s.Replace("World", "C#");   // "  Hello, C#!  "
s.Contains("Hello");         // true
s.Split(", ");               // [ "  Hello", "World!  " ]
s.Length;                    // 17

// StringBuilder — для сборки строк в цикле
var sb = new System.Text.StringBuilder();
for (int i = 0; i < 5; i++)
    sb.Append($"item{i} ");
string result = sb.ToString();' })
} | Out-Null
Write-Host "OK: урок Строки"

# Урок: Приведение типов и Boxing (id=15)
POST "/api/builder/lessons/15/blocks" @{
    type="text"; order=0
    content=(bc @{ markdown="## Приведение типов

### Неявное (implicit) — безопасное
Компилятор делает сам, когда нет риска потери данных (расширение).
int -> long -> double

### Явное (explicit) — с возможной потерей данных
Нужно писать вручную — ты берёшь ответственность.
(int)3.99 -> 3 (дробная часть теряется!)

### Безопасные методы
- **int.TryParse()** — возвращает bool, не бросает исключение
- **as** — возвращает null если не получилось (только для классов)
- **is** — проверка типа + pattern matching

## Boxing и Unboxing

**Boxing** — упаковка value type в object (копия попадает на кучу). **Медленно!**

**Unboxing** — обратная операция.

> **На собеседовании:** Boxing создаёт давление на GC, избегай в горячих путях. **Generics** (List<int>) полностью избегают boxing." })
} | Out-Null

POST "/api/builder/lessons/15/blocks" @{
    type="code"; order=1
    content=(bc @{ language="csharp"; code='// Неявное — безопасное расширение
int i    = 42;
long l   = i;     // int -> long
double d = i;     // int -> double

// Явное — пишем вручную, берём ответственность
double pi      = 3.99;
int rounded    = (int)pi;   // 3 (дробь теряется!)

// Безопасный парсинг — не бросает исключение
bool ok  = int.TryParse("123abc", out int num);  // ok=false
int n    = int.Parse("42");           // бросает если ошибка
int n2   = Convert.ToInt32("42");     // то же самое

// as и is — для классов
object obj = "Hello";
string? s  = obj as string;   // "Hello"
object num3 = 42;
string? s2 = num3 as string;  // null (без исключения)

if (obj is string str)         // pattern matching
    Console.WriteLine(str.Length);

// Boxing / Unboxing — МЕДЛЕННО
object boxed   = 42;           // boxing: int -> object на куче
int unboxed    = (int)boxed;   // unboxing

// ПРАВИЛЬНО — generics, нет boxing
List<int> list = new List<int>();
list.Add(42);  // нет boxing!' })
} | Out-Null
Write-Host "OK: урок Приведение типов и Boxing"

Write-Host ""
Write-Host "==========================================="
Write-Host "  ВСЕ БЛОКИ СОЗДАНЫ С ПРАВИЛЬНОЙ UTF-8"
Write-Host "  Модуль 1: Введение (4 урока)"
Write-Host "  Модуль 4: Как работает .NET (4 урока)"
Write-Host "  Модуль 5: Основы C# (6 уроков)"
Write-Host "==========================================="
