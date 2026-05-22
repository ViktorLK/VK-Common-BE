using System;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Thread-safe AsyncLocal-based implementation of <see cref="IVKTenantContextAccessor"/>.
/// Follows AP.01, AP.03, and the Sandboxing requirement.
/// </summary>
internal sealed class DefaultTenantContextAccessor : IVKTenantContextAccessor
{
    private static readonly AsyncLocal<string?> CurrentTenantId = new();

    /// <inheritdoc />
    public IDisposable Freeze(string tenantId)
    {
        VKGuard.NotNullOrWhiteSpace(tenantId);

        var previous = CurrentTenantId.Value;
        CurrentTenantId.Value = tenantId;

        return new DisposableScope(() =>
        {
            CurrentTenantId.Value = previous;
        });
    }

    /// <inheritdoc />
    public string? GetActiveTenantId() => CurrentTenantId.Value;

    /// <summary>
    /// Thread-safe scope cleanup helper.
    /// </summary>
    private sealed class DisposableScope : IDisposable
    {
        private readonly Action _disposeAction;
        private int _disposed;

        public DisposableScope(Action disposeAction)
        {
            _disposeAction = VKGuard.NotNull(disposeAction);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _disposeAction();
            }
        }
    }
}
