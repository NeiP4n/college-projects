using System.Text;

namespace CodeLearn.Helpers;

/// <summary>
/// Глобальный прокси для Console.Out / Console.Error.
/// Устанавливается ОДИН РАЗ при старте приложения.
///
/// Во время выполнения пользовательского скрипта вызываем BeginCapture(writer) —
/// все Console.Write* в этом async-контексте попадают в writer.
/// Логгер, работающий в другом async-контексте, продолжает писать в realWriter.
/// </summary>
public sealed class AsyncLocalWriter : TextWriter
{
    // Хранит целевой writer для текущего async-контекста (null = пиши в realWriter)
    private static readonly AsyncLocal<TextWriter?> _capture = new();

    private readonly TextWriter _real;

    public AsyncLocalWriter(TextWriter real) => _real = real;

    /// <summary>
    /// Перенаправляет Console.Write* в <paramref name="target"/> для текущего
    /// async-контекста. Dispose снимает перенаправление.
    /// </summary>
    public static IDisposable BeginCapture(TextWriter target)
    {
        _capture.Value = target;
        return new Scope();
    }

    private sealed class Scope : IDisposable
    {
        public void Dispose() => _capture.Value = null;
    }

    // ── TextWriter overrides ──────────────────────────────────────────────────

    public override Encoding Encoding => (_capture.Value ?? _real).Encoding;

    public override void Write(char value)              => (_capture.Value ?? _real).Write(value);
    public override void Write(string? value)           => (_capture.Value ?? _real).Write(value);
    public override void WriteLine(string? value)       => (_capture.Value ?? _real).WriteLine(value);
    public override void WriteLine()                    => (_capture.Value ?? _real).WriteLine();

    public override Task WriteAsync(char value)         => (_capture.Value ?? _real).WriteAsync(value);
    public override Task WriteAsync(string? value)      => (_capture.Value ?? _real).WriteAsync(value);
    public override Task WriteLineAsync(string? value)  => (_capture.Value ?? _real).WriteLineAsync(value);
    public override Task WriteLineAsync()               => (_capture.Value ?? _real).WriteLineAsync();

    public override void Flush()                        => (_capture.Value ?? _real).Flush();
    public override Task FlushAsync()                   => (_capture.Value ?? _real).FlushAsync();
}
