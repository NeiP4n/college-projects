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

# =====================================================================
# Lesson 951: Делегаты
# =====================================================================
print("=== Lesson 951: Делегаты ===")
lid = 951

post(lid, "text", {"markdown": "## Делегаты в C#\n\nДелегат — это **тип**, который хранит ссылку на метод с определённой сигнатурой. Делегаты позволяют передавать методы как параметры, хранить их в переменных и вызывать динамически.\n\n```csharp\n// Объявление делегата\ndelegate int Operation(int a, int b);\n\n// Использование\nOperation add = (a, b) => a + b;\nConsole.WriteLine(add(3, 4)); // 7\n```\n\n### Зачем нужны делегаты?\n- Callback-функции\n- Обработчики событий\n- Паттерн «Стратегия»\n- Передача логики как параметра"})

post(lid, "code", {"language": "csharp", "title": "Объявление и использование делегата", "code": "using System;\n\n// Шаг 1: объявляем тип делегата\ndelegate double MathOp(double x, double y);\n\nclass Program\n{\n    static double Multiply(double x, double y) => x * y;\n\n    static void Main()\n    {\n        // Шаг 2: создаём переменную делегата\n        MathOp op = Multiply;\n\n        // Шаг 3: вызываем через делегат\n        Console.WriteLine(op(3.0, 4.0)); // 12\n\n        // Можно переназначить на лямбду\n        op = (x, y) => x + y;\n        Console.WriteLine(op(3.0, 4.0)); // 7\n    }\n}"})

post(lid, "compare_variants", {"question": "Что предпочесть: собственный делегат или встроенный Func<>?", "variants": [{"title": "Собственный delegate", "code": "delegate int Transformer(int x);\nTransformer square = x => x * x;\nConsole.WriteLine(square(5)); // 25"}, {"title": "Встроенный Func<int,int>", "code": "Func<int, int> square = x => x * x;\nConsole.WriteLine(square(5)); // 25"}], "correct": 1, "explanation": "Func<> и Action<> — стандартные обобщённые делегаты из BCL. Использовать их предпочтительнее: не нужно объявлять новый тип, код читается привычнее. Собственный delegate оправдан только когда нужно именованный тип для читаемости или поддерживаете специфическую подпись."})

post(lid, "code", {"language": "csharp", "title": "Многоадресный (multicast) делегат", "code": "using System;\n\ndelegate void Logger(string message);\n\nclass Program\n{\n    static void LogToConsole(string msg) => Console.WriteLine($\"[Console] {msg}\");\n    static void LogToFile(string msg)    => Console.WriteLine($\"[File]    {msg}\");\n\n    static void Main()\n    {\n        Logger log = LogToConsole;\n        log += LogToFile;          // добавляем второй обработчик\n\n        log(\"Приложение запущено\"); // вызовутся оба метода\n\n        log -= LogToFile;          // убираем обработчик\n        log(\"Только Console\");\n    }\n}"})

post(lid, "quiz", {"question": "Что произойдёт при вызове многоадресного делегата, если один из методов бросит исключение?", "options": ["Исключение игнорируется, остальные методы вызываются", "Выполнение останавливается, последующие методы не вызываются", "Компилятор не позволяет такой сценарий", "Все методы вызываются параллельно"], "correct": 1, "explanation": "При вызове multicast-делегата методы выполняются последовательно. Если один из них бросает исключение — цепочка прерывается, и остальные методы не вызываются. Для надёжности каждый вызов можно обернуть в try/catch, итерируя по GetInvocationList()."})

post(lid, "mini_task", {"language": "csharp", "title": "Делегат-фильтр", "description": "Реализуйте лямбду для делегата `filter`, которая возвращает `true` только для чисел, кратных 3 И больших 10.", "starterCode": "using System;\nusing System.Linq;\n\nvar numbers = new[] {3, 6, 12, 15, 9, 21, 4, 18};\nFunc<int, bool> filter = ___; // кратно 3 И > 10\nvar result = numbers.Where(filter).ToArray();\nConsole.WriteLine(string.Join(\", \", result));", "expectedOutput": "12, 15, 21, 18"})

post(lid, "knowledge_check", {"topic": "Делегаты", "question": "Какой модификатор доступа у типа delegate по умолчанию, если объявлен на уровне пространства имён?", "options": ["private", "internal", "public", "protected"], "correct": 1, "explanation": "Типы верхнего уровня (класс, делегат, интерфейс) по умолчанию имеют модификатор internal — доступны только внутри сборки. Для публичного API нужно явно указать public."})

# =====================================================================
# Lesson 952: Func<T> и Action<T>
# =====================================================================
print("\n=== Lesson 952: Func<T> и Action<T> ===")
lid = 952

post(lid, "text", {"markdown": "## Func\\<T\\> и Action\\<T\\>\n\nC# предоставляет набор готовых делегатов в пространстве имён `System`:\n\n| Делегат | Описание |\n|---------|----------|\n| `Action` | Метод без параметров, без возврата |\n| `Action<T>` | Метод с 1 параметром, без возврата |\n| `Action<T1,T2,...>` | До 16 параметров, без возврата |\n| `Func<TResult>` | Метод без параметров, возвращает TResult |\n| `Func<T,TResult>` | Метод с параметром T, возвращает TResult |\n| `Predicate<T>` | Аналог `Func<T, bool>` |\n\n> Последний тип-параметр у `Func<>` — всегда тип возвращаемого значения."})

post(lid, "code", {"language": "csharp", "title": "Action и Func в действии", "code": "using System;\nusing System.Collections.Generic;\n\nclass Program\n{\n    // Принимает Action — действие без возврата\n    static void ForEach<T>(IEnumerable<T> items, Action<T> action)\n    {\n        foreach (var item in items)\n            action(item);\n    }\n\n    // Принимает Func — преобразование с возвратом\n    static List<TOut> Map<TIn, TOut>(IEnumerable<TIn> items, Func<TIn, TOut> transform)\n    {\n        var result = new List<TOut>();\n        foreach (var item in items)\n            result.Add(transform(item));\n        return result;\n    }\n\n    static void Main()\n    {\n        var nums = new[] {1, 2, 3, 4, 5};\n\n        ForEach(nums, n => Console.Write(n + \" \")); // 1 2 3 4 5\n        Console.WriteLine();\n\n        var doubled = Map(nums, n => n * 2);\n        ForEach(doubled, n => Console.Write(n + \" \")); // 2 4 6 8 10\n    }\n}"})

post(lid, "prediction", {"language": "csharp", "code": "using System;\n\nFunc<int, int, int> max = (a, b) => a > b ? a : b;\nFunc<int, bool> isPositive = x => x > 0;\nAction<bool> print = b => Console.WriteLine(b ? \"yes\" : \"no\");\n\nprint(isPositive(max(-3, 5)));", "expectedOutput": "yes", "hint": "Вычислите изнутри наружу: max(-3,5)=5, isPositive(5)=true, print(true) выводит \"yes\"."})

post(lid, "mini_task", {"language": "csharp", "title": "Цепочка Func", "description": "Напишите лямбду `transform`, которая принимает строку, обрезает пробелы и переводит в верхний регистр.", "starterCode": "using System;\n\nFunc<string, string> transform = ___;\nConsole.WriteLine(transform(\"  hello world  \"));\nConsole.WriteLine(transform(\" csharp \"));", "expectedOutput": "HELLO WORLD\nCSHARP"})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "using System;\n\nclass Program\n{\n    static void Apply(int[] arr, Action<int> action)\n    {\n        for (int i = 0; i < arr.Length; i++)\n            action(arr[i]);\n    }\n\n    static void Main()\n    {\n        var nums = new[] {10, 20, 30};\n        // Хотим изменить каждый элемент массива, умножив на 2\n        Apply(nums, x => x * 2);\n        Console.WriteLine(string.Join(\", \", nums)); // ожидаем: 20, 40, 60\n    }\n}", "expectedOutput": "20, 40, 60", "hint": "Action<int> получает копию значения. Чтобы изменить массив, нужна другая сигнатура — либо передать индекс, либо использовать ref."})

post(lid, "knowledge_check", {"topic": "Func и Action", "question": "Какой из вариантов корректно объявляет делегат, принимающий два int и возвращающий bool?", "options": ["Action<int, int, bool>", "Func<int, int, bool>", "Predicate<int, int>", "Func<bool, int, int>"], "correct": 1, "explanation": "У Func<> последний тип-параметр — тип возврата. Func<int, int, bool> — два входных параметра int и возвращает bool. Predicate<T> принимает ровно один параметр."})

# =====================================================================
# Lesson 953: Лямбда-выражения
# =====================================================================
print("\n=== Lesson 953: Лямбда-выражения ===")
lid = 953

post(lid, "text", {"markdown": "## Лямбда-выражения\n\nЛямбда — краткий синтаксис для создания анонимных функций:\n\n```csharp\n// Выражение-лямбда (expression lambda)\n(int x) => x * x\n\n// С выводом типа\nx => x * x\n\n// Блочная лямбда (statement lambda)\n(int x, int y) => {\n    int sum = x + y;\n    return sum;\n};\n\n// Без параметров\n() => Console.WriteLine(\"Hello\")\n```\n\n### LINQ и лямбды\nЛямбды — основа LINQ-запросов: `Where`, `Select`, `OrderBy` и другие методы принимают `Func<>` и `Predicate<>`."})

post(lid, "code", {"language": "csharp", "title": "Лямбды в LINQ", "code": "using System;\nusing System.Linq;\n\nvar products = new[]\n{\n    (Name: \"Apple\",  Price: 50.0),\n    (Name: \"Banana\", Price: 30.0),\n    (Name: \"Cherry\", Price: 120.0),\n    (Name: \"Date\",   Price: 200.0),\n};\n\n// Фильтрация + сортировка + проекция\nvar result = products\n    .Where(p => p.Price >= 50)\n    .OrderByDescending(p => p.Price)\n    .Select(p => $\"{p.Name}: {p.Price:F0} руб.\");\n\nforeach (var line in result)\n    Console.WriteLine(line);"})

post(lid, "prediction", {"language": "csharp", "code": "using System;\nusing System.Linq;\n\nvar nums = new[] {1, 2, 3, 4, 5, 6, 7, 8};\n\nvar q = nums\n    .Where(n => n % 2 == 0)\n    .Select(n => n * n);\n\nConsole.WriteLine(string.Join(\", \", q));", "expectedOutput": "4, 16, 36, 64", "hint": "Сначала фильтруются чётные: 2,4,6,8 — затем каждый возводится в квадрат."})

post(lid, "mini_task", {"language": "csharp", "title": "Лямбда-сортировка", "description": "Напишите лямбду для сортировки слов по длине (по убыванию), а при равной длине — по алфавиту.", "starterCode": "using System;\nusing System.Linq;\n\nvar words = new[] {\"cat\", \"elephant\", \"dog\", \"ant\", \"bear\", \"fox\"};\nvar sorted = words.OrderBy(___).ThenBy(___);\nConsole.WriteLine(string.Join(\", \", sorted));", "expectedOutput": "elephant, bear, cat, dog, fox, ant"})

post(lid, "code_to_intent", {"code": "Func<string, string, string> merge = (a, b) =>\n    string.Join(\" \", new[] {a, b}\n        .Select(s => char.ToUpper(s[0]) + s[1..].ToLower()));", "options": ["Объединяет две строки через пробел, каждую капитализируя", "Переводит обе строки в верхний регистр", "Сортирует символы строки по алфавиту"], "correctIntent": 0})

post(lid, "transfer_test", {"question": "У вас есть `List<Order>` с полем `Amount`. Какой LINQ-вызов вернёт сумму заказов дороже 1000?", "options": ["orders.Where(o => o.Amount > 1000).Sum(o => o.Amount)", "orders.Sum(o => o.Amount > 1000)", "orders.Select(o => o.Amount > 1000).Count()"], "correct": 0, "explanation": "Where фильтрует, Sum агрегирует. Второй и третий варианты синтаксически или логически некорректны."})

# =====================================================================
# Lesson 954: Замыкания
# =====================================================================
print("\n=== Lesson 954: Замыкания ===")
lid = 954

post(lid, "text", {"markdown": "## Замыкания (Closures)\n\nЗамыкание — это функция, которая **захватывает переменные** из окружающего контекста. Захваченная переменная остаётся живой, пока жива функция.\n\n```csharp\nint counter = 0;\nAction increment = () => counter++; // захватывает counter\n\nincrement();\nincrement();\nConsole.WriteLine(counter); // 2\n```\n\n> Важно: захватывается **переменная**, а не её значение в момент создания лямбды!"})

post(lid, "prediction", {"language": "csharp", "code": "using System;\nusing System.Collections.Generic;\n\nvar actions = new List<Action>();\n\nfor (int i = 0; i < 3; i++)\n    actions.Add(() => Console.WriteLine(i));\n\nforeach (var a in actions)\n    a();", "expectedOutput": "3\n3\n3", "hint": "Все лямбды захватывают одну и ту же переменную i. К моменту вызова цикл завершился и i == 3. Классическая ловушка замыканий в цикле."})

post(lid, "code", {"language": "csharp", "title": "Исправление: копия переменной цикла", "code": "using System;\nusing System.Collections.Generic;\n\nvar actions = new List<Action>();\n\nfor (int i = 0; i < 3; i++)\n{\n    int copy = i;                          // создаём новую переменную на каждой итерации\n    actions.Add(() => Console.WriteLine(copy));\n}\n\nforeach (var a in actions)\n    a();\n// Вывод: 0, 1, 2"})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "using System;\n\nclass Program\n{\n    static Func<int> MakeCounter()\n    {\n        int count = 0;\n        return () => count;   // ОШИБКА: не увеличивает счётчик\n    }\n\n    static void Main()\n    {\n        var counter = MakeCounter();\n        Console.WriteLine(counter()); // ожидаем 1\n        Console.WriteLine(counter()); // ожидаем 2\n        Console.WriteLine(counter()); // ожидаем 3\n    }\n}", "expectedOutput": "1\n2\n3", "hint": "Лямбда должна инкрементировать count перед возвратом. Используйте ++count."})

post(lid, "mutation", {"language": "csharp", "baseCode": "Func<int> makeAdder(int x)\n{\n    return () => x + 10;\n}\nvar add = makeAdder(5);\nConsole.WriteLine(add()); // 15", "mutation": "Измените так, чтобы каждый вызов add() увеличивал накопленную сумму на x (т.е. первый вызов даёт x, второй 2x, третий 3x).", "expectedOutput": "5\n10\n15", "hint": "Объявите переменную-аккумулятор внутри makeAdder и захватите её в лямбде."})

post(lid, "knowledge_check", {"topic": "Замыкания", "question": "Замыкание захватывает переменную по ссылке или по значению?", "options": ["По значению на момент создания лямбды", "По ссылке (живёт, пока жива лямбда)", "Зависит от типа переменной (value/reference type)", "Всегда копирует значение в момент первого вызова"], "correct": 1, "explanation": "C# замыкания захватывают переменную по ссылке. Компилятор создаёт скрытый класс, который хранит эту переменную, и все лямбды, захватившие её, работают с одним и тем же полем этого класса."})

# =====================================================================
# Lesson 955: Событие event
# =====================================================================
print("\n=== Lesson 955: Событие event ===")
lid = 955

post(lid, "text", {"markdown": "## События (event) в C#\n\nСобытие — это механизм уведомления: **издатель** (publisher) сообщает подписчикам (subscribers) что что-то произошло.\n\n```csharp\n// Объявление события\npublic event EventHandler<MyArgs> SomethingHappened;\n\n// Вызов (только внутри класса-издателя)\nSomethingHappened?.Invoke(this, new MyArgs(...));\n\n// Подписка\npublisher.SomethingHappened += OnSomethingHappened;\n\n// Отписка\npublisher.SomethingHappened -= OnSomethingHappened;\n```\n\n### Ключевое слово event\n- Снаружи класса можно только `+=` и `-=`\n- Прямой вызов и присваивание `=` запрещены\n- Защищает от «угона» всего списка подписчиков"})

post(lid, "code", {"language": "csharp", "title": "Паттерн Publisher/Subscriber", "code": "using System;\n\npublic class TemperatureSensor\n{\n    private double _temperature;\n\n    // Стандартный шаблон: EventHandler<TArgs>\n    public event EventHandler<double> TemperatureChanged;\n\n    public double Temperature\n    {\n        get => _temperature;\n        set\n        {\n            if (Math.Abs(value - _temperature) > 0.5)\n            {\n                _temperature = value;\n                TemperatureChanged?.Invoke(this, value); // уведомляем\n            }\n        }\n    }\n}\n\nclass Program\n{\n    static void Main()\n    {\n        var sensor = new TemperatureSensor();\n\n        // Подписка лямбдой\n        sensor.TemperatureChanged += (s, temp) =>\n            Console.WriteLine($\"Температура изменилась: {temp:F1}°C\");\n\n        sensor.Temperature = 20.0;\n        sensor.Temperature = 20.3; // изменение < 0.5, событие не сработает\n        sensor.Temperature = 22.0;\n        sensor.Temperature = 19.0;\n    }\n}"})

post(lid, "code_to_intent", {"code": "sensor.TemperatureChanged += (s, temp) => Console.WriteLine(temp);\n// ... later ...\nsensor.TemperatureChanged -= (s, temp) => Console.WriteLine(temp);", "options": ["Подписывается и затем успешно отписывается от события", "Подписывается дважды, потому что лямбды — разные объекты", "Подписывается, а отписка не сработает — это разные объекты-лямбды"], "correctIntent": 2})

post(lid, "code", {"language": "csharp", "title": "Правильная отписка через именованный метод", "code": "using System;\n\nclass Program\n{\n    static void OnChanged(object sender, double temp)\n        => Console.WriteLine($\"Handler: {temp:F1}°C\");\n\n    static void Main()\n    {\n        var sensor = new TemperatureSensor();\n\n        sensor.TemperatureChanged += OnChanged; // подписка\n        sensor.Temperature = 25.0;              // сработает\n\n        sensor.TemperatureChanged -= OnChanged; // отписка — работает!\n        sensor.Temperature = 30.0;              // не сработает\n    }\n}\n\n// TemperatureSensor — из предыдущего примера\npublic class TemperatureSensor\n{\n    private double _temperature;\n    public event EventHandler<double> TemperatureChanged;\n    public double Temperature\n    {\n        get => _temperature;\n        set { if (Math.Abs(value - _temperature) > 0.5) { _temperature = value; TemperatureChanged?.Invoke(this, value); } }\n    }\n}"})

post(lid, "quiz", {"question": "Что произойдёт, если вызвать событие без проверки на null: `TemperatureChanged.Invoke(this, value)` при отсутствии подписчиков?", "options": ["Ничего, событие просто проигнорируется", "NullReferenceException", "Compile-time error", "ArgumentException"], "correct": 1, "explanation": "Если ни один подписчик не зарегистрирован, делегат события равен null. Вызов .Invoke() на null бросает NullReferenceException. Безопасный вариант — `TemperatureChanged?.Invoke(this, value)` с оператором ?."})

post(lid, "knowledge_check", {"topic": "Events", "question": "Чем `event` отличается от обычного поля-делегата?", "options": ["event нельзя сделать static", "Снаружи класса event доступен только через += и -=, прямое присваивание и вызов запрещены", "event всегда асинхронный", "event не поддерживает лямбды"], "correct": 1, "explanation": "Ключевое слово event инкапсулирует делегат: внешний код может только подписываться (+= ) и отписываться (-= ). Это предотвращает случайный сброс всех подписчиков через = и прямой вызов события."})

# =====================================================================
# Lesson 956: Анонимные типы
# =====================================================================
print("\n=== Lesson 956: Анонимные типы ===")
lid = 956

post(lid, "text", {"markdown": "## Анонимные типы\n\nАнонимный тип позволяет быстро создать объект с набором readonly-свойств без явного объявления класса.\n\n```csharp\nvar person = new { Name = \"Alice\", Age = 30 };\nConsole.WriteLine(person.Name); // Alice\n```\n\n### Особенности\n- Тип генерируется компилятором, имя недоступно в коде\n- Все свойства **readonly**\n- Два анонимных типа с одинаковыми именами и порядком свойств — один тип\n- Главным образом используются в LINQ-проекциях\n- В C# 7+ часто заменяются **кортежами** (tuples)"})

post(lid, "code", {"language": "csharp", "title": "Анонимные типы в LINQ-проекции", "code": "using System;\nusing System.Linq;\n\nvar employees = new[]\n{\n    new { Name = \"Alice\", Dept = \"Dev\",  Salary = 120_000 },\n    new { Name = \"Bob\",   Dept = \"HR\",   Salary = 80_000  },\n    new { Name = \"Carol\", Dept = \"Dev\",  Salary = 130_000 },\n    new { Name = \"Dave\",  Dept = \"HR\",   Salary = 75_000  },\n};\n\nvar summary = employees\n    .GroupBy(e => e.Dept)\n    .Select(g => new { Dept = g.Key, AvgSalary = g.Average(e => e.Salary) });\n\nforeach (var row in summary)\n    Console.WriteLine($\"{row.Dept}: {row.AvgSalary:F0}\");"})

post(lid, "compare_variants", {"question": "Анонимный тип vs кортеж (ValueTuple) — что выбрать для временной проекции?", "variants": [{"title": "Анонимный тип", "code": "var point = new { X = 1, Y = 2 };\nConsole.WriteLine(point.X);"}, {"title": "Кортеж (ValueTuple)", "code": "var point = (X: 1, Y: 2);\nConsole.WriteLine(point.X);\n// Можно вернуть из метода:\n(int X, int Y) GetPoint() => (1, 2);"}], "correct": 1, "explanation": "Кортежи предпочтительнее начиная с C# 7: их можно возвращать из методов, деконструировать, использовать как параметры. Анонимные типы ограничены локальным контекстом и не могут выходить за пределы метода без object или dynamic."})

post(lid, "prediction", {"language": "csharp", "code": "var a = new { X = 1, Y = 2 };\nvar b = new { X = 1, Y = 2 };\nConsole.WriteLine(a.Equals(b));\nConsole.WriteLine(ReferenceEquals(a, b));", "expectedOutput": "True\nFalse", "hint": "Анонимные типы переопределяют Equals и GetHashCode на основе значений свойств. Equals вернёт true, но это разные объекты."})

post(lid, "knowledge_check", {"topic": "Анонимные типы", "question": "Можно ли изменить свойство анонимного типа после создания?", "options": ["Да, через обычное присваивание", "Нет, все свойства readonly", "Да, но только через рефлексию", "Зависит от типа свойства (value vs reference)"], "correct": 1, "explanation": "Свойства анонимного типа всегда генерируются как readonly. Компилятор не создаёт сеттеры. Это делает анонимные типы неизменяемыми по дизайну."})

post(lid, "transfer_test", {"question": "Где анонимный тип НЕ может быть использован?", "options": ["Как локальная переменная в методе", "Как тип возвращаемого значения метода (без dynamic)", "Как элемент массива с var"], "correct": 1, "explanation": "Анонимные типы нельзя указать как возвращаемый тип метода — имя типа недоступно в коде. Можно только вернуть object или dynamic, что теряет типобезопасность."})

# =====================================================================
# Lesson 957: try / catch / finally
# =====================================================================
print("\n=== Lesson 957: try / catch / finally ===")
lid = 957

post(lid, "text", {"markdown": "## try / catch / finally\n\nМеханизм обработки исключений в C#:\n\n```csharp\ntry\n{\n    // код, который может бросить исключение\n}\ncatch (SpecificException ex)\n{\n    // обработка конкретного типа\n}\ncatch (Exception ex)\n{\n    // обработка любого исключения\n}\nfinally\n{\n    // выполняется ВСЕГДА: и при успехе, и при исключении\n}\n```\n\n### Ключевые правила\n- `finally` выполняется всегда (кроме `Environment.FailFast`)\n- Можно несколько `catch` — проверяются сверху вниз\n- Более специфичные типы должны быть выше\n- `catch` без типа (голый `catch`) ловит все, включая неуправляемые"})

post(lid, "drag_order", {"pieces": ["finally { освобождаем ресурсы }", "catch (FormatException ex) { ... }", "try { парсим ввод }", "catch (Exception ex) { ... }"], "solution": [2, 1, 3, 0], "hint": "Сначала try, затем catch от более специфичного к общему, в конце finally."})

post(lid, "code", {"language": "csharp", "title": "Классический try/catch/finally", "code": "using System;\nusing System.IO;\n\nclass Program\n{\n    static void Main()\n    {\n        StreamReader reader = null;\n        try\n        {\n            reader = new StreamReader(\"data.txt\");\n            string content = reader.ReadToEnd();\n            int value = int.Parse(content.Trim());\n            Console.WriteLine($\"Значение: {value}\");\n        }\n        catch (FileNotFoundException)\n        {\n            Console.WriteLine(\"Файл не найден\");\n        }\n        catch (FormatException)\n        {\n            Console.WriteLine(\"Файл содержит не число\");\n        }\n        finally\n        {\n            reader?.Close(); // выполнится всегда\n            Console.WriteLine(\"Ресурсы освобождены\");\n        }\n    }\n}"})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "using System;\n\nclass Program\n{\n    static int Divide(int a, int b)\n    {\n        try\n        {\n            return a / b;\n        }\n        catch (Exception ex)\n        {\n            Console.WriteLine($\"Ошибка: {ex.Message}\");\n            return 0;\n        }\n        catch (DivideByZeroException)\n        {\n            Console.WriteLine(\"Деление на ноль!\");\n            return -1;\n        }\n    }\n\n    static void Main() => Console.WriteLine(Divide(10, 0));\n}", "expectedOutput": "Деление на ноль!\n-1", "hint": "Более специфичный catch (DivideByZeroException) никогда не сработает, потому что стоит после общего catch (Exception). Поменяйте их местами."})

post(lid, "prediction", {"language": "csharp", "code": "using System;\n\ntry\n{\n    Console.WriteLine(\"try\");\n    throw new Exception(\"oops\");\n}\ncatch (Exception ex)\n{\n    Console.WriteLine(\"catch: \" + ex.Message);\n    return;\n}\nfinally\n{\n    Console.WriteLine(\"finally\");\n}", "expectedOutput": "try\ncatch: oops\nfinally", "hint": "finally выполняется даже при return из catch."})

post(lid, "knowledge_check", {"topic": "try/catch/finally", "question": "Что произойдёт, если в блоке finally бросить новое исключение, когда уже обрабатывается другое?", "options": ["Оба исключения будут упакованы в AggregateException", "Исходное исключение будет потеряно, распространится исключение из finally", "Исключение из finally игнорируется", "Compile-time warning"], "correct": 1, "explanation": "Если в finally бросить исключение во время обработки другого, исходное исключение теряется. Это классическая ловушка — никогда не бросайте исключения из finally намеренно."})

# =====================================================================
# Lesson 958: Иерархия исключений
# =====================================================================
print("\n=== Lesson 958: Иерархия исключений ===")
lid = 958

post(lid, "text", {"markdown": "## Иерархия исключений .NET\n\n```\nObject\n└── Exception\n    ├── SystemException\n    │   ├── ArgumentException\n    │   │   ├── ArgumentNullException\n    │   │   └── ArgumentOutOfRangeException\n    │   ├── InvalidOperationException\n    │   ├── NullReferenceException\n    │   ├── IndexOutOfRangeException\n    │   ├── IOException\n    │   │   ├── FileNotFoundException\n    │   │   └── DirectoryNotFoundException\n    │   ├── DivideByZeroException\n    │   └── OverflowException\n    └── ApplicationException  ← устаревший базовый класс\n```\n\n> Catch ловит указанный тип **и все его наследники**. `catch (Exception)` поймает всё."})

post(lid, "code", {"language": "csharp", "title": "Catch по иерархии", "code": "using System;\n\nstatic void Process(string input)\n{\n    if (input == null)\n        throw new ArgumentNullException(nameof(input));\n    if (input.Length == 0)\n        throw new ArgumentException(\"Пустая строка\", nameof(input));\n\n    Console.WriteLine(int.Parse(input));\n}\n\ntry { Process(null); }\ncatch (ArgumentNullException ex) { Console.WriteLine($\"Null: {ex.ParamName}\"); }\ncatch (ArgumentException ex)     { Console.WriteLine($\"Arg:  {ex.Message}\"); }\ncatch (FormatException)          { Console.WriteLine(\"Не число\"); }"})

post(lid, "code_to_intent", {"code": "catch (IOException ex) when (ex.Message.Contains(\"disk full\"))\n{\n    Console.WriteLine(\"Нет места на диске\");\n}", "options": ["Ловит любое IOException и проверяет сообщение внутри блока", "Ловит IOException только если сообщение содержит 'disk full' — фильтрует на уровне catch", "Ловит все исключения и проверяет тип внутри"], "correctIntent": 1})

post(lid, "quiz", {"question": "Какое исключение НЕ является наследником ArgumentException?", "options": ["ArgumentNullException", "ArgumentOutOfRangeException", "InvalidOperationException", "Все перечисленные являются наследниками"], "correct": 2, "explanation": "InvalidOperationException наследует от SystemException напрямую, а не от ArgumentException. ArgumentNullException и ArgumentOutOfRangeException — прямые наследники ArgumentException."})

post(lid, "semantic_diff", {"intendedCode": "// Хотели поймать только ошибки формата\ntry { var n = int.Parse(input); }\ncatch (FormatException) { /* обработка */ }", "actualCode": "// Написали слишком широко\ntry { var n = int.Parse(input); }\ncatch (Exception) { /* обработка */ }", "explanation": "Слишком широкий catch (Exception) поглотит всё: OutOfMemoryException, ThreadAbortException — вещи, которые программа не должна перехватывать. Всегда ловите наиболее специфичный тип."})

post(lid, "transfer_test", {"question": "Метод открывает файл и может бросить FileNotFoundException или UnauthorizedAccessException. Какой catch написать, чтобы обработать оба случая одним блоком?", "options": ["catch (Exception ex)", "catch (IOException ex)", "catch (SystemException ex)"], "correct": 1, "explanation": "FileNotFoundException и UnauthorizedAccessException оба наследуют IOException. Поймать их одним блоком через catch (IOException) — точный и семантически правильный вариант."})

# =====================================================================
# Lesson 959: Кастомные исключения
# =====================================================================
print("\n=== Lesson 959: Кастомные исключения ===")
lid = 959

post(lid, "text", {"markdown": "## Кастомные исключения\n\nСоздавайте собственные классы исключений когда:\n- Нужно передать дополнительный контекст об ошибке\n- Хотите, чтобы вызывающий код мог поймать именно ваш тип\n- Стандартные исключения не описывают ситуацию точно\n\n### Шаблон корректного кастомного исключения\n\n```csharp\n[Serializable]\npublic class OrderException : Exception\n{\n    public int OrderId { get; }\n\n    public OrderException(int orderId, string message)\n        : base(message)\n    {\n        OrderId = orderId;\n    }\n\n    public OrderException(int orderId, string message, Exception inner)\n        : base(message, inner)\n    {\n        OrderId = orderId;\n    }\n}\n```\n\n> Всегда реализуйте конструктор с `inner` (InnerException) для сохранения цепочки ошибок."})

post(lid, "code", {"language": "csharp", "title": "Кастомное исключение с контекстом", "code": "using System;\n\npublic class ValidationException : Exception\n{\n    public string FieldName { get; }\n    public object InvalidValue { get; }\n\n    public ValidationException(string fieldName, object value, string message)\n        : base(message)\n    {\n        FieldName = fieldName;\n        InvalidValue = value;\n    }\n\n    public ValidationException(string fieldName, object value, string message, Exception inner)\n        : base(message, inner) { FieldName = fieldName; InvalidValue = value; }\n}\n\nclass OrderService\n{\n    public void PlaceOrder(int quantity)\n    {\n        if (quantity <= 0)\n            throw new ValidationException(\n                nameof(quantity), quantity,\n                $\"Количество должно быть > 0, получено: {quantity}\");\n\n        Console.WriteLine($\"Заказ на {quantity} единиц принят\");\n    }\n}\n\nclass Program\n{\n    static void Main()\n    {\n        var service = new OrderService();\n        try\n        {\n            service.PlaceOrder(-5);\n        }\n        catch (ValidationException ex)\n        {\n            Console.WriteLine($\"Поле '{ex.FieldName}': {ex.Message}\");\n            Console.WriteLine($\"Некорректное значение: {ex.InvalidValue}\");\n        }\n    }\n}"})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "public class AppException : Exception\n{\n    public AppException(string message) : base(message) { }\n    // Используется так:\n    // throw new AppException(\"что-то пошло не так\");\n    // но InnerException всегда null даже когда ловим другое исключение\n}\n\nstatic void Load()\n{\n    try { /* ... IO операция ... */ throw new System.IO.IOException(\"disk error\"); }\n    catch (Exception ex)\n    {\n        // теряем исходное исключение!\n        throw new AppException(\"Не удалось загрузить данные\");\n    }\n}", "expectedOutput": "AppException с InnerException == IOException(\"disk error\")", "hint": "Добавьте конструктор AppException(string message, Exception inner) и передавайте ex как inner при пробросе."})

post(lid, "mini_task", {"language": "csharp", "title": "Создание доменного исключения", "description": "Создайте класс `InsufficientFundsException` с полем `decimal Shortfall`. Выбросьте его, если баланс < суммы списания.", "starterCode": "using System;\n\n// TODO: объявите InsufficientFundsException\n\nclass BankAccount\n{\n    public decimal Balance { get; private set; } = 100m;\n\n    public void Withdraw(decimal amount)\n    {\n        // TODO: бросьте InsufficientFundsException если не хватает средств\n        Balance -= amount;\n    }\n}\n\nclass Program\n{\n    static void Main()\n    {\n        var acc = new BankAccount();\n        try { acc.Withdraw(150m); }\n        catch (InsufficientFundsException ex)\n        { Console.WriteLine($\"Не хватает: {ex.Shortfall}\"); }\n    }\n}", "expectedOutput": "Не хватает: 50"})

post(lid, "knowledge_check", {"topic": "Кастомные исключения", "question": "От какого класса рекомендуется наследовать кастомные исключения в современном C#?", "options": ["ApplicationException", "Exception", "SystemException", "BaseException"], "correct": 1, "explanation": "Наследоваться нужно от Exception. ApplicationException изначально задумывался как базовый для пользовательских исключений, но Microsoft признала это решение ошибочным — теперь он не несёт семантического смысла."})

# =====================================================================
# Lesson 960: throw, rethrow и when
# =====================================================================
print("\n=== Lesson 960: throw, rethrow и when ===")
lid = 960

post(lid, "text", {"markdown": "## throw, rethrow и when\n\n### throw vs throw ex\n\n```csharp\ncatch (Exception ex)\n{\n    throw ex;  // ПЛОХО: сбрасывает stack trace, теряем исходное место ошибки\n    throw;     // ХОРОШО: сохраняет оригинальный stack trace\n}\n```\n\n### Exception filters (when)\n\n```csharp\ncatch (HttpException ex) when (ex.StatusCode == 503)\n{\n    // поймает только 503, остальные HttpException пройдут мимо\n}\n```\n\nФильтр `when` вычисляется **до** входа в catch-блок и не сбрасывает stack trace при отказе."})

post(lid, "two_path", {"question": "Как правильно пробросить исключение дальше с сохранением стека?", "pathA": {"title": "throw ex (НЕПРАВИЛЬНО)", "code": "catch (Exception ex)\n{\n    Log(ex);\n    throw ex; // stack trace обрезается до этой строки!\n}"}, "pathB": {"title": "throw (ПРАВИЛЬНО)", "code": "catch (Exception ex)\n{\n    Log(ex);\n    throw; // сохраняет оригинальный stack trace\n}"}, "explanation": "throw ex создаёт новое исключение с текущей строкой как точкой бросания — вы теряете исходный стек. throw без аргумента пробрасывает то же исключение, не изменяя его StackTrace. Это критично для диагностики."})

post(lid, "code", {"language": "csharp", "title": "Exception filter with when", "code": "using System;\nusing System.Net;\nusing System.Net.Http;\n\nasync Task<string> FetchAsync(string url)\n{\n    try\n    {\n        using var client = new HttpClient();\n        return await client.GetStringAsync(url);\n    }\n    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)\n    {\n        Console.WriteLine(\"404 — ресурс не найден\");\n        return null;\n    }\n    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)\n    {\n        Console.WriteLine(\"503 — сервис недоступен, попробуйте позже\");\n        throw; // пробрасываем для повтора на уровне выше\n    }\n    // другие HttpRequestException не поймаются — распространяются дальше\n}"})

post(lid, "prediction", {"language": "csharp", "code": "using System;\n\nstatic void Inner()\n{\n    throw new InvalidOperationException(\"original\");\n}\n\nstatic void Outer()\n{\n    try { Inner(); }\n    catch (Exception ex)\n    {\n        Console.WriteLine(\"caught\");\n        throw; // rethrow\n    }\n}\n\ntry { Outer(); }\ncatch (Exception ex) { Console.WriteLine(ex.Message); }", "expectedOutput": "caught\noriginal", "hint": "throw без аргумента пробрасывает то же исключение. Сообщение остаётся 'original'."})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "using System;\n\nstatic void Process()\n{\n    try\n    {\n        throw new Exception(\"deep error\");\n    }\n    catch (Exception ex)\n    {\n        Console.WriteLine(\"Processing failed\");\n        throw ex; // ОШИБКА: теряем оригинальный stack trace\n    }\n}", "expectedOutput": "Process() с сохранённым оригинальным stack trace", "hint": "Замените 'throw ex' на 'throw' для сохранения оригинального стека."})

post(lid, "knowledge_check", {"topic": "throw/rethrow/when", "question": "Что делает `when` в catch-блоке?", "options": ["Это синтаксический сахар для if внутри catch, поведение одинаковое", "Фильтрует исключение до входа в catch, не сбрасывает stack trace при отказе фильтра", "Позволяет поймать несколько типов исключений через OR", "Устаревший синтаксис, заменён is pattern matching"], "correct": 1, "explanation": "when — это exception filter. Он вычисляется вне catch-блока, поэтому если условие ложно, исключение продолжает распространяться как ненайманное (без сброса стека). Это отличает его от if внутри catch."})

# =====================================================================
# Lesson 961: Best practices обработки ошибок
# =====================================================================
print("\n=== Lesson 961: Best practices обработки ошибок ===")
lid = 961

post(lid, "text", {"markdown": "## Best Practices обработки исключений\n\n### Золотые правила\n\n1. **Ловите конкретные типы** — не `catch (Exception)` без причины\n2. **Не глотайте исключения** — пустой catch скрывает баги\n3. **Используйте throw, не throw ex** — сохраняйте stack trace\n4. **finally для ресурсов** — или лучше `using`\n5. **Логируйте с контекстом** — сохраняйте `ex.ToString()`, не только `ex.Message`\n6. **Fail fast** — не продолжайте работу в некорректном состоянии\n7. **Не используйте исключения для потока управления** — они дорогие"})

post(lid, "semantic_diff", {"intendedCode": "// Корректная обработка: логируем и пробрасываем\ncatch (DbException ex)\n{\n    _logger.LogError(ex, \"DB error for user {UserId}\", userId);\n    throw; // caller решает что делать\n}", "actualCode": "// Проблемный код: глотаем исключение\ncatch (DbException ex)\n{\n    _logger.LogError(ex.Message); // теряем stack trace\n    // ничего не throw — caller думает всё ОК\n}", "explanation": "Пустой catch без throw скрывает ошибку. Caller получает null или дефолтное значение и продолжает работу в некорректном состоянии. Логируйте ex.ToString() или передавайте ex в LogError — это сохраняет полный стек."})

post(lid, "fix_bug", {"language": "csharp", "buggyCode": "using System;\nusing System.IO;\n\nclass FileProcessor\n{\n    public string ReadConfig(string path)\n    {\n        try\n        {\n            return File.ReadAllText(path);\n        }\n        catch (Exception)\n        {\n            // молча игнорируем любую ошибку\n            return null;\n        }\n    }\n}", "expectedOutput": "FileNotFoundException явно обрабатывается, остальные исключения пробрасываются", "hint": "Ловите только FileNotFoundException с осмысленной обработкой. Общий catch (Exception) без действий — антипаттерн «swallowing exceptions»."})

post(lid, "code", {"language": "csharp", "title": "using вместо try/finally для ресурсов", "code": "using System;\nusing System.IO;\n\nclass Program\n{\n    // ПЛОХО: ручное управление ресурсом\n    static string ReadBad(string path)\n    {\n        StreamReader r = null;\n        try   { r = new StreamReader(path); return r.ReadToEnd(); }\n        finally { r?.Dispose(); }\n    }\n\n    // ХОРОШО: using гарантирует Dispose даже при исключении\n    static string ReadGood(string path)\n    {\n        using var r = new StreamReader(path);\n        return r.ReadToEnd();\n    }\n\n    // ЕЩЁ ЛУЧШЕ: modern C# using declaration (C# 8+)\n    static string ReadModern(string path)\n    {\n        using var reader = new StreamReader(path); // Dispose в конце блока\n        return reader.ReadToEnd();\n    }\n}"})

post(lid, "transfer_test", {"question": "Приложение получает HttpRequestException из внешнего API. Правильная стратегия:", "options": ["catch (Exception ex) { return null; } — не ломаем пользователя", "catch (HttpRequestException ex) { logger.LogError(ex, ...); throw new ServiceUnavailableException(..., ex); }", "catch (HttpRequestException) { throw new Exception(ex.Message); } — перепаковываем"], "correct": 1, "explanation": "Правильно: ловить конкретный тип, логировать с полным контекстом, пробросить доменное исключение с InnerException. Вариант 1 глотает ошибку. Вариант 3 теряет оригинальный stack trace и тип."})

post(lid, "checkpoint", {"title": "Best Practices: итоговые правила", "description": "Проверьте, что усвоили ключевые практики обработки исключений в C#", "tasks": ["Используйте catch с конкретным типом, а не catch (Exception) без необходимости", "Никогда не оставляйте пустой catch-блок — это скрывает ошибки", "Используйте throw (без ex) для проброса с сохранением stack trace", "Предпочитайте using для IDisposable-ресурсов вместо try/finally", "Логируйте ex.ToString() или передавайте ex объект в логгер — не только ex.Message", "Создавайте кастомные исключения с InnerException для сохранения цепочки ошибок", "Не используйте исключения для управления потоком выполнения — это медленно и нечитаемо"]})

print("\nDone! All lessons filled.")
