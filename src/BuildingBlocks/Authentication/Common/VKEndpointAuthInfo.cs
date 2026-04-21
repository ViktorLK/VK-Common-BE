namespace VK.Blocks.Authentication;

/// <summary>
/// Represents descriptive metadata for an authenticated endpoint.
/// This model is populated by the source generator to provide a view of the authentication requirements.
/// </summary>
public sealed record VKEndpointAuthInfo
{
    /// <summary>
    /// Gets the full name of the endpoint (e.g. "Namespace.Controller.Method").
    /// </summary>
    public required string EndpointName { get; init; }

    /// <summary>
    /// Gets the authentication group assigned to this endpoint, if any.
    /// </summary>
    public string? AuthGroup { get; init; }

    /// <summary>
    /// Gets the list of authentication schemes required by this endpoint.
    /// </summary>
    public string[] Schemes { get; init; } = [];

    /// <summary>
    /// Gets the list of authentication policies applied to this endpoint.
    /// </summary>
    public string[] Policies { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the endpoint allows anonymous access.
    /// </summary>
    public bool IsAnonymous { get; init; }
}




