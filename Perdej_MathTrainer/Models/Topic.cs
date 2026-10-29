namespace MathTrainer.Models;

public enum SchoolLevel { Elementary, Middle, High, University, Research }

public class Topic
{
    public string      Id          { get; set; } = string.Empty;
    public string      Name        { get; set; } = string.Empty;
    public SchoolLevel Level       { get; set; }
    public int         LevelNum    { get; set; } = 1;
    public string      Description { get; set; } = string.Empty;
    public string      Icon        { get; set; } = "📐";
    public string      Theory      { get; set; } = string.Empty;

    public static List<Topic> GetAll() => new()
    {
        // LEVEL 1 - Elementary
        new() { Id="number_theory",       Name="Теория чисел",             LevelNum=1, Level=SchoolLevel.Elementary, Icon="🔢", Description="Делители, простые числа, НОД/НОК",           Theory="Натуральные числа делятся на делители. Простое число имеет ровно два делителя. НОД — наибольший общий делитель, НОК — наименьшее общее кратное." },
        new() { Id="arithmetic_structs",  Name="Арифметические структуры", LevelNum=1, Level=SchoolLevel.Elementary, Icon="➕", Description="Порядок действий, скобки, приоритеты",        Theory="Порядок действий: сначала умножение и деление, затем сложение и вычитание. Скобки меняют порядок." },
        new() { Id="rational_numbers",    Name="Рациональные числа",        LevelNum=1, Level=SchoolLevel.Elementary, Icon="🔣", Description="Дроби, сравнение, операции",                  Theory="Рациональное число — отношение p/q, где q≠0. Для сравнения дробей приводят к общему знаменателю." },
        new() { Id="power_transforms",    Name="Степенные преобразования",  LevelNum=1, Level=SchoolLevel.Elementary, Icon="⚡", Description="Свойства степеней, упрощение",               Theory="aⁿ·aᵐ = aⁿ⁺ᵐ, aⁿ/aᵐ = aⁿ⁻ᵐ, (aⁿ)ᵐ = aⁿᵐ. Любое число в нулевой степени равно 1." },
        new() { Id="percent_models",      Name="Процентные модели",         LevelNum=1, Level=SchoolLevel.Elementary, Icon="💯", Description="Проценты, задачи на увеличение/уменьшение",  Theory="1% = 1/100. Чтобы найти p% от числа X, вычисляют X·p/100." },
        new() { Id="number_patterns",     Name="Числовые закономерности",   LevelNum=1, Level=SchoolLevel.Elementary, Icon="🔍", Description="Последовательности, паттерны",               Theory="Закономерность — правило построения ряда чисел. Нужно найти разность, отношение или другую операцию между соседними членами." },

        // LEVEL 2 - Middle
        new() { Id="linear_equations",    Name="Линейные уравнения",        LevelNum=2, Level=SchoolLevel.Middle, Icon="📊", Description="Уравнения вида ax+b=c",                         Theory="Линейное уравнение решается переносом слагаемых: ax = c-b, x = (c-b)/a. Проверка: подстановка." },
        new() { Id="linear_ineq",         Name="Линейные неравенства",      LevelNum=2, Level=SchoolLevel.Middle, Icon="⚖️", Description="Неравенства, знак при делении на <0",           Theory="При умножении/делении неравенства на отрицательное число знак меняется на противоположный." },
        new() { Id="progressions",        Name="Прогрессии",                LevelNum=2, Level=SchoolLevel.Middle, Icon="📈", Description="Арифметическая и геометрическая прогрессии",    Theory="Арифметическая: aₙ = a₁+(n-1)d. Геометрическая: bₙ = b₁·qⁿ⁻¹." },
        new() { Id="polynomials",         Name="Многочлены",                LevelNum=2, Level=SchoolLevel.Middle, Icon="🔠", Description="Сложение, умножение многочленов",                Theory="Многочлен — сумма одночленов. При умножении каждый член одного множится на каждый член другого." },
        new() { Id="algebraic_transforms",Name="Алгебраические преобразования",LevelNum=2,Level=SchoolLevel.Middle,Icon="🔄",Description="Упрощение выражений",                           Theory="Формулы сокращённого умножения: (a+b)²=a²+2ab+b², (a-b)(a+b)=a²-b²." },
        new() { Id="factorization",       Name="Факторизация выражений",    LevelNum=2, Level=SchoolLevel.Middle, Icon="🔀", Description="Вынесение за скобку, формулы",                  Theory="Разложить на множители — представить многочлен в виде произведения. Основные методы: общий множитель, формулы разности квадратов." },

        // LEVEL 3 - Middle geometry
        new() { Id="planimetry",          Name="Планиметрия",               LevelNum=3, Level=SchoolLevel.Middle, Icon="📐", Description="Треугольники, углы, теорема Пифагора",          Theory="Сумма углов треугольника = 180°. Теорема Пифагора: a²+b²=c² для прямоугольного треугольника." },
        new() { Id="area_geometry",       Name="Геометрия площадей",        LevelNum=3, Level=SchoolLevel.Middle, Icon="🔷", Description="Площади фигур",                                Theory="S прямоугольника = ab. S треугольника = ½·a·h. S круга = πr²." },
        new() { Id="similarity",          Name="Подобие и масштабирование",  LevelNum=3, Level=SchoolLevel.Middle, Icon="🔭", Description="Подобные фигуры, коэффициент подобия",         Theory="Подобные фигуры имеют равные углы и пропорциональные стороны. Площади подобных фигур относятся как квадрат коэффициента подобия." },
        new() { Id="circles_arcs",        Name="Окружности и дуги",         LevelNum=3, Level=SchoolLevel.Middle, Icon="⭕", Description="Длина, центральный угол, хорды",               Theory="Длина окружности C=2πr. Длина дуги = (α/360)·2πr, где α — центральный угол." },
        new() { Id="coord_geometry",      Name="Координатная геометрия",    LevelNum=3, Level=SchoolLevel.Middle, Icon="📍", Description="Расстояние, середина, уравнение прямой",        Theory="Расстояние между точками: d=√((x₂-x₁)²+(y₂-y₁)²). Уравнение прямой: y=kx+b." },
        new() { Id="plane_vectors",       Name="Векторы на плоскости",      LevelNum=3, Level=SchoolLevel.Middle, Icon="➡️", Description="Сложение векторов, скалярное произведение",    Theory="Вектор — направленный отрезок. Скалярное произведение: a·b = |a|·|b|·cos(α)." },

        // LEVEL 4 - High
        new() { Id="quadratic_models",    Name="Квадратичные модели",       LevelNum=4, Level=SchoolLevel.High, Icon="🎯", Description="Квадратные уравнения, дискриминант",             Theory="Решение ax²+bx+c=0: D=b²-4ac, x=(-b±√D)/(2a). При D<0 вещественных корней нет." },
        new() { Id="functions_graphs",    Name="Функции и графики",         LevelNum=4, Level=SchoolLevel.High, Icon="📉", Description="Область определения, свойства функций",          Theory="Функция — правило, сопоставляющее каждому x ровно один y. Нули функции — точки пересечения с Ox." },
        new() { Id="systems_equations",   Name="Системы уравнений",         LevelNum=4, Level=SchoolLevel.High, Icon="🔗", Description="Методы подстановки и сложения",                 Theory="Система уравнений решается методом подстановки или сложения (линейные комбинации уравнений)." },
        new() { Id="parametric_problems", Name="Параметрические задачи",    LevelNum=4, Level=SchoolLevel.High, Icon="🎛️", Description="Уравнения с параметром",                       Theory="Уравнение с параметром а решается для всех допустимых значений а. Исследуют случаи a=0 и a≠0." },
        new() { Id="modular_constructs",  Name="Модульные конструкции",     LevelNum=4, Level=SchoolLevel.High, Icon="📦", Description="Уравнения и неравенства с модулем",             Theory="|x| = x при x≥0 и -x при x<0. Уравнение |f(x)|=g(x) распадается на два случая." },
        new() { Id="extreme_values",      Name="Экстремальные значения",    LevelNum=4, Level=SchoolLevel.High, Icon="🏔️", Description="Максимум/минимум без производной",             Theory="Для квадратного трёхчлена ax²+bx+c вершина параболы при x=-b/(2a), y_min/max = -D/(4a)." },

        // LEVEL 5 - High trigonometry
        new() { Id="trig_ratios",         Name="Тригонометрические отношения",LevelNum=5,Level=SchoolLevel.High, Icon="📐", Description="sin, cos, tg, ctg",                            Theory="sin²α+cos²α=1. tg α = sin α/cos α. Основные значения: sin 30°=0.5, cos 60°=0.5." },
        new() { Id="trig_formulas",       Name="Формулы преобразования",    LevelNum=5, Level=SchoolLevel.High, Icon="🔁", Description="Формулы сложения, двойного угла",               Theory="cos(α+β)=cosα·cosβ-sinα·sinβ. sin(2α)=2sinα·cosα. cos(2α)=cos²α-sin²α." },
        new() { Id="trig_equations",      Name="Тригонометрические уравнения",LevelNum=5,Level=SchoolLevel.High,Icon="🔢", Description="sinx=a, cosx=a, общие решения",                Theory="sinx=a: x=(-1)ⁿarcsin(a)+πn. cosx=a: x=±arccos(a)+2πn." },
        new() { Id="trig_inequalities",   Name="Тригонометрические неравенства",LevelNum=5,Level=SchoolLevel.High,Icon="⚖️",Description="sinx>a, cosx<a",                              Theory="Неравенства sin x > a решают на тригонометрическом круге, находя дугу где синус больше a." },
        new() { Id="periodic_processes",  Name="Периодические процессы",    LevelNum=5, Level=SchoolLevel.High, Icon="🌊", Description="Период, амплитуда, фаза",                       Theory="y=A·sin(ωx+φ): A — амплитуда, T=2π/ω — период, φ — начальная фаза." },
        new() { Id="harmonic_analysis",   Name="Гармонический анализ",      LevelNum=5, Level=SchoolLevel.High, Icon="🎵", Description="Суперпозиция тригонометрических функций",       Theory="Сумма колебаний одной частоты: A₁sinωt+A₂cosωt = A·sin(ωt+φ), где A=√(A₁²+A₂²)." },

        // LEVEL 6 - High calculus intro
        new() { Id="sequence_limits",     Name="Пределы последовательностей",LevelNum=6,Level=SchoolLevel.High, Icon="∞", Description="Сходимость, бесконечно малые",                  Theory="Предел последовательности — число L, к которому стремятся члены при n→∞. lim(1/n)=0." },
        new() { Id="function_limits",     Name="Пределы функций",           LevelNum=6, Level=SchoolLevel.High, Icon="🎯", Description="Замечательные пределы, непрерывность",          Theory="lim(sin x/x)=1 при x→0. Функция непрерывна, если lim f(x) при x→a равен f(a)." },
        new() { Id="derivative",          Name="Производная",               LevelNum=6, Level=SchoolLevel.High, Icon="📐", Description="Правила дифференцирования",                     Theory="(xⁿ)'=n·xⁿ⁻¹. (sin x)'=cos x. (eˣ)'=eˣ. Правило произведения: (uv)'=u'v+uv'." },
        new() { Id="func_analysis",       Name="Исследование функций",      LevelNum=6, Level=SchoolLevel.High, Icon="🔬", Description="Монотонность, экстремумы через производную",   Theory="Функция возрастает где f'(x)>0, убывает где f'(x)<0. Экстремум в точках где f'=0." },
        new() { Id="integral_methods",    Name="Интегральные методы",       LevelNum=6, Level=SchoolLevel.High, Icon="∫", Description="Первообразная, формула Ньютона-Лейбница",       Theory="∫xⁿdx = xⁿ⁺¹/(n+1)+C. ∫∫ₐᵇf(x)dx = F(b)-F(a), где F' = f." },
        new() { Id="optimization",        Name="Оптимизационные задачи",    LevelNum=6, Level=SchoolLevel.High, Icon="🏆", Description="Задачи на нахождение наибольшего/наименьшего",  Theory="Составляют функцию цели, берут производную, находят критические точки, определяют экстремум." },

        // LEVEL 7 - University discrete
        new() { Id="combinatorics",       Name="Комбинаторные конструкции", LevelNum=7, Level=SchoolLevel.University, Icon="🎲", Description="Перестановки, размещения, сочетания",   Theory="C(n,k)=n!/(k!(n-k)!). A(n,k)=n!/(n-k)!. Число перестановок n элементов = n!." },
        new() { Id="graph_theory",        Name="Теория графов",             LevelNum=7, Level=SchoolLevel.University, Icon="🕸️", Description="Вершины, рёбра, пути, эйлеровы графы", Theory="Граф эйлеров если каждая вершина чётной степени. Гамильтонов путь проходит через каждую вершину ровно раз." },
        new() { Id="logic_systems",       Name="Логические системы",        LevelNum=7, Level=SchoolLevel.University, Icon="💡", Description="Высказывания, таблицы истинности",       Theory="Конъюнкция A∧B истинна когда оба истинны. Дизъюнкция A∨B ложна только когда оба ложны." },
        new() { Id="recurrence",          Name="Рекуррентные соотношения",  LevelNum=7, Level=SchoolLevel.University, Icon="🔄", Description="Числа Фибоначчи, характеристические уравнения", Theory="Рекуррентное соотношение: aₙ выражается через предыдущие члены. Числа Фибоначчи: Fₙ=Fₙ₋₁+Fₙ₋₂." },
        new() { Id="boolean_algebra",     Name="Булева алгебра",            LevelNum=7, Level=SchoolLevel.University, Icon="0️⃣", Description="Операции над битами, законы",            Theory="Законы де Моргана: ¬(A∧B)=¬A∨¬B. XOR: A⊕B=1 только при A≠B." },
        new() { Id="algo_thinking",       Name="Алгоритмическое мышление",  LevelNum=7, Level=SchoolLevel.University, Icon="🖥️", Description="Сложность, O-нотация, алгоритмы",      Theory="O(n) — линейная сложность, O(n²) — квадратичная, O(log n) — логарифмическая. Бинарный поиск: O(log n)." },

        // LEVEL 8 - University probability
        new() { Id="classical_prob",      Name="Классическая вероятность",  LevelNum=8, Level=SchoolLevel.University, Icon="🎯", Description="P=m/n, благоприятные исходы",          Theory="P(A)=m/n, где m — число благоприятных, n — число всех равновозможных исходов." },
        new() { Id="combinat_prob",       Name="Комбинаторная вероятность", LevelNum=8, Level=SchoolLevel.University, Icon="🃏", Description="Вероятности с сочетаниями",              Theory="Используют формулы комбинаторики для подсчёта числа благоприятных и всех исходов." },
        new() { Id="random_variables",    Name="Случайные величины",        LevelNum=8, Level=SchoolLevel.University, Icon="📊", Description="Дискретные, непрерывные СВ",            Theory="Дискретная СВ задаётся законом распределения — таблицей значений xᵢ и вероятностей pᵢ." },
        new() { Id="expected_value",      Name="Математическое ожидание",   LevelNum=8, Level=SchoolLevel.University, Icon="⚖️", Description="M(X), дисперсия D(X)",                 Theory="M(X) = Σxᵢpᵢ. D(X) = M(X²) - (M(X))². Стандартное отклонение σ = √D(X)." },
        new() { Id="statistical_analysis",Name="Статистический анализ",    LevelNum=8, Level=SchoolLevel.University, Icon="📈", Description="Среднее, медиана, мода",                Theory="Среднее = (сумма элементов)/n. Медиана — средний элемент упорядоченного ряда. Мода — наиболее частый." },
        new() { Id="decision_theory",     Name="Теория принятия решений",   LevelNum=8, Level=SchoolLevel.University, Icon="🎰", Description="Дерево решений, критерии выбора",       Theory="Ожидаемая полезность = Σ(вероятность × выигрыш). Критерий максимина — максимизирует минимальный выигрыш." },

        // LEVEL 9 - Olympic
        new() { Id="invariants",          Name="Инварианты",                LevelNum=9, Level=SchoolLevel.University, Icon="🔒", Description="Сохраняющиеся характеристики",          Theory="Инвариант — величина, не меняющаяся при допустимых операциях. Чётность суммы, остаток от деления." },
        new() { Id="dirichlet_principle", Name="Принцип Дирихле",           LevelNum=9, Level=SchoolLevel.University, Icon="🐦", Description="Ящики и кролики",                      Theory="Если n+1 предмет разложить по n ящикам, то хотя бы в одном ящике окажется не менее 2 предметов." },
        new() { Id="estimation_methods",  Name="Метод оценок",              LevelNum=9, Level=SchoolLevel.University, Icon="🎯", Description="Верхние и нижние оценки",               Theory="Оценить величину — значит найти числа A и B такие что A ≤ X ≤ B. Часто используют неравенства AM-GM." },
        new() { Id="constructive_methods",Name="Конструктивные методы",     LevelNum=9, Level=SchoolLevel.University, Icon="🔧", Description="Построение примеров и контрпримеров",   Theory="Для доказательства существования конструируют конкретный пример. Для опровержения ищут контрпример." },
        new() { Id="extremal_principle",  Name="Экстремальный принцип",     LevelNum=9, Level=SchoolLevel.University, Icon="🏔️", Description="Рассмотрение экстремального элемента",  Theory="Рассматривают наибольший или наименьший элемент множества и получают противоречие или конструкцию." },
        new() { Id="nonstandard_problems",Name="Нестандартные задачи",      LevelNum=9, Level=SchoolLevel.University, Icon="🎪", Description="Задачи требующие нестандартных идей",  Theory="Олимпиадные задачи часто требуют нестандартных подходов: инвариантов, раскраски, двойного подсчёта." },

        // LEVEL 10 - University advanced
        new() { Id="linear_algebra",      Name="Линейная алгебра",          LevelNum=10, Level=SchoolLevel.University, Icon="🔢", Description="Матрицы, определители, системы",     Theory="det(AB)=det(A)·det(B). Система Ax=b имеет решение тогда и только тогда когда rang(A)=rang(A|b)." },
        new() { Id="analytic_geometry",   Name="Аналитическая геометрия",   LevelNum=10, Level=SchoolLevel.University, Icon="📍", Description="Прямые и плоскости в пространстве",  Theory="Уравнение плоскости: ax+by+cz+d=0. Прямая в пространстве задаётся точкой и направляющим вектором." },
        new() { Id="adv_number_theory",   Name="Теория чисел (высшая)",     LevelNum=10, Level=SchoolLevel.University, Icon="🔍", Description="Сравнения, малая теорема Ферма",     Theory="Малая теорема Ферма: aᵖ≡a (mod p) для простого p. Китайская теорема об остатках — решение систем сравнений." },
        new() { Id="abstract_algebra",    Name="Абстрактная алгебра",       LevelNum=10, Level=SchoolLevel.University, Icon="🔮", Description="Группы, кольца, поля",               Theory="Группа — множество с операцией удовлетворяющей: замкнутость, ассоциативность, нейтральный, обратный." },
        new() { Id="diff_equations",      Name="Дифференциальные уравнения",LevelNum=10, Level=SchoolLevel.University, Icon="∂", Description="ОДУ первого и второго порядка",        Theory="ОДУ y'=f(x) — простейшее. y'+p(x)y=q(x) — линейное. Решают методом вариации постоянных." },
        new() { Id="complex_analysis",    Name="Комплексный анализ",        LevelNum=10, Level=SchoolLevel.University, Icon="ℂ", Description="Комплексные числа, функции, интегралы", Theory="Комплексное число z=a+bi. |z|=√(a²+b²). Формула Эйлера: eⁱˣ=cos x+i·sin x." },

        // LEVEL 11 - Research
        new() { Id="proof_theory",        Name="Теория доказательств",      LevelNum=11, Level=SchoolLevel.Research, Icon="📜", Description="Формальные доказательства, вывод",     Theory="Формальное доказательство — конечная последовательность формул, каждая из которых аксиома или следствие правил вывода." },
        new() { Id="category_theory",     Name="Теория категорий",          LevelNum=11, Level=SchoolLevel.Research, Icon="🗂️", Description="Объекты, морфизмы, функторы",         Theory="Категория: объекты и морфизмы. Функтор сохраняет структуру между категориями. Монада — функтор с дополнительной структурой." },
        new() { Id="topology",            Name="Топология",                 LevelNum=11, Level=SchoolLevel.Research, Icon="🍩", Description="Непрерывные преобразования, гомеоморфизмы", Theory="Топология изучает свойства, сохраняющиеся при непрерывных деформациях. Тор и сфера — топологически разные." },
        new() { Id="functional_analysis", Name="Функциональный анализ",     LevelNum=11, Level=SchoolLevel.Research, Icon="🌐", Description="Банаховы пространства, операторы",     Theory="Банахово пространство — полное нормированное пространство. Теорема Хана-Банаха — о продолжении функционалов." },

        // LEVEL 12 - Research frontier
        new() { Id="knot_theory",         Name="Теория узлов",              LevelNum=12, Level=SchoolLevel.Research, Icon="🪢", Description="Инварианты узлов, трилистник",          Theory="Узел — вложение окружности в ℝ³. Инварианты: число перекрёстков, полином Александера, инвариант Джонса." },
        new() { Id="elliptic_curves",     Name="Эллиптические кривые",      LevelNum=12, Level=SchoolLevel.Research, Icon="🌙", Description="Уравнение y²=x³+ax+b, группа точек",   Theory="Эллиптическая кривая: y²=x³+ax+b. Точки образуют абелеву группу. Используются в криптографии и доказательстве ВТФ." },
        new() { Id="automorphic_forms",   Name="Автоморфные формы",         LevelNum=12, Level=SchoolLevel.Research, Icon="🎭", Description="Модулярные формы, L-функции",          Theory="Модулярная форма — аналитическая функция на верхней полуплоскости с симметриями относительно SL(2,ℤ)." },
        new() { Id="arithmetic_geometry", Name="Арифметическая геометрия",  LevelNum=12, Level=SchoolLevel.Research, Icon="🔭", Description="Многообразия над числовыми полями",     Theory="Арифметическая геометрия изучает геометрические объекты над числовыми полями. Схемы Гротендика — ключевой инструмент." },

        // LEVEL 13 - Unsolved problems
        new() { Id="riemann_hypothesis",  Name="Гипотеза Римана",           LevelNum=13, Level=SchoolLevel.Research, Icon="ℝ", Description="Нули дзета-функции Римана",             Theory="ζ(s)=Σ1/nˢ. Все нетривиальные нули лежат на критической прямой Re(s)=1/2 — не доказано с 1859 года." },
        new() { Id="p_vs_np",             Name="P против NP",               LevelNum=13, Level=SchoolLevel.Research, Icon="💻", Description="Задачи полиномиальной сложности",       Theory="P — задачи, решаемые за полиномиальное время. NP — проверяемые за полиномиальное время. P=NP? — открытый вопрос." },
        new() { Id="hodge_conjecture",    Name="Гипотеза Ходжа",            LevelNum=13, Level=SchoolLevel.Research, Icon="🔺", Description="Когомологии алгебраических многообразий", Theory="Каждый класс Ходжа на проективном комплексном многообразии является рациональной линейной комбинацией классов алгебраических циклов." },
        new() { Id="navier_stokes",       Name="Уравнения Навье—Стокса",    LevelNum=13, Level=SchoolLevel.Research, Icon="🌊", Description="Существование гладких решений",         Theory="∂v/∂t+(v·∇)v = -∇p/ρ+ν∆v. Существование и гладкость глобальных решений в 3D — открытая задача." },
        new() { Id="yang_mills",          Name="Теория Янга—Миллса",        LevelNum=13, Level=SchoolLevel.Research, Icon="⚛️", Description="Щель в массе, квантовые поля",         Theory="Неабелевы калибровочные теории описывают сильное взаимодействие. Существование щели в массе не доказано математически." },
        new() { Id="bsd_conjecture",      Name="Гипотеза Бёрча—Свиннертон-Дайера",LevelNum=13,Level=SchoolLevel.Research,Icon="🌙",Description="Ранг эллиптических кривых",    Theory="Ранг группы рациональных точек эллиптической кривой равен порядку нуля L-функции в точке s=1 — гипотеза BSD." },

        // LEVEL 14 - Millennium
        new() { Id="prime_distribution",  Name="Распределение простых чисел",LevelNum=14,Level=SchoolLevel.Research,Icon="🔢", Description="Теорема о простых числах, ζ-функция", Theory="π(x)~x/ln(x). Теорема о простых числах доказана в 1896. Гипотеза Римана дала бы более точную оценку." },
        new() { Id="computational_complexity",Name="Вычислительная сложность",LevelNum=14,Level=SchoolLevel.Research,Icon="⚙️",Description="NP-полнота, иерархии сложности",     Theory="NP-полная задача: SAT, задача о рюкзаке, задача коммивояжёра. Если P=NP, все NP-задачи решаемы за полином." },
        new() { Id="quantum_field",       Name="Квантовая теория поля",     LevelNum=14, Level=SchoolLevel.Research, Icon="⚛️", Description="Лагранжиан, перенормировка",           Theory="КТП объединяет КМ и СТО. Перенормировка устраняет расходимости. Стандартная модель описана КТП." },
        new() { Id="manifold_geometry",   Name="Геометрия многообразий",    LevelNum=14, Level=SchoolLevel.Research, Icon="🌐", Description="Риманова геометрия, тензоры",          Theory="Многообразие — топологическое пространство локально похожее на ℝⁿ. Тензор кривизны Римана характеризует искривление." },

        // LEVEL 15 - Terra Incognita
        new() { Id="unknown_structures",  Name="Неизвестные структуры",     LevelNum=15, Level=SchoolLevel.Research, Icon="❓", Description="Математика за пределами известного",   Theory="Математика непрерывно открывает новые структуры: ∞-категории, гомотопическая теория типов, унивалентные основания." },
        new() { Id="beyond_axioms",       Name="За пределами аксиом",       LevelNum=15, Level=SchoolLevel.Research, Icon="♾️", Description="Независимость, большие кардиналы",     Theory="Гёдель показал неполноту арифметики. Гипотеза континуума независима от ZFC. Большие кардиналы расширяют систему." },
        new() { Id="new_math_objects",    Name="Новые математические объекты",LevelNum=15,Level=SchoolLevel.Research,Icon="🌟",Description="Открытия XXI века",                  Theory="Спектральные последовательности, дифференциальные грады, мотивы — новые объекты современной математики." },

        // LEVEL 16 - Singularity
        new() { Id="singularity",         Name="Сингулярность знания",      LevelNum=16, Level=SchoolLevel.Research, Icon="🌌", Description="Граница математического знания",        Theory="На переднем крае математики стоят нерешённые задачи тысячелетия, гипотезы Ленглендса, теория мотивов и другие." },
    };
}
