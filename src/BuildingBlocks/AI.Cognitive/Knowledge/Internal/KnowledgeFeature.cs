using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Knowledge feature marker and registration hub.
/// </summary>
internal sealed partial class KnowledgeFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKKnowledgeOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKKnowledgeManager, BasicKnowledgeManager>();
        services.TryAddSingleton<IVKKnowledgeWeaver, BasicKnowledgeWeaver>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKKnowledgeOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
