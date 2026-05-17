using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Vectorics.Internal;

/// <summary>
/// Vectorics feature marker and registration hub.
/// </summary>
internal sealed partial class VectoricsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKVectoricsOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add vectorics-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKVectoricsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
