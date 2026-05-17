using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Tokenics.Quotas.Internal;

/// <summary>
/// Token Quotas feature marker and registration hub.
/// </summary>
internal sealed partial class QuotasFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKQuotasOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add quota-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKQuotasOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
