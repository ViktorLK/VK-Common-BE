using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Afferent.Audio.Internal;
using VK.Blocks.AI.Afferent.Guardrails.Internal;
using VK.Blocks.AI.Afferent.Text.Internal;
using VK.Blocks.AI.Afferent.Tokenics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the AI.Afferent block.
/// Complies with BB.03.
/// </summary>
internal static class AIAfferentBlockRegistration
{
    internal static IVKAIAfferentBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIAfferentOptions, VKAIAfferentOptions>? configure = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. Check-Self & Prerequisite Check
        if (services.IsVKBlockRegistered<VKAIAfferentBlock>())
        {
            return new AIAfferentBlockBuilder(services, configuration);
        }

        // 2. Options Registration
        VKAIAfferentOptions options = services.AddVKBlockOptions<VKAIAfferentOptions>(configuration, configure);

        // 3. Mark-Self (MUST be called BEFORE early exit)
        services.AddVKBlockMarker<VKAIAfferentBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKAIAfferentOptions>, AIAfferentOptionsValidator>();

        // 5. Diagnostics & Metadata

        var builder = new AIAfferentBlockBuilder(services, configuration);

        // 6. Feature Toggle (early exit if disabled)
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services & Pipeline Stages Registration
        services.TryAddScoped<IVKAudioTranscriber, DefaultAudioTranscriber>();
        services.TryAddScoped<IVKTextSplitter, DefaultTextSplitter>();
        services.TryAddScoped<IVKGuardrail, DefaultGuardrail>();

        // Pipeline stages
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentGuardrailsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentVisionPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentTextPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentDocumentPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentAudioPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentTokenicsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, AfferentRateLimitPipelineStage>());

        return builder;
    }
}
