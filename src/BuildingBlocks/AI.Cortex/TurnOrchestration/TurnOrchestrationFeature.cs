using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cortex.TurnOrchestration.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Feature marker and DI registration for Turn Orchestration slice.
/// </summary>
[VKFeature(typeof(VKAICortexBlock), OptionsType = typeof(VKTurnOrchestrationOptions))]
internal sealed partial class TurnOrchestrationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTurnOrchestrationOptions options)
    {
        services.TryAddScoped<IVKChatTurnOrchestrator, DefaultChatTurnOrchestrator>();
    }
}
