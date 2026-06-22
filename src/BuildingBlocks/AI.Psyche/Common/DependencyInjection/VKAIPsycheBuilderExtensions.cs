using System;
using VK.Blocks.AI.Psyche.Directive.Internal;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.AI.Psyche.Knowledge.Internal;
using VK.Blocks.AI.Psyche.Pattern.Internal;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.AI.Psyche.Pipelines.Internal;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.DependencyInjection;

/// <summary>
/// Extension methods for chaining AI Psyche features.
/// </summary>
public static class VKAIPsycheBuilderExtensions
{
    /// <summary>
    /// Adds the Directive feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKDirective(
        this IVKAIPsycheBuilder builder,
        Func<VKDirectiveOptions, VKDirectiveOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        DirectiveFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Echo feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKEcho(
        this IVKAIPsycheBuilder builder,
        Func<VKEchoOptions, VKEchoOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        EchoFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Knowledge feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKKnowledge(
        this IVKAIPsycheBuilder builder,
        Func<VKKnowledgeOptions, VKKnowledgeOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        KnowledgeFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Pattern feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKPattern(
        this IVKAIPsycheBuilder builder,
        Func<VKPatternOptions, VKPatternOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        PatternFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Persona feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKPersona(
        this IVKAIPsycheBuilder builder,
        Func<VKPersonaOptions, VKPersonaOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        PersonaFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Pipeline feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKPipeline(
        this IVKAIPsycheBuilder builder,
        Func<VKPipelinesOptions, VKPipelinesOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        PipelinesFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Weaving feature to the AI Psyche building block.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKWeaving(
        this IVKAIPsycheBuilder builder,
        Func<VKWeavingOptions, VKWeavingOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        WeavingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI Psyche features.
    /// </summary>
    public static IVKAIPsycheBuilder AddVKDefaultFeatures(this IVKAIPsycheBuilder builder)
    {
        VKGuard.NotNull(builder); // [AP.01]
        return builder
            .AddVKDirective()
            .AddVKEcho()
            .AddVKKnowledge()
            .AddVKPattern()
            .AddVKPersona()
            .AddVKPipeline()
            .AddVKWeaving();
    }
}
