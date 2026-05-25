using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Persona: Defines identity consistency.
/// Metaphor: Ego - The "Basic Face" of a digital life.
/// Value: Ensures brand consistency (Industrial) and unique soul identity (PWP).
/// Responsible for managing names, tones, personality tags, and core drivers.
/// </summary>
public interface IVKPersonaStore
{
    /// <summary>
    /// Adds a new persona. Fails if a persona with the same identifier already exists.
    /// </summary>
    /// <param name="anchor">The persona anchor to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result representing success or failure.</returns>
    // [CS.01] // [CS.03]
    Task<VKResult> AddPersonaAsync(VKPersonaAnchor anchor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the persona anchor.</returns>
    Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(string personaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available personas.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the collection of persona cards.</returns>
    Task<VKResult<IEnumerable<VKPersonaAnchor>>> GetAllPersonasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="updatedAnchor">The updated persona anchor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> UpdatePersonaAsync(string personaId, VKPersonaAnchor updatedAnchor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> DeletePersonaAsync(string personaId, CancellationToken cancellationToken = default);
}
