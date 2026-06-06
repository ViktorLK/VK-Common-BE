namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Defines a factory for creating Semantic Kernel instances.
/// </summary>
internal interface IAISKKernelFactory
{
    /// <summary>
    /// Creates a new Kernel instance using the resolved options.
    /// </summary>
    Microsoft.SemanticKernel.Kernel CreateKernel();
}
