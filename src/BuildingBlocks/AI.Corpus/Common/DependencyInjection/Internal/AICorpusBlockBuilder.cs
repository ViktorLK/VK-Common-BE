using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;

/// <summary>
/// Concrete builder implementation for configuring AI.Corpus block features.
/// Follows AP.01 / AP.03.
/// </summary>
internal sealed class AICorpusBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    : VKBlockBuilder<VKAICorpusBlock>(services, configuration!), IVKAICorpusBuilder;
