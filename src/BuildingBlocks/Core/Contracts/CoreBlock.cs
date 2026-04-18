using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Core.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Core building block.
/// Required to satisfy Rule 13 dependency checks in other modules.
/// </summary>
public sealed class CoreBlock : IVKBlock
{
    /// <summary>
    /// The unique identifier for the Core building block.
    /// </summary>
    public const string Identifier = "Core";

    /// <inheritdoc />
    public static string BlockName => Identifier;
}


