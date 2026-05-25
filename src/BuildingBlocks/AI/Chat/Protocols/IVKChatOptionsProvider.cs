namespace VK.Blocks.AI;

/// <summary>
/// Provides access to the Chat options.
/// Allows for dynamic resolution of options (e.g., per-request overrides).
/// </summary>
public interface IVKChatOptionsProvider
{
    /// <summary>
    /// Gets the Chat options for the current context.
    /// </summary>
    /// <returns>The options instance.</returns>
    VKChatOptions GetOptions();
}
