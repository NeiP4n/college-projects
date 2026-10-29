import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 992: goto ─────────────────────────────────────────────────────────────────
print('=== 992: goto ===')

post(992, 'text', {'markdown': """## goto — и когда его избегать

`goto` — оператор безусловного перехода к метке. Существует в C# с первой версии.

```csharp
goto myLabel;
Console.WriteLine("Это не выполнится");
myLabel:
Console.WriteLine("Перешли сюда!");
```

### Единственный легитимный случай — выход из вложенных циклов
```csharp
for (int i = 0; i < 5; i++)
    for (int j = 0; j < 5; j++)
        if (i == 2 && j == 2)
            goto done;
done:
Console.WriteLine("Вышли из двойного цикла");
```

### Почему goto считается плохой практикой?
1. **Спагетти-код** — поток управления становится непредсказуемым
2. **Сложно отлаживать** — непонятно откуда пришли
3. **Есть лучшие альтернативы**: break, return, исключения, флаги, рефакторинг в метод

### Альтернатива через метод
```csharp
// Вместо goto — метод + return
bool FindPair(int[,] grid)
{
    for (int i = 0; i < 5; i++)
        for (int j = 0; j < 5; j++)
            if (grid[i, j] == 0) return true;
    return false;
}
```

> 🚫 Правило: если тянет написать `goto` — рефактори код в отдельный метод."""})

post(992, 'code', {'language': 'csharp', 'title': 'goto vs альтернатива', 'code': """// goto: выход из вложенного цикла (один из редких случаев)
bool found = false;
for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
        if (i + j == 3)
        {
            Console.WriteLine($"goto: нашли i={i}, j={j}");
            goto exitLoops;
        }
exitLoops:

// Лучше: метод с return
static (int i, int j)? FindPair(int target)
{
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
            if (i + j == target) return (i, j);
    return null;
}

var result = FindPair(3);
if (result.HasValue)
    Console.WriteLine($"метод: нашли i={result.Value.i}, j={result.Value.j}");"""})

post(992, 'quiz', {'question': 'В каком случае использование goto в C# наиболее оправдано?', 'options': [
    'Для создания бесконечных циклов',
    'Для выхода сразу из нескольких вложенных циклов, когда рефакторинг в метод невозможен',
    'Для вызова методов без передачи параметров',
    'goto никогда не оправдано — его нет в современном C#'
], 'correct': 1, 'explanation': 'goto существует в C# и иногда используется для выхода из глубоко вложенных циклов. Но в большинстве случаев лучше вынести логику в отдельный метод и использовать return.'})

post(992, 'control_question', {'question': 'Назови лучшую альтернативу goto для выхода из вложенного цикла в C#.', 'answer': 'Вынести тело цикла в отдельный метод и использовать return вместо goto'})

# ── 993: yield return ─────────────────────────────────────────────────────────
print('=== 993: yield return ===')

post(993, 'text', {'markdown': """## yield return — ленивые последовательности

`yield return` позволяет создавать **ленивые (lazy) генераторы** — методы, которые возвращают элементы **по одному**, без создания всей коллекции в памяти.

```csharp
IEnumerable<int> Squares(int n)
{
    for (int i = 1; i <= n; i++)
        yield return i * i;  // каждый раз возвращает следующий элемент
}

foreach (var sq in Squares(5))
    Console.Write(sq + " "); // 1 4 9 16 25
```

### Как это работает?
Компилятор превращает метод с `yield` в **конечный автомат**.
Выполнение приостанавливается на `yield return` и возобновляется при следующем `MoveNext()`.

### yield break — завершение последовательности
```csharp
IEnumerable<int> UpTo(int[] arr, int limit)
{
    foreach (var x in arr)
    {
        if (x > limit) yield break; // стоп!
        yield return x;
    }
}
```

### Когда полезно?
- Бесконечные последовательности (числа Фибоначчи)
- Фильтрация без промежуточного списка
- Ленивое чтение больших файлов построчно
- LINQ использует тот же принцип внутри!"""})

post(993, 'code', {'language': 'csharp', 'title': 'Генератор чисел Фибоначчи', 'code': """static IEnumerable<long> Fibonacci()
{
    long a = 0, b = 1;
    while (true)           // бесконечная последовательность!
    {
        yield return a;
        (a, b) = (b, a + b);
    }
}

// Берём первые 10 чисел Фибоначчи (LINQ + lazy)
foreach (var n in Fibonacci().Take(10))
    Console.Write(n + " ");
// 0 1 1 2 3 5 8 13 21 34"""})

post(993, 'quiz', {'question': 'Что происходит при вызове yield return в методе-генераторе?', 'options': [
    'Метод завершается и возвращает значение навсегда',
    'Выполнение метода приостанавливается, возвращается текущее значение, при следующей итерации продолжится с этого места',
    'Создаётся новый поток для выполнения оставшегося кода',
    'Компилятор заменяет весь метод на List<T> с добавлением элементов'
], 'correct': 1, 'explanation': 'yield return приостанавливает выполнение метода. При следующем вызове MoveNext() (в foreach) выполнение продолжится со следующей строки после yield return.'})

post(993, 'task', {'title': 'Генератор чётных чисел', 'description': 'Напиши метод EvenNumbers(int max), возвращающий чётные числа от 2 до max включительно. Выведи их через пробел.', 'starterCode': """static IEnumerable<int> EvenNumbers(int max)
{
    // используй yield return
}

Console.WriteLine(string.Join(" ", EvenNumbers(10)));
""", 'expectedOutput': '2 4 6 8 10', 'language': 'csharp'})

print('Done!')
