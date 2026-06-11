using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public int StageOrder => VKWeavingStageOrder.Extraction;

    public bool IsActive => true;
    public bool IsParallel => true;
    public int? ParallelGroup => 1;

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
        context.AddFragment(new VKPromptFragment()
        {
            TierType = tierType,
            Role = VKChatRole.System,
            RenderOrder = 0,
            Metadata = directive,
        });

        return VKResult.Success();
    }
}
