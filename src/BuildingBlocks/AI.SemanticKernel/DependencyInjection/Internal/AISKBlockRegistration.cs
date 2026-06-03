using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using VK.Blocks.AI.SemanticKernel.Agents.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Speech.Internal;
using VK.Blocks.AI.SemanticKernel.Audio.Transcription.Internal;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.Filters;
using VK.Blocks.AI.SemanticKernel.Filters.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Plugins.Internal;
using VK.Blocks.AI.SemanticKernel.Retrieval.Internal;
using VK.Blocks.AI.SemanticKernel.Text.Internal;
using VK.Blocks.AI.SemanticKernel.Vectorics.ReRanking.Internal;
using VK.Blocks.AI.SemanticKernel.Vectorics.SemanticCache.Internal;
using VK.Blocks.AI.Text.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Industrial registration logic for the Semantic Kernel building block.
/// </summary>
internal static class AISKBlockRegistration
{
    public static IVKAISKBuilder Register(
        IServiceCollection services,
        IConfiguration? configuration = null,
        Func<VKAISKOptions, VKAISKOptions>? configure = null)
    {
        VKGuard.NotNull(services);

        var builder = new AISKBlockBuilder(services, configuration);

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<VKAISKBlock>())
        {
            return builder;
        }

        // 2. Initialize Options (dual-registration pattern)
        var options = services.AddVKBlockOptions<VKAISKOptions>(
            configuration!,
            configure);

        // 3. Register Marker (Source Generated Metadata)
        services.AddVKBlockMarker<VKAISKBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAISKOptions>, AISKOptionsValidator>());

        // 5. Diagnostics Filters (Level 1)
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKDiagnosticsFilter>());

        // 5.5 Guardrails Filters
        // Register the underlying engine for Privacy Filtering
        services.TryAddScoped<IVKPrivacyFilter, RegexPrivacyFilter>();

        // Register the underlying engine for Injection Detection
        services.TryAddScoped<IVKInjectionDetector, RegexInjectionDetector>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKPrivacyFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKInjectionFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, VKSensitiveContentFilter>());

        // 5.6 Tokenics Filter
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKTokenicsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKTokenicsFilter>());

        // 5.7 Auto Function Invocation Filter (Phase 2: IAutoFunctionInvocationFilter)
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAutoFunctionInvocationFilter, AISKAutoFunctionFilter>());

        // 6. Dynamic Options Provider (Public Interface)
        services.TryAddScoped<IVKAISKOptionsProvider, VKAISKDefaultOptionsProvider>();

        // 7. Plugin Providers (Internal)
        services.TryAddSingleton<IAISKPluginProvider, AISKConfigPluginProvider>();

        // 8. Infrastructure (Required for caching)
        services.AddMemoryCache();

        // 9. Kernel Factory (Internal with optional Caching decorator)
        services.TryAddScoped<IAISKKernelFactory, AISKKernelFactory>();
        if (options.EnableKernelCaching)
        {
            services.Decorate<IAISKKernelFactory, AISKCachedKernelFactory>();
        }

        // 9.5 Modern Vector Store
        services.TryAddSingleton<VectorStore, InMemoryVectorStore>();

        // 10. Core Services
        RegisterCoreServices(services, configuration);

        return builder;
    }

    private static void RegisterCoreServices(IServiceCollection services, IConfiguration? configuration)
    {
        // 1. HttpClient with Resilience
        services.AddHttpClient(AISKConstants.HttpClientName)
            .AddStandardResilienceHandler()
            .Configure(ConfigureResilience);

        // 2. Kernel Registration (Delegated to Factory)
        services.TryAddScoped(sp => sp.GetRequiredService<IAISKKernelFactory>().CreateKernel());

        // 3. Feature Registrations with No-Op fallbacks

        // Chat
        var chatOptions = services.AddVKBlockOptions<VKChatOptions>(configuration!);
        if (chatOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, AISKChatEngine>());
        }
        else
        {
            services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, NoOpAISKChatEngine>());
        }

        // Embedding
        var embedOptions = services.AddVKBlockOptions<VKEmbeddingsOptions>(configuration!);
        if (embedOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, AISKEmbeddingEngine>());
        }
        else
        {
            services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, NoOpAISKEmbeddingEngine>());
        }

        // Retrieval
        var retrievalOptions = services.AddVKBlockOptions<VKRetrievalOptions>(configuration!);
        if (retrievalOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKRetrievalEngine, AISKRetrievalEngine>());
        }
        else
        {
            services.Replace(ServiceDescriptor.Scoped<IVKRetrievalEngine, NoOpAISKRetrievalEngine>());
        }

        // ReRanking
        var reRankingOptions = services.AddVKBlockOptions<VKReRankingOptions>(configuration!);
        if (reRankingOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKReRanker, AISKReRankerEngine>());
        }
        else
        {
            services.Replace(ServiceDescriptor.Scoped<IVKReRanker, NoOpAISKReRankerEngine>());
        }

        // Agents
        var agentOptions = services.AddVKBlockOptions<VKAgentsOptions>(configuration!);
        if (agentOptions.Enabled)
        {
            services.TryAddSingleton<AISKAgentToolAdapter>();
            services.TryAddSingleton<IVKAgentFactory, AISKAgentFactory>();
            services.TryAddScoped<IVKAgentGroup, AISKAgentGroupRunner>();

            // Register built-in default tools
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKAtomicTool, VK.Blocks.AI.SemanticKernel.AtomicTools.Internal.WebSearchTool>());
        }

        // Text
        var textOptions = services.AddVKBlockOptions<VKTextOptions>(configuration!);
        if (textOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, AISKTextEngine>());
        }
        else
        {
            services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, NoOpVKTextEngine>());
        }

        // Speech
        var speechOptions = services.AddVKBlockOptions<VKSpeechOptions>(configuration!);
        if (speechOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKSpeechEngine, AISKSpeechEngine>());
        }

        // Transcription
        var transcriptionOptions = services.AddVKBlockOptions<VKTranscriptionOptions>(configuration!);
        if (transcriptionOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKTranscriptionEngine, AISKTranscriptionEngine>());
        }

        // Semantic Cache
        var semanticCacheOptions = services.AddVKBlockOptions<VKSemanticCacheOptions>(configuration!);
        if (semanticCacheOptions.Enabled)
        {
            services.Replace(ServiceDescriptor.Scoped<IVKSemanticCache, AISKSemanticCache>());
        }
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider sp)
    {
        var chatOptions = sp.GetRequiredService<IOptions<VKChatOptions>>().Value;
        var globalOptions = sp.GetRequiredService<IOptions<VKAIDefaultsOptions>>().Value;

        // 1. Retry Policy
        var retryCount = chatOptions.RetryCount ?? globalOptions.RetryCount;
        options.Retry.MaxRetryAttempts = retryCount;

        // 2. Total Request Timeout
        var timeout = chatOptions.Timeout ?? globalOptions.Timeout;
        options.TotalRequestTimeout.Timeout = timeout;

        // 3. Circuit Breaker
        var cbThreshold = chatOptions.CircuitBreakerThreshold ?? globalOptions.CircuitBreakerThreshold;
        options.CircuitBreaker.MinimumThroughput = cbThreshold * 10; // Simple mapping

        var cbBreakDuration = chatOptions.CircuitBreakerBreakDuration ?? globalOptions.CircuitBreakerBreakDuration;
        options.CircuitBreaker.BreakDuration = cbBreakDuration;
    }
}
