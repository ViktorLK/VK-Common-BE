using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Value object representing a contract version and compatibility markers.
/// </summary>
public sealed record VKAIEidosContractVersion
{
    public required string VersionName { get; init; }
    public int Major { get; init; } = 1;
    public int Minor { get; init; } = 0;
    public bool IsBackwardCompatible { get; init; } = true;

    public static VKAIEidosContractVersion V1 { get; } = new() { VersionName = "v1.0", Major = 1, Minor = 0 };
}
