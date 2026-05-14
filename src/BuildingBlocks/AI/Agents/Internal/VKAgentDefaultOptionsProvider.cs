using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAgentOptionsProvider"/> that uses static IOptions.
/// </summary>
internal sealed class VKAgentDefaultOptionsProvider(IOptions<VKAgentOptions> options) : IVKAgentOptionsProvider
{
    private readonly IOptions<VKAgentOptions> _options = options;

    public VKAgentOptions GetOptions() => _options.Value;
}
