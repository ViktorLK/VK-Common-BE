using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Immutable value object representing a validated email address.
/// Follows AP.01 (sealed record).
/// </summary>
[VKValueObject]
public sealed partial record VKEmail
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    /// <summary>
    /// Gets the normalized lowercase email value.
    /// </summary>
    public string Value { get; init; }

    private VKEmail(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="VKEmail"/> instance after validating format.
    /// </summary>
    public static VKResult<VKEmail> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return VKResult.Failure<VKEmail>(VKUserErrors.EmailRequired);
        }

        var trimmed = email.Trim().ToLowerInvariant();
        if (trimmed.Length > 256 || !EmailRegex().IsMatch(trimmed))
        {
            return VKResult.Failure<VKEmail>(VKUserErrors.EmailInvalidFormat);
        }

        return VKResult.Success(new VKEmail(trimmed));
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    public static implicit operator string(VKEmail email) => email.Value;
}
