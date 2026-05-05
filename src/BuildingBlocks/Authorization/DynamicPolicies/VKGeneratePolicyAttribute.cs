using System;

namespace VK.Blocks.Authorization;

/// <summary>
/// Triggers the automatic generation of an authorization policy, attribute, and handler for the decorated enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class VKGeneratePolicyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the comparison operator for the policy. Defaults to <see cref="VKAuthorizationOperator.GreaterThanOrEqual"/>.
    /// </summary>
    public VKAuthorizationOperator Operator { get; set; } = VKAuthorizationOperator.GreaterThanOrEqual;

    /// <summary>
    /// Gets or sets the claim type to read the value from. 
    /// If null, a default behavior based on the enum name will be used.
    /// </summary>
    public string? ClaimType { get; set; }
}
