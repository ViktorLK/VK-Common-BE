using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// A default implementation of the thought stream gatekeeper that approves everything.
/// </summary>
internal sealed class DefaultThoughtStream : IVKThoughtStream
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
