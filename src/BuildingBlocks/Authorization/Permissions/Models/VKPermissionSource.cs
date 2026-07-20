namespace VK.Blocks.Authorization;

/// <summary>
/// Specifies the source of VKPermission data for evaluation.
/// </summary>
public enum VKPermissionSource
{
    /// <summary>Read permissions directly from the user's JWT claims.</summary>
    Claims,

    /// <summary>Read permissions from a persistent store (database).</summary>
    Database
}
