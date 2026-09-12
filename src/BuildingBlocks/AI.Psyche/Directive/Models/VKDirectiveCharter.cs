using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a Directive containing core system prompt instructions and safety rules.
/// Encapsulates prompt guidelines, safety policies, and output constraints with rich domain invariants.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKDirectiveCharter : VKAggregateRoot<VKDirectiveId>
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the administrative human-readable name or title for this directive.
    /// Used for management, logging, and diagnostics; not rendered into LLM prompt payload.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Gets the high-level overview or core system instructions for this directive.
    /// </summary>
    public string? Overview { get; private set; }

    /// <summary>
    /// Gets the behavioral guidelines and principles for AI interaction.
    /// </summary>
    public string? BehaviorRules { get; private set; }

    /// <summary>
    /// Gets the safety protocols and refusal policies to prevent harmful outputs.
    /// </summary>
    public string? SafetyRules { get; private set; }

    /// <summary>
    /// Gets the formatting and structural output constraints (e.g. Markdown, JSON schema).
    /// </summary>
    public string? OutputConstraints { get; private set; }

    /// <summary>
    /// Gets the layout priority order within this tier (0 = highest priority, rendered earliest).
    /// Follows strict ascending ordering (0, 1, 2...).
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Gets the precalculated token count for this directive's prompt text.
    /// Default is 0 (uncalculated).
    /// </summary>
    public int TokenCount { get; private set; }

    // =========================================================================
    // Constructor (Private to prevent direct unvalidated instantiation)
    // =========================================================================

    private VKDirectiveCharter(
        VKDirectiveId id,
        string? name,
        string? overview,
        string? behaviorRules,
        string? safetyRules,
        string? outputConstraints,
        int priority = 0,
        int tokenCount = 0) : base(id)
    {
        Name = name;
        Overview = overview;
        BehaviorRules = behaviorRules;
        SafetyRules = safetyRules;
        OutputConstraints = outputConstraints;
        Priority = priority;
        TokenCount = tokenCount;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new directive charter aggregate root.
    /// Enforces boundary validation.
    /// </summary>
    public static VKResult<VKDirectiveCharter> Create(
        VKDirectiveId id,
        string? overview = null,
        string? behaviorRules = null,
        string? safetyRules = null,
        string? outputConstraints = null,
        int priority = 0,
        int tokenCount = 0,
        string? name = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        return VKResult.Success(new VKDirectiveCharter(id, name, overview, behaviorRules, safetyRules, outputConstraints, priority, tokenCount));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKDirectiveCharter Rehydrate(
        VKDirectiveId id,
        string? overview,
        string? behaviorRules,
        string? safetyRules,
        string? outputConstraints,
        int priority = 0,
        int tokenCount = 0,
        string? name = null)
    {
        return new VKDirectiveCharter(id, name, overview, behaviorRules, safetyRules, outputConstraints, priority, tokenCount);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the administrative human-readable name of this directive charter.
    /// </summary>
    public VKResult UpdateName(string? name)
    {
        Name = name;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the core overview and instructions for this directive.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateOverview(string? overview)
    {
        Overview = overview;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the behavioral guidelines and interaction principles.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateBehaviorRules(string? behaviorRules)
    {
        BehaviorRules = behaviorRules;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the safety protocols and refusal policies.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateSafetyRules(string? safetyRules)
    {
        SafetyRules = safetyRules;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the formatting and structural output constraints.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateOutputConstraints(string? outputConstraints)
    {
        OutputConstraints = outputConstraints;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates all directive rules and prompt instructions atomically.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateContent(
        string? overview,
        string? behaviorRules,
        string? safetyRules,
        string? outputConstraints)
    {
        Overview = overview;
        BehaviorRules = behaviorRules;
        SafetyRules = safetyRules;
        OutputConstraints = outputConstraints;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the precalculated token count for this directive's prompt text.
    /// </summary>
    public VKResult UpdateTokenCount(int tokenCount)
    {
        TokenCount = Math.Max(0, tokenCount);
        return VKResult.Success();
    }
}
