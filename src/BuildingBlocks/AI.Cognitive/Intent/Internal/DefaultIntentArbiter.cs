using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// A default, no-op implementation of <see cref="IVKIntentArbiter"/> that simply returns
/// the first candidate intent (usually the highest confidence one) or the default intent.
/// </summary>
internal sealed class DefaultIntentArbiter : IVKIntentArbiter
{
    public Task<VKResult<VKIntentContext>> ArbitrateAsync(
        string input,
        VKIntentArbiterArgs args,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(input); // [AP.01]
        VKGuard.NotNull(args); // [AP.01]

        var first = args.Candidates.Count > 0 ? args.Candidates[0] : args.DefaultIntent;

        return Task.FromResult(VKResult.Success(new VKIntentContext
        {
            Intent = first,
            Confidence = 1.0,
            RefinedInput = input,
            Source = "DefaultIntentArbiter"
        }));
    }
}
