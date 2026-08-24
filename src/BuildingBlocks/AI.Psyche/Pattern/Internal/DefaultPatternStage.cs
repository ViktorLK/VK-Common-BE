using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

internal sealed class DefaultPatternStage : IVKPsychePipelineStage
{
    private readonly VKPatternOptions _options;
    private readonly IVKPatternStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePattern;

    public bool IsActive => _options.Enabled;

    public DefaultPatternStage(
        VKPatternOptions options,
        IVKPatternStore store,
        VKWeavingOptions weavingOptions)
    {
        _options = VKGuard.NotNull(options);
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
            if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Pattern))
            {
                return VKResult.Success();
            }

            if (context.Request.PatternIds.Count == 0)
            {
                return VKResult.Success();
            }

            var patternsResult = await _store.GetPatternsAsync(context.Request.PatternIds, ct).ConfigureAwait(false); // [CS.03]
            if (patternsResult.IsFailure)
            {
                return VKResult.Failure(patternsResult.Errors); // [CS.01]
            }

            var currentPatterns = patternsResult.Value;

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
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics["PatternStage"] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
