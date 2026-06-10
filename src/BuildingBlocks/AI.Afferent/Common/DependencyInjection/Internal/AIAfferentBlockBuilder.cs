using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

/// <summary>
/// Builder implementation for configuring the AI Afferent block.
/// Complies with AP.01.
/// </summary>
internal sealed class AIAfferentBlockBuilder : VKBlockBuilder<VKAIAfferentBlock>, IVKAIAfferentBuilder
{
    public AIAfferentBlockBuilder(IServiceCollection services, IConfiguration configuration)
        : base(services, configuration)
    {
    }
}
