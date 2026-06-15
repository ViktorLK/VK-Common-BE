using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

internal sealed class DefaultPatternStage : IVKPsycheBeforePipelineStage
{
    private readonly VKPatternOptions _options;
    private readonly IVKPatternStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePattern;

    public bool IsActive => _options.Enabled;

    public DefaultPatternStage(
        IOptions<VKPatternOptions> options,
        IVKPatternStore store,
        IOptions<VKWeavingOptions> weavingOptions)
    {
        _options = VKGuard.NotNull(options).Value;
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Pattern))
        {
            return VKResult.Success();
        }

        var patternsResult = await _store.GetCurrentPatternsAsync(ct).ConfigureAwait(false); // [CS.03]
        if (patternsResult.IsFailure)
        {
            return VKResult.Failure(patternsResult.Errors); // [CS.01]
        }

        var currentPatterns = patternsResult.Value.ToList();

        foreach (var pattern in currentPatterns)
        {
            context.AddFragment(new VKPromptFragment
            {
                TierType = VKPromptTierType.Pattern,
                Segment = pattern.Segment,
                Metadata = pattern
            });
        }

        return VKResult.Success();
    }
}
