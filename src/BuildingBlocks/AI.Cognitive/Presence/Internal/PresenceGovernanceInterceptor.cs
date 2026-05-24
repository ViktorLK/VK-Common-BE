using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Pre-inference early governance interceptor that enforces tenant context sandbox freezing,
/// rate limiting gates, and financial token balance gates before RAG or downstream AI processing.
/// Follows CS.01, CS.03, AP.01, and BB.01.
/// </summary>
// [AP.01] Sealed default for classes
internal sealed class PresenceGovernanceInterceptor : IVKCognitivePipelineInterceptor
{
    private readonly IVKPresenceRateLimiter _rateLimiter;
    private readonly IVKPresenceBalanceAuditor _balanceAuditor;
    private readonly IVKConstitutionProvider _constitutionProvider;
    private readonly IVKPresenceQuotaProvider _quotaProvider;

    public int Priority => -200; // Early execution before Intent classification or RAG

    public PresenceGovernanceInterceptor(
        IVKPresenceRateLimiter rateLimiter,
        IVKPresenceBalanceAuditor balanceAuditor,
        IVKConstitutionProvider constitutionProvider,
        IVKPresenceQuotaProvider quotaProvider)
    {
        // [AP.01] Boundary checks using VKGuard
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _balanceAuditor = VKGuard.NotNull(balanceAuditor);
        _constitutionProvider = VKGuard.NotNull(constitutionProvider);
        _quotaProvider = VKGuard.NotNull(quotaProvider);
    }

    public async Task<VKResult> OnBeforeChatAsync(
        VKCognitivePipelineContext context,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] Boundary checks using VKGuard
        VKGuard.NotNull(context);

        // 1. Read presence state (already captured and frozen by Pipeline)
        var state = context.InitialPresenceState;
        if (state is null)
        {
            return VKResult.Failure(PresenceErrors.GovernanceSnapshotMissing); // Or a specific error
        }
        var tenantId = state.TenantId ?? "Default";
        var userId = state.UserId ?? "Anonymous";

        // Validate suspended tenant early
        if (tenantId == "Suspended")
        {
            return VKResult.Failure(PresenceErrors.TenantSuspendedOrNotFound); // [CS.01] Result Pattern used for failure
        }

        // 2. Rate Limiting & QPS Gate [Requirement 2]
        var rateLimitResult = await _rateLimiter.AuditRateLimitAsync(tenantId, userId, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured
        if (rateLimitResult.IsFailure)
        {
            return VKResult.Failure(rateLimitResult.Errors); // [CS.01] Result Pattern used for failure
        }

        // 3. Financial Token Balance Gate [Requirement 3]
        var balanceResult = await _balanceAuditor.AuditBalanceAsync(tenantId, userId, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured
        if (balanceResult.IsFailure)
        {
            return VKResult.Failure(balanceResult.Errors); // [CS.01] Result Pattern used for failure
        }

        // 4. Fetch dynamic constitution per user/tenant
        var constitutionResult = await _constitutionProvider.GetConstitutionAsync(tenantId, userId, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured
        if (constitutionResult.IsFailure)
        {
            return VKResult.Failure(constitutionResult.Errors); // [CS.01] Result Pattern used for failure
        }

        // 5. Fetch dynamic token quota per user/tenant
        var quotaResult = await _quotaProvider.GetQuotaAsync(tenantId, userId, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured
        if (quotaResult.IsFailure)
        {
            return VKResult.Failure(quotaResult.Errors); // [CS.01] Result Pattern used for failure
        }

        // 6. Propagate state downstream in pipeline context to prevent re-evaluation
        context.GovernanceSnapshot = new VKGovernanceSnapshot
        {
            TenantId = tenantId,
            UserId = userId,
            State = state,
            Constitution = constitutionResult.Value,
            Quota = quotaResult.Value
        };

        // Backward compatibility: Pipeline still reads TenantId from Context for Freeze
        if (context.Args is not null)
        {
            context.Args.Context["TenantId"] = tenantId;
        }

        return VKResult.Success(); // [CS.01] Result Pattern used for success
    }

    public Task<VKResult> OnAfterChatAsync(
        VKCognitivePipelineContext context,
        VKChatMessage chatResponse,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success());
    }
}
