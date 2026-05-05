using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Evaluates dynamic requirements against values provided by an <see cref="IVKDynamicPoliciesProvider"/>.
/// </summary>
internal sealed class DefaultDynamicPoliciesEvaluator(
    IVKDynamicPoliciesProvider provider,
    ILogger<DefaultDynamicPoliciesEvaluator> logger)
    : IVKDynamicPoliciesEvaluator
{
    private readonly IVKDynamicPoliciesProvider _provider = VKGuard.NotNull(provider);
    private readonly ILogger<DefaultDynamicPoliciesEvaluator> _logger = VKGuard.NotNull(logger);
    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> EvaluateAsync(
        ClaimsPrincipal user,
        VKDynamicPoliciesArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        args ??= VKDynamicPoliciesArgs.Empty;
        var requirement = args.Requirement;
        if (requirement is null)
        {
            return VKResult.Success(false);
        }
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;
        var requirementName = $"{requirement.Attribute} {requirement.Operator} {requirement.Value}";

        var sw = Stopwatch.StartNew();
        var result = await _provider.GetAttributeValueAsync(user, requirement.Attribute, ct)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorResult = VKResult.Failure<bool>(result.FirstError);
            sw.RecordEvaluation(DynamicPoliciesConstants.FeatureName, errorResult);
            _logger.LogDynamicAuthorizationError(userId, requirementName, result.FirstError.Code, result.FirstError.Description);
            return errorResult;
        }


        var claimValue = result.Value;

        var finalResult = requirement.Operator switch
        {
            DynamicPoliciesConstants.OperatorEquals => VKResult.Success(string.Equals(claimValue, requirement.Value?.ToString(), StringComparison.OrdinalIgnoreCase)),
            DynamicPoliciesConstants.OperatorExists => VKResult.Success(claimValue != null),
            DynamicPoliciesConstants.OperatorContains => VKResult.Success(
                claimValue != null &&
                requirement.Value != null &&
                claimValue.Contains(requirement.Value.ToString()!, StringComparison.OrdinalIgnoreCase)),
            _ => VKResult.Failure<bool>(VKAuthorizationErrors.InvalidOperator)
        };

        sw.RecordEvaluation(DynamicPoliciesConstants.FeatureName, finalResult);

        if (finalResult.IsSuccess && finalResult.Value)
        {
            _logger.LogDynamicAuthorizationSucceeded(userId, requirementName, requirement.Operator);
        }
        else if (finalResult.IsSuccess)
        {
            _logger.LogDynamicAuthorizationFailed(userId, requirementName, requirement.Operator, "Value mismatch");
        }
        else
        {
            _logger.LogDynamicAuthorizationError(userId, requirementName, finalResult.FirstError.Code, finalResult.FirstError.Description);
        }


        return finalResult;
    }
}
