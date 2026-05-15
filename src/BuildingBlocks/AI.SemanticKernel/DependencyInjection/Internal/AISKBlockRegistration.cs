using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Plugins.Internal;
using VK.Blocks.AI.SemanticKernel.Retrieval.Internal;
using VK.Blocks.AI.SemanticKernel.Agents.Internal;
using VK.Blocks.AI.SemanticKernel.Text.Internal;
using VK.Blocks.AI.Text;
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
            services.TryAddScoped<IVKChatEngine, AISKChatEngine>();
        }
        else
        {
            services.TryAddScoped<IVKChatEngine, NoOpAISKChatEngine>();
        }

        // Embedding
        var embedOptions = services.AddVKBlockOptions<VKEmbeddingOptions>(configuration!);
        if (embedOptions.Enabled)
        {
            services.TryAddScoped<IVKEmbeddingEngine, AISKEmbeddingEngine>();
        }
        else
        {
            services.TryAddScoped<IVKEmbeddingEngine, NoOpAISKEmbeddingEngine>();
        }

        // Retrieval
        var retrievalOptions = services.AddVKBlockOptions<VKRetrievalOptions>(configuration!);
        if (retrievalOptions.Enabled)
        {
            services.TryAddScoped<IVKRetrievalEngine, AISKRetrievalEngine>();
        }
        else
        {
            services.TryAddScoped<IVKRetrievalEngine, NoOpAISKRetrievalEngine>();
        }

        // Agents
        var agentOptions = services.AddVKBlockOptions<VKAgentOptions>(configuration!);
        if (agentOptions.Enabled)
        {
            services.TryAddSingleton<AISKAgentToolAdapter>();
            services.TryAddTransient<IVKAgent>(sp => new AISKAgent(
                sp.GetRequiredService<Microsoft.SemanticKernel.Kernel>(),
                sp.GetRequiredService<IOptions<VKAISKOptions>>().Value.DeploymentName ?? "Unknown",
                "DefaultAgent",
                sp.GetRequiredService<IOptions<VKAgentOptions>>()));
        }

        // Text
        var textOptions = services.AddVKBlockOptions<VKTextOptions>(configuration!);
        if (textOptions.Enabled)
        {
            services.TryAddScoped<IVKTextEngine, AISKTextEngine>();
        }
        else
        {
            services.TryAddScoped<IVKTextEngine, NoOpVKTextEngine>();
        }
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider sp)
    {
        var chatOptions = sp.GetRequiredService<IOptions<VKChatOptions>>().Value;
        var globalOptions = sp.GetRequiredService<IOptions<VKAIOptions>>().Value;

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
