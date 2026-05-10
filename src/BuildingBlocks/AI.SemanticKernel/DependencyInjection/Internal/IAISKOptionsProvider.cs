namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Provides access to the Semantic Kernel options.
/// </summary>
internal interface IAISKOptionsProvider
{
    VKAISKOptions GetOptions();
}
