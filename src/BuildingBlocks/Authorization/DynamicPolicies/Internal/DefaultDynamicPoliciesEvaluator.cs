using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Evaluates dynamic requirements (atomic or composite) against values provided by an <see cref="IVKDynamicPoliciesProvider"/>.
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
        var sw = Stopwatch.StartNew();

        // 1. Collect all unique attributes required
        var attributesToLoad = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectAttributes(requirement, attributesToLoad);

        // 2. Load all attributes asynchronously
        var loadedAttributes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var attrName in attributesToLoad)
        {
            var result = await _provider.GetAttributeValueAsync(user, attrName, args.Resource, ct).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                var errorResult = VKResult.Failure<bool>(result.FirstError);
                sw.RecordEvaluation(DynamicPoliciesConstants.FeatureName, errorResult);
                _logger.LogDynamicAuthorizationError(userId, attrName, result.FirstError.Code, result.FirstError.Description);
                return errorResult;
            }
            loadedAttributes[attrName] = result.Value;
        }

        // 3. Build context and specification
        var authContext = new VKAuthorizationContext
        {
            User = user,
            Resource = args.Resource,
            Attributes = loadedAttributes
        };

        try
        {
            var spec = VKDynamicRuleSpecification.BuildSpecification(requirement);
            var isAllowed = spec.IsSatisfiedBy(authContext);
            var finalResult = VKResult.Success(isAllowed);

            sw.RecordEvaluation(DynamicPoliciesConstants.FeatureName, finalResult);

            if (isAllowed)
            {
                _logger.LogDynamicAuthorizationSucceeded(userId, requirement.GetType().Name, "Composite/Atomic Spec");
            }
            else
            {
                _logger.LogDynamicAuthorizationFailed(userId, requirement.GetType().Name, "Composite/Atomic Spec", "Specification not satisfied");
            }

            return finalResult;
        }
        catch (Exception ex)
        {
            var errorResult = VKResult.Failure<bool>(VKAuthorizationErrors.DynamicPoliciesFailed);
            sw.RecordEvaluation(DynamicPoliciesConstants.FeatureName, errorResult);
            _logger.LogDynamicAuthorizationError(userId, requirement.GetType().Name, errorResult.FirstError.Code, ex.Message);
            return errorResult;
        }
    }

    private static void CollectAttributes(IAuthorizationRequirement requirement, HashSet<string> attributes)
    {
        if (requirement is VKDynamicRequirement req)
        {
            attributes.Add(req.Attribute);
        }
        else if (requirement is VKCompositeDynamicRequirement composite)
        {
            foreach (var child in composite.Requirements)
            {
                CollectAttributes(child, attributes);
            }
        }
    }
}
