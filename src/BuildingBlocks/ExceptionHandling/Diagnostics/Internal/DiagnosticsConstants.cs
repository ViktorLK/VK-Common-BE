namespace VK.Blocks.ExceptionHandling.Diagnostics.Internal;

/// <summary>
/// Constants for ExceptionHandling diagnostics.
/// </summary>
internal static class DiagnosticsConstants
{
    // Note: Instance.ActivitySourceName is handled by VKBlockDiagnostics SG if configured
    public const string HandledCountName = "exception_handling.handled.count";
    public const string HandledCountDescription = "Number of exceptions processed by the handling pipeline.";

    public const string HandlerTagName = "handler";
    public const string HandledTagName = "handled";
    public const string ExceptionTypeTagName = "exception.type";
    public const string ErrorCodeTagName = "error.code";
}
