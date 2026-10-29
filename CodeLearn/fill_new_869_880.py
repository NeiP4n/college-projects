import json, urllib.request, urllib.error

BASE = "http://localhost:5003/api/builder/lessons"

def post(lid, btype, obj):
    url = f"{BASE}/{lid}/blocks"
    body = json.dumps({"type": btype, "content": json.dumps(obj, ensure_ascii=False)}).encode()
    req = urllib.request.Request(url, data=body, headers={"Content-Type": "application/json"}, method="POST")
    try:
        with urllib.request.urlopen(req) as r:
            st = r.status
    except urllib.error.HTTPError as e:
        st = e.code
        print(f"  ERR {e.code}")
    print(f"  [{st}] {btype}")

# ==========================================
# Lesson 962: Синхронность vs асинхронность
# ==========================================
print("\n=== Lesson 962: Синхронность vs асинхронность ===")

post(962, "text", {"markdown": """## Синхронный vs асинхронный код

**Синхронный** код выполняется последовательно: каждая операция ждёт завершения предыдущей. Поток **заблокирован** во время ожидания.

**Асинхронный** код позволяет потоку заниматься другой работой, пока ожидается результат долгой операции (например, I/O, сетевой запрос).

### Когда это важно?
- Сетевые запросы (HTTP, базы данных)
- Файловые операции
- Таймеры и задержки
- Любые I/O-bound операции

Без асинхронности серверное приложение с 100 одновременными запросами заблокирует 100 потоков — расточительно и медленно."""})

post(962, "two_path", {
    "question": "Как правильно ожидать результат асинхронной операции?",
    "pathA": {
        "title": "Блокирующий вызов (.Result / .Wait())",
        "code": "// ОПАСНО: может вызвать deadlock в UI/ASP.NET контексте\nstring html = DownloadPageAsync(\"https://example.com\").Result;\nConsole.WriteLine(html.Length);"
    },
    "pathB": {
        "title": "Правильный await",
        "code": "// ПРАВИЛЬНО: поток освобождается во время ожидания\nstring html = await DownloadPageAsync(\"https://example.com\");\nConsole.WriteLine(html.Length);"
    },
    "explanation": "Использование .Result или .Wait() блокирует поток и может привести к deadlock в контекстах с SynchronizationContext (WinForms, ASP.NET classic). Всегда предпочитайте await."
})

post(962, "code", {
    "language": "csharp",
    "title": "Демонстрация: синхронный vs асинхронный",
    "code": """using System;
using System.Threading;
using System.Threading.Tasks;

// Синхронный вариант
void SyncOperation()
{
    Console.WriteLine($"[Sync] Start. Thread: {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(2000); // блокируем поток на 2 секунды
    Console.WriteLine($"[Sync] Done.  Thread: {Thread.CurrentThread.ManagedThreadId}");
}

// Асинхронный вариант
async Task AsyncOperation()
{
    Console.WriteLine($"[Async] Start. Thread: {Thread.CurrentThread.ManagedThreadId}");
    await Task.Delay(2000); // поток свободен во время ожидания
    Console.WriteLine($"[Async] Done.  Thread: {Thread.CurrentThread.ManagedThreadId}");
}

SyncOperation();
await AsyncOperation();"""
})

post(962, "quiz", {
    "question": "Что происходит с потоком при вызове Thread.Sleep(2000)?",
    "options": [
        "Поток освобождается и может обрабатывать другие задачи",
        "Поток блокируется на 2 секунды и не может ничего делать",
        "Создаётся новый поток для ожидания",
        "Операция выполняется в фоновом потоке"
    ],
    "correct": 1,
    "explanation": "Thread.Sleep() блокирует текущий поток полностью. В отличие от await Task.Delay(), поток не может обрабатывать другие запросы во время ожидания."
})

post(962, "prediction", {
    "language": "csharp",
    "code": """using System;
using System.Threading.Tasks;

async Task<string> GetDataAsync()
{
    await Task.Delay(100);
    return "Hello, Async!";
}

Console.WriteLine("Before");
string result = await GetDataAsync();
Console.WriteLine(result);
Console.WriteLine("After");""",
    "expectedOutput": "Before\nHello, Async!\nAfter",
    "hint": "Код выполняется сверху вниз. await приостанавливает метод, но не меняет порядок вывода."
})

post(962, "knowledge_check", {
    "topic": "Синхронность vs асинхронность",
    "question": "В каком сценарии асинхронное программирование даёт наибольший выигрыш?",
    "options": [
        "Сложные математические вычисления (CPU-bound)",
        "Ожидание ответа от базы данных или HTTP-запроса (I/O-bound)",
        "Сортировка больших массивов в памяти",
        "Рекурсивные алгоритмы"
    ],
    "correct": 1,
    "explanation": "Асинхронность эффективна для I/O-bound операций: пока поток ждёт ответа из сети или диска, он может обрабатывать другие запросы. Для CPU-bound задач нужен параллелизм (Task.Run, Parallel)."
})

# ==========================================
# Lesson 963: Task и Task<T>
# ==========================================
print("\n=== Lesson 963: Task и Task<T> ===")

post(963, "text", {"markdown": """## Task и Task<T>

`Task` — основной тип для представления асинхронной операции в .NET.

| Тип | Описание |
|-----|----------|
| `Task` | Операция без возвращаемого значения |
| `Task<T>` | Операция, возвращающая значение типа T |
| `ValueTask<T>` | Облегчённый вариант для высокопроизводительного кода |

### Состояния Task
- **Created** — создан, но не запущен
- **Running** — выполняется
- **RanToCompletion** — успешно завершён
- **Faulted** — завершён с исключением
- **Canceled** — отменён

### Создание задач
```csharp
// Запустить в пуле потоков
Task t1 = Task.Run(() => DoWork());
Task<int> t2 = Task.Run(() => ComputeValue());

// Уже завершённая задача
Task<int> completed = Task.FromResult(42);
Task failed = Task.FromException(new Exception("Oops"));
```"""})

post(963, "code", {
    "language": "csharp",
    "title": "Работа с Task и Task<T>",
    "code": """using System;
using System.Threading.Tasks;

// Task без возвращаемого значения
async Task PrintMessageAsync(string message)
{
    await Task.Delay(100); // имитация работы
    Console.WriteLine(message);
}

// Task<T> с возвращаемым значением
async Task<int> CalculateSumAsync(int a, int b)
{
    await Task.Delay(50); // имитация I/O
    return a + b;
}

// Использование
await PrintMessageAsync("Привет из Task!");

int sum = await CalculateSumAsync(10, 32);
Console.WriteLine($"Сумма: {sum}");

// Проверка состояния задачи
Task<string> task = Task.FromResult("Done");
Console.WriteLine($"Статус: {task.Status}");       // RanToCompletion
Console.WriteLine($"Завершена: {task.IsCompleted}"); // True"""
})

post(963, "quiz", {
    "question": "Какой тип нужно использовать для асинхронного метода, который ничего не возвращает?",
    "options": [
        "void",
        "Task",
        "Task<void>",
        "ValueTask"
    ],
    "correct": 1,
    "explanation": "Асинхронный метод без возвращаемого значения должен возвращать Task (не void). Async void используется только для обработчиков событий и считается антипаттерном — исключения из async void не перехватываются."
})

post(963, "mini_task", {
    "language": "csharp",
    "title": "Асинхронная сумма массива",
    "description": "Реализуй асинхронный метод SumArrayAsync, который вычисляет сумму элементов массива. Метод должен симулировать асинхронную операцию через Task.Delay(1).",
    "starterCode": """async Task<int> SumArrayAsync(int[] nums)
{
    await Task.Delay(1); // simulate IO
    // посчитай и верни сумму массива
    return 0; // замени на правильный результат
}

int[] data = {5, 12, 3, 8, 21, 4, 9};
Console.WriteLine(await SumArrayAsync(data));""",
    "expectedOutput": "62"
})

post(963, "prediction", {
    "language": "csharp",
    "code": """using System;
using System.Threading.Tasks;

Task<int> t = Task.Run(() => {
    return 2 + 2;
});

int result = await t;
Console.WriteLine($"Result: {result}");
Console.WriteLine($"Completed: {t.IsCompleted}");""",
    "expectedOutput": "Result: 4\nCompleted: True",
    "hint": "После await задача завершена, поэтому IsCompleted = true."
})

post(963, "fix_bug", {
    "language": "csharp",
    "buggyCode": """async void FetchDataAsync()
{
    await Task.Delay(100);
    Console.WriteLine("Data fetched");
}

// Вызов
FetchDataAsync();
await Task.Delay(200);
Console.WriteLine("Done");""",
    "expectedOutput": "Data fetched\nDone",
    "hint": "async void — антипаттерн. Исключения из async void теряются, и нельзя дождаться завершения. Замени возвращаемый тип на Task."
})

# ==========================================
# Lesson 964: async / await основы
# ==========================================
print("\n=== Lesson 964: async / await основы ===")

post(964, "text", {"markdown": """## async / await — основы

Ключевые слова `async` и `await` позволяют писать асинхронный код, который выглядит как синхронный.

### Правила
1. Метод помечается `async` — это разрешает использование `await` внутри
2. `await` приостанавливает выполнение метода до завершения задачи
3. Пока задача выполняется, поток возвращается в пул и может делать другую работу
4. После завершения задачи выполнение продолжается с того же места

### Цепочка async
```csharp
// Если метод вызывает async метод — он тоже должен быть async
async Task<string> GetUserNameAsync(int id)
{
    var user = await GetUserAsync(id);   // await!
    return user.Name;
}
```

### async Main
```csharp
// Начиная с C# 7.1 — можно сделать Main асинхронным
static async Task Main(string[] args)
{
    await DoSomethingAsync();
}
```"""})

post(964, "code", {
    "language": "csharp",
    "title": "Цепочка async/await методов",
    "code": """using System;
using System.Threading.Tasks;

async Task<string> FetchFromDatabaseAsync(int id)
{
    await Task.Delay(100); // имитация запроса к БД
    return $"User_{id}";
}

async Task<string> GetGreetingAsync(int userId)
{
    string name = await FetchFromDatabaseAsync(userId);
    return $"Привет, {name}!";
}

async Task DisplayGreetingAsync(int userId)
{
    string greeting = await GetGreetingAsync(userId);
    Console.WriteLine(greeting);
}

await DisplayGreetingAsync(42);
Console.WriteLine("Finished");"""
})

post(964, "fix_bug", {
    "language": "csharp",
    "buggyCode": """using System;
using System.Threading.Tasks;

async Task<int> GetValueAsync()
{
    await Task.Delay(50);
    return 100;
}

// Попытка получить значение без await
async Task RunAsync()
{
    Task<int> task = GetValueAsync();
    Console.WriteLine($"Value: {task}"); // баг!
}

await RunAsync();""",
    "expectedOutput": "Value: 100",
    "hint": "task — это объект Task<int>, а не значение. Нужно использовать await task или task.Result (но await предпочтительнее)."
})

post(964, "fill_blank", {
    "template": "async Task<string> LoadDataAsync()\n{\n    ___ Task.Delay(500);\n    return ___;\n}",
    "blanks": ["await", "\"data\""],
    "hint": "Перед Task.Delay нужен await. Метод возвращает Task<string>, значит return должен вернуть строку."
})

post(964, "quiz", {
    "question": "Что произойдёт, если убрать await перед вызовом асинхронного метода?",
    "options": [
        "Код не скомпилируется",
        "Метод выполнится синхронно",
        "Вернётся объект Task, а не результат — выполнение продолжится немедленно, не дожидаясь завершения",
        "Программа зависнет"
    ],
    "correct": 2,
    "explanation": "Без await метод возвращает Task (обещание результата), и выполнение продолжается без ожидания. Это называется 'fire and forget' — можно потерять результат и исключения."
})

post(964, "knowledge_check", {
    "topic": "async/await",
    "question": "В каком случае метод ОБЯЗАН иметь модификатор async?",
    "options": [
        "Когда он вызывает другой async метод",
        "Когда он содержит оператор await",
        "Когда он возвращает Task",
        "Когда он работает с файлами"
    ],
    "correct": 1,
    "explanation": "Модификатор async обязателен только когда внутри метода используется await. Метод может возвращать Task и без async (просто возвращая Task.FromResult или цепочку задач)."
})

# ==========================================
# Lesson 965: ConfigureAwait и контекст синхронизации
# ==========================================
print("\n=== Lesson 965: ConfigureAwait и контекст синхронизации ===")

post(965, "text", {"markdown": """## ConfigureAwait и контекст синхронизации

После `await` выполнение по умолчанию **возобновляется в том же контексте синхронизации**, где был вызван await.

### Что такое SynchronizationContext?
- **UI (WinForms/WPF)**: контекст = UI-поток. После await код возобновляется в UI-потоке — можно обновлять элементы управления.
- **ASP.NET classic**: контекст привязан к HTTP-запросу.
- **ASP.NET Core / Console**: контекст по умолчанию отсутствует.

### ConfigureAwait(false)
```csharp
// Без ConfigureAwait(false) — возобновление в исходном контексте
await SomeAsync();

// С ConfigureAwait(false) — возобновление в любом потоке пула
await SomeAsync().ConfigureAwait(false);
```

### Когда использовать ConfigureAwait(false)?
- В **библиотечном коде** — всегда, чтобы не захватывать контекст и избежать deadlock
- В **UI/ASP.NET приложениях** — только если не нужен доступ к UI/контексту после await"""})

post(965, "semantic_diff", {
    "intendedCode": """// Библиотечный метод: не нужен контекст UI
public async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    string data = await client.GetStringAsync(url).ConfigureAwait(false);
    // Обработка данных — не требует UI-контекста
    return data.ToUpper();
}""",
    "actualCode": """// Тот же метод, но без ConfigureAwait(false)
public async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    string data = await client.GetStringAsync(url); // захватывает контекст!
    // В UI-приложении: возобновится в UI-потоке (лишняя нагрузка)
    // В ASP.NET classic: риск deadlock при .Result/.Wait()
    return data.ToUpper();
}""",
    "explanation": "ConfigureAwait(false) указывает, что продолжение может выполняться в любом потоке пула, а не обязательно в исходном контексте. Для библиотечного кода это лучшая практика: повышает производительность и избегает потенциальных deadlock."
})

post(965, "code", {
    "language": "csharp",
    "title": "Deadlock с .Result в UI-контексте (антипаттерн)",
    "code": """// ДЕМОНСТРАЦИЯ DEADLOCK — НЕ ДЕЛАЙТЕ ТАК В UI/ASP.NET!

// Метод без ConfigureAwait(false)
async Task<string> GetDataAsync()
{
    await Task.Delay(100); // ждёт завершения, пытается вернуться в UI-поток
    return "data";
}

// В UI event handler:
// private void Button_Click(object sender, EventArgs e)
// {
//     string result = GetDataAsync().Result; // DEADLOCK!
//     // UI-поток заблокирован .Result
//     // GetDataAsync пытается вернуться в UI-поток — но он занят!
//     // Взаимная блокировка.
// }

// ПРАВИЛЬНЫЙ вариант:
async Task<string> GetDataLibraryAsync()
{
    await Task.Delay(100).ConfigureAwait(false); // не захватывает контекст
    return "data";
}

// Теперь .Result безопаснее (хотя всё равно лучше await):
// string result = GetDataLibraryAsync().Result; // не deadlock
Console.WriteLine("Пример демонстрирует риск deadlock при смешивании sync/async");"""
})

post(965, "quiz", {
    "question": "Когда нужно использовать ConfigureAwait(false)?",
    "options": [
        "Всегда, в любом коде",
        "Никогда — это устаревший паттерн",
        "В библиотечном коде, не зависящем от контекста синхронизации",
        "Только в консольных приложениях"
    ],
    "correct": 2,
    "explanation": "ConfigureAwait(false) используется в библиотечном коде, чтобы не захватывать контекст синхронизации (избегать deadlock и снизить накладные расходы). В UI-коде, где нужен доступ к элементам после await, ConfigureAwait(false) использовать не стоит."
})

post(965, "transfer_test", {
    "question": "У вас есть ASP.NET Core приложение. Нужно ли использовать ConfigureAwait(false) в контроллерах?",
    "options": [
        "Да, всегда обязательно",
        "Нет — в ASP.NET Core нет SynchronizationContext, поэтому особого смысла нет, но для совместимости библиотек всё равно рекомендуется",
        "Нет, это сломает приложение",
        "Только в методах с [HttpGet]"
    ],
    "correct": 1,
    "explanation": "ASP.NET Core не имеет SynchronizationContext по умолчанию, поэтому deadlock из-за контекста там не возникает. Однако ConfigureAwait(false) в библиотечных методах всё равно считается хорошей практикой для переносимости."
})

post(965, "knowledge_check", {
    "topic": "ConfigureAwait",
    "question": "Что делает await task.ConfigureAwait(false)?",
    "options": [
        "Отменяет задачу при ошибке",
        "Выполняет задачу в новом потоке",
        "Разрешает продолжению выполняться в любом потоке пула, не захватывая текущий контекст",
        "Увеличивает приоритет задачи"
    ],
    "correct": 2,
    "explanation": "ConfigureAwait(false) говорит планировщику: 'продолжение после await может работать в любом подходящем потоке, не нужно возвращаться в исходный контекст синхронизации'."
})

# ==========================================
# Lesson 966: Task.WhenAll и Task.WhenAny
# ==========================================
print("\n=== Lesson 966: Task.WhenAll и Task.WhenAny ===")

post(966, "text", {"markdown": """## Task.WhenAll и Task.WhenAny

### Task.WhenAll
Ожидает завершения **всех** задач. Позволяет запускать задачи параллельно.

```csharp
// Параллельный запуск — быстрее, чем sequential await
var t1 = FetchUser(1);
var t2 = FetchUser(2);
var t3 = FetchUser(3);
string[] results = await Task.WhenAll(t1, t2, t3);
```

### Task.WhenAny
Завершается, когда завершится **любая одна** из задач.

```csharp
Task<string> fast = FetchFast();
Task<string> slow = FetchSlow();
Task<string> winner = await Task.WhenAny(fast, slow);
Console.WriteLine(await winner);
```

### Последовательный vs параллельный await
```csharp
// МЕДЛЕННО: 3 секунды (1+1+1)
var a = await Task1Async(); // ждём 1 сек
var b = await Task2Async(); // ждём ещё 1 сек
var c = await Task3Async(); // ждём ещё 1 сек

// БЫСТРО: ~1 секунда (параллельно)
var t1 = Task1Async();
var t2 = Task2Async();
var t3 = Task3Async();
var (a, b, c) = (await t1, await t2, await t3);
// или: await Task.WhenAll(t1, t2, t3);
```"""})

post(966, "mutation", {
    "language": "csharp",
    "baseCode": """using System;
using System.Threading.Tasks;

async Task<int> FetchValueAsync(int id, int delayMs)
{
    await Task.Delay(delayMs);
    return id * 10;
}

// Последовательное выполнение (медленно)
async Task RunSequential()
{
    var a = await FetchValueAsync(1, 300);
    var b = await FetchValueAsync(2, 300);
    var c = await FetchValueAsync(3, 300);
    Console.WriteLine($"{a} {b} {c}");
}

await RunSequential();""",
    "mutation": "Измени RunSequential на параллельное выполнение через Task.WhenAll — запусти все три задачи одновременно",
    "expectedOutput": "10 20 30",
    "hint": "Сначала создай все три Task (не await), потом вызови await Task.WhenAll(...) и получи результаты."
})

post(966, "code", {
    "language": "csharp",
    "title": "Task.WhenAny — гонка задач",
    "code": """using System;
using System.Threading.Tasks;

async Task<string> SlowServer()
{
    await Task.Delay(500);
    return "Slow Server";
}

async Task<string> FastServer()
{
    await Task.Delay(100);
    return "Fast Server";
}

async Task<string> Backup()
{
    await Task.Delay(300);
    return "Backup Server";
}

// WhenAny: берём ответ от самого быстрого сервера
Task<string> winner = await Task.WhenAny(SlowServer(), FastServer(), Backup());
Console.WriteLine($"Первым ответил: {await winner}");"""
})

post(966, "quiz", {
    "question": "Что вернёт Task.WhenAll если одна из задач завершится исключением?",
    "options": [
        "Проигнорирует исключение и вернёт результаты остальных задач",
        "Выбросит исключение первой упавшей задачи, как только она упадёт",
        "Дождётся всех задач, затем выбросит AggregateException со всеми исключениями",
        "Отменит все оставшиеся задачи"
    ],
    "correct": 2,
    "explanation": "Task.WhenAll всегда дожидается ВСЕХ задач, даже если некоторые упали. После завершения всех задач, если были исключения — выбрасывается AggregateException, содержащий все исключения."
})

post(966, "mini_task", {
    "language": "csharp",
    "title": "Параллельная загрузка данных",
    "description": "Реализуй метод FetchAllAsync, который параллельно получает данные для трёх ID и возвращает их сумму. Используй Task.WhenAll.",
    "starterCode": """async Task<int> FetchValueAsync(int id)
{
    await Task.Delay(1);
    return id * 5;
}

async Task<int> FetchAllAsync(int[] ids)
{
    // Запусти FetchValueAsync для каждого id параллельно через Task.WhenAll
    // Верни сумму всех результатов
    return 0; // замени
}

int[] ids = {1, 2, 3};
Console.WriteLine(await FetchAllAsync(ids));""",
    "expectedOutput": "30"
})

post(966, "knowledge_check", {
    "topic": "Task.WhenAll / WhenAny",
    "question": "Какой метод нужно использовать, чтобы получить результат самой быстрой из нескольких задач?",
    "options": [
        "Task.WhenAll",
        "Task.WhenAny",
        "Task.WaitAny",
        "Task.First"
    ],
    "correct": 1,
    "explanation": "Task.WhenAny возвращает Task, который завершается, когда завершается любая одна из переданных задач. Это удобно для паттернов 'гонка' или задания таймаута."
})

# ==========================================
# Lesson 967: CancellationToken
# ==========================================
print("\n=== Lesson 967: CancellationToken ===")

post(967, "text", {"markdown": """## CancellationToken — отмена операций

`CancellationToken` — стандартный механизм для **кооперативной отмены** асинхронных операций в .NET.

### Как работает
1. Создаётся `CancellationTokenSource` (источник отмены)
2. Из него берётся `CancellationToken` (токен)
3. Токен передаётся в асинхронные методы
4. При вызове `cts.Cancel()` токен переходит в состояние "отменён"
5. Методы периодически проверяют `token.IsCancellationRequested` или вызывают `token.ThrowIfCancellationRequested()`

```csharp
var cts = new CancellationTokenSource();
CancellationToken token = cts.Token;

// Через 2 секунды — отмена
cts.CancelAfter(TimeSpan.FromSeconds(2));

try
{
    await LongOperationAsync(token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Операция отменена");
}
```"""})

post(967, "code", {
    "language": "csharp",
    "title": "Пример использования CancellationToken",
    "code": """using System;
using System.Threading;
using System.Threading.Tasks;

async Task ProcessItemsAsync(int[] items, CancellationToken token)
{
    foreach (int item in items)
    {
        token.ThrowIfCancellationRequested(); // проверяем отмену
        await Task.Delay(200, token);          // Task.Delay тоже поддерживает токен
        Console.WriteLine($"Обработан: {item}");
    }
}

var cts = new CancellationTokenSource();
cts.CancelAfter(500); // отменить через 500ms

int[] items = {1, 2, 3, 4, 5};

try
{
    await ProcessItemsAsync(items, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Обработка отменена!");
}
finally
{
    cts.Dispose();
}"""
})

post(967, "quiz", {
    "question": "Что делает token.ThrowIfCancellationRequested()?",
    "options": [
        "Отменяет все запущенные задачи",
        "Создаёт новый CancellationToken",
        "Выбрасывает OperationCanceledException если отмена была запрошена",
        "Блокирует поток до отмены"
    ],
    "correct": 2,
    "explanation": "ThrowIfCancellationRequested() — удобный способ проверить статус токена: если отмена запрошена, метод выбрасывает OperationCanceledException, что прерывает выполнение асинхронной операции."
})

post(967, "fix_bug", {
    "language": "csharp",
    "buggyCode": """using System;
using System.Threading;
using System.Threading.Tasks;

async Task DoWorkAsync(CancellationToken token)
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100); // не передаём токен!
        Console.WriteLine($"Step {i}");
    }
}

var cts = new CancellationTokenSource();
cts.CancelAfter(250);

await DoWorkAsync(cts.Token);
Console.WriteLine("Done");""",
    "expectedOutput": "Step 0\nStep 1\nOperationCanceledException handled",
    "hint": "Task.Delay поддерживает CancellationToken как второй параметр. Передай token в Task.Delay и оберни вызов в try/catch."
})

post(967, "mini_task", {
    "language": "csharp",
    "title": "Скачивание с таймаутом",
    "description": "Реализуй метод DownloadWithTimeoutAsync, который принимает URL и таймаут в секундах. Если операция занимает больше указанного времени — должна быть выброшена OperationCanceledException.",
    "starterCode": """using System.Threading;
using System.Threading.Tasks;

async Task<string> SimulateDownloadAsync(int delayMs, CancellationToken token)
{
    await Task.Delay(delayMs, token);
    return "Downloaded content";
}

async Task<string> DownloadWithTimeoutAsync(int timeoutSeconds)
{
    // Создай CancellationTokenSource с таймаутом
    // Вызови SimulateDownloadAsync(500, token)
    // Верни результат или позволь OperationCanceledException проброситься
    return ""; // замени
}

try
{
    string result = await DownloadWithTimeoutAsync(1);
    Console.WriteLine(result);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Timeout!");
}""",
    "expectedOutput": "Downloaded content"
})

post(967, "knowledge_check", {
    "topic": "CancellationToken",
    "question": "Как создать токен, который автоматически отменяется через 3 секунды?",
    "options": [
        "new CancellationToken(TimeSpan.FromSeconds(3))",
        "CancellationToken.Timeout(3000)",
        "new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token",
        "CancellationToken.CancelAfter(3000)"
    ],
    "correct": 2,
    "explanation": "CancellationTokenSource принимает таймаут в конструктор или через метод CancelAfter. После истечения времени токен автоматически переходит в состояние отмены."
})

# ==========================================
# Lesson 968: Deadlocks и best practices
# ==========================================
print("\n=== Lesson 968: Deadlocks и best practices ===")

post(968, "text", {"markdown": """## Deadlocks и лучшие практики async/await

### Как возникает async deadlock?
1. UI-поток вызывает `asyncMethod().Result` или `.Wait()`
2. UI-поток блокируется, ожидая результат
3. Внутри `asyncMethod` есть `await` без `ConfigureAwait(false)`
4. После await код пытается вернуться в UI-поток
5. Но UI-поток занят — **deadlock!**

### Лучшие практики

| Правило | Пояснение |
|---------|-----------|
| Никогда `async void` | Исключения теряются, нельзя await |
| Всегда `ConfigureAwait(false)` в библиотеках | Избегает захват контекста |
| Не смешивать sync/async | Не использовать `.Result`/`.Wait()` |
| Передавать `CancellationToken` | Через все async методы |
| Использовать `await using` | Для IAsyncDisposable |
| `Task.Run` для CPU-bound | Не блокировать пул I/O-потоков |"""})

post(968, "semantic_diff", {
    "intendedCode": """// Правильно: async Task, ConfigureAwait(false), токен пробрасывается
public async Task ProcessAsync(CancellationToken token = default)
{
    var data = await _repository.GetDataAsync(token).ConfigureAwait(false);
    await _service.ProcessAsync(data, token).ConfigureAwait(false);
}""",
    "actualCode": """// Неправильно: async void, нет ConfigureAwait, нет токена
public async void ProcessAsync()
{
    var data = await _repository.GetDataAsync();
    await _service.ProcessAsync(data);
}""",
    "explanation": "Три ошибки: 1) async void — исключения не перехватываются, нельзя дождаться завершения; 2) нет ConfigureAwait(false) — риск deadlock в UI/ASP.NET; 3) нет CancellationToken — нельзя отменить операцию."
})

post(968, "fix_bug", {
    "language": "csharp",
    "buggyCode": """using System;
using System.Threading.Tasks;

class DataService
{
    public async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "data";
    }
}

class Controller
{
    private DataService _service = new DataService();

    // DEADLOCK: вызов из синхронного контекста с захватом контекста
    public string GetData()
    {
        return _service.GetDataAsync().Result; // deadlock в UI/ASP.NET!
    }
}

// Правильный вариант должен быть async
var controller = new Controller();
Console.WriteLine(controller.GetData());""",
    "expectedOutput": "data",
    "hint": "Сделай GetData асинхронным: async Task<string> GetDataAsync() и используй await вместо .Result."
})

post(968, "checkpoint", {
    "title": "Итоговая проверка: async best practices",
    "description": "Убедись, что ты усвоил ключевые правила безопасного async-кода в C#.",
    "tasks": [
        "Никогда не использую async void (кроме обработчиков событий) — всегда async Task",
        "Использую ConfigureAwait(false) в библиотечном коде, не зависящем от UI-контекста",
        "Не блокирую async код через .Result или .Wait() — всегда await",
        "Передаю CancellationToken через все async методы для поддержки отмены",
        "Для параллельных задач использую Task.WhenAll вместо последовательных await"
    ]
})

post(968, "quiz", {
    "question": "Почему async void считается антипаттерном?",
    "options": [
        "Он выполняется медленнее чем async Task",
        "Исключения из async void нельзя поймать через try/catch вызывающего кода, и нельзя дождаться завершения",
        "async void не компилируется в новых версиях C#",
        "Он создаёт новый поток при каждом вызове"
    ],
    "correct": 1,
    "explanation": "async void — антипаттерн потому что: 1) исключения попадают в SynchronizationContext и могут крашнуть приложение; 2) вызывающий код не может await такой метод и не знает когда он завершился. Исключение — обработчики событий (Button.Click += async (s,e) => {...})."
})

post(968, "transfer_test", {
    "question": "Ваш коллега написал: `Task.Run(() => SaveToDbAsync()).Wait();` в Web API контроллере. Что не так?",
    "options": [
        "Task.Run нельзя использовать в контроллерах",
        ".Wait() блокирует поток пула, уменьшая масштабируемость — нужно await",
        "SaveToDbAsync() нужно вызывать напрямую без Task.Run",
        "Ничего, это правильный паттерн"
    ],
    "correct": 1,
    "explanation": ".Wait() блокирует поток пула запросов, пока Task.Run выполняет async операцию в другом потоке пула. Это трата двух потоков вместо нуля. Правильно: await SaveToDbAsync() в async методе контроллера."
})

# ==========================================
# Lesson 969: NuGet и управление зависимостями
# ==========================================
print("\n=== Lesson 969: NuGet и управление зависимостями ===")

post(969, "text", {"markdown": """## NuGet — менеджер пакетов .NET

**NuGet** — официальный менеджер пакетов для .NET. Пакеты хранятся на [nuget.org](https://nuget.org).

### Основные команды CLI
```bash
# Добавить пакет
dotnet add package Newtonsoft.Json

# Добавить конкретную версию
dotnet add package Serilog --version 3.1.1

# Удалить пакет
dotnet remove package Newtonsoft.Json

# Восстановить пакеты
dotnet restore

# Список установленных пакетов
dotnet list package
```

### .csproj — файл проекта
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="Serilog" Version="3.1.1" />
  </ItemGroup>
</Project>
```

### Версионирование (SemVer)
`MAJOR.MINOR.PATCH` — например `2.1.0`:
- **MAJOR**: несовместимые изменения API
- **MINOR**: новые возможности, обратно совместимые
- **PATCH**: исправления багов"""})

post(969, "quiz", {
    "question": "Какая команда добавляет NuGet пакет в проект?",
    "options": [
        "nuget install PackageName",
        "dotnet add package PackageName",
        "dotnet install PackageName",
        "npm install PackageName"
    ],
    "correct": 1,
    "explanation": "dotnet add package PackageName — стандартная команда CLI для добавления NuGet пакета. Она обновляет .csproj файл и скачивает пакет."
})

post(969, "code", {
    "language": "csharp",
    "title": "Использование Newtonsoft.Json (популярный NuGet пакет)",
    "code": """// После: dotnet add package Newtonsoft.Json
using Newtonsoft.Json;

var person = new { Name = "Алиса", Age = 30, Skills = new[] { "C#", "SQL" } };

// Сериализация в JSON
string json = JsonConvert.SerializeObject(person, Formatting.Indented);
Console.WriteLine(json);

// Десериализация из JSON
string source = "{\"Name\":\"Боб\",\"Age\":25}";
dynamic obj = JsonConvert.DeserializeObject(source)!;
Console.WriteLine($"Имя: {obj.Name}, Возраст: {obj.Age}");"""
})

post(969, "knowledge_check", {
    "topic": "NuGet",
    "question": "Где NuGet хранит информацию о зависимостях проекта?",
    "options": [
        "В файле packages.config (только)",
        "В файле .csproj в секции <ItemGroup><PackageReference>",
        "В папке node_modules",
        "В глобальном реестре Windows"
    ],
    "correct": 1,
    "explanation": "Современные проекты .NET (SDK-style) хранят зависимости в .csproj файле как <PackageReference> элементы. Старый формат packages.config используется в legacy проектах."
})

post(969, "drag_order", {
    "pieces": [
        "dotnet new console -n MyApp",
        "cd MyApp",
        "dotnet add package Newtonsoft.Json",
        "// Добавить using Newtonsoft.Json в Program.cs",
        "dotnet run"
    ],
    "solution": [0, 1, 2, 3, 4],
    "hint": "Сначала создаём проект, входим в папку, добавляем пакет, используем в коде, запускаем."
})

post(969, "transfer_test", {
    "question": "Вы клонировали репозиторий .NET проекта. Папка bin/obj пуста, пакеты не скачаны. Что нужно сделать перед запуском?",
    "options": [
        "dotnet build --force",
        "dotnet restore (или просто dotnet build, который автоматически вызывает restore)",
        "Вручную скачать пакеты с nuget.org",
        "Скопировать папку packages из другого проекта"
    ],
    "correct": 1,
    "explanation": "dotnet restore читает .csproj и скачивает все зависимости из NuGet. dotnet build делает это автоматически. Вручную скачивать ничего не нужно — это делает NuGet."
})

# ==========================================
# Lesson 970: Конфигурация и appsettings.json
# ==========================================
print("\n=== Lesson 970: Конфигурация и appsettings.json ===")

post(970, "text", {"markdown": """## Конфигурация в .NET: appsettings.json

.NET предоставляет унифицированную систему конфигурации через интерфейс `IConfiguration`.

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyDb;Trusted_Connection=True;"
  },
  "AppSettings": {
    "MaxRetries": 3,
    "ApiUrl": "https://api.example.com",
    "EnableCache": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Иерархия источников конфигурации
1. appsettings.json
2. appsettings.{Environment}.json (переопределяет базовый)
3. Переменные окружения (переопределяют файлы)
4. Аргументы командной строки (наивысший приоритет)

### Чтение конфигурации
```csharp
var config = builder.Configuration;

// Прямое чтение
string url = config["AppSettings:ApiUrl"];
int retries = config.GetValue<int>("AppSettings:MaxRetries");

// Привязка к классу (Options pattern)
builder.Services.Configure<AppSettings>(
    config.GetSection("AppSettings"));
```"""})

post(970, "code", {
    "language": "csharp",
    "title": "Options Pattern — строгая типизация конфигурации",
    "code": """// appsettings.json:
// {
//   "SmtpSettings": {
//     "Host": "smtp.example.com",
//     "Port": 587,
//     "EnableSsl": true
//   }
// }

// Класс настроек
public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
}

// Регистрация в Program.cs
// builder.Services.Configure<SmtpSettings>(
//     builder.Configuration.GetSection("SmtpSettings"));

// Использование в сервисе через IOptions<T>
public class EmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public void Send(string to, string subject)
    {
        Console.WriteLine($"Sending via {_settings.Host}:{_settings.Port} (SSL={_settings.EnableSsl})");
    }
}

Console.WriteLine("Options Pattern обеспечивает строгую типизацию конфигурации");"""
})

post(970, "quiz", {
    "question": "Какой файл конфигурации имеет наивысший приоритет?",
    "options": [
        "appsettings.json",
        "appsettings.Production.json",
        "Переменные окружения",
        "Аргументы командной строки"
    ],
    "correct": 3,
    "explanation": "Приоритет (от низшего к высшему): appsettings.json → appsettings.{Env}.json → переменные окружения → аргументы командной строки. Аргументы CLI переопределяют всё."
})

post(970, "fill_blank", {
    "template": "// Чтение вложенного значения конфигурации\nstring connStr = config___;\nint port = config.GetValue<int>___;",
    "blanks": ["[\"ConnectionStrings:DefaultConnection\"]", "(\"SmtpSettings:Port\")"],
    "hint": "Вложенные ключи разделяются двоеточием. GetValue<T> позволяет указать тип."
})

post(970, "knowledge_check", {
    "topic": "Конфигурация .NET",
    "question": "Зачем использовать Options Pattern (IOptions<T>) вместо прямого чтения из IConfiguration?",
    "options": [
        "Он быстрее при чтении",
        "Он обеспечивает строгую типизацию, валидацию и внедрение зависимостей",
        "Только он поддерживает appsettings.json",
        "IConfiguration не работает в .NET 8"
    ],
    "correct": 1,
    "explanation": "Options Pattern даёт: строгую типизацию (IntelliSense, проверка компилятором), возможность валидации, удобное внедрение через DI, поддержку IOptionsSnapshot (перечитывает при изменениях) и IOptionsMonitor (уведомления об изменениях)."
})

post(970, "transfer_test", {
    "question": "Как переопределить строку подключения к БД на production-сервере, не изменяя appsettings.json?",
    "options": [
        "Создать файл appsettings.override.json",
        "Установить переменную окружения ConnectionStrings__DefaultConnection",
        "Изменить код приложения",
        "Это невозможно без изменения файла"
    ],
    "correct": 1,
    "explanation": "Переменные окружения переопределяют значения из appsettings.json. Вложенные ключи записываются через двойное подчёркивание __. Это стандартная практика для production-конфигурации и Docker."
})

# ==========================================
# Lesson 971: Логирование: ILogger и Serilog
# ==========================================
print("\n=== Lesson 971: Логирование: ILogger и Serilog ===")

post(971, "text", {"markdown": """## Логирование в .NET: ILogger и Serilog

### ILogger — стандартный интерфейс
.NET предоставляет встроенный интерфейс `ILogger<T>` для логирования.

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(int orderId)
    {
        _logger.LogInformation("Processing order {OrderId}", orderId);
        try
        {
            // обработка...
            _logger.LogDebug("Order {OrderId} processed successfully", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
        }
    }
}
```

### Уровни логирования
| Уровень | Когда использовать |
|---------|-------------------|
| Trace | Детальная отладочная информация |
| Debug | Отладка в процессе разработки |
| Information | Нормальный ход работы приложения |
| Warning | Неожиданное поведение, но не ошибка |
| Error | Ошибки, требующие внимания |
| Critical | Критические сбои |

### Serilog — популярная библиотека
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```"""})

post(971, "code", {
    "language": "csharp",
    "title": "Настройка Serilog в ASP.NET Core",
    "code": """// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(); // заменяем встроенный логгер на Serilog

    var app = builder.Build();
    // ... middleware, routes...
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // важно: сбрасываем буфер при завершении
}"""
})

post(971, "quiz", {
    "question": "Почему в шаблоне лога лучше писать _logger.LogInformation(\"Order {OrderId}\", orderId) вместо _logger.LogInformation($\"Order {orderId}\")?",
    "options": [
        "Интерполированные строки не поддерживаются в ILogger",
        "Шаблон с {} сохраняет структурированные данные и избегает лишней аллокации строки если уровень отключён",
        "Это требование компилятора",
        "Никакой разницы нет"
    ],
    "correct": 1,
    "explanation": "Структурированное логирование ({OrderId}) сохраняет значение как отдельное поле, что позволяет фильтровать/искать по нему. Также избегает форматирования строки если уровень лога отключён (LogInformation не вызовется, строка не аллоцируется)."
})

post(971, "code_to_intent", {
    "code": """_logger.LogWarning("Retry attempt {Attempt} of {MaxRetries} for operation {Operation}",
    attempt, maxRetries, operationName);""",
    "options": [
        "Записать в лог информацию об ошибке с полным стектрейсом",
        "Записать предупреждение о повторной попытке с сохранением числовых значений как структурированных полей",
        "Отправить уведомление администратору о проблеме",
        "Остановить выполнение операции после превышения попыток"
    ],
    "correctIntent": 1
})

post(971, "fill_blank", {
    "template": "// Логирование ошибки с исключением\n_logger.Log___(ex, \"Failed to save user {UserId}\", userId);",
    "blanks": ["Error"],
    "hint": "При логировании исключений используйте LogError. Исключение передаётся первым параметром."
})

post(971, "knowledge_check", {
    "topic": "Логирование",
    "question": "Какой уровень логирования следует использовать для информации о нормальном ходе бизнес-операций (заказ создан, пользователь авторизован)?",
    "options": [
        "Debug",
        "Trace",
        "Information",
        "Warning"
    ],
    "correct": 2,
    "explanation": "LogInformation подходит для записи значимых событий нормального выполнения приложения. Debug и Trace — для детальной отладочной информации. Warning — для чего-то неожиданного, но не критичного."
})

# ==========================================
# Lesson 972: Dependency Injection
# ==========================================
print("\n=== Lesson 972: Dependency Injection ===")

post(972, "text", {"markdown": """## Dependency Injection (DI) в .NET

**Dependency Injection** — паттерн, при котором зависимости объекта предоставляются снаружи, а не создаются внутри.

### Жизненные циклы сервисов
| Lifetime | Описание | Метод регистрации |
|----------|----------|-------------------|
| **Singleton** | Один экземпляр на всё приложение | AddSingleton<> |
| **Scoped** | Один экземпляр на HTTP-запрос | AddScoped<> |
| **Transient** | Новый экземпляр при каждом запросе | AddTransient<> |

### Регистрация в Program.cs
```csharp
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

### Внедрение через конструктор
```csharp
public class OrderController
{
    private readonly IOrderRepository _repo;
    private readonly IEmailService _email;

    // DI-контейнер автоматически предоставит зависимости
    public OrderController(IOrderRepository repo, IEmailService email)
    {
        _repo = repo;
        _email = email;
    }
}
```"""})

post(972, "code_to_intent", {
    "code": """builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
// При разрешении IUserRepository получим MongoUserRepository""",
    "options": [
        "Зарегистрировать два репозитория и использовать оба одновременно",
        "Зарегистрировать SqlUserRepository как основной, MongoUserRepository как резервный",
        "Переопределить регистрацию: последняя запись выигрывает, IUserRepository будет разрешаться как MongoUserRepository",
        "Создать фабрику, которая выбирает реализацию динамически"
    ],
    "correctIntent": 2
})

post(972, "code", {
    "language": "csharp",
    "title": "DI: регистрация и использование сервисов",
    "code": """// Интерфейс
public interface IGreetingService
{
    string Greet(string name);
}

// Реализация
public class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Привет, {name}!";
}

// Потребитель
public class WelcomeController
{
    private readonly IGreetingService _greeting;

    public WelcomeController(IGreetingService greeting)
    {
        _greeting = greeting;
    }

    public void ShowWelcome(string userName)
    {
        Console.WriteLine(_greeting.Greet(userName));
    }
}

// Настройка DI-контейнера (Program.cs / вручную)
var services = new ServiceCollection();
services.AddTransient<IGreetingService, GreetingService>();
services.AddTransient<WelcomeController>();

var provider = services.BuildServiceProvider();

// Разрешение зависимостей
var controller = provider.GetRequiredService<WelcomeController>();
controller.ShowWelcome("Алиса");"""
})

post(972, "mini_task", {
    "language": "csharp",
    "title": "Реализация DI: интерфейс + класс",
    "description": "Зарегистрируй INotificationService как Transient, используя NotificationService в DI-контейнере. Разреши зависимость и вызови метод Notify.",
    "starterCode": """using Microsoft.Extensions.DependencyInjection;

public interface INotificationService
{
    void Notify(string message);
}

public class NotificationService : INotificationService
{
    public void Notify(string message)
    {
        Console.WriteLine($"Notification: {message}");
    }
}

// Настрой DI и вызови Notify("Hello DI!")
var services = new ServiceCollection();
// Зарегистрируй INotificationService -> NotificationService как Transient
// Разреши INotificationService
// Вызови Notify("Hello DI!")""",
    "expectedOutput": "Notification: Hello DI!"
})

post(972, "quiz", {
    "question": "Какой lifetime использовать для сервиса работы с БД (DbContext в Entity Framework)?",
    "options": [
        "Singleton — один контекст на всё приложение",
        "Scoped — один контекст на HTTP-запрос",
        "Transient — новый контекст при каждом обращении",
        "Любой — разницы нет"
    ],
    "correct": 1,
    "explanation": "DbContext должен быть Scoped: один экземпляр на HTTP-запрос обеспечивает Unit of Work (все операции в рамках запроса используют один контекст). Singleton опасен — DbContext не thread-safe. Transient — расточительно и нарушает транзакционность."
})

post(972, "knowledge_check", {
    "topic": "Dependency Injection",
    "question": "В чём главное преимущество DI по сравнению с созданием зависимостей через new?",
    "options": [
        "DI быстрее чем new",
        "DI позволяет не создавать объекты",
        "DI обеспечивает слабую связанность: зависимость от абстракции (интерфейса), а не от конкретной реализации — упрощает тестирование и замену реализаций",
        "DI автоматически управляет памятью"
    ],
    "correct": 2,
    "explanation": "Главное преимущество DI — Inversion of Control. Класс зависит от интерфейса, а не конкретной реализации. Это упрощает юнит-тестирование (можно подставить mock), облегчает замену реализаций (новый класс, та же регистрация)."
})

# ==========================================
# Lesson 973: Unit-тестирование с xUnit
# ==========================================
print("\n=== Lesson 973: Unit-тестирование с xUnit ===")

post(973, "text", {"markdown": """## Unit-тестирование с xUnit

**xUnit** — популярный фреймворк для юнит-тестирования в .NET.

### Установка
```bash
dotnet new xunit -n MyApp.Tests
dotnet add reference ../MyApp/MyApp.csproj
```

### Анатомия теста
```csharp
public class CalculatorTests
{
    [Fact]  // Одиночный тест
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange — подготовка
        var calc = new Calculator();

        // Act — действие
        int result = calc.Add(2, 3);

        // Assert — проверка
        Assert.Equal(5, result);
    }

    [Theory]  // Параметризованный тест
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        var calc = new Calculator();
        Assert.Equal(expected, calc.Add(a, b));
    }
}
```

### Полезные Assert
- `Assert.Equal(expected, actual)`
- `Assert.NotNull(obj)`
- `Assert.Throws<ExceptionType>(() => ...)`
- `Assert.True(condition)` / `Assert.False(condition)`
- `Assert.Contains(item, collection)`"""})

post(973, "code", {
    "language": "csharp",
    "title": "Тестируемый класс Calculator",
    "code": """// Класс, который будем тестировать
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;

    public double Divide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("Делитель не может быть нулём");
        return (double)a / b;
    }
}

// Запуск тестов: dotnet test"""
})

post(973, "mini_task", {
    "language": "csharp",
    "title": "Напиши xUnit [Fact] тест",
    "description": "Напиши xUnit тест для метода Divide класса Calculator. Тест должен проверять что деление 10 на 2 возвращает 5.0, и что деление на ноль выбрасывает DivideByZeroException.",
    "starterCode": """using Xunit;

public class Calculator
{
    public double Divide(int a, int b)
    {
        if (b == 0) throw new DivideByZeroException();
        return (double)a / b;
    }
}

public class CalculatorTests
{
    [Fact]
    public void Divide_TenByTwo_ReturnsFive()
    {
        // Arrange
        var calc = new Calculator();

        // Act
        // вызови calc.Divide(10, 2)

        // Assert
        // проверь что результат равен 5.0
    }

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var calc = new Calculator();
        // Используй Assert.Throws<DivideByZeroException>(() => calc.Divide(10, 0))
    }
}

// Для запуска тестов используется: dotnet test
Console.WriteLine("Tests defined");""",
    "expectedOutput": "Tests defined"
})

post(973, "quiz", {
    "question": "Чем отличается [Fact] от [Theory] в xUnit?",
    "options": [
        "[Fact] выполняется только в Debug, [Theory] — всегда",
        "[Fact] — одиночный тест без параметров, [Theory] — параметризованный тест с несколькими наборами данных через [InlineData]",
        "[Theory] быстрее выполняется чем [Fact]",
        "[Fact] можно использовать только для async методов"
    ],
    "correct": 1,
    "explanation": "[Fact] — обычный тест, который запускается один раз. [Theory] с [InlineData] позволяет запустить один тест с разными наборами входных данных — каждый [InlineData] создаёт отдельный тест-кейс."
})

post(973, "fix_bug", {
    "language": "csharp",
    "buggyCode": """using Xunit;

public class StringHelper
{
    public string Reverse(string input) =>
        new string(input.Reverse().ToArray());
}

public class StringHelperTests
{
    [Fact]
    public void Reverse_HelloString_ReturnsOlleh()
    {
        var helper = new StringHelper();
        string result = helper.Reverse("hello");

        // Баг в Assert: перепутаны expected и actual
        Assert.Equal(result, "olleh");
    }
}""",
    "expectedOutput": "Test passes",
    "hint": "В Assert.Equal первый аргумент — ожидаемое значение (expected), второй — фактическое (actual). Это важно для читаемости сообщений об ошибках."
})

post(973, "checkpoint", {
    "title": "Итог: Unit-тестирование с xUnit",
    "description": "Проверь, что ты освоил ключевые концепции написания юнит-тестов.",
    "tasks": [
        "Знаю разницу между [Fact] и [Theory]/[InlineData]",
        "Использую паттерн Arrange-Act-Assert в каждом тесте",
        "Умею проверять выброс исключений через Assert.Throws<T>",
        "Понимаю порядок аргументов Assert.Equal(expected, actual)",
        "Знаю как запустить тесты через dotnet test"
    ]
})

post(973, "knowledge_check", {
    "topic": "xUnit",
    "question": "Как в xUnit проверить, что метод выбрасывает исключение определённого типа?",
    "options": [
        "Assert.Exception<T>(() => method())",
        "Assert.Throws<T>(() => method())",
        "try { method(); Assert.Fail(); } catch(T) {}",
        "Assert.IsException(typeof(T), () => method())"
    ],
    "correct": 1,
    "explanation": "Assert.Throws<T>(() => ...) — стандартный способ проверки исключений в xUnit. Он возвращает объект исключения, который можно дополнительно проверить (например, Message)."
})

print("\n=== Все блоки добавлены! ===")
