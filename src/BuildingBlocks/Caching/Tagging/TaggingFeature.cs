using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

[VKFeature(typeof(VKCachingBlock), OptionsType = typeof(VKTaggingOptions))]
internal sealed partial class TaggingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTaggingOptions options)
    {
        _ = services;
        _ = options;
    }

    static partial void ValidateFeatureCustom(VKTaggingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
