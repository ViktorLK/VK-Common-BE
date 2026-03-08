using System.Security.Claims;
using VK.Blocks.Authorization.Abstractions;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Evaluates dynamic requirements against a user's claims.
/// </summary>
public sealed class AttributeEvaluator : IAttributeEvaluator
{
    #region Public Methods

    /// <inheritdoc />
    public bool Evaluate(ClaimsPrincipal user, DynamicRequirement requirement)
    {
        var claimValue = user.FindFirst(requirement.Attribute)?.Value;

        return requirement.Operator switch
        {
            DynamicPoliciesConstants.OperatorEquals => Equals(claimValue, requirement.Value?.ToString()),
            DynamicPoliciesConstants.OperatorExists => claimValue != null,
            _ => false
        };
    }

    #endregion
}

