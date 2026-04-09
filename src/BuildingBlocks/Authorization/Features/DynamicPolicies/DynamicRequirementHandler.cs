using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authorization.Common;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Handles authorization checks for <see cref="DynamicRequirement"/> using <see cref="IDynamicPolicyEvaluator"/>.
/// </summary>
public sealed class DynamicRequirementHandler(IDynamicPolicyEvaluator evaluator) 
    : AuthorizationHandler<DynamicRequirement>
{
    #region Protected Methods

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await evaluator.EvaluateAsync(context.User, requirement, default)
            .ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    #endregion
}
