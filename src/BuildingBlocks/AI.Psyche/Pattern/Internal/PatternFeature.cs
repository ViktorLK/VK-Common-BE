using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

/// <summary>
/// Pattern feature marker and registration hub.
/// </summary>
internal sealed partial class PatternFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKPatternOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddSingleton<IVKPatternStore, InMemoryPatternStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, DefaultPatternStage>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPatternOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);
    }
}
