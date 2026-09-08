using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Carries the context and results of intent orchestration.
/// Priority fields (Intent, Confidence, QueryCue, PersonaHint) are ordered first
/// to enable early consumption during streaming JSON parse scenarios.
/// </summary>
public sealed record VKIntentContext
{
    // === Priority Fields (available earliest during streaming parse) ===

    /// <summary>
    /// Gets the primary identified intent.
    /// </summary>
    public required VKIntent Intent { get; init; }

    /// <summary>
    /// Gets the confidence score of the intent identification (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Gets the extracted query cue for downstream memory retrieval.
    /// When non-null, this is a refined search query derived from the user's input context.
    /// </summary>
    public string? QueryCue { get; init; }

    /// <summary>
    /// Gets the persona hint for persona routing/switching.
    /// When non-null, indicates the user's input suggests a specific persona context.
    /// </summary>
    public string? PersonaHint { get; init; }

    /// <summary>
    /// Gets the detected primary emotion of the user (e.g., "Happy", "Frustrated", "Venting").
    /// </summary>
    public string? Emotion { get; init; }

    /// <summary>
    /// Gets the intensity of the detected emotion on a 1-10 scale.
    /// </summary>
    public int? EmotionIntensity { get; init; }

    /// <summary>
    /// Gets the complexity level of user request ("Simple" | "Complex").
    /// </summary>
    public string? Complexity { get; init; }

    /// <summary>
    /// Gets the urgency level of the request ("Low" | "Medium" | "High").
    /// </summary>
    public string? Urgency { get; init; }

    /// <summary>
    /// Gets the domain/topic tag (e.g., "Technical", "Billing", "General").
    /// </summary>
    public string? Topic { get; init; }

    /// <summary>
    /// Gets the detected language code (e.g., "zh", "ja", "en").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets whether fulfilling this request requires external knowledge or long-term memory retrieval.
    /// </summary>
    public bool RequiresKnowledge { get; init; }

    /// <summary>
    /// Gets the safety category classification ("Safe" | "Sensitive" | "Harmful").
    /// </summary>
    public string? SafetyCategory { get; init; }

    // === Standard Fields ===

    /// <summary>
    /// Gets the refined input or command extracted from the raw input.
    /// </summary>
    public string? RefinedInput { get; init; }

    /// <summary>
    /// Gets the schema version of this intent context for forward-compatible evolution.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Gets the source of the intent resolution (e.g., "LLM.Structured", "LLM.EidosBound", "Heuristic").
    /// Used for observability and debugging.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets additional metadata or parameters extracted from the input.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
