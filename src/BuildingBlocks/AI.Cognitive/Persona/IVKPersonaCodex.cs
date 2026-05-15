using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for providing AI personas.
/// </summary>
public interface IVKPersonaCodex
{
    /// <summary>
    /// Gets a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the persona card.</returns>
    Task<VKResult<VKPersonaCard>> GetPersonaAsync(string personaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available personas.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the collection of persona cards.</returns>
    Task<VKResult<IEnumerable<VKPersonaCard>>> GetAllPersonasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="updatedCard">The updated persona card.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> UpdatePersonaAsync(string personaId, VKPersonaCard updatedCard, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a persona by identifier.
    /// </summary>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> DeletePersonaAsync(string personaId, CancellationToken cancellationToken = default);
}
