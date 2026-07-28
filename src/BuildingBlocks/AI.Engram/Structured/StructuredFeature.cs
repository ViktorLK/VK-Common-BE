using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Engram.Structured.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Structured memory feature marker and registration hub.
/// Follows BB.06 (Modular Feature Pattern).
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKStructuredOptions))]
internal sealed partial class StructuredFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKStructuredOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKFactSensitivityPolicy, DefaultFactSensitivityPolicy>();
        services.TryAddSingleton<IVKFactCapacityPolicy, DefaultFactCapacityPolicy>();
        services.TryAddScoped<IVKStructuredMemoryStore, InMemoryStructuredMemoryStore>();
        services.TryAddScoped<DefaultStructuredInjectionStage>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<VK.Blocks.AI.Psyche.IVKPsychePipelineStage, DefaultStructuredInjectionStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKStructuredOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.MaxFactsPerTenant <= 0)
        {
            failures.Add("MaxFactsPerTenant must be greater than zero.");
        }
    }
}
