using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Pattern.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

[VKTrace("psyche.stage.pattern")]
internal sealed class DefaultPatternStage : IVKPsychePipelineStage
{
    private readonly VKPatternOptions _options;
    private readonly IVKPsychePatternRepository _patternRepository;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultPatternStage> _logger;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePattern;
    public bool IsActive => _options.Enabled;

    public DefaultPatternStage(
        VKPatternOptions options,
        IVKPsychePatternRepository patternRepository,
        VKWeavingOptions weavingOptions,
        ILogger<DefaultPatternStage>? logger = null)
    {
        _options = VKGuard.NotNull(options);
        _patternRepository = VKGuard.NotNull(patternRepository);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _logger = logger ?? NullLogger<DefaultPatternStage>.Instance;
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Pattern))
        {
            return VKResult.Success();
        }

        if (context.Request.PatternIds.Count == 0)
        {
            return VKResult.Success();
        }

        var patternsResult = await _patternRepository.ListByIdsAsync(context.Request.PatternIds, ct).ConfigureAwait(false); // [CS.03]
        if (patternsResult.IsFailure)
        {
            return VKResult.Failure(patternsResult.Errors); // [CS.01]
        }

        var currentPatterns = patternsResult.Value;

        foreach (var pattern in currentPatterns)
        {
            _logger.PatternResolved(pattern.Id.Value.ToString());

            context.AddFragment(new VKPromptFragment
            {
                TierType = VKPromptTierType.Pattern,
                Segment = pattern.Segment,
                Metadata = pattern
            });
        }

        if (currentPatterns.Count > 0)
        {
            PatternDiagnostics.RecordPatternsResolved(currentPatterns.Count, "Pattern");
        }

        return VKResult.Success();
    }
}
