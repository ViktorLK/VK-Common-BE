using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy interface for validating capacity quotas when storing structured facts.
/// Allows external decoupling and custom tenant quota providers.
/// </summary>
public interface IVKFactCapacityPolicy
{
    /// <summary>
    /// Validates whether a new fact can be stored under the given scope/tenant.
    /// </summary>
    Task<VKResult> ValidateCapacityAsync(VKTenantId? tenantId, int currentFactCount, CancellationToken cancellationToken = default);
}
