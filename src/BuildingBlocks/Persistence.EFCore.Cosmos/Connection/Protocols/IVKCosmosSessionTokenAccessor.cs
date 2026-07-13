namespace VK.Blocks.Persistence.Cosmos.Connection;

/// <summary>
/// Public accessor for Cosmos DB session token state.
/// </summary>
public interface IVKCosmosSessionTokenAccessor
{
    /// <summary>
    /// Gets the current session token.
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// Captures the session token.
    /// </summary>
    /// <param name="sessionToken">The session token to capture.</param>
    void Capture(string? sessionToken);
}
