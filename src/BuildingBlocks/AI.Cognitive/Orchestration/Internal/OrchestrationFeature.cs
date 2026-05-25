using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// Orchestration feature marker and registration hub.
/// </summary>
internal sealed partial class OrchestrationFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKOrchestrationOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKThoughtStream, DefaultThoughtStream>();
        services.TryAddScoped<IVKCognitivePipeline, DefaultCognitivePipeline>();
        services.TryAddScoped<IVKIntentNexus, DefaultIntentNexus>();

        // Background Auditing
        services.TryAddSingleton<IVKAuditSynapseQueue, DefaultAuditSynapseQueue>();
        services.AddHostedService<AuditSynapseWorker>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKOrchestrationOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
