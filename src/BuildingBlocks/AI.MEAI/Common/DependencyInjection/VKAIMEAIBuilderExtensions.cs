using System.Linq;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Extension methods for customizing AI.Extensions building block registrations via <see cref="IVKAIMEAIBuilder"/>.
/// Adheres to AP.06 (Anti-overprotection: explicit builder overrides).
/// </summary>
public static partial class VKAIMEAIBuilderExtensions
{
    /// <summary>
    /// Overrides the default <see cref="IVKChatEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideChatEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKChatEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKChatEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKTextEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideTextEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKTextEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKTextEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKSpeechEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideSpeechEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKSpeechEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKSpeechEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKTranscriptionEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideTranscriptionEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKTranscriptionEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKTranscriptionEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKImageGenerationEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideImageGenerationEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKImageGenerationEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKImageGenerationEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKEmbeddingsEngine"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideEmbeddingsEngine<TEngine>(this IVKAIMEAIBuilder builder)
        where TEngine : class, IVKEmbeddingsEngine
    {
        VKGuard.NotNull(builder);
        builder.WithScoped<VKAIMEAIBlock, IVKEmbeddingsEngine, TEngine>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKPrivacyFilter"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverridePrivacyFilter<TFilter>(this IVKAIMEAIBuilder builder)
        where TFilter : class, IVKPrivacyFilter
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IVKPrivacyFilter, TFilter>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKInjectionDetector"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideInjectionDetector<TDetector>(this IVKAIMEAIBuilder builder)
        where TDetector : class, IVKInjectionDetector
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IVKInjectionDetector, TDetector>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKTokenCounter"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideTokenCounter<TCounter>(this IVKAIMEAIBuilder builder)
        where TCounter : class, IVKTokenCounter
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IVKTokenCounter, TCounter>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKAIContextPruner"/> implementation.
    /// </summary>
    [System.Obsolete("Context pruning belongs to AI.Psyche (OWN-PROMPT-BUDGET) and AI.AgentFramework (OWN-RUN-CONTEXT-REDUCTION). AI.MEAI enforces hard request caps only (OWN-REQUEST-CAP, MEA-04).")]
    public static IVKAIMEAIBuilder OverrideContextPruner<TPruner>(this IVKAIMEAIBuilder builder)
        where TPruner : class, IVKAIContextPruner
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IVKAIContextPruner, TPruner>();
        return builder;
    }

    /// <summary>
    /// Overrides the default <see cref="IVKAICache"/> implementation.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideAICache<TCache>(this IVKAIMEAIBuilder builder)
        where TCache : class, IVKAICache
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IVKAICache, TCache>();
        return builder;
    }

    /// <summary>
    /// Overrides the singleton <see cref="IChatClient"/> registration.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideChatClient<TClient>(this IVKAIMEAIBuilder builder)
        where TClient : class, IChatClient
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IChatClient, TClient>();
        return builder;
    }

    /// <summary>
    /// Overrides the singleton <see cref="IEmbeddingGenerator{String, Embedding}"/> registration.
    /// </summary>
    public static IVKAIMEAIBuilder OverrideEmbeddingGenerator<TGenerator>(this IVKAIMEAIBuilder builder)
        where TGenerator : class, IEmbeddingGenerator<string, Embedding<float>>
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAIMEAIBlock, IEmbeddingGenerator<string, Embedding<float>>, TGenerator>();
        return builder;
    }

    /// <summary>
    /// Registers the OpenAI provider adapter for AI.MEAI.
    /// </summary>
    public static IVKAIMEAIBuilder AddOpenAI(this IVKAIMEAIBuilder builder)
        => AddProvider<OpenAIProviderAdapter>(builder);

    /// <summary>
    /// Registers the Azure OpenAI provider adapter for AI.MEAI.
    /// </summary>
    public static IVKAIMEAIBuilder AddAzureOpenAI(this IVKAIMEAIBuilder builder)
        => AddProvider<AzureOpenAIProviderAdapter>(builder);

    /// <summary>
    /// Registers the Ollama provider adapter for AI.MEAI.
    /// </summary>
    public static IVKAIMEAIBuilder AddOllama(this IVKAIMEAIBuilder builder)
        => AddProvider<OllamaProviderAdapter>(builder);

    /// <summary>
    /// Registers the Google Gemini provider adapter for AI.MEAI.
    /// </summary>
    public static IVKAIMEAIBuilder AddGoogle(this IVKAIMEAIBuilder builder)
        => AddProvider<GoogleProviderAdapter>(builder);

    /// <summary>
    /// Registers an OpenAI-compatible provider adapter (e.g. Custom, DeepSeek, XAI, Mistral) for AI.MEAI.
    /// </summary>
    public static IVKAIMEAIBuilder AddOpenAICompatible(
        this IVKAIMEAIBuilder builder,
        VKAIProviderType provider = VKAIProviderType.Custom)
    {
        VKGuard.NotNull(builder);
        var alreadyRegistered = Enumerable.Any(
            builder.Services,
            d => d.ServiceType == typeof(IMEAIProviderAdapter) &&
                 d.ImplementationInstance is IMEAIProviderAdapter adapter &&
                 adapter.Provider == provider);
        if (!alreadyRegistered)
        {
            builder.Services.Add(ServiceDescriptor.Singleton<IMEAIProviderAdapter>(new OpenAICompatibleProviderAdapter(provider)));
        }

        return builder;
    }

    /// <summary>
    /// Registers all built-in AI.MEAI provider adapters.
    /// </summary>
    public static IVKAIMEAIBuilder AddAllProviders(this IVKAIMEAIBuilder builder)
        => builder.AddOpenAI().AddAzureOpenAI().AddOllama().AddGoogle().AddOpenAICompatible();

    private static IVKAIMEAIBuilder AddProvider<TAdapter>(IVKAIMEAIBuilder builder)
        where TAdapter : class, IMEAIProviderAdapter
    {
        VKGuard.NotNull(builder);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMEAIProviderAdapter, TAdapter>());
        return builder;
    }
}
