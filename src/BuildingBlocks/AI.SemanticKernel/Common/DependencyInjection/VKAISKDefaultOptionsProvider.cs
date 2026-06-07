using Microsoft.Extensions.Options;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Default implementation of <see cref="IVKAISKOptionsProvider"/> that reads from static configuration.
/// </summary>
public sealed class VKAISKDefaultOptionsProvider(IOptions<VKAISKDefaultsOptions> options) : IVKAISKOptionsProvider
{
    private readonly VKAISKDefaultsOptions _options = options.Value;

    /// <inheritdoc />
    public VKAISKDefaultsOptions GetOptions() => _options;
}

