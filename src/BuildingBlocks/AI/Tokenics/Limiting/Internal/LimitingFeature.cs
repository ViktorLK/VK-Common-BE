using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Tokenics.Limiting.Internal;

/// <summary>
/// Token Limiting feature marker and registration hub.
/// </summary>
internal sealed partial class LimitingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKLimitingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenRateLimiter, NoOpVKTokenRateLimiter>();
    }

    /// <summary>Add limiting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKLimitingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
