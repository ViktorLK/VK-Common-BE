using Microsoft.Extensions.Logging;


namespace VK.Blocks.Infrastructure.Diagnostics.Internal;

/// <summary>
/// Structured logging for the Infrastructure block.
/// </summary>
internal static partial class InfrastructureLog
{
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Information,
        Message = "Infrastructure block initialized with identifier: {Identifier}")]
    public static partial void InfrastructureInitialized(ILogger logger, string identifier);
}
