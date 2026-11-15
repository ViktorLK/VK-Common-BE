using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Tokenics feature marker and registration hub.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKTokenicsOptions))]
internal sealed partial class TokenicsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTokenicsOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add tokenics-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKTokenicsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
