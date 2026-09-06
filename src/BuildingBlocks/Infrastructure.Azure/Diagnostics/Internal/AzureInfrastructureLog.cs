namespace VK.Blocks.Infrastructure.Azure.Diagnostics.Internal;

using Microsoft.Extensions.Logging;



/// <summary>
/// Structured logging for Azure Infrastructure.
/// </summary>
internal static partial class AzureInfrastructureLog
{
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Information,
        Message = "Azure Infrastructure initialized with Region: {Region}")]
    public static partial void InfrastructureInitialized(ILogger logger, string? region);
}
