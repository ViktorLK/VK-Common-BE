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

        services.TryAddSingleton<IVKPsycheKnowledgeRepository, InMemoryKnowledgeRepository>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultKnowledgeStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultKnowledgeFinalizerStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKKnowledgeOptions options, List<string> failures)
    {
        if (options.MaxEntriesToInject is <= 0)
        {
            failures.Add("MaxEntriesToInject, if set, must be greater than zero.");
        }

        if (options.ReservedTokens is <= 0)
        {
            failures.Add("ReservedTokens, if set, must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.DefaultXmlTag))
        {
            failures.Add("DefaultXmlTag must not be null or whitespace.");
        }
    }
}
