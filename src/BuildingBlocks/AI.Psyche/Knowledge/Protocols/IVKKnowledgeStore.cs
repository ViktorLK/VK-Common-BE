using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Knowledge: Manages Lorebooks and static facts to eliminate hallucinations.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// Stores automatically resolve TenantId via injected <see cref="IVKIdentityContext"/>.
/// </summary>
public interface IVKKnowledgeStore
{
    /// <summary>
    /// Gets relevant knowledge entries for a persona within ambient identity context.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default);
}
