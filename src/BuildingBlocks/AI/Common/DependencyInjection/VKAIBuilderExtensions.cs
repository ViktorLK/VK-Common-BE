using System;
using VK.Blocks.AI.Agents.Internal;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.AI.Audio.Speech.Internal;
using VK.Blocks.AI.Audio.Transcription.Internal;
using VK.Blocks.AI.Chat.Internal;
using VK.Blocks.AI.Common.DependencyInjection.Internal;
using VK.Blocks.AI.Guardrails.Content.Internal;
using VK.Blocks.AI.Guardrails.Injection.Internal;
using VK.Blocks.AI.Guardrails.Internal;
using VK.Blocks.AI.Guardrails.Privacy.Internal;
using VK.Blocks.AI.Text.Internal;
using VK.Blocks.AI.Tokenics.Budgeting.Internal;
using VK.Blocks.AI.Tokenics.Costing.Internal;
using VK.Blocks.AI.Tokenics.Counting.Internal;
using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.AI.Tokenics.Limiting.Internal;
using VK.Blocks.AI.Vectorics.Embeddings.Internal;
using VK.Blocks.AI.Vectorics.Internal;
using VK.Blocks.AI.Vectorics.ReRanking.Internal;
using VK.Blocks.AI.Vectorics.Retrieval.Internal;
using VK.Blocks.AI.Vectorics.SemanticCache.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Fluent API extensions for <see cref="IVKAIBuilder"/> to configure AI features.
/// Following the separation of concerns pattern (Block vs Builder extensions).
/// </summary>
public static class VKAIBuilderExtensions
{
    // ========================================================================
    // 1. INFRASTRUCTURE & CORE FEATURES
    // ========================================================================

    /// <summary>
    /// Adds AI Defaults (Retry, Timeout, Provider) used as fallback for all features.
    /// </summary>
    public static IVKAIBuilder AddVKDefaults(
        this IVKAIBuilder builder,
        Func<VKAIDefaultsOptions, VKAIDefaultsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return AIDefaultsFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Chat feature (Conversational AI).
    /// </summary>
    public static IVKAIBuilder AddVKChat(
        this IVKAIBuilder builder,
        Func<VKChatOptions, VKChatOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return ChatFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Text feature (Completion, Summary, Extraction).
    /// </summary>
    public static IVKAIBuilder AddVKText(
        this IVKAIBuilder builder,
        Func<VKTextOptions, VKTextOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return TextFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Agents feature (Tool calling and autonomous orchestration).
    /// </summary>
    public static IVKAIBuilder AddVKAgents(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return AgentsFeature.Register(builder);
    }

    // ========================================================================
    // 2. VECTORICS SUB-FEATURES
    // ========================================================================

    /// <summary>
    /// Adds the Embeddings feature (Vector representation of text).
    /// </summary>
    public static IVKAIBuilder AddVKEmbeddings(
        this IVKAIBuilder builder,
        Func<VKEmbeddingsOptions, VKEmbeddingsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return EmbeddingsFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Retrieval feature (Vector search and RAG).
    /// </summary>
    public static IVKAIBuilder AddVKRetrieval(
        this IVKAIBuilder builder,
        Func<VKRetrievalOptions, VKRetrievalOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return RetrievalFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Re-Ranking feature (Refining search results).
    /// </summary>
    public static IVKAIBuilder AddVKReRanking(
        this IVKAIBuilder builder,
        Func<VKReRankingOptions, VKReRankingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return ReRankingFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Semantic Caching feature.
    /// </summary>
    public static IVKAIBuilder AddVKSemanticCache(
        this IVKAIBuilder builder,
        Func<VKSemanticCacheOptions, VKSemanticCacheOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return SemanticCacheFeature.Register(builder, transform);
    }

    // ========================================================================
    // 3. AUDIO SUB-FEATURES
    // ========================================================================

    /// <summary>
    /// Adds the Audio Speech (TTS) feature.
    /// </summary>
    public static IVKAIBuilder AddVKSpeech(
        this IVKAIBuilder builder,
        Func<VKSpeechOptions, VKSpeechOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return SpeechFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Audio Transcription (STT) feature.
    /// </summary>
    public static IVKAIBuilder AddVKTranscription(
        this IVKAIBuilder builder,
        Func<VKTranscriptionOptions, VKTranscriptionOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return TranscriptionFeature.Register(builder, transform);
    }

    // ========================================================================
    // 4. GUARDRAILS SUB-FEATURES
    // ========================================================================

    /// <summary>
    /// Adds the Content Guard feature (Safety filtering).
    /// </summary>
    public static IVKAIBuilder AddVKContent(
        this IVKAIBuilder builder,
        Func<VKContentOptions, VKContentOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return ContentFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Privacy Guard feature (PII protection).
    /// </summary>
    public static IVKAIBuilder AddVKPrivacy(
        this IVKAIBuilder builder,
        Func<VKPrivacyOptions, VKPrivacyOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return PrivacyFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Injection Guard feature (Prompt injection prevention).
    /// </summary>
    public static IVKAIBuilder AddVKInjection(
        this IVKAIBuilder builder,
        Func<VKInjectionOptions, VKInjectionOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return InjectionFeature.Register(builder, transform);
    }

    // ========================================================================
    // 5. TOKENICS SUB-FEATURES
    // ========================================================================

    /// <summary>
    /// Adds the Token Counting feature.
    /// </summary>
    public static IVKAIBuilder AddVKCounting(
        this IVKAIBuilder builder,
        Func<VKCountingOptions, VKCountingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return CountingFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Token Costing feature.
    /// </summary>
    public static IVKAIBuilder AddVKCosting(
        this IVKAIBuilder builder,
        Func<VKCostingOptions, VKCostingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return CostingFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Token Limiting (Rate limiting) feature.
    /// </summary>
    public static IVKAIBuilder AddVKLimiting(
        this IVKAIBuilder builder,
        Func<VKLimitingOptions, VKLimitingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return LimitingFeature.Register(builder, transform);
    }

    /// <summary>
    /// Adds the Token Budgeting feature.
    /// </summary>
    public static IVKAIBuilder AddVKBudgeting(
        this IVKAIBuilder builder,
        Func<VKBudgetingOptions, VKBudgetingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return BudgetingFeature.Register(builder, transform);
    }

    // ========================================================================
    // 6. PILLAR AGGREGATES (AUTO-ENABLES SUB-FEATURES)
    // ========================================================================

    /// <summary>
    /// Adds the Vectorics pillar (Embeddings, Retrieval, SemanticCache, Re-Ranking).
    /// </summary>
    public static IVKAIBuilder AddVKVectorics(
        this IVKAIBuilder builder,
        Func<VKVectoricsOptions, VKVectoricsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        builder = VectoricsFeature.Register(builder, transform);

        builder.AddVKEmbeddings();
        builder.AddVKRetrieval();
        builder.AddVKSemanticCache();
        builder.AddVKReRanking();

        return builder;
    }

    /// <summary>
    /// Adds the Audio pillar (Speech and Transcription).
    /// </summary>
    public static IVKAIBuilder AddVKAudio(
        this IVKAIBuilder builder,
        Func<VKAudioOptions, VKAudioOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        builder = AudioFeature.Register(builder, transform);

        builder.AddVKSpeech();
        builder.AddVKTranscription();

        return builder;
    }

    /// <summary>
    /// Adds the Guardrails pillar (Content, Privacy, Injection).
    /// </summary>
    public static IVKAIBuilder AddVKGuardrails(
        this IVKAIBuilder builder,
        Func<VKGuardrailsOptions, VKGuardrailsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        builder = GuardrailsFeature.Register(builder, transform);

        builder.AddVKContent();
        builder.AddVKPrivacy();
        builder.AddVKInjection();

        return builder;
    }

    /// <summary>
    /// Adds the Tokenics pillar (Counting, Costing, Limiting, Quotas, Budgeting).
    /// </summary>
    public static IVKAIBuilder AddVKTokenics(
        this IVKAIBuilder builder,
        Func<VKTokenicsOptions, VKTokenicsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        builder = TokenicsFeature.Register(builder, transform);

        builder.AddVKCounting();
        builder.AddVKCosting();
        builder.AddVKLimiting();
        builder.AddVKBudgeting();

        return builder;
    }

    // ========================================================================
    // 7. FINAL AGGREGATE
    // ========================================================================

    /// <summary>
    /// Automatically enables all standard core AI features in one go.
    /// </summary>
    public static IVKAIBuilder AddVKAIDefaultFeatures(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKChat()
            .AddVKText()
            .AddVKAudio()
            .AddVKVectorics()
            .AddVKGuardrails()
            .AddVKTokenics()
            .AddVKAgents();
    }
}
