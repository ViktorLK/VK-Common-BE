using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration options for the Intent Layer within AI.Cognitive.
/// Controls LLM extraction timeouts, circuit breaker thresholds, and structured output preferences.
/// Follows AP.01 (sealed) and BB.07 (isolated options file).
/// </summary>
public sealed partial record VKIntentOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets whether intent extraction is enabled.
    /// Default: true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the SLA timeout for LLM intent extraction in milliseconds.
    /// If the LLM call exceeds this duration, the system falls back to heuristic classification.
    /// Default: 500ms.
    /// </summary>
    public int ExtractionTimeoutMs { get; init; } = 500;

    /// <summary>
    /// Gets the maximum number of recent chat history messages to include in the intent prompt.
    /// Keeping this low reduces prefill token cost and latency.
    /// Default: 3.
    /// </summary>
    public int MaxHistoryMessages { get; init; } = 3;

    /// <summary>
    /// Gets whether to prefer structured JSON output (SendStructuredAsync) over free-text parsing.
    /// When true and the provider supports it, uses JSON Schema-constrained generation for reliable output.
    /// Falls back to free-text parsing automatically if the provider does not support structured output.
    /// Default: true.
    /// </summary>
    public bool PreferStructuredOutput { get; init; } = true;

    /// <summary>
    /// Gets the circuit breaker failure threshold before tripping.
    /// After this many consecutive LLM failures/timeouts, the circuit trips and routes directly to heuristic fallback.
    /// Default: 3.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; init; } = 3;

    /// <summary>
    /// Gets the circuit breaker recovery window in seconds.
    /// After the circuit trips, it enters half-open state after this duration to retry LLM calls.
    /// Default: 30 seconds.
    /// </summary>
    public int CircuitBreakerRecoveryWindowSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the minimum confidence threshold below which results are treated as low-confidence.
    /// Low-confidence results may trigger downstream fallback behaviors.
    /// Default: 0.4.
    /// </summary>
    public double LowConfidenceThreshold { get; init; } = 0.4;

    /// <summary>
    /// Gets the default confidence threshold.
    /// Default: 0.5.
    /// </summary>
    public double ConfidenceThreshold { get; init; } = 0.5;

    /// <summary>
    /// Gets the optional model name override for the intent LLM call.
    /// When null, uses the default model configured in the chat engine.
    /// Typically set to a small/fast model (e.g., GPT-4o-mini) for cost optimization.
    /// </summary>
    public string? ModelName { get; init; }
}
