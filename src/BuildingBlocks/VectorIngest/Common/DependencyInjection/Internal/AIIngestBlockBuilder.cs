using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Ingest block builder.
/// </summary>
internal sealed class AIIngestBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKVectorIngestBlock>(services, configuration), IVKVectorIngestBuilder;
