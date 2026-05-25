using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class TokenicsDiagnostics
{
    [LoggerMessage(
        EventId = VKTokenicsDiagnosticTokens.QuotaExceededEventId,
        Level = LogLevel.Warning,
        Message = "Token quota exceeded. Current: {Current}, Estimated: {Estimated}, Limit: {Limit}")]
    public static partial void QuotaExceeded(ILogger logger, long current, long estimated, long limit);
}
