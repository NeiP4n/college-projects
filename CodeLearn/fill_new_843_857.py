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
        print(f"  ERR {e.code}: {e.read()[:200]}")
    print(f"  [{st}] {btype}")


# ===========================================================
# Lesson 936: Массивы: одномерные и многомерные
# ===========================================================
print("\n=== Lesson 936: Массивы ===")

post(936, "text", {"markdown": """## Массивы в C#

**Массив** — это коллекция элементов одного типа, хранящихся в непрерывном блоке памяти.

### Одномерный массив
```csharp
int[] nums = new int[5];          // инициализация по размеру
int[] scores = {90, 85, 72, 88}; // инициализация значениями
```

### Многомерный (прямоугольный) массив
```csharp
int[,] matrix = new int[3, 4];   // 3 строки, 4 столбца
int[,] grid = {{1,2},{3,4},{5,6}};
```

### Зубчатый массив (jagged)
```csharp
int[][] jagged = new int[3][];
jagged[0] = new int[]{1, 2};
jagged[1] = new int[]{3, 4, 5};
```

Доступ по индексу: `matrix[row, col]` для прямоугольных, `jagged[i][j]` для зубчатых."""})

post(936, "code", {
    "language": "csharp",
    "title": "Объявление и обход массивов",
    "code": """// Одномерный массив
int[] temps = {23, 17, 30, 25, 19};
int sum = 0;
foreach (int t in temps)
    sum += t;
Console.WriteLine($"Средняя температура: {sum / temps.Length}°C");

// Двумерный массив — матрица 3x3
int[,] matrix = {
    {1, 2, 3},
    {4, 5, 6},
    {7, 8, 9}
};
// Обход строк и столбцов
for (int i = 0; i < matrix.GetLength(0); i++) {
    for (int j = 0; j < matrix.GetLength(1); j++)
        Console.Write($"{matrix[i,j]} ");
    Console.WriteLine();
}"""
})

post(936, "quiz", {
    "question": "Как получить количество строк в двумерном массиве `int[,] m`?",
    "options": ["m.Length", "m.GetLength(0)", "m.GetLength(1)", "m.Rows"],
    "correct": 1,
    "explanation": "`GetLength(0)` возвращает размер первого измерения (строки), `GetLength(1)` — второго (столбцы). `Length` возвращает общее число элементов."
})

post(936, "mini_task", {
    "language": "csharp",
    "title": "Сумма элементов матрицы",
    "description": "Дана матрица 3x3. Посчитайте сумму всех её элементов и выведите результат.",
    "starterCode": """int[,] matrix = {
    {3,  7,  2},
    {5,  1,  8},
    {4,  6,  9}
};
int total = 0;
// Обойди все строки (i) и столбцы (j), добавляй matrix[i,j] к total

Console.WriteLine(total);""",
    "expectedOutput": "45"
})

post(936, "prediction", {
    "language": "csharp",
    "code": """int[] arr = {10, 20, 30, 40, 50};
Console.WriteLine(arr[2]);
Console.WriteLine(arr.Length);""",
    "expectedOutput": "30\n5",
    "hint": "Индексы массива начинаются с 0. arr[2] — третий элемент."
})

post(936, "fix_bug", {
    "language": "csharp",
    "buggyCode": """int[,] m = new int[2, 3];
for (int i = 0; i <= m.GetLength(0); i++)
    for (int j = 0; j <= m.GetLength(1); j++)
        m[i, j] = i * 3 + j;
Console.WriteLine(m[1, 2]);""",
    "expectedOutput": "5",
    "hint": "Условие цикла `<=` вызовет выход за границы массива. Индексы идут от 0 до Length-1."
})

post(936, "checkpoint", {
    "title": "Итог: Массивы",
    "description": "Проверь, что ты освоил работу с массивами в C#.",
    "tasks": [
        "Объявить и инициализировать одномерный массив",
        "Создать и обойти двумерный массив с помощью GetLength()",
        "Вычислить сумму/среднее по элементам массива"
    ]
})


# ===========================================================
# Lesson 937: List<T>
# ===========================================================
print("\n=== Lesson 937: List<T> ===")

post(937, "text", {"markdown": """## List<T> — динамический список

`List<T>` — обобщённая коллекция с динамическим размером. Хранит элементы в массиве под капотом и автоматически расширяется.

### Основные операции
| Метод | Описание |
|-------|----------|
| `Add(item)` | Добавить в конец |
| `Insert(index, item)` | Вставить по индексу |
| `Remove(item)` | Удалить первое вхождение |
| `RemoveAt(index)` | Удалить по индексу |
| `Contains(item)` | Проверить наличие |
| `Count` | Количество элементов |
| `Sort()` | Сортировка |
| `Find(predicate)` | Поиск по условию |

```csharp
var list = new List<string>();
list.Add("яблоко");
list.Add("банан");
list.Remove("яблоко");
Console.WriteLine(list.Count); // 1
```"""})

post(937, "code", {
    "language": "csharp",
    "title": "Работа со списком студентов",
    "code": """var students = new List<string> {"Анна", "Борис", "Вера", "Григорий"};

// Добавить нового студента
students.Add("Дмитрий");

// Удалить студента
students.Remove("Борис");

// Найти студентов на букву 'В'
var filtered = students.FindAll(s => s.StartsWith("В"));
Console.WriteLine($"Студентов на В: {filtered.Count}");

// Сортировка
students.Sort();
Console.WriteLine("Отсортированный список:");
foreach (var s in students)
    Console.WriteLine($"  {s}");"""
})

post(937, "mini_task", {
    "language": "csharp",
    "title": "Фильтрация цен",
    "description": "Дан список цен. Посчитайте сумму только тех цен, которые больше 10. Выведите результат.",
    "starterCode": """var prices = new List<double> {15.5, 42.0, 8.99, 27.3, 61.0, 3.5};
double total = 0;
foreach (var p in prices) {
    // добавь p к total только если p > 10
}
Console.WriteLine(total);""",
    "expectedOutput": "145.8"
})

post(937, "quiz", {
    "question": "Какой метод List<T> удаляет элемент по его значению (не по индексу)?",
    "options": ["RemoveAt()", "Delete()", "Remove()", "Pop()"],
    "correct": 2,
    "explanation": "`Remove(item)` удаляет первое вхождение элемента по значению. `RemoveAt(index)` удаляет по индексу."
})

post(937, "fill_blank", {
    "template": "var list = new List<___>();\nlist.Add(42);\nConsole.WriteLine(list.___);",
    "blanks": ["int", "Count"],
    "hint": "Первый пропуск — тип элементов, второй — свойство для получения количества."
})

post(937, "mutation", {
    "language": "csharp",
    "baseCode": """var nums = new List<int> {5, 2, 8, 1, 9, 3};
nums.Sort();
foreach (var n in nums)
    Console.Write(n + " ");""",
    "mutation": "Вместо сортировки по возрастанию отфильтруй список: оставь только числа больше 4",
    "expectedOutput": "5 8 9 ",
    "hint": "Используй RemoveAll() или создай новый список через FindAll()."
})


# ===========================================================
# Lesson 938: Dictionary<K,V>
# ===========================================================
print("\n=== Lesson 938: Dictionary<K,V> ===")

post(938, "text", {"markdown": """## Dictionary<K,V> — словарь ключ-значение

`Dictionary<TKey, TValue>` хранит пары «ключ → значение». Поиск по ключу выполняется за O(1) (хэш-таблица).

### Ключевые операции
```csharp
var dict = new Dictionary<string, int>();
dict["яблоко"] = 5;         // добавить / обновить
dict.Add("банан", 3);       // добавить (исключение если ключ есть)

if (dict.ContainsKey("груша"))
    Console.WriteLine(dict["груша"]);

dict.TryGetValue("банан", out int count); // безопасное получение
dict.Remove("яблоко");

foreach (var kv in dict)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

### Когда использовать
- Быстрый поиск по имени, коду, идентификатору
- Подсчёт частоты элементов
- Группировка данных"""})

post(938, "code", {
    "language": "csharp",
    "title": "Подсчёт частоты слов",
    "code": """string text = "кот пёс кот рыба пёс кот";
var words = text.Split(' ');
var freq = new Dictionary<string, int>();

foreach (var word in words) {
    if (freq.ContainsKey(word))
        freq[word]++;
    else
        freq[word] = 1;
}

foreach (var kv in freq)
    Console.WriteLine($"{kv.Key}: {kv.Value}");"""
})

post(938, "mini_task", {
    "language": "csharp",
    "title": "Словарь: подсчёт слов в тексте",
    "description": "Дан текст. Посчитай частоту каждого слова и выведи слова в формате 'слово: количество'. Слова разделены пробелами.",
    "starterCode": """string text = "яблоко банан яблоко груша банан яблоко";
var words = text.Split(' ');
var freq = new Dictionary<string, int>();

foreach (var word in words) {
    // если слово уже есть в словаре — увеличь счётчик
    // иначе — добавь слово со значением 1
}

foreach (var kv in freq)
    Console.WriteLine($"{kv.Key}: {kv.Value}");""",
    "expectedOutput": "яблоко: 3\nбанан: 2\nгруша: 1"
})

post(938, "quiz", {
    "question": "Что произойдёт при вызове `dict.Add(key, value)`, если ключ уже существует?",
    "options": [
        "Значение будет перезаписано",
        "Метод вернёт false",
        "Будет выброшено исключение ArgumentException",
        "Ничего не произойдёт"
    ],
    "correct": 2,
    "explanation": "`Add()` бросает `ArgumentException` при дублировании ключа. Для безопасного добавления/обновления используйте `dict[key] = value`."
})

post(938, "fix_bug", {
    "language": "csharp",
    "buggyCode": """var scores = new Dictionary<string, int>();
scores.Add("Анна", 90);
scores.Add("Борис", 85);
scores.Add("Анна", 95); // повторный ключ
Console.WriteLine(scores["Анна"]);""",
    "expectedOutput": "95",
    "hint": "Для обновления значения по существующему ключу используйте scores[\"Анна\"] = 95; вместо Add()."
})

post(938, "drag_order", {
    "pieces": [
        "var d = new Dictionary<string, int>();",
        "d[\"ключ\"] = 42;",
        "if (d.ContainsKey(\"ключ\"))",
        "    Console.WriteLine(d[\"ключ\"]);"
    ],
    "solution": [0, 1, 2, 3],
    "hint": "Сначала создаём словарь, затем добавляем элемент, потом проверяем наличие и выводим."
})


# ===========================================================
# Lesson 939: HashSet<T>, Queue<T>, Stack<T>
# ===========================================================
print("\n=== Lesson 939: HashSet, Queue, Stack ===")

post(939, "text", {"markdown": """## Специализированные коллекции

### HashSet<T> — множество уникальных элементов
```csharp
var set = new HashSet<int> {1, 2, 3};
set.Add(2);       // игнорируется, уже есть
set.Add(4);
Console.WriteLine(set.Count); // 4
// Операции над множествами:
set.UnionWith(other);
set.IntersectWith(other);
set.ExceptWith(other);
```

### Queue<T> — очередь (FIFO)
```csharp
var queue = new Queue<string>();
queue.Enqueue("первый");
queue.Enqueue("второй");
string item = queue.Dequeue(); // "первый"
string peek = queue.Peek();    // посмотреть без удаления
```

### Stack<T> — стек (LIFO)
```csharp
var stack = new Stack<int>();
stack.Push(10);
stack.Push(20);
int top = stack.Pop();   // 20
int peek = stack.Peek(); // 10
```"""})

post(939, "code", {
    "language": "csharp",
    "title": "Проверка уникальности и стек",
    "code": """// HashSet: найти уникальные слова
var words = new string[] {"да", "нет", "да", "может", "нет", "да"};
var unique = new HashSet<string>(words);
Console.WriteLine($"Уникальных слов: {unique.Count}");

// Stack: проверка скобок
string expr = "((a+b)*(c-d))";
var stack = new Stack<char>();
bool valid = true;
foreach (char c in expr) {
    if (c == '(') stack.Push(c);
    else if (c == ')') {
        if (stack.Count == 0) { valid = false; break; }
        stack.Pop();
    }
}
valid = valid && stack.Count == 0;
Console.WriteLine($"Скобки корректны: {valid}");"""
})

post(939, "quiz", {
    "question": "Какой порядок извлечения элементов у Queue<T>?",
    "options": ["LIFO (последний добавленный — первый извлечённый)", "FIFO (первый добавленный — первый извлечённый)", "Случайный", "По приоритету"],
    "correct": 1,
    "explanation": "Queue — очередь, работает по принципу FIFO: первый добавленный элемент первым и извлекается (Enqueue — добавить, Dequeue — извлечь)."
})

post(939, "prediction", {
    "language": "csharp",
    "code": """var stack = new Stack<int>();
stack.Push(1);
stack.Push(2);
stack.Push(3);
Console.WriteLine(stack.Pop());
Console.WriteLine(stack.Peek());
Console.WriteLine(stack.Count);""",
    "expectedOutput": "3\n2\n2",
    "hint": "Pop() удаляет и возвращает верхний элемент. Peek() только смотрит, не удаляя."
})

post(939, "mini_task", {
    "language": "csharp",
    "title": "Дедупликация списка через HashSet",
    "description": "Дан список чисел с повторениями. Используя HashSet, найди уникальные числа и выведи их количество.",
    "starterCode": """var numbers = new List<int> {4, 7, 2, 4, 9, 7, 1, 2, 4, 3};
// Создай HashSet из numbers
var unique = new HashSet<int>(numbers);
// Выведи количество уникальных чисел
Console.WriteLine(unique.Count);""",
    "expectedOutput": "6"
})

post(939, "knowledge_check", {
    "topic": "Специализированные коллекции",
    "question": "Какую коллекцию лучше использовать для задачи «обрабатывать задачи в том порядке, в котором они поступили»?",
    "options": ["Stack<T>", "HashSet<T>", "Queue<T>", "List<T>"],
    "correct": 2,
    "explanation": "Queue реализует принцип FIFO — задачи обрабатываются в порядке поступления. Stack — LIFO (обратный порядок), HashSet — без порядка."
})


# ===========================================================
# Lesson 940: IEnumerable и foreach
# ===========================================================
print("\n=== Lesson 940: IEnumerable и foreach ===")

post(940, "text", {"markdown": """## IEnumerable<T> и foreach

`IEnumerable<T>` — базовый интерфейс для всех перечисляемых коллекций. Реализует паттерн «итератор».

```csharp
public interface IEnumerable<T> {
    IEnumerator<T> GetEnumerator();
}
```

### Как работает foreach
```csharp
foreach (var item in collection) { ... }
// Компилятор разворачивает в:
var enumerator = collection.GetEnumerator();
while (enumerator.MoveNext()) {
    var item = enumerator.Current;
    // тело цикла
}
```

### yield return — ленивые последовательности
```csharp
IEnumerable<int> GetEven(int max) {
    for (int i = 0; i <= max; i += 2)
        yield return i;
}
foreach (var n in GetEven(10))
    Console.Write(n + " "); // 0 2 4 6 8 10
```
`yield return` создаёт элементы по запросу — без загрузки всей коллекции в память."""})

post(940, "code", {
    "language": "csharp",
    "title": "Пользовательский IEnumerable с yield",
    "code": """// Генератор чисел Фибоначчи
IEnumerable<int> Fibonacci(int count) {
    int a = 0, b = 1;
    for (int i = 0; i < count; i++) {
        yield return a;
        (a, b) = (b, a + b);
    }
}

Console.WriteLine("Числа Фибоначчи:");
foreach (int n in Fibonacci(8))
    Console.Write(n + " ");
Console.WriteLine();

// Работа с IEnumerable<T> — принимаем любую коллекцию
void PrintAll(IEnumerable<int> items) {
    foreach (var item in items)
        Console.Write(item + " ");
    Console.WriteLine();
}

int[] arr = {10, 20, 30};
var list = new List<int> {40, 50};
PrintAll(arr);
PrintAll(list);"""
})

post(940, "quiz", {
    "question": "Что делает `yield return` в методе?",
    "options": [
        "Завершает метод и возвращает список",
        "Возвращает один элемент и приостанавливает выполнение до следующего запроса",
        "Создаёт новый поток",
        "Работает как обычный return"
    ],
    "correct": 1,
    "explanation": "`yield return` возвращает значение вызывающему коду и «замораживает» состояние метода. При следующей итерации foreach выполнение продолжается со следующей строки."
})

post(940, "prediction", {
    "language": "csharp",
    "code": """IEnumerable<int> Gen() {
    yield return 10;
    yield return 20;
    yield return 30;
}
foreach (var n in Gen())
    Console.WriteLine(n);""",
    "expectedOutput": "10\n20\n30",
    "hint": "yield return возвращает элементы по одному при каждой итерации foreach."
})

post(940, "fill_blank", {
    "template": "IEnumerable<int> OddNumbers(int max) {\n    for (int i = 1; i <= max; i += ___)\n        ___ return i;\n}",
    "blanks": ["2", "yield"],
    "hint": "Шаг цикла для нечётных чисел равен 2. Для возврата элементов из итератора используется ключевое слово yield."
})

post(940, "mini_task", {
    "language": "csharp",
    "title": "Итератор квадратов",
    "description": "Реализуй метод Squares, возвращающий квадраты чисел от 1 до n с помощью yield return. Выведи первые 5 квадратов.",
    "starterCode": """IEnumerable<int> Squares(int n) {
    for (int i = 1; i <= n; i++) {
        // используй yield return для возврата i*i
    }
}

foreach (var sq in Squares(5))
    Console.WriteLine(sq);""",
    "expectedOutput": "1\n4\n9\n16\n25"
})


# ===========================================================
# Lesson 941: Generics: обобщённые типы
# ===========================================================
print("\n=== Lesson 941: Generics ===")

post(941, "text", {"markdown": """## Generics — обобщённые типы

Обобщения позволяют писать код, не зависящий от конкретного типа данных. Типобезопасность проверяется на этапе компиляции.

### Обобщённый метод
```csharp
T Max<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) > 0 ? a : b;

Console.WriteLine(Max(3, 7));       // 7
Console.WriteLine(Max("кот","пёс")); // пёс
```

### Обобщённый класс
```csharp
public class Pair<T1, T2> {
    public T1 First { get; set; }
    public T2 Second { get; set; }
    public Pair(T1 f, T2 s) { First = f; Second = s; }
}

var p = new Pair<string, int>("возраст", 25);
Console.WriteLine($"{p.First}: {p.Second}");
```

### Преимущества перед object
- Нет boxing/unboxing для value types
- Ошибки типов в compile time, не в runtime
- Читаемый, переиспользуемый код"""})

post(941, "code", {
    "language": "csharp",
    "title": "Обобщённый репозиторий",
    "code": """// Универсальное хранилище с возможностью добавить/получить
public class SimpleRepository<T> {
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);
    public T Get(int index) => _items[index];
    public int Count => _items.Count;
    public IEnumerable<T> All() => _items;
}

// Использование с разными типами
var intRepo = new SimpleRepository<int>();
intRepo.Add(10);
intRepo.Add(20);
Console.WriteLine($"Чисел: {intRepo.Count}, первое: {intRepo.Get(0)}");

var strRepo = new SimpleRepository<string>();
strRepo.Add("привет");
strRepo.Add("мир");
foreach (var s in strRepo.All())
    Console.WriteLine(s);"""
})

post(941, "mini_task", {
    "language": "csharp",
    "title": "Реализуй обобщённый Stack<T>",
    "description": "Реализуй класс GenericStack<T> с методами Push, Pop и свойством Count. Используй List<T> внутри. Проверь работу на стеке целых чисел.",
    "starterCode": """public class GenericStack<T> {
    private List<T> _data = new List<T>();

    public void Push(T item) {
        // добавь item в конец _data
    }

    public T Pop() {
        // возьми последний элемент, удали его из _data, верни
        T item = _data[_data.Count - 1];
        _data.RemoveAt(_data.Count - 1);
        return item;
    }

    public int Count => _data.Count;
}

var stack = new GenericStack<int>();
stack.Push(1);
stack.Push(2);
stack.Push(3);
Console.WriteLine(stack.Pop());
Console.WriteLine(stack.Count);""",
    "expectedOutput": "3\n2"
})

post(941, "quiz", {
    "question": "В чём главное преимущество `List<int>` перед `ArrayList` (нетипизированный список)?",
    "options": [
        "List<int> работает медленнее, но удобнее",
        "List<int> исключает boxing и даёт ошибки типов на этапе компиляции",
        "ArrayList поддерживает больше методов",
        "Нет разницы"
    ],
    "correct": 1,
    "explanation": "Generics убирают boxing/unboxing для value-типов (int, double и т.д.) и делают ошибки типов видимыми на этапе компиляции, а не в runtime."
})

post(941, "compare_variants", {
    "question": "Какой вариант правильно объявляет обобщённый метод для нахождения минимума?",
    "variants": [
        {"title": "Вариант A", "code": "object Min(object a, object b) => a;"},
        {"title": "Вариант B", "code": "T Min<T>(T a, T b) where T : IComparable<T>\n    => a.CompareTo(b) < 0 ? a : b;"},
        {"title": "Вариант C", "code": "int Min<T>(T a, T b) => 0;"}
    ],
    "correct": 1,
    "explanation": "Вариант B правильный: параметр типа T с ограничением IComparable<T> позволяет сравнивать элементы и сохраняет типобезопасность."
})


# ===========================================================
# Lesson 942: Generics: ограничения where
# ===========================================================
print("\n=== Lesson 942: Generics where ===")

post(942, "text", {"markdown": """## Ограничения обобщённых типов (where)

Ограничения сообщают компилятору, какими возможностями должен обладать тип T.

| Ограничение | Значение |
|------------|----------|
| `where T : class` | T — ссылочный тип |
| `where T : struct` | T — значимый тип |
| `where T : new()` | T имеет конструктор без параметров |
| `where T : IComparable<T>` | T реализует интерфейс |
| `where T : BaseClass` | T наследует класс |
| `where T : class, new()` | несколько ограничений |

### Пример
```csharp
T CreateAndPrint<T>() where T : new() {
    var obj = new T(); // можно создать, т.к. есть new()
    Console.WriteLine(obj);
    return obj;
}

T FindMin<T>(IList<T> list) where T : IComparable<T> {
    T min = list[0];
    foreach (var item in list)
        if (item.CompareTo(min) < 0) min = item;
    return min;
}
```"""})

post(942, "code", {
    "language": "csharp",
    "title": "Ограничения в действии",
    "code": """// Метод работает только с типами, реализующими IComparable<T>
T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
    if (value.CompareTo(min) < 0) return min;
    if (value.CompareTo(max) > 0) return max;
    return value;
}

Console.WriteLine(Clamp(15, 0, 10));   // 10
Console.WriteLine(Clamp(-5, 0, 10));   // 0
Console.WriteLine(Clamp(7, 0, 10));    // 7
Console.WriteLine(Clamp('z', 'a', 'm')); // m

// Ограничение new() — фабричный метод
T Create<T>() where T : new() => new T();
var list = Create<List<int>>();
list.Add(99);
Console.WriteLine(list[0]);"""
})

post(942, "quiz", {
    "question": "Что означает ограничение `where T : new()`?",
    "options": [
        "T должен быть числом",
        "T должен иметь публичный конструктор без параметров",
        "T — новый тип данных",
        "T не может быть null"
    ],
    "correct": 1,
    "explanation": "`where T : new()` позволяет создавать экземпляр T внутри обобщённого метода с помощью `new T()`. T должен иметь публичный конструктор без параметров."
})

post(942, "fill_blank", {
    "template": "T FindMax<T>(List<T> items) ___ T : ___ {\n    T max = items[0];\n    foreach (var x in items)\n        if (x.CompareTo(max) > 0) max = x;\n    return max;\n}",
    "blanks": ["where", "IComparable<T>"],
    "hint": "Ключевое слово для ограничений — where. Для сравнения нужен интерфейс IComparable<T>."
})

post(942, "mini_task", {
    "language": "csharp",
    "title": "Обобщённый поиск минимума",
    "description": "Реализуй метод FindMin<T>, который находит минимальный элемент в списке. Используй ограничение where T : IComparable<T>.",
    "starterCode": """T FindMin<T>(List<T> items) where T : IComparable<T> {
    T min = items[0];
    foreach (var item in items) {
        // если item меньше min — обнови min
    }
    return min;
}

var nums = new List<int> {8, 3, 15, 1, 7};
Console.WriteLine(FindMin(nums));

var words = new List<string> {"груша", "яблоко", "абрикос", "банан"};
Console.WriteLine(FindMin(words));""",
    "expectedOutput": "1\nабрикос"
})

post(942, "knowledge_check", {
    "topic": "Ограничения Generics",
    "question": "Если метод должен создавать экземпляры типа T внутри своего тела, какое ограничение обязательно?",
    "options": ["where T : class", "where T : new()", "where T : struct", "where T : IDisposable"],
    "correct": 1,
    "explanation": "`where T : new()` гарантирует наличие конструктора без параметров, что позволяет писать `new T()` внутри метода."
})


# ===========================================================
# Lesson 943: Сортировка: IComparable и IComparer
# ===========================================================
print("\n=== Lesson 943: IComparable и IComparer ===")

post(943, "text", {"markdown": """## Сортировка: IComparable и IComparer

### IComparable<T> — «естественный» порядок
Реализуется самим классом. Определяет порядок сортировки «по умолчанию».

```csharp
public class Product : IComparable<Product> {
    public string Name { get; set; }
    public decimal Price { get; set; }

    public int CompareTo(Product other) =>
        Price.CompareTo(other.Price); // по цене
}

var list = new List<Product> { ... };
list.Sort(); // использует CompareTo
```

### IComparer<T> — внешняя стратегия сортировки
Позволяет сортировать по разным критериям без изменения класса.

```csharp
public class ByName : IComparer<Product> {
    public int Compare(Product x, Product y) =>
        string.Compare(x.Name, y.Name, StringComparison.Ordinal);
}

list.Sort(new ByName()); // сортировка по имени
```

### Возвращаемые значения CompareTo / Compare
- **< 0** — x идёт раньше y
- **0** — равны
- **> 0** — x идёт после y"""})

post(943, "code", {
    "language": "csharp",
    "title": "Класс Student с IComparable и IComparer",
    "code": """public class Student : IComparable<Student> {
    public string Name { get; set; }
    public double GPA { get; set; }

    // Естественный порядок — по GPA убывание
    public int CompareTo(Student other) =>
        other.GPA.CompareTo(GPA);

    public override string ToString() =>
        $"{Name} ({GPA:F1})";
}

// Внешний компаратор — по имени
public class ByNameComparer : IComparer<Student> {
    public int Compare(Student x, Student y) =>
        string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
}

var students = new List<Student> {
    new Student { Name = "Вера",    GPA = 4.5 },
    new Student { Name = "Анна",    GPA = 3.8 },
    new Student { Name = "Борис",   GPA = 4.9 },
    new Student { Name = "Григорий",GPA = 4.2 },
};

students.Sort();
Console.WriteLine("По GPA (убывание):");
students.ForEach(s => Console.WriteLine($"  {s}"));

students.Sort(new ByNameComparer());
Console.WriteLine("По имени:");
students.ForEach(s => Console.WriteLine($"  {s}"));"""
})

post(943, "mini_task", {
    "language": "csharp",
    "title": "IComparer: сортировка товаров по цене",
    "description": "Реализуй класс PriceComparer (IComparer<Product>) для сортировки товаров по цене по возрастанию. Используй его для сортировки списка.",
    "starterCode": """public class Product {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public override string ToString() => $"{Name}: {Price}р";
}

public class PriceComparer : IComparer<Product> {
    public int Compare(Product x, Product y) {
        // верни сравнение x.Price с y.Price
        return x.Price.CompareTo(y.Price);
    }
}

var products = new List<Product> {
    new Product { Name = "Мышь",      Price = 1200 },
    new Product { Name = "Клавиатура",Price = 2500 },
    new Product { Name = "Монитор",   Price = 18000 },
    new Product { Name = "Наушники",  Price = 3200 },
};

products.Sort(new PriceComparer());
foreach (var p in products)
    Console.WriteLine(p);""",
    "expectedOutput": "Мышь: 1200р\nКлавиатура: 2500р\nНаушники: 3200р\nМонитор: 18000р"
})

post(943, "quiz", {
    "question": "Какое значение должен вернуть `CompareTo(other)`, если текущий объект должен идти ПОСЛЕ other при сортировке?",
    "options": ["Отрицательное число", "0", "Положительное число", "null"],
    "correct": 2,
    "explanation": "CompareTo возвращает: <0 — текущий идёт раньше, 0 — равны, >0 — текущий идёт после. Sort() использует это для построения порядка."
})

post(943, "compare_variants", {
    "question": "Нужно сортировать List<Person> по возрасту, не изменяя класс Person. Какой подход правильный?",
    "variants": [
        {"title": "Реализовать IComparable<Person> в классе Person", "code": "class Person : IComparable<Person> {\n    public int CompareTo(Person o) => Age.CompareTo(o.Age);\n}"},
        {"title": "Создать отдельный AgeComparer : IComparer<Person>", "code": "class AgeComparer : IComparer<Person> {\n    public int Compare(Person x, Person y) => x.Age.CompareTo(y.Age);\n}\nlist.Sort(new AgeComparer());"},
        {"title": "Использовать ручной цикл с перестановками", "code": "for (int i=0;i<list.Count-1;i++)\n    for(int j=0;j<list.Count-1-i;j++)\n        if(list[j].Age>list[j+1].Age) { var t=list[j]; list[j]=list[j+1]; list[j+1]=t; }"}
    ],
    "correct": 1,
    "explanation": "IComparer<T> — правильное решение когда нельзя (или не нужно) изменять класс. Он инкапсулирует стратегию сравнения отдельно от данных."
})

post(943, "checkpoint", {
    "title": "Итог: Сортировка",
    "description": "Убедись, что понимаешь оба интерфейса сортировки.",
    "tasks": [
        "Реализовать IComparable<T> в классе для сортировки по умолчанию",
        "Создать IComparer<T> для альтернативной стратегии сортировки",
        "Объяснить значения возвращаемых чисел: <0, 0, >0"
    ]
})


# ===========================================================
# Lesson 944: Введение в LINQ
# ===========================================================
print("\n=== Lesson 944: Введение в LINQ ===")

post(944, "text", {"markdown": """## Введение в LINQ

**LINQ** (Language Integrated Query) — встроенный язык запросов C#, позволяющий работать с коллекциями декларативно.

```csharp
// Без LINQ
var result = new List<int>();
foreach (var n in numbers)
    if (n > 5) result.Add(n);

// С LINQ — то же самое
var result = numbers.Where(n => n > 5).ToList();
```

### Подключение
```csharp
using System.Linq; // необходим для методов расширения
```

### Два синтаксиса
**Method syntax (fluent):**
```csharp
var q = list.Where(x => x > 0).Select(x => x * 2);
```

**Query syntax (SQL-подобный):**
```csharp
var q = from x in list
        where x > 0
        select x * 2;
```

### Ленивое выполнение
LINQ-запрос не выполняется до момента перебора результата. `ToList()`, `ToArray()`, `Count()` — форсируют выполнение."""})

post(944, "code", {
    "language": "csharp",
    "title": "Первый LINQ-запрос",
    "code": """using System.Linq;

var numbers = new List<int> {1, 7, 3, 9, 2, 5, 8, 4, 6};

// Чётные числа, отсортированные по убыванию
var result = numbers
    .Where(n => n % 2 == 0)
    .OrderByDescending(n => n)
    .ToList();

Console.WriteLine("Чётные по убыванию:");
Console.WriteLine(string.Join(", ", result));

// Проекция: квадраты нечётных чисел
var oddSquares = numbers
    .Where(n => n % 2 != 0)
    .Select(n => n * n);

Console.WriteLine("Квадраты нечётных:");
Console.WriteLine(string.Join(", ", oddSquares));"""
})

post(944, "prediction", {
    "language": "csharp",
    "code": """var nums = new[] {3, 1, 4, 1, 5, 9, 2, 6};
var q = nums.Where(x => x > 3).Count();
Console.WriteLine(q);""",
    "expectedOutput": "4",
    "hint": "Where фильтрует, Count считает оставшиеся. Числа больше 3: 4, 5, 9, 6."
})

post(944, "quiz", {
    "question": "Когда LINQ-запрос фактически выполняется (ленивое выполнение)?",
    "options": [
        "Сразу при объявлении запроса",
        "При итерации результата (foreach, ToList, Count и т.д.)",
        "При компиляции программы",
        "Никогда — только хранит описание"
    ],
    "correct": 1,
    "explanation": "LINQ использует отложенное (ленивое) выполнение: запрос создаёт описание, а реальные вычисления происходят при итерации — в foreach, ToList(), Count() и т.д."
})

post(944, "mini_task", {
    "language": "csharp",
    "title": "Фильтрация и подсчёт через LINQ",
    "description": "Используя LINQ, найди количество строк длиннее 5 символов.",
    "starterCode": """var words = new List<string> {"кот", "слон", "крокодил", "пёс", "жираф", "обезьяна", "лев"};
// Используй LINQ Where и Count
int count = words.Where(w => w.Length > 5).Count();
Console.WriteLine(count);""",
    "expectedOutput": "2"
})

post(944, "knowledge_check", {
    "topic": "Введение в LINQ",
    "question": "Какой namespace нужно подключить для использования LINQ-методов расширения?",
    "options": ["System.Collections", "System.Linq", "System.Generic", "System.Query"],
    "correct": 1,
    "explanation": "`using System.Linq;` подключает методы расширения LINQ (Where, Select, OrderBy и т.д.) для IEnumerable<T>."
})


# ===========================================================
# Lesson 945: Where, Select, OrderBy
# ===========================================================
print("\n=== Lesson 945: Where, Select, OrderBy ===")

post(945, "text", {"markdown": """## Where, Select, OrderBy

### Where — фильтрация
```csharp
var adults = people.Where(p => p.Age >= 18);
```

### Select — проекция (трансформация)
```csharp
var names = people.Select(p => p.Name);
var nameLengths = people.Select(p => new { p.Name, Len = p.Name.Length });
```

### OrderBy / OrderByDescending
```csharp
var sorted = list.OrderBy(x => x.Price);           // по возрастанию
var sorted = list.OrderByDescending(x => x.Price); // по убыванию
// Несколько полей:
var q = list.OrderBy(x => x.Category).ThenBy(x => x.Price);
```

### Цепочки
```csharp
var result = products
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Select(p => $"{p.Name}: {p.Price}р");
```"""})

post(945, "code", {
    "language": "csharp",
    "title": "Обработка списка сотрудников",
    "code": """var employees = new[] {
    new { Name = "Алексей", Dept = "IT",   Salary = 85000 },
    new { Name = "Мария",   Dept = "HR",   Salary = 60000 },
    new { Name = "Сергей",  Dept = "IT",   Salary = 95000 },
    new { Name = "Елена",   Dept = "HR",   Salary = 70000 },
    new { Name = "Павел",   Dept = "IT",   Salary = 75000 },
};

// Сотрудники IT с зарплатой > 80000, отсортированные по имени
var result = employees
    .Where(e => e.Dept == "IT" && e.Salary > 80000)
    .OrderBy(e => e.Name)
    .Select(e => $"{e.Name}: {e.Salary:N0}р");

foreach (var r in result)
    Console.WriteLine(r);"""
})

post(945, "mini_task", {
    "language": "csharp",
    "title": "Where + Select + OrderBy",
    "description": "Из списка продуктов выбери те, цена которых от 500 до 3000. Отсортируй по цене по возрастанию. Выведи только названия.",
    "starterCode": """var products = new[] {
    new { Name = "Карандаш",  Price = 50 },
    new { Name = "Тетрадь",   Price = 120 },
    new { Name = "Рюкзак",    Price = 2800 },
    new { Name = "Учебник",   Price = 750 },
    new { Name = "Ноутбук",   Price = 55000 },
    new { Name = "Калькулятор",Price = 1500 },
};

var result = products
    .Where(p => p.Price >= 500 && p.Price <= 3000)
    .OrderBy(p => p.Price)
    .Select(p => p.Name);

foreach (var name in result)
    Console.WriteLine(name);""",
    "expectedOutput": "Учебник\nКалькулятор\nРюкзак"
})

post(945, "fix_bug", {
    "language": "csharp",
    "buggyCode": """var nums = new[] {5, 2, 8, 1, 9, 3, 7};
var result = nums
    .Where(n => n > 4)
    .Select(n => n * 10)
    .OrderByAscending(n => n);
foreach (var n in result)
    Console.WriteLine(n);""",
    "expectedOutput": "50\n70\n80\n90",
    "hint": "Метод называется OrderBy(), а не OrderByAscending()."
})

post(945, "quiz", {
    "question": "Что возвращает `Select(x => x.Name)` для `List<Person>`?",
    "options": [
        "List<Person> с отфильтрованными элементами",
        "IEnumerable<string> — последовательность имён",
        "Первый элемент со свойством Name",
        "Dictionary<int, string>"
    ],
    "correct": 1,
    "explanation": "Select трансформирует каждый элемент коллекции. `Select(x => x.Name)` из `IEnumerable<Person>` создаёт `IEnumerable<string>` с именами."
})

post(945, "drag_order", {
    "pieces": [
        ".Where(p => p.Age >= 18)",
        "var result = people",
        ".OrderBy(p => p.Name)",
        ".Select(p => p.Name);"
    ],
    "solution": [1, 0, 2, 3],
    "hint": "Цепочка LINQ: источник → фильтрация → сортировка → проекция."
})


# ===========================================================
# Lesson 946: GroupBy и Join
# ===========================================================
print("\n=== Lesson 946: GroupBy и Join ===")

post(946, "text", {"markdown": """## GroupBy и Join

### GroupBy — группировка
```csharp
var grouped = products.GroupBy(p => p.Category);
foreach (var group in grouped) {
    Console.WriteLine($"Категория: {group.Key}");
    foreach (var p in group)
        Console.WriteLine($"  {p.Name}");
}
```

Агрегация в группах:
```csharp
var stats = products
    .GroupBy(p => p.Category)
    .Select(g => new {
        Category = g.Key,
        Count = g.Count(),
        TotalSum = g.Sum(p => p.Price)
    });
```

### Join — соединение коллекций
```csharp
var result = orders.Join(
    customers,
    o => o.CustomerId,      // ключ из orders
    c => c.Id,              // ключ из customers
    (o, c) => new { o.OrderId, c.Name, o.Amount }
);
```"""})

post(946, "code", {
    "language": "csharp",
    "title": "GroupBy с суммой по категориям",
    "code": """var products = new[] {
    new { Name = "Молоко",    Category = "Молочные",  Price = 85.0 },
    new { Name = "Кефир",     Category = "Молочные",  Price = 72.0 },
    new { Name = "Сыр",       Category = "Молочные",  Price = 320.0 },
    new { Name = "Хлеб",      Category = "Хлебобулочные", Price = 55.0 },
    new { Name = "Батон",     Category = "Хлебобулочные", Price = 48.0 },
    new { Name = "Яблоки",    Category = "Фрукты",    Price = 120.0 },
    new { Name = "Бананы",    Category = "Фрукты",    Price = 95.0 },
};

var stats = products
    .GroupBy(p => p.Category)
    .Select(g => new {
        Category = g.Key,
        Count = g.Count(),
        Total = g.Sum(p => p.Price)
    })
    .OrderByDescending(s => s.Total);

foreach (var s in stats)
    Console.WriteLine($"{s.Category}: {s.Count} товара, итого {s.Total}р");"""
})

post(946, "mini_task", {
    "language": "csharp",
    "title": "GroupBy: продажи по отделам",
    "description": "Сгруппируй продажи по отделу и посчитай суммарную выручку каждого отдела. Выведи в формате 'Отдел: сумма'.",
    "starterCode": """var sales = new[] {
    new { Dept = "Электроника", Amount = 15000.0 },
    new { Dept = "Одежда",      Amount = 3200.0 },
    new { Dept = "Электроника", Amount = 8500.0 },
    new { Dept = "Одежда",      Amount = 5100.0 },
    new { Dept = "Продукты",    Amount = 2300.0 },
    new { Dept = "Электроника", Amount = 22000.0 },
    new { Dept = "Продукты",    Amount = 1800.0 },
};

var result = sales
    .GroupBy(s => s.Dept)
    .Select(g => new { Dept = g.Key, Total = g.Sum(s => s.Amount) })
    .OrderByDescending(r => r.Total);

foreach (var r in result)
    Console.WriteLine($"{r.Dept}: {r.Total}");""",
    "expectedOutput": "Электроника: 45500\nОдежда: 8300\nПродукты: 4100"
})

post(946, "code", {
    "language": "csharp",
    "title": "Join: заказы и клиенты",
    "code": """var customers = new[] {
    new { Id = 1, Name = "Иван Петров" },
    new { Id = 2, Name = "Мария Сидорова" },
    new { Id = 3, Name = "Алексей Кузнецов" },
};

var orders = new[] {
    new { OrderId = 101, CustomerId = 2, Amount = 5400 },
    new { OrderId = 102, CustomerId = 1, Amount = 1200 },
    new { OrderId = 103, CustomerId = 3, Amount = 8900 },
    new { OrderId = 104, CustomerId = 1, Amount = 3300 },
};

var result = orders.Join(
    customers,
    o => o.CustomerId,
    c => c.Id,
    (o, c) => new { c.Name, o.OrderId, o.Amount }
).OrderBy(r => r.Name);

foreach (var r in result)
    Console.WriteLine($"Заказ #{r.OrderId} — {r.Name}: {r.Amount}р");"""
})

post(946, "quiz", {
    "question": "Что возвращает `group.Key` при использовании GroupBy?",
    "options": [
        "Количество элементов в группе",
        "Значение поля, по которому производилась группировка",
        "Первый элемент группы",
        "Индекс группы в результирующей коллекции"
    ],
    "correct": 1,
    "explanation": "`group.Key` — это значение поля, использованного в GroupBy. Например, при `GroupBy(p => p.Category)` Key содержит название категории."
})


# ===========================================================
# Lesson 947: Агрегация: Sum, Count, Min, Max, Average
# ===========================================================
print("\n=== Lesson 947: Агрегация ===")

post(947, "text", {"markdown": """## Агрегирующие методы LINQ

| Метод | Описание |
|-------|----------|
| `Count()` | Количество элементов |
| `Count(predicate)` | Количество, удовлетворяющих условию |
| `Sum(x => x.Val)` | Сумма |
| `Min(x => x.Val)` | Минимум |
| `Max(x => x.Val)` | Максимум |
| `Average(x => x.Val)` | Среднее |
| `Aggregate(seed, func)` | Произвольная агрегация |

```csharp
var numbers = new[] {4, 7, 2, 9, 1, 5};

Console.WriteLine(numbers.Sum());           // 28
Console.WriteLine(numbers.Min());           // 1
Console.WriteLine(numbers.Max());           // 9
Console.WriteLine(numbers.Average());       // 4.666...
Console.WriteLine(numbers.Count(n=>n>4));   // 3
```

### Aggregate
```csharp
// Факториал через Aggregate
int fact = Enumerable.Range(1, 5).Aggregate(1, (acc, x) => acc * x);
Console.WriteLine(fact); // 120
```"""})

post(947, "code", {
    "language": "csharp",
    "title": "Статистика по оценкам",
    "code": """var grades = new[] {
    new { Student = "Анна",    Score = 88 },
    new { Student = "Борис",   Score = 72 },
    new { Student = "Вера",    Score = 95 },
    new { Student = "Григорий",Score = 61 },
    new { Student = "Дарья",   Score = 84 },
};

Console.WriteLine($"Количество: {grades.Count()}");
Console.WriteLine($"Средний балл: {grades.Average(g => g.Score):F1}");
Console.WriteLine($"Максимум: {grades.Max(g => g.Score)}");
Console.WriteLine($"Минимум: {grades.Min(g => g.Score)}");
Console.WriteLine($"Сдали (>= 70): {grades.Count(g => g.Score >= 70)}");

var best = grades.OrderByDescending(g => g.Score).First();
Console.WriteLine($"Лучший студент: {best.Student} ({best.Score})");"""
})

post(947, "prediction", {
    "language": "csharp",
    "code": """var nums = new[] {10, 20, 30, 40, 50};
Console.WriteLine(nums.Sum());
Console.WriteLine(nums.Average());
Console.WriteLine(nums.Count(n => n > 25));""",
    "expectedOutput": "150\n30\n3",
    "hint": "Sum = 150, Average = 150/5 = 30. Числа больше 25: 30, 40, 50 — три штуки."
})

post(947, "mini_task", {
    "language": "csharp",
    "title": "Анализ продаж через агрегацию",
    "description": "Вычисли общую выручку, среднюю сумму заказа, максимальный и минимальный заказ. Выведи каждое значение на отдельной строке.",
    "starterCode": """var orders = new[] {1500.0, 3200.0, 943.0, 4700.0, 2100.0, 670.0, 5300.0};

double total   = orders.Sum();
double average = orders.Average();
double max     = orders.Max();
double min     = orders.Min();

Console.WriteLine(total);
Console.WriteLine(average);
Console.WriteLine(max);
Console.WriteLine(min);""",
    "expectedOutput": "18320\n2617.142857142857\n5300\n670"
})

post(947, "fill_blank", {
    "template": "// Произведение всех чисел через Aggregate\nvar nums = new[]{2, 3, 4, 5};\nint product = nums.___(1, (acc, x) => acc ___ x);\nConsole.WriteLine(product); // 120",
    "blanks": ["Aggregate", "*"],
    "hint": "Метод для произвольной агрегации называется Aggregate. Произведение — умножение."
})

post(947, "quiz", {
    "question": "Что вернёт `list.Count(x => x > 10)` для `var list = new[]{5, 12, 3, 18, 7}`?",
    "options": ["5", "2", "30", "0"],
    "correct": 1,
    "explanation": "Count с предикатом считает элементы, удовлетворяющие условию. Числа > 10: 12 и 18 — итого 2."
})


# ===========================================================
# Lesson 948: First, Single, Any, All, Take, Skip
# ===========================================================
print("\n=== Lesson 948: First, Single, Any, All, Take, Skip ===")

post(948, "text", {"markdown": """## First, Single, Any, All, Take, Skip

### Методы поиска элементов
| Метод | Описание | Исключение если не найдено |
|-------|----------|--------------------------|
| `First()` | Первый элемент | InvalidOperationException |
| `FirstOrDefault()` | Первый или default | — |
| `Single()` | Единственный элемент | Если 0 или >1 |
| `SingleOrDefault()` | Единственный или default | Если >1 |
| `Last()` | Последний элемент | Если пусто |

### Проверки
```csharp
list.Any(x => x > 5);   // есть хоть один > 5?
list.All(x => x > 0);   // все > 0?
list.Any();              // коллекция не пуста?
```

### Пагинация
```csharp
list.Take(5);            // первые 5 элементов
list.Skip(10);           // пропустить 10, взять остальные
list.Skip(10).Take(5);   // страница: 10 пропустить, 5 взять
```"""})

post(948, "code", {
    "language": "csharp",
    "title": "Поиск и пагинация",
    "code": """var products = new[] {
    new { Name = "Ручка",     Price = 30 },
    new { Name = "Тетрадь",   Price = 85 },
    new { Name = "Рюкзак",    Price = 2500 },
    new { Name = "Линейка",   Price = 25 },
    new { Name = "Пенал",     Price = 450 },
    new { Name = "Карандаш",  Price = 20 },
    new { Name = "Папка",     Price = 180 },
};

// Первый дешевле 100 рублей
var cheap = products.FirstOrDefault(p => p.Price < 100);
Console.WriteLine($"Дешёвый: {cheap?.Name}");

// Есть ли товар дороже 1000?
Console.WriteLine($"Есть дорогой: {products.Any(p => p.Price > 1000)}");

// Все ли товары дороже 10?
Console.WriteLine($"Все > 10р: {products.All(p => p.Price > 10)}");

// Страница 2 (по 3 элемента)
var page2 = products.OrderBy(p => p.Name).Skip(3).Take(3);
Console.WriteLine("Страница 2:");
foreach (var p in page2)
    Console.WriteLine($"  {p.Name}");"""
})

post(948, "prediction", {
    "language": "csharp",
    "code": """var nums = new[] {3, 7, 1, 9, 5};
Console.WriteLine(nums.First(n => n > 5));
Console.WriteLine(nums.Any(n => n > 10));
Console.WriteLine(nums.All(n => n > 0));""",
    "expectedOutput": "7\nFalse\nTrue",
    "hint": "First(n>5) ищет первый элемент >5 в порядке хранения: 7 подходит. Any(n>10) — нет таких чисел. All(n>0) — все числа положительные."
})

post(948, "mini_task", {
    "language": "csharp",
    "title": "Take, Skip и поиск",
    "description": "Используя LINQ, реши три задачи подряд: возьми первые 3 числа, пропусти первые 4 и возьми 2, найди первое чётное.",
    "starterCode": """var nums = new[] {11, 4, 7, 2, 9, 6, 13, 8, 5, 3};

// Первые 3 числа
var first3 = nums.Take(3);
Console.WriteLine(string.Join(" ", first3));

// Пропустить 4, взять 2
var page = nums.Skip(4).Take(2);
Console.WriteLine(string.Join(" ", page));

// Первое чётное число
int firstEven = nums.First(n => n % 2 == 0);
Console.WriteLine(firstEven);""",
    "expectedOutput": "11 4 7\n9 6\n4"
})

post(948, "quiz", {
    "question": "Чем отличается `First()` от `FirstOrDefault()`?",
    "options": [
        "First() быстрее",
        "FirstOrDefault() возвращает null/default вместо исключения если элемент не найден",
        "FirstOrDefault() возвращает последний элемент",
        "Нет разницы"
    ],
    "correct": 1,
    "explanation": "`First()` бросает InvalidOperationException если коллекция пуста или нет подходящего. `FirstOrDefault()` возвращает null (для ссылочных типов) или 0/false/etc (для значимых)."
})

post(948, "constraint", {
    "language": "csharp",
    "title": "Take без использования Take",
    "description": "Реализуй выборку первых N элементов из массива, не используя метод Take().",
    "constraint": "запрещено использовать Take() и Skip()",
    "starterCode": """int[] nums = {10, 20, 30, 40, 50, 60};
int n = 3;
var result = new List<int>();
// Добавь в result первые n элементов без Take()

Console.WriteLine(string.Join(" ", result));""",
    "expectedOutput": "10 20 30",
    "hint": "Используй цикл for с условием i < n, или Where с индексом через Select с индексом."
})


# ===========================================================
# Lesson 949: Цепочки запросов
# ===========================================================
print("\n=== Lesson 949: Цепочки запросов ===")

post(949, "text", {"markdown": """## Цепочки LINQ-запросов

Реальные задачи требуют нескольких шагов обработки. LINQ позволяет выстраивать цепочки методов, каждый из которых принимает `IEnumerable<T>` и возвращает `IEnumerable<T>`.

```
Source → Where → GroupBy → Select → OrderBy → Take → ToList
```

### Пример: топ-3 категории по средней цене
```csharp
var top3 = products
    .Where(p => p.InStock)
    .GroupBy(p => p.Category)
    .Select(g => new {
        Category = g.Key,
        AvgPrice = g.Average(p => p.Price),
        Count = g.Count()
    })
    .Where(s => s.Count >= 2)
    .OrderByDescending(s => s.AvgPrice)
    .Take(3);
```

### Промежуточная материализация
Иногда нужно разбить цепочку, чтобы переиспользовать промежуточный результат:
```csharp
var filtered = source.Where(...).ToList(); // материализовать
var stat1 = filtered.Sum(...);
var stat2 = filtered.Max(...);
```"""})

post(949, "code", {
    "language": "csharp",
    "title": "Многошаговая аналитика продаж",
    "code": """var sales = new[] {
    new { Region = "Север", Product = "Молоко",   Month = 1, Amount = 1200.0 },
    new { Region = "Север", Product = "Хлеб",     Month = 1, Amount = 943.0  },
    new { Region = "Юг",    Product = "Молоко",   Month = 1, Amount = 2100.0 },
    new { Region = "Юг",    Product = "Сыр",      Month = 1, Amount = 4500.0 },
    new { Region = "Север", Product = "Сыр",      Month = 2, Amount = 3200.0 },
    new { Region = "Юг",    Product = "Хлеб",     Month = 2, Amount = 1100.0 },
    new { Region = "Восток",Product = "Молоко",   Month = 2, Amount = 1800.0 },
    new { Region = "Восток",Product = "Сыр",      Month = 2, Amount = 5100.0 },
};

// Топ-2 региона по суммарной выручке (только Month=2)
var top = sales
    .Where(s => s.Month == 2)
    .GroupBy(s => s.Region)
    .Select(g => new { Region = g.Key, Total = g.Sum(s => s.Amount) })
    .OrderByDescending(r => r.Total)
    .Take(2);

Console.WriteLine("Топ-2 региона за месяц 2:");
foreach (var r in top)
    Console.WriteLine($"  {r.Region}: {r.Total}р");

// Продукты, продававшиеся в обоих регионах (Север и Юг)
var northProducts = sales.Where(s=>s.Region=="Север").Select(s=>s.Product).Distinct();
var southProducts = sales.Where(s=>s.Region=="Юг").Select(s=>s.Product).Distinct();
var both = northProducts.Intersect(southProducts).OrderBy(p => p);
Console.WriteLine("Продукты и на Севере и на Юге:");
foreach (var p in both) Console.WriteLine($"  {p}");"""
})

post(949, "mini_task", {
    "language": "csharp",
    "title": "Сложная цепочка: студенты",
    "description": "Найди среднюю оценку каждого студента за все предметы. Выведи только студентов со средним >= 75, отсортированных по среднему по убыванию.",
    "starterCode": """var grades = new[] {
    new { Name = "Анна",    Subject = "Математика", Score = 88 },
    new { Name = "Анна",    Subject = "Физика",     Score = 72 },
    new { Name = "Борис",   Subject = "Математика", Score = 65 },
    new { Name = "Борис",   Subject = "Физика",     Score = 70 },
    new { Name = "Вера",    Subject = "Математика", Score = 95 },
    new { Name = "Вера",    Subject = "Физика",     Score = 91 },
    new { Name = "Григорий",Subject = "Математика", Score = 78 },
    new { Name = "Григорий",Subject = "Физика",     Score = 82 },
};

var result = grades
    .GroupBy(g => g.Name)
    .Select(g => new { Name = g.Key, Avg = g.Average(x => x.Score) })
    .Where(s => s.Avg >= 75)
    .OrderByDescending(s => s.Avg);

foreach (var s in result)
    Console.WriteLine($"{s.Name}: {s.Avg:F1}");""",
    "expectedOutput": "Вера: 93.0\nГригорий: 80.0\nАнна: 80.0"
})

post(949, "transfer_test", {
    "question": "У тебя есть коллекция логов (поля: DateTime, Level, Message). Нужно получить топ-5 самых частых сообщений уровня ERROR за последний час. Какая цепочка LINQ правильная?",
    "options": [
        "logs.Where(l=>l.Level==\"ERROR\" && l.DateTime>=DateTime.Now.AddHours(-1)).GroupBy(l=>l.Message).OrderByDescending(g=>g.Count()).Take(5).Select(g=>g.Key)",
        "logs.GroupBy(l=>l.Message).Where(l=>l.Level==\"ERROR\").Take(5)",
        "logs.Select(l=>l.Message).Where(l=>l==\"ERROR\").Take(5)"
    ],
    "correct": 0,
    "explanation": "Правильная последовательность: фильтр по уровню и времени → группировка по сообщению → сортировка по частоте → ограничение Top-5 → проекция на ключ."
})

post(949, "checkpoint", {
    "title": "Итог: Цепочки LINQ",
    "description": "Проверь умение составлять многошаговые запросы.",
    "tasks": [
        "Написать цепочку с Where → GroupBy → Select → OrderBy → Take",
        "Использовать промежуточную материализацию ToList() для переиспользования",
        "Применить Intersect или Except для работы с множествами"
    ]
})


# ===========================================================
# Lesson 950: Query syntax vs Method syntax
# ===========================================================
print("\n=== Lesson 950: Query syntax vs Method syntax ===")

post(950, "text", {"markdown": """## Query syntax vs Method syntax

В C# LINQ запросы можно писать двумя способами. Оба компилируются в одинаковый IL-код.

### Method syntax (fluent)
```csharp
var result = students
    .Where(s => s.Score > 70)
    .OrderBy(s => s.Name)
    .Select(s => s.Name);
```

### Query syntax (декларативный, SQL-подобный)
```csharp
var result = from s in students
             where s.Score > 70
             orderby s.Name
             select s.Name;
```

### Что выбрать?
| Критерий | Method | Query |
|----------|--------|-------|
| GroupBy + Select | Часто нагляднее | Нужен `into` |
| Join | Многословный | Читается как SQL |
| Простые цепочки | Компактнее | Схоже |
| Aggregate, Take, Skip | Есть | **Нет** (только method) |

**Рекомендация:** Method syntax используется чаще. Query — там, где помогает читаемость (сложные join)."""})

post(950, "two_path", {
    "question": "Один и тот же запрос: получить имена студентов с оценкой > 80, отсортированные по алфавиту. Сравни синтаксисы:",
    "pathA": {
        "title": "Method syntax",
        "code": """var students = new[] {
    new { Name = "Анна",    Score = 85 },
    new { Name = "Борис",   Score = 72 },
    new { Name = "Вера",    Score = 91 },
    new { Name = "Григорий",Score = 88 },
};

var result = students
    .Where(s => s.Score > 80)
    .OrderBy(s => s.Name)
    .Select(s => s.Name);

foreach (var name in result)
    Console.WriteLine(name);"""
    },
    "pathB": {
        "title": "Query syntax",
        "code": """var students = new[] {
    new { Name = "Анна",    Score = 85 },
    new { Name = "Борис",   Score = 72 },
    new { Name = "Вера",    Score = 91 },
    new { Name = "Григорий",Score = 88 },
};

var result = from s in students
             where s.Score > 80
             orderby s.Name
             select s.Name;

foreach (var name in result)
    Console.WriteLine(name);"""
    },
    "explanation": "Оба варианта дают одинаковый результат: Анна, Вера, Григорий. Компилятор преобразует query syntax в вызовы методов Where, OrderBy, Select."
})

post(950, "code", {
    "language": "csharp",
    "title": "GroupBy: method vs query syntax",
    "code": """var orders = new[] {
    new { Customer = "Иван",   Amount = 1500 },
    new { Customer = "Мария",  Amount = 2300 },
    new { Customer = "Иван",   Amount = 893  },
    new { Customer = "Мария",  Amount = 1100 },
    new { Customer = "Алексей",Amount = 3400 },
};

// Method syntax
var ms = orders
    .GroupBy(o => o.Customer)
    .Select(g => new { Customer = g.Key, Total = g.Sum(o => o.Amount) })
    .OrderByDescending(r => r.Total);

// Query syntax
var qs = from o in orders
         group o by o.Customer into g
         let total = g.Sum(x => x.Amount)
         orderby total descending
         select new { Customer = g.Key, Total = total };

Console.WriteLine("Method syntax:");
foreach (var r in ms) Console.WriteLine($"  {r.Customer}: {r.Total}");

Console.WriteLine("Query syntax:");
foreach (var r in qs) Console.WriteLine($"  {r.Customer}: {r.Total}");"""
})

post(950, "quiz", {
    "question": "Какие методы LINQ НЕДОСТУПНЫ в query syntax и требуют method syntax?",
    "options": [
        "Where и Select",
        "OrderBy и GroupBy",
        "Take, Skip, Aggregate",
        "Join и Let"
    ],
    "correct": 2,
    "explanation": "Take, Skip, Aggregate, Distinct, Count, First и другие агрегирующие и пагинационные методы не имеют аналогов в query syntax и используются только как методы расширения."
})

post(950, "fill_blank", {
    "template": "// Query syntax\nvar result = ___ n in numbers\n             ___ n > 5\n             ___ n * 2;",
    "blanks": ["from", "where", "select"],
    "hint": "Query syntax начинается с from, затем where для фильтрации, select для проекции."
})

post(950, "mini_task", {
    "language": "csharp",
    "title": "Перепиши query syntax → method syntax",
    "description": "Дан запрос в query syntax. Перепиши его в method syntax и убедись, что результат одинаков.",
    "starterCode": """var products = new[] {
    new { Name = "Ноутбук",   Price = 55000, Category = "Электроника" },
    new { Name = "Телефон",   Price = 35000, Category = "Электроника" },
    new { Name = "Диван",     Price = 28000, Category = "Мебель"      },
    new { Name = "Стол",      Price = 12000, Category = "Мебель"      },
    new { Name = "Наушники",  Price = 8000,  Category = "Электроника" },
};

// Query syntax (дан):
var qs = from p in products
         where p.Category == "Электроника"
         orderby p.Price
         select p.Name;

// TODO: перепиши в method syntax:
var ms = products
    .Where(p => p.Category == "Электроника")
    .OrderBy(p => p.Price)
    .Select(p => p.Name);

Console.WriteLine(string.Join(", ", ms));""",
    "expectedOutput": "Наушники, Телефон, Ноутбук"
})

post(950, "transfer_test", {
    "question": "Коллега показал код с query syntax. Какой из вариантов — его эквивалент в method syntax?\n\n```\nvar r = from x in data where x.Val > 0 select x.Val * 2;\n```",
    "options": [
        "data.Where(x => x.Val > 0).Select(x => x.Val * 2)",
        "data.Select(x => x.Val * 2).Where(x => x > 0)",
        "data.Filter(x => x.Val > 0).Map(x => x.Val * 2)"
    ],
    "correct": 0,
    "explanation": "where → Where(), select → Select(). Порядок операторов сохраняется: сначала фильтрация, затем проекция."
})

print("\nДоготово!")
