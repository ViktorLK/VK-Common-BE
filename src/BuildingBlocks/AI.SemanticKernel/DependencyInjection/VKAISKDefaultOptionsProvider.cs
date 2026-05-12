using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Default implementation of <see cref="IVKAISKOptionsProvider"/> that reads from static configuration.
/// </summary>
public sealed class VKAISKDefaultOptionsProvider(IOptions<VKAISKOptions> options) : IVKAISKOptionsProvider
{
    private readonly VKAISKOptions _options = options.Value;

    /// <inheritdoc />
    public VKAISKOptions GetOptions() => _options;
}
