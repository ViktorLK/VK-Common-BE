using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI;

/// <summary>
/// Observes completed AI provider invocations for telemetry, health tracking, or quota accounting.
/// </summary>
// [ARCH-02] [CS.03]
public interface IVKAIInvocationObserver
{
    /// <summary>
    /// Gets a value indicating whether this observer is a no-op fallback implementation.
    /// </summary>
    bool IsNoOp => false;

    /// <summary>
    /// Reports the outcome of an AI provider invocation.
    /// </summary>
    /// <param name="outcome">The invocation outcome.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask ReportAsync(
        VKAIInvocationOutcome outcome,
        CancellationToken cancellationToken = default);
}
