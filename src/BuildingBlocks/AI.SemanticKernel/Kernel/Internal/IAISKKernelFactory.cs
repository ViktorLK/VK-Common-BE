namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Defines a factory for creating Semantic Kernel instances.
/// </summary>
internal interface IAISKKernelFactory
{
    Microsoft.SemanticKernel.Kernel CreateKernel();
}
