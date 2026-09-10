namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain evaluation service for calculating token counts on Psyche aggregate roots and prompt segments.
/// Enables write-time token calculation during creation and updates so inference pipelines operate with O(1) integer arithmetic.
/// Follows AP.01, AP.03, and AP.07.
/// </summary>
public interface IVKPsycheTokenEvaluator
{
    /// <summary>
    /// Evaluates the token count of a persona's rendered Markdown text and updates <see cref="VKPersonaAnchor.TokenCount"/>.
    /// </summary>
    /// <param name="persona">The persona anchor aggregate root.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>The calculated token count.</returns>
    int EvaluateAndRefresh(VKPersonaAnchor persona, string? modelId = null);

    /// <summary>
    /// Evaluates the token count of a directive's rendered Markdown text and updates <see cref="VKDirectiveCharter.TokenCount"/>.
    /// </summary>
    /// <param name="directive">The directive charter aggregate root.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>The calculated token count.</returns>
    int EvaluateAndRefresh(VKDirectiveCharter directive, string? modelId = null);

    /// <summary>
    /// Evaluates the token count of a knowledge entry's segment and updates <see cref="VKKnowledgeEntry.TokenCount"/>.
    /// </summary>
    /// <param name="knowledge">The knowledge entry aggregate root.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>The calculated token count.</returns>
    int EvaluateAndRefresh(VKKnowledgeEntry knowledge, string? modelId = null);

    /// <summary>
    /// Evaluates the token count of a pattern entry's segment and updates <see cref="VKPatternEntry.TokenCount"/>.
    /// </summary>
    /// <param name="pattern">The pattern entry aggregate root.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>The calculated token count.</returns>
    int EvaluateAndRefresh(VKPatternEntry pattern, string? modelId = null);

    /// <summary>
    /// Evaluates the estimated token count of a user profile presence and updates <see cref="VKProfilePresence.TokenCount"/>.
    /// </summary>
    /// <param name="profile">The profile presence aggregate root.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>The calculated token count.</returns>
    int EvaluateAndRefresh(VKProfilePresence profile, string? modelId = null);

    /// <summary>
    /// Evaluates the token count of a prompt segment content.
    /// </summary>
    /// <param name="segment">The prompt segment record.</param>
    /// <param name="modelId">Optional target AI model ID for model-specific tokenization.</param>
    /// <returns>A new <see cref="VKPromptSegment"/> with the evaluated <see cref="VKPromptSegment.TokenCount"/>.</returns>
    VKPromptSegment Evaluate(VKPromptSegment segment, string? modelId = null);
}
