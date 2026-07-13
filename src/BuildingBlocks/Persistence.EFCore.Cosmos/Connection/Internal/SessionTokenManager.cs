namespace VK.Blocks.Persistence.Cosmos.Connection.Internal;

/// <summary>
/// Manages Cosmos DB session tokens per asynchronous execution context to guarantee read-your-own-writes consistency.
/// </summary>
internal sealed class SessionTokenManager : IVKCosmosSessionTokenAccessor
{
    private static readonly System.Threading.AsyncLocal<string?> _sessionToken = new();

    /// <summary>
    /// Gets the current session token.
    /// </summary>
    public string? CurrentToken => _sessionToken.Value;

    /// <summary>
    /// Captures a session token for the current execution context.
    /// </summary>
    /// <param name="sessionToken">The session token string.</param>
    public void Capture(string? sessionToken)
    {
        _sessionToken.Value = sessionToken;
    }
}
