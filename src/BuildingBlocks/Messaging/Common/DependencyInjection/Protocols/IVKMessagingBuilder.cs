using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Builder interface for extension providers to chain dependencies.
/// </summary>
public partial interface IVKMessagingBuilder : IVKBlockBuilder<VKMessagingBlock>;

public sealed partial class VKMessagingBuilder : VKBlockBuilder<VKMessagingBlock>, IVKMessagingBuilder
{
    public VKMessagingBuilder(IServiceCollection services, IConfiguration configuration)
        : base(services, configuration)
    {
    }
}
