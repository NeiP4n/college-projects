import urllib.request, json

BASE = 'http://localhost:5003/api/builder/lessons'

def post(lid, btype, obj):
    url = f'{BASE}/{lid}/blocks'
    body = json.dumps({'type': btype, 'content': json.dumps(obj, ensure_ascii=False)}).encode('utf-8')
    req = urllib.request.Request(url, data=body, headers={'Content-Type': 'application/json'}, method='POST')
    try:
        with urllib.request.urlopen(req) as r: print(f'  [{r.status}] {btype}')
    except urllib.error.HTTPError as e: print(f'  [ERR {e.code}] {btype}: {e.read().decode()[:100]}')

# ── 994: Extension-методы ─────────────────────────────────────────────────────
print('=== 994: Extension-методы ===')

post(994, 'text', {'markdown': """## Extension-методы

Extension-методы позволяют **добавлять методы к существующим типам** без изменения их кода и без наследования.

### Синтаксис
```csharp
public static class StringExtensions
{
    // первый параметр с this — это расширяемый тип
    public static bool IsPalindrome(this string s)
    {
        var r = new string(s.Reverse().ToArray());
        return s == r;
    }
}

// Использование — как будто метод встроен в string!
Console.WriteLine("radar".IsPalindrome()); // True
Console.WriteLine("hello".IsPalindrome()); // False
```

### Правила
1. Метод должен быть в **статическом классе**
2. Первый параметр — `this T имя`
3. Метод сам должен быть **static**
4. LINQ полностью построен на extension-методах!

### Примеры из BCL
- `string.IsNullOrEmpty(s)` → теперь можно `s.IsNullOrEmpty()` — через расширение
- `IEnumerable<T>.Where()`, `Select()`, `OrderBy()` — все это extension-методы LINQ

### Когда использовать?
- Нельзя изменить исходный класс (sealed, сторонняя библиотека)
- Удобный fluent API
- Утилитарные методы для примитивных типов"""})

post(994, 'code', {'language': 'csharp', 'title': 'Расширения для int и string', 'code': """public static class MyExtensions
{
    public static bool IsEven(this int n) => n % 2 == 0;
    public static string Repeat(this string s, int times) => string.Concat(Enumerable.Repeat(s, times));
    public static string TruncateAt(this string s, int max)
        => s.Length <= max ? s : s[..max] + "...";
}

// Теперь используем как встроенные
Console.WriteLine(42.IsEven());           // True
Console.WriteLine(7.IsEven());            // False
Console.WriteLine("ha".Repeat(3));        // hahaha
Console.WriteLine("Длинный текст".TruncateAt(5)); // Длинн...

// Цепочка extension-методов (fluent style)
var result = "  Hello World  "
    .Trim()
    .ToLower()
    .Replace("world", "C#");
Console.WriteLine(result); // hello c#"""})

post(994, 'quiz', {'question': 'Что обязательно для extension-метода?', 'options': [
    'Он должен быть в классе, унаследованном от расширяемого типа',
    'Первый параметр должен иметь модификатор this, метод и класс должны быть static',
    'Нужно переопределить метод с ключевым словом override',
    'Расширяемый тип должен быть помечен атрибутом [Extensible]'
], 'correct': 1, 'explanation': 'Extension-метод: 1) в static классе, 2) сам static, 3) первый параметр с this T. Тогда его можно вызывать как метод экземпляра типа T.'})

post(994, 'task', {'title': 'Extension для числа', 'description': 'Напиши extension-метод Clamp(this int val, int min, int max), ограничивающий значение диапазоном. Проверь: 15.Clamp(0,10) = 10, (-5).Clamp(0,10) = 0, 5.Clamp(0,10) = 5.', 'starterCode': """public static class IntExtensions
{
    public static int Clamp(this int val, int min, int max)
    {
        // твой код
    }
}

Console.WriteLine(15.Clamp(0, 10));
Console.WriteLine((-5).Clamp(0, 10));
Console.WriteLine(5.Clamp(0, 10));
""", 'expectedOutput': '10\n0\n5', 'language': 'csharp'})

# ── 995: Named аргументы ──────────────────────────────────────────────────────
print('=== 995: Named аргументы и default-значения ===')

post(995, 'text', {'markdown': """## Named аргументы и default-значения

### Параметры по умолчанию
```csharp
void Greet(string name, string greeting = "Привет", bool exclaim = true)
{
    Console.WriteLine($"{greeting}, {name}{(exclaim ? "!" : ".")}");
}

Greet("Аня");                     // Привет, Аня!
Greet("Боря", "Здравствуй");     // Здравствуй, Боря!
Greet("Вася", exclaim: false);   // Привет, Вася.
```

### Named аргументы — передача по имени
Позволяют указать любой параметр в любом порядке:
```csharp
void Send(string to, string subject, string body, bool urgent = false)
{ ... }

// Можно в любом порядке с именами
Send(body: "Текст письма", to: "user@mail.ru", subject: "Тема", urgent: true);
```

### Преимущества named аргументов
1. **Читаемость**: `CreateRect(width: 100, height: 50)` — сразу понятно
2. **Пропуск параметров**: можно передать только нужные optional-параметры
3. **Защита от ошибок**: `Move(x: 10, y: 20)` не перепутаешь с `Move(y: 10, x: 20)`

### Правила default-параметров
- Должны быть **в конце** списка параметров
- Значение должно быть константой времени компиляции
- Нельзя использовать `out`/`ref` с default-значениями"""})

post(995, 'code', {'language': 'csharp', 'title': 'Named и default в действии', 'code': """static string FormatPrice(
    decimal amount,
    string currency = "RUB",
    int decimals = 2,
    bool showPlus = false)
{
    string sign = showPlus && amount > 0 ? "+" : "";
    return $"{sign}{amount.ToString($"F{decimals}")} {currency}";
}

Console.WriteLine(FormatPrice(1234.5m));                          // 1234,50 RUB
Console.WriteLine(FormatPrice(99m, currency: "USD"));            // 99,00 USD
Console.WriteLine(FormatPrice(50m, decimals: 0, showPlus: true)); // +50 RUB
Console.WriteLine(FormatPrice(-20m, "EUR", showPlus: true));     // -20,00 EUR"""})

post(995, 'quiz', {'question': 'Что выведет: void F(int a, int b = 10, int c = 20) => Console.WriteLine(a+b+c); F(1, c: 5);', 'options': [
    '16',
    '6',
    '26',
    'Ошибка компиляции'
], 'correct': 0, 'explanation': 'F(1, c: 5): a=1, b=10 (default), c=5. Итого: 1+10+5=16. Named аргумент c: 5 пропускает b, который берёт значение по умолчанию.'})

post(995, 'task', {'title': 'Функция с named аргументами', 'description': 'Напиши функцию Power(int base, int exp=2). Вызови: Power(3) и Power(exp:3, base:2). Выведи результаты.', 'starterCode': """static int Power(int @base, int exp = 2)
{
    int result = 1;
    for (int i = 0; i < exp; i++) result *= @base;
    return result;
}

Console.WriteLine(Power(3));
Console.WriteLine(Power(exp: 3, base: 2));
""", 'expectedOutput': '9\n8', 'language': 'csharp'})

print('Done!')
