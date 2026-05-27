using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

internal sealed class DefaultReasoningPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly VKReasoningOptions _options;
    private readonly IVKIntentNexus _intentNexus;

    public int Order => 100; // Reasoning happens early (before persona/knowledge extraction)

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public DefaultReasoningPipelineStage(
        IOptions<VKReasoningOptions> options,
        IVKIntentNexus intentNexus)
    {
        _options = VKGuard.NotNull(options).Value;
        _intentNexus = VKGuard.NotNull(intentNexus);
    }

    public Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // TODO _intentNexus.Route
        // [CS.01] Directly returns a successful Chat intent context bypass intent nexus routing
        context.IntentContext = new VKIntentContext
        {
            Intent = VKIntent.Chat,
            RefinedInput = context.Input,
            Confidence = 1.0
        };

        return Task.CompletedTask;
    }
}
