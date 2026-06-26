using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using VK.Blocks.AI.SemanticKernel.Agents.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Speech.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Transcription.Internal;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Filters;
using VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Plugins.Internal;
using VK.Blocks.AI.SemanticKernel.Text.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

/// <summary>
/// Industrial manual mappings for the Semantic Kernel building block.
/// </summary>
public static class VKAISKServiceCollectionExtensions
{
    /// <summary>
    /// Registers all internal implementations and engines for Semantic Kernel.
    /// This should be called alongside the SG-generated AddVKAISKBlock() method.
    /// </summary>
    public static IVKAISKBuilder AddVKAISKImplementations(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. HttpClient with Resilience
        services.AddHttpClient(AISKConstants.HttpClientName)
            .AddStandardResilienceHandler()
            .Configure(ConfigureResilience);

        // 2. Filters
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKDiagnosticsFilter>());

        services.TryAddScoped<IVKPrivacyFilter, RegexPrivacyFilter>();
        services.TryAddScoped<IVKInjectionDetector, RegexInjectionDetector>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKPrivacyFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKInjectionFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, VKSensitiveContentFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKTokenicsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKTokenicsFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAutoFunctionInvocationFilter, AISKAutoFunctionFilter>());

        // 3. Infrastructure & Plugin Providers
        services.AddMemoryCache();
        services.TryAddSingleton<IAISKPluginProvider, AISKConfigPluginProvider>();

        // 4. Kernel Factory
        services.TryAddScoped<IAISKKernelFactory, AISKKernelFactory>();
        // Note: Caching decoration should be driven by Defaults Options at runtime.
        services.Decorate<IAISKKernelFactory>((inner, provider) =>
        {
            var options = provider.GetRequiredService<IOptions<VKAISKDefaultsOptions>>().Value;
            return options.EnableKernelCaching
                ? new AISKCachedKernelFactory(
                    inner,
                    provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                    provider.GetRequiredService<IVKAISKOptionsProvider>(),
                    provider.GetRequiredService<IOptions<VKAIDefaultsOptions>>(),
                    provider.GetRequiredService<IVKChatOptionsProvider>(),
                    provider.GetRequiredService<IOptions<VKEmbeddingsOptions>>())
                : inner;
        });

        services.TryAddScoped(sp => sp.GetRequiredService<IAISKKernelFactory>().CreateKernel());

        // 5. Modern Vector Store (SK Native)
        services.TryAddSingleton<Microsoft.Extensions.VectorData.VectorStore, Microsoft.SemanticKernel.Connectors.InMemory.InMemoryVectorStore>();

        // 6. Feature Engines Mapping (Replace NoOp implementations registered by VK.Blocks.AI)
        services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, AISKChatEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, AISKEmbeddingEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, AISKTextEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKSpeechEngine, AISKSpeechEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTranscriptionEngine, AISKTranscriptionEngine>());

        // Agents Mapping
        services.TryAddSingleton<AISKAgentToolAdapter>();
        services.TryAddScoped<IVKAgentFactory, AISKAgentFactory>();
        services.TryAddScoped<IVKAgentGroup, AISKAgentGroupRunner>();
        // services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKAtomicTool, VK.Blocks.AI.SemanticKernel.AtomicTools.Internal.WebSearchTool>());

        var builder = new AISKBlockBuilder(services, configuration);
        builder.AddVKDefaults();

        return builder;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider sp)
    {
        var chatOptions = sp.GetRequiredService<IOptions<VKChatOptions>>().Value;
        var globalOptions = sp.GetRequiredService<IOptions<VKAIDefaultsOptions>>().Value;

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
