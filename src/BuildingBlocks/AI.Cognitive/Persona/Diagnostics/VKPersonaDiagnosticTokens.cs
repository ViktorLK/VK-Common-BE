namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Persona feature.
/// </summary>
public static class VKPersonaDiagnosticTokens
{
    // Logs (Event IDs)
    public const int PersonaLoadedEventId = 400;
    public const int PersonaSaveFailedEventId = 401;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string LoadDuration = "vk.ai.cognitive.persona.load_duration";
        public const string ActivePersonasCount = "vk.ai.cognitive.persona.active_count";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string PersonaId = "vk.ai.persona.id";
        public const string Role = "vk.ai.persona.role";
    }
}
