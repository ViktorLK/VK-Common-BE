using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

// [AP.01] sealed default implementation
internal sealed class PipelineSystemInstructionsExtractor : IVKPromptExtractor
{
    public Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken ct)
    {
        // [AP.01]
        VKGuard.NotNull(context);

        var fragments = new List<VKPromptFragment>();

        // [AP.05] Extract consolidated system instructions from args if present
        if (context.Args is VKCognitivePipelineArgs pipelineArgs && !string.IsNullOrWhiteSpace(pipelineArgs.SystemInstructions))
        {
            fragments.Add(new VKPromptFragment
            {
                Content = pipelineArgs.SystemInstructions,
                Position = VKKnowledgePositions.BeforeDefs,
                TierType = VKPromptTierType.SystemInstructions,
                Priority = 100, // Core system constitution takes top priority
                Depth = 0,
                Metadata = new Dictionary<string, object?> { { "Source", "PipelineArgs" } }
            });
        }

        IReadOnlyList<VKPromptFragment> resultList = fragments; // [AP.01]
        return Task.FromResult(VKResult.Success(resultList));
    }
}
