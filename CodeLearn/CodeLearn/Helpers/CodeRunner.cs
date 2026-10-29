using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CodeLearn.Helpers
{
    // Глобальные переменные, доступные в пользовательском скрипте.
    // Студент может обращаться к args как к обычной переменной:
    //   if (args.Length > 0) Console.WriteLine($"Hello, {args[0]}!");
    //   else                 Console.WriteLine("Hello, World!");
    public sealed class ScriptGlobals
    {
        public string[] args { get; init; } = Array.Empty<string>();
    }

    // Изолированный запуск пользовательского C#.
    // Вызывается в ДОЧЕРНЕМ процессе (CodeLearn.exe --run-code <file> [args...]):
    // веб-хост НЕ стартует, БД/секреты не загружаются, процесс убиваем по таймауту.
    public static class CodeRunner
    {
        public const string Marker = "###CLRUN###";

        public record Envelope(bool ok, string output, string error);

        public static async Task RunFile(string path, string[]? scriptArgs = null)
        {
            var realOut = Console.Out;
            string code;
            try { code = await File.ReadAllTextAsync(path); }
            catch { Emit(realOut, new Envelope(false, "", "Не удалось прочитать код")); return; }

            var captured = new StringWriter();
            Console.SetOut(captured);
            try
            {
                var globals = new ScriptGlobals
                {
                    args = scriptArgs ?? Array.Empty<string>()
                };
                var options = BuildOptions();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                await CSharpScript.RunAsync(code, options, globals, typeof(ScriptGlobals), cts.Token);
                Console.SetOut(realOut);
                Emit(realOut, new Envelope(true, Trim(captured.ToString()), ""));
            }
            catch (CompilationErrorException ex)
            {
                Console.SetOut(realOut);
                var errors = string.Join("\n", ex.Diagnostics
                    .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                    .Select(d => $"Строка {d.Location.GetLineSpan().StartLinePosition.Line + 1}: {d.GetMessage()}"));
                Emit(realOut, new Envelope(false, "", errors));
            }
            catch (OperationCanceledException)
            {
                Console.SetOut(realOut);
                Emit(realOut, new Envelope(false, "", "⏱ Превышен лимит времени"));
            }
            catch (Exception ex)
            {
                Console.SetOut(realOut);
                Emit(realOut, new Envelope(false, Trim(captured.ToString()), ex.Message));
            }
        }

        private static void Emit(TextWriter realOut, Envelope e)
            => realOut.WriteLine(Marker + JsonSerializer.Serialize(e));

        private static string Trim(string s)
        {
            s = s.TrimEnd();
            return s.Length > 10_000 ? s[..10_000] + "\n… [вывод обрезан]" : s;
        }

        public static ScriptOptions BuildOptions() =>
            ScriptOptions.Default
                .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest)
                .WithImports(
                    "System", "System.Linq", "System.Collections", "System.Collections.Generic",
                    "System.Text", "System.Text.RegularExpressions", "System.Text.Json",
                    "System.Threading", "System.Threading.Tasks",
                    "System.Globalization", "System.Diagnostics")
                .AddReferences(
                    typeof(object).Assembly,
                    typeof(Enumerable).Assembly,
                    typeof(StringBuilder).Assembly,
                    typeof(System.Text.RegularExpressions.Regex).Assembly,
                    typeof(System.Diagnostics.Stopwatch).Assembly,
                    typeof(System.Threading.Tasks.Task).Assembly,
                    typeof(System.Threading.Thread).Assembly,
                    typeof(Console).Assembly,
                    typeof(System.Collections.Generic.List<>).Assembly,
                    typeof(JsonSerializer).Assembly);
    }
}
