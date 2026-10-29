$BASE = "http://localhost:5003"

function Invoke-Api($method, $url, $body) {
    $json  = $body | ConvertTo-Json -Depth 10 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-RestMethod -Uri "$BASE$url" -Method $method -Body $bytes -ContentType "application/json; charset=utf-8" -ErrorAction Stop
}

function Del-Block($blockId) {
    Invoke-RestMethod -Uri "$BASE/api/builder/blocks/$blockId" -Method DELETE | Out-Null
    Write-Host "Deleted block $blockId"
}

function Add-Block($lessonId, $type, $contentJson, $order) {
    $body = @{ lessonId = $lessonId; type = $type; content = $contentJson; order = $order }
    $r = Invoke-Api "POST" "/api/builder/lessons/$lessonId/blocks" $body
    Write-Host "  Created block order=$order type=$type -> id=$($r.id)"
    return $r
}

function Add-Text($lid, $md, $order) {
    $contentJson = @{ markdown = $md } | ConvertTo-Json -Compress
    Add-Block $lid "text" $contentJson $order
}

function Add-Code($lid, $code, $order) {
    $contentJson = @{ language = "csharp"; code = $code } | ConvertTo-Json -Compress
    Add-Block $lid "code" $contentJson $order
}

# Delete bad blocks
Del-Block 92
Del-Block 93
Del-Block 98
Del-Block 99

# ──── Lesson 16: if / else ────────────────────────────────────
$lid = 16
Write-Host "Fixing lesson $lid (if/else)..."

$md = @'
## if / else — ветвление программы

Оператор **if** позволяет выполнять блоки кода при определённых условиях.

### Базовый синтаксис

```csharp
if (условие)
{
    // выполняется, если условие истинно
}
else if (другое_условие)
{
    // проверяется, если первое ложно
}
else
{
    // выполняется во всех остальных случаях
}
```

> **Best practice:** Всегда используйте фигурные скобки, даже для однострочных блоков.

### Тернарный оператор

Компактная запись для простых условий:

```csharp
тип переменная = условие ? значение_если_true : значение_если_false;
```
'@

Add-Text $lid $md 0

$code = @'
int temperature = 25;

// Базовый if-else
if (temperature > 30)
{
    Console.WriteLine("Жарко!");
}
else if (temperature < 10)
{
    Console.WriteLine("Холодно.");
}
else
{
    Console.WriteLine("Нормально.");
}

// Тернарный оператор — компактная форма
string status = temperature > 30 ? "Жарко" : "Нормально";
Console.WriteLine(status);

// Цепочка условий
if (temperature < 0)
    Console.WriteLine("Мороз!");
else if (temperature < 10)
    Console.WriteLine("Холодно.");
else if (temperature < 25)
    Console.WriteLine("Комфортно.");
else
    Console.WriteLine("Жарко!");
'@

Add-Code $lid $code 1

# ──── Lesson 17: switch ───────────────────────────────────────
$lid = 17
Write-Host "Fixing lesson $lid (switch)..."

$md = @'
## switch — выбор из нескольких вариантов

**switch** удобен, когда нужно проверить переменную на несколько значений.

### Классический switch vs switch expression (C# 8+)

| Стиль | Когда использовать |
|-------|-------------------|
| `switch { case: }` | Сложная логика, несколько операторов в case |
| `switch expression` | Присваивание результата, чистый лаконичный код |

### switch expression (рекомендуется для простых случаев)

```csharp
string result = variable switch
{
    значение1 => "результат1",
    значение2 or значение3 => "результат2",
    _ => "по умолчанию"
};
```
'@

Add-Text $lid $md 0

$code = @'
// Классический switch
int day = 3;
string dayName;
switch (day)
{
    case 1: dayName = "Понедельник"; break;
    case 2: dayName = "Вторник"; break;
    case 3:
    case 4: dayName = "Среда или Четверг"; break;
    case 5: dayName = "Пятница"; break;
    default: dayName = "Выходной"; break;
}

// Switch expression (C# 8+) — современный стиль
string dayName2 = day switch
{
    1 => "Понедельник",
    2 => "Вторник",
    3 or 4 => "Среда или Четверг",
    5 => "Пятница",
    6 or 7 => "Выходной",
    _ => "Неверный день"
};

// Switch с типами
object obj = 3.14;
string description = obj switch
{
    int i    => $"Целое число: {i}",
    double d => $"Дробное: {d}",
    string s => $"Строка: {s}",
    null     => "Null",
    _        => "Неизвестный тип"
};
Console.WriteLine(description); // Дробное: 3.14
'@

Add-Code $lid $code 1

# ──── Lesson 20: Объявление методов ──────────────────────────
$lid = 20
Write-Host "Fixing lesson $lid (methods declaration)..."

$md = @'
## Методы — блоки многократно используемого кода

**Метод** — именованный блок кода, который можно вызывать из других мест программы. Методы позволяют не повторять код и разбивать программу на логические части.

### Синтаксис

```csharp
[модификатор] возвращаемый_тип ИмяМетода([параметры])
{
    // тело
    return значение; // если не void
}
```

### Expression-bodied методы (C# 6+)

Для однострочных методов можно использовать стрелочный синтаксис:

```csharp
static int Square(int x) => x * x;
```

### Соглашения об именовании

- Методы называются в **PascalCase**: `CalculateTotal`, `GetUserName`
- Параметры — в **camelCase**: `firstName`, `totalAmount`
'@

Add-Text $lid $md 0

$code = @'
// Метод без возвращаемого значения
static void SayHello(string name)
{
    Console.WriteLine($"Hello, {name}!");
}

// Метод с возвращаемым значением
static int Add(int a, int b)
{
    return a + b;
}

// Expression-bodied (краткая запись)
static int Square(int x) => x * x;
static string Greet(string name) => $"Hello, {name}!";

// Вызов методов
SayHello("Ivan");           // Hello, Ivan!
int result = Add(3, 5);     // 8
int sq = Square(4);         // 16
string msg = Greet("Anna"); // Hello, Anna!

Console.WriteLine($"3 + 5 = {result}");
Console.WriteLine($"4^2 = {sq}");
'@

Add-Code $lid $code 1

# ──── Lesson 21: Параметры методов ───────────────────────────
$lid = 21
Write-Host "Fixing lesson $lid (parameters)..."

$md = @'
## Параметры методов

C# поддерживает несколько способов передачи параметров:

| Модификатор | Поведение |
|-------------|-----------|
| (нет) | По значению — метод получает копию |
| `ref` | По ссылке — изменяет оригинал |
| `out` | Как ref, но без инициализации до вызова |
| `in` | По ссылке только для чтения (оптимизация) |
| `params` | Переменное количество аргументов |

### Параметры по умолчанию

```csharp
static void Log(string msg, string level = "INFO") { ... }
Log("Started");         // level = "INFO"
Log("Error", "ERROR");  // level = "ERROR"
```

### Именованные аргументы

```csharp
Log(level: "DEBUG", msg: "Test"); // порядок не важен
```
'@

Add-Text $lid $md 0

$code = @'
// by value — метод получает КОПИЮ
static void TryChange(int x)
{
    x = 100; // меняем только копию
}
int num = 5;
TryChange(num);
Console.WriteLine(num); // 5 — не изменился

// ref — передаём ОРИГИНАЛ
static void DoubleIt(ref int x) { x *= 2; }
int val = 10;
DoubleIt(ref val);
Console.WriteLine(val); // 20

// out — возврат нескольких значений
static bool TryDiv(int a, int b, out double result)
{
    if (b == 0) { result = 0; return false; }
    result = (double)a / b;
    return true;
}
if (TryDiv(10, 3, out double q))
    Console.WriteLine(q); // 3.333...

// params — переменное число аргументов
static int Sum(params int[] numbers)
{
    int total = 0;
    foreach (int n in numbers) total += n;
    return total;
}
Console.WriteLine(Sum(1, 2, 3));        // 6
Console.WriteLine(Sum(10, 20, 30, 40)); // 100

// Параметры по умолчанию + именованные
static void Log(string msg, string level = "INFO", bool stamp = true)
{
    string prefix = stamp ? $"[{DateTime.Now:HH:mm}] " : "";
    Console.WriteLine($"{prefix}[{level}] {msg}");
}
Log("Server started");
Log("Error!", "ERROR");
Log("Debug info", stamp: false);
'@

Add-Code $lid $code 1

Write-Host ""
Write-Host "All bad blocks replaced successfully."
