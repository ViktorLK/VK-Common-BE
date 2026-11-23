using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Text.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Text feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAITextOptions))]
internal sealed partial class TextFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAITextOptions options)
    {
        _ = options;
        VKAIMEAIBlock.EnsureChatClientRegistered(services);
        services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, DefaultTextEngine>());
    }
}
