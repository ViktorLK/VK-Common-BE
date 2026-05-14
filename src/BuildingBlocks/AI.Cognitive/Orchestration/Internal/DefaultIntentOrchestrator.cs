using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIntentNexus"/> that fallback to basic chat intent.
/// </summary>
internal sealed class DefaultIntentOrchestrator : IVKIntentNexus
{
    public ValueTask<VKResult<VKIntentContext>> RouteAsync(string input, IVKAIArgs? args = null, CancellationToken ct = default)
    {
        return ValueTask.FromResult(VKResult.Success(new VKIntentContext
        {
            Intent = VKIntent.Chat,
            RefinedInput = input
        }));
    }
}
