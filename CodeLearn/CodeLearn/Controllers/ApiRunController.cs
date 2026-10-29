using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CodeLearn.Controllers
{
    [Authorize]
    [Route("api/run")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("run")]
    public class ApiRunController : Controller
    {
        private static readonly SemaphoreSlim _csharpSem = new(3, 3);

        public record RunRequest(string Code, string Language);
        public record RunResult(bool Success, string Output, string Error, long Ms);

        public record TestCaseInput(string Name, object[] Args);
        public record TestCaseResult(string Name, bool Passed, object Expected, object Actual, string Error);
        public record TestRunRequest(string UserCode, string FunctionName, TestCaseInput[] TestCases, string Language);
        public record TestRunResult(bool CompileSuccess, bool AllPassed, TestCaseResult[] Results, string Error, long Ms);

        private static readonly string[] _blockedPatterns =
        [
            // сеть
            "System.Net", "HttpClient", "WebClient", "TcpClient", "WebRequest", "Dns.", ".Sockets",
            // запуск процессов
            "Process.Start", "new Process", "ProcessStartInfo",
            // файловая система (потоки)
            "StreamReader", "StreamWriter", "FileStream",
            // окружение / выход
            "Environment.Exit", "GetEnvironmentVariable", "Environment.GetEnvironment",
            // загрузка/исполнение чужого кода, нативщина, реестр
            "AppDomain", "Assembly.Load", "LoadFrom", "LoadFile", "Activator.CreateInstance",
            "DllImport", "Marshal.", "GCHandle", "Registry", "DynamicMethod",
            // stdin
            "Console.OpenStandardInput", "Console.In",
            // рефлексия по строковому имени типа — прямой и через Assembly
            "Type.GetType", "Assembly.GetType",
            // вызов методов через рефлексию (обход блокировок через MethodInfo.Invoke)
            ".GetMethod(", ".GetMethods(", ".GetField(", ".GetFields(",
            ".GetProperty(", ".GetProperties(", ".GetConstructor(",
            "MethodInfo", "FieldInfo", "PropertyInfo", "ConstructorInfo",
            // опасные базовые типы рефлексии
            "RuntimeTypeHandle", "RuntimeMethodHandle",
        ];

        // File. / Directory. — с границей слова, чтобы не ловить someFile.Length
        private static readonly Regex _blockedFile =
            new(@"\b(File|Directory)\s*\.", RegexOptions.IgnoreCase);

        private static string? CheckCodeSafety(string code)
        {
            // Убираем строковые литералы и комментарии перед проверкой
            var stripped = Regex.Replace(code,
                @"(//[^\n]*|/\*[\s\S]*?\*/|""(?:[^""\\]|\\.)*""|'(?:[^'\\]|\\.)*')", " ");

            foreach (var pattern in _blockedPatterns)
                if (stripped.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return $"Использование '{pattern}' запрещено в учебном окружении.";

            if (_blockedFile.IsMatch(stripped))
                return "Доступ к файловой системе (File/Directory) запрещён в учебном окружении.";

            // Циклы НЕ запрещаем — от зависаний защищает wall-clock таймаут.
            return null;
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Execute([FromBody] RunRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new RunResult(false, "", "Код не передан", 0));

            if (req.Code.Length > 20_000)
                return BadRequest(new RunResult(false, "", "Код слишком длинный (макс. 20 000 символов)", 0));

            // Проверка безопасности для C# (Python запускается отдельным процессом — безопасен)
            if (req.Language?.ToLowerInvariant() is "csharp" or "cs")
            {
                var safetyError = CheckCodeSafety(req.Code);
                if (safetyError != null)
                    return Ok(new RunResult(false, "", safetyError, 0));
            }

            var sw = Stopwatch.StartNew();

            var result = (req.Language?.ToLowerInvariant()) switch
            {
                "csharp" or "cs" => await RunCSharp(req.Code),
                "python" or "py" => await RunPython(req.Code),
                _ => new RunResult(false, "", $"Неизвестный язык: {req.Language}", 0)
            };

            sw.Stop();
            return Ok(result with { Ms = sw.ElapsedMilliseconds });
        }

        // ── Авто-вызов Main() для class-based кода ──────────────────────────────
        // Roslyn scripting сам не вызывает static Main — нужно добавить вызов явно.
        // Main() обычно приватный, поэтому вызываем через рефлексию, а не напрямую.
        private static string InjectMainCall(string code)
        {
            // Ищем первый класс, который содержит static * Main(
            var m = Regex.Match(code,
                @"class\s+(\w+)",
                RegexOptions.Multiline);
            if (!m.Success) return code;

            bool hasMain = Regex.IsMatch(code,
                @"static\s+(?:void|int|Task(?:<int>)?)\s+Main\s*\(",
                RegexOptions.Multiline);
            if (!hasMain) return code;

            var className = m.Groups[1].Value;

            // Не добавляем, если вызов уже есть в коде
            if (Regex.IsMatch(code, $@"\b{className}\.Main\s*\("))
                return code;

            // Рефлексия обходит private/protected — не падает с "уровень защиты"
            // Если у Main нет параметров — вызываем без аргументов,
            // иначе передаём глобальную переменную args (из ScriptGlobals).
            return code + $@"
var __main = typeof({className})
    .GetMethod(""Main"",
        System.Reflection.BindingFlags.Static |
        System.Reflection.BindingFlags.Public  |
        System.Reflection.BindingFlags.NonPublic);
if (__main?.GetParameters().Length == 0)
    __main.Invoke(null, null);
else
    __main?.Invoke(null, new object[] {{ args ?? System.Array.Empty<string>() }});
";
        }

        // ── C# через Roslyn Scripting API ────────────────────────────────────────
        private static async Task<RunResult> RunCSharp(string code)
        {
            await _csharpSem.WaitAsync();
            try
            {
                code = InjectMainCall(code);
                var (ok, output, error) = await RunInChildProcess(code);
                return new RunResult(ok, output, error, 0);
            }
            finally
            {
                _csharpSem.Release();
            }
        }

        // ── Запуск C# в ИЗОЛИРОВАННОМ дочернем процессе (CodeLearn --run-code) ──────
        // Процесс можно реально убить по таймауту (бесконечный цикл больше не висит),
        // у него нет в памяти БД/секретов, рабочая папка — temp.
        private static async Task<(bool ok, string output, string error)> RunInChildProcess(string code)
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"clrun_{Guid.NewGuid():N}.cs");
            try
            {
                await System.IO.File.WriteAllTextAsync(tmp, code);

                var psi = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    WorkingDirectory       = Path.GetTempPath(),   // не даём относительный доступ к папке приложения
                };

                // Если хост — dotnet, запускаем «dotnet <app.dll> --run-code», иначе сам exe
                var procPath = Environment.ProcessPath ?? "dotnet";
                var fileName = Path.GetFileNameWithoutExtension(procPath).ToLowerInvariant();
                if (fileName == "dotnet")
                {
                    psi.FileName = procPath;
                    psi.ArgumentList.Add(System.Reflection.Assembly.GetEntryAssembly()!.Location);
                }
                else psi.FileName = procPath;
                psi.ArgumentList.Add("--run-code");
                psi.ArgumentList.Add(tmp);

                using var proc = Process.Start(psi);
                if (proc == null) return (false, "", "Не удалось запустить раннер");

                var outTask = proc.StandardOutput.ReadToEndAsync();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try { await proc.WaitForExitAsync(cts.Token); }
                catch (OperationCanceledException)
                {
                    try { proc.Kill(true); } catch { }
                    return (false, "", "⏱ Превышен лимит времени (возможно бесконечный цикл)");
                }

                var stdout = await outTask;
                var line = stdout.Split('\n')
                    .FirstOrDefault(l => l.Contains(CodeLearn.Helpers.CodeRunner.Marker));
                if (line == null) return (false, "", "Раннер не вернул результат");

                var json = line[(line.IndexOf(CodeLearn.Helpers.CodeRunner.Marker, StringComparison.Ordinal)
                                 + CodeLearn.Helpers.CodeRunner.Marker.Length)..].Trim();
                var env = JsonSerializer.Deserialize<CodeLearn.Helpers.CodeRunner.Envelope>(json);
                return env == null ? (false, "", "Ошибка разбора результата") : (env.ok, env.output, env.error);
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
            finally
            {
                try { System.IO.File.Delete(tmp); } catch { }
            }
        }

        // ── Python через subprocess ───────────────────────────────────────────────
        private static async Task<RunResult> RunPython(string code)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), $"cl_{Guid.NewGuid():N}.py");
            try
            {
                await System.IO.File.WriteAllTextAsync(tmpFile, code);

                // Windows: python, Linux/Mac: python3
                var exe = OperatingSystem.IsWindows() ? "python" : "python3";

                using var cts  = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName               = exe,
                        Arguments              = $"\"{tmpFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true,
                        UseShellExecute        = false,
                        CreateNoWindow         = true
                    }
                };

                proc.Start();

                var outTask = proc.StandardOutput.ReadToEndAsync();
                var errTask = proc.StandardError.ReadToEndAsync();

                try { await proc.WaitForExitAsync(cts.Token); }
                catch (OperationCanceledException)
                {
                    try { proc.Kill(true); } catch { }
                    return new RunResult(false, "", "⏱ Превышен лимит времени (5 сек)", 0);
                }

                var stdout = TrimOutput(await outTask);
                var stderr = (await errTask).Trim();

                if (proc.ExitCode != 0 && !string.IsNullOrEmpty(stderr))
                    return new RunResult(false, stdout, stderr, 0);

                return new RunResult(true, stdout, "", 0);
            }
            catch (Exception ex)
            {
                return new RunResult(false, "", $"Python недоступен: {ex.Message}", 0);
            }
            finally
            {
                try { System.IO.File.Delete(tmpFile); } catch { }
            }
        }

        [HttpPost("test"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExecuteTest([FromBody] TestRunRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserCode))
                return BadRequest(new TestRunResult(false, false, Array.Empty<TestCaseResult>(), "Код не передан", 0));

            if (req.TestCases == null || req.TestCases.Length == 0)
                return BadRequest(new TestRunResult(false, false, Array.Empty<TestCaseResult>(), "Нет тест-кейсов", 0));

            // Та же проверка безопасности что и для Execute
            if (req.Language?.ToLowerInvariant() is "csharp" or "cs")
            {
                var safetyError = CheckCodeSafety(req.UserCode);
                if (safetyError != null)
                    return Ok(new TestRunResult(false, false, Array.Empty<TestCaseResult>(), safetyError, 0));
            }

            var sw = Stopwatch.StartNew();

            var result = (req.Language?.ToLowerInvariant()) switch
            {
                "csharp" or "cs" => await RunCSharpTests(req.UserCode, req.FunctionName, req.TestCases),
                _ => new TestRunResult(false, false, Array.Empty<TestCaseResult>(), $"Неизвестный язык: {req.Language}", 0)
            };

            sw.Stop();
            return Ok(result with { Ms = sw.ElapsedMilliseconds });
        }

        private static async Task<TestRunResult> RunCSharpTests(string userCode, string functionName, TestCaseInput[] testCases)
        {
            await _csharpSem.WaitAsync();
            try
            {
                var results = new List<TestCaseResult>();
                var allPassed = true;

                foreach (var tc in testCases)
                {
                    try
                    {
                        var argsLiteral = FormatArgsLiteral(tc.Args);
                        var fullCode = $@"
{userCode}

// Test runner
var __result = {functionName}({argsLiteral});
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(__result));
";

                        var output = await RunAndGetOutput(fullCode);
                        if (output.error != null)
                        {
                            results.Add(new TestCaseResult(tc.Name, false, null, null, output.error));
                            allPassed = false;
                            continue;
                        }

                        object actual;
                        try
                        {
                            // Берём только последнюю непустую строку — там JSON от тест-раннера.
                            // Всё что выше — Console.WriteLine из пользовательского кода (вывод отладки).
                            var lastLine = output.output?
                                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .LastOrDefault() ?? "";
                            actual = JsonSerializer.Deserialize<object>(lastLine) ?? lastLine;
                        }
                        catch
                        {
                            actual = output.output ?? "";
                        }

                        var passed = ObjectsEqual(actual, tc.Args.LastOrDefault());

                        if (!passed) allPassed = false;
                        results.Add(new TestCaseResult(tc.Name, passed, tc.Args.LastOrDefault(), actual, null));
                    }
                    catch (Exception ex)
                    {
                        results.Add(new TestCaseResult(tc.Name, false, null, null, ex.Message));
                        allPassed = false;
                    }
                }

                return new TestRunResult(true, allPassed, results.ToArray(), null, 0);
            }
            finally
            {
                _csharpSem.Release();
            }
        }

        private static async Task<(string? output, string? error)> RunAndGetOutput(string code)
        {
            // Тесты тоже гоняем в изолированном дочернем процессе
            var (ok, output, error) = await RunInChildProcess(code);
            return ok ? (output, null) : (null, error);
        }

        private static string FormatArgsLiteral(object?[] args)
        {
            var literals = new List<string>();
            for (int i = 0; i < args.Length - 1; i++)
            {
                var arg = args[i];
                literals.Add(FormatValueLiteral(arg));
            }
            return string.Join(", ", literals);
        }

        private static string FormatValueLiteral(object? val)
        {
            if (val == null) return "null";
            if (val is string s) return $"\"{s.Replace("\"", "\\\"")}\"";
            if (val is bool b) return b ? "true" : "false";
            if (val is char c) return $"'{c}'";
            if (val is int or long or short or sbyte) return val.ToString()!;
            if (val is float f) return $"{f}f";
            if (val is double d) return d.ToString()!;
            if (val is decimal m) return $"{m}m";
            return val.ToString()!;
        }

        private static bool ObjectsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            if (a is JsonElement ja && b is JsonElement jb)
                return JsonEquals(ja, jb);

            if (a is JsonElement je)
                return JsonElementToObject(je)?.ToString() == b.ToString();

            if (b is JsonElement je2)
                return a.ToString() == JsonElementToObject(je2)?.ToString();

            return a.ToString() == b.ToString();
        }

        private static bool JsonEquals(JsonElement a, JsonElement b)
        {
            if (a.ValueKind != b.ValueKind) return false;

            return a.ValueKind switch
            {
                JsonValueKind.Number => a.GetDecimal() == b.GetDecimal(),
                JsonValueKind.String => a.GetString() == b.GetString(),
                JsonValueKind.True or JsonValueKind.False => a.GetBoolean() == b.GetBoolean(),
                JsonValueKind.Null => true,
                _ => a.ToString() == b.ToString()
            };
        }

        private static object? JsonElementToObject(JsonElement e)
        {
            return e.ValueKind switch
            {
                JsonValueKind.Number => e.GetDecimal(),
                JsonValueKind.String => e.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => e.ToString()
            };
        }

        private static string TrimOutput(string s)
        {
            s = s.TrimEnd();
            return s.Length > 10_000 ? s[..10_000] + "\n… [вывод обрезан]" : s;
        }
    }
}
