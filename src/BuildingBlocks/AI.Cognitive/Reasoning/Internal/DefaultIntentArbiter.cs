using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

/// <summary>
/// A default, no-op implementation of <see cref="IVKIntentArbiter"/> that simply returns
/// the first candidate intent (usually the highest confidence one).
/// </summary>
internal sealed class DefaultIntentArbiter : IVKIntentArbiter
{
    public Task<VKResult<VKIntentContext>> ArbitrateAsync(
        IEnumerable<VKIntent> candidates,
        VKIntentArbiterArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(candidates);

        var first = candidates.FirstOrDefault();

        return Task.FromResult(VKResult.Success(new VKIntentContext
        {
            Intent = first == default ? VKIntent.Unknown : first,
            Confidence = 1.0
        }));
    }
}
