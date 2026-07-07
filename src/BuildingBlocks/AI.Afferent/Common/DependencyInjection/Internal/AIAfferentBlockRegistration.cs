using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.Environment.Internal;
using VK.Blocks.AI.Afferent.IngressAudio.Internal;
using VK.Blocks.AI.Afferent.IngressGuardrails.Internal;
using VK.Blocks.AI.Afferent.IngressSensors.Internal;
using VK.Blocks.AI.Afferent.IngressText.Internal;
using VK.Blocks.AI.Afferent.IngressTokenics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class AIAfferentBlockRegistration
{
    // [SG Hook]
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
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressGuardrailsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, EnvironmentPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressSensorsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressVisionPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressTextPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressDocumentPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressAudioPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressTokenicsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, IngressRateLimitPipelineStage>());
    }
}
