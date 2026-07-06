using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Text.Internal;

/// <summary>
/// Text feature marker and registration hub.
/// </summary>
internal sealed partial class TextFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTextOptions options) =>
        services.TryAddSingleton<IVKTextEngine, NoOpVKTextEngine>();

    /// <summary>Add text-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKTextOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
