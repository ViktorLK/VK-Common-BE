using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Pipeline stage for injecting persona configuration into the context.
/// </summary>
internal sealed class DefaultPersonaStage : IVKWeavingStage
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

    public int StageOrder => VKWeavingStageOrder.Extraction;
    public bool IsActive => true;
    public bool IsParallel => true;
    public int? ParallelGroup => 1;

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Persona))
        {
            return VKResult.Success();
        }

        var personaResult = await _store.GetPersonaAsync(context.PersonaId, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (personaResult.IsFailure)
        {
            return VKResult.Failure(personaResult.Errors); // [CS.01]
        }

        PersonaDiagnostics.PersonaResolved(_logger, context.PersonaId, personaResult.Value.Name);

        context.AddFragment(new VKPromptFragment()
        {
            TierType = VKPromptTierType.Persona,
            Role = VKChatRole.System,
            Depth = null,
            RenderOrder = 0,
            Metadata = personaResult.Value,
        });

        return VKResult.Success();
    }
}
