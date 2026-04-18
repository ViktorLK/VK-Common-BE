namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Provides centralized constants for building block registration and infrastructure.
/// </summary>
public static class CoreConstants
{
    /// <summary>
    /// Message template for an InvalidOperationException when a required building block dependency is missing.
    /// {0} = The name of the required block (e.g. "Caching").
    /// {1} = The name of the dependent block (e.g. "Web.Caching").
    /// </summary>
    public const string MissingBlockDependencyMessage =
        "VKBlock '{1}' requires '{0}' to be registered first. Please ensure the required block is added during startup.";
}
