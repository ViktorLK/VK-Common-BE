using System;

namespace VK.Blocks.Core;

/// <summary>
/// Source-generator attribute that maps a method parameter to a telemetry tag/dimension when recording metrics.
/// Follows BB.04 and OR.01.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class VKMetricTagAttribute : Attribute
{
    /// <summary>
    /// Gets the tag key name (e.g. "gen_ai.session.id" or "ai.psyche.stage").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="VKMetricTagAttribute"/>.
    /// </summary>
    /// <param name="name">The tag key name.</param>
    public VKMetricTagAttribute(string name)
    {
        Name = name;
    }
}
