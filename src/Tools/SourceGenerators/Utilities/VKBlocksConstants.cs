namespace VK.Tools.SourceGenerators.Utilities;

/// <summary>
/// Local mirror of global constants for use within Source Generators.
/// </summary>
/// <remarks>
/// [IMPORTANT] This class MUST be kept in sync with <see cref="VK.Blocks.Core.Common.VKBlocksConstants"/>.
/// Source Generators target netstandard2.0 and cannot reference the Core library directly, 
/// hence this manual mirror to maintain zero runtime dependencies.
/// </remarks>
internal static class VKBlocksConstants
{
    /// <summary>
    /// The standard prefix used for all diagnostic sources, metrics, and meters.
    /// </summary>
    public const string VKBlocksPrefix = "VK.Blocks";

    /// <summary>
    /// The standard prefix used for configuration sections in appsettings.json.
    /// </summary>
    public const string VKBlocksConfigPrefix = "VKBlocks";
}
