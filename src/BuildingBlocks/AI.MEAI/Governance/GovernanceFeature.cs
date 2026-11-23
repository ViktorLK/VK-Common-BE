using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Governance.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Governance feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIGovernanceOptions))]
internal sealed partial class GovernanceFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIGovernanceOptions options)
    {
        _ = options;
        VKAIMEAIBlock.ReplaceIfFallbackOrAddSingleton<IVKTokenCounter, TiktokenTokenCounter>(services);
        services.TryAddSingleton<IVKTokenizer, TiktokenTokenCounter>();
    }
}
