using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.DynamicPolicies.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// A specification that wraps a dynamic policy requirement.
/// </summary>
public sealed class VKDynamicRuleSpecification : VKSpecification<VKAuthorizationContext>
{
    private readonly VKDynamicRequirement _requirement;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKDynamicRuleSpecification"/> class.
    /// </summary>
    public VKDynamicRuleSpecification(VKDynamicRequirement requirement)
        : base(context => Evaluate(context, requirement))
    {
        _requirement = requirement;
    }

    private static bool Evaluate(VKAuthorizationContext context, VKDynamicRequirement req)
    {
        if (!context.Attributes.TryGetValue(req.Attribute, out var claimValue) || claimValue is null)
        {
            return false;
        }

        return req.Operator switch
        {
            DynamicPoliciesConstants.OperatorEquals => string.Equals(claimValue, req.Value?.ToString(), StringComparison.OrdinalIgnoreCase),
            DynamicPoliciesConstants.OperatorExists => claimValue is not null,
            DynamicPoliciesConstants.OperatorContains => req.Value is not null && claimValue.Contains(req.Value.ToString()!, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// Recursively builds a <see cref="VKSpecification{T}"/> tree from a requirement.
    /// </summary>
    public static VKSpecification<VKAuthorizationContext> BuildSpecification(IAuthorizationRequirement requirement)
    {
        if (requirement is VKDynamicRequirement req)
        {
            return new VKDynamicRuleSpecification(req);
        }

        if (requirement is VKCompositeDynamicRequirement composite)
        {
            if (composite.Requirements.Count == 0)
            {
                return new VKTrueSpecification();
            }

            var specs = composite.Requirements.Select(BuildSpecification).ToList();

            if (composite.Operator == VKLogicalOperator.And)
            {
                var result = specs[0];
                for (var i = 1; i < specs.Count; i++)
                {
                    result = new VKAndSpecification<VKAuthorizationContext>(result, specs[i]);
                }
                return result;
            }

            if (composite.Operator == VKLogicalOperator.Or)
            {
                var result = specs[0];
                for (var i = 1; i < specs.Count; i++)
                {
                    result = new VKOrSpecification<VKAuthorizationContext>(result, specs[i]);
                }
                return result;
            }

            if (composite.Operator == VKLogicalOperator.Not)
            {
                return new VKNotSpecification<VKAuthorizationContext>(specs[0]);
            }
        }

        throw new ArgumentException($"Unsupported requirement type for dynamic specifications: {requirement.GetType().Name}", nameof(requirement));
    }

    private sealed class VKTrueSpecification : VKSpecification<VKAuthorizationContext>
    {
        public VKTrueSpecification() : base(context => true) { }
    }
}
