using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Engram.Revision.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Revision feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKRevisionOptions))]
internal sealed partial class RevisionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRevisionOptions options)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultRevisionStage>());
        services.TryAddScoped<IVKRevisionService, DefaultRevisionService>();
        services.TryAddScoped<IVKContradictionArbitrator, DefaultContradictionArbitrator>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKRevisionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
