using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing an AI Persona anchor.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKPersonaAnchor : VKAggregateRoot<VKPersonaId>
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the unique name of the persona.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the narrative description and role definition for this persona.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the personality traits and behavioral nuances.
    /// </summary>
    public IReadOnlyDictionary<string, string> Traits { get; private set; }

    /// <summary>
    /// Gets arbitrary extension metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Extensions { get; private set; }

    /// <summary>
    /// Gets the layout priority order within this tier (0 = highest priority, rendered earliest).
    /// Follows strict ascending ordering (0, 1, 2...).
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Gets the precalculated token count for this persona's prompt text.
    /// Default is 0 (uncalculated).
    /// </summary>
    public int TokenCount { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKPersonaAnchor(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits,
        IReadOnlyDictionary<string, object>? extensions,
        int priority = 0,
        int tokenCount = 0) : base(id)
    {
        Name = name;
        Description = description;
        Traits = traits ?? new Dictionary<string, string>();
        Extensions = extensions ?? new Dictionary<string, object>();
        Priority = priority;
        TokenCount = tokenCount;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new persona anchor aggregate root.
    /// </summary>
    public static VKResult<VKPersonaAnchor> Create(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null,
        int priority = 0,
        int tokenCount = 0)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotNull(description);

        return VKResult.Success(new VKPersonaAnchor(id, name, description, traits, extensions, priority, tokenCount));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKPersonaAnchor Rehydrate(
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        IReadOnlyDictionary<string, object>? extensions = null,
        int priority = 0,
        int tokenCount = 0)
    {
        return new VKPersonaAnchor(id, name, description, traits, extensions, priority, tokenCount);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the persona's core identification and narrative description.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult UpdateDetails(string name, string description)
    {
        Name = VKGuard.NotNullOrWhiteSpace(name);
        Description = VKGuard.NotNull(description);
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Sets or updates a single personality trait key-value pair.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult SetTrait(string key, string value)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        var dict = new Dictionary<string, string>(Traits) { [key] = value };
        Traits = dict;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Removes a personality trait if present.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult RemoveTrait(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (!Traits.ContainsKey(key))
        {
            return VKResult.Success();
        }

        var dict = new Dictionary<string, string>(Traits);
        dict.Remove(key);
        Traits = dict;
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Replaces the entire traits dictionary.
    /// Resets precalculated token count to 0.
    /// </summary>
    public VKResult ReplaceTraits(IReadOnlyDictionary<string, string> traits)
    {
        Traits = new Dictionary<string, string>(VKGuard.NotNull(traits));
        TokenCount = 0;
        return VKResult.Success();
    }

    /// <summary>
    /// Sets or updates an extension metadata entry.
    /// </summary>
    public VKResult SetExtension(string key, object value)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        var dict = new Dictionary<string, object>(Extensions) { [key] = value };
        Extensions = dict;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the precalculated token count for this persona's prompt text.
    /// </summary>
    public VKResult UpdateTokenCount(int tokenCount)
    {
        TokenCount = Math.Max(0, tokenCount);
        return VKResult.Success();
    }
}
