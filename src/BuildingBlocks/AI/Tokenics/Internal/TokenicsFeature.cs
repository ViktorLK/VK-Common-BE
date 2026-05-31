using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Tokenics.Counting.Internal;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Tokenics feature marker and registration hub.
/// </summary>
internal sealed partial class TokenicsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKTokenicsOptions options)
    {
        services.TryAddSingleton<IVKTokenCounter, DefaultTokenCounter>();
        _ = options;
    }

    /// <summary>Add tokenics-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKTokenicsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
