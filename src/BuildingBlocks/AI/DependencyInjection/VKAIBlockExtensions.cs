using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.AI.Chat.Internal;
using VK.Blocks.AI.DependencyInjection.Internal;
using VK.Blocks.AI.Embeddings.Internal;
using VK.Blocks.AI.Moderation.Internal;
using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Service collection extensions for the core AI building block.
/// </summary>
public static class VKAIBlockExtensions
{
    /// <summary>
    /// Adds the AI building block services using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKAIBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AIBlockRegistration.Register(services, configuration: configuration);
    }

    /// <summary>
    /// Adds the AI building block services using a functional transformation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">The functional transformation to apply to the default options.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKAIBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIOptions, VKAIOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return AIBlockRegistration.Register(services, configuration, transform);
    }

    /// <summary>
    /// Adds the Chat feature to the AI building block.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKChat(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return ChatFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Embeddings feature to the AI building block.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKEmbeddings(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return EmbeddingsFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Audio features (Speech and Transcription) to the AI building block.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKAudio(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return AudioFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Moderation feature to the AI building block.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKModeration(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return ModerationFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Tokenics feature to the AI building block.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKTokenics(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return TokenicsFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Automatically enables all standard core AI features.
    /// </summary>
    /// <param name="builder">The AI block builder.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKDefaultFeatures(this IVKAIBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKChat()
            .AddVKEmbeddings()
            .AddVKAudio()
            .AddVKModeration()
            .AddVKTokenics();
    }
}
