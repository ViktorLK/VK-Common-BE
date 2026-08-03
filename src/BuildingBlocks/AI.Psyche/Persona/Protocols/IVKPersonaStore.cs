using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Persona: Defines identity consistency.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// Stores automatically resolve TenantId via injected <see cref="IVKIdentityContext"/>.
/// </summary>
public interface IVKPersonaStore
{
    /// <summary>
    /// Gets a persona by identifier within ambient identity context.
    /// </summary>
    Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default);
}
