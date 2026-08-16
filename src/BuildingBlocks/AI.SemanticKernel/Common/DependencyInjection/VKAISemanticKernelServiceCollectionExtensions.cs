using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Agents.Internal;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
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
    public static void AddVKAISemanticKernelImplementations(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. HttpClient with Standard Resilience
        services.AddHttpClient(AISemanticKernelConstants.HttpClientName)
            .AddStandardResilienceHandler();

        // 2. Filters
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISemanticKernelDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelDiagnosticsFilter>());

        services.TryAddScoped<IVKPrivacyFilter, RegexPrivacyFilter>();
        services.TryAddScoped<IVKInjectionDetector, RegexInjectionDetector>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelPrivacyFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelInjectionFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISemanticKernelTokenicsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISemanticKernelTokenicsFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAutoFunctionInvocationFilter, AISemanticKernelAutoFunctionFilter>());

        // 3. Infrastructure & Plugin Providers
        services.AddMemoryCache();
        services.TryAddScoped<IAISemanticKernelKernelFactory, AISemanticKernelKernelFactory>();
        services.Decorate<IAISemanticKernelKernelFactory, AISemanticKernelCachedKernelFactory>();
        services.TryAddScoped<Microsoft.SemanticKernel.Kernel>(sp => sp.GetRequiredService<IAISemanticKernelKernelFactory>().CreateKernel());

        // 4. Default Engines (Replace NoOp implementations registered by VK.Blocks.AI)
        services.Replace(ServiceDescriptor.Scoped<IVKChatEngine, AISemanticKernelChatEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKTextEngine, AISemanticKernelTextEngine>());
        services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, AISemanticKernelEmbeddingEngine>());

        // 5. Multi-Provider Registrations (Keyed Services)
        foreach (VKAIProviderType providerType in Enum.GetValues<VKAIProviderType>())
        {
            services.AddVKKeyedChatEngine<AISemanticKernelChatEngine>(providerType);
            services.AddVKKeyedTextEngine<AISemanticKernelTextEngine>(providerType);
        }

        // 6. Caching & Idempotency Decorators
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
    }
}
