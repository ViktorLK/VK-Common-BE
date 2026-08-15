using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for resolving provider-specific AI engines registered via .NET Keyed Services.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKAIEngineAccessor
{
    /// <summary>
    /// Gets the <see cref="IVKChatEngine"/> registered for the given provider type.
    /// </summary>
    IVKChatEngine? GetChatEngine(VKAIProviderType providerType);

    /// <summary>
    /// Gets the <see cref="IVKChatEngine"/> registered for the given provider key/name.
    /// </summary>
    IVKChatEngine? GetChatEngine(string providerName);

    /// <summary>
    /// Gets a typed engine or service registered for the specified service key.
    /// </summary>
    T? GetEngine<T>(object serviceKey) where T : class;
}
