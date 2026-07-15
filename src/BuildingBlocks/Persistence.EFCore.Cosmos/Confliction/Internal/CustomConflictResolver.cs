using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Confliction.Internal;

/// <summary>
/// Resolution settings for globally distributed multi-region databases.
/// </summary>
internal sealed class CustomConflictResolver
{
    public void ConfigureLastWriteWins(ContainerProperties containerProperties, string conflictResolutionPath = "/_ts")
    {
        VKGuard.NotNull(containerProperties);
        VKGuard.NotNullOrWhiteSpace(conflictResolutionPath);

        containerProperties.ConflictResolutionPolicy = new ConflictResolutionPolicy
        {
            Mode = ConflictResolutionMode.LastWriterWins,
            ResolutionPath = conflictResolutionPath
        };
    }

    /// <summary>
    /// Configures custom stored procedure-based conflict resolution.
    /// </summary>
    public void ConfigureCustomResolution(ContainerProperties containerProperties, string storedProcedureName)
    {
        VKGuard.NotNull(containerProperties);
        VKGuard.NotNullOrWhiteSpace(storedProcedureName);

        containerProperties.ConflictResolutionPolicy = new ConflictResolutionPolicy
        {
            Mode = ConflictResolutionMode.Custom,
            ResolutionProcedure = storedProcedureName
        };
    }
}
