using System.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.Results;

/// <summary>
/// Provides extension methods for <see cref="Stopwatch"/> to support Rule 6 (Observability) compliance.
/// </summary>
public static class StopwatchExtensions
{
    /// <summary>
    /// Stops the stopwatch and records the result of the process.
    /// This is the standard entry point for observability diagnostics in VK.Blocks.
    /// </summary>
    /// <param name="sw">The stopwatch instance.</param>
    /// <param name="actionName">The name of the action being recorded.</param>
    /// <param name="result">The result of the operation.</param>
    public static void RecordProcess(this Stopwatch sw, string actionName, IResult result)
    {
        sw.Stop();
        
        // FUTURE: This base implementation can be extended to hook into 
        // global ActivitySource or Meter if VK.Blocks.Core gets OTel primitives.
        // For now, it ensures the stopwatch is stopped and serves as a standardized hook.
    }
}
