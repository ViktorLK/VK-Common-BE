using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Default implementation of <see cref="IVKChatOptionsProvider"/> that uses static IOptions.
/// </summary>
internal sealed class VKChatDefaultOptionsProvider(IOptions<VKChatOptions> options) : IVKChatOptionsProvider
{
    private readonly IOptions<VKChatOptions> _options = options;

    public VKChatOptions GetOptions() => _options.Value;
}
