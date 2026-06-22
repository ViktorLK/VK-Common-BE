using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Vector Store block builder.
/// </summary>
internal sealed class VectorStoreBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKVectorStoreBlock>(services, configuration), IVKVectorStoreBuilder;
