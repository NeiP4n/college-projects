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
        print(f"  ERR {e.code}: {e.read().decode()[:200]}")
    print(f"  [{st}] {btype}")

# =========================================================
# Lesson 974: Испытание: числовые алгоритмы
# =========================================================
print("\n=== Lesson 974: Числовые алгоритмы ===")

post(974, "text", {"markdown": """## Испытание: числовые алгоритмы

В этом уроке ты применишь классические числовые алгоритмы на практике.

**Что тебя ждёт:**
- Решето Эратосфена для поиска простых чисел
- Числа Фибоначчи
- Алгоритм Евклида для нахождения НОД

Пиши чистый, понятный код. Удачи!"""})

post(974, "challenge_task", {
    "language": "csharp",
    "title": "Простые числа от 2 до 100 (Решето Эратосфена)",
    "description": "Реализуй решето Эратосфена для нахождения всех простых чисел от 2 до 100.",
    "requirements": [
        "Создай булев массив размером 101, все элементы true по умолчанию",
        "Для каждого числа от 2 до sqrt(100) помечай кратные как false",
        "Собери оставшиеся true-индексы в результат",
        "Выведи числа через ', '"
    ],
    "starterCode": """bool[] sieve = new bool[101];
// Инициализируй решето — все числа считаем простыми
for (int i = 2; i < sieve.Length; i++)
    sieve[i] = true;

// Твой код: реализуй решето Эратосфена
// Для каждого p от 2 до sqrt(100): если sieve[p] == true,
// пометь все кратные p (начиная с p*p) как false
for (int p = 2; p * p <= 100; p++)
{
    // твой код здесь
}

var primes = new List<int>();
for (int i = 2; i <= 100; i++)
    if (sieve[i]) primes.Add(i);

Console.WriteLine(string.Join(", ", primes));""",
    "expectedOutput": "2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97"
})

post(974, "mini_task", {
    "language": "csharp",
    "title": "Числа Фибоначчи до N=20",
    "description": "Сгенерируй последовательность Фибоначчи: все числа, не превышающие 20. Начни с 0 и 1.",
    "starterCode": """var fibs = new List<int>();
int a = 0, b = 1;
// Твой код: добавляй числа Фибоначчи пока они <= 20
// Используй цикл while

Console.WriteLine(string.Join(", ", fibs));""",
    "expectedOutput": "0, 1, 1, 2, 3, 5, 8, 13"
})

post(974, "challenge_task", {
    "language": "csharp",
    "title": "НОД через алгоритм Евклида",
    "description": "Реализуй рекурсивный алгоритм Евклида для нахождения наибольшего общего делителя. Вычисли НОД для нескольких пар.",
    "requirements": [
        "Реализуй метод GCD(int a, int b) рекурсивно: GCD(a, b) = GCD(b, a % b), база — GCD(a, 0) = a",
        "Вычисли GCD(48, 18), GCD(100, 75), GCD(252, 105)",
        "Выведи результаты в формате 'GCD(a, b) = результат'"
    ],
    "starterCode": """static int GCD(int a, int b)
{
    // Твой код: рекурсивный алгоритм Евклида
    // Базовый случай: если b == 0, вернуть a
    // Иначе: вернуть GCD(b, a % b)
}

Console.WriteLine($"GCD(48, 18) = {GCD(48, 18)}");
Console.WriteLine($"GCD(100, 75) = {GCD(100, 75)}");
Console.WriteLine($"GCD(252, 105) = {GCD(252, 105)}");""",
    "expectedOutput": "GCD(48, 18) = 6\nGCD(100, 75) = 25\nGCD(252, 105) = 21"
})

post(974, "quiz", {
    "question": "Какова временная сложность решета Эратосфена для нахождения простых чисел до N?",
    "options": [
        "O(N²)",
        "O(N log log N)",
        "O(N log N)",
        "O(sqrt(N))"
    ],
    "correct": 1,
    "explanation": "Решето Эратосфена работает за O(N log log N) — это почти линейно. Каждое составное число помечается только несколько раз (при обработке каждого из его простых делителей)."
})

# =========================================================
# Lesson 975: Испытание: строковые алгоритмы
# =========================================================
print("\n=== Lesson 975: Строковые алгоритмы ===")

post(975, "text", {"markdown": """## Испытание: строковые алгоритмы

Строки — основа обработки данных. В этом уроке ты реализуешь:

- **Подсчёт слов** — Split, группировка, сортировка
- **Палиндром** — классическая проверка
- **Разворот строки** — найди и исправь баг в алгоритме

Помни: в C# строки иммутабельны. Используй `StringBuilder` или LINQ для эффективной работы."""})

post(975, "challenge_task", {
    "language": "csharp",
    "title": "Подсчёт слов в тексте",
    "description": "Дан текст. Подсчитай количество каждого уникального слова (без учёта регистра), выведи топ-5 по частоте.",
    "requirements": [
        "Разбей текст на слова через Split, удали пунктуацию через char.IsLetter",
        "Приведи все слова к нижнему регистру",
        "Сгруппируй и подсчитай через Dictionary<string, int> или LINQ GroupBy",
        "Выведи топ-5 слов в формате 'слово: N' по убыванию частоты"
    ],
    "starterCode": """string text = "to be or not to be that is the question whether tis nobler in the mind to suffer";

// Твой код:
// 1. Разбей text.Split(' ') и получи массив слов
// 2. Приведи к нижнему регистру
// 3. Подсчитай частоту каждого слова
var wordCount = new Dictionary<string, int>();
// заполни словарь...

// 4. Выведи топ-5 по убыванию
var top5 = wordCount
    // твой LINQ: OrderByDescending, Take(5)
    ;

foreach (var (word, count) in top5)
    Console.WriteLine($"{word}: {count}");""",
    "expectedOutput": "to: 3\nbe: 2\nthe: 2\nor: 1\nnot: 1"
})

post(975, "mini_task", {
    "language": "csharp",
    "title": "Проверка палиндрома",
    "description": "Проверь, является ли строка \"racecar\" палиндромом. Игнорируй регистр и небуквенные символы.",
    "starterCode": """string s = "racecar";
// Твой код:
// 1. Оставь только буквы, приведи к нижнему регистру
string clean = new string(s.Where(char.IsLetter).Select(char.ToLower).ToArray());
// 2. Сравни строку с её разворотом
bool isPalindrome = /* твой код */ false;

Console.WriteLine($"\\"{s}\\" is palindrome: {isPalindrome}");""",
    "expectedOutput": "\"racecar\" is palindrome: True"
})

post(975, "fix_bug", {
    "language": "csharp",
    "buggyCode": """string input = "Hello, World!";
char[] chars = input.ToCharArray();
// Баг: неправильные индексы при развороте — цикл идёт слишком далеко
for (int i = 0; i <= chars.Length; i++)
{
    char temp = chars[i];
    chars[i] = chars[chars.Length - i];  // ошибка: должно быть chars.Length - 1 - i
    chars[chars.Length - i] = temp;
}
string reversed = new string(chars);
Console.WriteLine(reversed);""",
    "expectedOutput": "!dlroW ,olleH",
    "hint": "В цикле разворота два баги: условие должно быть i < chars.Length / 2 (а не <=), и индекс зеркального элемента должен быть chars.Length - 1 - i (а не chars.Length - i)."
})

post(975, "quiz", {
    "question": "Какой метод C# наиболее эффективен для многократной конкатенации строк в цикле?",
    "options": [
        "string += часть",
        "string.Concat()",
        "StringBuilder.Append()",
        "string.Format()"
    ],
    "correct": 2,
    "explanation": "StringBuilder работает за O(N) суммарно — он изменяет внутренний буфер. Конкатенация через += создаёт новый объект при каждом вызове, что даёт O(N²) при N операциях."
})

# =========================================================
# Lesson 976: Испытание: сортировка и поиск
# =========================================================
print("\n=== Lesson 976: Сортировка и поиск ===")

post(976, "text", {"markdown": """## Испытание: сортировка и поиск

Алгоритмы сортировки и поиска — фундамент computer science. Сегодня ты:

- Реализуешь **пузырьковую сортировку** вручную (без Array.Sort)
- Напишешь **бинарный поиск** в отсортированном массиве
- Найдёшь **K наибольших элементов** без полной сортировки

Понимание этих алгоритмов помогает выбирать правильный инструмент в реальных задачах."""})

post(976, "challenge_task", {
    "language": "csharp",
    "title": "Пузырьковая сортировка",
    "description": "Реализуй пузырьковую сортировку массива целых чисел по возрастанию. Не используй Array.Sort или LINQ OrderBy.",
    "requirements": [
        "Внешний цикл — N-1 проходов по массиву",
        "Внутренний цикл — сравнивает соседние элементы и меняет их местами",
        "Оптимизация: флаг swapped — прервать если за проход не было обменов",
        "Вывести результат через string.Join"
    ],
    "starterCode": """int[] arr = { 64, 34, 25, 12, 22, 11, 90, 5, 47, 38 };

// Твой код: пузырьковая сортировка
int n = arr.Length;
for (int i = 0; i < n - 1; i++)
{
    bool swapped = false;
    for (int j = 0; j < n - 1 - i; j++)
    {
        // Если arr[j] > arr[j+1] — поменяй местами
        // твой код
    }
    // Если swapped == false — массив уже отсортирован, break
}

Console.WriteLine(string.Join(", ", arr));""",
    "expectedOutput": "5, 11, 12, 22, 25, 34, 38, 47, 64, 90"
})

post(976, "mini_task", {
    "language": "csharp",
    "title": "Бинарный поиск",
    "description": "Реализуй бинарный поиск числа 47 в отсортированном массиве. Верни индекс найденного элемента или -1.",
    "starterCode": """int[] sorted = { 5, 11, 12, 22, 25, 34, 38, 47, 64, 90 };
int target = 47;

int left = 0, right = sorted.Length - 1;
int result = -1;

// Твой код: бинарный поиск
while (left <= right)
{
    int mid = left + (right - left) / 2;
    // Если sorted[mid] == target — нашли!
    // Если sorted[mid] < target — искать справа
    // Иначе — искать слева
}

Console.WriteLine($"Index of {target}: {result}");""",
    "expectedOutput": "Index of 47: 7"
})

post(976, "constraint", {
    "language": "csharp",
    "title": "K наибольших элементов без полной сортировки",
    "description": "Найди 3 наибольших элемента из массива. Задача должна решаться эффективнее полной сортировки.",
    "constraint": "Нельзя использовать Array.Sort, LINQ OrderBy/OrderByDescending. Используй частичный отбор (partial selection) — проход по массиву с поддержкой минимальной кучи или k-sized буфера.",
    "starterCode": """int[] data = { 64, 34, 25, 12, 22, 11, 90, 5, 47, 38, 73, 99, 15 };
int k = 3;

// Твой код: найди k наибольших без сортировки всего массива
// Подход 1: поддерживай список из k элементов, заменяй минимум
// Подход 2: используй SortedSet<int> размером k
var topK = new SortedSet<int>();
foreach (int x in data)
{
    // Добавь x в topK
    // Если topK.Count > k — удали минимальный элемент
}

Console.WriteLine($"Top {k}: {string.Join(", ", topK.Reverse())}");""",
    "expectedOutput": "Top 3: 99, 90, 73",
    "hint": "SortedSet<int> хранит элементы в порядке возрастания. Min — наименьший, Reverse() даёт убывающий порядок. Когда размер превышает k — удали Min."
})

post(976, "quiz", {
    "question": "Какова временная сложность бинарного поиска в отсортированном массиве из N элементов?",
    "options": [
        "O(N)",
        "O(N log N)",
        "O(log N)",
        "O(1)"
    ],
    "correct": 2,
    "explanation": "Бинарный поиск делит область поиска пополам на каждом шаге. За log₂(N) шагов область сужается до 1 элемента — отсюда O(log N). Для N=1000 это всего ~10 сравнений."
})

# =========================================================
# Lesson 977: Испытание: банковская система
# =========================================================
print("\n=== Lesson 977: Банковская система ===")

post(977, "text", {"markdown": """## Испытание: банковская система

Спроектируй банковскую систему с нуля, применяя принципы ООП.

**Задачи:**
- Класс `BankAccount` с операциями Deposit/Withdraw/Transfer
- Наследование: `SavingsAccount` с начислением процентов
- Отладка логики защиты от овердрафта

**Принципы:** инкапсуляция баланса, валидация входных данных, выброс исключений при ошибках."""})

post(977, "challenge_task", {
    "language": "csharp",
    "title": "Класс BankAccount с полной логикой",
    "description": "Реализуй класс BankAccount с депозитом, снятием и переводом средств. Баланс не может быть отрицательным.",
    "requirements": [
        "Приватное поле decimal _balance, публичное свойство Balance",
        "Deposit(decimal amount): проверить amount > 0, прибавить к балансу",
        "Withdraw(decimal amount): проверить amount > 0 и amount <= Balance, иначе throw InvalidOperationException",
        "Transfer(BankAccount to, decimal amount): вызвать Withdraw на себе, Deposit на целевом счёте",
        "Проверить работу: создать два счёта, сделать перевод, вывести балансы"
    ],
    "starterCode": """class BankAccount
{
    private decimal _balance;
    public string Owner { get; }
    public decimal Balance => _balance;

    public BankAccount(string owner, decimal initialBalance)
    {
        Owner = owner;
        _balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        // Твой код: проверка и добавление к балансу
    }

    public void Withdraw(decimal amount)
    {
        // Твой код: проверка суммы и баланса, списание
    }

    public void Transfer(BankAccount to, decimal amount)
    {
        // Твой код: снять с this, положить на to
    }
}

var alice = new BankAccount("Alice", 1000m);
var bob = new BankAccount("Bob", 500m);
alice.Transfer(bob, 250m);
Console.WriteLine($"Alice: {alice.Balance}");
Console.WriteLine($"Bob: {bob.Balance}");""",
    "expectedOutput": "Alice: 750\nBob: 750"
})

post(977, "mini_task", {
    "language": "csharp",
    "title": "SavingsAccount с процентами",
    "description": "Создай класс SavingsAccount, наследующий BankAccount, с методом ApplyInterest(decimal rate).",
    "starterCode": """class BankAccount
{
    protected decimal _balance;
    public string Owner { get; }
    public decimal Balance => _balance;
    public BankAccount(string owner, decimal balance) { Owner = owner; _balance = balance; }
    public void Deposit(decimal amount) { if (amount > 0) _balance += amount; }
    public void Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > _balance) throw new InvalidOperationException("Недостаточно средств");
        _balance -= amount;
    }
}

class SavingsAccount : BankAccount
{
    // Твой код: конструктор, вызывающий base(owner, balance)
    // Метод ApplyInterest(decimal rate): начислить _balance *= (1 + rate)
    // Округли до 2 знаков через Math.Round
}

var savings = new SavingsAccount("Carol", 1000m);
savings.ApplyInterest(0.05m); // 5%
Console.WriteLine($"After interest: {savings.Balance}");""",
    "expectedOutput": "After interest: 1050"
})

post(977, "fix_bug", {
    "language": "csharp",
    "buggyCode": """class BankAccount
{
    private decimal _balance;
    public decimal Balance => _balance;
    public BankAccount(decimal initial) { _balance = initial; }

    public bool Withdraw(decimal amount)
    {
        // Баг: условие защиты от овердрафта неправильное
        // Снятие проходит даже когда денег недостаточно
        if (amount > 0 && _balance >= 0)  // должно быть _balance >= amount
        {
            _balance -= amount;
            return true;
        }
        return false;
    }
}

var acc = new BankAccount(100m);
bool ok1 = acc.Withdraw(150m); // должно вернуть false, денег не хватает
bool ok2 = acc.Withdraw(50m);  // должно вернуть true
Console.WriteLine($"Withdraw 150: {ok1}");
Console.WriteLine($"Withdraw 50: {ok2}");
Console.WriteLine($"Balance: {acc.Balance}");""",
    "expectedOutput": "Withdraw 150: False\nWithdraw 50: True\nBalance: 50",
    "hint": "Условие в Withdraw неверное: `_balance >= 0` всегда истинно для неотрицательного баланса. Нужно `_balance >= amount` — проверить, хватает ли средств на конкретную сумму."
})

post(977, "checkpoint", {
    "title": "Банковская система: итоги",
    "description": "Проверь, что ты освоил ключевые концепции ООП на примере банковской системы.",
    "tasks": [
        "Инкапсуляция: баланс защищён через приватное поле и публичное свойство",
        "Валидация: все операции проверяют корректность входных данных",
        "Наследование: SavingsAccount расширяет BankAccount без дублирования кода",
        "Исключения: InvalidOperationException при недопустимых операциях",
        "Метод Transfer атомарно использует Withdraw + Deposit"
    ]
})

# =========================================================
# Lesson 978: Испытание: геометрические фигуры
# =========================================================
print("\n=== Lesson 978: Геометрические фигуры ===")

post(978, "text", {"markdown": """## Испытание: геометрические фигуры

Полиморфизм — одна из мощнейших возможностей ООП. В этом уроке ты:

- Создашь **абстрактный базовый класс** `Shape`
- Реализуешь `Circle`, `Rectangle`, `Triangle`
- Напишешь код, работающий с любой фигурой **без if/switch**

Это классический пример открытого/закрытого принципа (OCP): новую фигуру добавить легко — базовый код менять не нужно."""})

post(978, "challenge_task", {
    "language": "csharp",
    "title": "Иерархия Shape с Area() и Perimeter()",
    "description": "Создай абстрактный класс Shape и три наследника: Circle, Rectangle, Triangle. Каждый реализует Area() и Perimeter().",
    "requirements": [
        "abstract class Shape с abstract double Area() и abstract double Perimeter() и virtual string Describe()",
        "Circle(double radius): Area = π*r², Perimeter = 2*π*r",
        "Rectangle(double width, double height): Area = w*h, Perimeter = 2*(w+h)",
        "Triangle(double a, double b, double c): Area через формулу Герона, Perimeter = a+b+c",
        "Создать массив Shape[] и вывести Area и Perimeter каждой фигуры"
    ],
    "starterCode": """using System;

abstract class Shape
{
    public abstract double Area();
    public abstract double Perimeter();
    public virtual string Describe() => $"{GetType().Name}: Area={Area():F2}, P={Perimeter():F2}";
}

class Circle : Shape
{
    private double _r;
    public Circle(double radius) { _r = radius; }
    // Твой код: Area() и Perimeter()
}

class Rectangle : Shape
{
    private double _w, _h;
    public Rectangle(double width, double height) { _w = width; _h = height; }
    // Твой код: Area() и Perimeter()
}

class Triangle : Shape
{
    private double _a, _b, _c;
    public Triangle(double a, double b, double c) { _a = a; _b = b; _c = c; }
    // Твой код: Area() через формулу Герона, Perimeter()
    // Формула Герона: s = (a+b+c)/2, Area = sqrt(s*(s-a)*(s-b)*(s-c))
}

Shape[] shapes = { new Circle(5), new Rectangle(4, 6), new Triangle(3, 4, 5) };
foreach (var shape in shapes)
    Console.WriteLine(shape.Describe());""",
    "expectedOutput": "Circle: Area=78.54, P=31.42\nRectangle: Area=24.00, P=20.00\nTriangle: Area=6.00, P=12.00"
})

post(978, "mini_task", {
    "language": "csharp",
    "title": "Найти фигуру с максимальной площадью",
    "description": "Из массива Shape[] найди фигуру с наибольшей площадью и выведи её описание.",
    "starterCode": """// Используй те же классы Shape, Circle, Rectangle, Triangle что выше
Shape[] shapes = { new Circle(3), new Rectangle(10, 2), new Triangle(6, 8, 10), new Circle(5) };

// Твой код: найди фигуру с максимальной Area()
// Можно использовать LINQ MaxBy или цикл
Shape maxShape = null;
double maxArea = double.MinValue;
foreach (var s in shapes)
{
    // обнови maxShape и maxArea если s.Area() больше текущего максимума
}

Console.WriteLine($"Largest: {maxShape.Describe()}");""",
    "expectedOutput": "Largest: Circle: Area=78.54, P=31.42"
})

post(978, "constraint", {
    "language": "csharp",
    "title": "Полиморфный вывод без if/switch",
    "description": "Выведи тип и площадь каждой фигуры, используя ТОЛЬКО полиморфизм. Никаких проверок типа.",
    "constraint": "Запрещено использовать if, switch, is, as, typeof, GetType() в коде клиента. Только вызовы виртуальных методов через базовый тип Shape.",
    "starterCode": """// Добавь в Shape метод PrintInfo() — виртуальный, выводит тип и площадь
// Переопредели его в каждом наследнике со специфичным форматом

abstract class Shape
{
    public abstract double Area();
    public abstract double Perimeter();
    // Твой код: добавь virtual void PrintInfo()
}

class Circle : Shape
{
    private double _r;
    public Circle(double r) { _r = r; }
    public override double Area() => Math.PI * _r * _r;
    public override double Perimeter() => 2 * Math.PI * _r;
    // Переопредели PrintInfo(): "Circle (r=X): Area=Y"
}

class Rectangle : Shape
{
    private double _w, _h;
    public Rectangle(double w, double h) { _w = w; _h = h; }
    public override double Area() => _w * _h;
    public override double Perimeter() => 2 * (_w + _h);
    // Переопредели PrintInfo(): "Rectangle (WxH): Area=Y"
}

Shape[] shapes = { new Circle(5), new Rectangle(4, 6) };
// Вызывай только shape.PrintInfo() — без if/is/typeof!
foreach (var shape in shapes)
    shape.PrintInfo();""",
    "expectedOutput": "Circle (r=5): Area=78.54\nRectangle (4x6): Area=24.00",
    "hint": "Виртуальный метод PrintInfo() в каждом классе знает свой тип — не нужно проверять его снаружи. Это и есть полиморфизм: один вызов, разное поведение."
})

# =========================================================
# Lesson 979: Испытание: инвентарь RPG
# =========================================================
print("\n=== Lesson 979: Инвентарь RPG ===")

post(979, "text", {"markdown": """## Испытание: инвентарь RPG

Создадим систему инвентаря для RPG-игры — практический пример коллекций и ООП.

**Компоненты:**
- Класс `Item` с Name, Weight, Value
- Класс `Inventory` с ограничением вместимости
- LINQ для анализа предметов

Это реальный паттерн: класс-сущность + класс-контейнер с бизнес-логикой."""})

post(979, "challenge_task", {
    "language": "csharp",
    "title": "Item и Inventory с управлением весом",
    "description": "Реализуй класс Item и класс Inventory с ограничением максимального веса. Добавление предмета должно проверять лимит.",
    "requirements": [
        "record Item(string Name, double Weight, decimal Value) или класс с этими полями",
        "Inventory(double maxWeight): хранит список предметов, проверяет суммарный вес",
        "Add(Item item): добавить если TotalWeight + item.Weight <= maxWeight, иначе false",
        "Remove(string name): удалить первый предмет с таким именем",
        "Свойство TotalWeight: сумма весов всех предметов",
        "Вывести содержимое и общий вес"
    ],
    "starterCode": """record Item(string Name, double Weight, decimal Value);

class Inventory
{
    private List<Item> _items = new();
    private double _maxWeight;
    public double TotalWeight => /* сумма весов */ 0;

    public Inventory(double maxWeight) { _maxWeight = maxWeight; }

    public bool Add(Item item)
    {
        // Проверь TotalWeight + item.Weight <= _maxWeight
        // Если да — добавь в _items и вернуть true
        // Иначе false
        return false;
    }

    public bool Remove(string name)
    {
        // Найди и удали первый предмет с таким Name
        // Вернуть true если удалён, false если не найден
        return false;
    }
}

var inv = new Inventory(maxWeight: 20.0);
inv.Add(new Item("Sword", 5.5, 100m));
inv.Add(new Item("Shield", 8.0, 80m));
inv.Add(new Item("Potion", 0.5, 25m));
bool added = inv.Add(new Item("Armor", 15.0, 200m)); // не влезет
Console.WriteLine($"Items: {inv.TotalWeight:F1} kg, heavy item added: {added}");
inv.Remove("Potion");
Console.WriteLine($"After remove: {inv.TotalWeight:F1} kg");""",
    "expectedOutput": "Items: 14.0 kg, heavy item added: False\nAfter remove: 13.5 kg"
})

post(979, "mini_task", {
    "language": "csharp",
    "title": "Лучшие предметы по Value/Weight через LINQ",
    "description": "Найди топ-3 предмета с наилучшим соотношением ценности к весу (Value/Weight) через LINQ.",
    "starterCode": """record Item(string Name, double Weight, decimal Value);

var items = new List<Item>
{
    new("Dagger",    0.5,  40m),
    new("Sword",     5.5, 100m),
    new("Armor",    15.0, 200m),
    new("Ring",      0.1,  60m),
    new("Potion",    0.5,  25m),
    new("Bow",       2.0,  75m),
};

// Твой код: LINQ — OrderByDescending по (double)Value / Weight, Take(3)
var best = items
    // .OrderByDescending(i => ...)
    // .Take(3)
    ;

foreach (var item in best)
    Console.WriteLine($"{item.Name}: ratio={(double)item.Value / item.Weight:F1}");""",
    "expectedOutput": "Ring: ratio=600.0\nDagger: ratio=80.0\nBow: ratio=37.5"
})

post(979, "fix_bug", {
    "language": "csharp",
    "buggyCode": """record Item(string Name, double Weight, decimal Value);

class Inventory
{
    private List<Item> _items = new();
    private double _maxWeight;
    public double TotalWeight => _items.Sum(i => i.Weight);
    public Inventory(double maxWeight) { _maxWeight = maxWeight; }

    public bool Add(Item item)
    {
        // Баг: проверка выполняется ПОСЛЕ добавления предмета
        _items.Add(item);
        if (TotalWeight > _maxWeight)
        {
            return false; // предмет уже добавлен, но возвращаем false!
        }
        return true;
    }
}

var inv = new Inventory(10.0);
inv.Add(new Item("Sword", 6.0, 100m));
bool ok = inv.Add(new Item("Armor", 8.0, 200m)); // должно быть False, суммарно 14 > 10
Console.WriteLine($"Heavy item added: {ok}");
Console.WriteLine($"Total weight: {inv.TotalWeight}"); // должно быть 6, не 14""",
    "expectedOutput": "Heavy item added: False\nTotal weight: 6",
    "hint": "Проверку нужно делать ДО добавления: `if (TotalWeight + item.Weight > _maxWeight) return false;`. Иначе предмет попадает в список, даже если метод возвращает false."
})

post(979, "checkpoint", {
    "title": "Инвентарь RPG: итоги",
    "description": "Убедись, что ключевые концепции усвоены.",
    "tasks": [
        "record vs class: record Item — иммутабельная запись данных с автосгенерированным Equals",
        "Инкапсуляция: _items приватный, доступ только через Add/Remove",
        "Валидация ПЕРЕД изменением состояния: проверить лимит до добавления",
        "LINQ: OrderByDescending + Take для эффективного топ-N без ручных сортировок",
        "TotalWeight как вычисляемое свойство — всегда актуально, не требует обновления"
    ]
})

# =========================================================
# Lesson 980: Испытание: анализ продаж
# =========================================================
print("\n=== Lesson 980: Анализ продаж ===")

post(980, "text", {"markdown": """## Испытание: анализ продаж LINQ

Реальный сценарий: аналитик получил список продаж и должен быстро извлечь инсайты.

**Данные:** список объектов `Sale(Product, Category, Amount, Date)`

**Задачи:**
- Топ-3 категории по суммарной выручке
- Средний чек по месяцам
- Исправить баг в GroupBy-агрегации

Это именно то, что делают .NET разработчики каждый день — анализ данных через LINQ."""})

post(980, "challenge_task", {
    "language": "csharp",
    "title": "Топ-3 категории по выручке",
    "description": "Дан список продаж. Сгруппируй по категории, суммируй выручку, выведи топ-3.",
    "requirements": [
        "GroupBy по Category",
        "Для каждой группы вычислить Sum(Amount)",
        "Отсортировать по убыванию суммы",
        "Взять первые 3 через Take(3)",
        "Вывести в формате 'Category: XXXX'"
    ],
    "starterCode": """record Sale(string Product, string Category, decimal Amount, DateTime Date);

var sales = new List<Sale>
{
    new("Laptop",   "Electronics", 1200m, new DateTime(2024,1,5)),
    new("Phone",    "Electronics",  800m, new DateTime(2024,1,12)),
    new("Shirt",    "Clothing",      50m, new DateTime(2024,1,15)),
    new("Pants",    "Clothing",      80m, new DateTime(2024,2,3)),
    new("Desk",     "Furniture",    350m, new DateTime(2024,2,7)),
    new("Chair",    "Furniture",    200m, new DateTime(2024,2,14)),
    new("Tablet",   "Electronics",  600m, new DateTime(2024,3,1)),
    new("Sofa",     "Furniture",    900m, new DateTime(2024,3,10)),
    new("Jacket",   "Clothing",     150m, new DateTime(2024,3,18)),
    new("Monitor",  "Electronics",  450m, new DateTime(2024,3,22)),
};

// Твой код: GroupBy → Sum → OrderByDescending → Take(3)
var top3 = sales
    .GroupBy(s => s.Category)
    .Select(g => new { Category = g.Key, Total = g.Sum(s => s.Amount) })
    // .OrderByDescending(...)
    // .Take(3)
    ;

foreach (var cat in top3)
    Console.WriteLine($"{cat.Category}: {cat.Total}");""",
    "expectedOutput": "Electronics: 3050\nFurniture: 1450\nClothing: 280"
})

post(980, "mini_task", {
    "language": "csharp",
    "title": "Средний чек по месяцам",
    "description": "Из того же списка продаж вычисли средний чек (Average Amount) по каждому месяцу.",
    "starterCode": """record Sale(string Product, string Category, decimal Amount, DateTime Date);
var sales = new List<Sale>
{
    new("Laptop",  "Electronics", 1200m, new DateTime(2024,1,5)),
    new("Phone",   "Electronics",  800m, new DateTime(2024,1,12)),
    new("Shirt",   "Clothing",      50m, new DateTime(2024,1,15)),
    new("Pants",   "Clothing",      80m, new DateTime(2024,2,3)),
    new("Desk",    "Furniture",    350m, new DateTime(2024,2,7)),
    new("Chair",   "Furniture",    200m, new DateTime(2024,2,14)),
    new("Tablet",  "Electronics",  600m, new DateTime(2024,3,1)),
    new("Sofa",    "Furniture",    900m, new DateTime(2024,3,10)),
    new("Jacket",  "Clothing",     150m, new DateTime(2024,3,18)),
    new("Monitor", "Electronics",  450m, new DateTime(2024,3,22)),
};

// Твой код: сгруппируй по Date.Month, вычисли Average(Amount)
var byMonth = sales
    .GroupBy(s => s.Date.Month)
    .Select(g => new { Month = g.Key, Avg = g.Average(s => s.Amount) })
    .OrderBy(x => x.Month);

foreach (var m in byMonth)
    Console.WriteLine($"Month {m.Month}: {m.Avg:F2}");""",
    "expectedOutput": "Month 1: 683.33\nMonth 2: 210.00\nMonth 3: 525.00"
})

post(980, "fix_bug", {
    "language": "csharp",
    "buggyCode": """record Sale(string Product, string Category, decimal Amount, DateTime Date);
var sales = new List<Sale>
{
    new("Laptop",  "Electronics", 1200m, new DateTime(2024,1,5)),
    new("Phone",   "Electronics",  800m, new DateTime(2024,1,12)),
    new("Tablet",  "Electronics",  600m, new DateTime(2024,3,1)),
    new("Shirt",   "Clothing",      50m, new DateTime(2024,1,15)),
    new("Jacket",  "Clothing",     150m, new DateTime(2024,3,18)),
};

// Баг: Count() вместо Sum() для подсчёта выручки
var result = sales
    .GroupBy(s => s.Category)
    .Select(g => new { Category = g.Key, Revenue = g.Count() }) // должно быть g.Sum(s => s.Amount)
    .OrderByDescending(x => x.Revenue);

foreach (var r in result)
    Console.WriteLine($"{r.Category}: {r.Revenue}");""",
    "expectedOutput": "Electronics: 2600\nClothing: 200",
    "hint": "g.Count() подсчитывает количество продаж в группе, а не сумму выручки. Нужно g.Sum(s => s.Amount) для суммирования поля Amount."
})

post(980, "quiz", {
    "question": "Что возвращает LINQ-метод GroupBy?",
    "options": [
        "IEnumerable<T> — плоский список элементов",
        "IEnumerable<IGrouping<TKey, TElement>> — коллекцию групп, каждая с Key и элементами",
        "Dictionary<TKey, List<TElement>>",
        "ILookup<TKey, TElement> — только lookup, не IEnumerable"
    ],
    "correct": 1,
    "explanation": "GroupBy возвращает IEnumerable<IGrouping<TKey, TElement>>. Каждая группа — это IGrouping с Key (ключ группировки) и реализацией IEnumerable<TElement> для итерации по элементам группы. Метод ToLookup() даёт ILookup — он вычисляется немедленно."
})

# =========================================================
# Lesson 981: Испытание: обработка текста
# =========================================================
print("\n=== Lesson 981: Обработка текста ===")

post(981, "text", {"markdown": """## Испытание: обработка текста LINQ

Текстовый анализ — классическая задача для LINQ. В этом уроке только цепочки запросов, никакого foreach!

**Правила игры:**
- Только LINQ: `Where`, `GroupBy`, `Select`, `OrderBy`, `Distinct` и т.д.
- Никаких `foreach`-циклов для трансформации данных
- `foreach` разрешён только для вывода результата

Это тренирует декларативное мышление — описывай ЧТО нужно, не КАК."""})

post(981, "challenge_task", {
    "language": "csharp",
    "title": "Топ-5 слов по частоте через LINQ",
    "description": "Дан текст. Через цепочку LINQ-операторов найди топ-5 слов по частоте появления.",
    "requirements": [
        "Split(' ', '\\n', '\\r', '.', ',', '!', '?') для разбивки на слова",
        "Where(w => w.Length > 0) для фильтрации пустых",
        "Select(w => w.ToLower()) для нормализации",
        "GroupBy(w => w) для группировки",
        "OrderByDescending(g => g.Count()).Take(5) для топ-5",
        "Select результат в анонимный тип { Word, Count }"
    ],
    "starterCode": """string text = @"the quick brown fox jumps over the lazy dog
the dog barked at the fox and the fox ran away
the quick fox was not lazy it was the fastest fox";

var top5 = text
    .Split(new char[]{' ', '\n', '\r', '.', ','}, StringSplitOptions.RemoveEmptyEntries)
    // .Select(w => w.ToLower())
    // .GroupBy(w => w)
    // .Select(g => new { Word = g.Key, Count = g.Count() })
    // .OrderByDescending(x => x.Count)
    // .Take(5)
    ;

foreach (var item in top5)
    Console.WriteLine($"{item.Word}: {item.Count}");""",
    "expectedOutput": "the: 8\nfox: 4\nquick: 2\ndog: 2\nlazy: 2"
})

post(981, "mini_task", {
    "language": "csharp",
    "title": "Самое длинное уникальное слово",
    "description": "Найди самое длинное слово в тексте без дубликатов. Только LINQ, без foreach для обработки.",
    "starterCode": """string text = "the quick brown fox jumps over the lazy brown dog";

// Твой код: только LINQ
// 1. Split по пробелу
// 2. Select ToLower
// 3. Distinct() — убрать дубликаты
// 4. OrderByDescending(w => w.Length)
// 5. First() — взять первое (самое длинное)
string longest = text
    // твоя LINQ цепочка
    .Split(' ')
    .Select(w => w.ToLower())
    // продолжи...
    .First();

Console.WriteLine($"Longest unique word: {longest}");""",
    "expectedOutput": "Longest unique word: jumps"
})

post(981, "constraint", {
    "language": "csharp",
    "title": "Анализ текста: только LINQ",
    "description": "Выполни полный текстовый анализ используя ТОЛЬКО LINQ. Найди: количество уникальных слов, среднюю длину слова, самое редкое слово.",
    "constraint": "Абсолютно никаких foreach, for, while, do-while циклов для обработки данных. Только LINQ-цепочки. foreach разрешён ТОЛЬКО для Console.WriteLine в конце.",
    "starterCode": """string text = "to be or not to be that is the question whether tis nobler in the mind to suffer";

var words = text.Split(' ').Select(w => w.ToLower()).ToList();

// Всё через LINQ — без циклов!
int uniqueCount = words.Distinct().Count();
double avgLength = words.Average(w => w.Length);
// Найди слово, которое встречается реже всего:
string rarest = words
    .GroupBy(w => w)
    // .OrderBy(g => g.Count())  — наименее частое первым
    // .Select(g => g.Key)
    // .First()
    ;

Console.WriteLine($"Unique words: {uniqueCount}");
Console.WriteLine($"Avg length: {avgLength:F2}");
Console.WriteLine($"Rarest: {rarest}");""",
    "expectedOutput": "Unique words: 14\nAvg length: 3.72\nRarest: or",
    "hint": "Для нахождения самого редкого слова: GroupBy → OrderBy(g => g.Count()) → Select(g => g.Key) → First(). Это даст слово с минимальным количеством вхождений (алфавитно первое при ничьей)."
})

post(981, "quiz", {
    "question": "Чем отличается Distinct() от GroupBy() в контексте нахождения уникальных элементов?",
    "options": [
        "Distinct() быстрее, GroupBy() точнее",
        "Distinct() возвращает уникальные элементы, GroupBy() группирует для подсчёта — используй GroupBy когда нужна частота",
        "GroupBy() работает только со строками",
        "Разницы нет, они взаимозаменяемы"
    ],
    "correct": 1,
    "explanation": "Distinct() просто убирает дубликаты — подходит когда нужен список уникальных значений. GroupBy() группирует ВСЕ элементы по ключу, позволяя считать Count(), Sum() и т.д. Если нужна частота — GroupBy. Если просто уникальность — Distinct."
})

# =========================================================
# Lesson 982: Испытание: pipeline данных
# =========================================================
print("\n=== Lesson 982: Pipeline данных ===")

post(982, "text", {"markdown": """## Испытание: pipeline данных

Pipeline — это цепочка трансформаций, где выход одного шага — вход следующего. LINQ идеально подходит для этого паттерна.

**Шаги pipeline:**
1. **Фильтрация** — убрать невалидные данные
2. **Трансформация** — преобразовать формат
3. **Агрегация** — свести к итогу
4. **Форматирование** — подготовить для вывода

Такой подход используется в ETL-процессах, обработке логов, аналитике."""})

post(982, "challenge_task", {
    "language": "csharp",
    "title": "Цепочка трансформаций: фильтр → трансформация → агрегация",
    "description": "Обработай список транзакций через полный pipeline: отфильтруй невалидные, конвертируй валюту, агрегируй по типу, форматируй вывод.",
    "requirements": [
        "Фильтр: оставить только транзакции с Amount > 0 и Status == 'completed'",
        "Трансформация: конвертировать USD в RUB (курс 90.0), округлить до 2 знаков",
        "Агрегация: GroupBy Type, суммировать Amount в RUB",
        "Форматирование: вывести 'Type: X,XXX.XX RUB' по убыванию суммы"
    ],
    "starterCode": """record Transaction(string Type, decimal Amount, string Currency, string Status);

var transactions = new List<Transaction>
{
    new("income",   1500m, "USD", "completed"),
    new("expense",   200m, "USD", "completed"),
    new("income",   -100m, "USD", "completed"),  // невалидная
    new("expense",   350m, "USD", "pending"),     // не завершена
    new("income",    800m, "USD", "completed"),
    new("expense",   150m, "USD", "completed"),
    new("transfer",  500m, "USD", "completed"),
    new("expense",     0m, "USD", "completed"),   // невалидная
};

const decimal usdToRub = 90m;

var result = transactions
    // 1. Фильтр: Amount > 0 && Status == "completed"
    .Where(t => /* твой код */ true)
    // 2. Трансформация: конвертировать Amount * usdToRub
    .Select(t => new { t.Type, AmountRub = Math.Round(t.Amount * usdToRub, 2) })
    // 3. Агрегация: GroupBy Type, Sum AmountRub
    .GroupBy(t => t.Type)
    .Select(g => new { Type = g.Key, Total = g.Sum(t => t.AmountRub) })
    // 4. Сортировка по убыванию
    .OrderByDescending(x => x.Total);

foreach (var item in result)
    Console.WriteLine($"{item.Type}: {item.Total:N2} RUB");""",
    "expectedOutput": "income: 207000.00 RUB\ntransfer: 45000.00 RUB\nexpense: 31500.00 RUB"
})

post(982, "mini_task", {
    "language": "csharp",
    "title": "Группировка студентов по оценке",
    "description": "Сгруппируй студентов по буквенной оценке: A(90-100), B(75-89), C(60-74), F(<60). Выведи группы в порядке A, B, C, F.",
    "starterCode": """record Student(string Name, int Score);

var students = new List<Student>
{
    new("Alice", 95), new("Bob", 72), new("Carol", 88),
    new("Dave", 55), new("Eve", 91), new("Frank", 63),
    new("Grace", 78), new("Hank", 45), new("Ivy", 82),
};

// Твой код: добавь вычисляемую оценку через Select
static string GetGrade(int score) => score >= 90 ? "A" : score >= 75 ? "B" : score >= 60 ? "C" : "F";

var grouped = students
    .Select(s => new { s.Name, s.Score, Grade = GetGrade(s.Score) })
    .GroupBy(s => s.Grade)
    .OrderBy(g => g.Key); // A, B, C, F — лексикографически

foreach (var group in grouped)
{
    var names = string.Join(", ", group.Select(s => s.Name));
    Console.WriteLine($"{group.Key}: {names}");
}""",
    "expectedOutput": "A: Alice, Eve\nB: Carol, Grace, Ivy\nC: Bob, Frank\nF: Dave, Hank"
})

post(982, "quiz", {
    "question": "Когда лучше использовать query syntax (from x in ... select) вместо method syntax (x.Where().Select())?",
    "options": [
        "Query syntax всегда предпочтительнее, он более читаем",
        "Method syntax быстрее в рантайме",
        "Query syntax удобнее для сложных join и let-переменных; method syntax лаконичнее для простых цепочек",
        "Они полностью идентичны и выбор — только вкусовщина"
    ],
    "correct": 2,
    "explanation": "Компилятор транслирует query syntax в method syntax — производительность одинаковая. Query syntax удобнее для join (естественный SQL-подобный синтаксис) и let (именованные промежуточные переменные). Method syntax — для коротких цепочек Where/Select/OrderBy, особенно с лямбдами."
})

# =========================================================
# Lesson 983: Испытание: параллельные задачи
# =========================================================
print("\n=== Lesson 983: Параллельные задачи ===")

post(983, "text", {"markdown": """## Испытание: параллельные async задачи

Параллельное выполнение — ключевое преимущество async/await. Разница между sequential и parallel может быть огромной.

**Правило:** если задачи независимы — запускай их параллельно через `Task.WhenAll`.

**Типичные ошибки:**
- `await task1; await task2;` — последовательно, медленно
- `Task.WhenAll(task1, task2)` — параллельно, быстро

Сегодня ты почувствуешь эту разницу на практике."""})

post(983, "challenge_task", {
    "language": "csharp",
    "title": "5 параллельных задач через Task.WhenAll",
    "description": "Запусти 5 async-задач параллельно через Task.WhenAll, собери результаты и вычисли итог.",
    "requirements": [
        "Создать 5 задач с разными задержками (100ms, 200ms, 150ms, 300ms, 250ms)",
        "Каждая возвращает число (свой индекс * 10)",
        "Запустить ВСЕ через Task.WhenAll — они должны выполняться параллельно",
        "Суммировать результаты и измерить общее время (должно быть ~300ms, не 1000ms)",
        "Вывести результаты и время выполнения"
    ],
    "starterCode": """using System;
using System.Threading.Tasks;
using System.Diagnostics;

async Task<int> SimulateWork(int id, int delayMs)
{
    await Task.Delay(delayMs);
    return id * 10;
}

var sw = Stopwatch.StartNew();

// Твой код: создай и запусти задачи параллельно
int[] delays = { 100, 200, 150, 300, 250 };
// Создай массив Task<int>[] tasks
// Каждая: SimulateWork(i, delays[i])
Task<int>[] tasks = new Task<int>[5];
for (int i = 0; i < 5; i++)
    tasks[i] = SimulateWork(i, delays[i]);

// Жди все задачи параллельно
int[] results = await Task.WhenAll(tasks);

sw.Stop();
int sum = 0;
foreach (var r in results) sum += r;

Console.WriteLine($"Results: {string.Join(", ", results)}");
Console.WriteLine($"Sum: {sum}");
Console.WriteLine($"Time: ~{(sw.ElapsedMilliseconds / 100) * 100}ms");""",
    "expectedOutput": "Results: 0, 10, 20, 30, 40\nSum: 100\nTime: ~300ms"
})

post(983, "mini_task", {
    "language": "csharp",
    "title": "Sequential vs Parallel: измерь разницу",
    "description": "Выполни те же 3 задачи последовательно (sequential) и параллельно (WhenAll). Сравни время.",
    "starterCode": """using System.Diagnostics;

async Task<int> Work(int id, int ms) { await Task.Delay(ms); return id; }

// Sequential
var sw1 = Stopwatch.StartNew();
int r1 = await Work(1, 200);
int r2 = await Work(2, 200);
int r3 = await Work(3, 200);
sw1.Stop();
Console.WriteLine($"Sequential: {sw1.ElapsedMilliseconds}ms, sum={r1+r2+r3}");

// Parallel — твой код
var sw2 = Stopwatch.StartNew();
// используй Task.WhenAll для Work(1,200), Work(2,200), Work(3,200)
int[] results = await Task.WhenAll(/* твои задачи */);
sw2.Stop();
Console.WriteLine($"Parallel: {sw2.ElapsedMilliseconds}ms, sum={results.Sum()}");""",
    "expectedOutput": "Sequential: ~600ms, sum=6\nParallel: ~200ms, sum=6"
})

post(983, "fix_bug", {
    "language": "csharp",
    "buggyCode": """using System.Diagnostics;

async Task<string> FetchData(int id)
{
    await Task.Delay(300); // симуляция HTTP-запроса
    return $"data-{id}";
}

// Баг: задачи запускаются последовательно вместо параллельно
var sw = Stopwatch.StartNew();
string a = await FetchData(1);  // ждём 300ms
string b = await FetchData(2);  // ждём ещё 300ms
string c = await FetchData(3);  // ждём ещё 300ms
sw.Stop();

Console.WriteLine($"{a}, {b}, {c}");
Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms"); // ~900ms вместо ~300ms""",
    "expectedOutput": "data-1, data-2, data-3\nTime: ~300ms",
    "hint": "Запусти все три задачи БЕЗ await, сохрани Task<string> в переменные, затем await Task.WhenAll(t1, t2, t3). Так они стартуют параллельно и суммарное время равно максимуму, а не сумме."
})

post(983, "quiz", {
    "question": "Когда использовать Task.WhenAll vs Task.WhenAny?",
    "options": [
        "WhenAll — когда нужен результат первой завершившейся задачи; WhenAny — когда нужны все",
        "WhenAll — когда нужны ВСЕ результаты; WhenAny — когда достаточно первого (например, race condition или timeout)",
        "WhenAny быстрее, поэтому лучше всегда использовать его",
        "Разницы нет, оба ждут все задачи"
    ],
    "correct": 1,
    "explanation": "Task.WhenAll ждёт завершения ВСЕХ задач и возвращает массив результатов. Task.WhenAny возвращает Task первой завершившейся задачи — полезно для timeout-паттернов (WhenAny(actualWork, Task.Delay(timeout))) или когда достаточно любого ответа из нескольких источников."
})

# =========================================================
# Lesson 984: Испытание: таймаут и отмена
# =========================================================
print("\n=== Lesson 984: Таймаут и отмена ===")

post(984, "text", {"markdown": """## Испытание: таймаут и отмена

Реальные async-операции могут зависнуть. Профессиональный код всегда обрабатывает:

- **CancellationToken** — отмена по запросу пользователя или таймауту
- **Retry с backoff** — повтор с увеличивающейся задержкой
- **async Task vs async void** — критическая разница для обработки ошибок

Без этих паттернов твоё приложение будет зависать или молча терять исключения."""})

post(984, "challenge_task", {
    "language": "csharp",
    "title": "Операция с CancellationToken и таймаутом",
    "description": "Реализуй async-метод, который принимает CancellationToken и отменяется если выполняется дольше 2 секунд.",
    "requirements": [
        "Метод ProcessData(int[] data, CancellationToken ct) возвращает Task<int>",
        "Внутри цикл по data: для каждого элемента вызвать ct.ThrowIfCancellationRequested()",
        "Симулировать работу через await Task.Delay(100, ct)",
        "Запустить с CancellationTokenSource(TimeSpan.FromSeconds(2))",
        "Поймать OperationCanceledException и вывести сообщение"
    ],
    "starterCode": """using System.Threading;

async Task<int> ProcessData(int[] data, CancellationToken ct)
{
    int sum = 0;
    foreach (int item in data)
    {
        ct.ThrowIfCancellationRequested(); // проверить отмену
        await Task.Delay(300, ct);         // симуляция работы, поддерживает отмену
        sum += item;
    }
    return sum;
}

int[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // 10 элементов * 300ms = 3 сек

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // таймаут 2 сек

try
{
    int result = await ProcessData(data, cts.Token);
    Console.WriteLine($"Result: {result}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled: timeout exceeded");
}""",
    "expectedOutput": "Operation cancelled: timeout exceeded"
})

post(984, "mini_task", {
    "language": "csharp",
    "title": "Retry с экспоненциальным backoff",
    "description": "Реализуй метод RetryAsync, который повторяет операцию до 3 раз с задержками 1s, 2s, 4s при неудаче.",
    "starterCode": """int attempt = 0;

async Task<string> UnreliableOperation()
{
    attempt++;
    if (attempt < 3) throw new HttpRequestException($"Network error (attempt {attempt})");
    return "Success!";
}

async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    int delayMs = 500; // начальная задержка
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            Console.WriteLine($"Retry {i+1}: {ex.Message}");
            await Task.Delay(delayMs);
            delayMs *= 2; // экспоненциальный рост задержки
        }
    }
    return await operation(); // последняя попытка без catch
}

string result = await RetryAsync(UnreliableOperation);
Console.WriteLine(result);""",
    "expectedOutput": "Retry 1: Network error (attempt 1)\nRetry 2: Network error (attempt 2)\nSuccess!"
})

post(984, "fix_bug", {
    "language": "csharp",
    "buggyCode": """// Баг: async void вместо async Task — исключение потеряется
// async void нельзя await, исключения не перехватываются вызывающим кодом

async void LoadDataBuggy()
{
    await Task.Delay(100);
    throw new InvalidOperationException("Data load failed!"); // это исключение ПОТЕРЯЕТСЯ
}

// Правильный вариант должен быть async Task
async Task LoadDataCorrect()
{
    await Task.Delay(100);
    throw new InvalidOperationException("Data load failed!");
}

// Демонстрация проблемы:
try
{
    // LoadDataBuggy(); // нельзя await void — исключение не поймаем
    await LoadDataCorrect(); // await Task — исключение пробрасывается
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}""",
    "expectedOutput": "Caught: Data load failed!",
    "hint": "async void используй ТОЛЬКО для event handlers (Button.Click += async void Handler). В любом другом случае — async Task. Только Task можно await, только Task пробрасывает исключения к вызывающему коду."
})

post(984, "checkpoint", {
    "title": "Таймаут и отмена: итоги",
    "description": "Ключевые паттерны надёжного async-кода.",
    "tasks": [
        "CancellationToken передавай в каждый async-метод как последний параметр",
        "ct.ThrowIfCancellationRequested() в начале каждой итерации цикла",
        "Передавай token в Task.Delay(ms, ct) — иначе отмена не прервёт ожидание",
        "async void только для event handlers, везде else — async Task или async Task<T>",
        "Retry с экспоненциальным backoff: начальная задержка удваивается с каждой попыткой",
        "CancellationTokenSource(TimeSpan) — встроенный таймаут без дополнительного кода"
    ]
})

# =========================================================
# Lesson 985: Испытание: async pipeline
# =========================================================
print("\n=== Lesson 985: Async pipeline ===")

post(985, "text", {"markdown": """## Испытание: async pipeline

Producer/Consumer — фундаментальный паттерн конкурентного программирования.

**Как работает:**
- **Producer** генерирует данные и помещает в очередь
- **Consumer** берёт данные и обрабатывает
- **Channel<T>** — современный инструмент .NET для этого паттерна

Это основа для высокопроизводительных систем: очереди сообщений, стриминг данных, обработка событий."""})

post(985, "challenge_task", {
    "language": "csharp",
    "title": "Producer/Consumer через Channel<T>",
    "description": "Реализуй паттерн producer/consumer: producer генерирует числа, consumer суммирует их.",
    "requirements": [
        "Создать Channel<int>.CreateUnbounded<int>()",
        "Producer: записать числа 1..10 через channel.Writer.WriteAsync, затем channel.Writer.Complete()",
        "Consumer: читать через channel.Reader.ReadAllAsync(), суммировать",
        "Запустить producer и consumer как параллельные Task, дождаться обоих через WhenAll",
        "Вывести сумму"
    ],
    "starterCode": """using System.Threading.Channels;

var channel = Channel.CreateUnbounded<int>();

async Task Producer()
{
    for (int i = 1; i <= 10; i++)
    {
        await channel.Writer.WriteAsync(i);
        await Task.Delay(50); // симуляция генерации данных
    }
    channel.Writer.Complete(); // сигнал что данные закончились
}

async Task<int> Consumer()
{
    int sum = 0;
    // Твой код: читай через channel.Reader.ReadAllAsync()
    // await foreach (var item in channel.Reader.ReadAllAsync())
    //     sum += item;
    return sum;
}

// Запусти параллельно
var producerTask = Producer();
var consumerTask = Consumer();
await Task.WhenAll(producerTask, consumerTask);

Console.WriteLine($"Sum: {consumerTask.Result}");""",
    "expectedOutput": "Sum: 55"
})

post(985, "mini_task", {
    "language": "csharp",
    "title": "Async map-reduce на списке чисел",
    "description": "Реализуй async map-reduce: map умножает каждое число на 2 (параллельно), reduce суммирует результаты.",
    "starterCode": """async Task<int> MapItem(int x)
{
    await Task.Delay(10); // симуляция async-обработки
    return x * 2;
}

int[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// MAP: параллельно применить MapItem к каждому элементу
Task<int>[] mapTasks = data.Select(x => MapItem(x)).ToArray();
int[] mapped = await Task.WhenAll(mapTasks);

// REDUCE: суммировать все результаты
int total = mapped.Sum();

Console.WriteLine($"Mapped: {string.Join(", ", mapped)}");
Console.WriteLine($"Total: {total}");""",
    "expectedOutput": "Mapped: 2, 4, 6, 8, 10, 12, 14, 16, 18, 20\nTotal: 110"
})

post(985, "checkpoint", {
    "title": "Async Марафон завершён!",
    "description": "Ты прошёл полный курс async/await. Вот ключевые правила которые ты теперь знаешь:",
    "tasks": [
        "await vs WhenAll: независимые задачи — всегда WhenAll, зависимые — последовательный await",
        "async void только для event handlers, везде async Task или async Task<T>",
        "CancellationToken в каждый async-метод, ThrowIfCancellationRequested() в циклах",
        "Task.Delay(ms, ct) а не Task.Delay(ms) — чтобы отмена прерывала ожидание",
        "Channel<T> для producer/consumer — современная замена BlockingCollection",
        "Async map-reduce: Select(MapItem) создаёт задачи, WhenAll запускает параллельно",
        "ConfigureAwait(false) в библиотечном коде для избежания deadlock на SynchronizationContext"
    ]
})

print("\n=== Все уроки 974-985 заполнены! ===")
