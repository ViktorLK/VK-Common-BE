namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Provides access to the Semantic Kernel options.
/// Allows for dynamic resolution of options (e.g., per-request overrides).
/// </summary>
public interface IVKAISKOptionsProvider
{
    /// <summary>
    /// Gets the Semantic Kernel options for the current context.
    /// </summary>
    /// <returns>The options instance.</returns>
    VKAISKOptions GetOptions();
}
