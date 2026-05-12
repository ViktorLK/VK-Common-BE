namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Defines a factory for creating Semantic Kernel instances.
/// </summary>
public interface IVKAISKKernelFactory
{
    /// <summary>
    /// Creates a new Kernel instance using the resolved options.
    /// </summary>
    Microsoft.SemanticKernel.Kernel CreateKernel();
}
