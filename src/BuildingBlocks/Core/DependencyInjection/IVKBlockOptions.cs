namespace VK.Blocks.Core;

/// <summary>
/// Defines the base contract for building block configuration options.
/// </summary>
/// <remarks>
/// <para>
/// <b>ARCHITECTURE NOTE (Source Resolution Pattern):</b><br/>
/// This interface uses C# 11 Static Abstract Members to provide zero-reflection access 
/// to the configuration section name. This ensures compile-time safety and optimal 
/// startup performance.
/// </para>
/// </remarks>
public interface IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name for the building block options.
    /// Typically follows the "VKBlocks:Category:Feature" pattern.
    /// </summary>
    static abstract string SectionName { get; }
}
