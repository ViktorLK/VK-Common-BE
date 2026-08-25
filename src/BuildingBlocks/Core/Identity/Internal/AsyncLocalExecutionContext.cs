using System;
using System.Threading;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Thread-safe single AsyncLocal-backed execution context supporting Level 1 (Tenant), Level 2 (Identity), and Level 3 (Security) tiers.
/// Eliminates state divergence and multiple AsyncLocal overhead.
/// Follows AP.01, AP.03, CS.01, CS.06.
/// </summary>
internal static class AsyncLocalExecutionContext
{
    private static readonly AsyncLocal<IVKTenantContext?> _current = new();

    /// <summary>
    /// Gets the active execution context. Never null (safely falls back to <see cref="DefaultTenantContext.DefaultInstance"/>).
    /// </summary>
    public static IVKTenantContext Current => _current.Value ?? DefaultTenantContext.DefaultInstance;

    /// <summary>
    /// Gets a value indicating whether an explicit execution context has been assigned to the active async flow.
    /// </summary>
    public static bool HasExplicitContext => _current.Value is not null;

    /// <summary>
    /// Pushes an active execution context into the ambient flow and returns a token that reverts to the prior context on disposal.
    /// </summary>
    public static IDisposable BeginScope(IVKTenantContext context)
    {
        VKGuard.NotNull(context);
        var prior = _current.Value;
        _current.Value = context;
        return new ScopeToken(prior);
    }

    private sealed class ScopeToken(IVKTenantContext? prior) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _current.Value = prior;
            }
        }
    }
}
