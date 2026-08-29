using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Default implementation of the Psyche Directive Stage.
/// Injects system guardrails, safety rules, and operational constraints into the system prompt.
/// </summary>
[VKTrace("psyche.stage.directive")]
internal sealed class DefaultDirectiveStage : IVKPsychePipelineStage
{
    private readonly VKDirectiveOptions _options;
    private readonly IVKPsycheDirectiveRepository _directiveRepository;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultDirectiveStage> _logger;

    public DefaultDirectiveStage(
        VKDirectiveOptions options,
        IVKPsycheDirectiveRepository directiveRepository,
        VKWeavingOptions weavingOptions,
        ILogger<DefaultDirectiveStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _directiveRepository = VKGuard.NotNull(directiveRepository);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _logger = VKGuard.NotNull(logger);
    }

    /// <summary>
    /// Executes early in the weaving pipeline to guarantee Directive guardrails are loaded first.
    /// </summary>
    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheDirective;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Directive))
        {
            return VKResult.Success();
        }

        if (context.Request.DirectiveIds.Count == 0)
        {
            return VKResult.Success();
        }

        var resolveResult = await _directiveRepository.ListByIdsAsync(context.Request.DirectiveIds, cancellationToken).ConfigureAwait(false); // [CS.03]
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
            _logger.DirectiveResolved(directive.Id.Value.ToString());

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

        if (resolveResult.Value.Count > 0)
        {
            DirectiveDiagnostics.RecordDirectivesResolved(resolveResult.Value.Count, "Directive");
        }

        return VKResult.Success();
    }
}
