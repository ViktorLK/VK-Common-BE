using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Persona: Defines identity consistency.
/// Metaphor: Ego - The "Basic Face" of a digital life.
/// Value: Ensures brand consistency (Industrial) and unique soul identity (PWP).
/// Responsible for managing names, tones, personality tags, and core drivers.
/// </summary>
public interface IVKPersonaStore
{
    /// <summary>
    /// Gets a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the persona anchor.</returns>
    Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default);
}
