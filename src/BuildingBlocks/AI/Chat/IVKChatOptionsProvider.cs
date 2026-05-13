namespace VK.Blocks.AI;

/// <summary>
/// Defines a provider for resolving <see cref="VKChatOptions"/> dynamically.
/// </summary>
public interface IVKChatOptionsProvider
{
    /// <summary>
    /// Gets the current chat options, potentially resolved from a dynamic context.
    /// </summary>
    VKChatOptions GetOptions();
}
