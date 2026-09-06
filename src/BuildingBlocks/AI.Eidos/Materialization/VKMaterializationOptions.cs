using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Materialization Feature slice.
/// Governed by [BB.05] & [BB.07].
/// </summary>
public sealed partial record VKMaterializationOptions : IVKBlockOptions // [AP.01]
{
    public bool AutoExtractMarkdownJson { get; init; } = true;
    public bool EnableAutoRepair { get; init; } = false;
    public int MaxRepairAttempts { get; init; } = 2;
    public VKMaterializationToleranceMode ToleranceMode { get; init; } = VKMaterializationToleranceMode.Strict;
    public int MaxTotalAttempts { get; init; } = 5;
}
