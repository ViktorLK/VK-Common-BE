using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Default implementation of <see cref="IVKChatOptionsProvider"/> that reads from static configuration options.
/// </summary>
internal sealed class ChatDefaultOptionsProvider(IOptions<VKChatOptions> options) : VK.Blocks.AI.IVKChatOptionsProvider
{
    private readonly VKChatOptions _options = options.Value;

    /// <inheritdoc />
    public VKChatOptions GetOptions() => _options;
}
