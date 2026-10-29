/* ══════════════════════════════════════════════════════════════════
   starPerlin.js — плавающие токены на фоне
   Тема выбирается по data-theme="<slug>" на контейнере #stars.
   Пример: <div id="stars" data-theme="python">
   При отсутствии атрибута используется тема по умолчанию (csharp).
══════════════════════════════════════════════════════════════════ */

/* ── Таблица тем ──────────────────────────────────────────────────────────── */

const STAR_THEMES = {

    /* ── C# / .NET ── */
    csharp: {
        codeChance:  0.25,
        digitChance: 0.35,
        digits: ["0", "1"],
        tokens: [
            "Console.WriteLine()", "Console.Write()", "Console.ReadLine()", "Console.ReadKey()",
            "int.Parse()", "double.Parse()", "bool.Parse()",
            "Convert.ToInt32()", "Convert.ToDouble()", "Convert.ToString()",
            "Math.Sqrt()", "Math.Pow()", "Math.Abs()", "Math.Max()", "Math.Min()",
            "Math.Round()", "Math.Floor()", "Math.Ceiling()", "Math.Clamp()",
            "string.Format()", "string.Concat()", "string.Join()",
            "string.IsNullOrEmpty()", "string.IsNullOrWhiteSpace()",
            "ToString()", "Substring()", "Replace()", "Contains()", "StartsWith()", "EndsWith()",
            "Trim()", "TrimStart()", "TrimEnd()", "PadLeft()", "PadRight()",
            "ToUpper()", "ToLower()", "Split()", "IndexOf()", "LastIndexOf()",
            "Equals()", "CompareTo()", "GetHashCode()", "GetType()",
            "List.Add()", "List.AddRange()", "List.Remove()", "List.RemoveAt()",
            "List.RemoveAll()", "List.Clear()", "List.Contains()", "List.Insert()",
            "List.IndexOf()", "List.Find()", "List.FindAll()", "List.Sort()", "List.Reverse()",
            "Array.Sort()", "Array.Reverse()", "Array.Copy()", "Array.Clear()",
            "Array.IndexOf()", "Array.Resize()",
            "DateTime.Now", "DateTime.UtcNow", "DateTime.Parse()", "DateTime.TryParse()",
            "DateTime.AddDays()", "DateTime.AddHours()", "DateTime.AddMinutes()",
            "Random.Next()", "Random.NextDouble()", "Random.NextBytes()",
            "Guid.NewGuid()", "Environment.Exit()",
            "Path.Combine()", "Path.GetFileName()", "Path.GetExtension()",
            "File.ReadAllText()", "File.WriteAllText()", "File.Exists()",
            "File.Delete()", "File.Copy()", "File.Move()",
            "Directory.CreateDirectory()", "Directory.Delete()", "Directory.GetFiles()",
        ]
    },

    /* ── Python ── */
    python: {
        codeChance:  0.28,
        digitChance: 0.22,
        digits: ["0", "1", "True", "False", "None"],
        tokens: [
            "print()", "input()", "len()", "range()", "type()", "isinstance()",
            "list()", "dict()", "set()", "tuple()", "int()", "str()", "float()", "bool()",
            "map()", "filter()", "zip()", "enumerate()", "sorted()", "reversed()",
            "sum()", "min()", "max()", "abs()", "round()", "pow()",
            "open()", "read()", "write()", "close()",
            "hasattr()", "getattr()", "setattr()", "delattr()",
            "os.path.join()", "os.path.exists()", "os.listdir()", "os.getcwd()", "os.makedirs()",
            "json.loads()", "json.dumps()", "json.load()", "json.dump()",
            "re.match()", "re.findall()", "re.sub()", "re.split()",
            "datetime.now()", "timedelta()", "datetime.strptime()",
            "requests.get()", "requests.post()", "requests.put()", "requests.delete()",
            "pd.DataFrame()", "pd.read_csv()", "pd.read_json()", "pd.merge()",
            "np.array()", "np.zeros()", "np.ones()", "np.arange()", "np.reshape()",
            "np.mean()", "np.std()", "np.sum()", "np.max()", "np.min()",
            "lambda", "yield", "async def", "await",
            ".append()", ".extend()", ".pop()", ".remove()", ".insert()",
            ".split()", ".strip()", ".upper()", ".lower()", ".replace()",
            ".format()", ".join()", ".startswith()", ".endswith()", ".find()",
            ".keys()", ".values()", ".items()", ".get()", ".update()",
        ]
    },

    /* ── JavaScript / Frontend ── */
    javascript: {
        codeChance:  0.28,
        digitChance: 0.22,
        digits: ["0", "1", "null", "NaN", "undefined", "true", "false"],
        tokens: [
            "console.log()", "console.error()", "console.warn()", "console.table()",
            "document.querySelector()", "document.querySelectorAll()",
            "document.getElementById()", "document.createElement()",
            "addEventListener()", "removeEventListener()", "dispatchEvent()",
            "fetch()", "Promise.resolve()", "Promise.reject()", "Promise.all()",
            "async function", "await", "setTimeout()", "setInterval()", "clearTimeout()",
            "Array.map()", "Array.filter()", "Array.reduce()", "Array.find()",
            "Array.forEach()", "Array.some()", "Array.every()", "Array.flat()",
            "Array.from()", "Array.includes()", "Array.indexOf()", "Array.slice()",
            "Array.splice()", "Array.push()", "Array.pop()", "Array.shift()",
            "JSON.stringify()", "JSON.parse()",
            "Object.keys()", "Object.values()", "Object.entries()", "Object.assign()",
            "Object.create()", "Object.freeze()", "Object.defineProperty()",
            "String.split()", "String.trim()", "String.includes()", "String.slice()",
            "String.replace()", "String.startsWith()", "String.endsWith()",
            "Math.random()", "Math.floor()", "Math.ceil()", "Math.round()", "Math.abs()",
            "localStorage.setItem()", "localStorage.getItem()", "localStorage.removeItem()",
            "sessionStorage.setItem()", "sessionStorage.getItem()",
            "window.location.href", "history.pushState()", "history.back()",
            "classList.add()", "classList.remove()", "classList.toggle()", "classList.contains()",
            "setAttribute()", "getAttribute()", "removeAttribute()", "innerHTML", "textContent",
            "new Promise()", "new Map()", "new Set()", "new Date()",
            "import", "export default", "const", "let", "=>",
        ]
    },

    /* ── TypeScript ── */
    typescript: {
        codeChance:  0.28,
        digitChance: 0.22,
        digits: ["0", "1", "null", "undefined", "true", "false"],
        tokens: [
            "interface", "type", "enum", "namespace", "declare",
            "readonly", "abstract", "implements", "extends", "as",
            "never", "unknown", "any", "void", "keyof", "typeof",
            "Partial<T>", "Required<T>", "Pick<T, K>", "Omit<T, K>",
            "Record<K, V>", "Readonly<T>", "Array<T>", "Promise<T>",
            "console.log()", "fetch()", "async function", "await",
            "Array.map()", "Array.filter()", "Array.reduce()", "Array.find()",
            "JSON.stringify()", "JSON.parse()",
            "Object.keys()", "Object.values()", "Object.entries()",
            "Math.random()", "Math.floor()", "Math.ceil()",
            "new Map<K, V>()", "new Set<T>()",
            "import type", "export type", "export default",
            "useEffect()", "useState()", "useRef()", "useMemo()",
            "useCallback()", "useContext()", "useReducer()",
        ]
    },

    /* ── SQL / Базы данных ── */
    sql: {
        codeChance:  0.35,
        digitChance: 0.15,
        digits: ["NULL", "0", "1", "TRUE", "FALSE"],
        tokens: [
            "SELECT", "SELECT *", "SELECT DISTINCT", "FROM", "WHERE",
            "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL OUTER JOIN",
            "ON", "GROUP BY", "ORDER BY", "HAVING", "LIMIT", "OFFSET",
            "COUNT(*)", "COUNT(id)", "SUM()", "AVG()", "MAX()", "MIN()",
            "INSERT INTO", "VALUES()", "UPDATE", "SET", "DELETE FROM",
            "CREATE TABLE", "CREATE INDEX", "ALTER TABLE", "DROP TABLE",
            "ADD COLUMN", "DROP COLUMN", "RENAME TO",
            "PRIMARY KEY", "FOREIGN KEY", "REFERENCES", "UNIQUE", "NOT NULL",
            "DISTINCT", "UNION", "UNION ALL", "INTERSECT", "EXCEPT",
            "COALESCE()", "ISNULL()", "NULLIF()", "CASE WHEN ... THEN ... END",
            "CAST()", "CONVERT()", "LIKE '%...%'", "BETWEEN", "IN (...)", "EXISTS",
            "NOW()", "CURRENT_DATE", "GETDATE()", "DATEADD()", "DATEDIFF()",
            "SUBSTRING()", "UPPER()", "LOWER()", "TRIM()", "REPLACE()",
            "ROW_NUMBER()", "RANK()", "DENSE_RANK()", "PARTITION BY",
            "WITH cte AS (...)", "RETURNING",
        ]
    },

    /* ── Rust ── */
    rust: {
        codeChance:  0.28,
        digitChance: 0.22,
        digits: ["0", "1", "true", "false", "0u8", "0i32"],
        tokens: [
            "fn main()", "fn()", "impl", "trait", "struct", "enum", "pub",
            "let mut", "let", "const", "static", "type", "where",
            "match", "if let", "while let", "loop", "break", "continue", "return",
            "Some()", "None", "Ok()", "Err()", "Result<T, E>", "Option<T>",
            "Vec::new()", "Vec::with_capacity()", "HashMap::new()", "HashSet::new()",
            "String::new()", "String::from()", "to_string()", "to_owned()",
            ".push()", ".pop()", ".len()", ".is_empty()", ".contains()", ".iter()",
            ".map()", ".filter()", ".collect()", ".fold()", ".enumerate()",
            ".unwrap()", ".expect()", ".unwrap_or()", ".unwrap_or_else()",
            "println!()", "print!()", "eprintln!()", "format!()", "panic!()", "todo!()",
            "Box::new()", "Rc::new()", "Arc::new()", "RefCell::new()", "Mutex::new()",
            "&str", "&mut", "lifetime 'a", "dyn Trait", "impl Trait",
            "use std::io", "use std::fs", "use std::collections",
            "async fn", "await", "tokio::main", "tokio::spawn()",
            "|x| x + 1", "closure", ">>=", "?", "::",
        ]
    },

    /* ── Java ── */
    java: {
        codeChance:  0.25,
        digitChance: 0.30,
        digits: ["0", "1", "null", "true", "false"],
        tokens: [
            "System.out.println()", "System.out.print()", "System.err.println()",
            "Scanner.nextLine()", "Scanner.nextInt()", "Scanner.nextDouble()",
            "Integer.parseInt()", "Double.parseDouble()", "Boolean.parseBoolean()",
            "String.format()", "String.valueOf()", "String.join()", "String.split()",
            ".length()", ".charAt()", ".substring()", ".contains()", ".replace()",
            ".trim()", ".toUpperCase()", ".toLowerCase()", ".startsWith()", ".endsWith()",
            "Math.sqrt()", "Math.pow()", "Math.abs()", "Math.max()", "Math.min()", "Math.random()",
            "ArrayList.add()", "ArrayList.remove()", "ArrayList.size()", "ArrayList.get()",
            "HashMap.put()", "HashMap.get()", "HashMap.containsKey()", "HashMap.remove()",
            "Arrays.sort()", "Arrays.fill()", "Arrays.copyOf()", "Arrays.toString()",
            "Collections.sort()", "Collections.reverse()", "Collections.shuffle()",
            "new ArrayList<>()", "new HashMap<>()", "new HashSet<>()",
            "Optional.of()", "Optional.empty()", "Optional.ofNullable()",
            "Stream.of()", ".stream()", ".filter()", ".map()", ".collect()", ".forEach()",
            "@Override", "@Autowired", "@Service", "@Repository", "@Controller",
            "public static void main()", "throws Exception", "try { } catch (Exception e)",
            "instanceof", "extends", "implements", "interface", "abstract class",
        ]
    },

    /* ── Go ── */
    go: {
        codeChance:  0.28,
        digitChance: 0.25,
        digits: ["0", "1", "nil", "true", "false"],
        tokens: [
            "fmt.Println()", "fmt.Printf()", "fmt.Sprintf()", "fmt.Errorf()", "fmt.Scanf()",
            "fmt.Fprintf()", "log.Println()", "log.Fatalf()", "log.Printf()",
            "func main()", "func()", "defer", "go", "chan", "select", "case", "default",
            "make([]int, 0)", "make(map[string]int)", "make(chan int)",
            "len()", "cap()", "append()", "copy()", "delete()", "close()", "panic()", "recover()",
            "strings.Contains()", "strings.HasPrefix()", "strings.HasSuffix()",
            "strings.Split()", "strings.Join()", "strings.TrimSpace()", "strings.Replace()",
            "strings.ToUpper()", "strings.ToLower()", "strings.Index()", "strings.Builder",
            "strconv.Itoa()", "strconv.Atoi()", "strconv.ParseFloat()", "strconv.FormatBool()",
            "os.Open()", "os.Create()", "os.Getenv()", "os.Exit()", "os.Args",
            "errors.New()", "errors.Is()", "errors.As()", "error",
            "sort.Slice()", "sort.Ints()", "sort.Strings()",
            "sync.Mutex{}", "sync.WaitGroup{}", "atomic.AddInt64()",
            "http.Get()", "http.Post()", "http.NewRequest()", "json.Marshal()", "json.Unmarshal()",
            "context.Background()", "context.WithTimeout()", "context.WithCancel()",
            ":=", "var", "interface{}", "struct{}", "type alias",
        ]
    },

    /* ── PHP ── */
    php: {
        codeChance:  0.28,
        digitChance: 0.25,
        digits: ["0", "1", "null", "true", "false", "''", "[]"],
        tokens: [
            "echo", "print", "var_dump()", "print_r()", "die()", "exit()",
            "isset()", "empty()", "is_null()", "is_array()", "is_string()", "is_numeric()",
            "intval()", "floatval()", "strval()", "boolval()", "gettype()",
            "strlen()", "strtolower()", "strtoupper()", "trim()", "ltrim()", "rtrim()",
            "str_replace()", "str_contains()", "str_starts_with()", "str_ends_with()",
            "substr()", "strpos()", "strrpos()", "sprintf()", "printf()", "number_format()",
            "explode()", "implode()", "str_split()", "chunk_split()", "wordwrap()",
            "array_push()", "array_pop()", "array_shift()", "array_unshift()",
            "array_merge()", "array_slice()", "array_splice()", "array_unique()",
            "array_keys()", "array_values()", "array_flip()", "array_reverse()",
            "in_array()", "array_search()", "array_map()", "array_filter()", "array_reduce()",
            "count()", "sort()", "rsort()", "asort()", "arsort()", "ksort()", "krsort()",
            "file_get_contents()", "file_put_contents()", "file_exists()", "is_file()",
            "date()", "time()", "mktime()", "strtotime()", "microtime()",
            "json_encode()", "json_decode()", "serialize()", "unserialize()",
            "header()", "session_start()", "$_GET", "$_POST", "$_SESSION", "$_COOKIE",
            "new", "class", "extends", "implements", "interface", "abstract",
            "public", "private", "protected", "static", "final",
        ]
    },

    /* ── C / C++ ── */
    cpp: {
        codeChance:  0.28,
        digitChance: 0.30,
        digits: ["0", "1", "NULL", "nullptr", "true", "false", "0x0"],
        tokens: [
            "#include <iostream>", "#include <vector>", "#include <string>",
            "#include <map>", "#include <unordered_map>", "#include <algorithm>",
            "#include <cmath>", "#include <cstring>", "#include <memory>",
            "std::cout <<", "std::cin >>", "std::cerr <<", "std::endl",
            "std::string", "std::vector<>", "std::map<>", "std::unordered_map<>",
            "std::set<>", "std::pair<>", "std::tuple<>", "std::optional<>",
            "std::make_shared<>()", "std::make_unique<>()", "std::shared_ptr<>",
            "std::unique_ptr<>", "std::weak_ptr<>",
            "std::sort()", "std::find()", "std::count()", "std::copy()",
            "std::transform()", "std::accumulate()", "std::reverse()",
            "std::begin()", "std::end()", "std::next()", "std::prev()",
            "malloc()", "calloc()", "realloc()", "free()",
            "printf()", "scanf()", "sprintf()", "snprintf()", "fprintf()",
            "sizeof()", "alignof()", "decltype()", "typeid()", "static_assert()",
            "const_cast<>", "static_cast<>", "dynamic_cast<>", "reinterpret_cast<>",
            "virtual", "override", "final", "explicit", "inline", "constexpr", "auto",
            "template<typename T>", "namespace std", "using namespace std",
            "new", "delete", "delete[]", "&ref", "*ptr", "->member",
        ]
    },

};

/* ── Алиасы (slug → ключ темы) ──────────────────────────────────────────── */

const THEME_ALIASES = {
    // C#
    'csharp': 'csharp', 'dotnet': 'csharp', 'net': 'csharp',
    'backend': 'csharp', 'csharp-basics': 'csharp',
    // Python
    'python': 'python', 'django': 'python', 'flask': 'python',
    'fastapi': 'python', 'pandas': 'python', 'ml': 'python',
    // JavaScript
    'javascript': 'javascript', 'js': 'javascript', 'node': 'javascript',
    'nodejs': 'javascript', 'react': 'javascript', 'vue': 'javascript',
    'frontend': 'javascript', 'web': 'javascript',
    // TypeScript
    'typescript': 'typescript', 'ts': 'typescript', 'angular': 'typescript',
    // SQL
    'sql': 'sql', 'postgresql': 'sql', 'mysql': 'sql', 'sqlite': 'sql',
    'mssql': 'sql', 'database': 'sql', 'db': 'sql', 'data': 'sql',
    // Rust
    'rust': 'rust', 'systems': 'rust',
    // Java
    'java': 'java', 'spring': 'java', 'kotlin': 'java',
    // Go
    'go': 'go', 'golang': 'go',
    // PHP
    'php': 'php', 'laravel': 'php', 'symfony': 'php',
    // C/C++
    'cpp': 'cpp', 'c': 'cpp', 'clang': 'cpp',
};

/* ── Определить тему по slug ─────────────────────────────────────────────── */

function resolveTheme(slug) {
    if (!slug) return STAR_THEMES.csharp;
    const key = (slug || '').toLowerCase().trim();
    // Точное совпадение алиаса
    if (THEME_ALIASES[key]) return STAR_THEMES[THEME_ALIASES[key]];
    // Частичное совпадение — первый сегмент до '-'
    const prefix = key.split('-')[0];
    if (THEME_ALIASES[prefix]) return STAR_THEMES[THEME_ALIASES[prefix]];
    // Прямое совпадение с ключом темы
    if (STAR_THEMES[key]) return STAR_THEMES[key];
    // Резерв
    return STAR_THEMES.csharp;
}


/* ══════════════════════════════════════════════════════════════════
   ИНИЦИАЛИЗАЦИЯ
══════════════════════════════════════════════════════════════════ */

document.addEventListener("DOMContentLoaded", () => {

    const starContainer = document.getElementById("stars");
    if (!starContainer) return;

    // Читаем тему из data-атрибута контейнера
    const theme = resolveTheme(starContainer.dataset.theme || '');

    const STAR_COUNT = 35;

    /* ── Параметры анимации ── */
    const STAR_LIFE_TIME   = 500;
    const STAR_TWINKLE_SPEED = 0.02;
    const LIFE_VARIATION   = 60;
    const SPEED_VARIATION  = 0.04;

    const CODE_CHANCE  = theme.codeChance;
    const DIGIT_CHANCE = theme.digitChance;

    /* ── Состояние ── */
    const stars = [];
    let containerWidth  = 0;
    let containerHeight = 0;


    /* ── Размер контейнера ── */

    function updateContainerSize() {
        containerWidth  = starContainer.clientWidth;
        containerHeight = starContainer.clientHeight;
    }

    updateContainerSize();
    window.addEventListener("resize", updateContainerSize);


    /* ── Утилиты ── */

    function randomFromArray(arr) {
        return arr[Math.floor(Math.random() * arr.length)];
    }


    /* ── Зона исключения (напр. .auth-card) ── */

    function getExclusionRect() {
        const card = document.querySelector(".auth-card");
        if (!card) return null;
        const cr  = card.getBoundingClientRect();
        const sr  = starContainer.getBoundingClientRect();
        const pad = 20;
        return {
            x1: cr.left   - sr.left - pad,
            y1: cr.top    - sr.top  - pad,
            x2: cr.right  - sr.left + pad,
            y2: cr.bottom - sr.top  + pad,
        };
    }

    function insideExclusion(x, y, rect) {
        if (!rect) return false;
        return x >= rect.x1 && x <= rect.x2 && y >= rect.y1 && y <= rect.y2;
    }


    /* ── Создание частицы ── */

    function createStar() {

        const star = document.createElement("div");
        const rand = Math.random();

        if (rand < CODE_CHANCE) {
            // Токен кода из темы курса
            star.className   = "code";
            star.textContent = randomFromArray(theme.tokens);
        }
        else if (rand < CODE_CHANCE + DIGIT_CHANCE) {
            // Цифра/литерал из темы курса
            star.className   = "digit";
            star.textContent = randomFromArray(theme.digits);
        }
        else {
            // Обычная звезда (точка)
            star.className = "star";
        }

        /* Позиция — обходим зону карточки */
        const excl = getExclusionRect();
        let x, y, attempts = 0;
        do {
            x = Math.random() * containerWidth;
            y = Math.random() * containerHeight;
            attempts++;
        } while (insideExclusion(x, y, excl) && attempts < 25);

        star.style.left = x + "px";
        star.style.top  = y + "px";

        const size = 1 + Math.random() * 2;

        if (star.className === "star") {
            star.style.width  = size + "px";
            star.style.height = size + "px";
        }
        else if (star.className === "digit") {
            star.style.fontSize = (10 + Math.random() * 8) + "px";
        }
        else {
            star.style.fontSize = (11 + Math.random() * 6) + "px";
        }

        starContainer.appendChild(star);

        stars.push({
            el:   star,
            life: 0,
            maxLife:
                STAR_LIFE_TIME +
                (Math.random() * LIFE_VARIATION - LIFE_VARIATION / 2),
            twinkleSpeed:
                STAR_TWINKLE_SPEED +
                (Math.random() * SPEED_VARIATION - SPEED_VARIATION / 2),
        });
    }


    /* ── Анимация ── */

    function animateStars() {

        for (let i = stars.length - 1; i >= 0; i--) {

            const star = stars[i];
            star.life++;

            const opacity =
                Math.sin(star.life * star.twinkleSpeed) * 0.5 + 0.5;

            star.el.style.opacity =
                opacity * (1 - star.life / star.maxLife);

            if (star.life > star.maxLife) {
                star.el.remove();
                stars.splice(i, 1);
                createStar();
            }
        }

        requestAnimationFrame(animateStars);
    }


    /* ── Старт ── */

    for (let i = 0; i < STAR_COUNT; i++) createStar();
    animateStars();

});
