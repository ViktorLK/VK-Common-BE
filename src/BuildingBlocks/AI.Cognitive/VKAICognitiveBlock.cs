using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Cognitive.Attention.Internal;
using VK.Blocks.AI.Cognitive.Intent.Internal;
using VK.Blocks.AI.Cognitive.Planning.Internal;
using VK.Blocks.AI.Cognitive.Presence.Internal;
using VK.Blocks.AI.Cognitive.Projection.Internal;
using VK.Blocks.AI.Cognitive.Reflection.Internal;
using VK.Blocks.AI.Eidos;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock), typeof(VKAIEidosBlock)])]
public sealed partial class VKAICognitiveBlock
{
    static partial void RegisterBlockCustom(IVKAICognitiveBuilder builder) // [SG Hook]
    {
        var services = builder.Services;

        // Core Services - Override/Provide Cognitive implementations
        services.AddScoped<IVKPresenceTracker, DefaultPresenceTracker>();
        services.AddScoped<IVKReasoningPlanner, DefaultReasoningPlanner>();
        services.AddScoped<IVKIntentRouter, IndustrialIntentRouter>();
        services.AddScoped<IVKReflectionService, DefaultReflectionService>();
        services.AddScoped<IVKAttentionFilter, DefaultAttentionFilter>();
        services.AddScoped<IVKProjectionEngine, DefaultProjectionEngine>();
    }
}
