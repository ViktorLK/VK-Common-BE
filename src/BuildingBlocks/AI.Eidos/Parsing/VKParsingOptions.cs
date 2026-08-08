using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Parsing Feature slice.
/// </summary>
public sealed partial record VKParsingOptions : IVKBlockOptions
{
    public bool AutoExtractMarkdownJson { get; init; } = true;
    public bool EnableAutoRepair { get; init; } = false;
    public int MaxRepairAttempts { get; init; } = 2;
}
