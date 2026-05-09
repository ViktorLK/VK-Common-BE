using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Evaluates whether a specific feature or entitlement is enabled for a tenant.
/// </summary>
public interface IVKTenantFeatureEvaluator
{
    /// <summary>
    /// Checks if the specified feature is enabled for the current tenant in the scoped context.
    /// </summary>
    /// <param name="featureName">The name of the feature to check (e.g., "Reporting").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <c>true</c> if the feature is enabled; otherwise, <c>false</c>.</returns>
    ValueTask<VKResult<bool>> IsFeatureEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the specified feature is enabled for a specific tenant ID.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <c>true</c> if the feature is enabled; otherwise, <c>false</c>.</returns>
    ValueTask<VKResult<bool>> IsFeatureEnabledAsync(string tenantId, string featureName, CancellationToken cancellationToken = default);
}
