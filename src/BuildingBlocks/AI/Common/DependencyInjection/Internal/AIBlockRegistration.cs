namespace VK.Blocks.AI.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class AIBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKAIBuilder builder)
    {
        // Automatically enable core defaults (Provider, Retry, etc.)
        builder.AddVKAIDefaults();
    }
}
