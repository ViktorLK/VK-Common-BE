using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Directive feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKDirectiveOptions))]
internal sealed partial class DirectiveFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDirectiveOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKDirectiveStore, InMemoryDirectiveStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultDirectiveStage>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultDirectiveFormatter>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKDirectiveOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
