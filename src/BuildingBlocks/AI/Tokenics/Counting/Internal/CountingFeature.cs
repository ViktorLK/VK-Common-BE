using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Tokenics.Counting.Internal;

/// <summary>
/// Token Counting feature marker and registration hub.
/// </summary>
internal sealed partial class CountingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKCountingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenCounter, DefaultTokenCounter>();
    }

    /// <summary>Add counting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKCountingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
