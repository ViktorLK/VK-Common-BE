namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for permission information used during the generation process.
/// </summary>
/// <param name="Value">The raw permission value.</param>
/// <param name="Module">The module this permission belongs to.</param>
/// <param name="SuggestedIdentifier">The suggested C# identifier name.</param>
/// <param name="DisplayName">The human-readable name.</param>
/// <param name="Description">The description of the permission.</param>
internal sealed record PermissionInfo(
    string Value,
    string Module,
    string? SuggestedIdentifier,
    string? DisplayName = null,
    string? Description = null);
