namespace VK.Blocks.Mapping.Options;

/// <summary>
/// Configuration options for the mapping framework.
/// </summary>
public sealed class MappingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether mapping configurations should be validated.
    /// </summary>
    public bool ValidateConfigurations { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable internal AutoMapper execution plan compilation.
    /// </summary>
    public bool EnableCompilation { get; set; } = true;
}
