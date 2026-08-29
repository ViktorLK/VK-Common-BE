using System;

namespace VK.Blocks.Validation;

/// <summary>
/// Marks a property or field as containing sensitive data (e.g. password, token, credit card),
/// preventing its raw value from being included in validation error messages or attempted value records.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class VKSensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Gets the placeholder mask to display instead of the raw sensitive value.
    /// Default is <c>******</c>.
    /// </summary>
    public string Mask { get; init; } = "******";
}
