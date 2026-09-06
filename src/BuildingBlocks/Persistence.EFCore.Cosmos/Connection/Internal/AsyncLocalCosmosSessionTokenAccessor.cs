using System;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Connection.Internal;

/// <summary>
/// Manages Cosmos DB session tokens per asynchronous execution context to guarantee read-your-own-writes consistency.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class AsyncLocalCosmosSessionTokenAccessor : IVKCosmosSessionTokenAccessor
{
    private static readonly AsyncLocal<string?> CurrentContext = new();

    /// <inheritdoc />
    public string? CurrentToken => CurrentContext.Value;

    /// <inheritdoc />
    public void Capture(string? sessionToken)
    {
        CurrentContext.Value = sessionToken;
    }

    /// <inheritdoc />
    public IDisposable BeginScope(string? sessionToken)
    {
        var prior = CurrentContext.Value;
        CurrentContext.Value = sessionToken;
        return new ScopeToken(prior);
    }

    private sealed class ScopeToken : IDisposable
    {
        private readonly string? _prior;
        private int _disposed;

        public ScopeToken(string? prior)
        {
            _prior = prior;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                CurrentContext.Value = _prior;
            }
        }
    }
}
