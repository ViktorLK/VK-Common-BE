using System;
using System.Threading;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Thread-safe AsyncLocal-backed implementation of <see cref="IVKSecurityContextAccessor"/>.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class AsyncLocalSecurityContextAccessor : IVKSecurityContextAccessor
{
    private static readonly AsyncLocal<IVKSecurityContext?> CurrentContext = new();

    /// <inheritdoc />
    public IVKSecurityContext Current => CurrentContext.Value ?? DefaultSecurityContext.Instance;

    /// <inheritdoc />
    public IDisposable BeginScope(IVKSecurityContext context)
    {
        VKGuard.NotNull(context);
        var prior = CurrentContext.Value;
        CurrentContext.Value = context;
        return new ScopeToken(prior);
    }

    private sealed class ScopeToken : IDisposable
    {
        private readonly IVKSecurityContext? _prior;
        private int _disposed;

        public ScopeToken(IVKSecurityContext? prior)
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
