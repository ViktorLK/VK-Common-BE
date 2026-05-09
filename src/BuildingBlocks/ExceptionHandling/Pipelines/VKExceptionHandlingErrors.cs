using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Predefined errors for the ExceptionHandling building block.
/// </summary>
public static class VKExceptionHandlingErrors
{
    /// <summary>
    /// Returned when an unhandled exception occurs and is processed by the default handler.
    /// </summary>
    public static readonly VKError Unhandled = VKError.Failure(
        "Exception.Unhandled",
        "An unhandled exception occurred during the execution of the request.");

    /// <summary>
    /// Returned when the exception handling pipeline itself fails.
    /// </summary>
    public static readonly VKError PipelineFailure = VKError.Failure(
        "Exception.PipelineFailure",
        "The exception handling pipeline encountered an internal error.");
}
