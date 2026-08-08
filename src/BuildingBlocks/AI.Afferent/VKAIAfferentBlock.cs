using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.Environment.Internal;
using VK.Blocks.AI.Afferent.IngressAudio.Internal;
using VK.Blocks.AI.Afferent.IngressGuardrails.Internal;
using VK.Blocks.AI.Afferent.IngressSensors.Internal;
using VK.Blocks.AI.Afferent.IngressText.Internal;
using VK.Blocks.AI.Afferent.IngressTokenics.Internal;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Afferent;

[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKAIAfferentBlock
{

    static partial void RegisterBlockCustom(IVKAIAfferentBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Additional sub-feature options
        services.AddVKBlockOptions<VKEnvironmentOptions>(configuration);
        services.AddVKBlockOptions<VKIngressSensorsOptions>(configuration);
        services.AddVKBlockOptions<VKIngressAudioOptions>(configuration);
        services.AddVKBlockOptions<VKIngressTextOptions>(configuration);
        services.AddVKBlockOptions<VKIngressTokenicsOptions>(configuration);
        services.AddVKBlockOptions<VKIngressGuardrailsOptions>(configuration);

        // Core Services & Pipeline Stages Registration
        services.TryAddScoped<IVKIngressAudioService, DefaultIngressAudioService>();
        services.TryAddScoped<IVKTextSplitter, DefaultTextSplitter>();
        services.TryAddScoped<IVKIngressGuardrail, DefaultIngressGuardrail>();
        services.TryAddScoped<IVKEnvironmentPerceptionProvider, DefaultEnvironmentPerceptionProvider>();
        services.TryAddSingleton<IVKSystemEventDispatcher, DefaultSystemEventDispatcher>();

        // Pipeline stages
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressGuardrailsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EnvironmentPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressSensorsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressVisionPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressTextPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressDocumentPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressAudioPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressTokenicsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressRateLimitPipelineStage>());
    }

}
