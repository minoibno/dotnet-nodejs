using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Common;
using LogLevel = NuGet.Common.LogLevel;

namespace Minoibno.Dotnet.NodeJs.Publisher;

[SuppressMessage("Usage", "CA2254:Template should be a static expression")]
public sealed class NugetLogger : NuGet.Common.ILogger {
    private readonly ILogger<NugetLogger> log;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public NugetLogger(ILogger<NugetLogger> log) {
        this.log = log;
        jsonSerializerOptions = new();
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc />
    public void LogDebug(string data) => log.LogDebug(data);

    /// <inheritdoc />
    public void LogVerbose(string data) => log.LogDebug(data);

    /// <inheritdoc />
    public void LogInformation(string data) => log.LogInformation(data);

    /// <inheritdoc />
    public void LogMinimal(string data) => log.LogInformation(data);

    /// <inheritdoc />
    public void LogWarning(string data) => log.LogWarning(data);

    /// <inheritdoc />
    public void LogError(string data) => log.LogError(data);

    /// <inheritdoc />
    public void LogInformationSummary(string data) => log.LogInformation(data);

    /// <inheritdoc />
    public void Log(LogLevel level, string data) => log.Log(
        level switch {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Minimal => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        },
        data
    );

    /// <inheritdoc />
    public Task LogAsync(LogLevel level, string data) {
        Log(level, data);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Log(ILogMessage message) => log.Log(
        message.Level switch {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Minimal => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(message), message.Level, null)
        },
        "NuGet Error/Warning\n{Message}",
        JsonSerializer.Serialize(message, jsonSerializerOptions)
    );

    /// <inheritdoc />
    public Task LogAsync(ILogMessage message) {
        Log(message);

        return Task.CompletedTask;
    }
}
