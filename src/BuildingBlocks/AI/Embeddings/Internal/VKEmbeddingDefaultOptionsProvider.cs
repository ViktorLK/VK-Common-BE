using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Embeddings.Internal;

/// <summary>
/// Default implementation of <see cref="IVKEmbeddingOptionsProvider"/> that uses static IOptions.
/// </summary>
internal sealed class VKEmbeddingDefaultOptionsProvider(IOptions<VKEmbeddingOptions> options) : IVKEmbeddingOptionsProvider
{
    private readonly IOptions<VKEmbeddingOptions> _options = options;

    public VKEmbeddingOptions GetOptions() => _options.Value;
}
