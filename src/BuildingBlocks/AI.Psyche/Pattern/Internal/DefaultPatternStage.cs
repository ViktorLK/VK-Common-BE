using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Pattern.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

[VKTrace("psyche.stage.pattern")]
internal sealed class DefaultPatternStage : IVKPsychePipelineStage
{
    private readonly VKPatternOptions _options;
    private readonly IVKPsychePatternRepository _patternRepository;
    private readonly ILogger<DefaultPatternStage> _logger;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePattern;
    public bool IsActive => _options.Enabled;

    public DefaultPatternStage(
        VKPatternOptions options,
        IVKPsychePatternRepository patternRepository,
        ILogger<DefaultPatternStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _patternRepository = VKGuard.NotNull(patternRepository);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var isEnabled = context.Args<VKPatternArgs>()?.Enabled ?? _options.Enabled;
        if (!isEnabled)
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
        if (currentPatterns.Count == 0)
        {
            return VKResult.Success();
        }

        // Dual-factor deterministic sorting: DepthPriority first (ascending), then request declaration order
        var requestOrder = new Dictionary<VKPatternId, int>(context.Request.PatternIds.Count);
        for (int i = 0; i < context.Request.PatternIds.Count; i++)
        {
            requestOrder.TryAdd(context.Request.PatternIds[i], i);
        }

        var sortedPatterns = currentPatterns
            .DistinctBy(p => p.Id)
            .OrderBy(p => p.Segment.DepthPriority)
            .ThenBy(p => requestOrder.GetValueOrDefault(p.Id, int.MaxValue))
            .ToList();

        context.SetState<IReadOnlyList<VKPatternEntry>>(sortedPatterns);

        int resolvedCount = 0;
        foreach (var pattern in sortedPatterns)
        {
            if (string.IsNullOrWhiteSpace(pattern.Segment.Content))
            {
                continue;
            }

            _logger.PatternResolved(pattern.Id);

            var segment = pattern.Segment with { Tier = VKPromptTierType.Pattern };
            context.AddSegment(segment);
            resolvedCount++;
        }

        if (resolvedCount > 0)
        {
            PatternDiagnostics.RecordPatternsResolved(resolvedCount, "Pattern");
        }

        return VKResult.Success();
    }
}
