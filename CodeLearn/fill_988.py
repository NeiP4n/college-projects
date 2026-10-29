import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

print('=== 988: Enum ===')

post(988, 'text', {'markdown': """## Enum: перечисления

**Enum** (перечисление) — это тип, который ограничивает переменную набором именованных констант.

```csharp
enum Direction { North, South, East, West }
```

По умолчанию значения — целые числа начиная с 0:
- `North = 0`, `South = 1`, `East = 2`, `West = 3`

### Зачем использовать enum?
- Код становится **читаемым**: `dir = Direction.North` лучше, чем `dir = 0`
- **Защита от ошибок**: нельзя передать неверное значение
- Работает с `switch` как родной

### Пользовательские значения
```csharp
enum HttpStatus
{
    Ok = 200,
    NotFound = 404,
    Error = 500
}
```

### [Flags] — битовые перечисления
Позволяют комбинировать значения через `|`:
```csharp
[Flags]
enum Permissions { Read = 1, Write = 2, Execute = 4 }
var p = Permissions.Read | Permissions.Write; // = 3
```"""})

post(988, 'code', {'language': 'csharp', 'title': 'Работа с enum', 'code': """enum Season { Spring, Summer, Autumn, Winter }

Season current = Season.Summer;
Console.WriteLine(current);          // Summer
Console.WriteLine((int)current);     // 1

// switch с enum
string msg = current switch
{
    Season.Spring => "Весна пришла!",
    Season.Summer => "Жарко!",
    Season.Autumn => "Листопад",
    Season.Winter => "Снег",
    _             => "Неизвестно"
};
Console.WriteLine(msg); // Жарко!

// Парсинг из строки
Season s = Enum.Parse<Season>("Winter");
Console.WriteLine(s); // Winter"""})

post(988, 'quiz', {'question': 'Какое числовое значение будет у первого элемента enum по умолчанию?', 'options': [
    '1',
    '0',
    '-1',
    'Зависит от имени элемента'
], 'correct': 1, 'explanation': 'По умолчанию первый элемент enum = 0, второй = 1 и т.д. Можно задать вручную: enum E { A = 10, B = 20 }'})

post(988, 'task', {'title': 'День недели', 'description': 'Объяви enum DayOfWeekRu с днями (Mon..Sun). Выведи числовое значение среды (Wed).', 'starterCode': """enum DayOfWeekRu { Mon, Tue, Wed, Thu, Fri, Sat, Sun }

// Выведи числовое значение Wed
""", 'expectedOutput': '2', 'language': 'csharp'})
print('Done!')
