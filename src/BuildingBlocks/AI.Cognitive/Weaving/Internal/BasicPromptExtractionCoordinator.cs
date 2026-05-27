using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicPromptExtractionCoordinator : IVKPromptExtractionCoordinator
{
    private readonly IEnumerable<IVKPromptExtractor> _extractors;

    public BasicPromptExtractionCoordinator(IEnumerable<IVKPromptExtractor> extractors)
    {
        _extractors = VKGuard.NotNull(extractors);
    }

    public async Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAllAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = cts.Token;

        var pendingTasks = _extractors
            .Select(extractor => extractor.ExtractAsync(context, linkedToken))
            .ToList();

        var fragments = new List<VKPromptFragment>();

        while (pendingTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(pendingTasks).ConfigureAwait(false);
            pendingTasks.Remove(completedTask);

            var extractResult = await completedTask.ConfigureAwait(false);
            if (extractResult.IsFailure)
            {
                // Short-circuit: cancel remaining tasks and return failure immediately
                cts.Cancel();
                return VKResult.Failure<IReadOnlyList<VKPromptFragment>>(extractResult.FirstError);
            }

            fragments.AddRange(extractResult.Value);
        }

        return VKResult.Success<IReadOnlyList<VKPromptFragment>>(fragments);
    }
}
