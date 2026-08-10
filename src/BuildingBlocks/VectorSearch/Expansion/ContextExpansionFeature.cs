using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Expansion.Internal;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Context Expansion feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKContextExpansionOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ContextExpansionFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKContextExpansionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKContextExpansionStrategy, NoOpContextExpansionStrategy>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, DefaultContextExpansionStage>());
    }

    static partial void ValidateFeatureCustom(VKContextExpansionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
