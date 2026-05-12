using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.Plugins.Internal;
using VK.Blocks.AI.SemanticKernel.Retrieval.Internal;
using VK.Blocks.AI.SemanticKernel.Text.Internal;
using VK.Blocks.AI.Text;
using VK.Blocks.AI.Text.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Semantic Kernel building block.
/// </summary>
internal static class AISKBlockRegistration
{
    public static IVKAISKBuilder Register(
        IServiceCollection services,
        IConfiguration? configuration = null,
        Func<VKAISKOptions, VKAISKOptions>? configure = null)
    {
        VKGuard.NotNull(services);

        IVKAISKBuilder builder = new AISKBlockBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKAISKBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAISKOptions options = services.AddVKBlockOptions(configuration!, configure);

        // Register core and feature sub-options
        services.AddVKBlockOptions<VKAIOptions>(configuration!);
        services.AddVKBlockOptions<VKChatOptions>(configuration!);
        services.AddVKBlockOptions<VKEmbeddingOptions>(configuration!);
        services.AddVKBlockOptions<VKRetrievalOptions>(configuration!);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKAISKBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAISKOptions>, AISKOptionsValidator>());

        // 5. Diagnostics handled by Source Generator

        // 7. Diagnostics Filters
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKDiagnosticsFilter>());

        // 8. Dynamic Options Provider
        services.TryAddScoped<IVKAISKOptionsProvider, VKAISKDefaultOptionsProvider>();
        services.TryAddScoped<IVKAISKKernelFactory, VKAISKKernelFactory>();

        // 9. Plugin Providers
        services.TryAddSingleton<IVKAISKPluginProvider, AISKConfigPluginProvider>();

        // 10. Core Services
        RegisterCoreServices(services, configuration);

        return builder;
    }

    private static void RegisterCoreServices(IServiceCollection services, IConfiguration? configuration)
    {
        // 1. HttpClient with Resilience (Logic Down: Consuming VK Options)
        services.AddHttpClient(AISKConstants.HttpClientName)
            .AddStandardResilienceHandler()
            .Configure(ConfigureResilience);

        // 2. Kernel Registration (Delegated to Factory)
        services.TryAddScoped(sp => sp.GetRequiredService<IVKAISKKernelFactory>().CreateKernel());

        // 3. Feature Registrations with No-Op fallbacks
        var chatOptions = services.AddVKBlockOptions<VKChatOptions>(configuration!);
        if (chatOptions.Enabled)
        {
            services.TryAddScoped<IVKChatEngine, AISKChatEngine>();
        }
        else
        {
            services.TryAddScoped<IVKChatEngine, NoOpAISKChatEngine>();
        }

        var embedOptions = services.AddVKBlockOptions<VKEmbeddingOptions>(configuration!);
        if (embedOptions.Enabled)
        {
            services.TryAddScoped<IVKEmbeddingEngine, AISKEmbeddingEngine>();
        }
        else
        {
            services.TryAddScoped<IVKEmbeddingEngine, NoOpAISKEmbeddingEngine>();
        }

        var retrievalOptions = services.AddVKBlockOptions<VKRetrievalOptions>(configuration!);
        if (retrievalOptions.Enabled)
        {
            services.TryAddScoped<IVKRetrievalEngine, AISKRetrievalEngine>();
        }
        else
        {
            services.TryAddScoped<IVKRetrievalEngine, NoOpAISKRetrievalEngine>();
        }

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

    /// <summary>
    /// Configures the standard resilience handler using VK.Blocks AI options.
    /// Hierarchy: VKChatOptions (Feature-level) > VKAIOptions (Global-level).
    /// </summary>
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
        options.CircuitBreaker.MinimumThroughput = cbThreshold;

        var cbBreakDuration = chatOptions.CircuitBreakerBreakDuration ?? globalOptions.CircuitBreakerBreakDuration;
        options.CircuitBreaker.BreakDuration = cbBreakDuration;
    }
}
