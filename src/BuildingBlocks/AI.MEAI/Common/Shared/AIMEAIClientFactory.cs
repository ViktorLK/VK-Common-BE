#pragma warning disable MEAI001

using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Microsoft;
using OllamaSharp;
using OpenAI;
using VK.Blocks.AI.MEAI.Chat.Internal;
using VK.Blocks.AI.MEAI.Embeddings.Internal;
using VK.Blocks.AI.MEAI.Governance.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Factory for building MEAI IChatClient, IEmbeddingGenerator, and related AI clients from typed options
/// wrapped with a deterministic pipeline of DelegatingChatClient middlewares.
/// Adheres to ARCH-05, OR.03, R1-R4, T1-T3, and G1-G3.
/// </summary>
internal static class AIMEAIClientFactory
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "The underlying IChatClient lifecycle is managed by ChatClientBuilder and the dependency injection container.")]
    public static IChatClient CreateChatClient(
        VKAIMEAIProviderSettings providerSettings,
        VKAIMEAIClientChainSettings chainSettings,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(providerSettings);
        VKGuard.NotNull(chainSettings);
        VKGuard.NotNull(serviceProvider);

        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = ResolveHttpClient(httpFactory, providerSettings.Provider);

        var adapter = ResolveAdapter(serviceProvider, providerSettings.Provider, requireExplicitRegistration: true);
        var baseClient = adapter.CreateChatClient(providerSettings, httpClient);

        // Wrap with pipeline middlewares adhering to deterministic order (ARCH-05, MEA-03, SYN-05)
        // Outer-to-inner order in ChatClientBuilder:
        // 1. OpenTelemetry & Audit logging (outermost: records logical call telemetry and audit)
        // 2. Idempotency & Caching (caches logical-call requests; bypasses tool invocation and routing on hit)
        // 3. Function invocation (executes tools on cache miss unless an upstream runtime already owns tool invocation)
        // 4. OutboundGuard & RequestCap (Prompt injection, Privacy PII masking, Token governance & hard request cap)
        // 5. Routing / Failover (resolves candidates, performs bounded failover per concrete LLM round)
        // 6. Per-Connection Transport (baseClient)
        var builder = new ChatClientBuilder(baseClient);

        // 1. OpenTelemetry & Audit
        if (chainSettings.EnableOpenTelemetry)
        {
            builder.UseOpenTelemetry(sourceName: VKAIMEAIDiagnosticsConstants.ServiceName, configure: t =>
            {
                t.EnableSensitiveData = false;
            });
        }

        if (chainSettings.EnableAudit)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(VKAIMEAIDiagnosticsConstants.ServiceName);
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            builder.Use(inner => new AuditChatClient(inner, logger, timeProvider));
        }

        // 2. Idempotency & Caching
        if (chainSettings.EnableCaching)
        {
            var cache = serviceProvider.GetRequiredService<IVKAICache>();
            var serializer = serviceProvider.GetRequiredService<IVKJsonSerializer>();
            builder.Use(inner => new IdempotencyChatClient(inner, cache, serializer));
        }

        // 3. Function invocation (single ownership gate per G3)
        if (chainSettings.EnableFunctionInvocation)
        {
            builder.Use(inner =>
            {
                var invoking = new ChatClientBuilder(inner).UseFunctionInvocation().Build();
                return new FunctionInvocationGateChatClient(inner, invoking);
            });
        }

        // 4. OutboundGuard & RequestCap (Prompt injection, Privacy, Token governance)
        if (chainSettings.EnableInjectionGuard)
        {
            var injectionDetector = serviceProvider.GetRequiredService<IVKInjectionDetector>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(VKAIMEAIDiagnosticsConstants.ServiceName);
            builder.Use(inner => new PromptInjectionChatClient(inner, injectionDetector, logger));
        }

        if (chainSettings.EnablePrivacy)
        {
            var privacyFilter = serviceProvider.GetRequiredService<IVKPrivacyFilter>();
            builder.Use(inner => new PrivacyChatClient(inner, privacyFilter));
        }

        if (chainSettings.EnableTokenGovernance)
        {
            var tokenCounter = serviceProvider.GetService<IVKTokenCounter>();
            var rateLimiter = serviceProvider.GetService<IVKTokenRateLimiter>();
            var usageAggregator = serviceProvider.GetService<IVKTokenUsageAggregator>();
            if (tokenCounter is not null || rateLimiter is not null || usageAggregator is not null)
            {
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(VKAIMEAIDiagnosticsConstants.ServiceName);
                // [MEA-04] Hard request cap only; context pruning belongs to AI.Psyche and AI.AgentFramework.
                builder.Use(inner => new TokenGovernanceChatClient(
                    inner,
                    tokenCounter,
                    contextPruner: null,
                    rateLimiter,
                    usageAggregator,
                    chainSettings.MaxContextTokens,
                    logger));
            }
        }

        // 5. Routing / Failover (optional: wired when Synapse or routing services are registered)
        var routingPolicy = serviceProvider.GetService<IVKAIRoutingPolicy>();
        var invocationObserver = serviceProvider.GetService<IVKAIInvocationObserver>();
        var guidGenerator = serviceProvider.GetService<IVKGuidGenerator>();
        var routingTimeProvider = serviceProvider.GetService<TimeProvider>();
        if (routingPolicy is not null && invocationObserver is not null && guidGenerator is not null)
        {
            var meaiOptions = serviceProvider.GetService<IOptions<VKAIMEAIOptions>>()?.Value;
            var maxTotalAttempts = Math.Max(1, meaiOptions?.TransportResilience.MaxTotalAttempts ?? 4);
            var candidateClients = new ConcurrentDictionary<string, IChatClient>(StringComparer.OrdinalIgnoreCase);

            builder.Use(inner => new AIFailoverChatClient(
                inner,
                routingPolicy,
                invocationObserver,
                guidGenerator,
                routingTimeProvider,
                candidate => ResolveCandidateChatClient(
                    candidate,
                    inner,
                    providerSettings,
                    meaiOptions,
                    candidateClients,
                    httpFactory,
                    serviceProvider),
                maxTotalAttempts,
                onDispose: () => DisposeCandidateClients(candidateClients, inner)));
        }

        return builder.Build();
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "The underlying IEmbeddingGenerator lifecycle is managed by the dependency injection container.")]
    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(
        VKAIMEAIProviderSettings providerSettings,
        VKAIMEAIClientChainSettings chainSettings,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(providerSettings);
        VKGuard.NotNull(chainSettings);
        VKGuard.NotNull(serviceProvider);

        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = ResolveHttpClient(httpFactory, providerSettings.Provider);

        var adapter = ResolveAdapter(serviceProvider, providerSettings.Provider, requireExplicitRegistration: false);
        var baseGenerator = adapter.CreateEmbeddingGenerator(providerSettings, httpClient);

        // Wrap with pipeline middlewares adhering to deterministic order (ARCH-05, ADR-021)
        var builder = baseGenerator.AsBuilder();

        if (chainSettings.EnablePrivacy)
        {
            var privacyFilter = serviceProvider.GetRequiredService<IVKPrivacyFilter>();
            builder.Use(inner => new PrivacyEmbeddingGenerator(inner, privacyFilter));
        }

        if (chainSettings.EnableOpenTelemetry)
        {
            builder.UseOpenTelemetry(sourceName: VKAIMEAIDiagnosticsConstants.ServiceName);
        }

        if (chainSettings.EnableAudit)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(VKAIMEAIDiagnosticsConstants.ServiceName);
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            builder.Use(inner => new AuditEmbeddingGenerator(inner, logger, timeProvider));
        }

        return builder.Build();
    }

    public static ITextToSpeechClient CreateTextToSpeechClient(
        VKAIMEAIProviderSettings options,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(serviceProvider);

        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = ResolveHttpClient(httpFactory, options.Provider);
        var adapter = ResolveAdapter(serviceProvider, options.Provider, requireExplicitRegistration: false);

        return adapter.CreateTextToSpeechClient(options, httpClient);
    }

    public static ISpeechToTextClient CreateSpeechToTextClient(
        VKAIMEAIProviderSettings options,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(serviceProvider);

        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = ResolveHttpClient(httpFactory, options.Provider);
        var adapter = ResolveAdapter(serviceProvider, options.Provider, requireExplicitRegistration: false);

        return adapter.CreateSpeechToTextClient(options, httpClient);
    }

    public static IImageGenerator CreateImageGenerator(
        VKAIMEAIProviderSettings options,
        IServiceProvider serviceProvider)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(serviceProvider);

        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = ResolveHttpClient(httpFactory, options.Provider);
        var adapter = ResolveAdapter(serviceProvider, options.Provider, requireExplicitRegistration: false);

        return adapter.CreateImageGenerator(options, httpClient);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "Resolved candidate IChatClient instances are cached and disposed with the AIFailoverChatClient pipeline.")]
    private static IChatClient ResolveCandidateChatClient(
        VKAIRouteCandidate candidate,
        IChatClient innerClient,
        VKAIMEAIProviderSettings currentProviderSettings,
        VKAIMEAIOptions? meaiOptions,
        ConcurrentDictionary<string, IChatClient> candidateClients,
        IHttpClientFactory httpFactory,
        IServiceProvider serviceProvider)
    {
        if (meaiOptions is null || meaiOptions.ProviderSettings.Count == 0)
        {
            return innerClient;
        }

        if (!TryResolveCandidateProviderSettings(candidate, meaiOptions, out var matchedKey, out var targetSettings))
        {
            return innerClient;
        }

        if (IsSameProviderSettings(targetSettings, currentProviderSettings))
        {
            return innerClient;
        }

        var candidateModel = !string.IsNullOrWhiteSpace(candidate.DeploymentName)
            ? candidate.DeploymentName
            : candidate.ModelId;
        var hasCustomCandidateModel = string.IsNullOrWhiteSpace(targetSettings.ChatModelId) && !string.IsNullOrWhiteSpace(candidateModel);
        var cacheKey = hasCustomCandidateModel
            ? $"{matchedKey}:{candidateModel}"
            : matchedKey;

        return candidateClients.GetOrAdd(cacheKey, _ =>
        {
            var effectiveSettings = hasCustomCandidateModel
                ? targetSettings with { ChatModelId = candidateModel }
                : targetSettings;
            var candidateHttpClient = ResolveHttpClient(httpFactory, effectiveSettings.Provider);
            var candidateAdapter = ResolveAdapter(serviceProvider, effectiveSettings.Provider, requireExplicitRegistration: true);
            return candidateAdapter.CreateChatClient(effectiveSettings, candidateHttpClient);
        });
    }

    private static bool TryResolveCandidateProviderSettings(
        VKAIRouteCandidate candidate,
        VKAIMEAIOptions meaiOptions,
        [NotNullWhen(true)] out string? matchedKey,
        [NotNullWhen(true)] out VKAIMEAIProviderSettings? matchedSettings)
    {
        if (!string.IsNullOrWhiteSpace(candidate.ConnectionId) &&
            meaiOptions.ProviderSettings.TryGetValue(candidate.ConnectionId, out var byConnectionId))
        {
            matchedKey = candidate.ConnectionId;
            matchedSettings = byConnectionId;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(candidate.Provider))
        {
            if (meaiOptions.ProviderSettings.TryGetValue(candidate.Provider, out var byProviderKey))
            {
                matchedKey = candidate.Provider;
                matchedSettings = byProviderKey;
                return true;
            }

            if (Enum.TryParse<VKAIProviderType>(candidate.Provider, ignoreCase: true, out var providerType))
            {
                foreach (var (key, settings) in meaiOptions.ProviderSettings)
                {
                    if (settings.Provider == providerType)
                    {
                        matchedKey = key;
                        matchedSettings = settings;
                        return true;
                    }
                }
            }
        }

        matchedKey = null;
        matchedSettings = null;
        return false;
    }

    private static bool IsSameProviderSettings(VKAIMEAIProviderSettings left, VKAIMEAIProviderSettings right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        return left.Provider == right.Provider &&
               string.Equals(left.Endpoint, right.Endpoint, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(left.ApiKey?.Reveal(), right.ApiKey?.Reveal(), StringComparison.Ordinal) &&
               string.Equals(left.OrganizationId, right.OrganizationId, StringComparison.Ordinal) &&
               string.Equals(left.ProjectId, right.ProjectId, StringComparison.Ordinal) &&
               string.Equals(left.ChatModelId, right.ChatModelId, StringComparison.OrdinalIgnoreCase) &&
               left.TimeoutSeconds == right.TimeoutSeconds;
    }

    private static HttpClient ResolveHttpClient(IHttpClientFactory httpFactory, VKAIProviderType provider)
        => httpFactory.CreateClient(VKAIMEAIDiagnosticsConstants.GetHttpClientName(provider))
           ?? httpFactory.CreateClient(VKAIMEAIDiagnosticsConstants.HttpClientName);

    private static IMEAIProviderAdapter ResolveAdapter(
        IServiceProvider serviceProvider,
        VKAIProviderType provider,
        bool requireExplicitRegistration)
    {
        var registered = System.Linq.Enumerable.ToList(serviceProvider.GetServices<IMEAIProviderAdapter>());
        var match = System.Linq.Enumerable.FirstOrDefault(registered, a => a.Provider == provider);
        if (match is not null)
        {
            return match;
        }

        if (requireExplicitRegistration || registered.Count > 0)
        {
            throw new NotSupportedException(
                $"Provider '{provider}' is not registered. Call Add{provider}() on the AI.MEAI builder.");
        }

        return provider switch
        {
            VKAIProviderType.OpenAI => new OpenAIProviderAdapter(),
            VKAIProviderType.AzureOpenAI => new AzureOpenAIProviderAdapter(),
            VKAIProviderType.Ollama => new OllamaProviderAdapter(),
            VKAIProviderType.Google => new GoogleProviderAdapter(),
            VKAIProviderType.Custom or VKAIProviderType.DeepSeek or VKAIProviderType.XAI or VKAIProviderType.Mistral => new OpenAICompatibleProviderAdapter(provider),
            _ => throw new NotSupportedException($"Provider '{provider}' is not supported.")
        };
    }

    // Shared OpenAI-compatible wire-protocol adapter helpers (R3, T2)
    internal static IChatClient CreateOpenAIWireChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
    {
        var rawModelId = options.Provider == VKAIProviderType.AzureOpenAI
            ? (options.ChatModelId ?? throw new InvalidOperationException("ChatModelId (Deployment Name) is required for Azure OpenAI."))
            : (options.ChatModelId ?? "gpt-4o-mini");
        var modelId = options.ResolveModelOrDeployment(rawModelId) ?? rawModelId;

        var client = CreateOpenAICompatibleClient(options, httpClient, "chat");
        return client.GetChatClient(modelId).AsIChatClient();
    }

    internal static IEmbeddingGenerator<string, Embedding<float>> CreateOpenAIWireEmbeddingGenerator(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var rawModelId = options.Provider == VKAIProviderType.AzureOpenAI
            ? (options.EmbeddingModelId ?? throw new InvalidOperationException("EmbeddingModelId (Deployment Name) is required for Azure OpenAI."))
            : (options.EmbeddingModelId ?? "text-embedding-3-small");
        var modelId = options.ResolveModelOrDeployment(rawModelId) ?? rawModelId;

        var client = CreateOpenAICompatibleClient(options, httpClient, "embeddings");
        return client.GetEmbeddingClient(modelId).AsIEmbeddingGenerator();
    }

    internal static ITextToSpeechClient CreateOpenAIWireTextToSpeechClient(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var rawModelId = options.SpeechModelId ?? options.ChatModelId ?? "tts-1";
        var modelId = options.ResolveModelOrDeployment(rawModelId) ?? rawModelId;
        var client = CreateOpenAICompatibleClient(options, httpClient, "text-to-speech");
        return client.GetAudioClient(modelId).AsITextToSpeechClient();
    }

    internal static ISpeechToTextClient CreateOpenAIWireSpeechToTextClient(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var rawModelId = options.TranscriptionModelId ?? options.ChatModelId ?? "whisper-1";
        var modelId = options.ResolveModelOrDeployment(rawModelId) ?? rawModelId;
        var client = CreateOpenAICompatibleClient(options, httpClient, "speech-to-text");
        return client.GetAudioClient(modelId).AsISpeechToTextClient();
    }

    internal static IImageGenerator CreateOpenAIWireImageGenerator(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var rawModelId = options.ImageModelId ?? options.ChatModelId ?? "dall-e-3";
        var modelId = options.ResolveModelOrDeployment(rawModelId) ?? rawModelId;
        var client = CreateOpenAICompatibleClient(options, httpClient, "image generation");
        return client.GetImageClient(modelId).AsIImageGenerator();
    }

    private static OpenAIClient CreateOpenAICompatibleClient(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient,
        string operationContext)
    {
        var apiKey = options.ApiKey?.Reveal() ?? string.Empty;

        if (options.Provider == VKAIProviderType.AzureOpenAI)
        {
            var endpoint = options.Endpoint ?? throw new InvalidOperationException($"Endpoint is required for Azure OpenAI {operationContext}.");
            var azureOptions = new AzureOpenAIClientOptions
            {
                RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
                Transport = new HttpClientPipelineTransport(httpClient)
            };

            if (options.TimeoutSeconds > 0)
            {
                azureOptions.NetworkTimeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            }

            return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey), azureOptions);
        }

        var openAIOptions = new OpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        if (options.Endpoint is not null)
        {
            openAIOptions.Endpoint = new Uri(options.Endpoint);
        }

        if (options.TimeoutSeconds > 0)
        {
            openAIOptions.NetworkTimeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }

        if (!string.IsNullOrEmpty(options.OrganizationId))
        {
            openAIOptions.OrganizationId = options.OrganizationId;
        }

        if (!string.IsNullOrEmpty(options.ProjectId))
        {
            openAIOptions.ProjectId = options.ProjectId;
        }

        return new OpenAIClient(new ApiKeyCredential(apiKey), openAIOptions);
    }

    internal static OllamaApiClient CreateOllamaChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
    {
        var endpoint = options.Endpoint ?? "http://localhost:11434";
        var modelId = options.ChatModelId ?? "llama3.2";

        // OllamaSharp 5.5.2 has no internal retry mechanism; outbound retries are governed solely by AIMEAITransportResilienceHandler.
        var client = new OllamaApiClient(httpClient, endpoint);
        client.SelectedModel = modelId;
        return client;
    }

    internal static OllamaApiClient CreateOllamaEmbeddingGenerator(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var endpoint = options.Endpoint ?? "http://localhost:11434";
        var modelId = options.EmbeddingModelId ?? "nomic-embed-text";

        // OllamaSharp 5.5.2 has no internal retry mechanism; outbound retries are governed solely by AIMEAITransportResilienceHandler.
        var client = new OllamaApiClient(httpClient, endpoint);
        client.SelectedModel = modelId;
        return client;
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "Ownership of the inner GeminiChatClient is transferred to GoogleNoRetryChatClient.")]
    internal static IChatClient CreateGoogleChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
    {
        var modelId = options.ChatModelId ?? "gemini-2.5-flash";

        var cleanModel = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        var googleAi = CreateGoogleClient(options, httpClient);
        return new GoogleNoRetryChatClient(new GeminiChatClient(googleAi.GenerativeModel(cleanModel)));
    }

    internal static GeminiEmbeddingGenerator CreateGoogleEmbeddingGenerator(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var modelId = options.EmbeddingModelId ?? "text-embedding-004";

        var cleanModel = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        var googleAi = CreateGoogleClient(options, httpClient);
        return new GeminiEmbeddingGenerator(googleAi.GenerativeModel(cleanModel));
    }

    internal static ISpeechToTextClient CreateGoogleSpeechToTextClient(
        VKAIMEAIProviderSettings options,
        HttpClient httpClient)
    {
        var cleanModel = (options.TranscriptionModelId ?? options.ChatModelId ?? "gemini-2.5-flash")
            .Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        var googleAi = CreateGoogleClient(options, httpClient);
        return googleAi.GenerativeModel(cleanModel).AsISpeechToTextClient();
    }

    private static GoogleAI CreateGoogleClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
    {
        var apiKey = options.ApiKey?.Reveal() ?? string.Empty;

        // [MEA-06] Mscc.GenerativeAI BaseModel.SendAsync executes a 1-based retry loop (`index <= retry.Maximum`);
        // Maximum = 1 with an empty StatusCodes list runs at most 1 HTTP attempt with zero SDK-internal retries.
        var requestOptions = new Mscc.GenerativeAI.Types.RequestOptions
        {
            Retry = new Mscc.GenerativeAI.Types.Retry
            {
                Maximum = 1,
                StatusCodes = []
            },
            Timeout = TimeSpan.Zero
        };

        return new GoogleAI(
            apiKey: apiKey,
            httpClientFactory: new ResilientHttpClientFactory(httpClient),
            requestOptions: requestOptions);
    }

    private static void DisposeCandidateClients(
        ConcurrentDictionary<string, IChatClient> candidateClients,
        IChatClient fallbackClient)
    {
        foreach (var (_, client) in candidateClients)
        {
            if (!ReferenceEquals(client, fallbackClient))
            {
                client.Dispose();
            }
        }

        candidateClients.Clear();
    }

    /// <summary>
    /// Ensures <see cref="GeminiChatClient"/> does not re-enable <c>Mscc.GenerativeAI</c> internal retries
    /// when <see cref="ChatOptions.AdditionalProperties"/> is non-empty (which causes
    /// <c>AbstractionMapper.ToGeminiGenerateContentRequestOptions</c> to construct a fresh default
    /// <see cref="Mscc.GenerativeAI.Types.Retry"/>).
    /// </summary>
    private sealed class GoogleNoRetryChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
    {
        public override Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => base.GetResponseAsync(messages, EnsureNoRetryOptions(options), cancellationToken);

        public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => base.GetStreamingResponseAsync(messages, EnsureNoRetryOptions(options), cancellationToken);

        private static ChatOptions? EnsureNoRetryOptions(ChatOptions? options)
        {
            if (options?.AdditionalProperties is null || options.AdditionalProperties.Count == 0)
            {
                return options;
            }

            var cloned = options.Clone();
            cloned.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            if (!cloned.AdditionalProperties.ContainsKey("RetryMaximum"))
            {
                cloned.AdditionalProperties["RetryMaximum"] = 1;
            }

            if (!cloned.AdditionalProperties.ContainsKey("RetryStatusCodes"))
            {
                cloned.AdditionalProperties["RetryStatusCodes"] = Array.Empty<int>();
            }

            return cloned;
        }
    }

    private sealed class ResilientHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;
    }

    /// <summary>
    /// Enforces single tool-invocation ownership per call chain (G3):
    /// when an upstream runtime (e.g. AgentFramework) sets <c>vk.autoInvokeTools = false</c>,
    /// this gate bypasses the inner MEAI <see cref="FunctionInvokingChatClient"/>.
    /// </summary>
    private sealed class FunctionInvocationGateChatClient(
        IChatClient directInnerClient,
        IChatClient functionInvokingClient) : DelegatingChatClient(functionInvokingClient)
    {
        private readonly IChatClient _directInnerClient = VKGuard.NotNull(directInnerClient);
        private readonly IChatClient _functionInvokingClient = VKGuard.NotNull(functionInvokingClient);

        public override Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var target = SelectTarget(options);
            return target.GetResponseAsync(messages, options, cancellationToken);
        }

        public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var target = SelectTarget(options);
            return target.GetStreamingResponseAsync(messages, options, cancellationToken);
        }

        private IChatClient SelectTarget(ChatOptions? options)
        {
            if (options?.AdditionalProperties is not null)
            {
                if (options.AdditionalProperties.TryGetValue("vk.autoInvokeTools", out var autoInvoke) &&
                    autoInvoke is false)
                {
                    return _directInnerClient;
                }

                if (options.AdditionalProperties.TryGetValue("vk.maxAutoToolRounds", out var maxRoundsObj) &&
                    maxRoundsObj is int maxRounds &&
                    maxRounds > 0 &&
                    _functionInvokingClient is FunctionInvokingChatClient fic)
                {
                    fic.MaximumIterationsPerRequest = maxRounds;
                }
            }

            return _functionInvokingClient;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _functionInvokingClient.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
