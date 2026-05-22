using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Vector Store block builder.
/// </summary>
internal sealed class AIVectorStoreBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKAIVectorStoreBlock>(services, configuration), IVKAIVectorStoreBuilder;
