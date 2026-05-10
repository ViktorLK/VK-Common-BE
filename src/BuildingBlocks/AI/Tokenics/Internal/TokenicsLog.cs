using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Source-generated logger messages for the Tokenics feature.
/// </summary>
internal static partial class TokenicsLog
{
    [LoggerMessage(
        EventId = 400,
        Level = LogLevel.Information,
        Message = "Token calculation performed for model: {Model}")]
    public static partial void TokenCalculationPerformed(ILogger logger, string? model);

    [LoggerMessage(
        EventId = 401,
        Level = LogLevel.Warning,
        Message = "Tokenics operation failed: {Reason}")]
    public static partial void TokenicsOperationFailed(ILogger logger, string reason);
}
