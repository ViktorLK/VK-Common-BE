using System;

using VK.Blocks.Authorization.Abstractions;

namespace VK.Blocks.Authorization.Features.DynamicPolicies.Metadata;

/// <summary>
/// Triggers the automatic generation of an authorization policy, attribute, and handler for the decorated enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class GeneratePolicyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the comparison operator for the policy. Defaults to <see cref="AuthorizationOperator.GreaterThanOrEqual"/>.
    /// </summary>
    public AuthorizationOperator Operator { get; set; } = AuthorizationOperator.GreaterThanOrEqual;

    /// <summary>
    /// Gets or sets the claim type to read the value from. 
    /// If null, a default behavior based on the enum name will be used.
    /// </summary>
    public string? ClaimType { get; set; }
}
