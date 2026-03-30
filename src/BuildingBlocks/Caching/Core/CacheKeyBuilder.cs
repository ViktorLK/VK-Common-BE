using VK.Blocks.Caching.Options;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Caching.Core;

/// <summary>
/// Standardized cache key builder with prefix support.
/// </summary>
public sealed class CacheKeyBuilder(IOptions<CachingOptions> options) : ICacheKeyBuilder
{
    private readonly string _prefix = options.Value.KeyPrefix;

    /// <summary>
    /// Builds a full cache key.
    /// </summary>
    public string BuildKey(string key)
    {
        return string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}";
    }
}
