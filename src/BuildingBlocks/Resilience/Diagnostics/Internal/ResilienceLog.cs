using Microsoft.Extensions.Logging;

namespace VK.Blocks.Resilience.Diagnostics.Internal;

internal static partial class ResilienceLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Executing resilience pipeline with operation key: {OperationKey}")]
    public static partial void LogPipelineExecution(this ILogger logger, string operationKey);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Resilience strategy execution failed for operation key: {OperationKey}. Error: {Error}")]
    public static partial void LogStrategyFailure(this ILogger logger, string operationKey, string error);
}
