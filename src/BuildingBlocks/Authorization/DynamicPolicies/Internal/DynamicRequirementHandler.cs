using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Evaluates <see cref="VKDynamicRequirement"/> by delegating to <see cref="IVKDynamicPoliciesEvaluator"/>.
/// </summary>
internal sealed class DynamicRequirementHandler(
    IVKDynamicPoliciesEvaluator evaluator,
    IOptions<VKAuthorizationOptions> globalOptions,
    ILogger<DynamicRequirementHandler> logger,
    IVKAuthorizationAuditHook? auditHook = null)
    : AuthorizationHandler<IAuthorizationRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (requirement is not VKDynamicRequirement && requirement is not VKCompositeDynamicRequirement)
        {
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var sw = Stopwatch.StartNew();
        var result = await evaluator.EvaluateAsync(context.User, new VKDynamicPoliciesArgs { Requirement = requirement, Resource = context.Resource }, default)
            .ConfigureAwait(false);

        var requirementName = requirement is VKDynamicRequirement req 
            ? $"{DynamicPoliciesConstants.FeatureName}:{req.Attribute}" 
            : $"{DynamicPoliciesConstants.FeatureName}:Composite";

        if (!result.IsSuccess || !result.Value)
        {
            if (globalOptions.Value.ShouldFailOpen(DynamicPoliciesConstants.FeatureName, logger))
            {
                result = VKResult.Success(true);
            }
        }

        // 1. Record evaluation metrics (OR.01 Compliance)
        sw.RecordEvaluation(requirementName, result);

        if (auditHook is not null)
        {
            await auditHook.AuditDecisionAsync(requirementName, context.User, result, default).ConfigureAwait(false);
        }

        context.ApplyResult(requirement, result, this);
    }
}
