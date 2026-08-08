using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Agents.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Speech.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Transcription.Internal;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Filters;
using VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Plugins.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.ImageGeneration.Internal;
using VK.Blocks.AI.SemanticKernel.Text.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

/// <summary>
/// Industrial manual mappings for the Semantic Kernel building block.
/// </summary>
public static class VKAISemanticKernelServiceCollectionExtensions
{
    /// <summary>
    /// Registers all internal implementations and engines for Semantic Kernel.
    /// This should be called alongside the SG-generated AddVKAISemanticKernelBlock() method.
    /// </summary>
    public static IVKAISemanticKernelBuilder AddVKAISemanticKernelImplementations(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. HttpClient with Resilience
        services.AddHttpClient(AISemanticKernelConstants.HttpClientName)
            .AddStandardResilienceHandler()
            .Configure(ConfigureResilience);

        // 2. Filters
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISemanticKernelDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelDiagnosticsFilter>());

        services.TryAddScoped<IVKPrivacyFilter, RegexPrivacyFilter>();
        services.TryAddScoped<IVKInjectionDetector, RegexInjectionDetector>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelPrivacyFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelInjectionFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, VKSensitiveContentFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelTokenicsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISemanticKernelTokenicsFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAutoFunctionInvocationFilter, AISemanticKernelAutoFunctionFilter>());

        // 3. Infrastructure & Plugin Providers
        services.AddMemoryCache();
        services.TryAddSingleton<IAISemanticKernelPluginProvider, AISemanticKernelConfigPluginProvider>();

        // 4. Kernel Factory
        services.TryAddScoped<IAISemanticKernelKernelFactory, AISemanticKernelKernelFactory>();
        // Note: Caching decoration should be driven by Defaults Options at runtime.
        services.Decorate<IAISemanticKernelKernelFactory>((inner, provider) =>
        {
            var options = provider.GetRequiredService<IOptions<VKAISemanticKernelOptions>>().Value;
            return options.EnableKernelCaching
                ? new AISemanticKernelCachedKernelFactory(
                    inner,
                    provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                    provider.GetRequiredService<IVKAISemanticKernelOptionsProvider>(),
                    provider.GetRequiredService<IOptions<VKAIOptions>>(),
                    provider.GetRequiredService<IVKChatOptionsProvider>(),
                    provider.GetRequiredService<IOptions<VKEmbeddingsOptions>>())
                : inner;
        });

        services.TryAddScoped(sp => sp.GetRequiredService<IAISemanticKernelKernelFactory>().CreateKernel());

        // 5. Modern Vector Store (SK Native)
        services.TryAddSingleton<Microsoft.Extensions.VectorData.VectorStore, Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore>();

        // 6. Feature Engines Mapping (Replace NoOp implementations registered by VK.Blocks.AI)
        services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, AISemanticKernelChatEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, AISemanticKernelEmbeddingEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, AISemanticKernelTextEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKImageGenerationEngine, AISemanticKernelImageGenerationEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKSpeechEngine, AISemanticKernelSpeechEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTranscriptionEngine, AISemanticKernelTranscriptionEngine>());

        // 7. Caching & Idempotency Decorators
        services.Decorate<IVKChatEngine>((inner, provider) =>
        {
            var cache = provider.GetService<IVKAICache>();
            if (cache is null)
                return inner;

            var cached = new VKChatCachingDecorator(inner, cache);
            return new VKChatIdempotencyDecorator(cached, cache);
        });

        services.Decorate<IVKTextEngine>((inner, provider) =>
        {
            var cache = provider.GetService<IVKAICache>();
            if (cache is null)
                return inner;

            var cached = new VKTextCachingDecorator(inner, cache);
            return new VKTextIdempotencyDecorator(cached, cache);
        });

        // Agents Mapping
        services.TryAddSingleton<AISemanticKernelAgentToolAdapter>();
        services.TryAddScoped<IVKAgentFactory, AISemanticKernelAgentFactory>();
        services.TryAddScoped<IVKAgentGroup, AISemanticKernelAgentGroupRunner>();
        services.AddHostedService<VK.Blocks.AI.SemanticKernel.Common.Routing.Internal.VKAIProbeBackgroundService>();

        var builder = new AISemanticKernelBlockBuilder(services, configuration);

        return builder;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider sp)
    {
        var chatOptions = sp.GetRequiredService<IOptions<VKChatOptions>>().Value;
        var globalOptions = sp.GetRequiredService<IOptions<VKAIOptions>>().Value;

        var retryCount = chatOptions.RetryCount ?? globalOptions.RetryCount;
        options.Retry.MaxRetryAttempts = retryCount;

        var timeout = chatOptions.Timeout ?? globalOptions.Timeout;
        options.TotalRequestTimeout.Timeout = timeout;
        options.AttemptTimeout.Timeout = timeout;

        var cbThreshold = chatOptions.CircuitBreakerThreshold ?? globalOptions.CircuitBreakerThreshold;
        options.CircuitBreaker.MinimumThroughput = cbThreshold * 10;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(timeout.TotalSeconds * 2.2);

        var cbBreakDuration = chatOptions.CircuitBreakerBreakDuration ?? globalOptions.CircuitBreakerBreakDuration;
        options.CircuitBreaker.BreakDuration = cbBreakDuration;
    }
}
