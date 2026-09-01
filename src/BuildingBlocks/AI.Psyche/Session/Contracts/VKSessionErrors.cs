using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Session feature.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static error definitions and constant descriptors.")]
public static class VKSessionErrors
{
    /// <summary>
    /// Error returned when the session thread was not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Session.NotFound", "The requested session thread was not found.");

    /// <summary>
    /// Error returned when the session thread is not in an active operational status.
    /// </summary>
    public static readonly VKError SessionNotActive = new("AI.Session.NotActive", "The requested session thread is not in an active status.");
}
