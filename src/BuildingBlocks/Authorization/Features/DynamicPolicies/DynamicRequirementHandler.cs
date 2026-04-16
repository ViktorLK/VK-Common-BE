using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Common;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Handles authorization checks for <see cref="DynamicRequirement"/> using <see cref="IDynamicPolicyEvaluator"/>.
/// </summary>
public sealed class DynamicRequirementHandler(IDynamicPolicyEvaluator evaluator)
    : AuthorizationHandler<DynamicRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var sw = Stopwatch.StartNew();
        var result = await evaluator.EvaluateAsync(context.User, requirement, default)
            .ConfigureAwait(false);

        // 1. Record evaluation metrics (Rule 6 Compliance)
        sw.RecordEvaluation($"DynamicPolicy:{requirement.Attribute}", result);

        context.ApplyResult(requirement, result, this);
    }
}
