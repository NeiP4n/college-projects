import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 999: Ковариантность / контравариантность ───────────────────────────────────
print('=== 999: Ковариантность и контравариантность ===')

post(999, 'text', {'markdown': """## Ковариантность и контравариантность

Эти концепции определяют, как обобщённые типы соотносятся при наследовании.

### Ковариантность (out) — "производить"
Позволяет использовать `IEnumerable<Derived>` там, где ожидается `IEnumerable<Base>`:
```csharp
IEnumerable<string> strings = new List<string> { "a", "b" };
IEnumerable<object> objects = strings; // OK — string : object, IEnumerable<T> ковариантен
```
Работает потому что `IEnumerable<T>` объявлен как `IEnumerable<out T>`.

### Контравариантность (in) — "потреблять"
Позволяет использовать `Action<Base>` там, где ожидается `Action<Derived>`:
```csharp
Action<object> printObj = o => Console.WriteLine(o);
Action<string> printStr = printObj; // OK — Action<T> контравариантен (in T)
printStr("hello");
```

### Своя вариантность в интерфейсах
```csharp
interface IProducer<out T> { T Produce(); }    // out = ковариантен
interface IConsumer<in T>  { void Consume(T item); } // in = контравариантен
```

### Мнемоника
- **out** = только возвращает T → можно присвоить переменной базового типа
- **in** = только принимает T → можно присвоить переменной производного типа""" })

post(999, 'code', {'language': 'csharp', 'title': 'Ковариантность на практике', 'code': """// Ковариантность: IEnumerable<out T>
List<int> ints = new() { 1, 2, 3 };
// IEnumerable<object> objs = ints; // int — struct, не работает для значимых типов

List<string> words = new() { "apple", "banana" };
IEnumerable<object> objs = words; // OK: string → object
foreach (var o in objs)
    Console.WriteLine(o.ToString()!.ToUpper());

// Контравариантность: IComparer, Action
IComparer<object> objCmp = Comparer<object>.Default;
// IComparer<string> — НЕ является IComparer<object> (нет контравариантности у IComparer)

// Func<out TResult> — ковариантен по TResult
Func<string> getString = () => "hello";
Func<object> getObj    = getString; // OK
Console.WriteLine(getObj());"""})

post(999, 'quiz', {'question': 'Какой модификатор в generic-интерфейсе делает тип ковариантным?', 'options': [
    'in',
    'out',
    'ref',
    'covariant'
], 'correct': 1, 'explanation': 'out делает параметр типа ковариантным: interface I<out T>. Это значит T используется только как возвращаемый тип. Позволяет I<Derived> → I<Base>.'})

post(999, 'control_question', {'question': 'Почему нельзя написать List<object> list = new List<string>(), хотя string наследует object?', 'answer': 'List<T> инвариантен — не помечен ни out, ни in. Это защищает от ошибок: если бы можно было, то list.Add(42) добавил бы int в List<string>, нарушив типобезопасность.'})

# ── 1000: Span<T> ─────────────────────────────────────────────────────────────
print('=== 1000: Span<T> и Memory<T> ===')

post(1000, 'text', {'markdown': """## Span<T> и Memory<T>

`Span<T>` — высокопроизводительный тип для работы со срезами памяти **без копирования**.

### Проблема без Span
```csharp
string s = "Hello, World!";
string sub = s.Substring(7, 5); // создаёт НОВУЮ строку — аллокация!
```

### Span — нулевая аллокация
```csharp
ReadOnlySpan<char> span = s.AsSpan(7, 5);
Console.WriteLine(span.ToString()); // "World" — без новой строки!
```

### Что может Span?
- Срезы массивов, строк, стека
- Передача в методы без копирования
- Работа с бинарными данными

```csharp
int[] arr = { 1, 2, 3, 4, 5 };
Span<int> middle = arr.AsSpan(1, 3); // {2, 3, 4} — тот же массив!
middle[0] = 99;
Console.WriteLine(arr[1]); // 99 — изменили оригинал
```

### Memory<T> — для async
`Span<T>` нельзя хранить в классах или использовать в async-методах.
`Memory<T>` — аналог без этих ограничений:
```csharp
Memory<byte> buffer = new byte[1024];
await stream.ReadAsync(buffer);
```

### Ограничения Span
- ref struct — только на стеке
- Нельзя в async/await методах
- Нельзя хранить в полях классов""" })

post(1000, 'code', {'language': 'csharp', 'title': 'Span в действии', 'code': """string csv = "Alice,30,Engineer";
ReadOnlySpan<char> span = csv.AsSpan();

// Разбиваем без аллокаций
int comma1 = span.IndexOf(',');
var name = span[..comma1];
span = span[(comma1 + 1)..];

int comma2 = span.IndexOf(',');
var age = span[..comma2];
var role = span[(comma2 + 1)..];

Console.WriteLine($"Имя: {name}");
Console.WriteLine($"Возраст: {age}");
Console.WriteLine($"Роль: {role}");

// Span над стеком (stackalloc)
Span<int> stackArr = stackalloc int[5] { 1, 2, 3, 4, 5 };
int sum = 0;
foreach (var n in stackArr) sum += n;
Console.WriteLine($"Сумма: {sum}");"""})

post(1000, 'quiz', {'question': 'Чем Span<T> лучше Substring() при работе с подстроками?', 'options': [
    'Span поддерживает Unicode, Substring нет',
    'Span не создаёт новых объектов в куче — работает со срезом существующей памяти',
    'Span работает быстрее за счёт многопоточности',
    'Span автоматически кэширует результаты'
], 'correct': 1, 'explanation': 'Span<T> — это struct, указывающий на существующую память. Substring создаёт новую строку в куче. Span позволяет читать подстроку без аллокации — критично для производительных приложений.'})

post(1000, 'control_question', {'question': 'Почему Span<T> нельзя использовать в async-методах? Что использовать вместо?', 'answer': 'Span<T> — это ref struct, живёт только на стеке. В async-методах управление стеком передаётся между потоками, поэтому ref struct запрещён. Вместо него используй Memory<T> — он хранится в куче и поддерживает async.'})

# ── 1001: ImmutableCollections ────────────────────────────────────────────────
print('=== 1001: ImmutableCollections ===')

post(1001, 'text', {'markdown': """## ImmutableCollections

Пакет `System.Collections.Immutable` предоставляет коллекции, которые **нельзя изменить**.
Каждая операция (Add, Remove) возвращает **новую** коллекцию.

```csharp
using System.Collections.Immutable;

var list = ImmutableList.Create(1, 2, 3);
var list2 = list.Add(4);       // list НЕ изменился!
Console.WriteLine(list.Count);  // 3
Console.WriteLine(list2.Count); // 4
```

### Доступные типы
| Тип | Аналог |
|-----|--------|
| `ImmutableList<T>` | `List<T>` |
| `ImmutableArray<T>` | `T[]` |
| `ImmutableDictionary<K,V>` | `Dictionary<K,V>` |
| `ImmutableHashSet<T>` | `HashSet<T>` |
| `ImmutableStack<T>` | `Stack<T>` |

### Зачем?
1. **Потокобезопасность** — читать можно из любого потока без блокировок
2. **Предсказуемость** — функция не может тайно изменить переданную коллекцию
3. **Функциональный стиль** — неизменяемые данные проще тестировать

### Builder — эффективная сборка
```csharp
var builder = ImmutableList.CreateBuilder<int>();
for (int i = 0; i < 1000; i++) builder.Add(i);
var immutable = builder.ToImmutable(); // O(1), не 1000 копий
```""" })

post(1001, 'code', {'language': 'csharp', 'title': 'ImmutableList и ImmutableDictionary', 'code': """using System.Collections.Immutable;

// ImmutableList
var fruits = ImmutableList.Create("apple", "banana");
var moreFruits = fruits.Add("cherry").Add("date");

Console.WriteLine($"Оригинал: {fruits.Count}");      // 2
Console.WriteLine($"С добавлением: {moreFruits.Count}"); // 4
Console.WriteLine(string.Join(", ", moreFruits));

// ImmutableDictionary
var capitals = ImmutableDictionary.Create<string, string>()
    .Add("Россия", "Москва")
    .Add("Франция", "Париж");

var updated = capitals.SetItem("Германия", "Берлин");
Console.WriteLine(capitals.Count);  // 2 (неизменён)
Console.WriteLine(updated.Count);   // 3"""})

post(1001, 'quiz', {'question': 'Что вернёт ImmutableList<int>.Add(5)?', 'options': [
    'void — список изменяется на месте',
    'Новый ImmutableList<int> с добавленным элементом, исходный не изменился',
    'bool — успешно ли добавлен элемент',
    'ImmutableList<int> — тот же объект, только с новым элементом'
], 'correct': 1, 'explanation': 'Add у ImmutableList возвращает НОВУЮ коллекцию. Исходный список остаётся неизменным. Именно поэтому нужно сохранять результат: var list2 = list.Add(5);'})

post(1001, 'control_question', {'question': 'Когда стоит использовать ImmutableList вместо List? Назови два случая.', 'answer': '1) В многопоточном коде — не нужны блокировки, читать можно из любого потока. 2) Когда нужно гарантировать неизменяемость — функция не сможет случайно изменить переданную коллекцию.'})

# ── 1002: PLINQ ───────────────────────────────────────────────────────────────
print('=== 1002: PLINQ и AsParallel() ===')

post(1002, 'text', {'markdown': """## PLINQ — параллельный LINQ

**PLINQ** (Parallel LINQ) — автоматически распределяет LINQ-запрос по нескольким потокам.

```csharp
var numbers = Enumerable.Range(1, 10_000_000);

// Обычный LINQ — один поток
var sum = numbers.Where(n => n % 2 == 0).Sum();

// PLINQ — несколько потоков
var sumParallel = numbers
    .AsParallel()
    .Where(n => n % 2 == 0)
    .Sum();
```

### Когда PLINQ быстрее?
- Большой объём данных (тысячи элементов)
- Каждая операция затратна по CPU
- Нет побочных эффектов (чистые функции)

### Управление степенью параллелизма
```csharp
.AsParallel().WithDegreeOfParallelism(4) // максимум 4 потока
```

### AsOrdered — сохранить порядок
```csharp
var result = data.AsParallel().AsOrdered()
                 .Select(x => Process(x))
                 .ToList();
```

### Когда НЕ использовать PLINQ?
- Мало элементов (overhead потоков > выигрыш)
- IO-bound операции (лучше async/await)
- Операции с общим состоянием (нужны блокировки → не параллельно)

> 💡 Всегда измеряй производительность! PLINQ не всегда быстрее.""" })

post(1002, 'code', {'language': 'csharp', 'title': 'PLINQ — параллельная обработка', 'code': """using System.Diagnostics;

int[] data = Enumerable.Range(1, 5_000_000).ToArray();

// Обычный LINQ
var sw1 = Stopwatch.StartNew();
long sum1 = data.Where(n => n % 3 == 0).Select(n => (long)n).Sum();
sw1.Stop();

// PLINQ
var sw2 = Stopwatch.StartNew();
long sum2 = data.AsParallel()
                .Where(n => n % 3 == 0)
                .Select(n => (long)n)
                .Sum();
sw2.Stop();

Console.WriteLine($"LINQ:  {sum1,15} за {sw1.ElapsedMilliseconds}мс");
Console.WriteLine($"PLINQ: {sum2,15} за {sw2.ElapsedMilliseconds}мс");
Console.WriteLine(sum1 == sum2 ? "Результаты совпадают" : "ОШИБКА!");"""})

post(1002, 'quiz', {'question': 'Когда PLINQ (.AsParallel()) НЕ даст ускорения или замедлит?', 'options': [
    'При работе с коллекциями больше 1 миллиона элементов',
    'При небольшом объёме данных — накладные расходы на потоки превысят выигрыш',
    'При использовании Where() — он не параллелится',
    'В .NET 8+ PLINQ отключён по умолчанию'
], 'correct': 1, 'explanation': 'Создание потоков — дорогая операция. Для малых коллекций (< ~1000 элементов с быстрыми операциями) PLINQ работает медленнее обычного LINQ. Всегда измеряй!'})

post(1002, 'control_question', {'question': 'Как добавить PLINQ к существующему LINQ-запросу? Напиши пример.', 'answer': 'Вставить .AsParallel() после источника: numbers.AsParallel().Where(...).Select(...).ToList(). Для сохранения порядка добавить .AsOrdered().'})

# ── 1003: ToLookup ────────────────────────────────────────────────────────────
print('=== 1003: ToLookup и группировки ===')

post(1003, 'text', {'markdown': """## ToLookup и группировки

`ToLookup` создаёт **словарь с множественными значениями** — одному ключу соответствует группа элементов.

```csharp
var words = new[] { "apple", "ant", "banana", "bear", "cherry" };
var byLetter = words.ToLookup(w => w[0]);

foreach (var w in byLetter['a'])
    Console.WriteLine(w); // apple, ant
```

### ToLookup vs GroupBy
| | `ToLookup` | `GroupBy` |
|---|---|---|
| Выполнение | Немедленное (eager) | Ленивое (lazy) |
| Обращение | `lookup[key]` | нет прямого доступа |
| Несуществующий ключ | Пустая коллекция | — |
| Повторное использование | Да (как словарь) | Нет (переитерация) |

### Пример — группировка студентов
```csharp
var students = new[] {
    new { Name = "Аня",  Grade = "A" },
    new { Name = "Боря", Grade = "B" },
    new { Name = "Вася", Grade = "A" },
};

var byGrade = students.ToLookup(s => s.Grade);
Console.WriteLine(byGrade["A"].Count()); // 2
```

### Несуществующий ключ — не исключение
```csharp
var empty = byGrade["Z"]; // пустая коллекция, не KeyNotFoundException
Console.WriteLine(empty.Any()); // false
```""" })

post(1003, 'code', {'language': 'csharp', 'title': 'ToLookup — группировка заказов', 'code': """var orders = new[]
{
    (Id: 1, Customer: "Alice", Amount: 100),
    (Id: 2, Customer: "Bob",   Amount: 200),
    (Id: 3, Customer: "Alice", Amount: 150),
    (Id: 4, Customer: "Charlie", Amount: 300),
    (Id: 5, Customer: "Bob",   Amount: 50),
};

var byCustomer = orders.ToLookup(o => o.Customer);

foreach (var group in byCustomer)
{
    var total = group.Sum(o => o.Amount);
    Console.WriteLine($"{group.Key}: {group.Count()} заказа, итого {total}");
}

Console.WriteLine();
// Прямой доступ по ключу
foreach (var o in byCustomer["Alice"])
    Console.WriteLine($"  Alice: заказ #{o.Id} на {o.Amount}");"""})

post(1003, 'quiz', {'question': 'Что вернёт ToLookup при обращении к несуществующему ключу?', 'options': [
    'null',
    'KeyNotFoundException',
    'Пустая коллекция IEnumerable<T>',
    'default(T)'
], 'correct': 2, 'explanation': 'ToLookup безопасен: lookup["неизвестный_ключ"] вернёт пустую коллекцию, а не null или исключение. Это удобнее, чем Dictionary, где нужно проверять ContainsKey.'})

post(1003, 'task', {'title': 'Группировка по длине слова', 'description': 'Сгруппируй слова по длине через ToLookup. Выведи слова длиной 5 через запятую.', 'starterCode': """var words = new[] { "hi", "hello", "world", "hey", "there", "ocean" };
var byLen = words.ToLookup(w => w.Length);

Console.WriteLine(string.Join(", ", byLen[5]));
""", 'expectedOutput': 'hello, world, there, ocean', 'language': 'csharp'})

print('Done!')
