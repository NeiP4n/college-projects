import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

print('=== 989: Кортежи (ValueTuple) ===')

post(989, 'text', {'markdown': """## Кортежи (ValueTuple)

**Кортеж** — лёгкая структура для группировки нескольких значений без создания отдельного класса.

```csharp
var point = (3, 5);               // безымянный кортеж
var person = (Name: "Аня", Age: 20); // именованный
```

### Деструктуризация
```csharp
var (x, y) = (10, 20);
Console.WriteLine(x); // 10
Console.WriteLine(y); // 20
```

### Возврат нескольких значений из метода
```csharp
(int min, int max) MinMax(int[] arr)
    => (arr.Min(), arr.Max());

var (lo, hi) = MinMax(new[] { 3, 1, 4, 1, 5 });
// lo = 1, hi = 5
```

### ValueTuple vs Tuple
| | ValueTuple | Tuple |
|---|---|---|
| Тип | value (struct) | reference (class) |
| Именованные поля | ✅ | ❌ |
| Синтаксис | `(int, string)` | `Tuple<int,string>` |
| Рекомендуется | ✅ | Только для legacy |"""})

post(989, 'code', {'language': 'csharp', 'title': 'Кортежи в действии', 'code': """// Именованный кортеж
var person = (Name: "Максим", Age: 21, Score: 95.5);
Console.WriteLine($"{person.Name}, {person.Age} лет, балл: {person.Score}");

// Деструктуризация
var (name, age, score) = person;
Console.WriteLine($"Имя: {name}");

// Возврат двух значений из метода
static (string greet, int len) Analyze(string s)
    => ($"Привет, {s}!", s.Length);

var (greet, len) = Analyze("Мир");
Console.WriteLine($"{greet} (длина: {len})");"""})

post(989, 'quiz', {'question': 'Как правильно деструктурировать кортеж (10, "hello") в C#?', 'options': [
    'var [x, s] = (10, "hello");',
    'var (x, s) = (10, "hello");',
    '(var x, var s) = {10, "hello"};',
    'int x, string s = (10, "hello");'
], 'correct': 1, 'explanation': 'Деструктуризация кортежа: var (x, s) = (10, "hello"); — скобки слева от =. Это создаёт переменные x=10 и s="hello".'})

post(989, 'task', {'title': 'Метод с кортежем', 'description': 'Напиши метод Divide(int a, int b), возвращающий (int quotient, int remainder). Вызови с 17 и 5, выведи результат как "3 2".', 'starterCode': """static (int quotient, int remainder) Divide(int a, int b)
{
    // твой код
}

var (q, r) = Divide(17, 5);
Console.WriteLine($"{q} {r}");
""", 'expectedOutput': '3 2', 'language': 'csharp'})
print('Done!')
