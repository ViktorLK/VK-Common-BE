namespace VK.Labs.PersonaWeavePulsar.DependencyInjection;

/// <summary>
/// Configuration options for the PWP backend.
/// </summary>
public sealed record PwpOptions
{
    /// <summary>
    /// Maximum number of messages to fetch from history.
    /// </summary>
    public int TotalContextLimit { get; init; } = 32768;

    /// <summary>
    /// Maximum number of characters to keep in the context window (proxy for tokens).
    /// </summary>
    public int MaxResponseTokens { get; init; } = 2048;

    /// <summary>
    /// SQLite connection string for chat history.
    /// </summary>
    public string HistoryConnection { get; init; } = "Data Source=pwp-history.db";

    /// <summary>
    /// Default global system instructions (L1).
    /// </summary>
    public string DefaultMainPrompt { get; init; } = "You are in a roleplay chat. Write concise, expressive responses. Do not speak for the user.";

    /// <summary>
    /// Default global post-instructions (L1).
    /// </summary>
    public string DefaultPostInstructions { get; init; } = "Ensure responses are in the same language as the user and follow character traits.";

    /// <summary>
    /// Feature-specific toggles for industrial enhancements.
    /// </summary>
    public PwpFeatureOptions Features { get; init; } = new();
}

/// <summary>
/// Toggles for optional "Industrial DNA" features in PWP.
/// Defaults are set to match SillyTavern baseline (off for extras) or on if needed for core stability.
/// </summary>
public sealed record PwpFeatureOptions
{
    /// <summary>
    /// Enables Bio-Feedback sensor integration and emotional state tracking.
    /// </summary>
    public bool EnableBioFeedback { get; init; } = false;

    /// <summary>
    /// Enables the Proactive Engine (background AI pulses during inactivity).
    /// </summary>
    public bool EnableProactiveReflex { get; init; } = false;

    /// <summary>
    /// Enables detailed reasoning path capture (Thought Path) for diagnostics.
    /// </summary>
    public bool EnableDetailedDiagnostics { get; init; } = false;

    /// <summary>
    /// Enables the self-monitoring and engine health service.
    /// </summary>
    public bool EnableSelfMonitor { get; init; } = false;

    /// <summary>
    /// Threshold for inactivity before the Proactive Engine triggers (in minutes).
    /// </summary>
    public int InactivityThresholdMinutes { get; init; } = 5;
}
