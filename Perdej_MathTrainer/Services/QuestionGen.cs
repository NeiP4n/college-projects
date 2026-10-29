using MathTrainer.Models;
using System.Text.Json;

namespace MathTrainer.Services;

public class QuestionGen
{
    private readonly Random _rnd = new();

    private static Dictionary<string, string[][]>? _banks;
    private static Dictionary<string, string[][]> Banks =>
        _banks ??= BuildBanks();

    public List<Question> Generate(string topicId, int count = 5) => topicId switch
    {
        "counting"              => Counting(count),
        "arithmetic"            => Arithmetic(count),
        "powers"                => Powers(count),
        "fractions"             => Fractions(count),
        "percentages"           => Percentages(count),
        "linear_eq"             => LinearEq(count),
        "inequalities"          => Inequalities(count),
        "progression"           => Progression(count),
        "areas"                 => Areas(count),
        // Level 1
        "number_theory"         => NumberTheory(count),
        "arithmetic_structs"    => ArithmeticStructs(count),
        "rational_numbers"      => RationalNumbers(count),
        "power_transforms"      => PowerTransforms(count),
        "percent_models"        => PercentModels(count),
        "number_patterns"       => NumberPatterns(count),
        // Level 2
        "linear_equations"      => LinearEquations(count),
        "linear_ineq"           => LinearIneq(count),
        "progressions"          => Progressions(count),
        "polynomials"           => Polynomials(count),
        "algebraic_transforms"  => AlgebraicTransforms(count),
        "factorization"         => Factorization(count),
        // Level 3
        "planimetry"            => Planimetry(count),
        "area_geometry"         => AreaGeometry(count),
        "similarity"            => Similarity(count),
        "circles_arcs"          => CirclesArcs(count),
        "coord_geometry"        => CoordGeometry(count),
        "plane_vectors"         => PlaneVectors(count),
        // Level 4
        "quadratic_models"      => QuadraticModels(count),
        "functions_graphs"      => FunctionsGraphs(count),
        "systems_equations"     => SystemsEquations(count),
        "parametric_problems"   => ParametricProblems(count),
        "modular_constructs"    => ModularConstructs(count),
        "extreme_values"        => ExtremeValues(count),
        // Level 5
        "trig_ratios"           => TrigRatios(count),
        "trig_formulas"         => TrigFormulas(count),
        "trig_equations"        => TrigEquations(count),
        "trig_inequalities"     => TrigInequalities(count),
        "periodic_processes"    => PeriodicProcesses(count),
        "harmonic_analysis"     => HarmonicAnalysis(count),
        // Level 6
        "sequence_limits"       => SequenceLimits(count),
        "function_limits"       => FunctionLimits(count),
        "derivative"            => Derivative(count),
        "func_analysis"         => FuncAnalysis(count),
        "integral_methods"      => IntegralMethods(count),
        "optimization"          => Optimization(count),
        // Level 7
        "combinatorics"         => Combinatorics(count),
        "graph_theory"          => GraphTheory(count),
        "logic_systems"         => LogicSystems(count),
        "recurrence"            => Recurrence(count),
        "boolean_algebra"       => BooleanAlgebra(count),
        "algo_thinking"         => AlgoThinking(count),
        // Level 8
        "classical_prob"        => ClassicalProb(count),
        "combinat_prob"         => CombinatProb(count),
        "random_variables"      => RandomVariables(count),
        "expected_value"        => ExpectedValue(count),
        "statistical_analysis"  => StatisticalAnalysis(count),
        "decision_theory"       => DecisionTheory(count),
        // Level 9
        "invariants"            => Invariants(count),
        "dirichlet_principle"   => DirichletPrinciple(count),
        "estimation_methods"    => EstimationMethods(count),
        "constructive_methods"  => ConstructiveMethods(count),
        "extremal_principle"    => ExtremalPrinciple(count),
        "nonstandard_problems"  => NonstandardProblems(count),
        // Levels 10+
        "linear_algebra"        => TheoryQ(count, Banks["linear_algebra"]),
        "analytic_geometry"     => TheoryQ(count, Banks["analytic_geometry"]),
        "adv_number_theory"     => TheoryQ(count, Banks["adv_number_theory"]),
        "abstract_algebra"      => TheoryQ(count, Banks["abstract_algebra"]),
        "diff_equations"        => TheoryQ(count, Banks["diff_equations"]),
        "complex_analysis"      => TheoryQ(count, Banks["complex_analysis"]),
        "proof_theory"          => TheoryQ(count, Banks["proof_theory"]),
        "category_theory"       => TheoryQ(count, Banks["category_theory"]),
        "topology"              => TheoryQ(count, Banks["topology"]),
        "functional_analysis"   => TheoryQ(count, Banks["functional_analysis"]),
        "knot_theory"           => TheoryQ(count, Banks["knot_theory"]),
        "elliptic_curves"       => TheoryQ(count, Banks["elliptic_curves"]),
        "automorphic_forms"     => TheoryQ(count, Banks["automorphic_forms"]),
        "arithmetic_geometry"   => TheoryQ(count, Banks["arithmetic_geometry"]),
        "riemann_hypothesis"    => TheoryQ(count, Banks["riemann_hypothesis"]),
        "p_vs_np"               => TheoryQ(count, Banks["p_vs_np"]),
        "hodge_conjecture"      => TheoryQ(count, Banks["hodge_conjecture"]),
        "navier_stokes"         => TheoryQ(count, Banks["navier_stokes"]),
        "yang_mills"            => TheoryQ(count, Banks["yang_mills"]),
        "bsd_conjecture"        => TheoryQ(count, Banks["bsd_conjecture"]),
        "prime_distribution"    => TheoryQ(count, Banks["prime_distribution"]),
        "computational_complexity" => TheoryQ(count, Banks["computational_complexity"]),
        "quantum_field"         => TheoryQ(count, Banks["quantum_field"]),
        "manifold_geometry"     => TheoryQ(count, Banks["manifold_geometry"]),
        "unknown_structures"    => TheoryQ(count, Banks["unknown_structures"]),
        "beyond_axioms"         => TheoryQ(count, Banks["beyond_axioms"]),
        "new_math_objects"      => TheoryQ(count, Banks["new_math_objects"]),
        "singularity"           => TheoryQ(count, Banks["singularity"]),
        _                       => Counting(count)
    };

    // ── HELPERS ──────────────────────────────────────────────────────────────

    private string[] Shuffle(string a, string b, string c, string d)
        => new[] { a, b, c, d }.OrderBy(_ => _rnd.Next()).ToArray();

    private double[] ShuffleDbl(double[] arr)
        => arr.OrderBy(_ => _rnd.Next()).ToArray();

    private static int FindIdx(string[] opts, string val)
    { int i = Array.IndexOf(opts, val); return i >= 0 ? i : 0; }

    private static int FindIdxDbl(double[] opts, double val)
    { for (int i = 0; i < opts.Length; i++) if (Math.Abs(opts[i] - val) < 0.001) return i; return 0; }

    private static int GCD(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
    private static int LCM(int a, int b) => a / GCD(a, b) * b;
    private static long Factorial(int n) { long r = 1; for (int i = 2; i <= n; i++) r *= i; return r; }

    private List<Question> TheoryQ(int count, string[][] bank)
    {
        var qs = new List<Question>();
        var pool = bank.OrderBy(_ => _rnd.Next()).Take(count).ToList();
        foreach (var row in pool)
        {
            var opts = Shuffle(row[1], row[2], row[3], row[4]);
            qs.Add(new Question { Text = row[0], Options = opts, CorrectIndex = FindIdx(opts, row[1]), Explanation = row[1] });
        }
        return qs;
    }

    // ── LEVEL 1 ──────────────────────────────────────────────────────────────

    private List<Question> NumberTheory(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 30), b = _rnd.Next(2, 20);
            int gcd = GCD(a, b);
            q.Text = $"НОД({a}, {b}) = ?";
            q.Options = Shuffle($"{gcd}", $"{gcd + 1}", $"{a}", $"{b}");
            q.CorrectIndex = FindIdx(q.Options, $"{gcd}");
            q.Explanation = $"НОД({a},{b}) = {gcd}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> ArithmeticStructs(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 10), b = _rnd.Next(2, 8), c = _rnd.Next(1, 6);
            int r = a + b * c;
            q.Text = $"{a} + {b} × {c} = ?";
            q.Options = Shuffle($"{r}", $"{(a + b) * c}", $"{a * b + c}", $"{r + 1}");
            q.CorrectIndex = FindIdx(q.Options, $"{r}");
            q.Explanation = $"Сначала умножение: {b}×{c}={b*c}, затем {a}+{b*c}={r}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> RationalNumbers(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int n1 = _rnd.Next(1, 6), d1 = _rnd.Next(2, 8);
            int n2 = _rnd.Next(1, 6), d2 = d1;
            int numSum = n1 + n2, denSum = d1;
            int g = GCD(numSum, denSum);
            string correct = g == denSum ? $"{numSum / g}" : $"{numSum / g}/{denSum / g}";
            q.Text = $"{n1}/{d1} + {n2}/{d2} = ?";
            q.Options = Shuffle(correct, $"{n1 + n2}/{d1 + d2}", $"{n1 * n2}/{d1}", $"{(n1 + n2 + 1)}/{d1}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"{n1}/{d1} + {n2}/{d2} = {numSum}/{denSum} = {correct}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> PowerTransforms(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int b = _rnd.Next(2, 6);
            int e1 = _rnd.Next(1, 4), e2 = _rnd.Next(1, 4);
            int eSum = e1 + e2;
            q.Text = $"{b}^{e1} × {b}^{e2} = {b}^?";
            q.Options = Shuffle($"{eSum}", $"{e1 * e2}", $"{e1 + e2 + 1}", $"{(e1 - e2 < 0 ? 0 : e1 - e2)}");
            q.CorrectIndex = FindIdx(q.Options, $"{eSum}");
            q.Explanation = $"aⁿ·aᵐ = aⁿ⁺ᵐ: {b}^{e1}·{b}^{e2} = {b}^{eSum}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> PercentModels(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int total = _rnd.Next(5, 25) * 10, pct = _rnd.Next(1, 6) * 10;
            int res = total * pct / 100;
            if (i % 2 == 0)
            {
                q.Text = $"Число увеличили на {pct}%. Было {total}. Стало?";
                int became = total + res;
                q.Options = Shuffle($"{became}", $"{total - res}", $"{res}", $"{became + 10}");
                q.CorrectIndex = FindIdx(q.Options, $"{became}");
                q.Explanation = $"{total} + {total}×{pct}/100 = {total}+{res} = {became}";
            }
            else
            {
                q.Text = $"{pct}% от {total} = ?";
                q.Options = Shuffle($"{res}", $"{res + 10}", $"{res - 5}", $"{total / pct}");
                q.CorrectIndex = FindIdx(q.Options, $"{res}");
                q.Explanation = $"{total}×{pct}/100 = {res}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> NumberPatterns(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int start = _rnd.Next(1, 10), step = _rnd.Next(2, 6);
            int next = start + 4 * step;
            q.Text = $"Следующее число: {start}, {start+step}, {start+2*step}, {start+3*step}, ?";
            q.Options = Shuffle($"{next}", $"{next+1}", $"{next-step}", $"{next+step}");
            q.CorrectIndex = FindIdx(q.Options, $"{next}");
            q.Explanation = $"Шаг +{step}, следующий: {next}";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 2 ──────────────────────────────────────────────────────────────

    private List<Question> LinearEquations(int n) => LinearEq(n);
    private List<Question> LinearIneq(int n) => Inequalities(n);
    private List<Question> Progressions(int n) => Progression(n);

    private List<Question> Polynomials(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(1, 5), b = _rnd.Next(1, 8);
            int c = _rnd.Next(1, 5), d = _rnd.Next(1, 8);
            int coefX = a + c, coefFree = b + d;
            q.Text = $"({a}x + {b}) + ({c}x + {d}) = ?";
            q.Options = Shuffle($"{coefX}x + {coefFree}", $"{a*c}x + {b+d}", $"{coefX}x + {b*d}", $"{a+c+1}x + {coefFree}");
            q.CorrectIndex = FindIdx(q.Options, $"{coefX}x + {coefFree}");
            q.Explanation = $"({a}+{c})x + ({b}+{d}) = {coefX}x + {coefFree}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> AlgebraicTransforms(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 8);
            int a2 = a * a;
            q.Text = $"Раскройте: (x + {a})² = ?";
            string correct = $"x² + {2*a}x + {a2}";
            q.Options = Shuffle(correct, $"x² + {a}x + {a2}", $"x² + {2*a}x + {a}", $"x² + {a2}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"(x+a)² = x²+2ax+a²: x²+{2*a}x+{a2}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Factorization(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 6), b = _rnd.Next(2, 6);
            int diff = a * a - b * b;
            q.Text = $"{a}² - {b}² = ?";
            string correct = $"({a}-{b})({a}+{b})";
            q.Options = Shuffle(correct, $"({a}+{b})²", $"({a}-{b})²", $"{a}({a}-{b})");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"a²-b²=(a-b)(a+b): ({a}-{b})({a}+{b})={diff}";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 3 ──────────────────────────────────────────────────────────────

    private List<Question> Planimetry(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            if (i % 2 == 0)
            {
                int a = _rnd.Next(3, 12), b = _rnd.Next(3, 12);
                int c2 = a * a + b * b;
                q.Text = $"Прямоугольный треугольник: катеты {a} и {b}. Гипотенуза²=?";
                q.Options = Shuffle($"{c2}", $"{a + b}", $"{c2 + 1}", $"{(a + b) * (a + b)}");
                q.CorrectIndex = FindIdx(q.Options, $"{c2}");
                q.Explanation = $"c² = {a}²+{b}² = {a*a}+{b*b} = {c2}";
            }
            else
            {
                int ang1 = _rnd.Next(30, 80), ang2 = _rnd.Next(20, 90 - ang1);
                int ang3 = 180 - ang1 - ang2;
                q.Text = $"Углы треугольника: {ang1}°, {ang2}°. Третий угол?";
                q.Options = Shuffle($"{ang3}", $"{ang3 + 10}", $"{ang3 - 10}", $"{180 - ang1}");
                q.CorrectIndex = FindIdx(q.Options, $"{ang3}");
                q.Explanation = $"180-{ang1}-{ang2}={ang3}°";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> AreaGeometry(int n) => Areas(n);

    private List<Question> Similarity(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int side1 = _rnd.Next(2, 6), k = _rnd.Next(2, 4);
            int side2 = side1 * k;
            int area1 = side1 * side1, area2 = side2 * side2;
            q.Text = $"Стороны подобных квадратов {side1} и {side2}. Отношение площадей?";
            string correct = $"{k * k}:1";
            q.Options = Shuffle(correct, $"{k}:1", $"{k * k + 1}:1", $"1:{k}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"Коэффициент подобия {k}, площади: {k}²={k*k} раза";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> CirclesArcs(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int r = _rnd.Next(2, 8);
            double circ = Math.Round(2 * Math.PI * r, 1);
            q.Text = $"Длина окружности с r={r} (π≈3.14)?";
            double correct = Math.Round(2 * 3.14 * r, 1);
            q.Options = Shuffle($"{correct:F1}", $"{Math.Round(3.14 * r, 1):F1}", $"{Math.Round(2 * 3.14 * r + 1, 1):F1}", $"{r * r}");
            q.CorrectIndex = FindIdx(q.Options, $"{correct:F1}");
            q.Explanation = $"C=2πr=2×3.14×{r}={correct:F1}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> CoordGeometry(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int x1 = _rnd.Next(-5, 5), y1 = _rnd.Next(-5, 5);
            int x2 = _rnd.Next(-5, 5), y2 = _rnd.Next(-5, 5);
            int dx = x2 - x1, dy = y2 - y1;
            double d = Math.Round(Math.Sqrt(dx * dx + dy * dy), 2);
            q.Text = $"Расстояние между ({x1},{y1}) и ({x2},{y2})?";
            q.Options = Shuffle($"{d:F2}", $"{Math.Abs(dx) + Math.Abs(dy)}", $"{Math.Round(d + 1, 2):F2}", $"{Math.Round(d * 2, 2):F2}");
            q.CorrectIndex = FindIdx(q.Options, $"{d:F2}");
            q.Explanation = $"d=√(({dx})²+({dy})²)=√{dx*dx+dy*dy}≈{d:F2}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> PlaneVectors(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int ax = _rnd.Next(1, 5), ay = _rnd.Next(1, 5);
            int bx = _rnd.Next(1, 5), by = _rnd.Next(1, 5);
            int dot = ax * bx + ay * by;
            q.Text = $"Скалярное произведение a=({ax},{ay}) и b=({bx},{by})?";
            q.Options = Shuffle($"{dot}", $"{ax * bx}", $"{dot + 1}", $"{(ax + bx) * (ay + by)}");
            q.CorrectIndex = FindIdx(q.Options, $"{dot}");
            q.Explanation = $"a·b={ax}×{bx}+{ay}×{by}={dot}";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 4 ──────────────────────────────────────────────────────────────

    private List<Question> QuadraticModels(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int x1 = _rnd.Next(-5, 6), x2 = _rnd.Next(-5, 6);
            int bCoef = -(x1 + x2), cCoef = x1 * x2;
            string sign1 = bCoef >= 0 ? "+" : "-";
            string sign2 = cCoef >= 0 ? "+" : "-";
            q.Text = $"Корни x²{(bCoef >= 0 ? "+" : "")}{bCoef}x{(cCoef >= 0 ? "+" : "")}{cCoef}=0?";
            string correct = $"x={x1}, x={x2}";
            q.Options = Shuffle(correct, $"x={x1+1}, x={x2}", $"x={-x1}, x={-x2}", $"x={x1}, x={x2+1}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"(x-{x1})(x-{x2})=0, x={x1} или x={x2}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> FunctionsGraphs(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int k = _rnd.Next(-4, 5);
            int b = _rnd.Next(-5, 6);
            if (k == 0) k = 1;
            q.Text = $"Нуль функции f(x)={k}x+{b}?";
            if (k != 0)
            {
                string xZero = $"{-b}/{k}";
                double xVal = (double)(-b) / k;
                string correct = $"x={xVal:G4}";
                q.Options = Shuffle(correct, $"x={xVal + 1:G4}", $"x={-xVal:G4}", $"x={(double)b / k:G4}");
                q.CorrectIndex = FindIdx(q.Options, correct);
                q.Explanation = $"{k}x+{b}=0, x={-b}/{k}={xVal:G4}";
            }
            else
            {
                q.Text = "Нуль функции f(x)=5?";
                q.Options = Shuffle("Нет нулей", "x=0", "x=5", "x=-5");
                q.CorrectIndex = FindIdx(q.Options, "Нет нулей");
                q.Explanation = "Константа 5≠0 нигде";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> SystemsEquations(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int x = _rnd.Next(-5, 6), y = _rnd.Next(-5, 6);
            int a1 = _rnd.Next(1, 4), b1 = _rnd.Next(1, 4);
            int a2 = _rnd.Next(1, 4), b2 = _rnd.Next(1, 4);
            int c1 = a1 * x + b1 * y, c2 = a2 * x + b2 * y;
            q.Text = $"Система: {a1}x+{b1}y={c1}; {a2}x+{b2}y={c2}. Решение?";
            string correct = $"x={x}, y={y}";
            q.Options = Shuffle(correct, $"x={x+1}, y={y}", $"x={y}, y={x}", $"x={-x}, y={y}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"x={x}, y={y} — проверка: {a1}·{x}+{b1}·{y}={c1}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> ParametricProblems(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int aVal = _rnd.Next(1, 5);
            q.Text = $"При каком a уравнение ax=6 имеет единственный корень?";
            q.Options = Shuffle("a≠0", "a=0", "a=6", "a=1");
            q.CorrectIndex = FindIdx(q.Options, "a≠0");
            q.Explanation = "ax=6 имеет единственный корень при a≠0: x=6/a";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> ModularConstructs(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 8);
            q.Text = $"|x| = {a}. Решение?";
            string correct = $"x={a} или x=-{a}";
            q.Options = Shuffle(correct, $"x={a}", $"x=-{a}", $"x=±{a+1}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"|x|={a} → x={a} или x=-{a}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> ExtremeValues(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(1, 4), b = _rnd.Next(-6, 7);
            int xv = -b / (2 * a);
            double yv = (double)(-(b * b)) / (4 * a);
            q.Text = $"Минимум f(x)={a}x²+{b}x достигается при x=?";
            q.Options = Shuffle($"{xv}", $"{xv + 1}", $"{b}", $"{-b}");
            q.CorrectIndex = FindIdx(q.Options, $"{xv}");
            q.Explanation = $"x вершины = -b/(2a) = -{b}/{2*a} = {xv}";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 5 ──────────────────────────────────────────────────────────────

    private List<Question> TrigRatios(int n)
    {
        var qs = new List<Question>();
        var table = new (int deg, string sin, string cos)[]
        {
            (0,"0","1"),(30,"1/2","√3/2"),(45,"√2/2","√2/2"),(60,"√3/2","1/2"),(90,"1","0")
        };
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            var row = table[_rnd.Next(table.Length)];
            bool askSin = _rnd.Next(2) == 0;
            if (askSin)
            {
                q.Text = $"sin {row.deg}° = ?";
                q.Options = Shuffle(row.sin, row.cos, "1/3", "2/3");
                q.CorrectIndex = FindIdx(q.Options, row.sin);
                q.Explanation = $"sin {row.deg}° = {row.sin}";
            }
            else
            {
                q.Text = $"cos {row.deg}° = ?";
                q.Options = Shuffle(row.cos, row.sin, "1/3", "2/3");
                q.CorrectIndex = FindIdx(q.Options, row.cos);
                q.Explanation = $"cos {row.deg}° = {row.cos}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> TrigFormulas(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            switch (i % 3)
            {
                case 0:
                    q.Text = "sin(2α) = ?";
                    q.Options = Shuffle("2sinα·cosα", "sin²α+cos²α", "cos²α-sin²α", "2cos²α");
                    q.CorrectIndex = FindIdx(q.Options, "2sinα·cosα");
                    q.Explanation = "Формула двойного угла: sin(2α)=2sinα·cosα";
                    break;
                case 1:
                    q.Text = "cos(2α) = ?";
                    q.Options = Shuffle("cos²α-sin²α", "2sinα·cosα", "sin²α+cos²α", "1-sinα");
                    q.CorrectIndex = FindIdx(q.Options, "cos²α-sin²α");
                    q.Explanation = "cos(2α)=cos²α-sin²α";
                    break;
                default:
                    q.Text = "sin²α + cos²α = ?";
                    q.Options = Shuffle("1", "0", "2", "sin(2α)");
                    q.CorrectIndex = FindIdx(q.Options, "1");
                    q.Explanation = "Основное тождество: sin²α+cos²α=1";
                    break;
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> TrigEquations(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Общее решение sinx=0?";
            q.Options = Shuffle("x=πn", "x=π/2+πn", "x=2πn", "x=π/4+πn");
            q.CorrectIndex = FindIdx(q.Options, "x=πn");
            q.Explanation = "sinx=0 при x=πn, n∈ℤ";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });

            if (qs.Count >= n) break;

            var q2 = new Question();
            q2.Text = "Общее решение cosx=1?";
            q2.Options = Shuffle("x=2πn", "x=πn", "x=π/2+2πn", "x=π+2πn");
            q2.CorrectIndex = FindIdx(q2.Options, "x=2πn");
            q2.Explanation = "cosx=1 при x=2πn, n∈ℤ";
            qs.Add(q2);
        }
        return qs.Take(n).ToList();
    }

    private List<Question> TrigInequalities(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "При каких x: sinx > 0?";
            q.Options = Shuffle("x∈(2πn, π+2πn)", "x∈(-π/2, π/2)", "x∈(0, 2π)", "x∈(π, 2π)");
            q.CorrectIndex = FindIdx(q.Options, "x∈(2πn, π+2πn)");
            q.Explanation = "sinx>0 на верхней полуокружности: (2πn, π+2πn)";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> PeriodicProcesses(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int omega = _rnd.Next(2, 5);
            double T = Math.Round(2 * Math.PI / omega, 2);
            q.Text = $"Период y=sin({omega}x)?";
            string correct = $"2π/{omega}";
            q.Options = Shuffle(correct, $"2π·{omega}", $"π/{omega}", $"π·{omega}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"T=2π/ω=2π/{omega}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> HarmonicAnalysis(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a1 = _rnd.Next(3, 5), a2 = _rnd.Next(4, 6);
            int amp2 = a1 * a1 + a2 * a2;
            q.Text = $"Амплитуда {a1}sinωt+{a2}cosωt?";
            string correct = $"√{amp2}";
            q.Options = Shuffle(correct, $"√{amp2 + 1}", $"{a1 + a2}", $"{a1 * a2}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"A=√(A₁²+A₂²)=√({a1}²+{a2}²)=√{amp2}";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 6 ──────────────────────────────────────────────────────────────

    private List<Question> SequenceLimits(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int k = _rnd.Next(1, 5);
            q.Text = $"lim(n→∞) {k}/n = ?";
            q.Options = Shuffle("0", $"{k}", "∞", "1");
            q.CorrectIndex = FindIdx(q.Options, "0");
            q.Explanation = $"{k}/n → 0 при n→∞";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> FunctionLimits(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "lim(x→0) sin(x)/x = ?";
            q.Options = Shuffle("1", "0", "∞", "π");
            q.CorrectIndex = FindIdx(q.Options, "1");
            q.Explanation = "Первый замечательный предел: lim(sinx/x)=1 при x→0";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> Derivative(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int pow = _rnd.Next(2, 6);
            q.Text = $"(x^{pow})' = ?";
            string correct = $"{pow}x^{pow - 1}";
            q.Options = Shuffle(correct, $"{pow - 1}x^{pow}", $"x^{pow + 1}", $"{pow}x^{pow}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"(xⁿ)'=n·xⁿ⁻¹: (x^{pow})'={pow}x^{pow-1}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> FuncAnalysis(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(1, 5);
            q.Text = $"f(x)={a}x² возрастает при?";
            q.Options = Shuffle("x>0", "x<0", "x∈ℝ", "x=0");
            q.CorrectIndex = FindIdx(q.Options, "x>0");
            q.Explanation = $"f'={2 * a}x>0 при x>0";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> IntegralMethods(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int pow = _rnd.Next(1, 5);
            q.Text = $"∫x^{pow}dx = ?";
            string correct = $"x^{pow + 1}/{pow + 1}+C";
            q.Options = Shuffle(correct, $"x^{pow}/{pow}+C", $"{pow}x^{pow - 1}+C", $"x^{pow + 2}/{pow + 1}+C");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"∫xⁿdx=xⁿ⁺¹/(n+1)+C";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Optimization(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(1, 4);
            int xMin = _rnd.Next(-3, 4);
            int yMin = a * xMin * xMin;
            q.Text = $"Минимум f(x)={a}x² достигается при?";
            q.Options = Shuffle("x=0", $"x={a}", $"x={a+1}", "x=1");
            q.CorrectIndex = FindIdx(q.Options, "x=0");
            q.Explanation = $"f'={2 * a}x=0 при x=0, f(0)=0 — минимум";
            qs.Add(q);
        }
        return qs;
    }

    // ── LEVEL 7 ──────────────────────────────────────────────────────────────

    private List<Question> Combinatorics(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int total = _rnd.Next(4, 8), k = _rnd.Next(2, total - 1);
            long num = Factorial(total) / (Factorial(k) * Factorial(total - k));
            q.Text = $"C({total},{k}) = ?";
            q.Options = Shuffle($"{num}", $"{num + 1}", $"{Factorial(total) / Factorial(total - k)}", $"{k * (total - k)}");
            q.CorrectIndex = FindIdx(q.Options, $"{num}");
            q.Explanation = $"C({total},{k})={total}!/({k}!·{total - k}!)={num}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> GraphTheory(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Граф является эйлеровым если:";
            q.Options = Shuffle("Все вершины чётной степени", "Есть путь через каждую вершину", "Граф полный", "Нет циклов");
            q.CorrectIndex = FindIdx(q.Options, "Все вершины чётной степени");
            q.Explanation = "Теорема Эйлера: граф имеет эйлеров цикл ↔ все вершины чётной степени";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> LogicSystems(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            bool a = _rnd.Next(2) == 1, b = _rnd.Next(2) == 1;
            bool andRes = a && b;
            q.Text = $"A={a}, B={b}. A AND B = ?";
            q.Options = Shuffle($"{andRes}", $"{!andRes}", $"{a || b}", $"{!(a || b)}");
            q.CorrectIndex = FindIdx(q.Options, $"{andRes}");
            q.Explanation = $"{a} AND {b} = {andRes}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Recurrence(int n)
    {
        var qs = new List<Question>();
        int f1 = 1, f2 = 1;
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = f1, b = f2, c = a + b;
            q.Text = $"Фибоначчи: ...{a}, {b}, ?";
            q.Options = Shuffle($"{c}", $"{c + 1}", $"{b * 2}", $"{a + 1}");
            q.CorrectIndex = FindIdx(q.Options, $"{c}");
            q.Explanation = $"Fₙ=Fₙ₋₁+Fₙ₋₂: {a}+{b}={c}";
            qs.Add(q);
            f1 = f2; f2 = c;
        }
        return qs;
    }

    private List<Question> BooleanAlgebra(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(0, 16), b = _rnd.Next(0, 16);
            int xorRes = a ^ b;
            q.Text = $"{a} XOR {b} = ?";
            q.Options = Shuffle($"{xorRes}", $"{a & b}", $"{a | b}", $"{xorRes + 1}");
            q.CorrectIndex = FindIdx(q.Options, $"{xorRes}");
            q.Explanation = $"{a} XOR {b} = {xorRes}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> AlgoThinking(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Сложность бинарного поиска?";
            q.Options = Shuffle("O(log n)", "O(n)", "O(n²)", "O(1)");
            q.CorrectIndex = FindIdx(q.Options, "O(log n)");
            q.Explanation = "Бинарный поиск делит пространство пополам: O(log n)";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    // ── LEVEL 8 ──────────────────────────────────────────────────────────────

    private List<Question> ClassicalProb(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int total = _rnd.Next(4, 10), favourable = _rnd.Next(1, total);
            double p = Math.Round((double)favourable / total, 2);
            q.Text = $"В урне {total} шаров, {favourable} красных. P(красный)?";
            q.Options = Shuffle($"{p:F2}", $"{Math.Round((double)(total-favourable)/total, 2):F2}", $"{favourable}", $"{Math.Round(p + 0.1, 2):F2}");
            q.CorrectIndex = FindIdx(q.Options, $"{p:F2}");
            q.Explanation = $"P={favourable}/{total}={p:F2}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> CombinatProb(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int total = _rnd.Next(5, 9), k = 2;
            long c = Factorial(total) / (Factorial(k) * Factorial(total - k));
            long cFav = Factorial(4) / (Factorial(k) * Factorial(2));
            double p = Math.Round((double)cFav / c, 2);
            q.Text = $"Из {total} карт выбирают 2. Из них 4 туза. P(оба туза)?";
            q.Options = Shuffle($"{p:F2}", $"{Math.Round(p*2,2):F2}", $"{Math.Round(4.0/total,2):F2}", $"{Math.Round(p+0.05,2):F2}");
            q.CorrectIndex = FindIdx(q.Options, $"{p:F2}");
            q.Explanation = $"C(4,2)/C({total},2)={cFav}/{c}≈{p:F2}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> RandomVariables(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Сумма всех вероятностей дискретной СВ равна?";
            q.Options = Shuffle("1", "0", "∞", "M(X)");
            q.CorrectIndex = FindIdx(q.Options, "1");
            q.Explanation = "Аксиома: Σpᵢ=1 для дискретной СВ";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> ExpectedValue(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int x1 = _rnd.Next(1, 5), x2 = _rnd.Next(5, 10);
            double p1 = 0.4, p2 = 0.6;
            double mx = Math.Round(x1 * p1 + x2 * p2, 2);
            q.Text = $"M(X)=? Если X={x1} c P=0.4, X={x2} с P=0.6";
            q.Options = Shuffle($"{mx}", $"{Math.Round(mx + 1, 2)}", $"{(x1 + x2) / 2.0:F2}", $"{Math.Round(x1 * 0.6 + x2 * 0.4, 2)}");
            q.CorrectIndex = FindIdx(q.Options, $"{mx}");
            q.Explanation = $"M(X)={x1}×0.4+{x2}×0.6={mx}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> StatisticalAnalysis(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int[] data = Enumerable.Range(0, 5).Select(_ => _rnd.Next(1, 20)).OrderBy(x => x).ToArray();
            double mean = Math.Round(data.Average(), 1);
            int median = data[2];
            q.Text = $"Среднее {string.Join(",", data)}?";
            q.Options = Shuffle($"{mean}", $"{median}", $"{data[0]}", $"{data[4]}");
            q.CorrectIndex = FindIdx(q.Options, $"{mean}");
            q.Explanation = $"Среднее = сумма/5 = {data.Sum()}/5 = {mean}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> DecisionTheory(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Критерий максимина выбирает стратегию с:";
            q.Options = Shuffle("Максимальным минимальным выигрышем", "Максимальным выигрышем", "Минимальным риском", "Средним выигрышем");
            q.CorrectIndex = FindIdx(q.Options, "Максимальным минимальным выигрышем");
            q.Explanation = "Максимин: max(min выигрышей по строке) — осторожная стратегия";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    // ── LEVEL 9 ──────────────────────────────────────────────────────────────

    private List<Question> Invariants(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Чётность суммы не меняется при:";
            q.Options = Shuffle("Замене числа на число той же чётности", "Прибавлении нечётного", "Умножении на 2", "Любой операции");
            q.CorrectIndex = FindIdx(q.Options, "Замене числа на число той же чётности");
            q.Explanation = "Инвариант чётности: заменяя элемент на элемент той же чётности, сумма не меняет чётность";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> DirichletPrinciple(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int pigeons = _rnd.Next(10, 20), holes = _rnd.Next(5, 9);
            int min = pigeons / holes + (pigeons % holes > 0 ? 1 : 0);
            q.Text = $"{pigeons} голубей, {holes} ящиков. Минимум голубей в одном ящике?";
            q.Options = Shuffle($"{min}", $"{min - 1}", $"{pigeons - holes}", $"{holes}");
            q.CorrectIndex = FindIdx(q.Options, $"{min}");
            q.Explanation = $"⌈{pigeons}/{holes}⌉={min}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> EstimationMethods(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 6), b = _rnd.Next(2, 6);
            double am = (a + b) / 2.0, gm = Math.Round(Math.Sqrt(a * b), 2);
            q.Text = $"Для a={a}, b={b}: AM≥GM означает?";
            string correct = $"{am}≥{gm}";
            q.Options = Shuffle(correct, $"{gm}≥{am}", $"{am}={gm}", $"{a}≥{b}");
            q.CorrectIndex = FindIdx(q.Options, correct);
            q.Explanation = $"AM=({a}+{b})/2={am}, GM=√({a}·{b})≈{gm}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> ConstructiveMethods(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Чтобы опровергнуть утверждение \"все числа чётные\", нужно:";
            q.Options = Shuffle("Привести контрпример (нечётное число)", "Доказать от противного", "Построить все чётные", "Использовать индукцию");
            q.CorrectIndex = FindIdx(q.Options, "Привести контрпример (нечётное число)");
            q.Explanation = "Один контрпример опровергает общее утверждение";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> ExtremalPrinciple(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Экстремальный принцип: рассматриваем...";
            q.Options = Shuffle("Наибольший/наименьший элемент", "Средний элемент", "Случайный элемент", "Последний элемент");
            q.CorrectIndex = FindIdx(q.Options, "Наибольший/наименьший элемент");
            q.Explanation = "Экстремальный принцип: берём максимальный или минимальный элемент и исследуем его свойства";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    private List<Question> NonstandardProblems(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            q.Text = "Метод двойного подсчёта используется для:";
            q.Options = Shuffle("Подсчёта одного объекта двумя способами", "Решения уравнений", "Нахождения суммы ряда", "Геометрических доказательств");
            q.CorrectIndex = FindIdx(q.Options, "Подсчёта одного объекта двумя способами");
            q.Explanation = "Двойной подсчёт: |A|=|B| если обе стороны считают одно и то же";
            qs.Add(new Question { Text = q.Text, Options = q.Options, CorrectIndex = q.CorrectIndex, Explanation = q.Explanation });
        }
        return qs;
    }

    // ── EXISTING METHODS (kept unchanged) ────────────────────────────────────

    private List<Question> Counting(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(10, 100), b = _rnd.Next(10, 100);
            if (i % 2 == 0)
            {
                int sum = a + b;
                q.Text         = $"{a} + {b} = ?";
                q.Options      = Shuffle($"{sum-1}", $"{sum}", $"{sum+1}", $"{sum+2}");
                q.CorrectIndex = FindIdx(q.Options, $"{sum}");
                q.Explanation  = $"{a} + {b} = {sum}";
            }
            else
            {
                if (a < b) (a, b) = (b, a);
                int diff = a - b;
                q.Text         = $"{a} - {b} = ?";
                q.Options      = Shuffle($"{diff}", $"{diff+1}", $"{diff-1}", $"{diff+2}");
                q.CorrectIndex = FindIdx(q.Options, $"{diff}");
                q.Explanation  = $"{a} - {b} = {diff}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Arithmetic(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(2, 20), b = _rnd.Next(2, 15), result;
            switch (i % 3)
            {
                case 0:
                    result = a * b;
                    q.Text = $"{a} × {b} = ?";
                    q.Options = Shuffle($"{result}", $"{result+1}", $"{result-1}", $"{result+b}");
                    q.CorrectIndex = FindIdx(q.Options, $"{result}");
                    q.Explanation = $"{a} × {b} = {result}";
                    break;
                case 1:
                    result = a * b + _rnd.Next(1, 10);
                    int add = result - a * b;
                    q.Text = $"{a} × {b} + {add} = ?";
                    q.Options = Shuffle($"{result+1}", $"{result}", $"{result-1}", $"{result+2}");
                    q.CorrectIndex = FindIdx(q.Options, $"{result}");
                    q.Explanation = $"{a} × {b} + {add} = {result}";
                    break;
                default:
                    int c = a * b;
                    result = c / a;
                    q.Text = $"{c} ÷ {a} = ?";
                    q.Options = Shuffle($"{result-1}", $"{result+1}", $"{result}", $"{result+2}");
                    q.CorrectIndex = FindIdx(q.Options, $"{result}");
                    q.Explanation = $"{c} ÷ {a} = {result}";
                    break;
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Powers(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            if (i % 2 == 0)
            {
                int b = _rnd.Next(2, 10), e = _rnd.Next(2, 4);
                int r = (int)Math.Pow(b, e);
                q.Text = $"{b}^{e} = ?";
                q.Options = Shuffle($"{r}", $"{r+1}", $"{r-1}", $"{b*e}");
                q.CorrectIndex = FindIdx(q.Options, $"{r}");
                q.Explanation = $"{b}^{e} = {r}";
            }
            else
            {
                int s = _rnd.Next(2, 10);
                int sq = s * s;
                q.Text = $"√{sq} = ?";
                q.Options = Shuffle($"{s-1}", $"{s+1}", $"{s}", $"{s*2}");
                q.CorrectIndex = FindIdx(q.Options, $"{s}");
                q.Explanation = $"√{sq} = {s}, т.к. {s}×{s} = {sq}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Fractions(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int num = _rnd.Next(1, 8), den = _rnd.Next(2, 10);
            if (i % 2 == 0)
            {
                double val = Math.Round((double)num / den, 2);
                q.Text = $"Запишите {num}/{den} в виде десятичной дроби";
                var shuf = ShuffleDbl(new double[]{val, val+0.01, val-0.01, val+0.02});
                q.CorrectIndex = FindIdxDbl(shuf, val);
                q.Options = shuf.Select(x => $"{x:F2}").ToArray();
                q.Explanation = $"{num}/{den} = {val:F2}";
            }
            else
            {
                int n2 = _rnd.Next(1, 6), d2 = _rnd.Next(2, 6);
                double sum = Math.Round((double)num / den + (double)n2 / d2, 2);
                q.Text = $"{num}/{den} + {n2}/{d2} = ? (десятичной дробью)";
                var shuf = ShuffleDbl(new double[]{sum, sum+0.1, sum-0.1, sum+0.2});
                q.CorrectIndex = FindIdxDbl(shuf, sum);
                q.Options = shuf.Select(x => $"{x:F2}").ToArray();
                q.Explanation = $"{num}/{den} + {n2}/{d2} = {sum:F2}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Percentages(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int total = _rnd.Next(5, 30) * 10, pct = _rnd.Next(10, 51);
            int result = total * pct / 100;
            q.Text = $"Найдите {pct}% от {total}";
            q.Options = Shuffle($"{result}", $"{result+1}", $"{result-1}", $"{result*2}");
            q.CorrectIndex = FindIdx(q.Options, $"{result}");
            q.Explanation = $"{pct}% от {total} = {total}×{pct}/100 = {result}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> LinearEq(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q  = new Question();
            int a, b, c, diff, x;
            do
            {
                a = _rnd.Next(1, 10);
                b = _rnd.Next(-15, 16);
                c = _rnd.Next(-10, 21);
                diff = c - b;
            } while (diff % a != 0);

            x = diff / a;
            string sign = b >= 0 ? "+" : "-";
            q.Text = $"{a}x {sign} {Math.Abs(b)} = {c}";
            q.Options = Shuffle($"{x}", $"{x+1}", $"{x-1}", $"{-x}");
            q.CorrectIndex = FindIdx(q.Options, $"{x}");
            q.Explanation = $"{a}x = {c} - ({b}) = {diff}, x = {diff}/{a} = {x}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Inequalities(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            int a = _rnd.Next(1, 5), b = _rnd.Next(-10, 11), c = _rnd.Next(-10, 11);
            int rhs = c - b;
            if (rhs % a != 0) { a = 1; }
            int x    = rhs / a;
            string sign = b >= 0 ? "+" : "-";
            q.Text = $"{a}x {sign} {Math.Abs(b)} > {c}";
            q.Options = Shuffle($"x > {x}", $"x < {x}", $"x = {x}", $"x ≥ {x}");
            q.CorrectIndex = FindIdx(q.Options, $"x > {x}");
            q.Explanation = $"{a}x > {rhs}, x > {x}";
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Progression(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q  = new Question();
            int a1 = _rnd.Next(1, 10), d = _rnd.Next(1, 6), ni = _rnd.Next(4, 8);
            int an = a1 + (ni - 1) * d;
            int sn = ni * (a1 + an) / 2;
            if (i % 2 == 0)
            {
                q.Text = $"Прогрессия: a₁={a1}, d={d}. Найдите a{ni}";
                q.Options = Shuffle($"{an}", $"{an+d}", $"{an-d}", $"{a1+ni*d}");
                q.CorrectIndex = FindIdx(q.Options, $"{an}");
                q.Explanation = $"a{ni} = {a1} + ({ni}-1)×{d} = {an}";
            }
            else
            {
                q.Text = $"Прогрессия: a₁={a1}, a{ni}={an}. Найдите S{ni}";
                q.Options = Shuffle($"{sn-1}", $"{sn+1}", $"{sn}", $"{sn*2}");
                q.CorrectIndex = FindIdx(q.Options, $"{sn}");
                q.Explanation = $"S{ni} = {ni}×({a1}+{an})/2 = {sn}";
            }
            qs.Add(q);
        }
        return qs;
    }

    private List<Question> Areas(int n)
    {
        var qs = new List<Question>();
        for (int i = 0; i < n; i++)
        {
            var q = new Question();
            switch (i % 3)
            {
                case 0:
                {
                    int w = _rnd.Next(3, 12), h = _rnd.Next(3, 12);
                    q.Text = $"Площадь прямоугольника {w}×{h}";
                    q.Options = Shuffle($"{w*h}", $"{w+h}", $"{(w+h)*2}", $"{w*h+1}");
                    q.CorrectIndex = FindIdx(q.Options, $"{w*h}");
                    q.Explanation = $"S = {w}×{h} = {w*h}";
                    break;
                }
                case 1:
                {
                    int ba = _rnd.Next(4, 14), h = _rnd.Next(3, 12);
                    double s = (double)ba * h / 2;
                    q.Text = $"Площадь треугольника: основание {ba}, высота {h}";
                    q.Options = Shuffle($"{s:F1}", $"{ba*h}", $"{ba+h}", $"{s*2:F1}");
                    q.CorrectIndex = FindIdx(q.Options, $"{s:F1}");
                    q.Explanation = $"S = ½×{ba}×{h} = {s:F1}";
                    break;
                }
                default:
                {
                    int r = _rnd.Next(2, 7);
                    double area = Math.Round(Math.PI * r * r, 1);
                    var opts = ShuffleDbl(new double[]{area, Math.Round(2*Math.PI*r,1), Math.Round(Math.PI*r,1), Math.Round(area/2,1)});
                    q.Text = $"Площадь круга: радиус {r} (π≈3.14)";
                    q.CorrectIndex = FindIdxDbl(opts, area);
                    q.Options = opts.Select(x => $"{x:F1}").ToArray();
                    q.Explanation = $"S = 3.14×{r}² = {area:F1}";
                    break;
                }
            }
            qs.Add(q);
        }
        return qs;
    }

    // ── THEORY BANKS (levels 10+) ─────────────────────────────────────────────

    private static Dictionary<string, string[][]> BuildBanks()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theory_banks.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, string[][]>>(json)!;
    }
}
