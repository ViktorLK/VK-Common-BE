using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Testing;

/// <summary>
/// In-memory test logger implementation for capturing and inspecting log records in unit tests.
/// </summary>
/// <typeparam name="T">The category type.</typeparam>
public class VKTestLogger<T> : ILogger<T>
{
    private readonly ConcurrentBag<VKLogEntry> _logs = [];

    /// <summary>
    /// Gets the list of captured log entries.
    /// </summary>
    public IReadOnlyList<VKLogEntry> Logs => [.. _logs.OrderBy(l => l.Timestamp)];

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logs.Add(new VKLogEntry(logLevel, eventId, message, exception, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Checks if a log entry with the given level and substring exists.
    /// </summary>
    public bool HasLogged(LogLevel level, string messageSubstring) =>
        _logs.Any(l => l.LogLevel == level && l.Message.Contains(messageSubstring, StringComparison.OrdinalIgnoreCase));

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

/// <summary>
/// Represents a captured log entry in test execution.
/// </summary>
public sealed record VKLogEntry(
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception,
    DateTimeOffset Timestamp);
