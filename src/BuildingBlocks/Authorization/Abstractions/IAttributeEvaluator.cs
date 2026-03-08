using System.Security.Claims;
using VK.Blocks.Authorization.Features.DynamicPolicies;

namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Defines an evaluator for dynamic policy requirements based on attributes.
/// </summary>
public interface IAttributeEvaluator
{
    /// <summary>
    /// Evaluates whether the user satisfies the specified dynamic requirement.
    /// </summary>
    bool Evaluate(ClaimsPrincipal user, DynamicRequirement requirement);
}

