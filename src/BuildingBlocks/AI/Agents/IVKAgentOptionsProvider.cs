namespace VK.Blocks.AI;

/// <summary>
/// Defines a provider for resolving <see cref="VKAgentOptions"/> dynamically.
/// </summary>
public interface IVKAgentOptionsProvider
{
    /// <summary>
    /// Gets the current agent options.
    /// </summary>
    VKAgentOptions GetOptions();
}
