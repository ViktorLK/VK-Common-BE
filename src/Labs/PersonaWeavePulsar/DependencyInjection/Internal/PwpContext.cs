using VK.Blocks.AI;
using VK.Blocks.AI.SemanticKernel;

namespace VK.Labs.PersonaWeavePulsar.DependencyInjection.Internal;

/// <summary>
/// Scoped context to carry per-request overrides across the DI chain.
/// This bridges the gap between method arguments (P0) and the OptionsProvider.
/// </summary>
public sealed class PwpContext
{
    /// <summary>
    /// Gets or sets the current SessionId.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the current PersonaId.
    /// </summary>
    public string? PersonaId { get; set; }

    /// <summary>
    /// Gets or sets the L1 (P0) overrides for Semantic Kernel.
    /// </summary>
    public VKAISKOptions? OverrideOptions { get; set; }

    /// <summary>
    /// Gets or sets the L1 (P0) overrides for Chat connectivity.
    /// </summary>
    public VKChatOptions? OverrideChatOptions { get; set; }
}
