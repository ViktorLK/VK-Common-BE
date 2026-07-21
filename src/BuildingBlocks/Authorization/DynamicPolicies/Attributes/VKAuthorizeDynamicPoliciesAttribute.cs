using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring a dynamic policy evaluation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class VKAuthorizeDynamicPoliciesAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKAuthorizeDynamicPoliciesAttribute"/> class.
    /// </summary>
    /// <param name="attribute">The attribute name to check.</param>
    /// <param name="op">The operator (e.g., Equals, Exists, Contains).</param>
    /// <param name="value">The expected value.</param>
    public VKAuthorizeDynamicPoliciesAttribute(string attribute, string op, object value)
    {
        Attribute = attribute;
        Operator = op;
        Value = value;
    }

    /// <summary>
    /// Gets the attribute name to check.
    /// </summary>
    public string Attribute { get; }

    /// <summary>
    /// Gets the operator for comparison.
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// Gets the expected value.
    /// </summary>
    public object Value { get; }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKDynamicRequirement
        {
            Attribute = Attribute,
            Operator = Operator,
            Value = Value,
            Name = $"{Attribute} {Operator} {Value}"
        };
    }
}
