using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to resolve the Directive of a specific tenant.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// Stores automatically resolve TenantId via injected <see cref="IVKIdentityContext"/>.
/// </summary>
public interface IVKDirectiveStore
{
    /// <summary>
    /// Resolves the Directive containing prompts and safety rules for the specified directive ID within ambient identity context.
    /// </summary>
    Task<VKResult<VKDirectiveCharter>> GetDirectiveAsync(
        VKDirectiveId directiveId,
        CancellationToken cancellationToken = default);
}
