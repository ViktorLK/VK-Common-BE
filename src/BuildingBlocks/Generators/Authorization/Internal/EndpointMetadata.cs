using System.Collections.Immutable;

namespace VK.Blocks.Generators.Authorization.Internal;

/// <summary>
/// Intermediate model for endpoint authorization metadata used during the generation process.
/// </summary>
internal sealed record EndpointMetadata(
    string EndpointName,
    ImmutableArray<string> Permissions,
    ImmutableArray<string> Roles,
    string? MinimumRank,
    bool RequiresInternalNetwork,
    bool RequiresWorkingHours);
