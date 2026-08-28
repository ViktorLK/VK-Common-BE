using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Synapse.Resilience.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKAIResilienceOptions))]
internal sealed partial class AIResilienceFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIResilienceOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAIResilienceProvider, LocalAIResilienceProvider>();
    }
}
