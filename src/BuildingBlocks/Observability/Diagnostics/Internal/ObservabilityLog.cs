using Microsoft.Extensions.Logging;

namespace VK.Blocks.Observability.Diagnostics.Internal;

/// <summary>
/// Source-generated logging extensions for the Observability Core block.
/// Complies with OR.01.
/// </summary>
internal static partial class ObservabilityLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Debug,
        Message = "Observability block services registered.")]
    public static partial void LogBlockRegistered(this ILogger logger);
}

