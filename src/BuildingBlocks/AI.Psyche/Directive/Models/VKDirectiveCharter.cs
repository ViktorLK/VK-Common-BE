using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a Directive containing core system prompt instructions and safety rules.
/// Encapsulates prompt guidelines, safety policies, and output constraints with rich domain invariants.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKDirectiveCharter : VKAggregateRoot<VKDirectiveId>, IVKFragmentMetadata
{
    // =========================================================================
    // Properties
    // =========================================================================

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

    // =========================================================================
    // Constructor (Private to prevent direct unvalidated instantiation)
    // =========================================================================

    private VKDirectiveCharter(
        VKDirectiveId id,
        string? overview,
        string? behaviorRules,
        string? safetyRules,
        string? outputConstraints) : base(id)
    {
        Overview = overview;
        BehaviorRules = behaviorRules;
        SafetyRules = safetyRules;
        OutputConstraints = outputConstraints;
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
        string? outputConstraints = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        return VKResult.Success(new VKDirectiveCharter(id, overview, behaviorRules, safetyRules, outputConstraints));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKDirectiveCharter Rehydrate(
        VKDirectiveId id,
        string? overview,
        string? behaviorRules,
        string? safetyRules,
        string? outputConstraints)
    {
        return new VKDirectiveCharter(id, overview, behaviorRules, safetyRules, outputConstraints);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the core overview and instructions for this directive.
    /// </summary>
    public VKResult UpdateOverview(string? overview)
    {
        Overview = overview;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the behavioral guidelines and interaction principles.
    /// </summary>
    public VKResult UpdateBehaviorRules(string? behaviorRules)
    {
        BehaviorRules = behaviorRules;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the safety protocols and refusal policies.
    /// </summary>
    public VKResult UpdateSafetyRules(string? safetyRules)
    {
        SafetyRules = safetyRules;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the formatting and structural output constraints.
    /// </summary>
    public VKResult UpdateOutputConstraints(string? outputConstraints)
    {
        OutputConstraints = outputConstraints;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates all directive rules and prompt instructions atomically.
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
        return VKResult.Success();
    }
}
