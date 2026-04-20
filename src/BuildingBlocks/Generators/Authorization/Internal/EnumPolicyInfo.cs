namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for enum policy information used during the generation process.
/// </summary>
/// <param name="Namespace">The namespace of the enum.</param>
/// <param name="Name">The name of the enum.</param>
/// <param name="FullName">The full type name of the enum.</param>
/// <param name="Operator">The authorization operator to apply.</param>
/// <param name="ClaimType">The claim type to check against, if specified.</param>
internal sealed record EnumPolicyInfo(
    string Namespace,
    string Name,
    string FullName,
    string Operator,
    string? ClaimType);
