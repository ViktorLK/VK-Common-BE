namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Intent extraction layer.
/// </summary>
public static class VKIntentDiagnosticTokens
{
    // Base offset for Intent layer (Reasoning + 100 range)
    private const int IntentBase = 90100;

    // Logs (Event IDs)
    public const int IntentExtractedEventId = IntentBase + 1;
    public const int IntentExtractionTimeoutEventId = IntentBase + 2;
    public const int CircuitBreakerTrippedEventId = IntentBase + 3;
    public const int CircuitBreakerHalfOpenEventId = IntentBase + 4;
    public const int StructuredOutputFallbackEventId = IntentBase + 5;
    public const int IntentExtractionFailedEventId = IntentBase + 6;
    public const int IntentParsingFailedEventId = IntentBase + 7;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string IntentExtractionDuration = "vk.ai.cognitive.intent.extraction_duration";
        public const string IntentExtractionSource = "vk.ai.cognitive.intent.extraction_source";
        public const string CircuitBreakerState = "vk.ai.cognitive.intent.circuit_breaker_state";
    }
}
