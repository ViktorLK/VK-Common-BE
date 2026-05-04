using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Evaluates <see cref="VKDynamicRequirement"/> by delegating to <see cref="IVKDynamicPoliciesEvaluator"/>.
/// </summary>
internal sealed class DynamicRequirementHandler(
    IVKDynamicPoliciesEvaluator evaluator)
    : AuthorizationHandler<VKDynamicRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VKDynamicRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var sw = Stopwatch.StartNew();
        var result = await evaluator.EvaluateAsync(context.User, requirement, default)
            .ConfigureAwait(false);

        // 1. Record evaluation metrics (Rule 6 Compliance)
        sw.RecordEvaluation($"{DynamicPoliciesConstants.FeatureName}:{requirement.Attribute}", result);

        context.ApplyResult(requirement, result, this);
    }
}
