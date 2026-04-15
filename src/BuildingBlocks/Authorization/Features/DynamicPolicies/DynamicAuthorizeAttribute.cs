using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Marks a controller or action as requiring dynamic evaluation based on specified attributes.
/// Uses .NET's modern <see cref="IAuthorizationRequirementData"/> to bypass string-based policy parsing.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DynamicAuthorizeAttribute"/> class.
/// </remarks>
/// <param name="attribute">The claim or attribute name to evaluate.</param>
/// <param name="operator">The operator to apply (e.g., Equals, Exists, Contains).</param>
/// <param name="value">The expected value to compare against.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class DynamicAuthorizeAttribute(string attribute, string @operator, object? value = null) : AuthorizeAttribute, IAuthorizationRequirementData
{

    /// <summary>
    /// Gets the claim or attribute to evaluate.
    /// </summary>
    public string Attribute { get; } = attribute;

    /// <summary>
    /// Gets the operator used for evaluation.
    /// </summary>
    public string Operator { get; } = @operator;

    /// <summary>
    /// Gets the expected value to compare against.
    /// </summary>
    public object? Value { get; } = value;

    /// <summary>
    /// Returns the authorization requirements defined by this attribute.
    /// </summary>
    /// <returns>A collection of requirements.</returns>
    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new DynamicRequirement
        {
            Name = $"Dynamic:{Attribute}:{Operator}",
            Attribute = Attribute,
            Operator = Operator,
            Value = Value
        };
    }
}
