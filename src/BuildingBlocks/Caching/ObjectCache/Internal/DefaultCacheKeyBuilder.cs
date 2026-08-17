using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Standardized cache key builder enforcing tenant prefix isolation.
/// </summary>
internal sealed class DefaultCacheKeyBuilder(
    IOptions<VKCachingOptions> options,
    IVKIdentityContext identityContext) : ICacheKeyBuilder
{
    private readonly string _globalPrefix = options.Value.KeyPrefix;

    /// <inheritdoc />
    public string BuildKey(string key)
    {
        var tenantId = identityContext.TenantId;
        var tenantPrefix = tenantId != VKTenantId.Default ? tenantId.ToString() : "global";

        var prefix = string.IsNullOrEmpty(_globalPrefix) ? tenantPrefix : $"{_globalPrefix}:{tenantPrefix}";
        return $"{prefix}:{key}";
    }
}
