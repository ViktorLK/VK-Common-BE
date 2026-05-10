using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAISKOptionsProvider"/> that reads from static configuration.
/// </summary>
internal sealed class DefaultAISKOptionsProvider(IOptions<VKAISKOptions> options) : IAISKOptionsProvider
{
    private readonly VKAISKOptions _options = options.Value;

    /// <inheritdoc />
    public VKAISKOptions GetOptions() => _options;
}
