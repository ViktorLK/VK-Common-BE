using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cortex.LoopOrchestration.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Feature marker and DI registration for Loop Orchestration slice.
/// </summary>
[VKFeature(typeof(VKAICortexBlock), OptionsType = typeof(VKLoopOrchestrationOptions))]
internal sealed partial class LoopOrchestrationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKLoopOrchestrationOptions options)
    {
        services.TryAddScoped<IVKAgentLoopOrchestrator, DefaultAgentLoopOrchestrator>();
    }
}
