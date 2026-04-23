using System.Diagnostics;

namespace VK.Blocks.Core;

/// <summary>
/// Provides extension methods for <see cref="Stopwatch"/> to support Rule 6 (Observability) compliance.
/// </summary>
public static class VKStopwatchExtensions
{
    /// <summary>
    /// Stops the stopwatch and records the result of the process.
    /// This is the standard entry point for observability diagnostics in VK.Blocks.
    /// </summary>
    /// <param name="sw">The stopwatch instance.</param>
    /// <param name="actionName">The name of the action being recorded.</param>
    /// <param name="result">The result of the operation.</param>
    public static void RecordProcess(this Stopwatch sw, string actionName, IVKResult result)
    {
        sw.Stop();

        // Rule 6: Support distributed tracing by enriching the current activity with process metadata.
        // This ensures that even without explicit logging, the trace contains performance and success indicators.
        Activity? activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        // Record process-specific metrics as activity tags
        activity.SetTag("vk.process.name", actionName);
        activity.SetTag("vk.process.duration_ms", sw.ElapsedMilliseconds);
        activity.SetTag("vk.process.success", result.IsSuccess);

        // Enrich failure details if the process did not complete successfully
        if (result.IsFailure)
        {
            activity.SetTag("vk.process.error_code", result.FirstError.Code);
            activity.SetTag("vk.process.error_type", result.FirstError.Type.ToString());
        }
    }
}
