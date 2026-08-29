using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Pipeline stage for injecting persona configuration into the context.
/// Follows BB.01, AP.01, and OR.01.
/// </summary>
[VKTrace("psyche.stage.persona")]
internal sealed class DefaultPersonaStage : IVKPsychePipelineStage
{
    private readonly VKPersonaOptions _options;
    private readonly IVKPsychePersonaRepository _personaRepository;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultPersonaStage> _logger;

    public DefaultPersonaStage(
        VKPersonaOptions options,
        IVKPsychePersonaRepository personaRepository,
        VKWeavingOptions weavingOptions,
        ILogger<DefaultPersonaStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _personaRepository = VKGuard.NotNull(personaRepository);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePersona;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Persona))
        {
            return VKResult.Success();
        }

        if (context.Request.PersonaIds.Count == 0)
        {
            return VKResult.Success();
        }

        var personasResult = await _personaRepository.ListByIdsAsync(context.Request.PersonaIds, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (personasResult.IsFailure)
        {
            return VKResult.Failure(personasResult.Errors); // [CS.01]
        }

        var tierType = VKPromptTierType.Persona;
        var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
            ? idx * PsycheConstants.Layout.TierCoordinateGap
            : PromptLayout.DefaultRenderOrders[tierType];

        foreach (var persona in personasResult.Value)
        {
            context.SetState(persona);
            _logger.PersonaResolved(persona.Id, persona.Name);

            context.AddFragment(new VKPromptFragment()
            {
                TierType = tierType,
                RenderOrder = baseRenderOrder,
                Metadata = persona,
                Segment = new VKPromptSegment
                {
                    Role = VKChatRole.System
                }
            });
        }

        if (personasResult.Value.Count > 0)
        {
            PersonaDiagnostics.RecordPersonasResolved(personasResult.Value.Count, "Persona");
        }

        return VKResult.Success();
    }
}
