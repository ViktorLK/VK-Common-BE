using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Pipeline stage for injecting persona configuration into the context.
/// </summary>
internal sealed class DefaultPersonaStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKPersonaStore _store;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultPersonaStage> _logger;

    public DefaultPersonaStage(
        IVKPersonaStore store,
        IOptions<VKWeavingOptions> weavingOptions,
        ILogger<DefaultPersonaStage> logger)
    {
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePersona;
    public bool IsActive => true;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Persona))
        {
            return VKResult.Success();
        }

        var personaResult = await _store.GetPersonaAsync(context.Request.PersonaId, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (personaResult.IsFailure)
        {
            return VKResult.Failure(personaResult.Errors); // [CS.01]
        }

        _logger.PersonaResolved(context.Request.PersonaId, personaResult.Value.Name);

        var tierType = VKPromptTierType.Persona;
        var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
            ? idx * PsycheConstants.Layout.TierCoordinateGap
            : PromptLayout.DefaultRenderOrders[tierType];

        context.AddFragment(new VKPromptFragment()
        {
            TierType = tierType,
            RenderOrder = baseRenderOrder,
            Metadata = personaResult.Value,
            Segment = new VKPromptSegment
            {
                Role = VKChatRole.System
            }
        });

        return VKResult.Success();
    }
}
