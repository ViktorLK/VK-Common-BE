using System;

namespace VK.Blocks.AI.SemanticKernel.Common.Plugins;

/// <summary>
/// Attribute used to mark a class for automatic discovery as a Semantic Kernel plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VKAIPluginAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the plugin. If null, the class name will be used.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this plugin is enabled for auto-discovery.
    /// Defaults to true.
    /// </summary>
    public bool AutoRegister { get; init; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAIPluginAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    public VKAIPluginAttribute(string? name = null)
    {
        Name = name;
    }
}
