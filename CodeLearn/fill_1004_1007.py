import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 1004: Предикаты и Comparison<T> ──────────────────────────────────────────
print('=== 1004: Предикаты и Comparison<T> ===')

post(1004, 'text', {'markdown': """## Предикаты и Comparison\<T\>

### Predicate\<T\>
`Predicate<T>` — делегат, принимающий T и возвращающий `bool`. Используется в методах `List<T>`.

```csharp
List<int> numbers = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

Predicate<int> isEven = n => n % 2 == 0;

var evens = numbers.FindAll(isEven);     // [2, 4, 6, 8, 10]
int first = numbers.Find(isEven);        // 2
bool any = numbers.Exists(isEven);       // true
```

### Comparison\<T\>
`Comparison<T>` — делегат для сортировки: принимает два T, возвращает int (отрицательный/ноль/положительный).

```csharp
List<string> words = new() { "banana", "apple", "cherry", "date" };

// Сортировка по длине строки
words.Sort((a, b) => a.Length.CompareTo(b.Length));
// ["date", "apple", "banana", "cherry"]
```

### Отличие от Func\<T, bool\>
| | `Predicate<T>` | `Func<T, bool>` |
|---|---|---|
| Сигнатура | `bool(T)` | `bool(T)` |
| Использование | `List<T>.Find`, `FindAll`, `Exists` | LINQ `.Where()`, общие делегаты |
| Взаимозаменяемы | Не напрямую — разные типы делегатов |

### Удобный паттерн — фабрика предикатов
```csharp
static Predicate<int> InRange(int min, int max) => n => n >= min && n <= max;

var mid = numbers.FindAll(InRange(3, 7)); // [3, 4, 5, 6, 7]
```"""})

post(1004, 'code', {'language': 'csharp', 'title': 'Predicate и Comparison в действии', 'code': """List<string> names = new() { "Alice", "Bob", "Charlie", "Dan", "Eve" };

// Predicate: найти имена длиннее 3 символов
Predicate<string> longName = s => s.Length > 3;
var long_ = names.FindAll(longName);
Console.WriteLine(string.Join(", ", long_)); // Alice, Charlie

// Comparison: сортировка по убыванию длины
names.Sort((a, b) => b.Length.CompareTo(a.Length));
Console.WriteLine(string.Join(", ", names)); // Charlie, Alice, Eve, Bob, Dan

// Exists и Find
bool hasShort = names.Exists(s => s.Length <= 3);
Console.WriteLine(hasShort); // True

string first = names.Find(s => s.StartsWith("A"));
Console.WriteLine(first); // Alice"""})

post(1004, 'quiz', {'question': 'Что возвращает Comparison<T>, если первый аргумент "меньше" второго?', 'options': [
    'true',
    'Отрицательное число',
    '0',
    'Положительное число'
], 'correct': 1, 'explanation': 'Comparison<T> возвращает int: отрицательный если a < b, 0 если равны, положительный если a > b. Это стандартный контракт для сортировки.'})

post(1004, 'task', {'title': 'Фильтр по предикату', 'description': 'Создай список чисел от 1 до 10. Используй FindAll с Predicate для поиска чисел кратных 3. Выведи через пробел.', 'starterCode': """List<int> nums = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

Predicate<int> divisibleBy3 = n => n % 3 == 0;
var result = nums.FindAll(divisibleBy3);

Console.WriteLine(string.Join(" ", result));
""", 'expectedOutput': '3 6 9', 'language': 'csharp'})

# ── 1005: Weak-события и утечки памяти ───────────────────────────────────────
print('=== 1005: Weak-события и утечки памяти ===')

post(1005, 'text', {'markdown': """## Weak-события и утечки памяти через события

### Проблема: события удерживают подписчиков в памяти

```csharp
class Publisher
{
    public event EventHandler DataChanged;
}

class Subscriber
{
    public Subscriber(Publisher pub)
    {
        pub.DataChanged += OnChanged; // Publisher держит ссылку на Subscriber!
    }
    void OnChanged(object s, EventArgs e) { }
}

// Если Subscriber "умер", но Publisher жив — Subscriber НЕ будет собран GC
var pub = new Publisher();
var sub = new Subscriber(pub);
sub = null; // sub не удалится — Publisher держит его!
```

### Решение 1: явная отписка
```csharp
// В Subscriber реализуй IDisposable
public void Dispose()
{
    _publisher.DataChanged -= OnChanged; // освобождаем ссылку
}
```

### Решение 2: WeakReference\<T\>
```csharp
class WeakSubscriber
{
    private WeakReference<Action> _handler;

    public WeakSubscriber(Action handler)
    {
        _handler = new WeakReference<Action>(handler);
    }

    public void Invoke()
    {
        if (_handler.TryGetTarget(out var h))
            h();
        // иначе объект уже собран GC
    }
}
```

### Правило
> Если ваш класс подписывается на события внешних объектов — **всегда отписывайтесь** в `Dispose()` или деструкторе."""})

post(1005, 'code', {'language': 'csharp', 'title': 'WeakReference для безопасной подписки', 'code': """class EventSource
{
    public event Action Fired;
    public void Fire() => Fired?.Invoke();
}

class SafeListener : IDisposable
{
    private readonly EventSource _src;

    public SafeListener(EventSource src)
    {
        _src = src;
        _src.Fired += OnFired;
        Console.WriteLine("Подписан");
    }

    void OnFired() => Console.WriteLine("Событие!");

    public void Dispose()
    {
        _src.Fired -= OnFired; // явная отписка
        Console.WriteLine("Отписан");
    }
}

var src = new EventSource();
using var listener = new SafeListener(src);
src.Fire(); // Событие!
// Dispose вызовется автоматически: Отписан"""})

post(1005, 'quiz', {'question': 'Почему событийная подписка может вызывать утечку памяти?', 'options': [
    'Делегаты занимают слишком много памяти в стеке',
    'Publisher хранит ссылку на Subscriber через список обработчиков, не давая GC его собрать',
    'События не поддерживают сборку мусора',
    'Подписчики копируются в памяти при каждом вызове'
], 'correct': 1, 'explanation': 'Publisher хранит ссылку на делегат, который ссылается на экземпляр Subscriber. Пока Publisher жив — Subscriber не будет собран GC, даже если больше нигде не используется.'})

post(1005, 'control_question', {'question': 'Назови два способа избежать утечек памяти через события в C#.', 'answer': '1) Явная отписка от событий в Dispose() через оператор -= . 2) Использование WeakReference<T> для хранения слабой ссылки на подписчика, которую GC может очистить.'})

# ── 1006: AggregateException ──────────────────────────────────────────────────
print('=== 1006: AggregateException ===')

post(1006, 'text', {'markdown': """## AggregateException

`AggregateException` — специальное исключение, которое может содержать **несколько вложенных исключений**. Возникает при параллельном выполнении задач (Task, PLINQ).

### Откуда берётся
```csharp
var tasks = new[]
{
    Task.Run(() => throw new InvalidOperationException("Ошибка 1")),
    Task.Run(() => throw new ArgumentException("Ошибка 2")),
};

try
{
    Task.WaitAll(tasks);
}
catch (AggregateException ae)
{
    // ae.InnerExceptions содержит обе ошибки
    foreach (var ex in ae.InnerExceptions)
        Console.WriteLine(ex.Message);
}
```

### Flatten() — разворачивает вложенные AggregateException
```csharp
catch (AggregateException ae)
{
    foreach (var ex in ae.Flatten().InnerExceptions)
        Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
}
```

### Handle() — обработка конкретных типов
```csharp
catch (AggregateException ae)
{
    ae.Handle(ex =>
    {
        if (ex is InvalidOperationException)
        {
            Console.WriteLine("Обработано: " + ex.Message);
            return true; // обработано
        }
        return false; // пробрасываем дальше
    });
}
```

### Когда встречается
- `Task.WaitAll()` / `Task.WhenAll()` await
- PLINQ с `.ForAll()` / `.ToList()`
- `Parallel.For` / `Parallel.ForEach`"""})

post(1006, 'code', {'language': 'csharp', 'title': 'Обработка AggregateException', 'code': """var tasks = new[]
{
    Task.Run(() => { throw new InvalidOperationException("плохое состояние"); }),
    Task.Run(() => { throw new ArgumentNullException("param"); }),
    Task.Run(() => Console.WriteLine("Задача 3 успешна")),
};

try
{
    Task.WaitAll(tasks);
}
catch (AggregateException ae)
{
    Console.WriteLine($"Всего ошибок: {ae.InnerExceptions.Count}");

    ae.Flatten().Handle(ex =>
    {
        Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
        return true; // все обработаны
    });
}"""})

post(1006, 'quiz', {'question': 'Что делает метод AggregateException.Flatten()?', 'options': [
    'Преобразует исключение в строку с деталями всех ошибок',
    'Разворачивает вложенные AggregateException в плоский список InnerExceptions',
    'Выбрасывает только первое исключение из списка',
    'Логирует все исключения в консоль'
], 'correct': 1, 'explanation': 'Flatten() рекурсивно разворачивает вложенные AggregateException, создавая единый плоский список всех реальных исключений. Удобно когда задачи вложены друг в друга.'})

post(1006, 'control_question', {'question': 'Чем AggregateException отличается от обычного Exception? В каких ситуациях он возникает?', 'answer': 'AggregateException содержит коллекцию InnerExceptions — несколько исключений одновременно. Возникает при параллельном выполнении: Task.WaitAll(), await Task.WhenAll(), PLINQ, Parallel.For/ForEach — когда несколько задач могут бросить исключения независимо.'})

# ── 1007: ExceptionDispatchInfo ───────────────────────────────────────────────
print('=== 1007: ExceptionDispatchInfo ===')

post(1007, 'text', {'markdown': """## ExceptionDispatchInfo

`ExceptionDispatchInfo` позволяет **захватить исключение** и **перебросить его позже**, сохраняя оригинальный стек вызовов.

### Проблема без EDI
```csharp
Exception captured = null;
try { throw new InvalidOperationException("ошибка"); }
catch (Exception ex) { captured = ex; }

// Позже...
throw captured; // ТЕРЯЕТ оригинальный стек вызовов!
```

### Решение с ExceptionDispatchInfo
```csharp
using System.Runtime.ExceptionServices;

ExceptionDispatchInfo edi = null;
try { throw new InvalidOperationException("ошибка"); }
catch (Exception ex) { edi = ExceptionDispatchInfo.Capture(ex); }

// Позже...
edi.Throw(); // сохраняет оригинальный стек + добавляет текущий
```

### Когда нужно?
- Передача исключений между потоками
- Сохранение исключения для повторного броска из другого контекста
- Именно так `async/await` сохраняет стек при `await Task.WhenAll()`

### Под капотом async/await
Когда вы `await` задачу с исключением — .NET использует EDI внутри, чтобы стек вызовов указывал на оригинальное место исключения, а не на точку `await`."""})

post(1007, 'code', {'language': 'csharp', 'title': 'Захват и повторный бросок исключения', 'code': """using System.Runtime.ExceptionServices;

static ExceptionDispatchInfo CaptureError()
{
    try
    {
        throw new InvalidOperationException("Что-то пошло не так");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Захвачено: {ex.Message}");
        return ExceptionDispatchInfo.Capture(ex);
    }
}

var edi = CaptureError();

// Имитируем работу...
Console.WriteLine("Продолжаем работу...");

// Перебрасываем позже, сохраняя стек
try
{
    edi.Throw();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Переброшено: {ex.Message}");
}"""})

post(1007, 'quiz', {'question': 'Зачем использовать ExceptionDispatchInfo вместо обычного throw captured?', 'options': [
    'EDI быстрее, так как не создаёт новый объект исключения',
    'EDI сохраняет оригинальный стек вызовов при повторном бросании исключения',
    'Обычный throw не работает в async методах',
    'EDI автоматически логирует исключение перед броском'
], 'correct': 1, 'explanation': 'throw captured заменяет стек вызовов текущим местом броска, теряя оригинальный стек. EDI.Throw() сохраняет исходный стек и добавляет текущий — ценно при отладке многопоточного кода.'})

post(1007, 'control_question', {'question': 'Как async/await связан с ExceptionDispatchInfo?', 'answer': '.NET использует ExceptionDispatchInfo внутри async/await механизма: когда задача завершается с исключением, EDI сохраняет оригинальный стек. При await исключение перебрасывается с EDI.Throw(), поэтому стек указывает на место реального броска, а не на await-точку.'})

print('Done!')
