namespace VK.Blocks.Core;

/// <summary>
/// Defines the base contract for building block configuration options.
/// </summary>
/// <remarks>
/// <para>
/// <b>VK.Blocks Options Standard Blueprint (AP.01 / AP.04):</b>
/// <list type="bullet">
/// <item><description>Must be a <c>sealed record</c> with <c>{ get; init; }</c> properties to guarantee immutability.</description></item>
/// <item><description>Must provide default values for all properties instead of C# <c>required</c> to satisfy the generic <c>new()</c> constraint.</description></item>
/// <item><description>Use DataAnnotations (e.g., <c>[Required]</c>) for fail-fast validation at startup (<c>ValidateOnStart()</c>).</description></item>
/// <item><description>Use <c>IValidateOptions&lt;T&gt;</c> for complex, conditional, or cross-property validation.</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name for the building block options.
    /// Resolved statically at compile-time/runtime without reflection.
    /// Typically follows the "VKBlocks:Category:Feature" pattern.
    /// </summary>
    static abstract string SectionName { get; }
}


