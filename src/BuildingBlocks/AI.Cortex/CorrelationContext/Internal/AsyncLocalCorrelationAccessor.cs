using System;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex.CorrelationContext.Internal;

/// <summary>
/// Default AsyncLocal-backed implementation of <see cref="IVKCortexCorrelationAccessor"/> with leak-safe scope restoration.
/// Follows [AP.01].
/// </summary>
internal sealed class AsyncLocalCorrelationAccessor : IVKCortexCorrelationAccessor
{
    private static readonly AsyncLocal<VKCortexCorrelationContext?> Current = new();

    /// <inheritdoc />
    public VKCortexCorrelationContext? CurrentContext => Current.Value;

    /// <inheritdoc />
    public IDisposable BeginScope(VKCortexCorrelationContext context)
    {
        VKGuard.NotNull(context);
        var prior = Current.Value;
        Current.Value = context;
        return new ScopeToken(prior);
    }

    private sealed class ScopeToken : IDisposable
    {
        private readonly VKCortexCorrelationContext? _prior;
        private int _disposed;

        public ScopeToken(VKCortexCorrelationContext? prior)
        {
            _prior = prior;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                Current.Value = _prior;
            }
        }
    }
}
