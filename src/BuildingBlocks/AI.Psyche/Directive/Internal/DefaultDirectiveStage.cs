using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Pipeline stage to fetch the Directive and prepend it to the weaving context's system instructions.
/// Implements AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultDirectiveStage : IVKPsychePipelineStage
{
    private readonly VKDirectiveOptions _options;
    private readonly IVKDirectiveStore _store;
    private readonly ILogger<DefaultDirectiveStage> _logger;
    private readonly VKWeavingOptions _weavingOptions;

    public DefaultDirectiveStage(
        VKDirectiveOptions options,
        IVKDirectiveStore store,
        ILogger<DefaultDirectiveStage> logger,
        VKWeavingOptions weavingOptions)
    {
        _options = VKGuard.NotNull(options);
        _store = VKGuard.NotNull(store);
        _logger = VKGuard.NotNull(logger);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
    }

    /// <summary>
    /// Executes early in the weaving pipeline (Order = 5) to guarantee Directive guardrails are loaded first.
    /// </summary>
    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheDirective;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
            if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Directive))
            {
                return VKResult.Success();
            }

            if (context.Request.DirectiveIds.Count == 0)
            {
                return VKResult.Success();
            }

            var resolveResult = await _store.GetDirectivesAsync(context.Request.DirectiveIds, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (resolveResult.IsFailure)
            {
                return VKResult.Failure(resolveResult.Errors);
            }

            var tierType = VKPromptTierType.Directive;
            var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
                ? idx * PsycheConstants.Layout.TierCoordinateGap
                : PromptLayout.DefaultRenderOrders[tierType];

            foreach (var directive in resolveResult.Value)
            {
                context.AddFragment(new VKPromptFragment()
                {
                    TierType = tierType,
                    RenderOrder = baseRenderOrder,
                    Metadata = directive,
                    Segment = new VKPromptSegment
                    {
                        Role = VKChatRole.System
                    }
                });
            }

            return VKResult.Success();
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.DirectiveStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
