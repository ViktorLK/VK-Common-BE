using System;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Responsible for rendering user profile presence metadata into prompt instruction text.
/// Follows AP.01 and AP.03.
/// </summary>
public interface IVKProfileRenderer
{
    /// <summary>
    /// Renders the specified profile presence aggregate into prompt instruction text.
    /// </summary>
    /// <param name="profile">The user profile presence aggregate.</param>
    /// <param name="referenceTime">Optional reference time for rendering current time context (defaults to UtcNow).</param>
    /// <returns>The rendered prompt instruction text.</returns>
    string Render(VKProfilePresence profile, DateTimeOffset? referenceTime = null);
}
