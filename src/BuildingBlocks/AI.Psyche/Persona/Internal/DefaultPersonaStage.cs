using System.Collections.Generic;
using System.Linq;
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
    private readonly IVKPersonaRenderer _personaRenderer;
    private readonly ILogger<DefaultPersonaStage> _logger;

    public DefaultPersonaStage(
        VKPersonaOptions options,
        IVKPsychePersonaRepository personaRepository,
        IVKPersonaRenderer personaRenderer,
        ILogger<DefaultPersonaStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _personaRepository = VKGuard.NotNull(personaRepository);
        _personaRenderer = VKGuard.NotNull(personaRenderer);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsychePersona;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var isEnabled = context.Args<VKPersonaArgs>()?.Enabled ?? _options.Enabled;
        if (!isEnabled)
        {
            return VKResult.Success();
        }

        var personaId = context.Request.PersonaId;
        if (personaId.IsNullOrEmpty())
        {
            return VKResult.Success();
        }

        var personaResult = await _personaRepository.FindByIdAsync(personaId.Value, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (personaResult.IsFailure)
        {
            return VKResult.Failure(personaResult.Errors); // [CS.01]
        }

        var persona = personaResult.Value;
        if (persona is null)
        {
            return VKResult.Success();
        }

        context.SetState(persona);

        _logger.PersonaResolved(persona.Id, persona.Name);

        var content = _personaRenderer.Render(persona);
        if (!string.IsNullOrWhiteSpace(content))
        {
            _logger.PersonaRendered(persona.Id, content.Length);
            context.AddSegment(new VKPromptSegment
            {
                Tier = VKPromptTierType.Persona,
                TagName = PsycheConstants.XmlTags.Persona,
                Content = content,
                DepthPriority = 0,
                TokenCount = persona.TokenCount
            });

            PersonaDiagnostics.RecordPersonasResolved(1, "Persona");
        }

        return VKResult.Success();
    }
}
