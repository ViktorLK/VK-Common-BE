using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Chat feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIChatOptions))]
internal sealed partial class ChatFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIChatOptions options)
    {
        _ = options;
        VKAIMEAIBlock.EnsureChatClientRegistered(services);
        VKAIMEAIBlock.ReplaceChatEnginePreservingDecorator(services);
    }
}
