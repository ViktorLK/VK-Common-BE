using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Streaming.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKStreamingOptions))]
internal sealed partial class StreamingFeature // [AP.01]
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKStreamingOptions options)
    {
        services.TryAddSingleton<IVKStreamingParser, DefaultStreamingParser>();
    }
}
