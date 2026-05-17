using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Vectorics.Retrieval.Internal;

/// <summary>
/// Retrieval feature marker and registration hub.
/// </summary>
internal sealed partial class RetrievalFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKRetrievalOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKRetrievalEngine, NoOpVKRetrievalEngine>();
    }

    /// <summary>Add retrieval-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKRetrievalOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
