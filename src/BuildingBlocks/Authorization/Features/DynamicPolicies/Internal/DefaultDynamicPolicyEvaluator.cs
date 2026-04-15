using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.DynamicPolicies.Internal;

/// <summary>
/// Evaluates dynamic requirements against values provided by an <see cref="IDynamicPolicyProvider"/>.
/// </summary>
public sealed class DefaultDynamicPolicyEvaluator(
    IDynamicPolicyProvider provider,
    ILogger<DefaultDynamicPolicyEvaluator> logger)
    : IDynamicPolicyEvaluator
{
    private const string PolicyName = "DynamicPolicy";

    /// <inheritdoc />
    public async ValueTask<Result<bool>> EvaluateAsync(
        ClaimsPrincipal user,
        DynamicRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";
        var requirementName = $"{requirement.Attribute} {requirement.Operator} {requirement.Value}";

        var sw = Stopwatch.StartNew();
        var result = await provider.GetAttributeValueAsync(user, requirement.Attribute, cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResult = Result.Failure<bool>(result.FirstError);
            sw.RecordEvaluation(PolicyName, errorResult);
            logger.LogDynamicAuthorizationError(userId, requirementName, result.FirstError.Code, result.FirstError.Description);
            return errorResult;
        }

        var claimValue = result.Value;

        var finalResult = requirement.Operator switch
        {
            DynamicPoliciesConstants.OperatorEquals => Result.Success(string.Equals(claimValue, requirement.Value?.ToString(), StringComparison.OrdinalIgnoreCase)),
            DynamicPoliciesConstants.OperatorExists => Result.Success(claimValue != null),
            DynamicPoliciesConstants.OperatorContains => Result.Success(
                claimValue != null &&
                requirement.Value != null &&
                claimValue.Contains(requirement.Value.ToString()!, StringComparison.OrdinalIgnoreCase)),
            _ => Result.Failure<bool>(AuthorizationErrors.InvalidOperator)
        };

        sw.RecordEvaluation(PolicyName, finalResult);

        if (finalResult.IsSuccess && finalResult.Value)
        {
            logger.LogDynamicAuthorizationSucceeded(userId, requirementName, requirement.Operator);
        }
        else if (finalResult.IsSuccess)
        {
            logger.LogDynamicAuthorizationFailed(userId, requirementName, requirement.Operator, "Value mismatch");
        }
        else
        {
            logger.LogDynamicAuthorizationError(userId, requirementName, finalResult.FirstError.Code, finalResult.FirstError.Description);
        }

        return finalResult;
    }
}
