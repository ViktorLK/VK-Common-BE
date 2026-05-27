using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultPersonaPromptExtractor : IVKPromptExtractor
{
    private readonly IVKPersonaStore _store;

    public DefaultPersonaPromptExtractor(IVKPersonaStore store)
    {
        // [AP.01] Defensive boundary check
        _store = VKGuard.NotNull(store);
    }

    public async Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken ct)
    {
        // [AP.01] VKGuard boundary check
        VKGuard.NotNull(context);

        // 1. Fetch persona from the store using context.PersonaId
        var personaResult = await _store.GetPersonaAsync(context.PersonaId, ct).ConfigureAwait(false); // [CS.03]
        if (personaResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKPromptFragment>>(personaResult.FirstError); // [CS.01]
        }

        // 2. Construct prompt fragment containing the raw PersonaAnchor in Metadata (formatting is done by Formatter)
        var fragment = new VKPromptFragment
        {
            TierType = VKPromptTierType.Persona,
            Content = string.Empty, // Populated by IVKPromptFormatter downstream
            Position = VKKnowledgePositions.UserAtDepth,
            Priority = 0,
            Metadata = personaResult.Value // Pass the raw entity to formatting phase
        };

        IReadOnlyList<VKPromptFragment> fragments = [fragment]; // [AP.01]
        return VKResult.Success(fragments);
    }
}
