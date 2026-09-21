using System.Diagnostics;

namespace VK.Blocks.Observability;

/// <summary>
/// Manages hierarchical <see cref="Activity"/> (Span) lifecycles.
/// Automatically creates child spans nested under the current parent activity.
/// Complies with Manifest §1 and §6.
/// </summary>
// [AP.03] Level 1 Public API
public interface IVKActivityScope
{
    /// <summary>
    /// Starts a new child activity under the current parent context.
    /// Disposing the returned handle ends the span.
    /// </summary>
    IVKActivityHandle StartActivity(string name, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Starts a new activity with an explicit parent context.
    /// </summary>
    IVKActivityHandle StartActivity(string name, ActivityKind kind, ActivityContext parentContext);
}
