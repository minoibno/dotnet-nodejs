using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Minoibno.Dotnet.NodeJs.Publisher;

public sealed class PublishingConsoleFormatter : ConsoleFormatter {
    public const string FormatterName = nameof(PublishingConsoleFormatter);

    /// <inheritdoc />
    public PublishingConsoleFormatter() : base(FormatterName) { }

    /// <inheritdoc />
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter) {
        DateTimeOffset now = DateTimeOffset.Now.ToUniversalTime();

        string formattedMessage = logEntry.Formatter(logEntry.State, logEntry.Exception);

        textWriter.WriteLine(
            "{0:yyyy-MM-dd HH:mm:ss.fff K} [{1,3:#}] {2}{3}{4} {5} - {6}{7}",
            now,
            Environment.CurrentManagedThreadId,
            logEntry.LogLevel switch {
                LogLevel.Trace => "\x1B[39m\x1B[22m",
                LogLevel.Debug => "\x1B[1m\x1B[34m",
                LogLevel.Information => "\x1B[1m\x1B[32m",
                LogLevel.Warning => "\x1B[1m\x1B[33m",
                LogLevel.Error => "\x1B[1m\x1B[31m",
                LogLevel.Critical => "\x1B[1m\x1B[35m",
                _ => throw new InvalidOperationException($"LogLevel<{logEntry.LogLevel}> is not supported")
            },
            logEntry.LogLevel,
            "\x1B[39m\x1B[22m",
            logEntry.Category,
            formattedMessage,
            logEntry.Exception != default ? textWriter.NewLine + logEntry.Exception : string.Empty
        );
    }
}
