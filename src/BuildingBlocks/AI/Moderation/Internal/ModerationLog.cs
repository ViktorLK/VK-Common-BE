using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Moderation.Internal;

/// <summary>
/// Source-generated logger messages for the Moderation feature.
/// </summary>
internal static partial class ModerationLog
{
    [LoggerMessage(
        EventId = 310,
        Level = LogLevel.Information,
        Message = "Moderation check performed for model: {Model}")]
    public static partial void ModerationCheckPerformed(ILogger logger, string? model);

    [LoggerMessage(
        EventId = 311,
        Level = LogLevel.Warning,
        Message = "Moderation request failed: {Reason}")]
    public static partial void ModerationRequestFailed(ILogger logger, string reason);
}
