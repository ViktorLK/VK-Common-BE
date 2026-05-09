using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Validation.Diagnostics.Internal;

/// <summary>
/// Infrastructure for validation diagnostics.
/// </summary>
[VKBlockDiagnostics<VKValidationBlock>]
internal static partial class ValidationDiagnostics
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Validation pipeline executed for model '{ModelType}'. Result: {IsValid}, ErrorCount: {ErrorCount}")]
    public static partial void LogPipelineExecuted(this ILogger logger, string modelType, bool isValid, int errorCount);
}
