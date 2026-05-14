using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Cognitive.DependencyInjection.Internal;

internal sealed class VKAICognitiveBlockBuilder : IVKAICognitiveBuilder
{
    public IServiceCollection Services { get; }
    public IConfiguration? Configuration { get; }

    public VKAICognitiveBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    {
        Services = services;
        Configuration = configuration;
    }
}
