using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Common.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAIBuilder"/>.
/// Inherits from <see cref="VKBlockBuilder{TMarker}"/> to share common infrastructure.
/// </summary>
internal sealed class AIBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    : VKBlockBuilder<VKAIBlock>(services, configuration!), IVKAIBuilder;
