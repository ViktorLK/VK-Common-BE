using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// A no-op/pass-through implementation of <see cref="IVKThoughtStream"/> that approves all thoughts.
/// Ensures standard deterministic execution.
/// </summary>
internal sealed class NoopThoughtStream : IVKThoughtStream
{
    public Task<VKResult<VKThoughtEvaluation>> EvaluateAsync(
        string reasoning,
        VKIntentContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(new VKThoughtEvaluation
        {
            IsApproved = true
        }));
    }

    public Task<VKResult<string>> InterceptDeltaAsync(string delta, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(delta));
    }
}
