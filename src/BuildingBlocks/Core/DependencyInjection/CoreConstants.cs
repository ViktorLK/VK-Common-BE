namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Provides centralized constants for building block registration and infrastructure.
/// </summary>
public static class CoreConstants
{
    /// <summary>
    /// Message template for an InvalidOperationException when a required building block dependency (Core) is missing.
    /// {0} = The name of the dependent block (e.g. "Authentication").
    /// </summary>
    public const string MissingCoreRegistrationMessage = 
        "VK.Blocks.Core must be registered before adding the {0} block. Call services.AddVKCoreBlock(configuration) first.";
}
