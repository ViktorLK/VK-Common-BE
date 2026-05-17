using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;

/// <summary>
/// Default implementation of <see cref="IVKAICognitiveBuilder"/>.
/// Inherits from <see cref="VKBlockBuilder{TMarker}"/> to share common infrastructure.
/// </summary>
internal sealed class VKAICognitiveBlockBuilder(IServiceCollection services, IConfiguration? configuration)
    : VKBlockBuilder<VKAICognitiveBlock>(services, configuration!), IVKAICognitiveBuilder;
