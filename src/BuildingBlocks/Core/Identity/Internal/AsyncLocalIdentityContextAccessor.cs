using System;
using System.Threading;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Thread-safe AsyncLocal-backed implementation of <see cref="IVKIdentityContextAccessor"/>.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class AsyncLocalIdentityContextAccessor : IVKIdentityContextAccessor
{
    private static readonly AsyncLocal<IVKIdentityContext?> CurrentContext = new();

    /// <inheritdoc />
    public IVKIdentityContext Current => CurrentContext.Value ?? DefaultIdentityContext.Instance;

    /// <inheritdoc />
    public IDisposable BeginScope(VKTenantId tenantId, VKUserId userId)
    {
        VKGuard.NotDefault(tenantId);
        VKGuard.NotDefault(userId);
        return BeginScope(new DefaultIdentityContext(tenantId, userId));
    }

    /// <inheritdoc />
    public IDisposable BeginScope(IVKIdentityContext context)
    {
        VKGuard.NotNull(context);
        var prior = CurrentContext.Value;
        CurrentContext.Value = context;
        return new ScopeToken(prior);
    }

    private sealed class ScopeToken : IDisposable
    {
        private readonly IVKIdentityContext? _prior;
        private int _disposed;

        public ScopeToken(IVKIdentityContext? prior)
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
