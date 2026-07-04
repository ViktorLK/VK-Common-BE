namespace VK.Blocks.Core;

/// <summary>
/// Defines a provider for resolving building block configuration options dynamically.
/// </summary>
/// <typeparam name="TOptions">The type of the options.</typeparam>
public interface IVKOptionsProvider<out TOptions> where TOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the current options.
    /// </summary>
    TOptions GetOptions();
}
