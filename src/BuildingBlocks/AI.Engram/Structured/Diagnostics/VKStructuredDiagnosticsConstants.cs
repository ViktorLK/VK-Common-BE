namespace VK.Blocks.AI.Engram;

/// <summary>
/// Public diagnostic tokens for Structured Memory feature.
/// Follows BB.04 / OR.01.
/// </summary>
public static class VKStructuredDiagnosticsConstants
{
    // Event IDs
    public const int FactStoredEventId = 300;
    public const int FactRemovedEventId = 301;
    public const int FactConflictResolvedEventId = 302;
    public const int FactTypeMismatchEventId = 303;
    public const int FactSensitiveAccessEventId = 304;

    // Tags
    public static class Tags
    {
        public const string FactKey = "vk.ai.structured.fact_key";
        public const string TenantId = "vk.tenant.id";
        public const string IsSensitive = "vk.ai.structured.is_sensitive";
    }
}
