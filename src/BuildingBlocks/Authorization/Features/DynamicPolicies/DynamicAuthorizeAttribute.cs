using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Marks a controller or action as requiring dynamic evaluation based on specified attributes.
/// Uses .NET's modern <see cref="IAuthorizationRequirementData"/> to bypass string-based policy parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class DynamicAuthorizeAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicAuthorizeAttribute"/> class.
    /// </summary>
    /// <param name="attribute">The claim or attribute name to evaluate.</param>
    /// <param name="operator">The operator to apply (e.g., Equals, Exists, Contains).</param>
    /// <param name="value">The expected value to compare against.</param>
    public DynamicAuthorizeAttribute(string attribute, string @operator, object? value = null)
    {
        Attribute = attribute;
        Operator = @operator;
        Value = value;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The claim or attribute to evaluate.
    /// </summary>
    public string Attribute { get; }

    /// <summary>
    /// The operator used for evaluation.
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// The expected value to compare against.
    /// </summary>
    public object? Value { get; }

    #endregion

    #region Implementation of IAuthorizationRequirementData

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

    #endregion
}
