using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Negotiation.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKNegotiationOptions))]
internal sealed partial class NegotiationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKNegotiationOptions options)
    {
        services.TryAddSingleton<IVKContractNegotiator, DefaultContractNegotiator>();
        services.TryAddSingleton<IVKProviderCapabilityDetector, BasicProviderCapabilityDetector>();
        services.TryAddSingleton<IVKContractProjector, DefaultContractProjector>();

        // Register Before Stage in Psyche Pipeline
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPsychePipelineStage, DefaultNegotiationStage>());
    }
}
