using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Persona: Defines identity consistency.
/// Follows CS.01, CS.03, and CS.04 batching patterns.
/// </summary>
public interface IVKPersonaStore
{
    /// <summary>
    /// Gets personas by identifiers.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKPersonaAnchor>>> GetPersonasAsync(
        IReadOnlyList<VKPersonaId> personaIds,
        CancellationToken cancellationToken = default);
}
