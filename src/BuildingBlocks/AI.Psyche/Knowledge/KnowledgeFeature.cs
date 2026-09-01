using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Knowledge feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKKnowledgeOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class KnowledgeFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKKnowledgeOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddSingleton<InMemoryKnowledgeRepository>();
        services.TryAddSingleton<IVKPsycheKnowledgeRepository>(sp => sp.GetRequiredService<InMemoryKnowledgeRepository>());
        services.TryAddSingleton<IVKReadRepository<VKKnowledgeEntry, VKKnowledgeId>>(sp => sp.GetRequiredService<InMemoryKnowledgeRepository>());
        services.TryAddSingleton<IVKKnowledgeRenderer, DefaultKnowledgeRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultKnowledgeStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultKnowledgeFinalizerStage>());

        // Register non-generic extractor and formatter
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultKnowledgeFormatter>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKKnowledgeOptions options, List<string> failures)
    {
        if (options.MaxEntriesToInject < 0)
        {
            failures.Add("MaxEntriesToInject must be non-negative.");
        }

        if (options.ReservedTokens < 0)
        {
            failures.Add("ReservedTokens must be non-negative.");
        }

        if (options.SemanticThreshold is < 0 or > 1)
        {
            failures.Add("SemanticThreshold must be between 0 and 1.");
        }
    }
}
