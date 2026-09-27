using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.IngressText.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// IngressText feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKIngressTextOptions))]
internal sealed partial class IngressTextFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngressTextOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKTextSplitter, DefaultTextSplitter>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressTextPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressDocumentPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressTextOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
