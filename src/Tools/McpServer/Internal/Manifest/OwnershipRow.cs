namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// One row of the ownership map: a capability owned by exactly one module.
/// </summary>
internal sealed record OwnershipRow
{
    public required string Id { get; init; }

    public required string Capability { get; init; }

    public required string Owner { get; init; }

    public required int Line { get; init; }

    public bool IsRetired => Capability.StartsWith("[Retired]", System.StringComparison.OrdinalIgnoreCase);
}
