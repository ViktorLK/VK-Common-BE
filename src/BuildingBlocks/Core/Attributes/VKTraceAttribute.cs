using System;
using System.Diagnostics;

namespace VK.Blocks.Core;

/// <summary>
/// Source-generator and diagnostic attribute that marks a method or class for automated distributed tracing (Activity Span).
/// Follows BB.04 and OR.01.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VKTraceAttribute : Attribute
{
    /// <summary>
    /// Gets the span/activity operation name (e.g. "psyche.stage.directive" or "vk.ai.chat.invoke").
    /// </summary>
    public string ActivityName { get; }

    /// <summary>
    /// Gets or sets the activity kind (defaults to <see cref="ActivityKind.Internal"/>).
    /// </summary>
    public ActivityKind Kind { get; init; } = ActivityKind.Internal;

    /// <summary>
    /// Initializes a new instance of <see cref="VKTraceAttribute"/>.
    /// </summary>
    /// <param name="activityName">The unique activity operation name.</param>
    public VKTraceAttribute(string activityName)
    {
        ActivityName = activityName;
    }
}
