using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// Early pipeline stage for Psyche orchestration that classifies user intent and populates <see cref="VKIntentContext"/>.
/// </summary>
internal sealed class DefaultIntentPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly VKIntentOptions _options;
    private readonly IVKIntentRouter _intentRouter;

    public int Order => 100; // Intent triage happens early before persona/knowledge extraction

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public DefaultIntentPipelineStage(
        IOptions<VKIntentOptions> options,
        IVKIntentRouter intentRouter)
    {
        _options = VKGuard.NotNull(options).Value; // [AP.01]
        _intentRouter = VKGuard.NotNull(intentRouter); // [AP.01]
    }

    public async Task ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context); // [AP.01]

        string input = context.Request.UserInput ?? string.Empty;
        var result = await _intentRouter.RouteAsync(input, null, ct).ConfigureAwait(false); // [CS.03]

        var intentContext = result.IsSuccess
            ? result.Value
            : new VKIntentContext
            {
                Intent = VKIntent.Chat,
                RefinedInput = input,
                Confidence = 1.0,
                Source = "PipelineFallback"
            };

        // Attach to VKPsycheContext extensibility container via SetState<T>
        context.SetState(intentContext);
    }
}
