using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default fallback implementation of <see cref="IVKPresenceBalanceAuditor"/> that always grants access.
/// Downstream modules can register their own implementation to query database budgets.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class DefaultBalanceAuditor : IVKPresenceBalanceAuditor
{
    /// <inheritdoc />
    public Task<VKResult> AuditBalanceAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(tenantId);
        VKGuard.NotNullOrWhiteSpace(userId);

        return Task.FromResult(VKResult.Success()); // [CS.01]
    }
}
