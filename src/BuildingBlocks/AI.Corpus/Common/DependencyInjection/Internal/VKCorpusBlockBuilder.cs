using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;

/// <summary>
/// Concrete builder implementation for configuring AI.Corpus block features.
/// Follows AP.01 / AP.03.
/// </summary>
internal sealed class VKCorpusBlockBuilder : VKBlockBuilder<VKCorpusBlock>, IVKCorpusBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="VKCorpusBlockBuilder"/>.
    /// </summary>
    public VKCorpusBlockBuilder(IServiceCollection services, IConfiguration? configuration)
        : base(services, configuration!)
    {
    }
}
