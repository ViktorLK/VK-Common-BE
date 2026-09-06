using System;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Connection;

/// <summary>
/// Public accessor for Cosmos DB session token state across asynchronous execution contexts.
/// Follows AP.01, AP.03, and CS.01.
/// </summary>
public interface IVKCosmosSessionTokenAccessor
{
    /// <summary>
    /// Gets the current session token.
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// Captures the session token for the current execution context.
    /// </summary>
    /// <param name="sessionToken">The session token to capture.</param>
    void Capture(string? sessionToken);

    /// <summary>
    /// Begins a scoped execution block with the specified session token, restoring previous token upon disposal.
    /// </summary>
    /// <param name="sessionToken">The session token to scope.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous token when disposed.</returns>
    IDisposable BeginScope(string? sessionToken);
}
