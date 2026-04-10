namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Specifies the comparison operator used for enum-based authorization policies.
/// </summary>
public enum AuthorizationOperator
{
    /// <summary>Requires the claim value to strictly equal the required value.</summary>
    Equals,

    /// <summary>Requires the claim value to be greater than or equal to the required value (Ranking logic).</summary>
    GreaterThanOrEqual,

    /// <summary>Requires the claim value to be less than or equal to the required value.</summary>
    LessThanOrEqual,

    /// <summary>Requires the claim value to be present in a set of allowed values.</summary>
    In
}
