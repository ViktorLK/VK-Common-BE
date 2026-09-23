using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPsycheTokenEvaluator"/>.
/// Computes token counts for Psyche domain aggregates and segments using registered renderers and token counter.
/// Follows AP.01, CS.01, AP.07.
/// </summary>
internal sealed class DefaultPsycheTokenEvaluator : IVKPsycheTokenEvaluator
{
    private readonly IVKPersonaRenderer _personaRenderer;
    private readonly IVKDirectiveRenderer _directiveRenderer;
    private readonly IVKProfileRenderer _profileRenderer;
    private readonly IVKTokenCounter _tokenCounter;

    public DefaultPsycheTokenEvaluator(
        IVKPersonaRenderer personaRenderer,
        IVKDirectiveRenderer directiveRenderer,
        IVKProfileRenderer profileRenderer,
        IVKTokenCounter tokenCounter)
    {
        _personaRenderer = VKGuard.NotNull(personaRenderer);
        _directiveRenderer = VKGuard.NotNull(directiveRenderer);
        _profileRenderer = VKGuard.NotNull(profileRenderer);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
    }

    /// <inheritdoc />
    public int EvaluateAndRefresh(VKPersonaAnchor persona, string? modelId = null)
    {
        VKGuard.NotNull(persona);

        var content = _personaRenderer.Render(persona);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : _tokenCounter.CountTokens(content, modelId);
        persona.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <inheritdoc />
    public int EvaluateAndRefresh(VKDirectiveCharter directive, string? modelId = null)
    {
        VKGuard.NotNull(directive);

        var content = _directiveRenderer.Render(directive);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : _tokenCounter.CountTokens(content, modelId);
        directive.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <inheritdoc />
    public int EvaluateAndRefresh(VKKnowledgeEntry knowledge, string? modelId = null)
    {
        VKGuard.NotNull(knowledge);

        var content = knowledge.Payload?.Content;
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : _tokenCounter.CountTokens(content, modelId);
        knowledge.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <inheritdoc />
    public int EvaluateAndRefresh(VKPatternEntry pattern, string? modelId = null)
    {
        VKGuard.NotNull(pattern);

        var content = pattern.Payload?.Content;
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : _tokenCounter.CountTokens(content, modelId);
        pattern.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <inheritdoc />
    public int EvaluateAndRefresh(VKProfilePresence profile, string? modelId = null)
    {
        VKGuard.NotNull(profile);

        var content = _profileRenderer.Render(profile);
        int tokens = string.IsNullOrWhiteSpace(content) ? 0 : _tokenCounter.CountTokens(content, modelId);
        profile.UpdateTokenCount(tokens);
        return tokens;
    }

    /// <inheritdoc />
    public VKPromptSegment Evaluate(VKPromptSegment segment, string? modelId = null)
    {
        VKGuard.NotNull(segment);

        if (string.IsNullOrWhiteSpace(segment.Payload.Content))
        {
            return segment.Payload.TokenCount == 0 ? segment : segment with { Payload = segment.Payload with { TokenCount = 0 } };
        }

        int tokens = _tokenCounter.CountTokens(segment.Payload.Content, modelId);
        return segment with { Payload = segment.Payload with { TokenCount = tokens } };
    }
}
