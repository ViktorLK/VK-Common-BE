using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Pipeline stage to fetch the Tenant Directive and prepend it to the weaving context's system instructions.
/// Implements AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultDirectiveStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKDirectiveStore _store;
    private readonly ILogger<DefaultDirectiveStage> _logger;
    private readonly VKWeavingOptions _weavingOptions;

    public DefaultDirectiveStage(
        IVKDirectiveStore store,
        ILogger<DefaultDirectiveStage> logger,
        IOptions<VKWeavingOptions> weavingOptions)
    {
        _store = VKGuard.NotNull(store);
        _logger = VKGuard.NotNull(logger);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
    }

    /// <summary>
    /// Executes early in the weaving pipeline (Order = 5) to guarantee Directive guardrails are loaded first.
    /// </summary>
    public VKPipelineStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheDirective;
    public bool IsActive => true;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Directive))
        {
            return VKResult.Success();
        }

        var directiveId = context.Args<VKDirectiveArgs>()?.DirectiveId;
        if (!directiveId.HasValue || directiveId.Value.IsEmpty)
        {
            directiveId = VKDirectiveId.Empty;
        }

        var resolveResult = await _store.GetDirectiveAsync(directiveId.Value, cancellationToken).ConfigureAwait(false);
        if (resolveResult.IsFailure)
        {
            return VKResult.Failure(resolveResult.Errors);
        }

        var tierType = VKPromptTierType.Directive;
        var directive = resolveResult.Value;
        var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
            ? idx * PsycheConstants.Layout.TierCoordinateGap
            : PromptLayout.DefaultRenderOrders[tierType];

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

        return VKResult.Success();
    }
}
