using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WordAIAgent.Services
{
    public class RoslynExecutor
    {
        private const string CODE_TEMPLATE = @"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using WordAIAgent.Services;

namespace AIGenerated
{{
    public static class AITask
    {{
        public static Word.Application WordApp {{ get; set; }}
        public static Word.Document ActiveDoc {{ get; set; }}
        public static WordService WordSvc {{ get; set; }}
        public static Action<string> Log {{ get; set; }}

        public static object RunMacro(string macroName, params object[] args) => WordSvc.RunMacro(macroName, args);
        public static object RunWordBasic(string commandName, params object[] args) => WordSvc.ExecuteWordBasic(commandName, args);
        public static void ExecuteMso(string controlId) => WordSvc.ExecuteMso(controlId);
        public static void ReplaceSelection(string text) => WordSvc.ReplaceSelection(text);
        public static void InsertText(string text) => WordSvc.InsertText(text);

        public static void Execute()
        {{
            {0}
        }}
    }}
}}";

        private readonly List<string> _assemblyPaths;

        public RoslynExecutor()
        {
            _assemblyPaths = new List<string>
            {
                typeof(object).Assembly.Location,
                typeof(Enumerable).Assembly.Location,
                typeof(File).Assembly.Location,
                typeof(StringBuilder).Assembly.Location,
                typeof(Console).Assembly.Location,
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location,
                Assembly.GetExecutingAssembly().Location
            };

            try
            {
                var wordInteropPath = typeof(Microsoft.Office.Interop.Word.Application).Assembly.Location;
                if (!string.IsNullOrEmpty(wordInteropPath))
                    _assemblyPaths.Add(wordInteropPath);
            }
            catch
            {
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!asm.IsDynamic &&
                        !string.IsNullOrEmpty(asm.Location) &&
                        !_assemblyPaths.Contains(asm.Location))
                    {
                        _assemblyPaths.Add(asm.Location);
                    }
                }
                catch
                {
                }
            }
        }

        public class ExecutionResult
        {
            public bool Success { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
            public string GeneratedCode { get; set; }
            public List<string> CompileErrors { get; set; } = new List<string>();
        }

        public static string ExtractCode(string aiResponse)
        {
            var patterns = new[]
            {
                @"```csharp\s*([\s\S]*?)```",
                @"```cs\s*([\s\S]*?)```",
                @"```\s*([\s\S]*?)```"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(aiResponse ?? string.Empty, pattern);
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }

            return (aiResponse ?? string.Empty).Trim();
        }

        public ExecutionResult Execute(string userCode)
        {
            var result = new ExecutionResult { GeneratedCode = userCode };
            var logBuilder = new StringBuilder();

            try
            {
                ThisAddIn.WordService.EnsureDocumentReady();

                var fullCode = string.Format(CODE_TEMPLATE, userCode);
                var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);
                var references = _assemblyPaths
                    .Where(File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(path => MetadataReference.CreateFromFile(path))
                    .Cast<MetadataReference>()
                    .ToList();

                var compilation = CSharpCompilation.Create(
                    $"AICode_{DateTime.Now.Ticks}",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        .WithOptimizationLevel(OptimizationLevel.Release)
                        .WithNullableContextOptions(NullableContextOptions.Disable));

                using (var ms = new MemoryStream())
                {
                    var emitResult = compilation.Emit(ms);
                    if (!emitResult.Success)
                    {
                        foreach (var diag in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        {
                            var line = diag.Location.GetLineSpan().StartLinePosition.Line + 1;
                            result.CompileErrors.Add($"Строка {line}: {diag.GetMessage()}");
                        }

                        result.Success = false;
                        result.Error = "Ошибки компиляции:\n" + string.Join("\n", result.CompileErrors);
                        return result;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());
                    var type = assembly.GetType("AIGenerated.AITask");
                    if (type == null)
                        throw new InvalidOperationException("Не найден тип AIGenerated.AITask.");

                    type.GetProperty("WordApp").SetValue(null, Globals.ThisAddIn.Application);
                    type.GetProperty("ActiveDoc").SetValue(null, Globals.ThisAddIn.Application.ActiveDocument);
                    type.GetProperty("WordSvc").SetValue(null, ThisAddIn.WordService);
                    type.GetProperty("Log").SetValue(null, new Action<string>(msg =>
                    {
                        if (!string.IsNullOrWhiteSpace(msg))
                            logBuilder.AppendLine(msg);
                    }));

                    type.GetMethod("Execute").Invoke(null, null);
                }

                result.Success = true;
                result.Output = logBuilder.Length > 0
                    ? logBuilder.ToString().Trim()
                    : "Код выполнен успешно.";
            }
            catch (TargetInvocationException tie)
            {
                result.Success = false;
                result.Error = $"Ошибка выполнения: {tie.InnerException?.Message ?? tie.Message}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Ошибка: {ex.Message}";
            }

            return result;
        }

        public static bool IsAvailable()
        {
            try
            {
                var unused = typeof(CSharpCompilation);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
