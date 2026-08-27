using VK.Blocks.Core;

namespace VK.Blocks.Testing;

/// <summary>
/// Thread-safe AsyncLocal based test identity context provider for simulating current Tenant and User.
/// </summary>
public sealed class VKTestIdentityContext
{
    /// <summary>
    /// Gets the current ambient TenantId, or default if unassigned.
    /// </summary>
    public static VKTenantId TenantId => VKAmbientExecutionContext.Current?.TenantId ?? VKTenantId.Default;

    /// <summary>
    /// Gets the current ambient UserId, or anonymous if unassigned.
    /// </summary>
    public static VKUserId UserId => VKAmbientExecutionContext.Current?.UserId ?? VKUserId.Anonymous;

    /// <summary>
    /// Sets the ambient TenantId for the current async flow and returns a disposable scope to revert it.
    /// </summary>
    public static IDisposable SetTenant(VKTenantId tenantId)
    {
        return VKAmbientExecutionContext.BeginScope(tenantId);
    }

    /// <summary>
    /// Sets the ambient UserId for the current async flow and returns a disposable scope to revert it.
    /// </summary>
    public static IDisposable SetUser(VKUserId userId)
    {
        return VKAmbientExecutionContext.BeginScope(userId);
    }

    /// <summary>
    /// Resets the ambient execution context.
    /// </summary>
    public static void Reset()
    {
        // Handled naturally by AsyncLocal scope disposal or empty context
    }

    private sealed class DisposableAction(Action action) : IDisposable
    {
        private Action? _action = action;

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
        }
    }
}
