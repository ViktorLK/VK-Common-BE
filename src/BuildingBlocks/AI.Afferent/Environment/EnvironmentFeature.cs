using VK.Blocks.AI.Afferent.Environment.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Environment feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKEnvironmentOptions))]
internal sealed partial class EnvironmentFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEnvironmentOptions options)
    {
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEnvironmentOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
