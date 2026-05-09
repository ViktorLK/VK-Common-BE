using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Options for the Observability building block.
/// </summary>
public sealed record VKObservabilityOptions : IVKBlockOptions
{
    public static string SectionName => VKBlocksConstants.VKBlocksConfigPrefix + "Observability";

    public bool Enabled { get; init; } = true;

    [Required, MinLength(1)]
    public string ApplicationName { get; init; } = "Unknown";

    [Required, MinLength(1)]
    public string ServiceVersion { get; init; } = "1.0.0";

    public string Environment { get; init; } = "Production";

    public bool EnableTracing { get; init; } = true;

    public bool EnableMetrics { get; init; } = true;

    public bool IncludeUserName { get; init; } = false;
}
