import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 996: IDisposable ──────────────────────────────────────────────────────────
print('=== 996: IDisposable и using ===')

post(996, 'text', {'markdown': """## IDisposable и using

Некоторые объекты держат **неуправляемые ресурсы**: файлы, соединения с БД, сетевые сокеты. GC не знает, когда их освобождать — нужен явный `Dispose()`.

### Паттерн using
```csharp
using (var file = new StreamReader("data.txt"))
{
    string content = file.ReadToEnd();
} // Dispose() вызывается автоматически, даже при исключении
```

### using declaration (C# 8+) — современный способ
```csharp
using var file = new StreamReader("data.txt");
// file.Dispose() вызовется в конце блока { }
```

### Реализация IDisposable
```csharp
class DatabaseConnection : IDisposable
{
    private bool _disposed = false;

    public void Query(string sql) { /* ... */ }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Закрыть соединение, освободить ресурсы
            _disposed = true;
        }
    }
}
```

### Правило большого пальца
> Если класс содержит поля типа `Stream`, `IDbConnection`, `HttpClient`, `Socket` — реализуй `IDisposable` и освобождай их в `Dispose()`."""})

post(996, 'code', {'language': 'csharp', 'title': 'IDisposable своими руками', 'code': """class Logger : IDisposable
{
    private bool _disposed;
    public string Name { get; }

    public Logger(string name)
    {
        Name = name;
        Console.WriteLine($"[{name}] открыт");
    }

    public void Log(string msg) => Console.WriteLine($"[{Name}] {msg}");

    public void Dispose()
    {
        if (!_disposed)
        {
            Console.WriteLine($"[{Name}] закрыт");
            _disposed = true;
        }
    }
}

using var log = new Logger("App");
log.Log("Старт");
log.Log("Работаем...");
// Dispose вызовется автоматически в конце блока"""})

post(996, 'quiz', {'question': 'Что гарантирует конструкция using (var r = ...) { }?', 'options': [
    'Объект будет уничтожен сборщиком мусора немедленно',
    'Метод Dispose() будет вызван по завершении блока, даже если выброшено исключение',
    'Объект нельзя использовать за пределами блока, но Dispose не вызывается',
    'Создаётся копия объекта, оригинал остаётся в памяти'
], 'correct': 1, 'explanation': 'using гарантирует вызов Dispose() в конце блока — даже при исключении. Это эквивалент try/finally с r.Dispose() в finally.'})

post(996, 'task', {'title': 'Считай ресурс с using', 'description': 'Создай класс Resource с IDisposable. Конструктор печатает "open", Dispose — "closed". Создай через using и убедись в порядке вывода.', 'starterCode': """class Resource : IDisposable
{
    public Resource() => Console.WriteLine("open");
    public void Dispose() => Console.WriteLine("closed");
}

using var r = new Resource();
Console.WriteLine("using");
""", 'expectedOutput': 'open\nusing\nclosed', 'language': 'csharp'})

# ── 997: Индексаторы ──────────────────────────────────────────────────────────
print('=== 997: Индексаторы ===')

post(997, 'text', {'markdown': """## Индексаторы

Индексатор позволяет обращаться к объекту **как к массиву** с помощью `[]`.

```csharp
class Matrix
{
    private int[,] _data = new int[3, 3];

    public int this[int row, int col]
    {
        get => _data[row, col];
        set => _data[row, col] = value;
    }
}

var m = new Matrix();
m[0, 0] = 42;
Console.WriteLine(m[0, 0]); // 42
```

### Индексатор может принимать любой тип ключа
```csharp
class Config
{
    private Dictionary<string, string> _data = new();

    public string this[string key]
    {
        get => _data.GetValueOrDefault(key, "");
        set => _data[key] = value;
    }
}

var cfg = new Config();
cfg["theme"] = "dark";
Console.WriteLine(cfg["theme"]); // dark
```

### Индекс с конца (C# 8+)
```csharp
var arr = new[] { 1, 2, 3, 4, 5 };
Console.WriteLine(arr[^1]); // 5 (последний)
Console.WriteLine(arr[^2]); // 4 (предпоследний)
```""" })

post(997, 'code', {'language': 'csharp', 'title': 'Своя коллекция с индексатором', 'code': """class WordList
{
    private List<string> _words = new();

    public void Add(string word) => _words.Add(word);

    public string this[int index] => _words[index].ToUpper();
    public string this[string word] => _words.Contains(word) ? "есть" : "нет";

    public int Count => _words.Count;
}

var list = new WordList();
list.Add("apple");
list.Add("banana");
list.Add("cherry");

Console.WriteLine(list[0]);         // APPLE
Console.WriteLine(list[2]);         // CHERRY
Console.WriteLine(list["banana"]);  // есть
Console.WriteLine(list["mango"]);   // нет"""})

post(997, 'quiz', {'question': 'Что такое индексатор в C#?', 'options': [
    'Специальный метод для поиска элемента по значению',
    'Свойство с параметром, позволяющее обращаться к объекту через []',
    'Атрибут, помечающий поле как ключевое',
    'Перегрузка оператора + для конкатенации'
], 'correct': 1, 'explanation': 'Индексатор — это свойство вида this[тип параметра]. Позволяет писать obj[key], как если бы это был массив или словарь.'})

post(997, 'task', {'title': 'Кэш с индексатором', 'description': 'Создай класс Cache с индексатором string this[string key]. При чтении несуществующего ключа — вернуть "miss". Выведи результаты.', 'starterCode': """class Cache
{
    private Dictionary<string, string> _store = new();

    public string this[string key]
    {
        get => _store.ContainsKey(key) ? _store[key] : "miss";
        set => _store[key] = value;
    }
}

var cache = new Cache();
cache["user"] = "Alice";
Console.WriteLine(cache["user"]);
Console.WriteLine(cache["role"]);
""", 'expectedOutput': 'Alice\nmiss', 'language': 'csharp'})

# ── 998: Явная реализация интерфейса ─────────────────────────────────────────
print('=== 998: Явная реализация интерфейса ===')

post(998, 'text', {'markdown': """## Явная реализация интерфейса

Когда класс реализует **два интерфейса с одинаковым методом**, используют явную реализацию:

```csharp
interface ILogger  { void Log(string msg); }
interface IPrinter { void Log(string msg); }

class Service : ILogger, IPrinter
{
    void ILogger.Log(string msg)  => Console.WriteLine($"[LOG] {msg}");
    void IPrinter.Log(string msg) => Console.WriteLine($"[PRINT] {msg}");
}
```

### Вызов явной реализации — только через интерфейс
```csharp
var svc = new Service();
// svc.Log("test"); // ❌ ошибка — метод скрыт

((ILogger)svc).Log("test");  // [LOG] test
((IPrinter)svc).Log("test"); // [PRINT] test
```

### Зачем ещё нужна явная реализация?
- **Скрыть** метод от внешнего кода (он виден только через интерфейс)
- Предоставить **разные реализации** одного метода
- Реализовать устаревший интерфейс без засорения публичного API

### Пример — скрытие IDisposable
```csharp
class MyClass : IDisposable
{
    void IDisposable.Dispose() { /* cleanup */ }
    // Пользователь не видит Dispose() напрямую
    // Только через using или ((IDisposable)obj).Dispose()
}
```""" })

post(998, 'code', {'language': 'csharp', 'title': 'Два интерфейса — один метод', 'code': """interface IShape   { double Area(); }
interface IFigure  { double Area(); }

class Circle : IShape, IFigure
{
    private double _r;
    public Circle(double r) => _r = r;

    // Явная реализация для IShape
    double IShape.Area()  => Math.PI * _r * _r;

    // Явная реализация для IFigure (возвращает округлённую)
    double IFigure.Area() => Math.Round(Math.PI * _r * _r, 2);
}

var c = new Circle(5);
Console.WriteLine(((IShape)c).Area());   // 78.5398...
Console.WriteLine(((IFigure)c).Area());  // 78.54"""})

post(998, 'quiz', {'question': 'Как вызвать явно реализованный метод ILogger.Log() у объекта Service?', 'options': [
    'service.Log("msg")',
    '((ILogger)service).Log("msg")',
    'service.ILogger.Log("msg")',
    'ILogger.Log(service, "msg")'
], 'correct': 1, 'explanation': 'Явно реализованные методы недоступны напрямую через переменную типа класса. Нужно привести к интерфейсу: ((ILogger)service).Log("msg") — или хранить в переменной типа ILogger.'})

post(998, 'control_question', {'question': 'Зачем нужна явная реализация интерфейса? Назови два случая.', 'answer': '1) Когда два интерфейса имеют метод с одинаковым именем — нужно реализовать каждый отдельно. 2) Чтобы скрыть метод от публичного API класса — он будет доступен только через интерфейс.'})

print('Done!')
