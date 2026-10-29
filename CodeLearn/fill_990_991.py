import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 990: checked / unchecked ──────────────────────────────────────────────────
print('=== 990: checked / unchecked ===')

post(990, 'text', {'markdown': """## checked / unchecked

По умолчанию арифметическое **переполнение** в C# происходит беззвучно:

```csharp
int max = int.MaxValue; // 2 147 483 647
int result = max + 1;   // -2 147 483 648 — переполнение, НЕТ исключения!
```

### checked — бросает OverflowException
```csharp
int result = checked(int.MaxValue + 1); // OverflowException!
```

Или блок:
```csharp
checked
{
    int x = int.MaxValue;
    int y = x + 1; // OverflowException
}
```

### unchecked — явно разрешает переполнение (по умолчанию)
```csharp
int result = unchecked(int.MaxValue + 1); // -2147483648, нет исключения
```

### Когда использовать?
| Ситуация | Рекомендация |
|----------|-------------|
| Финансовые расчёты | `checked` — переполнение = ошибка |
| Хэширование, битовые операции | `unchecked` — переполнение ожидаемо |
| Производительность (игры) | `unchecked` — без лишних проверок |"""})

post(990, 'code', {'language': 'csharp', 'title': 'checked vs unchecked', 'code': """// Переполнение без checked — тихо "оборачивается"
int a = int.MaxValue;
Console.WriteLine(a + 1);           // -2147483648

// С checked — исключение
try
{
    int b = checked(int.MaxValue + 1);
}
catch (OverflowException)
{
    Console.WriteLine("Переполнение поймано!");
}

// byte тоже переполняется
byte x = 255;
byte y = unchecked((byte)(x + 1));
Console.WriteLine(y); // 0"""})

post(990, 'quiz', {'question': 'Что произойдёт при выполнении: int x = checked(int.MaxValue + 1);', 'options': [
    'x получит значение int.MinValue (-2147483648)',
    'Будет выброшено исключение OverflowException',
    'Компилятор выдаст ошибку при сборке',
    'x получит значение 0'
], 'correct': 1, 'explanation': 'checked заставляет среду выполнения проверять переполнение. При int.MaxValue + 1 выбрасывается OverflowException. Без checked результат тихо "обернулся" бы в int.MinValue.'})

post(990, 'task', {'title': 'Поймай переполнение', 'description': 'Используя checked, поймай OverflowException при переполнении byte. Выведи "overflow".', 'starterCode': """byte b = 255;
try
{
    byte result = checked((byte)(b + 1));
    Console.WriteLine(result);
}
catch (OverflowException)
{
    // твой код
}
""", 'expectedOutput': 'overflow', 'language': 'csharp'})

# ── 991: is и as ──────────────────────────────────────────────────────────────
print('=== 991: is и as ===')

post(991, 'text', {'markdown': """## Операторы is и as

### is — проверка типа
```csharp
object obj = "hello";
if (obj is string s)         // pattern matching: проверка + приведение
    Console.WriteLine(s.Length); // 5
```

Старый способ (до C# 7):
```csharp
if (obj is string)           // только проверка
    Console.WriteLine(((string)obj).Length);
```

### as — мягкое приведение типа
```csharp
object obj = "hello";
string? s = obj as string;   // null если не строка, не бросает исключение
int? n = obj as int?;        // null — obj не int
```

### Сравнение подходов
| | Исключение при неудаче | Null-безопасный | Pattern var |
|---|---|---|---|
| `(T)obj` | ✅ InvalidCastException | ❌ | ❌ |
| `obj as T` | ❌ возвращает null | ✅ | ❌ |
| `obj is T t` | ❌ просто false | ✅ | ✅ |

### Рекомендация
Используй **`is` с pattern** — самый современный и безопасный способ."""})

post(991, 'code', {'language': 'csharp', 'title': 'is и as на практике', 'code': """object[] items = { 42, "hello", 3.14, true, "world" };

foreach (var item in items)
{
    if (item is string s)
        Console.WriteLine($"Строка: {s.ToUpper()}");
    else if (item is int n)
        Console.WriteLine($"Число: {n * 2}");
    else
        Console.WriteLine($"Другое: {item}");
}"""})

post(991, 'quiz', {'question': 'Что вернёт выражение: "hello" as int?', 'options': [
    'Выбросит InvalidCastException',
    'Вернёт 0',
    'Вернёт null (компилятор не разрешит — as работает только со ссылочными типами)',
    'Вернёт default(int) = 0'
], 'correct': 2, 'explanation': 'as работает только с ссылочными типами или nullable. "hello" as int — ошибка компиляции. Нужно "hello" as string, или obj as int? для nullable.'})

post(991, 'task', {'title': 'Подсчёт строк', 'description': 'Пройди по массиву objects и выведи количество элементов-строк.', 'starterCode': """object[] data = { 1, "hello", true, "world", 3.14, "!" };
int count = 0;

foreach (var item in data)
{
    if (item is string)
        count++;
}

Console.WriteLine(count);
""", 'expectedOutput': '3', 'language': 'csharp'})

print('Done!')
