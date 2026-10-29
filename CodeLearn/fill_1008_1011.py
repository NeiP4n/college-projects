import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 1008: ValueTask<T> ────────────────────────────────────────────────────────
print('=== 1008: ValueTask<T> ===')

post(1008, 'text', {'markdown': """## ValueTask\<T\>

`ValueTask<T>` — лёгкая альтернатива `Task<T>`, оптимизированная для случаев, когда результат **часто доступен синхронно**.

### Проблема с Task<T>
```csharp
// Task<T> всегда выделяет объект в куче — дорого при частых вызовах
async Task<int> GetCachedValue(int key)
{
    if (_cache.TryGetValue(key, out var val))
        return val; // Всё равно создаёт Task-объект!
    return await _db.GetAsync(key);
}
```

### Решение — ValueTask\<T\>
```csharp
async ValueTask<int> GetCachedValue(int key)
{
    if (_cache.TryGetValue(key, out var val))
        return val; // Никаких аллокаций при cache hit!
    return await _db.GetAsync(key);
}
```

### Важные ограничения ValueTask
- **Нельзя** ожидать несколько раз (`await vt; await vt;`)
- **Нельзя** вызывать `.Result` после `await`
- Нельзя хранить и ждать из нескольких мест

### Когда использовать
| Ситуация | Рекомендация |
|---------|-------------|
| Горячий путь, результат часто синхронный | `ValueTask<T>` |
| Общий код, несколько await | `Task<T>` |
| Публичный API библиотеки | `Task<T>` (безопаснее) |

### Синхронное завершение без async
```csharp
ValueTask<int> GetOne() => new ValueTask<int>(1); // ноль аллокаций
```"""})

post(1008, 'code', {'language': 'csharp', 'title': 'ValueTask vs Task — кэш-пример', 'code': """using System.Collections.Generic;

class DataService
{
    private Dictionary<string, string> _cache = new();

    // ValueTask: если в кэше — нет аллокации Task
    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached; // синхронный путь — без Task-объекта

        // Имитация асинхронного запроса
        await Task.Delay(10);
        string value = $"data:{key}";
        _cache[key] = value;
        return value;
    }
}

var svc = new DataService();

// Первый запрос — async
string v1 = await svc.GetAsync("user:1");
Console.WriteLine(v1); // data:user:1

// Второй запрос — из кэша, синхронно (нет аллокации)
string v2 = await svc.GetAsync("user:1");
Console.WriteLine(v2); // data:user:1"""})

post(1008, 'quiz', {'question': 'В каком случае ValueTask<T> даёт преимущество перед Task<T>?', 'options': [
    'Когда метод всегда выполняется асинхронно',
    'Когда результат часто доступен синхронно и важно избежать лишних аллокаций',
    'Когда нужно ожидать результат из нескольких мест',
    'При работе с файловой системой'
], 'correct': 1, 'explanation': 'ValueTask<T> экономит аллокации в куче, когда метод часто завершается синхронно (например, cache hit). При всегда-асинхронном пути выигрыша нет — там лучше Task<T>.'})

post(1008, 'control_question', {'question': 'Назови два ключевых ограничения ValueTask<T> по сравнению с Task<T>.', 'answer': '1) ValueTask нельзя ожидать несколько раз — только один await. 2) Нельзя хранить и использовать из нескольких точек кода одновременно. Task<T> более безопасен для общих сценариев.'})

# ── 1009: IAsyncEnumerable<T> и async streams ─────────────────────────────────
print('=== 1009: IAsyncEnumerable<T> и async streams ===')

post(1009, 'text', {'markdown': """## IAsyncEnumerable\<T\> и async streams

`IAsyncEnumerable<T>` (C# 8+) позволяет создавать **асинхронные потоки данных** — как yield return, но с await внутри.

### Проблема без async streams
```csharp
// Приходится ждать ВСЕХ данных перед обработкой
async Task<List<string>> GetAllLinesAsync(string path)
{
    var lines = new List<string>();
    // ... читаем всё, потом возвращаем
    return lines;
}
```

### Async stream с IAsyncEnumerable
```csharp
async IAsyncEnumerable<string> ReadLinesAsync(string path)
{
    using var reader = new StreamReader(path);
    while (!reader.EndOfStream)
    {
        yield return await reader.ReadLineAsync(); // каждая строка — отдельный await
    }
}

// Потребление — await foreach
await foreach (var line in ReadLinesAsync("data.txt"))
{
    Console.WriteLine(line); // обрабатываем строку СРАЗУ, не дожидаясь остальных
}
```

### Отмена через CancellationToken
```csharp
async IAsyncEnumerable<int> CountAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    for (int i = 0; !ct.IsCancellationRequested; i++)
    {
        await Task.Delay(100, ct);
        yield return i;
    }
}
```

### Когда использовать
- Чтение больших файлов построчно
- Пагинация API (страница за страницей)
- Обработка данных в реальном времени (IoT, WebSocket)""" })

post(1009, 'code', {'language': 'csharp', 'title': 'Генератор данных с async stream', 'code': """static async IAsyncEnumerable<int> GenerateAsync(int count)
{
    for (int i = 1; i <= count; i++)
    {
        await Task.Delay(10); // имитация асинхронной работы
        yield return i * i;   // квадраты чисел
    }
}

int sum = 0;
await foreach (var value in GenerateAsync(5))
{
    Console.WriteLine($"Получено: {value}");
    sum += value;
}
Console.WriteLine($"Сумма: {sum}");
// Получено: 1
// Получено: 4
// Получено: 9
// Получено: 16
// Получено: 25
// Сумма: 55"""})

post(1009, 'quiz', {'question': 'Чем await foreach отличается от обычного foreach при работе с IAsyncEnumerable?', 'options': [
    'await foreach ждёт все элементы перед началом итерации',
    'await foreach получает каждый элемент асинхронно, не блокируя поток между итерациями',
    'await foreach работает только с Task<IEnumerable<T>>',
    'await foreach автоматически распараллеливает обработку элементов'
], 'correct': 1, 'explanation': 'await foreach получает следующий элемент асинхронно через MoveNextAsync(), не блокируя поток в ожидании. Это позволяет обрабатывать элементы по мере их появления, не загружая всё в память.'})

post(1009, 'control_question', {'question': 'В чём преимущество IAsyncEnumerable<T> перед Task<List<T>> при чтении большого файла?', 'answer': 'IAsyncEnumerable<T> возвращает элементы по одному по мере готовности — не нужно ждать загрузки всего файла и держать его в памяти. Task<List<T>> требует загрузить всё, потом вернуть. Async stream позволяет обрабатывать данные с минимальным использованием памяти и начинать работу немедленно.'})

# ── 1010: Reflection и атрибуты ───────────────────────────────────────────────
print('=== 1010: Reflection и атрибуты ===')

post(1010, 'text', {'markdown': """## Reflection и атрибуты

### Reflection — изучение типов во время выполнения
```csharp
Type t = typeof(string);
Console.WriteLine(t.Name);          // String
Console.WriteLine(t.Namespace);     // System

// Методы типа
foreach (var m in t.GetMethods())
    Console.WriteLine(m.Name);
```

### Создание экземпляра через Reflection
```csharp
Type t = Type.GetType("System.Text.StringBuilder");
object sb = Activator.CreateInstance(t);
t.GetMethod("Append", new[] { typeof(string) })
 .Invoke(sb, new object[] { "hello" });
```

### Атрибуты — метаданные для кода
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
class AuthorAttribute : Attribute
{
    public string Name { get; }
    public AuthorAttribute(string name) => Name = name;
}

[Author("Alice")]
class MyService { }
```

### Чтение атрибутов через Reflection
```csharp
var attr = typeof(MyService).GetCustomAttribute<AuthorAttribute>();
Console.WriteLine(attr?.Name); // Alice
```

### Практические применения
- Dependency Injection контейнеры (ASP.NET Core)
- ORM (Entity Framework — маппинг по атрибутам)
- Сериализация (System.Text.Json)
- Тестовые фреймворки ([Test], [Fact])
- Роутинг ([HttpGet], [Route])""" })

post(1010, 'code', {'language': 'csharp', 'title': 'Кастомный атрибут и его чтение', 'code': """using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property)]
class RequiredAttribute : Attribute
{
    public string Message { get; }
    public RequiredAttribute(string message = "Поле обязательно") => Message = message;
}

class UserForm
{
    [Required("Имя не может быть пустым")]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    public string Phone { get; set; } // без атрибута
}

// Валидатор через Reflection
static void Validate(object obj)
{
    foreach (var prop in obj.GetType().GetProperties())
    {
        var attr = prop.GetCustomAttribute<RequiredAttribute>();
        if (attr != null)
        {
            var value = prop.GetValue(obj) as string;
            if (string.IsNullOrWhiteSpace(value))
                Console.WriteLine($"[!] {prop.Name}: {attr.Message}");
            else
                Console.WriteLine($"[ok] {prop.Name} = {value}");
        }
    }
}

var form = new UserForm { Name = "Alice" }; // Email не задан
Validate(form);"""})

post(1010, 'quiz', {'question': 'Что позволяет делать Reflection в C#?', 'options': [
    'Компилировать код быстрее за счёт кэширования',
    'Исследовать и динамически использовать типы, методы и атрибуты во время выполнения',
    'Автоматически генерировать комментарии к методам',
    'Синхронизировать потоки без блокировок'
], 'correct': 1, 'explanation': 'Reflection позволяет во время выполнения получать информацию о типах (Type), их методах, свойствах, атрибутах, и динамически создавать экземпляры и вызывать методы. Используется в DI, ORM, сериализации.'})

post(1010, 'control_question', {'question': 'Назови три реальных применения Reflection и атрибутов в .NET экосистеме.', 'answer': '1) Dependency Injection — ASP.NET Core сканирует типы для регистрации сервисов. 2) Entity Framework — читает атрибуты [Key], [Column] для маппинга на БД. 3) JSON-сериализация — System.Text.Json использует атрибуты [JsonPropertyName] и читает свойства через Reflection.'})

# ── 1011: Source Generators ───────────────────────────────────────────────────
print('=== 1011: Source Generators ===')

post(1011, 'text', {'markdown': """## Source Generators

Source Generators (C# 9+) — компилятор **генерирует C# код** на этапе компиляции, без Reflection во время выполнения.

### Зачем нужны?
- **Производительность**: код генерируется статически — нет Reflection overhead
- **AOT-совместимость**: работает в NativeAOT/MAUI без динамической кодогенерации
- **Встроенные в SDK**: многие системные API уже используют их

### Примеры готовых Source Generators в .NET

#### System.Text.Json — без Reflection
```csharp
[JsonSerializable(typeof(User))]
partial class UserContext : JsonSerializerContext { }

// Теперь сериализация без Reflection, только сгенерированный код
var json = JsonSerializer.Serialize(user, UserContext.Default.User);
```

#### LoggerMessage — оптимизированное логирование
```csharp
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} logged in")]
    public static partial void UserLoggedIn(ILogger logger, int userId);
}
```

#### Regex — компилируемые регулярки
```csharp
[GeneratedRegex(@"\d{4}-\d{2}-\d{2}")]
private static partial Regex DatePattern();
```

### Как работают Source Generators
1. Компилятор вызывает ваш ISourceGenerator
2. Генератор анализирует синтаксис/семантику кода
3. Добавляет новые .cs файлы в компиляцию
4. Итоговый код — обычный статический C#

### Когда применять
- Высокопроизводительная сериализация
- Автогенерация boilerplate (NotifyPropertyChanged)
- Валидация на этапе компиляции""" })

post(1011, 'code', {'language': 'csharp', 'title': 'Пример с LoggerMessage Source Generator', 'code': """using Microsoft.Extensions.Logging;

// Source Generator создаёт оптимизированный метод LogUserAction
public static partial class AppLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Пользователь {UserId} выполнил {Action}")]
    public static partial void UserAction(ILogger logger, int userId, string action);
}

// Использование — без Reflection, всё сгенерировано статически
// var logger = loggerFactory.CreateLogger("App");
// AppLog.UserAction(logger, 42, "login");

// Пример с [GeneratedRegex] — компилируемая регулярка
using System.Text.RegularExpressions;

partial class Validator
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex DateRegex();

    public static bool IsValidDate(string s) => DateRegex().IsMatch(s);
}

Console.WriteLine(Validator.IsValidDate("2024-01-15")); // True
Console.WriteLine(Validator.IsValidDate("не дата"));    // False"""})

post(1011, 'quiz', {'question': 'В чём главное преимущество Source Generators перед Reflection?', 'options': [
    'Source Generators работают только в .NET 6+',
    'Код генерируется при компиляции — нет накладных расходов Reflection во время выполнения',
    'Source Generators позволяют изменять код во время выполнения',
    'Reflection медленнее из-за работы с потоками'
], 'correct': 1, 'explanation': 'Source Generators создают обычный статический C# код на этапе компиляции. Reflection же исследует типы во время выполнения с накладными расходами. Source Generators дают производительность ручного кода, но с автогенерацией.'})

post(1011, 'control_question', {'question': 'Назови два встроенных в .NET Source Generator-а и какую задачу они решают.', 'answer': '1) [JsonSerializable] в System.Text.Json — генерирует быструю сериализацию без Reflection, совместимую с AOT. 2) [LoggerMessage] — генерирует оптимизированный код логирования без boxing параметров и форматирования строки при отключённом уровне лога.'})

print('Done!')
