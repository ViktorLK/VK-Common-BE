using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Framing.Internal;

/// <summary>
/// Core Framing feature registration and validation.
/// Follows BB.03 and BB.06.
/// </summary>
internal sealed partial class FramingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKFramingOptions options)
    {
        // Register the 500-level cognitive framing pipeline stage
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKOrchestrationPipelineStage, DefaultFramingPipelineStage>());
    }

    static partial void ValidateCustom(VKFramingOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.DefaultTokenLimit <= 0)
        {
            failures.Add("VKFramingOptions.DefaultTokenLimit must be greater than zero.");
        }

        if (options.TruncationThreshold <= 0.0f || options.TruncationThreshold > 1.0f)
        {
            failures.Add("VKFramingOptions.TruncationThreshold must be between 0.0 and 1.0.");
        }
    }
}
