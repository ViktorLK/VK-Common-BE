using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Tokenics feature marker and registration hub.
/// </summary>
internal sealed partial class TokenicsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKTokenicsOptions options)
    {
        _ = services;
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
