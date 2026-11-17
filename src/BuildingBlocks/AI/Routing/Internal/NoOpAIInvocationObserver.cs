using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Routing.Internal;

/// <summary>
/// Default no-op implementation of <see cref="IVKAIInvocationObserver"/>.
/// </summary>
// [ARCH-05] [AP.01]
internal sealed class NoOpAIInvocationObserver : IVKAIInvocationObserver
{
    /// <inheritdoc />
    public ValueTask ReportAsync(
        VKAIInvocationOutcome outcome,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
