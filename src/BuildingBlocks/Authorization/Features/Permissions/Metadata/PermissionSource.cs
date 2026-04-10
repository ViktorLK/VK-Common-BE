namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Specifies the source of permission data for evaluation.
/// </summary>
public enum PermissionSource
{
    /// <summary>Read permissions directly from the user's JWT claims.</summary>
    Claims,

    /// <summary>Read permissions from a persistent store (database).</summary>
    Database
}
