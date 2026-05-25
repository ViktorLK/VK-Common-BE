using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Predefined error constants for the Core Presence feature.
/// Follows CS.01 and AP.03.
/// </summary>
public static class VKPresenceErrors
{
    /// <summary>
    /// Error returned when the session is not found or has expired.
    /// </summary>
    public static readonly VKError SessionNotFound = new("AI.Cognitive.Presence.SessionNotFound", "The requested presence session was not found.");

    /// <summary>
    /// Error returned when presence tracking or telemetry fails.
    /// </summary>
    public static readonly VKError TrackingFailed = new("AI.Cognitive.Presence.TrackingFailed", "An error occurred while tracking or analyzing situational presence.");

    /// <summary>
    /// Error returned when recording invalid token values.
    /// </summary>
    public static readonly VKError InvalidTokenUsage = new("AI.Cognitive.Presence.InvalidTokenUsage", "The recorded token values must be non-negative.");
}
