using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Entitlements.Internal;

/// <summary>
/// Scoped implementation of <see cref="IVKTenantFeatureEvaluator"/>.
/// Provides logic to check if a specific feature is enabled for the current tenant.
/// </summary>
internal sealed class TenantFeatureEvaluator(
    IVKTenantContext tenantContext,
    IVKTenantStore tenantStore) : IVKTenantFeatureEvaluator
{
    private const string FeaturePrefix = "Features:";
    private readonly IVKTenantContext _tenantContext = VKGuard.NotNull(tenantContext);
    private readonly IVKTenantStore _tenantStore = VKGuard.NotNull(tenantStore);

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> IsFeatureEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(featureName);

        if (!_tenantContext.IsResolved)
        {
            return ValueTask.FromResult(VKResult.Success(false));
        }

        // Check the current context's metadata directly if possible to avoid redundant store calls
        if (_tenantContext.CurrentTenant?.Metadata is null)
        {
            return ValueTask.FromResult(VKResult.Success(false));
        }

        string key = FeaturePrefix + featureName;
        bool isEnabled = _tenantContext.CurrentTenant.Metadata.TryGetValue(key, out string? value) &&
                        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

        return ValueTask.FromResult(VKResult.Success(isEnabled));
    }

    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> IsFeatureEnabledAsync(string tenantId, string featureName, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(tenantId);
        VKGuard.NotNullOrWhiteSpace(featureName);

        // Check current context first to avoid store lookup if it matches
        if (string.Equals(_tenantContext.CurrentTenant?.Id, tenantId, StringComparison.OrdinalIgnoreCase))
        {
            return await IsFeatureEnabledAsync(featureName, cancellationToken).ConfigureAwait(false);
        }

        // Otherwise, fetch the tenant from the store
        VKResult<IVKTenantInfo> tenantResult = await _tenantStore.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenantResult.IsFailure)
        {
            return VKResult.Failure<bool>(tenantResult.FirstError);
        }

        IVKTenantInfo? tenant = tenantResult.Value;
        if (tenant?.Metadata is null)
        {
            return VKResult.Success(false);
        }

        string key = FeaturePrefix + featureName;
        bool isEnabled = tenant.Metadata.TryGetValue(key, out string? value) &&
                        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

        return VKResult.Success(isEnabled);
    }
}
