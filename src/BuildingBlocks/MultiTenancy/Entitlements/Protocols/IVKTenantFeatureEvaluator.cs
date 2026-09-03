using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Evaluates whether a specific feature is enabled for the currently resolved tenant.
/// Follows AP.01, CS.01.
/// </summary>
public interface IVKTenantFeatureEvaluator
{
    /// <summary>
    /// Checks whether the specified feature is enabled for the current tenant.
    /// </summary>
    /// <param name="featureName">The name of the feature to evaluate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> indicating whether the feature is enabled.</returns>
    Task<VKResult<bool>> IsFeatureEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the specified feature is enabled for an explicitly provided tenant ID.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="featureName">The name of the feature to evaluate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> indicating whether the feature is enabled.</returns>
    Task<VKResult<bool>> IsFeatureEnabledAsync(VKTenantId tenantId, string featureName, CancellationToken cancellationToken = default);
}
