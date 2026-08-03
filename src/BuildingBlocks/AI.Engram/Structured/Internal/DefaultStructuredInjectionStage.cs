using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Structured.Internal;

/// <summary>
/// Pipeline stage running BEFORE the LLM call to retrieve and inject structured facts into the Psyche context.
/// Implements AP.01 and CS.03.
/// </summary>
internal sealed class DefaultStructuredInjectionStage : IVKPsychePipelineStage
{
    private readonly IVKStructuredMemoryStore _structuredStore;
    private readonly VKStructuredOptions _options;
    private readonly ILogger<DefaultStructuredInjectionStage> _logger;

    public DefaultStructuredInjectionStage(
        IVKStructuredMemoryStore structuredStore,
        IOptions<VKStructuredOptions> options,
        ILogger<DefaultStructuredInjectionStage> logger)
    {
        _structuredStore = VKGuard.NotNull(structuredStore);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheDirective;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        var keysResult = await _structuredStore.ListKeysAsync(prefix: null, cancellationToken).ConfigureAwait(false);
        if (keysResult.IsFailure)
        {
            return VKResult.Failure(keysResult.Errors);
        }

        var keys = keysResult.Value.ToList();
        if (keys.Count == 0)
        {
            return VKResult.Success();
        }

        var sb = new StringBuilder();
        sb.AppendLine("[Structured Memory Facts]");

        // Append FactExtractions evaluated by Cognitive via zero-coupling context.State<T>()
        var assessment = context.State<VKReflectionAssessment>();
        if (assessment?.FactExtractions is { Count: > 0 } facts)
        {
            foreach (var fact in facts)
            {
                sb.AppendLine($"- {fact}");
            }
        }

        int count = 0;
        foreach (var key in keys)
        {
            if (count >= _options.MaxFactsPerTenant)
            {
                break;
            }

            var factResult = await _structuredStore.GetFactAsync<object>(key, cancellationToken).ConfigureAwait(false);
            if (factResult.IsSuccess && factResult.Value is not null)
            {
                sb.AppendLine($"- {key}: {factResult.Value}");
                count++;
            }
        }

        if (count > 0)
        {
            var summaryContent = sb.ToString().TrimEnd();
            var fragment = new VKPromptFragment
            {
                TierType = VKPromptTierType.Directive,
                Metadata = new VKStructuredFact
                {
                    Key = "structured:facts:batch",
                    Value = summaryContent
                },
                Segment = new VKPromptSegment
                {
                    Content = summaryContent,
                    Role = VKChatRole.System,
                    IsEnabled = true,
                    RelativeDepth = VKPromptRelativeDepth.AfterPersona,
                    DepthPriority = 450
                }
            };

            context.AddFragment(fragment);
        }

        return VKResult.Success();
    }
}
