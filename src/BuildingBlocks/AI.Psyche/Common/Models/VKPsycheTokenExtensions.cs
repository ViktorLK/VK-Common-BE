using System;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Convenient extension methods for calculating and refreshing token counts on Psyche models.
/// Follows AP.01, AP.03, and AP.07.
/// </summary>
public static class VKPsycheTokenExtensions
{
    /// <summary>
    /// Refreshes the token count of a persona anchor using the specified renderer and token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKPersonaAnchor persona,
        IVKPersonaRenderer renderer,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        VKGuard.NotNull(persona);
        VKGuard.NotNull(renderer);
        VKGuard.NotNull(tokenCounter);

        var content = renderer.Render(persona);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : tokenCounter.CountTokens(content, modelId);
        persona.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <summary>
    /// Refreshes the token count of a persona anchor using the injected token evaluator.
    /// </summary>
    public static int RefreshTokenCount(
        this VKPersonaAnchor persona,
        IVKPsycheTokenEvaluator evaluator,
        string? modelId = null)
    {
        VKGuard.NotNull(persona);
        VKGuard.NotNull(evaluator);
        return evaluator.EvaluateAndRefresh(persona, modelId);
    }

    /// <summary>
    /// Refreshes the token count of a directive charter using the specified renderer and token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKDirectiveCharter directive,
        IVKDirectiveRenderer renderer,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        VKGuard.NotNull(directive);
        VKGuard.NotNull(renderer);
        VKGuard.NotNull(tokenCounter);

        var content = renderer.Render(directive);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : tokenCounter.CountTokens(content, modelId);
        directive.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <summary>
    /// Refreshes the token count of a directive charter using the injected token evaluator.
    /// </summary>
    public static int RefreshTokenCount(
        this VKDirectiveCharter directive,
        IVKPsycheTokenEvaluator evaluator,
        string? modelId = null)
    {
        VKGuard.NotNull(directive);
        VKGuard.NotNull(evaluator);
        return evaluator.EvaluateAndRefresh(directive, modelId);
    }

    /// <summary>
    /// Refreshes the token count of a knowledge entry using the specified token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKKnowledgeEntry knowledge,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        VKGuard.NotNull(knowledge);
        VKGuard.NotNull(tokenCounter);

        var content = knowledge.Segment?.Content;
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : tokenCounter.CountTokens(content, modelId);
        knowledge.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <summary>
    /// Refreshes the token count of a knowledge entry using the injected token evaluator.
    /// </summary>
    public static int RefreshTokenCount(
        this VKKnowledgeEntry knowledge,
        IVKPsycheTokenEvaluator evaluator,
        string? modelId = null)
    {
        VKGuard.NotNull(knowledge);
        VKGuard.NotNull(evaluator);
        return evaluator.EvaluateAndRefresh(knowledge, modelId);
    }

    /// <summary>
    /// Refreshes the token count of a pattern entry using the specified token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKPatternEntry pattern,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        VKGuard.NotNull(pattern);
        VKGuard.NotNull(tokenCounter);

        var content = pattern.Segment?.Content;
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : tokenCounter.CountTokens(content, modelId);
        pattern.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <summary>
    /// Refreshes the token count of a pattern entry using the injected token evaluator.
    /// </summary>
    public static int RefreshTokenCount(
        this VKPatternEntry pattern,
        IVKPsycheTokenEvaluator evaluator,
        string? modelId = null)
    {
        VKGuard.NotNull(pattern);
        VKGuard.NotNull(evaluator);
        return evaluator.EvaluateAndRefresh(pattern, modelId);
    }

    /// <summary>
    /// Refreshes the estimated token count of a user profile presence using the specified renderer and token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKProfilePresence profile,
        IVKProfileRenderer renderer,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        VKGuard.NotNull(profile);
        VKGuard.NotNull(renderer);
        VKGuard.NotNull(tokenCounter);

        var content = renderer.Render(profile);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : tokenCounter.CountTokens(content, modelId);
        profile.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <summary>
    /// Refreshes the estimated token count of a user profile presence using the specified token counter.
    /// </summary>
    public static int RefreshTokenCount(
        this VKProfilePresence profile,
        IVKTokenCounter tokenCounter,
        string? modelId = null)
    {
        return RefreshTokenCount(profile, new Profile.Internal.DefaultProfileRenderer(), tokenCounter, modelId);
    }

    /// <summary>
    /// Refreshes the estimated token count of a user profile presence using the injected token evaluator.
    /// </summary>
    public static int RefreshTokenCount(
        this VKProfilePresence profile,
        IVKPsycheTokenEvaluator evaluator,
        string? modelId = null)
    {
        VKGuard.NotNull(profile);
        VKGuard.NotNull(evaluator);
        return evaluator.EvaluateAndRefresh(profile, modelId);
    }
}
