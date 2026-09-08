using System.ComponentModel;
using System.Text.Json.Serialization;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// Structured LLM output DTO for pre-turn intent, emotion, and context triage.
/// Follows AP.01 (sealed record) and AP.03 (internal model, no VK prefix).
/// Formatted with schema annotations for AI.Eidos automated contract generation.
/// </summary>
internal sealed record IntentTriageDto // [AP.01] sealed record, [AP.03] internal model
{
    /// <summary>
    /// The user's primary conversational intent for this turn.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("intent")]
    [Description("The primary user intent this turn. One of: chat, roleplay, consulting, task, system.")]
    public required string Intent { get; init; }

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("confidence")]
    [Description("Confidence score of intent identification, between 0.0 and 1.0.")]
    public required double Confidence { get; init; }

    /// <summary>
    /// The user's detected emotional state this turn.
    /// </summary>
    [JsonPropertyName("emotion")]
    [Description("Primary detected user emotion, e.g. happy, sad, anxious, angry, frustrated, neutral. Omit if none.")]
    public string? Emotion { get; init; }

    /// <summary>
    /// Intensity of the detected emotion, on a 1-10 scale.
    /// </summary>
    [JsonPropertyName("emotion_intensity")]
    [Description("Intensity of the detected emotion on a 1-10 scale. Omit if emotion is not set.")]
    public int? EmotionIntensity { get; init; }

    /// <summary>
    /// Complexity level of the user request.
    /// </summary>
    [JsonPropertyName("complexity")]
    [Description("Complexity level of the input: simple (direct answer) or complex (requires multi-step planning).")]
    public string? Complexity { get; init; }

    /// <summary>
    /// Urgency level of the request.
    /// </summary>
    [JsonPropertyName("urgency")]
    [Description("Urgency level: low, medium, or high.")]
    public string? Urgency { get; init; }

    /// <summary>
    /// Primary topic or domain keyword.
    /// </summary>
    [JsonPropertyName("topic")]
    [Description("Primary topic or domain keyword, e.g. general, technical, billing, account.")]
    public string? Topic { get; init; }

    /// <summary>
    /// Detected language code of the user input.
    /// </summary>
    [JsonPropertyName("language")]
    [Description("Detected ISO language code of user input, e.g. zh, ja, en.")]
    public string? Language { get; init; }

    /// <summary>
    /// Indicates whether answering requires domain knowledge or long-term memory retrieval.
    /// </summary>
    [JsonPropertyName("requires_knowledge")]
    [Description("True if fulfilling this turn requires domain knowledge base or long-term memory retrieval.")]
    public bool? RequiresKnowledge { get; init; }

    /// <summary>
    /// Early safety triage category.
    /// </summary>
    [JsonPropertyName("safety_category")]
    [Description("Early safety classification: safe, sensitive, or harmful.")]
    public string? SafetyCategory { get; init; }

    /// <summary>
    /// Refined query cue for downstream memory or corpus retrieval.
    /// </summary>
    [JsonPropertyName("query_cue")]
    [Description("Refined semantic search query for memory or corpus recall. Null if not applicable.")]
    public string? QueryCue { get; init; }

    /// <summary>
    /// Persona hint for persona routing or adaptation.
    /// </summary>
    [JsonPropertyName("persona_hint")]
    [Description("Persona context or style hint suggested by input. Null if not applicable.")]
    public string? PersonaHint { get; init; }

    /// <summary>
    /// Cleaned and normalized user message.
    /// </summary>
    [JsonPropertyName("refined_input")]
    [Description("Cleaned, normalized version of the user input.")]
    public string? RefinedInput { get; init; }
}
