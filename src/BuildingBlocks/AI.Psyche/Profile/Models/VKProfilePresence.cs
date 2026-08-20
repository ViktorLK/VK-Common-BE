using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Lightweight cognitive presence representing the profile in Psyche's execution pipeline.
/// Follows AP.01 (sealed record) and BB.01.
/// </summary>
/// <remarks>
/// Architectural Note on Scope:
/// - Keep Psyche's ProfilePresence ultra-lightweight (DisplayName, Language, TimeZone).
/// - Complex profile cognitive aspects MUST NOT be stored in Psyche to prevent pipeline bloat:
///   1. Long-term / Interaction Preferences -> Managed by <c>AI.Engram</c> (Semantic Memory Consolidation).
///   2. Personal Knowledge Base -> Managed by <c>AI.Corpus</c> (RAG Vector Indexing).
///   3. Skill & Capability Profiles -> Managed by <c>AI.Engram</c> (Episodic/Semantic Profile Graph).
///   4. Goals & Intentions -> Managed by <c>AI.Engram</c> (Working Memory / Goal Tracking).
///   5. Work Context & Tasks -> Managed by <c>AI.Engram</c> (Contextual Reminder/Task Memory).
/// </remarks>
public sealed record VKProfilePresence : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the unique profile identifier.
    /// </summary>
    public required VKProfileId Id { get; init; }

    /// <summary>
    /// Gets the preferred display name / roleplay alias in dialogue (e.g. "Hero" or "Alice").
    /// Used by Echo renderers (e.g. [DisplayName]: Hello) and prompt weaving.
    /// Defaults to null, falling back to real name or default speaker.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the preferred response language code (e.g. "zh-CN", "en-US").
    /// </summary>
    public string? PreferredLanguage { get; init; }

    /// <summary>
    /// Gets the profile's timezone identifier (e.g. "Asia/Shanghai", "UTC+8").
    /// Used for relative time resolution (e.g. "today", "tomorrow").
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Gets custom unstructured key-value preferences for lightweight downstream extension.
    /// </summary>
    public IReadOnlyDictionary<string, string> Preferences { get; init; } = new Dictionary<string, string>();
}
