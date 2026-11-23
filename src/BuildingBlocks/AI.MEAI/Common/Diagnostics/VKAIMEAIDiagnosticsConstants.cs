namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Diagnostic constants and semantic tokens for VK.Blocks.AI.MEAI.
/// Adheres to BB.04 and ARCH-12.
/// </summary>
public static class VKAIMEAIDiagnosticsConstants
{
    /// <summary>
    /// Service and activity source name.
    /// </summary>
    public const string ServiceName = "VK.Blocks.AI.MEAI";

    /// <summary>
    /// Named HttpClient for resilient AI transport pipelines.
    /// </summary>
    public const string HttpClientName = "VK.Blocks.AI.MEAI.Http";

    /// <summary>
    /// Meter name for GenAI metrics.
    /// </summary>
    public const string MeterName = "VK.Blocks.AI.MEAI";

    /// <summary>
    /// Gets the provider-isolated named HttpClient identifier for per-provider transport resilience pipeline isolation (OWN-TRANSPORT-RESILIENCE, MEA-06).
    /// </summary>
    public static string GetHttpClientName(VKAIProviderType provider) => $"{HttpClientName}.{provider}";

    /// <summary>
    /// Gets the named transport resilience pipeline identifier for the specified HttpClient name (OWN-TRANSPORT-RESILIENCE, MEA-06).
    /// </summary>
    public static string GetTransportPipelineName(string clientName) => $"ai:meai:transport:{clientName}";
}

