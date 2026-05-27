using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

internal sealed class PresenceGovernancePipelineStage : IVKOrchestrationPipelineStage
{
    private readonly IVKPresenceRateLimiter _rateLimiter;
    private readonly IVKPresenceBalanceAuditor _balanceAuditor;
    private readonly IVKConstitutionProvider _constitutionProvider;
    private readonly IVKPresenceQuotaProvider _quotaProvider;

    public int Order => 150;

    public bool IsActive => true;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public PresenceGovernancePipelineStage(
        IVKPresenceRateLimiter rateLimiter,
        IVKPresenceBalanceAuditor balanceAuditor,
        IVKConstitutionProvider constitutionProvider,
        IVKPresenceQuotaProvider quotaProvider)
    {
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _balanceAuditor = VKGuard.NotNull(balanceAuditor);
        _constitutionProvider = VKGuard.NotNull(constitutionProvider);
        _quotaProvider = VKGuard.NotNull(quotaProvider);
    }

    public async Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var state = context.InitialPresenceState;
        if (state is null)
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(PresenceErrors.GovernanceSnapshotMissing.Description);
            return;
        }

        var tenantId = state.TenantId ?? "Default";
        var userId = state.UserId ?? "Anonymous";

        if (tenantId == "Suspended")
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(PresenceErrors.TenantSuspendedOrNotFound.Description);
            return;
        }

        var rateLimitResult = await _rateLimiter.AuditRateLimitAsync(tenantId, userId, ct).ConfigureAwait(false);
        if (rateLimitResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(rateLimitResult.FirstError.Description);
            return;
        }

        var balanceResult = await _balanceAuditor.AuditBalanceAsync(tenantId, userId, ct).ConfigureAwait(false);
        if (balanceResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(balanceResult.FirstError.Description);
            return;
        }

        var constitutionResult = await _constitutionProvider.GetConstitutionAsync(tenantId, userId, ct).ConfigureAwait(false);
        if (constitutionResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(constitutionResult.FirstError.Description);
            return;
        }

        var quotaResult = await _quotaProvider.GetQuotaAsync(tenantId, userId, ct).ConfigureAwait(false);
        if (quotaResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<PresenceGovernancePipelineStage>(quotaResult.FirstError.Description);
            return;
        }

        context.GovernanceSnapshot = new VKGovernanceSnapshot
        {
            TenantId = tenantId,
            UserId = userId,
            State = state,
            Constitution = constitutionResult.Value,
            Quota = quotaResult.Value
        };

        if (context.Args is not null)
        {
            context.Args.Context["TenantId"] = tenantId;
        }
    }
}
