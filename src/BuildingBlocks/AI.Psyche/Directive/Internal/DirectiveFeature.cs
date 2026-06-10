using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Directive feature marker and registration hub.
/// </summary>
internal sealed partial class DirectiveFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKDirectiveOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKDirectiveStore, InMemoryDirectiveStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, DefaultDirectiveStage>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultDirectiveFormatter>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKDirectiveOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
