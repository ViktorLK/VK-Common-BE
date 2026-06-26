using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Recall builder.
/// </summary>
internal sealed class VectorSearchBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    : VKBlockBuilder<VKVectorSearchBlock>(services, configuration!), IVKVectorSearchBuilder;
