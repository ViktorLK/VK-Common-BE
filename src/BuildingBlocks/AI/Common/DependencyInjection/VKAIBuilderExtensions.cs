using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Fluent API extensions for <see cref="IVKAIBuilder"/> to configure AI features.
/// Following the separation of concerns pattern (Block vs Builder extensions).
/// </summary>
public static partial class VKAIBuilderExtensions
{
    // ========================================================================
    // PILLAR AGGREGATES (AUTO-ENABLES SUB-FEATURES)
    // ========================================================================

    /// <summary>
    /// Adds the Audio pillar (Speech and Transcription).
    /// </summary>
    public static IVKAIBuilder AddVKAudioPillar(
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
    public static IVKAIBuilder AddVKGuardrailsPillar(
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
    public static IVKAIBuilder AddVKTokenicsPillar(
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

}
