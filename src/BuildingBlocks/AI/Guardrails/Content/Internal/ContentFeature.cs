using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Guardrails.Content.Internal;

/// <summary>
/// Content Guard feature marker and registration hub.
/// </summary>
internal sealed partial class ContentFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKContentOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add content-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKContentOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
