using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Common.DependencyInjection.Internal;

/// <summary>
/// Internal implementation of the AI Ingest block builder.
/// </summary>
internal sealed class AIIngestBlockBuilder(IServiceCollection services, IConfiguration configuration)
    : VKBlockBuilder<VKAIIngestBlock>(services, configuration), IVKAIIngestBuilder;
