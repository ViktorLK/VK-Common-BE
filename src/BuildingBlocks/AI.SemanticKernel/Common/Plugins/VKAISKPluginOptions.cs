using System.Collections.Generic;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Options for configuring Semantic Kernel plugins.
/// </summary>
public sealed record VKAISKPluginOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether auto-discovery of plugins is enabled.
    /// Defaults to true.
    /// </summary>
    public bool AutoDiscoveryEnabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the list of plugin types to register explicitly from configuration.
    /// Key: Plugin Name, Value: Assembly-qualified type name.
    /// </summary>
    public Dictionary<string, string> Types { get; init; } = [];

    /// <summary>
    /// Gets or sets the list of assemblies to scan for plugins.
    /// If empty, the entry assembly and referenced assemblies will be scanned.
    /// </summary>
    public List<string> AssembliesToScan { get; init; } = [];
}
