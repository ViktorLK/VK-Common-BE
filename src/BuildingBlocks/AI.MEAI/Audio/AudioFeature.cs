#pragma warning disable MEAI001

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Audio.Internal;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Audio feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIAudioOptions))]
internal sealed partial class AudioFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIAudioOptions options)
    {
        _ = options;
        VKAIMEAIBlock.TryAddDefaultAndKeyedClient<ITextToSpeechClient>(
            services,
            static (providerOptions, _, sp) => AIMEAIClientFactory.CreateTextToSpeechClient(providerOptions, sp));

        VKAIMEAIBlock.TryAddDefaultAndKeyedClient<ISpeechToTextClient>(
            services,
            static (providerOptions, _, sp) => AIMEAIClientFactory.CreateSpeechToTextClient(providerOptions, sp));

        services.Replace(ServiceDescriptor.Scoped<IVKSpeechEngine, DefaultSpeechEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTranscriptionEngine, DefaultTranscriptionEngine>());
    }
}
