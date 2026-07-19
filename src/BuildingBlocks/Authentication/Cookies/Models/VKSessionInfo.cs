using System;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents information about an active authentication session.
/// </summary>
public sealed record VKSessionInfo
{
    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the user identifier associated with the session.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets or sets the serialized ticket data.
    /// </summary>
    public required string TicketData { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the session expires.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Gets the date and time when the session was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
