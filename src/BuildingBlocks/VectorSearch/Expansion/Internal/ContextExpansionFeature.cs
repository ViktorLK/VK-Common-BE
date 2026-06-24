using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorSearch.Expansion.Internal;

namespace VK.Blocks.VectorSearch.ContextExpansion.Internal;

/// <summary>
/// Context Expansion feature marker and registration hub.
/// </summary>
internal sealed partial class ContextExpansionFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKContextExpansionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKContextExpansionStrategy, NoOpContextExpansionStrategy>();
        services.TryAddScoped<IVKVectorSearchAfterPipelineStage, DefaultContextExpansionStage>();
    }

    static partial void ValidateCustom(VKContextExpansionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
