using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace VK.Blocks.Testing;

/// <summary>
/// Logger provider that bridges Microsoft.Extensions.Logging to xUnit's <see cref="ITestOutputHelper"/>.
/// </summary>
public sealed class VKTestOutputLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new VKTestOutputLogger(categoryName, _testOutputHelper);

    /// <inheritdoc />
    public void Dispose() { }

    private sealed class VKTestOutputLogger(string categoryName, ITestOutputHelper output) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                var formatted = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{logLevel}] [{categoryName}] {message}";
                if (exception is not null)
                {
                    formatted += Environment.NewLine + exception;
                }
                output.WriteLine(formatted);
            }
            catch
            {
                // Ignore output write failures if test has already finished
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
