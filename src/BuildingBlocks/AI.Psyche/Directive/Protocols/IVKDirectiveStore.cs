using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to resolve the Directive of a specific tenant.
/// Follows CS.01 and CS.03.
/// </summary>
public interface IVKDirectiveStore
{
    /// <summary>
    /// Resolves the Directive containing prompts and safety rules for the specified tenant.
    /// </summary>
    Task<VKResult<VKDirectiveCharter>> GetDirectiveAsync(
        VKDirectiveId directiveId,
        CancellationToken cancellationToken = default);
}
