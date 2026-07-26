using VK.Blocks.AI.Psyche.Directive.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Directive feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKDirectiveOptions))]
internal sealed partial class DirectiveFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDirectiveOptions options)
    {
        _ = options;
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
