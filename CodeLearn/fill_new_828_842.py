import json, urllib.request, urllib.error

BASE = "http://localhost:5003/api/builder/lessons"

def post(lid, btype, obj):
    url = f"{BASE}/{lid}/blocks"
    body = json.dumps({"type": btype, "content": json.dumps(obj, ensure_ascii=False)}).encode()
    req = urllib.request.Request(url, data=body, headers={"Content-Type":"application/json"}, method="POST")
    try:
        with urllib.request.urlopen(req) as r: st = r.status
    except urllib.error.HTTPError as e: st = e.code; print(f"  ERR {e.code}")
    print(f"  [{st}] {btype}")

# ============================================================
# MODULE 6: ООП: Основы
# ============================================================

# --------------------------------------------------
# Lesson 921: Классы и объекты
# --------------------------------------------------
print("=== Lesson 921: Классы и объекты ===")

post(921, "text", {"markdown": """## Классы и объекты

**Класс** — это шаблон (чертёж), описывающий данные и поведение.
**Объект** — конкретный экземпляр класса, созданный в памяти.

```csharp
class Car {
    public string Brand;
    public int Year;
    public void Drive() => Console.WriteLine($"{Brand} едет!");
}

var car = new Car();   // создаём объект
car.Brand = "Toyota";
car.Year = 2023;
car.Drive();           // Toyota едет!
```

> Класс — это тип. Объект — это значение этого типа.
"""})

post(921, "code", {
    "language": "csharp",
    "title": "Пример: класс Person",
    "code": """class Person {
    public string Name;
    public int Age;

    public void Greet() {
        Console.WriteLine($"Привет, я {Name}, мне {Age} лет.");
    }
}

// Создаём два независимых объекта
var alice = new Person { Name = "Alice", Age = 30 };
var bob   = new Person { Name = "Bob",   Age = 25 };

alice.Greet();  // Привет, я Alice, мне 30 лет.
bob.Greet();    // Привет, я Bob, мне 25 лет."""
})

post(921, "prediction", {
    "language": "csharp",
    "code": """class Counter {
    public int Value = 0;
    public void Increment() { Value++; }
}

var a = new Counter();
var b = a;          // b ссылается на тот же объект!
a.Increment();
b.Increment();
Console.WriteLine(a.Value);""",
    "expectedOutput": "2",
    "hint": "Классы — ссылочные типы. a и b указывают на один объект в куче."
})

post(921, "mini_task", {
    "language": "csharp",
    "title": "Создай класс Rectangle",
    "description": "Реализуй методы Area() и Perimeter(), возвращающие площадь и периметр прямоугольника.",
    "starterCode": """class Rectangle {
    public double Width;
    public double Height;

    public double Area() {
        // реализуй: ширина * высота
        return 0;
    }

    public double Perimeter() {
        // реализуй: 2 * (ширина + высота)
        return 0;
    }
}

var r = new Rectangle { Width = 4, Height = 3 };
Console.WriteLine(r.Area());
Console.WriteLine(r.Perimeter());""",
    "expectedOutput": "12\n14"
})

post(921, "quiz", {
    "question": "Что произойдёт при выполнении: `var x = new MyClass(); var y = x; y.Value = 42;`?",
    "options": [
        "x.Value останется прежним, y.Value станет 42",
        "x.Value тоже станет 42, т.к. оба ссылаются на один объект",
        "Ошибка компиляции",
        "Создадутся два отдельных объекта"
    ],
    "correct": 1,
    "explanation": "Класс — ссылочный тип. Присваивание копирует ссылку, а не объект."
})

post(921, "checkpoint", {
    "title": "Итог: Классы и объекты",
    "description": "Убедись, что понимаешь ключевые концепции.",
    "tasks": [
        "Знаешь разницу между классом (шаблон) и объектом (экземпляр)",
        "Умеешь создавать объекты с помощью new",
        "Понимаешь, что классы — ссылочные типы"
    ]
})

# --------------------------------------------------
# Lesson 922: Поля, свойства, автосвойства
# --------------------------------------------------
print("=== Lesson 922: Поля, свойства, автосвойства ===")

post(922, "text", {"markdown": """## Поля, свойства и автосвойства

**Поле** — переменная, объявленная прямо в классе.
**Свойство** — обёртка над полем с логикой get/set.
**Автосвойство** — компилятор сам создаёт backing field.

| Вид | Синтаксис | Когда использовать |
|-----|-----------|-------------------|
| Поле | `public int x;` | Редко (только private) |
| Свойство | `public int X { get {...} set {...} }` | Нужна валидация |
| Автосвойство | `public int X { get; set; }` | Простые данные |
"""})

post(922, "code", {
    "language": "csharp",
    "title": "Свойство с валидацией vs автосвойство",
    "code": """class Product {
    private decimal _price;

    // Свойство с валидацией
    public decimal Price {
        get => _price;
        set {
            if (value < 0) throw new ArgumentException("Цена не может быть отрицательной");
            _price = value;
        }
    }

    // Автосвойство — компилятор сам создаёт backing field
    public string Name { get; set; } = "Без названия";

    // Только для чтения автосвойство
    public DateTime CreatedAt { get; } = DateTime.Now;
}

var p = new Product { Name = "Ноутбук", Price = 999.99m };
Console.WriteLine($"{p.Name}: {p.Price}");"""
})

post(922, "fix_bug", {
    "language": "csharp",
    "buggyCode": """class Temperature {
    private double _celsius;

    public double Celsius {
        get => _celsius;
        set => _celsius = value;
    }

    // Fahrenheit должен вычисляться, а не храниться отдельно
    public double Fahrenheit {
        get => _celsius;   // BUG: неправильная формула
        set => _celsius = value;
    }
}

var t = new Temperature { Celsius = 100 };
Console.WriteLine(t.Fahrenheit);""",
    "expectedOutput": "212",
    "hint": "Fahrenheit = Celsius * 9/5 + 32. Setter должен конвертировать обратно."
})

post(922, "fill_blank", {
    "template": """class Circle {
    public double Radius { get; ___ }   // только get снаружи
    public double Area => Math.___(Radius) * Radius * Radius;

    public Circle(double r) { Radius = ___; }
}""",
    "blanks": ["private set", "PI", "r"],
    "hint": "private set позволяет менять свойство только внутри класса."
})

post(922, "quiz", {
    "question": "Какой модификатор сделает автосвойство доступным для записи только внутри класса?",
    "options": [
        "public int X { get; readonly set; }",
        "public int X { get; private set; }",
        "public int X { get; internal; }",
        "public int X { get; protected; }"
    ],
    "correct": 1,
    "explanation": "`private set` — стандартный способ разрешить запись только внутри класса."
})

post(922, "knowledge_check", {
    "topic": "Свойства C#",
    "question": "Что такое 'backing field' автосвойства?",
    "options": [
        "Дополнительный метод для валидации",
        "Скрытое приватное поле, которое компилятор создаёт автоматически",
        "Интерфейс, реализуемый автосвойством",
        "Атрибут над свойством"
    ],
    "correct": 1,
    "explanation": "Компилятор генерирует приватное поле вида `<PropertyName>k__BackingField`."
})

# --------------------------------------------------
# Lesson 923: Конструкторы
# --------------------------------------------------
print("=== Lesson 923: Конструкторы ===")

post(923, "text", {"markdown": """## Конструкторы

Конструктор — специальный метод, вызываемый при создании объекта (`new`).

- Имя совпадает с именем класса
- Нет возвращаемого типа
- Можно перегружать (несколько конструкторов)
- `this(...)` вызывает другой конструктор этого же класса
- Если не написать — компилятор добавит конструктор по умолчанию (без параметров)
"""})

post(923, "code", {
    "language": "csharp",
    "title": "Перегрузка конструкторов и this()",
    "code": """class Point {
    public double X { get; }
    public double Y { get; }

    // Основной конструктор
    public Point(double x, double y) {
        X = x; Y = y;
    }

    // Конструктор по умолчанию — делегирует основному через this()
    public Point() : this(0, 0) { }

    // Конструктор копирования
    public Point(Point other) : this(other.X, other.Y) { }

    public override string ToString() => $"({X}, {Y})";
}

var origin = new Point();
var p1 = new Point(3, 4);
var p2 = new Point(p1);
Console.WriteLine(origin);   // (0, 0)
Console.WriteLine(p1);       // (3, 4)
Console.WriteLine(p2);       // (3, 4)"""
})

post(923, "prediction", {
    "language": "csharp",
    "code": """class Logger {
    public string Prefix;
    public Logger() { Prefix = "INFO"; }
    public Logger(string prefix) { Prefix = prefix; }
}

var a = new Logger();
var b = new Logger("WARN");
Console.WriteLine(a.Prefix);
Console.WriteLine(b.Prefix);""",
    "expectedOutput": "INFO\nWARN",
    "hint": "Каждый конструктор инициализирует объект независимо."
})

post(923, "mini_task", {
    "language": "csharp",
    "title": "Конструктор для BankAccount",
    "description": "Добавь конструктор с начальным балансом и метод Deposit. Нельзя создать счёт с отрицательным балансом.",
    "starterCode": """class BankAccount {
    private decimal _balance;

    public BankAccount(decimal initialBalance) {
        // реализуй: проверь что initialBalance >= 0, иначе throw
        // установи _balance
    }

    public void Deposit(decimal amount) {
        // реализуй: прибавь amount к _balance (amount > 0)
    }

    public decimal GetBalance() => _balance;
}

var acc = new BankAccount(1000);
acc.Deposit(500);
Console.WriteLine(acc.GetBalance());""",
    "expectedOutput": "1500"
})

post(923, "drag_order", {
    "pieces": [
        "var obj = new MyClass(42);",
        "class MyClass { public int Value; }",
        "public MyClass(int v) { Value = v; }",
        "Console.WriteLine(obj.Value);"
    ],
    "solution": [1, 2, 0, 3],
    "hint": "Сначала объявляем класс, внутри — конструктор, потом создаём объект и используем."
})

post(923, "quiz", {
    "question": "Что произойдёт, если объявить в классе только конструктор с параметрами?",
    "options": [
        "Компилятор добавит конструктор без параметров автоматически",
        "Конструктор без параметров перестанет быть доступен",
        "Класс нельзя будет использовать",
        "Ошибка компиляции"
    ],
    "correct": 1,
    "explanation": "Компилятор генерирует конструктор по умолчанию только если не объявлен ни один другой."
})

# --------------------------------------------------
# Lesson 924: Модификаторы доступа
# --------------------------------------------------
print("=== Lesson 924: Модификаторы доступа ===")

post(924, "text", {"markdown": """## Модификаторы доступа

| Модификатор | Доступен из |
|-------------|------------|
| `public` | Откуда угодно |
| `private` | Только внутри класса |
| `protected` | Внутри класса и наследников |
| `internal` | В пределах сборки (проекта) |
| `protected internal` | Наследники или та же сборка |
| `private protected` | Наследники в той же сборке |

**Правило наименьших привилегий**: делай как можно более закрытым.
"""})

post(924, "code", {
    "language": "csharp",
    "title": "Инкапсуляция через модификаторы",
    "code": """class Employee {
    private string _name;           // только внутри класса
    private decimal _salary;

    public string Name {            // чтение — публично
        get => _name;
        private set => _name = value;   // запись — только внутри
    }

    public Employee(string name, decimal salary) {
        _name = name;
        _salary = salary;
    }

    public string GetPaySlip() =>   // публичный метод
        $"{_name}: {_salary:C}";

    private decimal CalculateTax() =>   // приватный: деталь реализации
        _salary * 0.13m;
}

var emp = new Employee("Иван", 80000);
Console.WriteLine(emp.GetPaySlip());
// emp._salary — ошибка компиляции: поле приватное"""
})

post(924, "fix_bug", {
    "language": "csharp",
    "buggyCode": """class Vault {
    public int SecretCode = 1234;   // BUG: секрет виден всем!

    public bool TryOpen(int code) {
        return code == SecretCode;
    }
}

var v = new Vault();
Console.WriteLine(v.TryOpen(1234));""",
    "expectedOutput": "True",
    "hint": "SecretCode должен быть private. Метод TryOpen остаётся public."
})

post(924, "quiz", {
    "question": "Какой модификатор используется по умолчанию для членов класса, если не указан явно?",
    "options": ["public", "internal", "private", "protected"],
    "correct": 2,
    "explanation": "Члены класса по умолчанию private. Сам класс по умолчанию internal."
})

post(924, "knowledge_check", {
    "topic": "Инкапсуляция",
    "question": "Почему поля обычно делают private, а не public?",
    "options": [
        "Так быстрее компилируется",
        "Чтобы контролировать доступ и валидировать данные через свойства/методы",
        "Это синтаксическое требование C#",
        "Private поля занимают меньше памяти"
    ],
    "correct": 1,
    "explanation": "Инкапсуляция скрывает детали реализации и защищает данные от некорректных изменений."
})

post(924, "transfer_test", {
    "question": "Ты пишешь библиотеку платежей. Метод `ValidateCard()` — внутренняя проверка. Какой модификатор выбрать?",
    "options": ["public", "private", "protected internal"],
    "correct": 1,
    "explanation": "Внутренняя деталь реализации должна быть private — она не должна быть видна потребителям библиотеки."
})

# --------------------------------------------------
# Lesson 925: Статические члены и Singleton
# --------------------------------------------------
print("=== Lesson 925: Статические члены и Singleton ===")

post(925, "text", {"markdown": """## Статические члены и Singleton

**static** — член принадлежит классу, а не конкретному объекту.

```csharp
class MathHelper {
    public static double PI = 3.14159;
    public static double CircleArea(double r) => PI * r * r;
}
// Вызов без создания объекта:
MathHelper.CircleArea(5);
```

**Singleton** — паттерн, гарантирующий единственный экземпляр класса.
"""})

post(925, "compare_variants", {
    "question": "Какой вариант Singleton правильно работает в многопоточном коде?",
    "variants": [
        {
            "title": "Наивный Singleton (небезопасен)",
            "code": """class NaiveSingleton {
    private static NaiveSingleton _instance;
    private NaiveSingleton() { }

    public static NaiveSingleton Instance {
        get {
            // ПРОБЛЕМА: два потока могут зайти сюда одновременно
            if (_instance == null)
                _instance = new NaiveSingleton();
            return _instance;
        }
    }
}"""
        },
        {
            "title": "Потокобезопасный Singleton (Lazy<T>)",
            "code": """class ThreadSafeSingleton {
    private static readonly Lazy<ThreadSafeSingleton> _lazy =
        new Lazy<ThreadSafeSingleton>(() => new ThreadSafeSingleton());

    private ThreadSafeSingleton() { }

    public static ThreadSafeSingleton Instance => _lazy.Value;
}
// Lazy<T> гарантирует инициализацию ровно один раз,
// даже при одновременном обращении из нескольких потоков."""
        }
    ],
    "correct": 1,
    "explanation": "Lazy<T> использует двойную блокировку внутри и гарантирует потокобезопасность без ручного lock."
})

post(925, "code", {
    "language": "csharp",
    "title": "Статический конструктор и статические поля",
    "code": """class AppConfig {
    public static readonly string AppName;
    public static readonly int MaxRetries;

    // Статический конструктор — вызывается один раз при первом обращении к классу
    static AppConfig() {
        AppName = "CodeLearn";
        MaxRetries = 3;
        Console.WriteLine("Config инициализирован");
    }
}

Console.WriteLine(AppConfig.AppName);
Console.WriteLine(AppConfig.MaxRetries);
// "Config инициализирован" выведется только один раз"""
})

post(925, "mini_task", {
    "language": "csharp",
    "title": "Счётчик экземпляров",
    "description": "Добавь статическое поле Count, которое увеличивается при каждом создании объекта.",
    "starterCode": """class Widget {
    private static int _count = 0;

    public Widget() {
        // реализуй: увеличь _count на 1
    }

    public static int GetCount() {
        // реализуй: верни _count
        return 0;
    }
}

new Widget();
new Widget();
new Widget();
Console.WriteLine(Widget.GetCount());""",
    "expectedOutput": "3"
})

post(925, "quiz", {
    "question": "Можно ли в статическом методе обращаться к нестатическим полям класса?",
    "options": [
        "Да, всегда",
        "Нет, нужен экземпляр объекта",
        "Только если поле public",
        "Только с ключевым словом this"
    ],
    "correct": 1,
    "explanation": "Статический метод не имеет доступа к this, поэтому не может обращаться к нестатическим членам."
})

# --------------------------------------------------
# Lesson 926: Перегрузка операторов
# --------------------------------------------------
print("=== Lesson 926: Перегрузка операторов ===")

post(926, "text", {"markdown": """## Перегрузка операторов

C# позволяет определить поведение операторов (`+`, `-`, `==`, `>` и т.д.) для своих классов.

```csharp
public static Vector operator +(Vector a, Vector b)
    => new Vector(a.X + b.X, a.Y + b.Y);
```

**Правила:**
- Метод `static` и помечен `operator`
- Для `==` нужно перегрузить и `!=`
- Рекомендуется также переопределить `Equals` и `GetHashCode`
"""})

post(926, "code", {
    "language": "csharp",
    "title": "Вектор с перегруженными операторами",
    "code": """class Vector {
    public double X { get; }
    public double Y { get; }
    public Vector(double x, double y) { X = x; Y = y; }

    public static Vector operator +(Vector a, Vector b)
        => new Vector(a.X + b.X, a.Y + b.Y);

    public static Vector operator *(Vector v, double scalar)
        => new Vector(v.X * scalar, v.Y * scalar);

    public static bool operator ==(Vector a, Vector b)
        => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(Vector a, Vector b) => !(a == b);

    public override string ToString() => $"({X}, {Y})";
}

var v1 = new Vector(1, 2);
var v2 = new Vector(3, 4);
Console.WriteLine(v1 + v2);      // (4, 6)
Console.WriteLine(v1 * 3);       // (3, 6)
Console.WriteLine(v1 == v2);     // False"""
})

post(926, "prediction", {
    "language": "csharp",
    "code": """class Money {
    public decimal Amount { get; }
    public Money(decimal amount) { Amount = amount; }

    public static Money operator +(Money a, Money b)
        => new Money(a.Amount + b.Amount);

    public override string ToString() => $"{Amount:F2}";
}

var m1 = new Money(10.50m);
var m2 = new Money(5.25m);
Console.WriteLine(m1 + m2);""",
    "expectedOutput": "15.75",
    "hint": "Оператор + складывает Amount и возвращает новый объект Money."
})

post(926, "fix_bug", {
    "language": "csharp",
    "buggyCode": """class Fraction {
    public int Num { get; }
    public int Den { get; }
    public Fraction(int n, int d) { Num = n; Den = d; }

    // BUG: умножение дробей реализовано неправильно
    public static Fraction operator *(Fraction a, Fraction b)
        => new Fraction(a.Num + b.Num, a.Den + b.Den);

    public override string ToString() => $"{Num}/{Den}";
}

var f1 = new Fraction(1, 2);
var f2 = new Fraction(2, 3);
Console.WriteLine(f1 * f2);""",
    "expectedOutput": "2/6",
    "hint": "Умножение дробей: числители перемножаются, знаменатели перемножаются."
})

post(926, "quiz", {
    "question": "Какое требование обязательно при перегрузке оператора `==`?",
    "options": [
        "Также перегрузить оператор `>`",
        "Также перегрузить оператор `!=`",
        "Объявить метод как virtual",
        "Вернуть тип bool через implicit"
    ],
    "correct": 1,
    "explanation": "C# требует, чтобы `==` и `!=` перегружались парно."
})

post(926, "mini_task", {
    "language": "csharp",
    "title": "Перегрузи оператор сравнения для Temperature",
    "description": "Реализуй операторы > и < для класса Temperature, сравнивающего значения в градусах Цельсия.",
    "starterCode": """class Temperature {
    public double Celsius { get; }
    public Temperature(double c) { Celsius = c; }

    public static bool operator >(Temperature a, Temperature b) {
        // реализуй: a.Celsius > b.Celsius
        return false;
    }

    public static bool operator <(Temperature a, Temperature b) {
        // реализуй: a.Celsius < b.Celsius
        return false;
    }
}

var t1 = new Temperature(100);
var t2 = new Temperature(20);
Console.WriteLine(t1 > t2);
Console.WriteLine(t1 < t2);""",
    "expectedOutput": "True\nFalse"
})

# --------------------------------------------------
# Lesson 927: Record-типы
# --------------------------------------------------
print("=== Lesson 927: Record-типы ===")

post(927, "text", {"markdown": """## Record-типы (C# 9+)

`record` — специальный вид класса, оптимизированный для **неизменяемых данных**.

Автоматически получает:
- `Equals` и `==` по значению (не по ссылке)
- `ToString()` с именами и значениями свойств
- Деконструктор
- `with`-выражение для создания копии с изменениями
"""})

post(927, "semantic_diff", {
    "intendedCode": """// Что мы хотим: тип данных с сравнением по значению
class PointClass {
    public int X { get; init; }
    public int Y { get; init; }

    public override bool Equals(object? obj)
        => obj is PointClass p && p.X == X && p.Y == Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"PointClass {{ X = {X}, Y = {Y} }}";
}
// 15+ строк кода для базовой функциональности""",
    "actualCode": """// Record делает всё это автоматически в одну строку:
record PointRecord(int X, int Y);

var p1 = new PointRecord(1, 2);
var p2 = new PointRecord(1, 2);
Console.WriteLine(p1 == p2);       // True  (значение, не ссылка)
Console.WriteLine(p1);              // PointRecord { X = 1, Y = 2 }

var p3 = p1 with { Y = 99 };       // копия с изменённым Y
Console.WriteLine(p3);              // PointRecord { X = 1, Y = 99 }""",
    "explanation": "Record заменяет ~15 строк шаблонного кода одной строкой, сохраняя семантику равенства по значению."
})

post(927, "code", {
    "language": "csharp",
    "title": "Record struct и позиционный синтаксис",
    "code": """// Позиционный record — компилятор создаёт свойства автоматически
record Person(string Name, int Age);

// record struct — значимый тип (стек)
record struct Point(double X, double Y);

var alice = new Person("Alice", 30);
var alice2 = alice with { Age = 31 };

Console.WriteLine(alice);    // Person { Name = Alice, Age = 30 }
Console.WriteLine(alice2);   // Person { Name = Alice, Age = 31 }

// Деконструкция
var (name, age) = alice;
Console.WriteLine($"{name} - {age}");"""
})

post(927, "prediction", {
    "language": "csharp",
    "code": """record Color(int R, int G, int B);

var red  = new Color(255, 0, 0);
var red2 = new Color(255, 0, 0);
var blue = new Color(0, 0, 255);

Console.WriteLine(red == red2);
Console.WriteLine(red == blue);
Console.WriteLine(red);""",
    "expectedOutput": "True\nFalse\nColor { R = 255, G = 0, B = 0 }",
    "hint": "Record сравнивает по значению всех свойств, ToString() включает имена и значения."
})

post(927, "quiz", {
    "question": "Что делает выражение `var b = a with { X = 10 }` для record?",
    "options": [
        "Изменяет свойство X объекта a",
        "Создаёт новый record, копируя все поля a, но с X = 10",
        "Создаёт псевдоним b для объекта a",
        "Ошибка: record неизменяем"
    ],
    "correct": 1,
    "explanation": "Выражение `with` создаёт новый экземпляр с изменёнными полями. Оригинал a не меняется."
})

post(927, "transfer_test", {
    "question": "Тебе нужен тип для представления координат GPS (широта, долгота). Значения после создания не меняются. Что лучше использовать?",
    "options": [
        "Обычный class с public полями",
        "record с позиционным синтаксисом",
        "static class"
    ],
    "correct": 1,
    "explanation": "Record идеален для неизменяемых данных — автоматически даёт equals по значению, ToString, деконструктор."
})

# --------------------------------------------------
# Lesson 928: Struct vs Class
# --------------------------------------------------
print("=== Lesson 928: Struct vs Class ===")

post(928, "text", {"markdown": """## Struct vs Class

| Характеристика | class | struct |
|---------------|-------|--------|
| Тип | Ссылочный (куча) | Значимый (стек) |
| Присваивание | Копирует ссылку | Копирует данные |
| null | Может быть null | Не может (без ?) |
| Наследование | Поддерживает | Не поддерживает |
| Интерфейсы | Да | Да |
| Подходит для | Сложные объекты | Маленькие данные |

**Правило**: struct для маленьких (<16 байт), неизменяемых, часто копируемых данных.
"""})

post(928, "prediction", {
    "language": "csharp",
    "code": """struct Point { public int X, Y; }
class PointC { public int X, Y; }

var ps = new Point { X = 1, Y = 2 };
var ps2 = ps;   // копия значения
ps2.X = 99;

var pc = new PointC { X = 1, Y = 2 };
var pc2 = pc;   // копия ссылки
pc2.X = 99;

Console.WriteLine(ps.X);
Console.WriteLine(pc.X);""",
    "expectedOutput": "1\n99",
    "hint": "Struct копирует данные, class копирует ссылку. Изменение ps2 не затрагивает ps, но изменение pc2 затрагивает pc."
})

post(928, "compare_variants", {
    "question": "Для хранения RGB-цвета (3 байта данных, часто передаётся в методы). Что выбрать?",
    "variants": [
        {
            "title": "struct Color",
            "code": """struct Color {
    public byte R, G, B;
    public Color(byte r, byte g, byte b) { R = r; G = g; B = b; }

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";
}
// 3 байта, копируется по значению, без аллокации в куче"""
        },
        {
            "title": "class Color",
            "code": """class Color {
    public byte R, G, B;
    public Color(byte r, byte g, byte b) { R = r; G = g; B = b; }

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";
}
// Аллокация в куче, давление на GC при большом количестве объектов"""
        }
    ],
    "correct": 0,
    "explanation": "Struct лучше для маленьких данных, которые часто создаются и передаются. Нет давления на GC."
})

post(928, "fix_bug", {
    "language": "csharp",
    "buggyCode": """struct Counter {
    public int Value;
    public void Increment() { Value++; }   // BUG: не работает как ожидается
}

var c = new Counter();
c.Increment();
c.Increment();
Console.WriteLine(c.Value);""",
    "expectedOutput": "2",
    "hint": "Метод получает копию struct. Нужно вернуть новое значение или использовать ref. Попробуй c.Value++ напрямую или перепиши Increment с ref."
})

post(928, "mini_task", {
    "language": "csharp",
    "title": "Struct для дроби",
    "description": "Создай struct Fraction с полями Numerator и Denominator и методом ToDouble().",
    "starterCode": """struct Fraction {
    public int Numerator;
    public int Denominator;

    public Fraction(int num, int den) {
        // реализуй: присвой поля
        Numerator = 0;
        Denominator = 1;
    }

    public double ToDouble() {
        // реализуй: верни числитель, делённый на знаменатель
        return 0;
    }
}

var f = new Fraction(1, 4);
Console.WriteLine(f.ToDouble());""",
    "expectedOutput": "0.25"
})

post(928, "knowledge_check", {
    "topic": "Struct vs Class",
    "question": "Почему struct не поддерживает наследование (от других struct/class)?",
    "options": [
        "Это ограничение CLR для значимых типов",
        "Struct слишком маленькие для vtable",
        "Потому что struct неизменяемые",
        "Ограничение только C#, не CLR"
    ],
    "correct": 0,
    "explanation": "CLR не поддерживает наследование для значимых типов (кроме реализации интерфейсов). Это фундаментальное ограничение рантайма."
})

# ============================================================
# MODULE 7: ООП: Наследование
# ============================================================

# --------------------------------------------------
# Lesson 929: Наследование и base
# --------------------------------------------------
print("=== Lesson 929: Наследование и base ===")

post(929, "text", {"markdown": """## Наследование и base

Наследование позволяет создать новый класс на основе существующего, **расширив** его поведение.

```csharp
class Animal {          // базовый класс
    public string Name { get; set; }
    public void Breathe() => Console.WriteLine("...");
}

class Dog : Animal {    // производный класс
    public void Bark() => Console.WriteLine("Гав!");
}
```

- `base.Method()` — вызвать реализацию базового класса
- `base(args)` — вызвать конструктор базового класса
- Конструктор базового класса вызывается **первым**
"""})

post(929, "code", {
    "language": "csharp",
    "title": "Цепочка конструкторов с base()",
    "code": """class Vehicle {
    public string Brand { get; }
    public int Year { get; }

    public Vehicle(string brand, int year) {
        Brand = brand;
        Year = year;
        Console.WriteLine($"Vehicle: {brand} {year}");
    }
}

class Car : Vehicle {
    public int Doors { get; }

    public Car(string brand, int year, int doors)
        : base(brand, year) {   // вызываем конструктор Vehicle
        Doors = doors;
        Console.WriteLine($"Car: {doors} дверей");
    }
}

var car = new Car("Toyota", 2023, 4);
// Вывод:
// Vehicle: Toyota 2023
// Car: 4 дверей"""
})

post(929, "prediction", {
    "language": "csharp",
    "code": """class A {
    public A() { Console.WriteLine("A()"); }
}
class B : A {
    public B() { Console.WriteLine("B()"); }
}
class C : B {
    public C() { Console.WriteLine("C()"); }
}

new C();""",
    "expectedOutput": "A()\nB()\nC()",
    "hint": "Конструкторы вызываются от базового к производному: сначала A, потом B, потом C."
})

post(929, "mini_task", {
    "language": "csharp",
    "title": "Расширь класс Shape",
    "description": "Создай класс Circle, наследующий Shape. Вызови base конструктор и реализуй GetInfo().",
    "starterCode": """class Shape {
    public string Color { get; }
    public Shape(string color) { Color = color; }
    public virtual string GetInfo() => $"Shape [{Color}]";
}

class Circle : Shape {
    public double Radius { get; }

    public Circle(string color, double radius)
        : base(/* вызови конструктор Shape с color */) {
        // реализуй: присвой Radius
        Radius = 0;
    }

    public override string GetInfo() {
        // реализуй: верни base.GetInfo() + " radius=" + Radius
        return "";
    }
}

var c = new Circle("red", 5);
Console.WriteLine(c.GetInfo());""",
    "expectedOutput": "Shape [red] radius=5"
})

post(929, "quiz", {
    "question": "Что происходит, если производный класс не вызывает `base()` явно?",
    "options": [
        "Ошибка компиляции",
        "Автоматически вызывается конструктор base без параметров",
        "Конструктор base не вызывается вообще",
        "Вызывается последний объявленный конструктор base"
    ],
    "correct": 1,
    "explanation": "Компилятор автоматически вставляет base(). Если конструктора без параметров нет в базовом классе — ошибка компиляции."
})

post(929, "counterfactual", {
    "code": """class Animal {
    public string Name;
    public Animal(string name) { Name = name; }
    public string Describe() => $"Я {Name}";
}
class Dog : Animal {
    public Dog(string name) : base(name) { }
}
var d = new Dog("Бобик");
Console.WriteLine(d.Describe());""",
    "removeLine": 9,
    "question": "Что произойдёт, если убрать `: base(name)` из конструктора Dog?",
    "answer": "Ошибка компиляции: класс Animal не имеет конструктора без параметров. Dog обязан явно вызвать base(name)."
})

# --------------------------------------------------
# Lesson 930: virtual, override, abstract
# --------------------------------------------------
print("=== Lesson 930: virtual, override, abstract ===")

post(930, "text", {"markdown": """## virtual, override, abstract

| Ключевое слово | Где | Что означает |
|---------------|-----|-------------|
| `virtual` | Базовый класс | Метод можно переопределить |
| `override` | Производный класс | Переопределяет virtual/abstract |
| `abstract` | Абстрактный метод | Нет реализации, наследник **обязан** переопределить |

**Без `virtual`** — переопределения нет, есть только сокрытие (`new`).
"""})

post(930, "prediction", {
    "language": "csharp",
    "code": """class Animal {
    public virtual string Sound() => "...";
}
class Cat : Animal {
    public override string Sound() => "Мяу";
}
class Dog : Animal {
    public override string Sound() => "Гав";
}

Animal[] animals = { new Cat(), new Dog(), new Animal() };
foreach (var a in animals)
    Console.WriteLine(a.Sound());""",
    "expectedOutput": "Мяу\nГав\n...",
    "hint": "virtual + override = полиморфный вызов. Тип переменной Animal, но вызывается реализация реального объекта."
})

post(930, "code", {
    "language": "csharp",
    "title": "abstract метод — обязательное переопределение",
    "code": """abstract class Shape {
    public abstract double Area();       // без реализации — наследник ОБЯЗАН override
    public virtual string Describe() =>  // с реализацией — наследник МОЖЕТ override
        $"Площадь: {Area():F2}";
}

class Square : Shape {
    public double Side { get; }
    public Square(double side) { Side = side; }
    public override double Area() => Side * Side;
}

class Triangle : Shape {
    public double Base, Height;
    public override double Area() => 0.5 * Base * Height;
}

Shape s = new Square(4);
Console.WriteLine(s.Describe());  // Площадь: 16.00"""
})

post(930, "fix_bug", {
    "language": "csharp",
    "buggyCode": """class Logger {
    public virtual void Log(string msg) {
        Console.WriteLine($"[LOG] {msg}");
    }
}
class FileLogger : Logger {
    // BUG: пропущено ключевое слово override
    public void Log(string msg) {
        Console.WriteLine($"[FILE] {msg}");
    }
}

Logger logger = new FileLogger();
logger.Log("test");""",
    "expectedOutput": "[FILE] test",
    "hint": "Без override происходит сокрытие (hiding), а не переопределение. При вызове через переменную Logger вызывается базовая версия."
})

post(930, "fill_blank", {
    "template": """abstract class Validator {
    public ___ bool IsValid(string input);  // должен быть переопределён
    public ___ string GetError() =>         // может быть переопределён
        "Неверный ввод";
}
class EmailValidator : Validator {
    public ___ bool IsValid(string input) =>
        input.Contains("@");
}""",
    "blanks": ["abstract", "virtual", "override"],
    "hint": "abstract — нет тела, virtual — есть тело но можно переопределить, override — переопределяем."
})

post(930, "quiz", {
    "question": "Можно ли объявить метод `override` без `virtual` в базовом классе?",
    "options": [
        "Да, override работает с любым методом",
        "Нет, можно только если базовый метод virtual или abstract",
        "Да, но только если базовый класс abstract",
        "Нет, нужно ещё sealed"
    ],
    "correct": 1,
    "explanation": "override требует, чтобы метод в базовом классе был помечен virtual, abstract или уже override."
})

# --------------------------------------------------
# Lesson 931: Абстрактные классы
# --------------------------------------------------
print("=== Lesson 931: Абстрактные классы ===")

post(931, "text", {"markdown": """## Абстрактные классы

**Абстрактный класс** — класс с ключевым словом `abstract`:
- Нельзя создать напрямую (`new AbstractClass()` — ошибка)
- Может содержать абстрактные методы (без тела)
- Может содержать обычные методы с реализацией
- Используется как **шаблон** для производных классов

```csharp
abstract class Document {
    public string Title { get; set; }
    public abstract void Print();      // обязателен к реализации
    public void Open() => Console.WriteLine($"Открыт: {Title}"); // есть реализация
}
```
"""})

post(931, "code", {
    "language": "csharp",
    "title": "Абстрактный класс для геометрических фигур",
    "code": """abstract class Shape {
    public string Name { get; protected set; }

    // Абстрактные методы — наследник ОБЯЗАН реализовать
    public abstract double Area();
    public abstract double Perimeter();

    // Шаблонный метод — использует абстрактные методы
    public void PrintStats() {
        Console.WriteLine($"{Name}: площадь={Area():F2}, периметр={Perimeter():F2}");
    }
}

class Circle : Shape {
    private double _r;
    public Circle(double r) { _r = r; Name = "Круг"; }
    public override double Area() => Math.PI * _r * _r;
    public override double Perimeter() => 2 * Math.PI * _r;
}

class Rectangle : Shape {
    private double _w, _h;
    public Rectangle(double w, double h) { _w = w; _h = h; Name = "Прямоугольник"; }
    public override double Area() => _w * _h;
    public override double Perimeter() => 2 * (_w + _h);
}

var shapes = new Shape[] { new Circle(5), new Rectangle(4, 3) };
foreach (var s in shapes) s.PrintStats();"""
})

post(931, "prediction", {
    "language": "csharp",
    "code": """abstract class Base {
    public abstract int GetValue();
    public int Double() => GetValue() * 2;
}
class Derived : Base {
    public override int GetValue() => 7;
}

var d = new Derived();
Console.WriteLine(d.Double());""",
    "expectedOutput": "14",
    "hint": "Double() вызывает GetValue() полиморфно — используется реализация из Derived."
})

post(931, "mini_task", {
    "language": "csharp",
    "title": "Реализуй абстрактный класс Animal",
    "description": "Создай Cat и Dog, реализующих абстрактный метод Speak(). Метод Introduce() определён в базовом классе.",
    "starterCode": """abstract class Animal {
    public string Name { get; }
    public Animal(string name) { Name = name; }
    public abstract string Speak();
    public void Introduce() =>
        Console.WriteLine($"Я {Name} и говорю: {Speak()}");
}

class Cat : Animal {
    public Cat(string name) : base(name) { }
    public override string Speak() {
        // реализуй: верни "Мяу"
        return "";
    }
}

class Dog : Animal {
    public Dog(string name) : base(name) { }
    public override string Speak() {
        // реализуй: верни "Гав"
        return "";
    }
}

new Cat("Мурка").Introduce();
new Dog("Бобик").Introduce();""",
    "expectedOutput": "Я Мурка и говорю: Мяу\nЯ Бобик и говорю: Гав"
})

post(931, "transfer_test", {
    "question": "Тебе нужно создать систему плагинов, где каждый плагин обязан реализовать методы Initialize() и Execute(). Общий метод Run() вызывает их в нужном порядке. Что использовать?",
    "options": [
        "Обычный класс с virtual методами",
        "Абстрактный класс с abstract Initialize() и Execute(), и конкретным Run()",
        "Интерфейс с методами Initialize, Execute и Run"
    ],
    "correct": 1,
    "explanation": "Абстрактный класс позволяет: (1) запретить создание экземпляра базового класса, (2) обязать реализацию abstract методов, (3) предоставить конкретный Run() с общей логикой."
})

# --------------------------------------------------
# Lesson 932: Интерфейсы
# --------------------------------------------------
print("=== Lesson 932: Интерфейсы ===")

post(932, "text", {"markdown": """## Интерфейсы

**Интерфейс** — контракт: набор методов/свойств без реализации (или с default-реализацией в C# 8+).

- `interface IName { ... }` — все члены публичные по умолчанию
- Класс **реализует** (`implement`) интерфейс, не наследует
- Один класс может реализовывать **несколько** интерфейсов
- Интерфейс не содержит полей (до C# 11)

```csharp
interface ISaveable {
    void Save();
    bool Load(string path);
}
```
"""})

post(932, "two_path", {
    "question": "У тебя есть несколько классов (Robot, NPC, Player), которым нужен метод Move(). Что выбрать?",
    "pathA": {
        "title": "Абстрактный класс MovableEntity",
        "code": """abstract class MovableEntity {
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public abstract void Move(int dx, int dy);
    // Можно добавить общие поля и методы
}
class Robot : MovableEntity {
    public override void Move(int dx, int dy) {
        X += dx; Y += dy;
        Console.WriteLine($"Robot -> ({X},{Y})");
    }
}
// НО: класс может наследовать только один abstract class!
// Если Robot уже наследует HardwareDevice — не получится."""
    },
    "pathB": {
        "title": "Интерфейс IMovable",
        "code": """interface IMovable {
    void Move(int dx, int dy);
}
// Класс может реализовывать несколько интерфейсов одновременно:
class Robot : HardwareDevice, IMovable, IDamageable {
    public void Move(int dx, int dy) {
        Console.WriteLine($"Robot moves by ({dx},{dy})");
    }
}
// Гибко: любой несвязанный класс может реализовать IMovable"""
    },
    "explanation": "Интерфейс предпочтителен когда: (1) классы из разных иерархий должны разделять поведение, (2) хочешь множественный «контракт». Абстрактный класс лучше когда есть общие данные/реализация и одна иерархия."
})

post(932, "code", {
    "language": "csharp",
    "title": "Реализация нескольких интерфейсов",
    "code": """interface IPrintable { void Print(); }
interface ISaveable   { void Save(string path); }

class Report : IPrintable, ISaveable {
    public string Title { get; set; }

    public void Print() =>
        Console.WriteLine($"Печать: {Title}");

    public void Save(string path) =>
        Console.WriteLine($"Сохранено в {path}");
}

// Полиморфизм через интерфейс:
IPrintable p = new Report { Title = "Годовой отчёт" };
p.Print();   // Печать: Годовой отчёт"""
})

post(932, "mini_task", {
    "language": "csharp",
    "title": "Реализуй интерфейс IComparable",
    "description": "Реализуй интерфейс IComparable<Product> в классе Product, сравнивая по Price.",
    "starterCode": """class Product : IComparable<Product> {
    public string Name { get; }
    public decimal Price { get; }

    public Product(string name, decimal price) {
        Name = name; Price = price;
    }

    public int CompareTo(Product other) {
        // реализуй: сравни this.Price с other.Price
        // возврати <0, 0 или >0
        return 0;
    }
}

var products = new[] {
    new Product("B", 30m),
    new Product("A", 10m),
    new Product("C", 20m)
};
Array.Sort(products);
foreach (var p in products)
    Console.WriteLine(p.Name);""",
    "expectedOutput": "A\nC\nB"
})

post(932, "quiz", {
    "question": "Сколько интерфейсов может реализовывать один класс в C#?",
    "options": ["Только один", "Не более двух", "Не более восьми", "Сколько угодно"],
    "correct": 3,
    "explanation": "C# поддерживает реализацию множества интерфейсов одновременно, ограничений нет."
})

# --------------------------------------------------
# Lesson 933: Полиморфизм
# --------------------------------------------------
print("=== Lesson 933: Полиморфизм ===")

post(933, "text", {"markdown": """## Полиморфизм

**Полиморфизм** — способность обращаться с объектами разных типов единообразно через общий базовый тип или интерфейс.

Вместо:
```csharp
if (animal is Cat) ((Cat)animal).Meow();
else if (animal is Dog) ((Dog)animal).Bark();
```

Делаем:
```csharp
animal.Speak();  // каждый знает как говорить сам
```

Ключ: `virtual` + `override` (или интерфейс), переменная базового типа.
"""})

post(933, "constraint", {
    "language": "csharp",
    "title": "Калькулятор без if/switch",
    "description": "Реализуй операции Add, Subtract, Multiply через полиморфизм. Нельзя использовать if, switch или тернарный оператор.",
    "constraint": "без if, switch, тернарного оператора — только полиморфизм",
    "starterCode": """abstract class Operation {
    public abstract double Execute(double a, double b);
}

class Add : Operation {
    public override double Execute(double a, double b) {
        // реализуй: a + b
        return 0;
    }
}

class Subtract : Operation {
    public override double Execute(double a, double b) {
        // реализуй: a - b
        return 0;
    }
}

class Multiply : Operation {
    public override double Execute(double a, double b) {
        // реализуй: a * b
        return 0;
    }
}

Operation[] ops = { new Add(), new Subtract(), new Multiply() };
foreach (var op in ops)
    Console.WriteLine(op.Execute(10, 3));""",
    "expectedOutput": "13\n7\n30",
    "hint": "Каждый класс знает свою операцию. Цикл вызывает Execute() полиморфно."
})

post(933, "code", {
    "language": "csharp",
    "title": "Полиморфизм через интерфейс IRenderer",
    "code": """interface IRenderer {
    void Render();
}

class HtmlRenderer : IRenderer {
    public void Render() => Console.WriteLine("<p>HTML</p>");
}

class JsonRenderer : IRenderer {
    public void Render() => Console.WriteLine("{ \"type\": \"json\" }");
}

class PdfRenderer : IRenderer {
    public void Render() => Console.WriteLine("[PDF binary]");
}

// Полиморфный код не знает о конкретных классах:
void RenderAll(IRenderer[] renderers) {
    foreach (var r in renderers) r.Render();
}

RenderAll(new IRenderer[] {
    new HtmlRenderer(), new JsonRenderer(), new PdfRenderer()
});"""
})

post(933, "prediction", {
    "language": "csharp",
    "code": """class Base {
    public virtual string Who() => "Base";
}
class Mid : Base {
    public override string Who() => "Mid";
}
class Top : Mid {
    public override string Who() => "Top";
}

Base obj = new Top();
Console.WriteLine(obj.Who());""",
    "expectedOutput": "Top",
    "hint": "Полиморфный вызов всегда выбирает реализацию самого производного типа."
})

post(933, "mutation", {
    "language": "csharp",
    "baseCode": """abstract class Discount {
    public abstract decimal Apply(decimal price);
}
class PercentDiscount : Discount {
    private decimal _pct;
    public PercentDiscount(decimal pct) { _pct = pct; }
    public override decimal Apply(decimal price) => price * (1 - _pct / 100);
}
class FixedDiscount : Discount {
    private decimal _amount;
    public FixedDiscount(decimal amount) { _amount = amount; }
    public override decimal Apply(decimal price) => price - _amount;
}
Discount d = new PercentDiscount(10);
Console.WriteLine(d.Apply(200));""",
    "mutation": "Замени PercentDiscount на FixedDiscount(50) и обнови вывод",
    "expectedOutput": "150",
    "hint": "Полиморфизм позволяет заменить реализацию не меняя вызывающий код."
})

# --------------------------------------------------
# Lesson 934: sealed и new
# --------------------------------------------------
print("=== Lesson 934: sealed и new ===")

post(934, "text", {"markdown": """## sealed и new

### sealed
- `sealed class` — нельзя наследовать
- `sealed override` — запрещает дальнейшее переопределение метода

### new (сокрытие)
- `new` скрывает метод базового класса, **не** переопределяет его
- При вызове через переменную базового типа — вызывается базовая версия
- Используйте осознанно: чаще нужен `override`
"""})

post(934, "compare_variants", {
    "question": "Что произойдёт при вызове через переменную базового типа?",
    "variants": [
        {
            "title": "Метод с override (полиморфизм)",
            "code": """class Base {
    public virtual string Greet() => "Hello from Base";
}
class Child : Base {
    public override string Greet() => "Hello from Child";
}

Base obj = new Child();
Console.WriteLine(obj.Greet());
// Вывод: Hello from Child
// override — виртуальная таблица выбирает Child.Greet"""
        },
        {
            "title": "Метод с new (сокрытие)",
            "code": """class Base {
    public virtual string Greet() => "Hello from Base";
}
class Child : Base {
    public new string Greet() => "Hello from Child";
}

Base obj = new Child();
Console.WriteLine(obj.Greet());
// Вывод: Hello from Base  ← СЮРПРИЗ!
// new скрывает, но не переопределяет. Через Base-ссылку — Base.Greet"""
        }
    ],
    "correct": 0,
    "explanation": "override работает полиморфно — всегда вызывается реализация реального объекта. new — сокрытие без полиморфизма."
})

post(934, "code", {
    "language": "csharp",
    "title": "sealed class и sealed override",
    "code": """// sealed class — нельзя наследовать
sealed class Singleton {
    private static readonly Singleton _instance = new();
    private Singleton() { }
    public static Singleton Instance => _instance;
}

// sealed override — запрещаем дальнейшее переопределение
class Animal {
    public virtual string Sound() => "...";
}
class Dog : Animal {
    public sealed override string Sound() => "Гав";
}
// class Labrador : Dog {
//     public override string Sound() => "...";  // ошибка! Sound sealed
// }

Console.WriteLine(new Dog().Sound());"""
})

post(934, "prediction", {
    "language": "csharp",
    "code": """class A {
    public virtual void M() => Console.WriteLine("A");
}
class B : A {
    public new void M() => Console.WriteLine("B");
}

A obj1 = new B();
B obj2 = new B();
obj1.M();
obj2.M();""",
    "expectedOutput": "A\nB",
    "hint": "obj1 — тип A, `new` не полиморфно → A.M(). obj2 — тип B → B.M()."
})

post(934, "quiz", {
    "question": "Зачем использовать `new` при сокрытии метода вместо просто объявить метод с тем же именем?",
    "options": [
        "new обязателен, иначе ошибка компиляции",
        "new явно документирует намеренное сокрытие и убирает предупреждение компилятора",
        "new делает метод полиморфным",
        "new автоматически вызывает base метод"
    ],
    "correct": 1,
    "explanation": "Без `new` компилятор выдаёт предупреждение. `new` говорит: 'я намеренно скрываю базовый метод'."
})

post(934, "knowledge_check", {
    "topic": "sealed",
    "question": "Почему String (System.String) в .NET помечен как sealed?",
    "options": [
        "Чтобы защитить от наследования и изменения неизменяемой семантики",
        "Потому что string — примитивный тип",
        "Для экономии памяти",
        "Sealed обязателен для всех встроенных типов"
    ],
    "correct": 0,
    "explanation": "String неизменяем (immutable). Разрешение наследования позволило бы нарушить этот контракт."
})

# --------------------------------------------------
# Lesson 935: Паттерн-матчинг с типами
# --------------------------------------------------
print("=== Lesson 935: Паттерн-матчинг с типами ===")

post(935, "text", {"markdown": """## Паттерн-матчинг с типами (C# 7–11)

Паттерн-матчинг — мощная альтернатива цепочкам `if (x is SomeType)`.

```csharp
object obj = ...;

// Type pattern
if (obj is string s) Console.WriteLine(s.Length);

// switch expression с паттернами
string desc = obj switch {
    int i    => $"число {i}",
    string s => $"строка '{s}'",
    null     => "null",
    _        => "что-то другое"
};
```
"""})

post(935, "code_to_intent", {
    "code": """string Classify(object obj) => obj switch {
    int n when n < 0  => "отрицательное",
    int n when n == 0 => "ноль",
    int n             => "положительное",
    string s          => $"строка длиной {s.Length}",
    null              => "null",
    _                 => "другой тип"
};

Console.WriteLine(Classify(-5));
Console.WriteLine(Classify(0));
Console.WriteLine(Classify("hi"));""",
    "options": [
        "Проверяет тип объекта и значение, возвращая текстовое описание",
        "Конвертирует объекты в числа",
        "Сортирует объекты по типу"
    ],
    "correctIntent": 0
})

post(935, "code", {
    "language": "csharp",
    "title": "Паттерн-матчинг с иерархией классов",
    "code": """abstract class Shape { }
class Circle    : Shape { public double Radius; }
class Rectangle : Shape { public double W, H; }
class Triangle  : Shape { public double Base, Height; }

double GetArea(Shape shape) => shape switch {
    Circle    c => Math.PI * c.Radius * c.Radius,
    Rectangle r => r.W * r.H,
    Triangle  t => 0.5 * t.Base * t.Height,
    null        => throw new ArgumentNullException(nameof(shape)),
    _           => throw new NotSupportedException()
};

Console.WriteLine(GetArea(new Circle    { Radius = 5 }));
Console.WriteLine(GetArea(new Rectangle { W = 4, H = 3 }));
Console.WriteLine(GetArea(new Triangle  { Base = 6, Height = 4 }));"""
})

post(935, "code_to_intent", {
    "code": """bool IsAdult(object person) => person switch {
    null                              => false,
    Person { Age: >= 18 }             => true,
    Person p when p.Age >= 16         => false,  // 16-17
    _                                 => false
};""",
    "options": [
        "Возвращает true только если Age >= 18, учитывая null-безопасность",
        "Проверяет, является ли объект экземпляром Person",
        "Конвертирует возраст в булево значение"
    ],
    "correctIntent": 0
})

post(935, "mini_task", {
    "language": "csharp",
    "title": "Напиши функцию с паттерн-матчингом",
    "description": "Реализуй метод Describe(), использующий switch expression с type patterns.",
    "starterCode": """class Cat  { public string Name = "Кот"; }
class Dog  { public string Name = "Пёс"; }
class Fish { }

string Describe(object animal) => animal switch {
    // реализуй три ветки: Cat, Dog, Fish
    // Cat  -> "Мяукает: " + cat.Name
    // Dog  -> "Лает: " + dog.Name
    // Fish -> "Молчит"
    // _    -> "Неизвестно"
    _ => "Неизвестно"
};

Console.WriteLine(Describe(new Cat()));
Console.WriteLine(Describe(new Dog()));
Console.WriteLine(Describe(new Fish()));""",
    "expectedOutput": "Мяукает: Кот\nЛает: Пёс\nМолчит"
})

post(935, "prediction", {
    "language": "csharp",
    "code": """object[] items = { 42, "hello", 3.14, null, true };
foreach (var item in items) {
    string result = item switch {
        int i    => $"int:{i}",
        string s => $"str:{s}",
        null     => "null",
        _        => "other"
    };
    Console.WriteLine(result);
}""",
    "expectedOutput": "int:42\nstr:hello\nother\nnull\nother",
    "hint": "double и bool не совпадают с int/string, попадают в _. null — отдельная ветка."
})

post(935, "transfer_test", {
    "question": "У тебя есть иерархия: Payment → CashPayment, CardPayment, CryptoPayment. Нужно вычислить комиссию по-разному для каждого типа. Что использовать?",
    "options": [
        "Цепочка if (payment is CashPayment) ... else if (payment is CardPayment) ...",
        "switch expression с type patterns",
        "Виртуальный метод CalculateFee() в каждом классе"
    ],
    "correct": 1,
    "explanation": "switch expression с паттернами — современный, читаемый способ. Виртуальный метод тоже хорош (полиморфизм), но если логика комиссии — внешняя ответственность, паттерн-матчинг чище."
})

print("\n=== Все блоки отправлены! ===")
