namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Specifies the prompt template format for Semantic Kernel.
/// </summary>
public enum AISemanticKernelTemplateFormat
{
    /// <summary>
    /// The default Semantic Kernel template format.
    /// </summary>
    Default,

    /// <summary>
    /// Handlebars template format.
    /// </summary>
    Handlebars,

    /// <summary>
    /// Liquid template format.
    /// </summary>
    Liquid
}
